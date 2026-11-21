using LinearRegression;
using Xunit.Abstractions;

namespace RegressionTest;

/// <summary>
///     Decision aid, not a bug test: shows which divisor (n or n - 1) <see cref="Regression.StdDevX" />,
///     <see cref="Regression.StdDevY" />, <see cref="Regression.VarianceX" /> and <see cref="Regression.VarianceY" />
///     use today, next to the alternative, on one small dataset.
/// </summary>
/// <remarks>
///     Reference values: numpy 2.x, <c>np.std(a, ddof=1)</c> (n - 1, "sample"), <c>np.std(a, ddof=0)</c>
///     (n, "population"), and the same for <c>np.var</c>. The x data is the classic textbook example
///     {2, 4, 4, 4, 5, 5, 7, 9}, whose population standard deviation is exactly 2.
///     Current behaviour: StdDev uses n - 1, Variance uses n, so StdDevX² != VarianceX.
/// </remarks>
public class DivisorComparisonTests(Xunit.ITestOutputHelper output)
{
    private static readonly List<(double x, double y)> Data =
    [
        (2, 1), (4, 3), (4, 2), (4, 5), (5, 4), (5, 6), (7, 8), (9, 7)
    ];

    // numpy reference values
    private const double SampleStdDevX = 2.138089935299395;      // ddof=1, n - 1
    private const double PopulationStdDevX = 2.0;                // ddof=0, n
    private const double PopulationVarianceX = 4.0;              // ddof=0, n
    private const double SampleVarianceX = 4.571428571428571;    // ddof=1, n - 1 (= 32/7)

    private const double SampleStdDevY = 2.449489742783178;
    private const double PopulationStdDevY = 2.29128784747792;
    private const double PopulationVarianceY = 5.25;
    private const double SampleVarianceY = 6.0;

    [Fact]
    public void StdDev_Uses_NMinus1_And_Variance_Uses_N_SideBySide()
    {
        var r = new Regression(Data);

        output.WriteLine("Statistic   current (library)   n-1 (sample)        n (population)");
        output.WriteLine($"StdDevX     {r.StdDevX,-19:R} {SampleStdDevX,-19:R} {PopulationStdDevX:R}");
        output.WriteLine($"StdDevY     {r.StdDevY,-19:R} {SampleStdDevY,-19:R} {PopulationStdDevY:R}");
        output.WriteLine($"VarianceX   {r.VarianceX,-19:R} {SampleVarianceX,-19:R} {PopulationVarianceX:R}");
        output.WriteLine($"VarianceY   {r.VarianceY,-19:R} {SampleVarianceY,-19:R} {PopulationVarianceY:R}");

        // Today: StdDev = sample (n - 1) ...
        Assert.Equal(SampleStdDevX, r.StdDevX, 12);
        Assert.Equal(SampleStdDevY, r.StdDevY, 12);

        // ... while Variance = population (n).
        Assert.Equal(PopulationVarianceX, r.VarianceX, 12);
        Assert.Equal(PopulationVarianceY, r.VarianceY, 12);

        // So the two are not each other's square / square root.
        Assert.NotEqual(r.VarianceX, r.StdDevX * r.StdDevX, 6);
        Assert.Equal(SampleVarianceX, r.StdDevX * r.StdDevX, 12);
    }
}
