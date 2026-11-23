using LinearRegression;

namespace RegressionTest;

/// <summary>
///     Population (N) variance and standard deviation: <see cref="Regression.VarianceXPopulation" />,
///     <see cref="Regression.VarianceYPopulation" />, <see cref="Regression.StdDevXPopulation" />,
///     <see cref="Regression.StdDevYPopulation" />. References: hand values and numpy <c>var/std(ddof=0)</c>
///     (= Excel VAR.P / STDEV.P).
/// </summary>
public class PopulationVarianceTests
{
    private static readonly List<(double x, double y)> Tiny5 = [(1, 2), (2, 4), (3, 5), (4, 4), (5, 5)];

    private static readonly List<(double x, double y)> Mixed7 =
    [
        (3.1, 10.2), (-2.7, 3.3), (8.4, -4.4), (0.05, 6.0), (5.5, 2.5), (-1.25, 9.9), (7.0, 1.1)
    ];

    [Fact]
    public void Tiny5_HandValues()
    {
        // Sxx = 10, Syy = 6, n = 5: VAR.P x = 2, y = 1.2; VAR.S x = 2.5, y = 1.5.
        var r = new Regression(Tiny5);
        Assert.Equal(2.0, r.VarianceXPopulation, 12);
        Assert.Equal(1.2, r.VarianceYPopulation, 12);
        Assert.Equal(Math.Sqrt(2.0), r.StdDevXPopulation, 12);
        Assert.Equal(Math.Sqrt(1.2), r.StdDevYPopulation, 12);

        // The defaults are unchanged (sample, n - 1).
        Assert.Equal(2.5, r.VarianceX, 12);
        Assert.Equal(1.5, r.VarianceY, 12);
    }

    [Fact]
    public void Mixed7_MatchesNumpyDdof0()
    {
        var r = new Regression(Mixed7);
        Assert.Equal(15.794183673469385, r.VarianceXPopulation, 12);
        Assert.Equal(3.97418968765576, r.StdDevXPopulation, 12);
        Assert.Equal(22.701224489795923, r.VarianceYPopulation, 12);
        Assert.Equal(4.764580200793762, r.StdDevYPopulation, 12);
    }

    [Fact]
    public void PopulationEqualsSampleTimesNMinus1OverN()
    {
        var r = new Regression(Mixed7);
        var factor = (r.N - 1) / r.N;
        Assert.Equal(r.VarianceX * factor, r.VarianceXPopulation, 12);
        Assert.Equal(r.VarianceY * factor, r.VarianceYPopulation, 12);
    }

    [Fact]
    public void FarFromZero_StaysAccurate()
    {
        // x = 1e9 + 1..5: VAR.P = 2 exactly; the raw-sum formula would lose every digit here.
        var r = new Regression(Tiny5.Select(p => (p.x + 1e9, p.y)));
        Assert.Equal(2.0, r.VarianceXPopulation, 9);
        Assert.Equal(Math.Sqrt(2.0), r.StdDevXPopulation, 9);
    }

    [Fact]
    public void Empty_IsNaN()
    {
        var r = new Regression();
        Assert.True(double.IsNaN(r.VarianceXPopulation));
        Assert.True(double.IsNaN(r.VarianceYPopulation));
        Assert.True(double.IsNaN(r.StdDevXPopulation));
        Assert.True(double.IsNaN(r.StdDevYPopulation));
    }

    [Fact]
    public void SinglePoint_IsZero_WhileSampleIsNaN()
    {
        var r = new Regression(3.5, -2.0);
        Assert.Equal(0.0, r.VarianceXPopulation);
        Assert.Equal(0.0, r.StdDevYPopulation);
        Assert.True(double.IsNaN(r.VarianceX));
    }

    [Fact]
    public void ConstantValues_ExactlyZero()
    {
        var r = new Regression([(0.1, 7.0), (0.1, 7.0), (0.1, 7.0)]);
        Assert.Equal(0.0, r.VarianceXPopulation);
        Assert.Equal(0.0, r.VarianceYPopulation);
    }

    [Theory]
    [InlineData(double.NaN, 1.0)]
    [InlineData(1.0, double.NaN)]
    [InlineData(double.PositiveInfinity, 1.0)]
    [InlineData(1.0, double.NegativeInfinity)]
    public void NonFiniteData_IsNaN(double badX, double badY)
    {
        var r = new Regression([(1, 2), (2, 3), (badX, badY)]);
        var xBad = !double.IsFinite(badX);
        Assert.True(double.IsNaN(xBad ? r.VarianceXPopulation : r.VarianceYPopulation));
        Assert.True(double.IsNaN(xBad ? r.StdDevXPopulation : r.StdDevYPopulation));
    }

    [Fact]
    public void MergeAndDec_MatchSinglePass()
    {
        var single = new Regression(Mixed7);
        var merged = new Regression(Mixed7.Take(3)).Merge(new Regression(Mixed7.Skip(3)));
        Assert.Equal(single.VarianceXPopulation, merged.VarianceXPopulation, 12);
        Assert.Equal(single.VarianceYPopulation, merged.VarianceYPopulation, 12);

        var dec = new Regression(Mixed7);
        dec.Add(4.0, 4.0);
        dec.Dec(4.0, 4.0);
        Assert.Equal(single.VarianceXPopulation, dec.VarianceXPopulation, 12);
        Assert.Equal(single.StdDevYPopulation, dec.StdDevYPopulation, 12);
    }
}
