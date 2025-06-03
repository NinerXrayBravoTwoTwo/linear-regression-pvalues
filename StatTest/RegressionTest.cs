using LinearRegression;
using Xunit.Abstractions;

// Ensure this namespace is correct and contains the 'Statistic' class.

namespace RegressionTest;

public class RegressionTest(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public void KnownDatasetStat()
    {
        var stat = new Regression();

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
        var original = new Regression();
        double x = 0, y = -1;

        while (x < 100)
            original.Add(x++, y++);

        _testOutputHelper.WriteLine(original.ToString());

        Assert.False(original.IsNaN);

        var clone = new Regression(original);

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
        var original = new Regression();

        double x = 0, y = -1;

        while (x < 500)
            original.Add(x++, y++);
        _testOutputHelper.WriteLine(original.ToString());

        var clone = new Regression(original); // Make a clone (new unrelated instance) of the statistic.

        Assert.False(original.IsNaN);
        Assert.False(clone.IsNaN);

        // test - 
        clone.Add(original); // Adding the original statistic to its clone, 
        // a> should only affect the clone.
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
        var original = new Regression();
        double x = 0;
        while (x++ < 500)
            original.Add(RandomGen.NextDouble() * 100, RandomGen.NextDouble() * 100);

        Assert.False(original.IsNaN);

        // test - 
        var clone = original.Merge(original); // Merging the original statistic to its clone, 

        _testOutputHelper.WriteLine(original.ToString());
        _testOutputHelper.WriteLine(clone.ToString());

        Assert.False(clone.IsNaN);

        // a> should only affect the clone.
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
    public void StatAddStatWithNaN()
    {
        // set up
        var original = new Regression();
        double x = 0, y = -1;
        while (x < 500)
            original.Add(x++, y++);
        _testOutputHelper.WriteLine(original.ToString());
        var clone = new Regression(original); // Make a clone (new unrelated instance) of the statistic.
        Assert.False(original.IsNaN);
        Assert.False(clone.IsNaN);
        // test - 
        clone.Add(original); // Adding the original statistic to its clone, 
        // a> should only affect the clone.
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
    public void StatAddStatWithNaNAndEmptyStat()
    {
        // set up
        var original = new Regression();
        double x = 0, y = -1;
        while (x < 500)
            original.Add(x++, y++);
        _testOutputHelper.WriteLine(original.ToString());
        var clone = new Regression(original); // Make a clone (new unrelated instance) of the statistic.
        Assert.False(original.IsNaN);
        Assert.False(clone.IsNaN);
        // test - 
        clone.Add(new Regression()); // Adding an empty statistic to its clone, 
        // a> should not affect the clone.
        // b> number of samples should remain the same
        // c> variance should remain the same 
        // d> other sum attributes should remain the same.
        // e> Mean should remain the same.
        Assert.Equal(original.NumberSamples, clone.NumberSamples);
        Assert.Equal(original.Qx2(), clone.Qx2());
        Assert.Equal(original.Qy2(), clone.Qy2());
        Assert.Equal(original.Sx, clone.Sx);
        Assert.Equal(original.Sy, clone.Sy);
        Assert.Equal(original.Sy2, clone.Sy2);
        Assert.Equal(original.Sxy, clone.Sxy);
        Assert.Equal(original.MeanX(), clone.MeanX());
        Assert.Equal(original.MeanY(), clone.MeanY());
        Assert.False(clone.IsNaN);
        // Assert
        _testOutputHelper.WriteLine(clone.ToString());
    }

    [Fact]
    public void StatAddStatWithNaNAndEmptyStatWithNaN()
    {
        // set up
        var original = new Regression();
        double x = 0, y = -1;
        while (x < 500)
            original.Add(x++, y++);
        _testOutputHelper.WriteLine(original.ToString());
        var clone = new Regression(original); // Make a clone (new unrelated instance) of the statistic.
        Assert.False(original.IsNaN);
        Assert.False(clone.IsNaN);
        // test - 
        clone.Add(new Regression()); // Adding an empty statistic to its clone, 
        // a> should not affect the clone.
        // b> number of samples should remain the same
        // c> variance should remain the same 
        // d> other sum attributes should remain the same.
        // e> Mean should remain the same.
        Assert.Equal(original.NumberSamples, clone.NumberSamples);
        Assert.Equal(original.Qx2(), clone.Qx2());
        Assert.Equal(original.Qy2(), clone.Qy2());
        Assert.Equal(original.Sx, clone.Sx);
        Assert.Equal(original.Sy, clone.Sy);
        Assert.Equal(original.Sy2, clone.Sy2);
        Assert.Equal(original.Sxy, clone.Sxy);
        Assert.Equal(original.MeanX(), clone.MeanX());
        Assert.Equal(original.MeanY(), clone.MeanY());
        Assert.False(clone.IsNaN);
        // Assert
        _testOutputHelper.WriteLine(clone.ToString());
    }

    [Fact]
    public void StatAddStatWithNaNAndEmptyStatWithNaNAndNaNDataPoint()
    {
        // set up
        var original = new Regression();
        double x = 0, y = -1;
        while (x < 500)
            original.Add(x++, y++);
        _testOutputHelper.WriteLine(original.ToString());
        var clone = new Regression(original); // Make a clone (new unrelated instance) of the statistic.
        Assert.False(original.IsNaN);
        Assert.False(clone.IsNaN);
        // test - 
        clone.Add(new Regression()); // Adding an empty statistic to its clone, 
        // a> should not affect the clone.
        // b> number of samples should remain the same
        // c> variance should remain the same 
        // d> other sum attributes should remain the same.
        // e> Mean should remain the same.
        Assert.Equal(original.NumberSamples, clone.NumberSamples);
        Assert.Equal(original.Qx2(), clone.Qx2());
        Assert.Equal(original.Qy2(), clone.Qy2());
        Assert.Equal(original.Sx, clone.Sx);
        Assert.Equal(original.Sy, clone.Sy);
        Assert.Equal(original.Sy2, clone.Sy2);
        Assert.Equal(original.Sxy, clone.Sxy);
        Assert.Equal(original.MeanX(), clone.MeanX());
        Assert.Equal(original.MeanY(), clone.MeanY());
        Assert.False(clone.IsNaN);

        // Add a NaN data point to the clone
        clone.Add(double.NaN, double.NaN);
        _testOutputHelper.WriteLine(clone.ToString());
        // The NaN data point should not affect the statistics
        Assert.True(clone.IsNaN); // The presence of NaN should make the statistic NaN
    }

    [Fact]
    public void StatAddStatWithNaNAndEmptyStatWithNaNAndNaNDataPointAndNaNDataPoint()
    {
        // set up
        var original = new Regression();
        double x = 0, y = -1;
        while (x < 500)
            original.Add(x++, y++);
        _testOutputHelper.WriteLine(original.ToString());
        var clone = new Regression(original); // Make a clone (new unrelated instance) of the statistic.
        Assert.False(original.IsNaN);
        Assert.False(clone.IsNaN);
        // test - 
        clone.Add(new Regression()); // Adding an empty statistic to its clone, 
        // a> should not affect the clone.
        // b> number of samples should remain the same
        // c> variance should remain the same 
        // d> other sum attributes should remain the same.
        // e> Mean should remain the same.
        Assert.Equal(original.NumberSamples, clone.NumberSamples);
        Assert.Equal(original.Qx2(), clone.Qx2());
        Assert.Equal(original.Qy2(), clone.Qy2());
        Assert.Equal(original.Sx, clone.Sx);
        Assert.Equal(original.Sy, clone.Sy);
        Assert.Equal(original.Sy2, clone.Sy2);
        Assert.Equal(original.Sxy, clone.Sxy);
        Assert.Equal(original.MeanX(), clone.MeanX());
        Assert.Equal(original.MeanY(), clone.MeanY());
        Assert.False(clone.IsNaN);
        // Add a NaN data point to the clone
        clone.Add(double.NaN, double.NaN);

        _testOutputHelper.WriteLine(clone.ToString());

        // The NaN data point should not affect the statistics
        Assert.True(clone.IsNaN); // The presence of NaN should make the statistic NaN
    }

    [Fact]
    public void StatAddStatWithNaNAndEmptyStatWithNaNAndNaNDataPointAndNaNDataPointAndNaNDataPoint()
    {
        // set up
        var original = new Regression();
        double x = 0, y = -1;
        while (x < 500)
            original.Add(x++, y++);
        _testOutputHelper.WriteLine(original.ToString());
        var clone = new Regression(original); // Make a clone (new unrelated instance) of the statistic.
        Assert.False(original.IsNaN);
        Assert.False(clone.IsNaN);
        // test - 
        clone.Add(new Regression()); // Adding an empty statistic to its clone, 
        // a> should not affect the clone.
        // b> number of samples should remain the same
        // c> variance should remain the same 
        // d> other sum attributes should remain the same.
        // e> Mean should remain the same.
        Assert.Equal(original.NumberSamples, clone.NumberSamples);
        Assert.Equal(original.Qx2(), clone.Qx2());
        Assert.Equal(original.Qy2(), clone.Qy2());
        Assert.Equal(original.Sx, clone.Sx);
        Assert.Equal(original.Sy, clone.Sy);
        Assert.Equal(original.Sy2, clone.Sy2);
        Assert.Equal(original.Sxy, clone.Sxy);
        Assert.Equal(original.MeanX(), clone.MeanX());
        Assert.Equal(original.MeanY(), clone.MeanY());
        Assert.Equal(original.RSquared(), clone.RSquared());
        Assert.False(clone.IsNaN);

        // Add a NaN data point to the clone
        clone.Add(double.NaN, double.NaN);

        _testOutputHelper.WriteLine(clone.ToString());

        // The NaN data point should not affect the statistics
        Assert.True(clone.IsNaN); // The presence of NaN should make the statistic NaN
    }

    [Fact]
    public void StatAddStatWithNaNAndEmptyStatWithNaNAndNaNDataPointAndNaNDataPointAndNaNDataPointAndNaNDataPoint()
    {
        // set up
        var original = new Regression();
        double x = 0, y = -1;
        while (x < 500)
            original.Add(x++, y++);
        _testOutputHelper.WriteLine(original.ToString());
        var clone = new Regression(original); // Make a clone (new unrelated instance) of the statistic.
        Assert.False(original.IsNaN);
        Assert.False(clone.IsNaN);
        // test - 
        clone.Add(new Regression()); // Adding an empty statistic to its clone, 
        // a> should not affect the clone.
        // b> number of samples should remain the same
        // c> variance should remain the same 
        // d> other sum attributes should remain the same.
        // e> Mean should remain the same.
        Assert.Equal(original.NumberSamples, clone.NumberSamples);
        Assert.Equal(original.Qx2(), clone.Qx2());
        Assert.Equal(original.Qy2(), clone.Qy2());
        Assert.Equal(original.Sx, clone.Sx);
        Assert.Equal(original.Sy, clone.Sy);
        Assert.Equal(original.Sy2, clone.Sy2);
        Assert.Equal(original.Sxy, clone.Sxy);
        Assert.Equal(original.MeanX(), clone.MeanX());
        Assert.Equal(original.MeanY(), clone.MeanY());
        Assert.Equal(original.RSquared(), clone.RSquared());
        Assert.False(clone.IsNaN);
        // Add a NaN data point to the clone
        clone.Add(double.NaN, double.NaN);
        _testOutputHelper.WriteLine(clone.ToString());
        // The NaN data point should not affect the statistics
        Assert.True(clone.IsNaN); // The presence of NaN should make the statistic NaN
    }
}