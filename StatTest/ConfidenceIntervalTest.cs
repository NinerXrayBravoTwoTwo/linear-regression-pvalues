using LinearRegression;
using Xunit.Abstractions;

namespace RegressionTest;

public class ConfidenceIntervalTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestConfidenceInterval()
    {
        // Arrange
        var dataPoints = new List<(double x, double y)>
        {
            (1.0, 2.0),
            (2.0, 3.0),
            (3.0, 5.0),
            (4.0, 4.0),
            (5.0, 6.0)
        };
        
        var regression = new RegressionPvalue(dataPoints);
        // Act
        var confidenceInterval = regression.ConfidenceInterval(0.95);
        // Assert
        Assert.NotNull(confidenceInterval);
        
        testOutputHelper.WriteLine($"Confidence Interval: [{confidenceInterval.Lower}, {confidenceInterval.Upper}]");
    }

    [Fact]
    public void TestConfidenceInterval85percent()
    {
        // Arrange
        var dataPoints = new List<(double x, double y)>
        {
            (1.0, 2.0),
            (2.0, 3.0),
            (3.0, 5.0),
            (4.0, 4.0),
            (5.0, 6.0)
        };

        var regression = new RegressionPvalue(dataPoints);
        // Act
        var confidenceInterval = regression.ConfidenceInterval(0.85);
        // Assert
        Assert.NotNull(confidenceInterval);

        testOutputHelper.WriteLine($"Confidence Interval: [{confidenceInterval.Lower}, {confidenceInterval.Upper}]");
    }

    [Fact]
    public void TestConfidenceIntervalWithInsufficientData()
    {
        // Arrange
        var dataPoints = new List<(double x, double y)>
        {
            (1.0, 2.0)
        };
        
        var regression = new RegressionPvalue(dataPoints);
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => regression.ConfidenceInterval(0.95));
    }
    [Fact]
    public void TestConfidenceIntervalWithNaNData()
    {
        // Arrange
        var dataPoints = new List<(double x, double y)>
        {
            (1.0, 2.0),
            (2.0, double.NaN),
            (3.0, 5.0)
        };
        
        var regression = new RegressionPvalue(dataPoints);
        
        // Act
        var confidenceInterval = regression.ConfidenceInterval(0.95);
        
        // Assert
        Assert.Equal((double.NaN, double.NaN), confidenceInterval);
    }
    [Fact]
    public void TestConfidenceIntervalWithInfinityData() {
        // Arrange
        var dataPoints = new List<(double x, double y)>
        {
            (1.0, 2.0),
            (2.0, double.PositiveInfinity),
            (3.0, 5.0)
        };
        
        var regression = new RegressionPvalue(dataPoints);
        
        // Act
        var confidenceInterval = regression.ConfidenceInterval(0.95);
        
        // Assert
        Assert.Equal((double.NaN, double.NaN), confidenceInterval);
    }

}