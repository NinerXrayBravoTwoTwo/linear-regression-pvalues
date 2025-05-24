using MetabolicStat.StatMath;
using Xunit.Abstractions;

namespace StatTest;

public class StatisticTest(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public void EmptyStatIsNan()
    {
        // create statistic
        var test = new Statistic();

        Assert.True(test.IsNaN);

        _testOutputHelper.WriteLine(test.ToString());
    }

    [Fact]
    public void KnownDatasetStat()
    {
        var stat = new Statistic();

        double x = 0;
        double y = -1;
        while (x < 100)
            stat.Add(x++, y++);

        _testOutputHelper.WriteLine(stat.ToString());

        Assert.False(stat.IsNaN);
        Assert.Equal(0, stat.MinX);
        Assert.Equal(1, stat.Correlation());
        Assert.Equal(49.5, stat.MeanX());
        Assert.Equal(1, stat.Slope());
    }

    [Fact]
    public void StatClone()
    {
        var original = new Statistic();
        double x = 0, y = -1;

        while (x < 100)
            original.Add(x++, y++);

        _testOutputHelper.WriteLine(original.ToString());

        Assert.False(original.IsNaN);

        var clone = new Statistic(original);

        Assert.False(clone.IsNaN);

        Assert.Equal(original.ToString(), clone.ToString());

        // The clone should not update if the original changes, i.e. they are not the same instance
        original.Add(0, 0);
        _testOutputHelper.WriteLine(clone.ToString());

        Assert.NotEqual(original.ToString(), clone.ToString());
        Assert.Equal(clone.NumberSamples + 1, original.NumberSamples);
    }

    [Fact]
    public void StatAddStat()
    {
        // set up
        var original = new Statistic();

        double x = 0, y = -1;

        while (x < 500)
            original.Add(x++, y++);
        _testOutputHelper.WriteLine(original.ToString());

        var clone = new Statistic(original); // Make a clone (new unrelated instance) of the statistic.

        Assert.False(original.IsNaN);
        Assert.False(clone.IsNaN);

        // test - 
        clone.Add(original); // Adding the original statistic to its clone, 
        // a> should only effect the clone.
        // b> number of samples should double
        // c> variance should remain the same 
        // d> other sum attributes should all be doubled.
        // e> Mean should remain the same.

        Assert.Equal(original.NumberSamples * 2, clone.NumberSamples);
        Assert.Equal(original.Qx2(), clone.Qx2());
        Assert.Equal(original.Qy2(), clone.Qy2());

        Assert.Equal(original.Sx * 2, clone.Sx);
        Assert.Equal(original.Sy * 2, clone.Sy);
        Assert.Equal(original.Sy2 * 2, clone.Sy2);
        Assert.Equal(original.Sxy * 2, clone.Sxy);

        Assert.Equal(original.MeanX(), clone.MeanX());
        Assert.Equal(original.MeanY(), clone.MeanY());

        Assert.False(clone.IsNaN);

        // Assert
        _testOutputHelper.WriteLine(clone.ToString());
    }

    [Fact]
    public void MergeStat()
    {
        // set up
        var original = new Statistic();
        double x = 0;
        while (x++ < 500)
            original.Add(RandomGen.NextDouble() * 100, RandomGen.NextDouble() * 100);

        Assert.False(original.IsNaN);

        // test - 
        var clone = original.Merge(original); // Merging the original statistic to its clone, 

        _testOutputHelper.WriteLine(original.ToString());
        _testOutputHelper.WriteLine(clone.ToString());

        Assert.False(clone.IsNaN);

        // a> should only effect the clone.
        // b> number of samples should double
        // c> variance should remain the same 
        // d> other sum attributes should all be doubled.
        // e> Mean should remain the same.

        Assert.Equal(original.NumberSamples * 2, clone.NumberSamples);
        Assert.Equal(original.Qx2(), clone.Qx2());
        Assert.Equal(original.Qy2(), clone.Qy2());
        Assert.Equal(original.Sx * 2, clone.Sx);
        Assert.Equal(original.Sy * 2, clone.Sy);
        Assert.Equal(original.Sy2 * 2, clone.Sy2);
        Assert.Equal(original.Sxy * 2, clone.Sxy);
        Assert.Equal(original.MeanX(), clone.MeanX());
        Assert.Equal(original.MeanY(), clone.MeanY());
        Assert.False(clone.IsNaN);

        Assert.NotEqual(original.Correlation(), clone.Correlation());  // Correlation should see a precision  

        // Assert
        _testOutputHelper.WriteLine(clone.ToString());
    }
}