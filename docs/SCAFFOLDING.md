# Scaffolding review (2026-11-21)

A record of unfinished or unused code found in a scan of `main` at `eb85e7b`, and what happened to
each item, so nothing disappears silently. Statistics did not change. Tests: 86 passed before, 87 passed after.

| # | Item (where) | What it was | Decision | Commit |
|---|---|---|---|---|
| 1 | `LinearRegression/Regresion.cs` | Misspelled file name | **Renamed** to `Regression.cs` with `git mv` (`git log --follow` keeps history) | `05a8c56` |
| 2 | `//public double SlopeI()` in `Regression` | Commented-out verbatim copy of `Slope`. It sat between Slope's XML doc and `Slope`, so the doc had no member to attach to | **Removed**. The doc now attaches to `Slope` | `a994f52` |
| 3 | Commented-out `throw new InvalidOperationException(...)` in `StdDevX`/`StdDevY` | An older choice to throw when N < 2 | **Removed**. The current behaviour (return NaN when N < 2) is kept and noted in a comment. The n vs n−1 divisor question is untouched | `a994f52` |
| 4 | Comment in `YIntercept`: "If something needs to be thrown it will happen in Slope()" | Stale: `Slope` returns NaN and never throws | **Replaced** with an accurate one-line comment | `a994f52` |
| 5 | `RegressionQValue`: commented-out `_isSavedQvalue`/`_savedQvalue` cache | An unfinished cache copied from `PValue` | **Removed**. A comment explains why there's no cache: a q-value depends on the family of p-values passed in | `a994f52` |
| 6 | `//ToDo: create a more detailed ToString method in RegressionPvalue` (`PValueTest`) | Unfinished feature | **Finished**. `RegressionPvalue.ToString()` is `Regression.ToString()` + ` PValue: <G4>`, or `n/a (needs 3+ points)` when n < 3. Two tests added | `e166e47` |
| 7 | Duplicated, commented-out `//testOutputHelper.WriteLine(...)` (`PValueTest`) | Leftover line | **Removed** | `e166e47` |
| 8 | `TestConfidenceIntervalWithNegativeData` (`ConfidenceIntervalTest`) | The negative data set was commented out, so the test ran on positive data and asserted nothing | **Finished**. Negative data restored. It asserts slope 0.9 and the SciPy 95% CI [0.0991, 1.7009] | `0726d4a` |
| 9 | `StatAddStatWithNaN` (`RegressionTest`) | A verbatim copy of `StatAddStat` with no NaN in it | **Finished**. It now adds a NaN point and checks that combining regressions keeps N and x sums while NaN propagates to the y sums and `IsNaN` | `0726d4a` |
| 10 | Three commented-out asserts on `stat.DataPoints` / `DataPointsCount()` (`RegressionTest`) | `Regression` has no `DataPoints` (only `RegressionPvalue` does, and it is tested there) | **Removed** | `0726d4a` |
| 11 | "Ensure this namespace ... contains the 'Statistic' class" (`RegressionTest`) | Stale comment from an old class name | **Removed** | `0726d4a` |
| 12 | "Assuming a QValueCalculator class exists" (`QValueTest`) | Stale comment | **Replaced**: it's a test-only reference implementation, also used by `RegressionQValueTests` | `0726d4a` |
| 13 | `RegressionPvalue` as a `partial` class over 3 files | Intentional since 1.0.6, to separate confidence-interval and margin-of-error code | **Kept**, documented here | — |
| 14 | `RegressionQValue` marked experimental | Working and tested since PR 3 | **Kept** as experimental | — |
| 15 | `StatTest/QValueCalculator.cs` | Test-only Benjamini-Hochberg implementation | **Kept** as a test reference | — |
| 16 | `Regression.Add(Regression other)` | Older way to combine two regressions in place; its doc said "DEPRECATED, use Merge" | **Removed** (Jill's decision, 2026-11-21). `Merge` replaces it. Callers updated: `RegressionTest.StatAddStat` and `StatAddStatWithNaN` now use `clone = clone.Merge(original)`. Before removal, a check showed bit-identical sums, N and min/max (and the same `IsNaN`) for both on normal, NaN, ±∞ and empty inputs. Differences: `Merge` returns a new instance instead of changing the caller, and `Merge(null)` throws `ArgumentNullException` where `Add(null)` threw `NullReferenceException`. Breaking change, noted in CHANGELOG | "Remove Add(Regression); Merge replaces it" (PR 4) |

## Left for Jill to decide (not changed)

- `RegressionTest.StatTestAccessDataPoints` and `RegressionTest.KnownDatasetStat` are identical tests. Keep both, remove one, or turn one into a different check.
- `PValueWithSingleDataPointReturnsNaN` and `PValueWithTwoDataPointsReturnsNaN` assert a throw, not NaN. The names are misleading.
- `PValueWithMixedDataTypesReturnsNaN`: its comment mentions "string, null", but the data is only a NaN.
- `TestConfidenceIntervalWithZeroData` uses random data for its first point. It only checks that `ConfidenceInterval` and `ConfidenceIntervalPlus` agree, so it can't fail on a value, but the name doesn't describe it.
- `Regression.ToString()` throws `DivideByZeroException` on an empty instance (via `VarianceX`), so `new RegressionPvalue().ToString()` throws too. Found while finishing item 6. Not changed, because `VarianceX` behaviour for N = 0 is part of the open divisor and variance question.
- `QValueTest` has an unused `testOutputHelper` parameter (warning CS9113).
- XML `cref="RegressionQvalue"` (wrong case) in `RegressionQValue` constructors. Fix in the XML-docs PR.
