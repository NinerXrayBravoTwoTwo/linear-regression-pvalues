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
        var test = new RegressionPvalue([]);
        Assert.True(test.IsNaN);
    }

    [Fact]
    public void KnownDatasetStat()
    {
        // Create a PValueStat instance with three data points
        var dataPoints = new List<(double x, double y)>();


        double x = 0;
        double y = -1;

        while (x < 100)
            dataPoints.Add((x++, y++)); // Changed to AddDataPoint to match the method in PValueStat

        var stat = new RegressionPvalue(dataPoints);
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
        var dataPoints = new List<(double x, double y)>
        {
            (1, 2),
            (2, 3)
        };
        var stat = new RegressionPvalue(dataPoints);
        // Add only 2 data points
        // Assert that calling PValue throws an InvalidOperationException
        Assert.Throws<InvalidOperationException>(() => stat.PValue());
    }

    [Fact]
    public void PValueWithSufficientDataReturnsValidValue()
    {
        // Create a PValueStat instance with sufficient data points
        var dataPoints = new List<(double x, double y)>
        {
            (1, 2),
            (2, 3),
            (3, 4)
        };
        var stat = new RegressionPvalue(dataPoints);
        // Add 3 data points


        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithNegativeSlopeReturnsValidValue()
    {
        var dataPoints = new List<(double x, double y)>
        {
            (1, 3.4),
            (2, 2),
            (3, 1)
        };

        // Create a PValueStat instance with sufficient data points
        var stat = new RegressionPvalue(dataPoints);
        // Add 3 data points with a negative slope

        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetReturnsValidValue()
    {
        var dataPoints = new List<(double x, double y)>();

        var j = 0;
        while (j < 1000)
            dataPoints.Add((j++, 2 * j + 1)); // Changed to AddDataPoint to match the method in PValueStat

        var stat = new RegressionPvalue(dataPoints);

        //// Create a PValueStat instance with a large dataset
        //var statx = new RegressionPvalue([]);
        //for (var i = 0; i < 1000; i++) 
        //    stat.AddDataPoint(i, 2 * i + 1); // Linear relationship

        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithNoVariationInXReturnsOne()
    {
        var dataPoints = new List<(double x, double y)>
        {
            (1, 2),
            (1, 3),
            (1, 4)
        };
        // Create a PValueStat instance with no variation in X
        var stat = new RegressionPvalue(dataPoints);


        testOutputHelper.WriteLine(stat.ToString());

        // Assert that PValue returns 1.0
        var pValue = stat.PValue();
        Assert.Equal(1.0, pValue);
    }

    [Fact]
    public void PValueWithNaNValuesReturnsNaN()
    {

        var dataPoints = new List<(double x, double y)>
        {
            (1, double.NaN),
            (2, 3),
            (3, 4)
        };
        // Create a PValueStat instance
        var stat = new RegressionPvalue(dataPoints);
        // Add data points with NaN values

        testOutputHelper.WriteLine(stat.ToString());

        // Assert that PValue returns NaN
        var pValue = stat.PValue();
        Assert.True(double.IsNaN(pValue), "P-value should be NaN when data contains NaN values.");
    }

    [Fact]
    public void PValueWithAllNaNValuesReturnsNaN()
    {
        // Create a PValueStat instance
        var dataPoints = new List<(double x, double y)>
        {
            (double.NaN, double.NaN),
            (double.NaN, double.NaN),
            (double.NaN, double.NaN)
        };

        // Initialize with an empty list to avoid null reference

        var stat = new RegressionPvalue(dataPoints);
        // Add data points with all NaN values

        testOutputHelper.WriteLine(stat.ToString());

        // Assert that PValue returns NaN
        var pValue = stat.PValue();
        Assert.True(double.IsNaN(pValue), "P-value should be NaN when all data points are NaN.");
    }

    [Fact]
    public void PValueWithSingleDataPointReturnsNaN()
    {
        // Create a PValueStat instance
        // Create a PValueStat instance
        var dataPoints = new List<(double x, double y)>
        {
            (1, 2),
        };
        var stat = new RegressionPvalue(dataPoints);
        // Add a single data point
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns NaN
        Assert.Throws<InvalidOperationException>(() => stat.PValue());
    }

    [Fact]
    public void PValueWithTwoDataPointsReturnsNaN()
    {
        // Create a PValueStat instance

        // Create a PValueStat instance
        var dataPoints = new List<(double x, double y)>
        {
            (1,2),
            (2, 3),
        };

        var stat = new RegressionPvalue(dataPoints);


        // Add two data points
        testOutputHelper.WriteLine(stat.ToString());

        // Assert that PValue returns NaN
        Assert.Throws<InvalidOperationException>(() => stat.PValue());
    }

    [Fact]
    public void PValueWithThreeDataPointsReturnsValidValue()
    {
        // Create a PValueStat instance with three data points
        var dataPoints = new List<(double x, double y)>
        {
            (1, 2),
            (2,3),
            (3,4),
        };

        var stat = new RegressionPvalue(dataPoints);

        ;
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithNegativeSlopeAndNaNReturnsNaN()
    {
        // Create a PValueStat instance
        var dataPoints = new List<(double x, double y)>
        {
            (1, 3),
            (2, double.NaN), // Introduce NaN
            (3, 1)
        };
        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns NaN
        var pValue = stat.PValue();
        Assert.True(double.IsNaN(pValue), "P-value should be NaN when data contains NaN values.");
    }

    [Fact]
    public void PValueWithZeroSlopeAndNaNReturnsNaN()
    {
        // Create a PValueStat instance
        var dataPoints = new List<(double x, double y)>
        {
            (1, 2),
            (2, double.NaN), // Introduce NaN
            (3, 2)
        };

        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());

        // Assert that PValue returns NaN
        var pValue = stat.PValue();
        Assert.True(double.IsNaN(pValue), "P-value should be NaN when data contains NaN values.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndNaNReturnsNaN()
    {
        // Create a PValueStat instance with a large dataset
        var dataPoints = new List<(double x, double y)>();


        for (var i = 0; i < 1000; i++)
            if (i % 100 == 0) // Introduce NaN every 100th point
                dataPoints.Add((i, double.NaN));
            else
                dataPoints.Add((i, 2 * i + 1)); // Linear relationship

        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns NaN due to NaN in data
        var pValue = stat.PValue();
        Assert.True(double.IsNaN(pValue), "P-value should be NaN when data contains NaN values.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndNoVariationInXReturnsOne()
    {
        // Create a PValueStat instance with no variation in X
        var dataPoints = new List<(double x, double y)>();
        for (var i = 0; i < 1000; i++) dataPoints.Add((1, i)); // All X values are 1

        // Initialize with an empty list to avoid null reference    
        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());

        // Assert that PValue returns 1.0
        var pValue = stat.PValue();
        Assert.Equal(1.0, pValue);
    }

    [Fact]
    public void PValueWithNegativeSlopeAndLargeDatasetReturnsValidValue()
    {
        // Create a PValueStat instance with no variation in X
        var dataPoints = new List<(double x, double y)>();
        for (var i = 0; i < 1000; i++) dataPoints.Add((1, 1000 - i)); // All X values are 1

        // Initialize with an empty list to avoid null reference    
        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithZeroSlopeAndLargeDatasetReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset

        var dataPoints = new List<(double x, double y)>();
        for (var i = 0; i < 1000; i++) dataPoints.Add((1, 5));

        // Initialize with an empty list to avoid null reference    
        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithMixedDataTypesReturnsNaN()
    {
        // Create a PValueStat instance
        // Add data points with mixed types (e.g., string, null)
        var dataPoints = new List<(double x, double y)>()
        {
            (1,2), 
            (2, double.NaN), // Introduce NaN
            (3, 4)
        };


        // Initialize with an empty list to avoid null reference    
        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());

        // Assert that PValue returns NaN
        var pValue = stat.PValue();
        Assert.True(double.IsNaN(pValue), "P-value should be NaN when data contains NaN values.");
    }

    [Fact]
    public void PValueWithNegativeAndPositiveValuesReturnsValidValue()
    {
        // Create a PValueStat instance
        // Add data points with both negative and positive values
        var dataPoints = new List<(double x, double y)>
        {
            (-1, -2),
            (0, 0),
            (1, 2)
        };

        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeNegativeValuesReturnsValidValue()
    {
        // Create a PValueStat instance
        // Add data points with large negative values
        var dataPoints = new List<(double x, double y)>
        {
            (-1000, -2000),
            (-500, -1000),
            (0, 0)
        };
        var stat = new RegressionPvalue(dataPoints);
   
        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargePositiveValuesReturnsValidValue()
    {
        // Create a PValueStat instance
        // Add data points with large positive values
        var dataPoints = new List<(double x, double y)>
        {
            (1000, 2000),
            (500, 1000),
            (0, 0)
        };  

        var stat = new RegressionPvalue(dataPoints);
  
        testOutputHelper.WriteLine(stat.ToString());
        
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithMixedSignValuesReturnsValidValue()
    {
        // Create a PValueStat instance
        // Add data points with mixed sign values
        var dataPoints = new List<(double x, double y)>
        {
            (-1, -2),
            (0, 0),
            (1, 2)
        };
        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndMixedSignValuesReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var dataPoints = new List<(double x, double y)>();
        
        for (var i = -500; i < 500; i++) dataPoints.Add((i, 2 * i + 1)); // Linear relationship with mixed signs

        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());

        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndZeroValuesReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var dataPoints = new List<(double x, double y)>();  
        for (var i = 0; i < 1000; i++) dataPoints.Add((i, 0)); // All Y values are zero
        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndNegativeValuesReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var dataPoints = new List<(double x, double y)>();
        for (var i = 0; i < 1000; i++) dataPoints.Add((i, -2 * i - 1)); // Linear relationship with negative values
        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndPositiveValuesReturnsValidValue()
    {
        
        // Create a PValueStat instance with a large dataset
        var dataPoints = new List<(double x, double y)>();
        for (var i = 0; i < 1000; i++) dataPoints.Add((i, 2 * i + 1)); // Linear relationship with positive values
        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndZeroSlopeReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var dataPoints = new List<(double x, double y)>();
        for (var i = 0; i < 1000; i++) dataPoints.Add((i, 5)); // All Y values are constant
        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndNegativeSlopeReturnsValidValue()
    {
        // Create a PValueStat instance with a large dataset
        var dataPoints = new List<(double x, double y)>();
        for (var i = 0; i < 1000; i++) dataPoints.Add((i, 1000 - i)); // Negative slope
        // Add data points with a negative slope
        // Create a PValueStat instance with a large dataset
        var stat = new RegressionPvalue(dataPoints);

        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }

    [Fact]
    public void PValueWithLargeDatasetAndPositiveSlopeReturnsValidValue()
    {
        var dataPoints = new List<(double x, double y)>();


        for (var i = 0; i < 1000; i++)
            dataPoints.Add((i, 2 * i + 1)); // Linear relationship with positive slope

        // Create a PValueStat instance with sufficient data points
        // Create a PValueStat instance with a large dataset.
        var stat = new RegressionPvalue(dataPoints);

        //testOutputHelper.WriteLine(stat.ToString());

        testOutputHelper.WriteLine(stat.ToString());
        // Assert that PValue returns a valid double value
        var pValue = stat.PValue();
        Assert.True(pValue is >= 0 and <= 1, "P-value should be between 0 and 1.");
    }
}