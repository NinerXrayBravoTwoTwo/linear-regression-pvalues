using MetabolicStat.StatMath;
using Xunit.Abstractions;
namespace StatTest;

public class PValueTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void EmptyPValueIsNaN() // Fixed spelling: PValue -> PValue, Nan -> NaN
    {
        // Create
        var test = new PValueStat();
        Assert.True(test.IsNaN);
    }

    [Fact]
    public void KnownDatasetStat()
    {
        // Fixed method name to match the class name
        var stat = new PValueStat();
        double x = 0;
        double y = -1;
        while (x < 100)
            stat.AddDataPoint(x++, y++); // Changed to AddDataPoint to match the method in PValueStat

        testOutputHelper.WriteLine(stat.ToString());

        Assert.False(stat.IsNaN);
        Assert.Equal(100, stat.DataPointsCount()); // Check if the number of data points is correct

        Assert.Equal(0, stat.MinX);
        Assert.Equal(1, stat.Correlation());
        Assert.Equal(49.5, stat.MeanX());
        Assert.Equal(1, stat.Slope());
    }
}