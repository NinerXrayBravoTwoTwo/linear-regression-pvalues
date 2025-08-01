using LinearRegression;
using Xunit.Abstractions;

// Ensure this namespace is correct and contains the 'Statistic' class.

namespace RegressionTest;

public class RegressionTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void StatTestAccessDataPoints()
    {
        var stat = new Regression();
        double x = 0;
        double y = -1;
        while (x < 100)
            stat.Add(x++, y++);

        testOutputHelper.WriteLine(stat.ToString());
        Assert.False(stat.IsNaN);
        Assert.Equal(0, stat.MinX);
        Assert.Equal(1, stat.Correlation);
        Assert.Equal(49.5, stat.MeanX);
        Assert.Equal(1, stat.Slope);

        // Assert.Equal(100, stat.DataPoints.Count);
    }

    [Fact]
    public void StatTestInitializeWithDataPoints()
    {

        // Create a PValueStat instance with insufficient data points
        var dataPoints = new List<(double x, double y)>
        {
            (100, 99),
            (101, 100)
        };
        var stat = new Regression(dataPoints);
        Assert.Equal(2, dataPoints.Count);
        double x = 0;
        double y = -1;
        while (x < 100)
            stat.Add(x++, y++);

        testOutputHelper.WriteLine(stat.ToString());

        //        Assert.Equal(102, stat.DataPointsCount());  
        Assert.Equal(102, stat.N);
        Assert.False(stat.IsNaN);
        Assert.Equal(0, stat.MinX);
        Assert.Equal(1, stat.Correlation);
        Assert.Equal(50.5, stat.MeanX);
        Assert.Equal(1, stat.Slope);

        //      Assert.Equal(100, stat.DataPoints.Count);
    }

    [Fact]
    public void KnownDatasetStat()
    {
        var stat = new Regression();

        double x = 0;
        double y = -1;
        while (x < 100)
            stat.Add(x++, y++);

        testOutputHelper.WriteLine(stat.ToString());

        Assert.False(stat.IsNaN);
        Assert.Equal(0, stat.MinX);
        Assert.Equal(1, stat.Correlation);
        Assert.Equal(49.5, stat.MeanX);
        Assert.Equal(1, stat.Slope);
    }

    [Fact]
    public void StatClone()
    {
        var original = new Regression();
        double x = 0, y = -1;

        while (x < 100)
            original.Add(x++, y++);

        testOutputHelper.WriteLine(original.ToString());

        Assert.False(original.IsNaN);

        var clone = new Regression(original);

        Assert.False(clone.IsNaN);

        Assert.Equal(original.ToString(), clone.ToString());

        // The clone should not update if the original changes, i.e. they are not the same instance
        original.Add(0, 0);
        testOutputHelper.WriteLine(clone.ToString());

        Assert.NotEqual(original.ToString(), clone.ToString());
        Assert.Equal(clone.N + 1, original.N);
    }

    [Fact]
    public void StatAddStat()
    {
        // set up
        var original = new Regression();

        double x = 0, y = -1;

        while (x < 500)
            original.Add(x++, y++);
        testOutputHelper.WriteLine(original.ToString());

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

        Assert.Equal(original.N * 2, clone.N);
        Assert.Equal(original.VarianceX, clone.VarianceX);
        Assert.Equal(original.VarianceY, clone.VarianceY);

        Assert.Equal(original.Sx * 2, clone.Sx);
        Assert.Equal(original.Sy * 2, clone.Sy);
        Assert.Equal(original.Sy2 * 2, clone.Sy2);
        Assert.Equal(original.Sxy * 2, clone.Sxy);

        Assert.Equal(original.MeanX, clone.MeanX);
        Assert.Equal(original.MeanY, clone.MeanY);

        Assert.False(clone.IsNaN);

        // Assert
        testOutputHelper.WriteLine(clone.ToString());
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

        testOutputHelper.WriteLine(original.ToString());
        testOutputHelper.WriteLine(clone.ToString());

        Assert.False(clone.IsNaN);

        // a> should only affect the clone.
        // b> number of samples should double
        // c> variance should remain the same 
        // d> other sum attributes should all be doubled.
        // e> Mean should remain the same.

        Assert.Equal(original.N * 2, clone.N);
        Assert.Equal(original.VarianceX, clone.VarianceX);
        Assert.Equal(original.VarianceY, clone.VarianceY);
        Assert.Equal(original.Sx * 2, clone.Sx);
        Assert.Equal(original.Sy * 2, clone.Sy);
        Assert.Equal(original.Sy2 * 2, clone.Sy2);
        Assert.Equal(original.Sxy * 2, clone.Sxy);
        Assert.Equal(original.MeanX, clone.MeanX);
        Assert.Equal(original.MeanY, clone.MeanY);
        Assert.False(clone.IsNaN);

        // Assert
        testOutputHelper.WriteLine(clone.ToString());
    }

    [Fact]
    public void StatAddStatWithNaN()
    {
        // set up
        var original = new Regression();
        double x = 0, y = -1;
        while (x < 500)
            original.Add(x++, y++);
        testOutputHelper.WriteLine(original.ToString());
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
        Assert.Equal(original.N * 2, clone.N);
        Assert.Equal(original.VarianceX, clone.VarianceX);
        Assert.Equal(original.VarianceY, clone.VarianceY);
        Assert.Equal(original.Sx * 2, clone.Sx);
        Assert.Equal(original.Sy * 2, clone.Sy);
        Assert.Equal(original.Sy2 * 2, clone.Sy2);
        Assert.Equal(original.Sxy * 2, clone.Sxy);
        Assert.Equal(original.MeanX, clone.MeanX);
        Assert.Equal(original.MeanY, clone.MeanY);
        Assert.False(clone.IsNaN);
        // Assert
        testOutputHelper.WriteLine(clone.ToString());
    }
}