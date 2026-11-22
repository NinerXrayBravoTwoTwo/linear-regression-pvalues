using LinearRegression;

namespace RegressionTest;

/// <summary>
///     Divisor check: <see cref="Regression.StdDevX" />, <see cref="Regression.StdDevY" />,
///     <see cref="Regression.VarianceX" /> and <see cref="Regression.VarianceY" /> all use the sample
///     divisor n - 1 (Jill's decision: real medical-study data are samples, not whole populations).
/// </summary>
/// <remarks>
///     Reference values: numpy 2.x, <c>np.std(a, ddof=1)</c> / <c>np.var(a, ddof=1)</c> (n - 1, "sample"),
///     with the ddof=0 (n, "population") values shown for comparison. The x data is the classic textbook
///     example {2, 4, 4, 4, 5, 5, 7, 9}, whose population standard deviation is exactly 2.
///     Before this change Variance used n, so StdDevX² != VarianceX.
/// </remarks>
public class DivisorComparisonTests(ITestOutputHelper output)
{
    private static readonly List<(double x, double y)> Data =
    [
        (2, 1), (4, 3), (4, 2), (4, 5), (5, 4), (5, 6), (7, 8), (9, 7)
    ];

    // numpy reference values
    private const double SampleStdDevX = 2.138089935299395;      // ddof=1, n - 1
    private const double PopulationStdDevX = 2.0;                // ddof=0, n
    private const double PopulationVarianceX = 4.0;              // ddof=0, n (old VarianceX)
    private const double SampleVarianceX = 4.571428571428571;    // ddof=1, n - 1 (= 32/7)

    private const double SampleStdDevY = 2.449489742783178;
    private const double PopulationStdDevY = 2.29128784747792;
    private const double PopulationVarianceY = 5.25;             // old VarianceY
    private const double SampleVarianceY = 6.0;

    [Fact]
    public void StdDev_And_Variance_Both_Use_NMinus1()
    {
        var r = new Regression(Data);

        output.WriteLine("Statistic   library             n-1 (sample)        n (population)");
        output.WriteLine($"StdDevX     {r.StdDevX,-19:R} {SampleStdDevX,-19:R} {PopulationStdDevX:R}");
        output.WriteLine($"StdDevY     {r.StdDevY,-19:R} {SampleStdDevY,-19:R} {PopulationStdDevY:R}");
        output.WriteLine($"VarianceX   {r.VarianceX,-19:R} {SampleVarianceX,-19:R} {PopulationVarianceX:R}");
        output.WriteLine($"VarianceY   {r.VarianceY,-19:R} {SampleVarianceY,-19:R} {PopulationVarianceY:R}");

        Assert.Equal(SampleStdDevX, r.StdDevX, 12);
        Assert.Equal(SampleStdDevY, r.StdDevY, 12);
        Assert.Equal(SampleVarianceX, r.VarianceX, 12);
        Assert.Equal(SampleVarianceY, r.VarianceY, 12);

        // StdDev and Variance are now each other's square / square root.
        Assert.Equal(r.VarianceX, r.StdDevX * r.StdDevX, 12);
        Assert.Equal(r.VarianceY, r.StdDevY * r.StdDevY, 12);

        // The change from the old (n) value is exactly n / (n - 1).
        var n = Data.Count;
        Assert.Equal(PopulationVarianceX * n / (n - 1), r.VarianceX, 12);
        Assert.Equal(PopulationVarianceY * n / (n - 1), r.VarianceY, 12);
    }
}
