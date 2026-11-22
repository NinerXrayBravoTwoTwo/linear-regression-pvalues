using LinearRegression;

namespace RegressionTest;

/// <summary>
///     Sample (n - 1) variance and standard deviation edge cases, and <see cref="Regression.ToString" /> for
///     tiny inputs. Reference values: numpy <c>var(ddof=1)</c> / <c>std(ddof=1)</c>.
/// </summary>
public class VarianceTests
{
    [Fact]
    public void Empty_VarianceAndStdDev_AreNaN_NoThrow()
    {
        // VarianceX/Y used to throw DivideByZeroException here
        var r = new Regression();
        Assert.True(double.IsNaN(r.VarianceX));
        Assert.True(double.IsNaN(r.VarianceY));
        Assert.True(double.IsNaN(r.StdDevX));
        Assert.True(double.IsNaN(r.StdDevY));
    }

    [Fact]
    public void OnePoint_VarianceAndStdDev_AreNaN()
    {
        // VarianceX/Y used to be 0 here (population formula); a sample variance needs 2 points (numpy gives nan)
        var r = new Regression([(1.0, 2.0)]);
        Assert.True(double.IsNaN(r.VarianceX));
        Assert.True(double.IsNaN(r.VarianceY));
        Assert.True(double.IsNaN(r.StdDevX));
        Assert.True(double.IsNaN(r.StdDevY));
    }

    [Fact]
    public void TwoPoints_SampleVariance()
    {
        // numpy.var([1, 2], ddof=1) = 0.5 (was 0.25)
        var r = new Regression([(1.0, 2.0), (2.0, 3.0)]);
        Assert.Equal(0.5, r.VarianceX, 12);
        Assert.Equal(0.5, r.VarianceY, 12);
        Assert.Equal(Math.Sqrt(0.5), r.StdDevX, 12);
    }

    [Fact]
    public void ToString_Empty_DoesNotThrow()
    {
        var text = new Regression().ToString();
        Assert.Contains("N=0", text);
        Assert.Contains("Variance: (xNaN yNaN)", text);
        Assert.Contains("MinMax: x(n/a) y(n/a)", text);
    }

    [Fact]
    public void ToString_OnePoint_DoesNotThrow()
    {
        var text = new Regression([(1.0, 2.0)]).ToString();
        Assert.Contains("N=1", text);
        Assert.Contains("Variance: (xNaN yNaN)", text);
        Assert.Contains("MinMax: x(1 <-> 1) y(2 <-> 2)", text);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(100000000.1)]
    public void AllEqual_VarianceIsExactlyZero(double x)
    {
        // Running sums leave rounding noise (StdDevX was 1.3e-9 for x = 0.1); all-equal is now exactly 0.
        var r = new Regression([(x, 1), (x, 5), (x, 9), (x, 2), (x, 7)]);
        Assert.Equal(0.0, r.VarianceX);
        Assert.Equal(0.0, r.StdDevX);
        Assert.Equal(11.2, r.VarianceY, 12); // numpy.var([1, 5, 9, 2, 7], ddof=1)
    }

    [Fact]
    public void NaNOrInfinity_VarianceIsNaN()
    {
        Assert.True(double.IsNaN(new Regression([(1.0, 2.0), (double.NaN, 3.0), (3.0, 4.0)]).VarianceX));
        Assert.True(double.IsNaN(new Regression([(1.0, 2.0), (2.0, double.PositiveInfinity), (3.0, 4.0)]).VarianceY));
    }

    [Fact]
    public void Merge_And_Dec_KeepSampleVariance()
    {
        // numpy.var([1, 2, 3, 4, 5], ddof=1) = 2.5; numpy.var([1, 2, 3, 4], ddof=1) = 1.6666...
        var a = new Regression([(1.0, 1.0), (2.0, 2.0)]);
        var b = new Regression([(3.0, 3.0), (4.0, 4.0), (5.0, 5.0)]);
        var merged = a.Merge(b);
        Assert.Equal(2.5, merged.VarianceX, 12);

        merged.Dec(5.0, 5.0);
        Assert.Equal(5.0 / 3.0, merged.VarianceX, 12);
    }
}
