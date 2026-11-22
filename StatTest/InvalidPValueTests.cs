using LinearRegression;

namespace RegressionTest;

/// <summary>
///     Invalid inputs must give <see cref="double.NaN" /> (never 1, never infinity, never a throw) from
///     <see cref="RegressionPvalue.PValue" /> and the related CI / margin-of-error members, so large batches
///     of regressions can filter invalid results with <see cref="double.IsNaN" />.
/// </summary>
public class InvalidPValueTests
{
    private static readonly (double x, double y)[] Normal = [(1, 2), (2, 3), (3, 5), (4, 4), (5, 6)];

    private static void AssertAllNaN(RegressionPvalue r)
    {
        Assert.True(double.IsNaN(r.PValue), "PValue");
        var ci = r.ConfidenceInterval();
        Assert.True(double.IsNaN(ci.Lower) && double.IsNaN(ci.Upper), "ConfidenceInterval");
        var plus = r.ConfidenceIntervalPlus();
        Assert.True(double.IsNaN(plus.Lower) && double.IsNaN(plus.Upper) && double.IsNaN(plus.Slope)
                    && double.IsNaN(plus.StandardError) && double.IsNaN(plus.PValue), "ConfidenceIntervalPlus");
        Assert.EndsWith(" PValue: n/a", r.ToString());
    }

    [Theory]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InfiniteY_AllNaN(double inf)
    {
        // was: PValue = +Infinity (b49b96d)
        AssertAllNaN(new RegressionPvalue([(1, 2), (2, inf), (3, 5), (4, 4)]));
    }

    [Theory]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InfiniteX_AllNaN(double inf)
    {
        AssertAllNaN(new RegressionPvalue([(1, 2), (inf, 3), (3, 5), (4, 4)]));
    }

    [Fact]
    public void NaNInX_AllNaN()
    {
        AssertAllNaN(new RegressionPvalue([(1, 2), (double.NaN, 3), (3, 5), (4, 4)]));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void FewerThanThreePoints_AllNaN(int n)
    {
        // was: InvalidOperationException
        AssertAllNaN(new RegressionPvalue(Normal.Take(n).ToList()));
    }

    [Theory]
    [InlineData(2.0)]
    [InlineData(0.1)]          // Sx2 - Sx^2/N rounds to ~1e-17, old code produced slope 0 and CI +/-4.7e9
    [InlineData(100000000.1)]  // old code produced slope 5.96e-8, p = 0.99999999
    public void AllXEqual_AllNaN(double x)
    {
        // was: PValue = 1.0
        var r = new RegressionPvalue([(x, 1), (x, 2), (x, 4), (x, 3)]);
        Assert.True(double.IsNaN(r.Slope));
        AssertAllNaN(r);
    }

    [Theory]
    [InlineData(7.0)]
    [InlineData(0.1)]
    public void ConstantY_PValueNaN_IntervalCollapsesToZero(double y)
    {
        // Slope 0 with zero residual: t = 0/0, so no test is possible. was: PValue = 1.0
        var r = new RegressionPvalue([(1, y), (2, y), (3, y), (4, y)]);
        Assert.Equal(0.0, r.Slope);
        Assert.True(double.IsNaN(r.PValue));
        Assert.Equal((0.0, 0.0), r.ConfidenceInterval());
        var plus = r.ConfidenceIntervalPlus();
        Assert.Equal(0.0, plus.StandardError);
        Assert.True(double.IsNaN(plus.PValue));
        Assert.EndsWith(" PValue: n/a", r.ToString());
    }

    [Fact]
    public void PerfectFit_IsValid_PValueZero()
    {
        // Zero residual with a non-zero slope: SE = 0, t = infinity, p = 0 is the mathematically correct limit.
        var r = new RegressionPvalue([(1, 2), (2, 4), (3, 6), (4, 8)]);
        Assert.Equal(0.0, r.PValue);
        Assert.Equal((2.0, 2.0), r.ConfidenceInterval());
        Assert.Equal((2.0, 2.0, 2.0, 0.0, 0.0), r.ConfidenceIntervalPlus());
        Assert.EndsWith(" PValue: 0", r.ToString());
    }

    [Fact]
    public void PerfectFitWithRounding_IsValid_PValueZero()
    {
        // SE ~ 7e-16 from rounding; t is huge but finite, p underflows to 0.
        var r = new RegressionPvalue([(0.1, 0.2), (0.2, 0.4), (0.3, 0.6), (0.4, 0.8), (0.5, 1.0)]);
        Assert.Equal(0.0, r.PValue);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(1.5)]
    [InlineData(-0.5)]
    [InlineData(double.NaN)]
    public void InvalidConfidenceLevel_BoundsNaN_PValueStillValid(double level)
    {
        // was: MathNet NonConvergenceException (1.5) or a degenerate interval
        var r = new RegressionPvalue(Normal.ToList());
        var ci = r.ConfidenceInterval(level);
        Assert.True(double.IsNaN(ci.Lower) && double.IsNaN(ci.Upper));
        var plus = r.ConfidenceIntervalPlus(level);
        Assert.True(double.IsNaN(plus.Lower) && double.IsNaN(plus.Upper));
        Assert.Equal(0.9, plus.Slope, 12);
        Assert.Equal(0.03738607346849862, plus.PValue, 12);
        Assert.True(double.IsNaN(r.MarginOfError(false, level).MarginOfError));
    }

    [Fact]
    public void MarginOfError_InfiniteData_NaN()
    {
        // was: finite (2, 2.484) for +Inf in y with useY = false, (-Inf, NaN) for -Inf in x
        var r = new RegressionPvalue([(1, 2), (2, double.PositiveInfinity), (3, 5), (4, 4)]);
        Assert.Equal((double.NaN, double.NaN), r.MarginOfError());
        Assert.Equal((double.NaN, double.NaN), r.MarginOfError(useY: true));
    }

    [Fact]
    public void ValidData_PValueIsNeverNaN_AndInRange()
    {
        var r = new RegressionPvalue(Normal.ToList());
        Assert.InRange(r.PValue, 0.0, 1.0);
        Assert.Equal(0.03738607346849862, r.PValue, 12);
    }

    [Fact]
    public void ValidButFlatSlope_PValueOne()
    {
        // A genuine p = 1 (slope exactly 0 with non-zero residual) is still reported as 1, not NaN.
        var r = new RegressionPvalue([(1, 1), (2, 3), (3, 3), (4, 1)]);
        Assert.Equal(0.0, r.Slope, 12);
        Assert.Equal(1.0, r.PValue, 12);
    }
}
