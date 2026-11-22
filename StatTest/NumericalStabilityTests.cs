using LinearRegression;

namespace RegressionTest;

/// <summary>
///     Numerical stability of the streaming (Welford) accumulation: results must not depend on where the data
///     sit on the number line. Before the switch, the raw-sum formulas (Sx2 - Sx^2/N) cancelled catastrophically:
///     with x = 1e9 + small, VarianceX became 0 and Slope/PValue NaN.
///     References: numpy <c>var(ddof=1)</c> and scipy <c>linregress</c> on the same doubles.
/// </summary>
public class NumericalStabilityTests
{
    private static readonly (double x, double y)[] Five = [(1, 2), (2, 3), (3, 5), (4, 4), (5, 6)];

    // Anscombe I; x are integers, so x + 1e9 or 1e12 is exact and the true statistics are unchanged.
    private static readonly (double x, double y)[] AnscombeI =
    [
        (10, 8.04), (8, 6.95), (13, 7.58), (9, 8.81), (11, 8.33), (14, 9.96),
        (6, 7.24), (4, 4.26), (12, 10.84), (7, 4.82), (5, 5.68)
    ];

    private static List<(double x, double y)> Shift(IEnumerable<(double x, double y)> d, double sx, double sy) =>
        d.Select(p => (p.x + sx, p.y + sy)).ToList();

    private static void AssertRel(double expected, double actual, double tol = 1e-12)
    {
        Assert.False(double.IsNaN(actual), $"expected {expected:R}, got NaN");
        Assert.True(Math.Abs(actual - expected) <= tol * Math.Abs(expected),
            $"expected {expected:R}, got {actual:R} (rel err {Math.Abs(actual - expected) / Math.Abs(expected):E1})");
    }

    private static void AssertInvariant(RegressionPvalue baseline, RegressionPvalue shifted)
    {
        AssertRel(baseline.Slope, shifted.Slope);
        AssertRel(baseline.VarianceX, shifted.VarianceX);
        AssertRel(baseline.VarianceY, shifted.VarianceY);
        AssertRel(baseline.StdDevX, shifted.StdDevX);
        AssertRel(baseline.Correlation, shifted.Correlation);
        AssertRel(baseline.PValue, shifted.PValue);
        var b = baseline.ConfidenceIntervalPlus();
        var s = shifted.ConfidenceIntervalPlus();
        AssertRel(b.StandardError, s.StandardError);
        AssertRel(b.Lower, s.Lower);
        AssertRel(b.Upper, s.Upper);
    }

    [Theory]
    [InlineData(1e6, 0.0)]
    [InlineData(1e9, 0.0)]
    [InlineData(0.0, 1e9)]
    [InlineData(1e9, 1e9)]
    [InlineData(1e12, 0.0)]
    [InlineData(-1e9, 1e6)]
    public void ShiftedData_FivePoint_Invariant(double sx, double sy)
    {
        // Five-point data are integers, so every shift here is exact. On main, (1e9, 0) gave Slope NaN, VarianceX 0.
        AssertInvariant(new RegressionPvalue(Five.ToList()), new RegressionPvalue(Shift(Five, sx, sy)));
    }

    [Theory]
    [InlineData(1e6)]
    [InlineData(1e9)]
    [InlineData(1e12)]
    public void ShiftedData_AnscombeX_Invariant(double sx)
    {
        AssertInvariant(new RegressionPvalue(AnscombeI.ToList()), new RegressionPvalue(Shift(AnscombeI, sx, 0)));
    }

    [Fact]
    public void ShiftedData_AnscombeY_MatchesScipyOnShiftedDoubles()
    {
        // y + 1e9 is not exact (ulp 1.2e-7), so compare with scipy linregress on the shifted doubles
        // (slope 0.5000909144228155) rather than with the unshifted value. main: 0.5000909978693182, VarianceY 0.
        var r = new RegressionPvalue(Shift(AnscombeI, 0, 1e9));
        AssertRel(0.5000909144228155, r.Slope, 1e-14);
        AssertRel(4.127269112070258, r.VarianceY, 1e-14);
    }

    [Fact]
    public void CatastrophicCancellation_MatchesNumpy()
    {
        // x = 1e9 + {4, 7, 13, 16, 10, 1, 19}: numpy var(ddof=1) 42, scipy linregress below.
        // main: VarianceX 0, Slope NaN.
        double[] dx = [4, 7, 13, 16, 10, 1, 19];
        double[] y = [2.5, 3.1, 6.2, 7.9, 5.0, 0.7, 9.6];
        var r = new RegressionPvalue(dx.Select((d, i) => (1e9 + d, y[i])).ToList());

        AssertRel(42.0, r.VarianceX, 1e-14);
        AssertRel(9.893333333333333, r.VarianceY, 1e-14);
        AssertRel(0.4833333333333333, r.Slope, 1e-14);
        AssertRel(-483333333.1666666, r.YIntercept, 1e-14);
        AssertRel(0.9958640886279952, r.Correlation, 1e-14);
        AssertRel(2.1082347515311324e-06, r.PValue, 1e-9);
        AssertRel(0.019720265943665834, r.ConfidenceIntervalPlus().StandardError, 1e-12);
    }

    [Fact]
    public void ClassicWelfordExample_Variance30()
    {
        // numpy.var([1e9+4, 1e9+7, 1e9+13, 1e9+16], ddof=1) = 30; the raw-sum formula gives 0 (or negative).
        var r = new Regression([(1e9 + 4, 0), (1e9 + 7, 1), (1e9 + 13, 2), (1e9 + 16, 3)]);
        Assert.Equal(30.0, r.VarianceX, 12);
    }

    [Fact]
    public void DecAfterAdd_RestoresState()
    {
        var data = Shift(AnscombeI, 1e9, 0);
        var reference = new Regression(data);
        var r = new Regression(data);

        (double x, double y)[] extra = [(1e9 + 100, 50), (1e9 - 3, -2), (1e9 + 0.5, 7.25)];
        foreach (var (x, y) in extra) r.Add(x, y);
        Assert.NotEqual(reference.Slope, r.Slope, 6);
        foreach (var (x, y) in extra.Reverse()) r.Dec(x, y);

        Assert.Equal(reference.N, r.N);
        AssertRel(reference.Slope, r.Slope, 1e-12);
        AssertRel(reference.VarianceX, r.VarianceX, 1e-12);
        AssertRel(reference.VarianceY, r.VarianceY, 1e-12);
        AssertRel(reference.YIntercept, r.YIntercept, 1e-12);
        AssertRel(reference.Correlation, r.Correlation, 1e-12);
        Assert.Equal(reference.Sx, r.Sx);
    }

    [Fact]
    public void DecInDifferentOrder_RemovesPoints()
    {
        // Removing points that were not the most recent ones: result equals a fresh pass over the rest.
        var all = Shift(Five, 1e9, 0);
        var r = new Regression(all);
        r.Dec(all[0].x, all[0].y);
        r.Dec(all[2].x, all[2].y);

        var rest = new Regression([all[1], all[3], all[4]]);
        AssertRel(rest.Slope, r.Slope, 1e-12);
        AssertRel(rest.VarianceX, r.VarianceX, 1e-12);
        AssertRel(rest.VarianceY, r.VarianceY, 1e-12);
    }

    [Fact]
    public void DecToEmpty_ThenAdd_StartsFresh()
    {
        var r = new Regression([(1e9 + 1, 2.0), (1e9 + 2, 3.0)]);
        r.Dec(1e9 + 2, 3.0);
        r.Dec(1e9 + 1, 2.0);
        Assert.Equal(0, r.N);
        Assert.True(double.IsNaN(r.VarianceX));

        foreach (var (x, y) in Five) r.Add(x, y);
        AssertRel(0.9, r.Slope, 1e-14);
        AssertRel(2.5, r.VarianceX, 1e-14);
    }

    [Fact]
    public void DecSingleArgument_ReversesAddSingleArgument()
    {
        var r = new Regression();
        foreach (var x in new[] { 1e9 + 3, 1e9 + 1, 1e9 + 4, 1e9 + 1, 1e9 + 5 }) r.Add(x);
        var before = new Regression(r);
        r.Add(1e9 + 9);
        r.Dec(1e9 + 9);
        AssertRel(before.Slope, r.Slope, 1e-12);
        AssertRel(before.VarianceX, r.VarianceX, 1e-12);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1e9)]
    public void Merge_EqualsSinglePass(double shift)
    {
        var all = Shift(AnscombeI, shift, shift == 0 ? 0 : 1e6);
        var single = new Regression(all);
        var merged = new Regression(all.Take(4)).Merge(new Regression(all.Skip(4)));

        Assert.Equal(single.N, merged.N);
        AssertRel(single.Slope, merged.Slope, 1e-12);
        AssertRel(single.VarianceX, merged.VarianceX, 1e-12);
        AssertRel(single.VarianceY, merged.VarianceY, 1e-12);
        AssertRel(single.YIntercept, merged.YIntercept, 1e-12);
        AssertRel(single.Correlation, merged.Correlation, 1e-12);
        Assert.Equal(single.MinX, merged.MinX);
        Assert.Equal(single.MaxY, merged.MaxY);
    }

    [Fact]
    public void Merge_WithEmpty_IsIdentity()
    {
        var a = new Regression(Shift(Five, 1e9, 0));
        foreach (var m in new[] { a.Merge(new Regression()), new Regression().Merge(a) })
        {
            Assert.Equal(a.N, m.N);
            AssertRel(a.Slope, m.Slope, 1e-15);
            AssertRel(a.VarianceX, m.VarianceX, 1e-15);
        }

        Assert.True(double.IsNaN(new Regression().Merge(new Regression()).VarianceX));
    }

    [Fact]
    public void Merge_NaNSide_PropagatesNaN()
    {
        var a = new Regression(Five);
        var b = new Regression([(1.0, double.NaN), (2.0, 3.0)]);
        var m = a.Merge(b);
        Assert.True(m.IsNaN);
        Assert.True(double.IsNaN(m.Slope));
        Assert.True(double.IsNaN(m.VarianceY));
    }

    [Fact]
    public void Clone_KeepsStreamingState()
    {
        var a = new Regression(Shift(Five, 1e9, 0));
        var c = new Regression(a);
        AssertRel(0.9, c.Slope, 1e-14);
        AssertRel(2.5, c.VarianceX, 1e-14);
    }

    [Fact]
    public void InfinityAndNaN_SemanticsUnchanged()
    {
        // Same values as the raw-sum implementation: means keep +inf, the rest is NaN.
        var inf = new Regression([(1.0, 2.0), (2.0, double.PositiveInfinity), (3.0, 5.0)]);
        Assert.Equal(double.PositiveInfinity, inf.MeanY);
        Assert.True(double.IsNaN(inf.VarianceY));
        Assert.True(double.IsNaN(inf.Slope));
        Assert.True(inf.IsNaN);

        var nan = new Regression([(1.0, 2.0), (double.NaN, 3.0), (3.0, 5.0)]);
        Assert.True(double.IsNaN(nan.MeanX));
        Assert.True(double.IsNaN(nan.VarianceX));
        Assert.True(double.IsNaN(nan.Slope));

        // Removing the bad point does not "heal" the instance (raw sums are NaN), as before.
        nan.Dec(double.NaN, 3.0);
        Assert.True(double.IsNaN(nan.Slope));
    }

    [Fact]
    public void SettingRawSums_FallsBackToRawSumFormulas()
    {
        // Public setters still work: the statistics follow the raw sums that were set.
        var r = new Regression(Five);
        r.Sxy += 1.0; // 69 -> 70
        // raw-sum slope: (70 - 15*20/5) / (55 - 15^2/5) = 10 / 10
        AssertRel(1.0, r.Slope, 1e-14);
    }
}
