using LinearRegression;
using Xunit.Abstractions;

namespace RegressionTest;
// Ensure this namespace matches your project structure

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

        testOutputHelper.WriteLine(stat.PValue()
            .ToString($"P-value: {stat.PValue():F4}")); // Print P-Value with 4 decimal places
    }


    [Fact]
    public void PValueWithInsufficientDataThrowsException()
    {
        // Create a PValueStat instance with insufficient data points
        var stat = new PValueStat();
        // Add only 2 data points
        stat.AddDataPoint(1, 2);
        stat.AddDataPoint(2, 3);
        // Assert that calling PValue throws an InvalidOperationException
        Assert.Throws<InvalidOperationException>(() => stat.PValue());
    }

    [Fact]
    public void PValueWithSufficientDataReturnsValidValue()
    {
        // Create a PValueStat instance with sufficient data points
        var stat = new PValueStat();
        // Add 3 data points
        stat.AddDataPoint(1, 2);
        stat.AddDataPoint(2, 3);
        stat.AddDataPoint(3, 4);

        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithNegativeSlopeReturnsValidValue()
    {
        // Create a PValueStat instance with sufficient data points
        var stat = new PValueStat();
        // Add 3 data points with a negative slope
        stat.AddDataPoint(1, 3);
        stat.AddDataPoint(2, 2);
        stat.AddDataPoint(3, 1);

        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithZeroSlopeReturnsValidValue()
    {
        // Create a PValueStat instance with sufficient data points
        var stat = new PValueStat();
        // Add 3 data points with a zero slope
        var x = 3;
        while (x-- > 0) stat.AddDataPoint(x, 2);

        testOutputHelper.WriteLine(stat.ToString());

        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var stat = new PValueStat();
        for (var i = 0; i < 1000; i++) stat.AddDataPoint(i, 2 * i + 1); // Linear relationship

        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithNoVariationInXReturnsOne()
    {
        // Create a PValueStat instance with no variation in X
        var stat = new PValueStat();
        stat.AddDataPoint(1, 2);
        stat.AddDataPoint(1, 3);
        stat.AddDataPoint(1, 4);

        testOutputHelper.WriteLine(stat.ToString());

        // Assert that PValue returns 1.0
        var pValue = stat.PValue();
        Assert.Equal(1.0, pValue);
    }

    [Fact]
    public void PValueWithNaNValuesReturnsNaN()
    {
        // Create a PValueStat instance
        var stat = new PValueStat();
        // Add data points with NaN values
        stat.AddDataPoint(1, double.NaN);
        stat.AddDataPoint(2, 3);
        stat.AddDataPoint(3, 4);

        testOutputHelper.WriteLine(stat.ToString());

        // Assert that PValue returns NaN
        var pValue = stat.PValue();
        Assert.True(double.IsNaN(pValue), "P-value should be NaN when data contains NaN values.");
    }

    [Fact]
    public void PValueWithAllNaNValuesReturnsNaN()
    {
        // Create a PValueStat instance
        var stat = new PValueStat();
        // Add data points with all NaN values
        stat.AddDataPoint(double.NaN, double.NaN);
        stat.AddDataPoint(double.NaN, double.NaN);
        stat.AddDataPoint(double.NaN, double.NaN);
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns NaN
        var pValue = stat.PValue();
        Assert.True(double.IsNaN(pValue), "P-value should be NaN when all data points are NaN.");
    }

    [Fact]
    public void PValueWithSingleDataPointReturnsNaN()
    {
        // Create a PValueStat instance
        var stat = new PValueStat();
        // Add a single data point
        stat.AddDataPoint(1, 2);
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns NaN
        Assert.Throws<InvalidOperationException>(() => stat.PValue());
    }

    [Fact]
    public void PValueWithTwoDataPointsReturnsNaN()
    {
        // Create a PValueStat instance
        var stat = new PValueStat();
        // Add two data points
        stat.AddDataPoint(1, 2);
        stat.AddDataPoint(2, 3);
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns NaN
        Assert.Throws<InvalidOperationException>(() => stat.PValue());
    }

    [Fact]
    public void PValueWithThreeDataPointsReturnsValidValue()
    {
        // Create a PValueStat instance
        var stat = new PValueStat();
        // Add three data points
        stat.AddDataPoint(1, 2);
        stat.AddDataPoint(2, 3);
        stat.AddDataPoint(3, 4);
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithNegativeSlopeAndNaNReturnsNaN()
    {
        // Create a PValueStat instance
        var stat = new PValueStat();
        // Add data points with a negative slope and NaN
        stat.AddDataPoint(1, 3);
        stat.AddDataPoint(2, double.NaN);
        stat.AddDataPoint(3, 1);
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns NaN
        var pValue = stat.PValue();
        Assert.True(double.IsNaN(pValue), "P-value should be NaN when data contains NaN values.");
    }

    [Fact]
    public void PValueWithZeroSlopeAndNaNReturnsNaN()
    {
        // Create a PValueStat instance
        var stat = new PValueStat();
        // Add data points with a zero slope and NaN
        stat.AddDataPoint(1, 2);
        stat.AddDataPoint(2, double.NaN);
        stat.AddDataPoint(3, 2);
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns NaN
        var pValue = stat.PValue();
        Assert.True(double.IsNaN(pValue), "P-value should be NaN when data contains NaN values.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndNaNReturnsNaN()
    {
        // Create a PValueStat instance with a large dataset
        var stat = new PValueStat();
        for (var i = 0; i < 1000; i++)
            if (i % 100 == 0) // Introduce NaN every 100th point
                stat.AddDataPoint(i, double.NaN);
            else
                stat.AddDataPoint(i, 2 * i + 1); // Linear relationship
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns NaN due to NaN in data
        var pValue = stat.PValue();
        Assert.True(double.IsNaN(pValue), "P-value should be NaN when data contains NaN values.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndNoVariationInXReturnsOne()
    {
        // Create a PValueStat instance with no variation in X
        var stat = new PValueStat();
        for (var i = 0; i < 1000; i++) stat.AddDataPoint(1, i); // All X values are 1
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns 1.0
        var pValue = stat.PValue();
        Assert.Equal(1.0, pValue);
    }

    [Fact]
    public void PValueWithNegativeSlopeAndLargeDatasetReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var stat = new PValueStat();
        for (var i = 0; i < 1000; i++) stat.AddDataPoint(i, 1000 - i); // Negative slope
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithZeroSlopeAndLargeDatasetReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var stat = new PValueStat();
        for (var i = 0; i < 1000; i++) stat.AddDataPoint(i, 5); // All Y values are constant
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithMixedDataTypesReturnsNaN()
    {
        // Create a PValueStat instance
        var stat = new PValueStat();
        // Add data points with mixed types (e.g., string, null)
        stat.AddDataPoint(1, 2);
        stat.AddDataPoint(2, double.NaN); // NaN value
        stat.AddDataPoint(3, 4);
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns NaN
        var pValue = stat.PValue();
        Assert.True(double.IsNaN(pValue), "P-value should be NaN when data contains NaN values.");
    }

    [Fact]
    public void PValueWithNegativeAndPositiveValuesReturnsValidValue()
    {
        // Create a PValueStat instance
        var stat = new PValueStat();
        // Add data points with both negative and positive values
        stat.AddDataPoint(-1, -2);
        stat.AddDataPoint(0, 0);
        stat.AddDataPoint(1, 2);
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeNegativeValuesReturnsValidValue()
    {
        // Create a PValueStat instance
        var stat = new PValueStat();
        // Add data points with large negative values
        stat.AddDataPoint(-1000, -2000);
        stat.AddDataPoint(-500, -1000);
        stat.AddDataPoint(0, 0);
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargePositiveValuesReturnsValidValue()
    {
        // Create a PValueStat instance
        var stat = new PValueStat();
        // Add data points with large positive values
        stat.AddDataPoint(1000, 2000);
        stat.AddDataPoint(500, 1000);
        stat.AddDataPoint(0, 0);
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithMixedSignValuesReturnsValidValue()
    {
        // Create a PValueStat instance
        var stat = new PValueStat();
        // Add data points with mixed sign values
        stat.AddDataPoint(-1, -2);
        stat.AddDataPoint(0, 0);
        stat.AddDataPoint(1, 2);
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndMixedSignValuesReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var stat = new PValueStat();
        for (var i = -500; i < 500; i++) stat.AddDataPoint(i, 2 * i + 1); // Linear relationship with mixed signs
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndZeroValuesReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var stat = new PValueStat();
        for (var i = 0; i < 1000; i++) stat.AddDataPoint(i, 0); // All Y values are zero
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndNegativeValuesReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var stat = new PValueStat();
        for (var i = 0; i < 1000; i++) stat.AddDataPoint(i, -2 * i - 1); // Linear relationship with negative values
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndPositiveValuesReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var stat = new PValueStat();
        for (var i = 0; i < 1000; i++) stat.AddDataPoint(i, 2 * i + 1); // Linear relationship with positive values
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndZeroSlopeReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var stat = new PValueStat();
        for (var i = 0; i < 1000; i++) stat.AddDataPoint(i, 5); // All Y values are constant
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndNegativeSlopeReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var stat = new PValueStat();
        for (var i = 0; i < 1000; i++) stat.AddDataPoint(i, 1000 - i); // Negative slope
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndPositiveSlopeReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var stat = new PValueStat();
        for (var i = 0; i < 1000; i++) stat.AddDataPoint(i, 2 * i + 1); // Linear relationship with positive slope
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }
}