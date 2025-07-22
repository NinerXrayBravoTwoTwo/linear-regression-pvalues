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
        
        testOutputHelper.WriteLine($"Confidence Interval: [{confidenceInterval.Lower}, {confidenceInterval.Upper}]");
    }

    [Fact]
    public void TestConfidenceInterval85Percent()
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
    public void TestConfidenceIntervalWithInfinityData()
    {
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

    [Fact]
    public void TestConfidenceIntervalWithNegativeData()
    {
        // Arrange
        var dataPoints = new List<(double x, double y)>
        {
        //    (-1.0, -2.0),
        //    (-2.0, -3.0),
        //    (-3.0, -5.0),
        //    (-4.0, -4.0),
        //    (-5.0, -6.0)
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

        testOutputHelper.WriteLine($"Confidence Interval: [{confidenceInterval.Lower}, {confidenceInterval.Upper}]");
    }

    [Fact]
    public void TestConfidenceIntervalWithZeroData()
    {
        // Arrange
        var dataPoints = new List<(double x, double y)>
        {
            (RandomGen.Next(RandomGen.Next(0, 10)), RandomGen.Next(RandomGen.Next(0, 10))),
            (2.0, 3.0),
            (3.0, 5.0),
            (4.0, 4.0),
            (5.0, 6.0)
        };

        var regression = new RegressionPvalue(dataPoints);

        // Act
        var result = regression.ConfidenceIntervalPlus(0.95);
        var CI = regression.ConfidenceInterval(0.95);

        // Assert

        Assert.Equal(CI.Lower, result.Lower);
        Assert.Equal(CI.Upper, result.Upper);

        testOutputHelper.WriteLine($"Confidence Interval: [{result.Lower}, {result.Upper} ]");

        testOutputHelper.WriteLine($"Confidence Extension: [{result.Lower}, {result.Upper}, {result.Slope}, " +
                                   $"{result.StandardError}, {result.PValue}]");
        testOutputHelper.WriteLine(regression.ToString());
    }
}

