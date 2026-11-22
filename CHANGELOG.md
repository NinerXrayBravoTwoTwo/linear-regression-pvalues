# Changelog

All notable changes to `LinearRegression.Pvalues`. Dates are release or version-bump dates.
The NuGet package carries the same notes in its README. The git history has notes on every small change.

## [Unreleased]

## [1.2.0] - 2026-11-21

Numeric results change in this release: p-values, confidence intervals, standard deviations and
margins of error from 1.1.x were wrong (see Fixed). Re-run any analysis that relied on them.

### Breaking
- Removed `Add(Regression)`; use `Merge`. `a.Add(b)` becomes `a = a.Merge(b)`; the sums, count and min/max
  are the same, but `Merge` returns a new `Regression` instead of changing `a`.

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

### Added
- `RegressionPvalue.ToString()` appends ` PValue: <value>`, or `n/a (needs 3+ points)` when there are
  fewer than 3 points. (PR #4)
- Reference-value tests against SciPy/numpy/statsmodels (README dataset, Anscombe's quartet I), edge-case
  tests, and a StdDev/Variance divisor comparison. 56 → 87 tests. (PRs #2–#4)

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

### Changed
- `RegressionPvalue.PValue` returns `+∞` when the data contains an infinite value (b49b96d).
  Under review: a p-value is normally in [0, 1], so this may become `NaN`.

## [1.1.3] - 2025-08-20
Version in the repository; not yet published to NuGet (latest published: 1.1.2).
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
