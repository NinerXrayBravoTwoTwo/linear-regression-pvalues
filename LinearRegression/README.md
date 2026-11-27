# LinearRegression.Pvalues

A small, dependency-light C# library for bivariate (x, y) linear regression. It computes slope,
y-intercept, correlation, R², means, standard deviations and variances from running sums, and adds a
two-tailed p-value for the slope, slope and intercept confidence intervals, a mean-response band and prediction interval at a given x, and a margin of error for the means.
It is aimed at developers, analysts and statisticians who want these numbers without a large
statistics package.

Source: <https://github.com/NinerXrayBravoTwoTwo/linear-regression-pvalues>

## Features

- **Streaming by design** — `Regression` keeps only running sums, so points can be added, removed
  (`Dec`) and whole regressions merged without storing the data.
- **Descriptive statistics** — N, sums, min/max, means, standard deviation (n − 1), variance (n).
- **Fit** — slope, y-intercept, Pearson correlation, R².
- **Inference** (`RegressionPvalue`) — slope p-value (Student t, n − 2 degrees of freedom), confidence
  interval at any level, standard error, intercept inference, a mean-response band around the fitted
  line (`MeanResponseInterval`) and a prediction interval for a new observation, margin of error for the mean of x or y.
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

Formulas and a worked example: [docs/MATH.md](https://github.com/NinerXrayBravoTwoTwo/linear-regression-pvalues/blob/main/docs/MATH.md).

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
| `MarginOfError(bool useY = false, double confidenceLevel = 0.95)` | method | `(Mean, MarginOfError)` for mean of x or y; NaN when n < 2, any NaN or ±∞ point, or level outside (0, 1) |
| `ToString()` | method | `Regression` summary plus ` PValue: …` (`n/a` when the p-value is NaN) |

### `RegressionQValue : RegressionPvalue` — experimental

| Member | Kind | Notes |
|---|---|---|
| `QValue(IReadOnlyList<double> pValues)` | method | Benjamini-Hochberg q-value of this slope within the family; NaN family entries are excluded from ranking and m; NaN when this `PValue` is NaN |
| `QValue(IReadOnlyList<RegressionPvalue> regressions)` | method | same, from a list of regressions |
| `BenjaminiHochberg(IReadOnlyList<double> pValues)` | static method | q-values for a whole family in one O(m log m) pass, same order as the input; NaN in → NaN out, excluded from m. Use this for large batches |

## Release notes
- **Version 1.3.0:  November 22, 2026** (new API, no breaking changes):
- New intercept inference: `InterceptStandardError`, `InterceptPValue` and `InterceptConfidenceInterval` (df = n − 2; matches Excel `LINEST`).
- New opt-in population (N) variance and standard deviation: `VarianceXPopulation`, `VarianceYPopulation`, `StdDevXPopulation`, `StdDevYPopulation` (Excel `VAR.P` / `STDEV.P`). The defaults stay n − 1.
- Fixed: tiny p-values no longer round to 0 (p is now 2·CDF(−|t|)); `Dec` after removing a far-away point now restores the statistics.
- XML docs for every public member, shown by IntelliSense. Formulas and history in [docs/MATH.md](https://github.com/NinerXrayBravoTwoTwo/linear-regression-pvalues/blob/main/docs/MATH.md).
- **Version 1.2.0:  November 21, 2026** (follows 1.1.2 on NuGet; 1.1.3 was never published, and its changes are included here):
- Corrected results: slope standard error (p-values and confidence intervals were too large/wide by √n), StdDev (n − 1) and MarginOfError. Re-run analyses that used 1.1.x.
- Invalid results are now `double.NaN`, never 1, ∞ or an exception: n < 3, NaN or infinite data, no variation in x or y, or a confidence level outside (0, 1). Filter them with `double.IsNaN`.
- Breaking-ish: `VarianceX`/`VarianceY` are now the sample (n − 1) variance, consistent with `StdDevX`/`StdDevY` (values grow by n/(n − 1)); NaN when N < 2.
- Numerically stable Welford accumulation (with reverse updates for `Dec` and a pairwise combine for `Merge`): results stay accurate for data far from zero, e.g. x = 1e9 + small, where 1.1.x returned variance 0 and slope/p-value NaN.
- New `RegressionQValue.BenjaminiHochberg(pValues)`: Benjamini-Hochberg q-values for a whole family in one pass; NaN p-values get NaN q-values and are excluded from the count.
- Fixed min/max when constructed from a list, `Merge(null)`, `Dec(x)`, a live view of the caller's list, and the Benjamini-Hochberg step-up.
- Breaking: removed `Add(Regression)`; use `Merge`.
- `ToString()` includes the p-value and no longer throws on an empty instance. License expression corrected to AGPL-3.0-or-later. Targets net8.0 and net10.0; XML docs included.
- **Version 1.1.3:  August 20, 2025** (never published to NuGet):
- Buffer the p-value calculation to avoid unnecessary recalculations.
- **Version 1.1.2:  August 08, 2025**:
- Fix the signature of the parameterless constructor so empty `RegressionPvalue` instances are reliable.
- **Version 1.1.1:  August 07, 2025**:
- Generalize the Pvalue constructor to use more generalized `IEnumerable<>` instead of `List<>`
This means some of your code may need to be updated with the .ElementAt(x) to replace [i].
Overall this should be a big win in compatibility and portability.
- **Version 1.1.0:  August 07, 2025**:
- Upgraded all the methods into properties so they are accessed without parens. *:) you are welcome*
- Major change to the interface so I bumped a major version number
- Experiment with feature to allow user to save a key value along with the x, y values.
The math does not require an id, that is its feature.  Later processing might want to identify a point, however.
*"Wouldn't it be nice to have an array of ids that lines up with the (x,y) data points?"*
- **Version 1.0.7:  July 22, 2025**: 
- Big possible change.  Renamed legacy Qx, Qy, Qx2, Qy2 to StdDev and Variance. Q was an old stand-in for sigma.
- **Version 1.0.6:  July 21, 2025**: 
- Added `MarginOfError` method which provides a +/- value for MeanX or MeanY
- Split the Confidence Interval work into partial classes to limit impact of changes.
- **Version 1.0.5:  July 21, 2025**: 
- Updated `PValueStat` (now `RegressionPvalue`) to include:
- Added `ConfidenceInterval` method to compute the confidence interval for the regression slope.
- Added `StandardError` as well for the standard error of the regression slope.
- Fixed a possible bug in PValue regression that calculated the variance of 'x'  (called Qx2 by oldtimers) outside the checks build into the `Regression` class.

Full history: [CHANGELOG.md](https://github.com/NinerXrayBravoTwoTwo/linear-regression-pvalues/blob/main/CHANGELOG.md)

## Lineage

- **1978** — algorithms first written for the TI-58 and TI-59 programmable calculators. The running-sum
  design (Σx, Σy, Σx², Σy², Σxy, n) comes from there: it needs no stored data and allows points to be
  removed again.
  Pairs were keyed in one at a time; that is why `Add` and `Dec` exist (see [docs/MATH.md](https://github.com/NinerXrayBravoTwoTwo/linear-regression-pvalues/blob/main/docs/MATH.md#why-add-and-dec-exist)).
- **1982** — ported to C.
- **1987** — ported to Perl 3.
- **2001 onward** — C# versions, revised in 2001, 2002, 2003, 2008, 2012, 2016, 2018, 2019, 2022 and 2025.
- **2025** — p-values, confidence intervals, margin of error, optional point ids, properties instead of
  methods, published on NuGet as `LinearRegression.Pvalues`.

Some names are traditional rather than modern (for example `Sx2` for Σx²). Older versions used `Qx`/`Qy`
(Q standing in for σ); these became `StdDevX`/`StdDevY` and `VarianceX`/`VarianceY` in 1.0.7.

## License

GNU Affero General Public License v3.0 or later (SPDX `AGPL-3.0-or-later`). See [`LICENSE`](https://github.com/NinerXrayBravoTwoTwo/linear-regression-pvalues/blob/main/LICENSE) for the full text.

## Contact

Jillian England

- Email: jill.england@comcast.net
- X: [@Firefox_XB9R](https://x.com/Firefox_XB9R)
- Facebook: Jillian England Seattle
- Issues and corrections: <https://github.com/NinerXrayBravoTwoTwo/linear-regression-pvalues/issues>

Any omissions or errors in this software or its documentation are unintentional; please report them so they can be corrected.

Copyright © Jillian England, 2001–2025.
