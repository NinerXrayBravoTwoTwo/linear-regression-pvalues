# Linear Regression P-Values

A small, dependency-light C# library for bivariate (x, y) linear regression. It computes slope,
y-intercept, correlation, R², means, standard deviations and variances from running sums, and adds a
two-tailed p-value for the slope, slope and intercept confidence intervals, a mean-response band and prediction interval at a given x, and a margin of error for the means.
It is aimed at developers, analysts and statisticians who want these numbers without a large
statistics package.

NuGet package: [`LinearRegression.Pvalues`](https://www.nuget.org/packages/LinearRegression.Pvalues)

## Features

- **Streaming by design** — `Regression` keeps only running sums, so points can be added, removed
  (`Dec`) and whole regressions merged without storing the data.
- **Descriptive statistics** — N, sums, min/max, means, standard deviation (n − 1), variance (n).
- **Fit** — slope, y-intercept, Pearson correlation, R².
- **Inference** (`RegressionPvalue`) — slope p-value (Student t, n − 2 degrees of freedom), confidence
  interval at any level, standard error, intercept inference, a mean-response band around the fitted
  line (`MeanResponseInterval`) and a prediction interval for a new observation, margin of error for the mean of x or y.
- **Curve fits** — exponential (y = b·e^(mx)), power (y = b·x^m) and logarithmic (y = b + m·ln x), or your own
  transforms, after the TI-59 ST-12 program: enter raw (x, y), get b, m and predictions back in original units.
- **Point ids** — optionally tag each (x, y) with a string id that stays aligned with the data.
- **Numerically stable streaming** — Welford updates (with Dec and Merge support) keep results accurate for data
  far from zero, e.g. x = 1e9 + small, where raw sums of squares cancel.
- **Experimental q-values** (`RegressionQValue`) — Benjamini-Hochberg false-discovery-rate adjustment.
- **No exceptions for ordinary bad data** — undefined results come back as `double.NaN`; the few
  exceptions are listed in the API overview.
- `Regression` and `RegressionPvalue` are marked `[Serializable]`.

## Installation

From NuGet:

```bash
dotnet add package LinearRegression.Pvalues
```

Or from source: clone this repository and add a project reference to `LinearRegression/LinearRegression.csproj`.

Requirements: .NET 8 or later (the package targets `net8.0` and `net10.0`). Building from source needs the .NET 10 SDK (see `global.json`). The only dependency is [MathNet.Numerics](https://numerics.mathdotnet.com/) 5.0
(for the Student t distribution), which NuGet installs for you.

Build and test:

```bash
dotnet build
dotnet test                      # net8.0 and net10.0, via Microsoft.Testing.Platform (global.json)
dotnet test --coverage --coverage-output-format cobertura   # code coverage, written under TestResults/
```

## Quick start

```csharp
using LinearRegression;

// 1. Running sums only (no points stored): add points one at a time.
var r = new Regression();
r.Add(1, 2);
r.Add(2, 3);
r.Add(3, 5);
r.Add(4, 4);
r.Add(5, 6);

Console.WriteLine($"Slope {r.Slope}, intercept {r.YIntercept}, r {r.Correlation}, R² {r.RSquared}");
Console.WriteLine($"Mean x {r.MeanX}, mean y {r.MeanY}, N {r.N}");

// 2. Keep the points so the slope can be tested: p-value and confidence intervals.
var data = new List<(double x, double y)> { (1, 2), (2, 3), (3, 5), (4, 4), (5, 6) };
var pr = new RegressionPvalue(data);

double p = pr.PValue;                                  // two-tailed p-value for slope != 0 (needs n >= 3)
var (lower, upper) = pr.ConfidenceInterval();          // 95% CI for the slope
var plus = pr.ConfidenceIntervalPlus(0.90);            // (Lower, Upper, Slope, StandardError, PValue)
var band = pr.MeanResponseInterval(3.0);               // 95% band for the fitted mean at x
var pred = pr.PredictionInterval(3.0);                 // 95% interval for a new observation at x
var (mean, moe) = pr.MarginOfError(useY: true);        // mean of y ± margin of error (95%)

Console.WriteLine($"p = {p}, 95% CI [{lower}, {upper}], 90% SE {plus.StandardError}, y = {mean} ± {moe}");
Console.WriteLine($"fitted band [{band.Lower}, {band.Upper}], prediction [{pred.Lower}, {pred.Upper}]");

// 3. Optional ids that line up with the points.
var tagged = new RegressionPvalue(new List<(string id, double x, double y)>
{
    ("a", 1, 2), ("b", 2, 3), ("c", 3, 5)
});
Console.WriteLine($"{tagged.IdPoints.ElementAt(0)} -> {tagged.DataPoints.ElementAt(0)}");

// 4. Combine two running regressions.
var left = new Regression(new List<(double x, double y)> { (1, 2), (2, 3) });
var right = new Regression(new List<(double x, double y)> { (3, 5), (4, 4), (5, 6) });
Regression combined = left.Merge(right);
Console.WriteLine(combined);

// 5. Curve fits: pass raw (x, y); the transform (here ln y) is applied for you. Slope, PValue, intervals and
//    DataPoints describe the linearized fit (x, ln y); CurveB, PredictY, PredictX and Curve*Interval are in
//    original units. US census 1890-1970, TI Applied Statistics (1977) p. 5-11.
var census = new List<(double x, double y)>
{
    (1890, 62947714), (1900, 75994575), (1910, 91972266), (1920, 105710620), (1930, 122775046),
    (1940, 131669275), (1950, 150697361), (1960, 179323175), (1970, 203235298)
};
var growth = new RegressionPvalue(census, CurveFit.Exponential);   // y = b·e^(m·x)
double b = growth.CurveB;                         // 0.000171644735...
double rate = growth.CurveM;                      // 0.0141183... per year
double pop2000 = growth.PredictY(2000);           // ≈ 314.5 million (the 2000 census counted ≈ 281 million)
double year300M = growth.PredictX(300e6);         // ≈ 1996.65
var curveBand = growth.CurveMeanResponseInterval(2000);   // (Fit, Lower, Upper) in people

var stream = new Regression(CurveFit.Power);      // streaming works too: Add/Dec take raw values
stream.Add(1, 2.1); stream.Add(2, 3.9); stream.Add(3, 6.2);
stream.Dec(3, 6.2);                               // re-enter the raw pair to remove it, as on the TI-59

var custom = new CurveTransform(x: null, y: v => 1 / v, inverseY: v => 1 / v);   // your own g(y) and its inverse
var recip = new Regression(census, custom);
```

### Experimental: q-values (false discovery rate)

When many regressions are screened at once, `RegressionQValue` adjusts a slope's p-value against
the whole family with the Benjamini-Hochberg procedure. This class is experimental and may change.

```csharp
var a = new RegressionQValue(new List<(double x, double y)> { (1, 2), (2, 3), (3, 5), (4, 4), (5, 6) });
var b = new RegressionQValue(new List<(double x, double y)> { (1, 5), (2, 1), (3, 4), (4, 2), (5, 3) });

double qA = a.QValue(new List<RegressionPvalue> { a, b });   // or a.QValue(listOfPValues)
```

## API overview

Formulas, conventions (n − 1 variance, n − 2 df, two-sided p) and a worked example: [docs/MATH.md](docs/MATH.md).

All statistics are read-only **properties** (no parentheses) since version 1.1.0. Operations that take
arguments are methods.

### `Regression` — running sums, no stored points

| Member | Kind | Notes |
|---|---|---|
| `Regression()`, `Regression(x, y)`, `Regression(Regression clone)`, `Regression(IEnumerable<(double x, double y)>)` | constructors | |
| `Add(x, y)` / `Add(x)` | methods | `Add(x)` uses y = sample number (1, 2, 3, ...) |
| `Dec(x, y)` / `Dec(x)` | methods | remove a previously added point; min/max are not rolled back |
| `Merge(Regression other)` | method | returns a new combined `Regression` |
| `N`, `Sx`, `Sy`, `Sx2`, `Sy2`, `Sxy` | properties | count and running sums |
| `MinX`, `MaxX`, `MinY`, `MaxY` | properties | |
| `MeanX`, `MeanY` | properties | 0 when N = 0 |
| `StdDevX`, `StdDevY` | properties | sample (n − 1), = √Variance, numpy `std(ddof=1)`; NaN when N < 2 |
| `VarianceX`, `VarianceY` | properties | sample (n − 1), numpy `var(ddof=1)`; exactly 0 when all values are equal; NaN when N < 2 |
| `VarianceXPopulation`, `VarianceYPopulation`, `StdDevXPopulation`, `StdDevYPopulation` | properties | population (N), Excel `VAR.P` / `STDEV.P`, numpy `ddof=0`; 0 for one point; NaN when N < 1 |
| `Slope`, `YIntercept`, `Correlation`, `RSquared` | properties | NaN when undefined (N < 2, no variation in x, ...) |
| `IsNaN` | property | true when any core statistic is NaN |
| `Regression(CurveFit)`, `Regression(CurveTransform)`, `Regression(IEnumerable<(double x, double y)>, CurveFit)`, `Regression(IEnumerable<(double x, double y)>, CurveTransform)` | constructors | curve fit on raw points; `Add`/`Dec` then take raw x, y and apply the transform. `CurveFit.Custom` throws `ArgumentOutOfRangeException` (use a `CurveTransform`) |
| `Curve`, `Transform` | properties | the `CurveFit` (Linear unless chosen) and its `CurveTransform`. With a curve, every other statistic (sums, means, min/max, `Slope`, `YIntercept`, `Correlation`, `RSquared`) describes the **linearized** fit, e.g. on (x, ln y) |
| `CurveB`, `CurveM` | properties | b and m of the curve in original form: `CurveB` = e^`YIntercept` for Exponential and Power, else `YIntercept`; `CurveM` = `Slope` |
| `PredictY(double x)`, `PredictX(double y)` | methods | prediction in original units (TI's x→y′ and y→x′). NaN when the fit is undefined, the value is outside the transform's domain, a custom transform has no inverse, the slope is 0 (`PredictX`) or the result is not finite. Also work with no curve (plain line) |
| `ToString()` | method | one-line summary |

### `RegressionPvalue : Regression` — keeps the points, tests the slope

| Member | Kind | Notes |
|---|---|---|
| `RegressionPvalue()` | constructor | empty instance |
| `RegressionPvalue(List<(double x, double y)>)` | constructor | |
| `RegressionPvalue(List<(string id, double x, double y)>)` | constructor | ids line up with points; need not be unique |
| `DataPoints`, `IdPoints` | properties | `IEnumerable<...>`; use `.ElementAt(i)` |
| `DataPointsCount()` | method | |
| `PValue` | property | two-tailed t-test of slope ≠ 0, in [0, 1]; cached after first read; **NaN when invalid**: n < 3, any NaN or ±∞ point, no variation in x, or no variation in y. Never 1 or ∞ as a stand-in, never throws. A perfect fit with non-zero slope gives 0 |
| `ConfidenceInterval(double confidenceLevel = 0.95)` | method | `(Lower, Upper)` for the slope; NaN in the same invalid cases as `PValue` (except constant y, which gives `(0, 0)`) or when the level is outside (0, 1) |
| `ConfidenceIntervalPlus(double confidenceLevel = 0.95)` | method | `(Lower, Upper, Slope, StandardError, PValue)`; all NaN when the slope test is invalid |
| `InterceptStandardError` | property | SE of the y-intercept, √(s²(1/n + x̄²/Sxx)) (Excel `LINEST` row 2, col 2); NaN when n < 3, any NaN or ±∞ point, or no variation in x |
| `InterceptPValue` | property | two-tailed t-test of intercept ≠ 0 (df = n − 2), in [0, 1]; NaN in the same cases; a perfect fit with non-zero intercept gives 0 |
| `InterceptConfidenceInterval(double confidenceLevel = 0.95)` | method | `(Lower, Upper)` for the intercept; NaN in the same cases or when the level is outside (0, 1) |
| `MeanResponseInterval(double x, double confidenceLevel = 0.95)` | method | `(Fit, Lower, Upper)` confidence band for the fitted mean at x (Excel-style trend-line band); NaN in the same cases as the slope test, or when x is non-finite, or when the level is outside (0, 1); collapses to Fit when the residual is zero |
| `PredictionInterval(double x, double confidenceLevel = 0.95)` | method | `(Fit, Lower, Upper)` interval for a single new observation at x (wider than `MeanResponseInterval`); same NaN and collapse rules |
| `RegressionPvalue(CurveFit)`, `RegressionPvalue(CurveTransform)` | constructors | empty curve-fit instance; `Add` updates only the sums, so the tests stay NaN (as for `RegressionPvalue()`) |
| `RegressionPvalue(List<(double x, double y)>, CurveFit)`, `RegressionPvalue(List<(double x, double y)>, CurveTransform)`, and the same with `(string id, double x, double y)` | constructors | raw points in, **transformed** points stored in `DataPoints`, so p-values, SEs and intervals all describe the linearized fit. A point outside the domain (ln of a value ≤ 0) becomes NaN, so the tests are NaN |
| `CurveMeanResponseInterval(double x, double confidenceLevel = 0.95)`, `CurvePredictionInterval(double x, double confidenceLevel = 0.95)` | methods | `MeanResponseInterval` / `PredictionInterval` computed at f(x) on the linearized fit, then back-transformed to original units; for Exponential and Power the band is for the median of y and is not symmetric. NaN when the linear interval is NaN, x is outside the domain, or a custom y transform has no inverse |
| `MarginOfError(bool useY = false, double confidenceLevel = 0.95)` | method | `(Mean, MarginOfError)` for mean of x or y; NaN when n < 2, any NaN or ±∞ point, or level outside (0, 1) |
| `ToString()` | method | `Regression` summary plus ` PValue: …` (`n/a` when the p-value is NaN) |

### `RegressionQValue : RegressionPvalue` — experimental

| Member | Kind | Notes |
|---|---|---|
| `QValue(IReadOnlyList<double> pValues)` | method | Benjamini-Hochberg q-value of this slope within the family; NaN family entries are excluded from ranking and m; NaN when this `PValue` is NaN |
| `QValue(IReadOnlyList<RegressionPvalue> regressions)` | method | same, from a list of regressions |
| `BenjaminiHochberg(IReadOnlyList<double> pValues)` | static method | q-values for a whole family in one O(m log m) pass, same order as the input; NaN in → NaN out, excluded from m. Use this for large batches |

## Lineage

- **1978** — algorithms first written for the TI-58 and TI-59 programmable calculators. The running-sum
  design (Σx, Σy, Σx², Σy², Σxy, n) comes from there: it needs no stored data and allows points to be
  removed again.
  Pairs were keyed in one at a time; that is why `Add` and `Dec` exist (see [docs/MATH.md](docs/MATH.md#why-add-and-dec-exist)).
- **1982** — ported to C.
- **1987** — ported to Perl 3.
- **2001 onward** — C# versions, revised in 2001, 2002, 2003, 2008, 2012, 2016, 2018, 2019, 2022 and 2025.
- **2025** — p-values, confidence intervals, margin of error, optional point ids, properties instead of
  methods, published on NuGet as `LinearRegression.Pvalues`.

Some names are traditional rather than modern (for example `Sx2` for Σx²). Older versions used `Qx`/`Qy`
(Q standing in for σ); these became `StdDevX`/`StdDevY` and `VarianceX`/`VarianceY` in 1.0.7.

## Repository layout

| Path | Contents |
|---|---|
| `LinearRegression/` | the library (NuGet package `LinearRegression.Pvalues`) and its package README |
| `StatTest/` | xUnit tests |
| `docs/MATH.md` | formulas and a worked example |
| `CHANGELOG.md` | release notes |
| `.github/workflows/ci.yml` | build and test (Release, net8.0 + net10.0) on pull requests and pushes to `main` |
| `.github/workflows/publish.yml` | publish to nuget.org with Trusted Publishing, then create the GitHub release |

## Releasing

Packages are published by GitHub Actions with nuget.org [Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing)
(OIDC). No long-lived NuGet API key is stored anywhere: the `NuGet/login` action swaps a GitHub OIDC token for a
NuGet API key that is valid for one hour.

One-time setup (already done for this repository):
- nuget.org **Trusted Publishing** policy:
  - package owner `Firefox_xb9r`, scope "Push new packages and package versions", package `LinearRegression.Pvalues`;
  - Repository Owner `NinerXrayBravoTwoTwo`, Repository `linear-regression-pvalues`;
  - Workflow File `publish.yml` (file name only), Environment `publish`.
- The publish job runs in the GitHub environment `publish`. GitHub creates it automatically on the first run. You can
  add protection rules to it later (Settings → Environments), such as required reviewers.
- The workflow logs in as nuget.org user `Firefox_xb9r`, which is not secret. To use another account, set the
  repository variable `NUGET_USER` (Settings → Secrets and variables → Actions → Variables).

To release a version:
1. Set `<Version>` in `LinearRegression/LinearRegression.csproj`, and move the CHANGELOG `[Unreleased]` notes to a new
   `## [X.Y.Z] - date` section (plus `PackageReleaseNotes` and the package README notes). Merge to `main`.
2. Tag the merge commit on `main` and push the tag: `git tag -a vX.Y.Z -m "…" && git push origin vX.Y.Z`.
   The **Publish** workflow then:
   - checks the tag is on `main` and matches the csproj version;
   - tests and packs in Release;
   - pushes the `.nupkg` and `.snupkg` (`--skip-duplicate`);
   - creates the GitHub release from the CHANGELOG section.
3. To publish a tag that already exists (for example `v1.2.0`), use **Actions → Publish → Run workflow**, keep the
   branch at `main`, and enter the tag.

## License

GNU Affero General Public License v3.0 or later (SPDX `AGPL-3.0-or-later`). See [`LICENSE`](LICENSE) for the full text.

## Contact

Jillian England

- Email: jill.england@comcast.net
- X: [@Firefox_XB9R](https://x.com/Firefox_XB9R)
- Facebook: Jillian England Seattle
- Issues and corrections: <https://github.com/NinerXrayBravoTwoTwo/linear-regression-pvalues/issues>

Any omissions or errors in this software or its documentation are unintentional; please report them so they can be corrected.

Copyright © Jillian England, 2001–2025.
