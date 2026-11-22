using LinearRegression;

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
        var confidenceInterval = regression.ConfidenceInterval();
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
        Assert.Throws<InvalidOperationException>(() => regression.ConfidenceInterval());
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
        var confidenceInterval = regression.ConfidenceInterval();

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
        var confidenceInterval = regression.ConfidenceInterval();

        // Assert
        Assert.Equal((double.NaN, double.NaN), confidenceInterval);
    }

    [Fact]
    public void TestConfidenceIntervalWithNegativeData()
    {
        // Arrange
        var dataPoints = new List<(double x, double y)>
        {
            (-1.0, -2.0),
            (-2.0, -3.0),
            (-3.0, -5.0),
            (-4.0, -4.0),
            (-5.0, -6.0)
        };

        var regression = new RegressionPvalue(dataPoints);

        // Act
        var confidenceInterval = regression.ConfidenceInterval();

        // Assert: negating both x and y keeps slope 0.9, so the 95% CI matches the positive
        // data set (SciPy linregress + t(0.975, 3): [0.09910190986563006, 1.70089809013437]).
        Assert.Equal(0.9, regression.Slope, 12);
        Assert.Equal(0.09910190986563006, confidenceInterval.Lower, 9);
        Assert.Equal(1.70089809013437, confidenceInterval.Upper, 9);

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
        var result = regression.ConfidenceIntervalPlus();
        var ci = regression.ConfidenceInterval();

        // Assert

        Assert.Equal(ci.Lower, result.Lower);
        Assert.Equal(ci.Upper, result.Upper);

        testOutputHelper.WriteLine($"Confidence Interval: [{result.Lower}, {result.Upper} ]");

        testOutputHelper.WriteLine($"Confidence Extension: [{result.Lower}, {result.Upper}, {result.Slope}, " +
                                   $"{result.StandardError}, {result.PValue}]");
        testOutputHelper.WriteLine(regression.ToString());
    }
}

