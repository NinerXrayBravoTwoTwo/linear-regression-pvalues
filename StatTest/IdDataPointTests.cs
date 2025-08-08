using LinearRegression;
using Xunit.Abstractions;

namespace RegressionTest;

public class IdDataPointTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test_IdDataPoint_Constructor()
    {
        //Arrange

        var dataPoints = new List<(string id, double x, double y)> { ("a", 1, 2), ("b", 2, 4), ("c", 3, 6) };

        //Act
        var result = new RegressionPvalue(dataPoints);

        //Assert
        Assert.Equal("a", result.IdPoints.ElementAt(0));
        Assert.Equal(1, result.DataPoints.ElementAt(0).x);
        Assert.Equal(2, result.DataPoints.ElementAt(0).y);

        Assert.Equal("b", result.IdPoints.ElementAt(1));
        Assert.Equal(2, result.DataPoints.ElementAt(1).x);
        Assert.Equal(4, result.DataPoints.ElementAt(1).y);

        for (var index = 0; index < dataPoints.Count; index++)
            testOutputHelper.WriteLine($"id:{result.IdPoints.ElementAt(index)}, x:{result.DataPoints.ElementAt(index).x} y:{result.DataPoints.ElementAt(index).y}");
    }

    [Fact]
    public void AlternateConstructor()
    {
        var dataPoints = new List<(double x, double y)> { (1, 2), (2, 4), (3, 6) };
        var regressionPvalue = new RegressionPvalue(dataPoints);
        Assert.Equal(3, regressionPvalue.DataPointsCount());
    }
}

