using LinearRegression;

namespace RegressionTest;

/// <summary>
///     One focused regression test per bug fixed in PR 3 (min/max init, Merge(null), Dec(x),
///     defensive copy of the caller's list).
/// </summary>
public class SmallBugRegressionTests
{
    // Bug 3: Regression(IEnumerable) skipped this(), so MinX/MinY/MaxX/MaxY started at 0.
    [Fact]
    public void EnumerableConstructor_AllPositive_MinIsSmallestValue()
    {
        var r = new Regression([(5.0, 50.0), (7.0, 70.0), (6.0, 65.0)]);

        Assert.Equal(5, r.MinX);
        Assert.Equal(7, r.MaxX);
        Assert.Equal(50, r.MinY);
        Assert.Equal(70, r.MaxY);
    }

    [Fact]
    public void EnumerableConstructor_AllNegative_MaxIsLargestValue()
    {
        var r = new Regression([(-5.0, -50.0), (-7.0, -70.0), (-6.0, -65.0)]);

        Assert.Equal(-7, r.MinX);
        Assert.Equal(-5, r.MaxX);
        Assert.Equal(-70, r.MinY);
        Assert.Equal(-50, r.MaxY);
    }

    [Fact]
    public void RegressionPvalueConstructor_AnscombeI_MinMax()
    {
        // Anscombe's quartet I: x in [4, 14], y in [4.26, 10.84]
        var r = new RegressionPvalue(
        [
            (10, 8.04), (8, 6.95), (13, 7.58), (9, 8.81), (11, 8.33), (14, 9.96),
            (6, 7.24), (4, 4.26), (12, 10.84), (7, 4.82), (5, 5.68)
        ]);

        Assert.Equal(4, r.MinX);
        Assert.Equal(14, r.MaxX);
        Assert.Equal(4.26, r.MinY);
        Assert.Equal(10.84, r.MaxY);
    }

    // Bug 4: ArgumentException.ThrowIfNullOrEmpty(nameof(other)) checked the string "other", never the argument.
    [Fact]
    public void Merge_Null_ThrowsArgumentNullException()
    {
        var r = new Regression(1, 2);

        var ex = Assert.Throws<ArgumentNullException>(() => r.Merge(null!));
        Assert.Equal("other", ex.ParamName);
    }

    // Bug 5: Add(x) stores y = N (after increment) but Dec(x) removed y = N - 1.
    [Fact]
    public void DecWithXOnly_UndoesLastAddWithXOnly()
    {
        var r = new Regression();
        r.Add(5); // (5, 1)
        r.Add(6); // (6, 2)
        r.Add(7); // (7, 3)

        r.Dec(7); // must remove (7, 3)

        Assert.Equal(2, r.N);
        Assert.Equal(5 + 6, r.Sx);
        Assert.Equal(1 + 2, r.Sy);
        Assert.Equal(1 * 1 + 2 * 2, r.Sy2);
        Assert.Equal(5 * 1 + 6 * 2, r.Sxy);
    }

    [Fact]
    public void DecWithXOnly_BackToEmptyLeavesZeroSums()
    {
        var r = new Regression();
        r.Add(3);
        r.Add(4);

        r.Dec(4);
        r.Dec(3);

        Assert.Equal(0, r.N);
        Assert.Equal(0, r.Sx);
        Assert.Equal(0, r.Sy);
        Assert.Equal(0, r.Sy2);
        Assert.Equal(0, r.Sxy);
    }

    // Bug 6: DataPoints/IdPoints were a lazy Select over the caller's list, so later changes to
    // that list changed DataPoints but not the running sums.
    [Fact]
    public void RegressionPvalue_CallerListChangedAfterConstruction_DataIsUnchanged()
    {
        var points = new List<(double x, double y)> { (1, 2), (2, 3), (3, 5), (4, 4), (5, 6) };
        var r = new RegressionPvalue(points);

        points.Add((6, -100));
        points[0] = (1, 1000);

        Assert.Equal(5, r.DataPointsCount());
        Assert.Equal((1.0, 2.0), r.DataPoints.First());
        Assert.Equal(r.N, r.DataPointsCount());
        Assert.Equal(0.03738607346849862, r.PValue, 9); // SciPy linregress p for the 5-point data
    }

    [Fact]
    public void RegressionPvalue_IdListChangedAfterConstruction_IdsAreUnchanged()
    {
        var points = new List<(string id, double x, double y)> { ("a", 1, 2), ("b", 2, 3), ("c", 3, 5) };
        var r = new RegressionPvalue(points);

        points.Add(("d", 4, 4));
        points[0] = ("z", 9, 9);

        Assert.Equal(["a", "b", "c"], r.IdPoints);
        Assert.Equal((1.0, 2.0), r.DataPoints.First());
        Assert.Equal(3, r.DataPointsCount());
    }
}
