using LinearRegression;
using Xunit.Abstractions;

namespace RegressionTest;

public class IdDataPointTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test_IdDataPoint_Constructor()
    {
        //Arrange
        var id = Guid.NewGuid();
        var dataPoints = new List<(string id, double x, double y)> { ("a", 1, 2), ("b", 2, 4), ("c", 3, 6) };

        //Act
        var result = new RegressionPvalue(dataPoints);

        //Assert
        Assert.Equal("a", result.IdPoints[0]);
        Assert.Equal(1, result.DataPoints[0].x);
        Assert.Equal(2, result.DataPoints[0].y);

        Assert.Equal("b", result.IdPoints[1]);
        Assert.Equal(2, result.DataPoints[1].x);
        Assert.Equal(4, result.DataPoints[1].y);

        for (var index = 0; index < dataPoints.Count; index++)
            testOutputHelper.WriteLine($"id:{result.IdPoints[index]}, x:{result.DataPoints[index].x} y:{result.DataPoints[index].y}");
    }
}

