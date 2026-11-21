# Changelog

All notable changes to `LinearRegression.Pvalues`. Dates are release or version-bump dates.
The NuGet package carries the same notes in its README. The git history has notes on every small change.

## [Unreleased]

### Documentation
- Rewrote the root and package READMEs to match the current API (properties, not methods, since 1.1.0;
  `RegressionPvalue` instead of the old `PValueStat`), with installation, usage examples that compile,
  an API overview, lineage, license (AGPL-3.0) and contact.
- Added this changelog.

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

[Unreleased]: https://github.com/NinerXrayBravoTwoTwo/linear-regression-pvalues/compare/472df5e...HEAD
