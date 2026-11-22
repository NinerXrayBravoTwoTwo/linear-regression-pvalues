using LinearRegression;

namespace RegressionTest;

/// <summary>
///     Issue #14: Dec after Add of a point far from the rest of the data must restore the statistics. The streaming
///     moments are double-double, so the downdate no longer cancels away the remaining sums.
/// </summary>
public class DecPrecisionTests
{
    private const double RelTol = 1e-9;

    private static readonly List<(double x, double y)> Small5 =
    [
        (1e6 + 0.00, 100.00), (1e6 + 0.01, 100.06), (1e6 + 0.02, 100.09), (1e6 + 0.03, 100.16), (1e6 + 0.04, 100.19)
    ];

    private static void AssertRelative(double expected, double actual, double relTol = RelTol)
    {
        Assert.False(double.IsNaN(actual), "unexpected NaN");
        Assert.True(Math.Abs(actual - expected) <= relTol * Math.Abs(expected),
            $"expected {expected:R}, got {actual:R}");
    }

    private static void AssertSameStatistics(Regression expected, Regression actual)
    {
        Assert.Equal(expected.N, actual.N);
        AssertRelative(expected.Slope, actual.Slope);
        AssertRelative(expected.YIntercept, actual.YIntercept);
        AssertRelative(expected.VarianceX, actual.VarianceX);
        AssertRelative(expected.VarianceY, actual.VarianceY);
        AssertRelative(expected.Correlation, actual.Correlation);
        AssertRelative(expected.MeanX, actual.MeanX);
    }

    [Fact]
    public void Issue14_Small5_AddDecFarPoint_RoundTrips()
    {
        // Before the fix: slope 4.8 -> 4.9152, VarianceX 2.5e-4 -> 2.441e-4.
        var r = new Regression(Small5);
        r.Add(0, 0);
        r.Dec(0, 0);
        AssertSameStatistics(new Regression(Small5), r);
    }

    [Fact]
    public void Issue14_Offset50_AddDecFarPoint_RoundTrips()
    {
        // Before the fix: slope 5.0745 -> 5.0400, intercept -5074396.86 -> -5039859.57.
        var data = InterceptStatsTests.Offset50;
        var r = new Regression(data);
        r.Add(123, -45);
        r.Dec(123, -45);
        AssertSameStatistics(new Regression(data), r);
        AssertRelative(5.0744968619169, r.Slope, 1e-12);
        AssertRelative(-5074396.8615448216, r.YIntercept, 1e-9); // mpmath reference
    }

    [Fact]
    public void FarPointAddedFirst_ThenRemoved_RoundTrips()
    {
        // The far point is the streaming reference (first point); removing it leaves data ~1e6 from the reference.
        var r = new Regression(0, 0);
        foreach (var (x, y) in Small5) r.Add(x, y);
        r.Dec(0, 0);
        AssertSameStatistics(new Regression(Small5), r);
    }

    [Fact]
    public void ManyFarOutliers_AddedAndRemovedInterleaved_RoundTrips()
    {
        var rng = new Random(14);
        var data = InterceptStatsTests.Offset50;
        var r = new Regression();
        var outliers = new List<(double x, double y)>();
        foreach (var (x, y) in data)
        {
            r.Add(x, y);
            var o = ((rng.NextDouble() - 0.5) * 2e7, (rng.NextDouble() - 0.5) * 1e5);
            outliers.Add(o);
            r.Add(o.Item1, o.Item2);
        }

        foreach (var (x, y) in outliers.OrderBy(_ => rng.Next())) r.Dec(x, y);

        AssertSameStatistics(new Regression(data), r);
    }

    [Fact]
    public void Clone_KeepsFullPrecisionState()
    {
        var r = new Regression(Small5);
        r.Add(0, 0);
        var clone = new Regression(r);
        clone.Dec(0, 0);
        AssertSameStatistics(new Regression(Small5), clone);
    }

    [Fact]
    public void MergeWithFarBlock_MatchesSinglePass()
    {
        var far = new List<(double x, double y)> { (0, 0), (1, 3), (2, 1) };
        var merged = new Regression(Small5).Merge(new Regression(far));
        AssertSameStatistics(new Regression(Small5.Concat(far)), merged);
    }

    [Fact]
    public void RegressionPvalue_AfterAddDecFarPoint_SlopeTestUnchanged()
    {
        var data = InterceptStatsTests.Offset50;
        var expected = new RegressionPvalue(data);
        var r = new RegressionPvalue(data);
        r.Add(123, -45);
        r.Dec(123, -45);
        AssertRelative(expected.Slope, r.Slope);
        AssertRelative(expected.PValue, r.PValue, 1e-6);
        AssertRelative(expected.InterceptStandardError, r.InterceptStandardError, 1e-6);
    }
}
