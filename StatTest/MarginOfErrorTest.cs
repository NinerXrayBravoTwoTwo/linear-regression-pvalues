using LinearRegression;
using Xunit.Abstractions;

namespace RegressionTest;

public class MarginOfErrorTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void MeanConfidenceWithVerify()
    {
        // Arrange
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
        var moe = regression.MarginOfError();

        // Assert
        Assert.Equal(regression.MeanX(), moe.Mean);

        testOutputHelper.WriteLine($"Mean and MarginOfError: [{moe.Mean}, {moe.MarginOfError:F4}]");
    }

    [Fact]
    public void MeanConfidenceWithoutVerify()
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
        var moe = regression.MarginOfError(true);

        // Assert
        Assert.Equal(regression.MeanY(), moe.Mean);
        testOutputHelper.WriteLine($"Mean and MarginOfError: [{moe.Mean}, {moe.MarginOfError:F4}]");
    }
    [Fact]
    public void MeanConfidenceWithInsufficientData()
    {
        // Arrange
        var dataPoints = new List<(double x, double y)>
        {
            (1.0, 2.0)
        };
        var regression = new RegressionPvalue(dataPoints);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => regression.MarginOfError());
    }

    [Fact]
    public void MeanConfidenceWithNaNData()
    {
        // Arrange
        var dataPoints = new List<(double x, double y)>
        {
            (1.0, 2.0),
            (double.NaN, 3.0),
            (3.0, 5.0)
        };
        var regression = new RegressionPvalue(dataPoints);
        // Act
        var moe = regression.MarginOfError();
        // Assert
        Assert.Equal(double.NaN, moe.Mean);
        Assert.Equal(double.NaN, moe.MarginOfError);
    }
}