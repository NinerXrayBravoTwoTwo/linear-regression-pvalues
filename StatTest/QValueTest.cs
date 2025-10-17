using Xunit.Abstractions;

namespace RegressionTest;

public class QValueTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestQValueCalculation()
    {
        // Arrange
        var pValues = new[] { 0.01, 0.02, 0.05, 0.10, 0.20 };
        var expectedQValues = new[] { 0.05, 0.05, 0.0833, 0.125, 0.2 }; // Example expected values
        var qValueCalculator = new QValueCalculator(); // Assuming a QValueCalculator class exists

        // Act
        var actualQValues = qValueCalculator.CalculateQValues(pValues);

        // Assert
        Assert.Equal(expectedQValues.Length, actualQValues.Length);
        for (var i = 0; i < expectedQValues.Length; i++)
            Assert.True(Math.Abs(expectedQValues[i] - actualQValues[i]) < 0.001,
                $"Q-value at index {i} is incorrect. Expected: {expectedQValues[i]}, Actual: {actualQValues[i]}");
    }
}

