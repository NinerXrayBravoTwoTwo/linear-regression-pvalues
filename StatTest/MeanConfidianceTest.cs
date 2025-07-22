using LinearRegression;
using Xunit.Abstractions;

namespace RegressionTest;

public class MeanConifidenceTest(ITestOutputHelper testOutputHelper)
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
        var meanConfidenceInterval = regression.MarginOfError(false, 0.95);
        
        // Assert
        testOutputHelper.WriteLine($"Mean and MarginOfError: [{meanConfidenceInterval.Mean}, {meanConfidenceInterval.MarginOfError:F4}]");
    }
}