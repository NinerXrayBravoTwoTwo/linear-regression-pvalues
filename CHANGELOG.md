# Changelog

All notable changes to `LinearRegression.Pvalues`. Dates are release or version-bump dates.
The NuGet package carries the same notes in its README. The git history has notes on every small change.

## [Unreleased]

New non-breaking API. This will be a minor bump to 1.4.0 when released.

### Added
- Mean-response and prediction intervals on `RegressionPvalue`: `MeanResponseInterval(x, level)` and
  `PredictionInterval(x, level)` return `(Fit, Lower, Upper)`. The mean-response interval is the band
  around the fitted line (Excel-style trend-line band): ŷ ± t·s·√(1/n + (x − x̄)²/Sxx). The prediction
  interval is the wider interval for a single new observation (an extra 1 under the square root). Same
  NaN cases as the slope tests, plus a non-finite x or a level outside (0, 1). A perfect fit (zero
  residual) collapses the interval to Fit. Matches statsmodels `OLS.get_prediction`.

## [1.3.0] - 2026-11-22

Minor version (semantic versioning): adds new public API (intercept inference, population variance and standard
deviation) with no breaking changes. Results from 1.2.0 are unchanged except where listed under Fixed: tiny
p-values now keep their digits instead of rounding to 0, and `Dec` of a far-away point now restores the statistics
(other results may differ only in the last digit or so).

### Added
- Intercept inference on `RegressionPvalue`: `InterceptStandardError` (√(s²(1/n + x̄²/Sxx)), Excel `LINEST`
  row 2, col 2), `InterceptPValue` (two-tailed, df = n − 2, small-p-safe 2·CDF(−|t|)) and
  `InterceptConfidenceInterval(level)`. Invalid cases return NaN, as for the slope. Example (x = 1..5,
  y = 2, 4, 5, 4, 5): a = 2.2, SE 0.938083, t 2.3452, p 0.100743, 95% CI [−0.785399, 5.185399].
- `docs/MATH.md`: formulas for every statistic, the n − 1 and n − 2 conventions, and a worked example.
- Population (N) variance and standard deviation, opt-in: `VarianceXPopulation`, `VarianceYPopulation`,
  `StdDevXPopulation`, `StdDevYPopulation` (Excel `VAR.P` / `STDEV.P`). The defaults stay n − 1, and regression
  SE, p-values and CIs keep df = n − 2.

### Documentation
- XML doc comments for every public member (summary, parameters, returns, NaN behavior). The XML file ships in the
  NuGet package, so IntelliSense shows them. (PR #18)
- `docs/MATH.md` "Why Add and Dec exist": the TI-58/59 calculator history behind `Add`/`Dec` (`2nd Σ+` and
  `INV 2nd Σ+`) and the six running-sum registers, from TI's *Personal Programming* (1977). (PR #16)

### Fixed
- **Tiny p-values no longer round to 0.** `PValue` (and so `ConfidenceIntervalPlus().PValue`) is now
  2·CDF(−|t|) instead of 2·(1 − CDF(|t|)), which lost every digit below about 1e−16. Example (n = 30, t = 25.05):
  p was 0, now 1.0294e−20 (SciPy 1.0294e−20). Small p-values also gain digits, e.g. 2.0625857e−10 →
  2.0625847e−10 (SciPy 2.0625847e−10). p-values near 1 and NaN for invalid results are unchanged.
- **`Dec` after removing a far-away point** (#14): with data far from zero, `Add` then `Dec` of a point far from
  the rest no longer leaves wrong statistics. Example (x = 1e6 + 0.00..0.04, Add/Dec (0, 0)): slope was
  4.8 → 4.9152 and `VarianceX` 2.5e−4 → 2.441e−4; now both restore exactly. The streaming moments are kept in
  double-double (about 32 digits). Add/Dec cost about 3× more (about 70 ns per point instead of 25 ns). See docs/MATH.md.

### Build and release
- GitHub Actions: `ci.yml` builds and tests (Release, net8.0 + net10.0) on pull requests and on pushes to `main`.
  `publish.yml` publishes to nuget.org with Trusted Publishing (OIDC, `NuGet/login@v1`, no stored API key) when a
  `vX.Y.Z` tag is pushed, or on demand for an existing tag. It then creates the GitHub release from the CHANGELOG
  section. Setup and steps are in the README under "Releasing".

## [1.2.0] - 2026-11-21

Numeric results change in this release: p-values, confidence intervals, standard deviations and
margins of error from 1.1.x were wrong (see Fixed). Re-run any analysis that relied on them.

1.2.0 also changes how invalid results are reported (always `NaN`), makes `VarianceX`/`VarianceY` the sample
(n − 1) variance, and accumulates the sums in a numerically stable way.

**NuGet note:** 1.1.3 was never published to NuGet. On nuget.org, 1.2.0 follows 1.1.2, so the 1.1.3 changes
below ship for the first time in 1.2.0.

### Breaking
- Removed `Add(Regression)`; use `Merge`. `a.Add(b)` becomes `a = a.Merge(b)`; the sums, count and min/max
  are the same, but `Merge` returns a new `Regression` instead of changing `a`.
- Invalid results are now `double.NaN` instead of 1, +∞ or an exception (n < 3 used to throw). See
  "Invalid results are NaN" below. (PR #7)
- `VarianceX`/`VarianceY` are now the sample (n − 1) variance, so their values grow by n/(n − 1). (PR #8)

### Fixed
- **Slope standard error** was too large by √n (it divided by `VarianceX` = Sxx/n instead of Sxx). This
  affected `PValue`, `ConfidenceInterval` and `ConfidenceIntervalPlus`. README 5-point example: p was 0.208,
  now 0.0374 (matches SciPy); 95% CI was [−0.891, 2.691], now [0.0991, 1.7009]. (PR #2)
- **`StdDevX`/`StdDevY`** computed √Sxx/(n−1) instead of √(Sxx/(n−1)), the n−1 sample standard deviation.
  This also fixes `MarginOfError`. Example: 0.7906 → 1.5811. `Correlation` was unaffected. (PR #2)
- `Regression(IEnumerable)` (and so `RegressionPvalue`) started `MinX`/`MinY`/`MaxX`/`MaxY` at 0
  instead of ±`double.MaxValue`. (PR #3)
- `Merge(null)` now throws `ArgumentNullException`. The old check tested a constant string. (PR #3)
- `Dec(x)` removed y = N−1; it now removes y = N, the value `Add(x)` stored. (PR #3)
- `RegressionPvalue` now copies the caller's list (`DataPoints`, `IdPoints`) instead of holding a live
  view of it, so later changes to that list no longer desynchronize the points and the sums. (PR #3)
- `RegressionQValue.QValue` (experimental): the Benjamini-Hochberg step-up is now a cumulative minimum
  from the top rank. It only compared with the next rank, which could overstate q (e.g. 0.10 instead of
  0.0625). Matches statsmodels `fdr_bh` / R `p.adjust("BH")`. (PR #3)

### Changed: invalid results are NaN (PR #7)
Invalid results are now always `double.NaN`: never 1, never ±∞, never an exception. With many regressions,
filter invalid ones with `double.IsNaN(r.PValue)`; a real p = 1 is no longer mixed in with them.

| Case | `PValue` before | `PValue` now |
|---|---|---|
| n < 3 | throws `InvalidOperationException` | NaN |
| ±∞ in x or y | +∞ (b49b96d) | NaN |
| All x equal | 1.0 (or a garbage value from rounding, e.g. x = 0.1 repeated) | NaN |
| All y equal (slope 0, zero residual) | 1.0 | NaN |
| Perfect fit, slope ≠ 0 | 0 | 0 (unchanged, valid) |

- `Slope` is NaN when every x is equal (`MinX == MaxX`). Rounding used to give garbage, e.g. 5.96e−8.
- `ConfidenceInterval`/`ConfidenceIntervalPlus` return NaN for n < 3 (it threw), ±∞ data, no x variation, or a
  confidence level outside (0, 1) (it threw MathNet `NonConvergenceException`). A constant y still gives `(0, 0)`.
- `MarginOfError` returns NaN for n < 2 (it threw) and for ±∞ data (it gave a finite or half-NaN result).
- `RegressionPvalue.ToString()` shows `PValue: n/a` whenever the p-value is NaN (was `n/a (needs 3+ points)`).
- `RegressionQValue.QValue`: NaN/out-of-range family entries are excluded from the ranking and from m (matches
  statsmodels `fdr_bh` on the valid subset) instead of making the result NaN. It returns NaN when this regression's
  own p-value is NaN (it threw for n < 3). An empty family means m = 1, so q = p (it threw).
- The +∞ p-value for infinite data came from b49b96d, which was never in a published package. It is
  now `NaN`, as shown in the table above.

### Changed: Variance uses the sample divisor n − 1 (PR #8)
`VarianceX`/`VarianceY` now return the sample variance (Sxx/(n − 1), numpy `var(ddof=1)`), the same divisor
`StdDevX`/`StdDevY` already used, so `StdDev² == Variance`. Values grow by n/(n − 1). Examples: README 5-point
2.0 → 2.5; Anscombe I x 10 → 11, y 3.752 → 4.127. Nothing else in the library read `VarianceX`/`VarianceY` (slope SE
uses Sxx directly), so slope, p-values, confidence intervals, margin of error, correlation and q-values are
unchanged for valid data.
- `VarianceX`/`VarianceY` with N < 2 return NaN. N = 0 used to throw `DivideByZeroException`; N = 1 used to give 0.
- `Regression.ToString()` (and so `RegressionPvalue.ToString()`) no longer throws on an empty instance; MinMax shows `n/a`.
- When every x (or y) is equal, Variance and StdDev are exactly 0. Rounding used to leave noise, e.g. StdDevX 1.3e−9 for
  x = 0.1 repeated, which also gave `MarginOfError` 1.6e−9 instead of 0.

### Changed: numerically stable streaming accumulation (PR #9)
Sxx, Syy and the co-moment Cxy are now accumulated with Welford's streaming update, relative to the first point
added ("shifted data"). `Dec` applies the exact reverse update; `Merge` uses the Chan–Golub–LeVeque pairwise combine.
Slope, intercept, RSS (now computed from centred residuals), correlation, variances, slope SE, p-values and CIs
no longer cancel catastrophically when the data are far from zero. Public API unchanged.
- Before, x = 1e9 + small gave `VarianceX` = 0 and `Slope`/`PValue` = NaN; y = 1e9 + small gave `VarianceY` = 0 and a
  p-value off by ~1e−6 relative. x = 1e6 + small gave p off by ~1e−9. Now the results match exact arithmetic to ~1e−14.
- Ordinary data change only in the last digits (≈1e−15 relative), usually toward the exact value
  (Anscombe I slope 0.5000909090909079 → 0.5000909090909091, exact 0.50009090909…).
- `Sx`, `Sy`, `Sx2`, `Sy2`, `Sxy` still hold the raw sums (accumulated alongside). Setting one directly switches that
  instance to the old raw-sum formulas. NaN/±∞ data also use the raw-sum formulas, so NaN propagation is unchanged.
- Cost: `Add` ≈ 3.8 → 6.4 ns per point (10M points, .NET 10, Release).

### Added
- `RegressionPvalue.ToString()` appends ` PValue: <value>`, or ` PValue: n/a` when the p-value is NaN. (PRs #4, #7)
- Reference-value tests against SciPy/numpy/statsmodels (README dataset, Anscombe's quartet I), edge-case
  tests, and a StdDev/Variance divisor comparison. (PRs #2–#4)
- `RegressionQValue.BenjaminiHochberg(IReadOnlyList<double>)`: q-values for a whole family in one O(m log m)
  pass. NaN p-values give NaN q-values. Calling `QValue` once per regression is O(m² log m) in total. (PR #7)
- Numerical-stability tests: shifted-data invariance, catastrophic cancellation, Dec/Merge round trips. (PR #9)
- 56 → 146 tests in total (PRs #2–#9).

### Packaging
- License expression corrected to `AGPL-3.0-or-later` to match `LICENSE` (it said `LGPL-3.0-or-later`).
- Targets `net8.0` and `net10.0` (was `net8.0`).
- XML documentation is now included in the package.
- `dotnet pack` works with multi-targeting (removed `GeneratePackageOnBuild`, NU5026).
- `global.json`: .NET 10 SDK and the Microsoft.Testing.Platform test runner (xunit v3), so `dotnet test`
  works on the .NET 10 SDK.
- Test project runs on Microsoft.Testing.Platform only: removed `xunit.abstractions` (tests use xunit v3's
  `Xunit.ITestOutputHelper`), and the VSTest-only `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`
  and `coverlet.collector`. Coverage now comes from `Microsoft.Testing.Extensions.CodeCoverage`
  (`dotnet test --coverage`). Test Explorer needs Visual Studio 2022 17.14+ (or another MTP-aware IDE).

### Documentation
- Rewrote the root and package READMEs to match the current API, with installation, compiling usage
  examples, an API overview, lineage, license and contact. (PR #1)
- Added this changelog. (PR #1)
- Scaffolding review: each item and decision recorded in [docs/SCAFFOLDING.md](docs/SCAFFOLDING.md);
  `Regresion.cs` renamed to `Regression.cs`; commented-out code removed. (PR #4)

## [1.1.3] - 2025-08-20
Never published to NuGet; these changes first ship in 1.2.0, which follows 1.1.2 on nuget.org.
- Cache the p-value after the first calculation so repeated reads of `PValue` are cheap.
- Removed an obsolete `RegressionPvalue` constructor.

## [1.1.2] - 2025-08-07
- Fixed the parameterless `RegressionPvalue()` constructor so empty instances are reliable.

## [1.1.1] - 2025-08-07
- `DataPoints` is now `IEnumerable<(double x, double y)>` instead of `List<>`. Code that indexed with
  `[i]` needs `.ElementAt(i)`.

## [1.1.0] - 2025-08-01
- Breaking: statistics that were methods are now properties (for example `Slope` instead of `Slope()`).
- Experimental: optional string id per point (`IdPoints`) via `RegressionPvalue(List<(string id, double x, double y)>)`.

## [1.0.7] - 2025-07-22
- Breaking: renamed legacy `Qx`, `Qy`, `Qx2`, `Qy2` to `StdDevX`, `StdDevY`, `VarianceX`, `VarianceY`.

## [1.0.6] - 2025-07-21
- Added `MarginOfError` (± value for the mean of x or y).
- Split confidence-interval code into partial classes.

## [1.0.5] - 2025-07-21
- Added `ConfidenceInterval` and `ConfidenceIntervalPlus` (slope, standard error, p-value).
- Fixed the p-value code computing the variance of x outside the checks built into `Regression`.

## [1.0.4] - 2025-06-08
- Made `DataPoints` publicly readable so the regression object can carry its data.

## [1.0.3] - 2025-06-03
## [1.0.1] - 2025-06-02
## [1.0.0] - 2025-06-02
- First NuGet releases.

[Unreleased]: https://github.com/NinerXrayBravoTwoTwo/linear-regression-pvalues/compare/v1.2.0...HEAD
