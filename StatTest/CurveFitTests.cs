using LinearRegression;

namespace RegressionTest;

/// <summary>
///     Curve fits (<see cref="CurveFit" />, <see cref="CurveTransform" />), issue #20, after TI ST-12.
///     References: statsmodels 0.15.0 OLS on the transformed points, and the census example of TI
///     <i>Applied Statistics</i> (1977) p. 5-11.
/// </summary>
public class CurveFitTests
{
    // TI Applied Statistics p. 5-11: US census 1890-1970.
    private static readonly List<(double x, double y)> Census =
    [
        (1890, 62947714), (1900, 75994575), (1910, 91972266), (1920, 105710620), (1930, 122775046),
        (1940, 131669275), (1950, 150697361), (1960, 179323175), (1970, 203235298)
    ];

    private static readonly List<(double x, double y)> Small =
        [(1, 2.1), (2, 3.9), (3, 6.2), (4, 7.8), (5, 10.5), (6, 11.9)];

    private const double RelTol = 1e-9;

    private static void AssertRelative(double expected, double actual, double relTol = RelTol)
    {
        Assert.False(double.IsNaN(actual), $"unexpected NaN (expected {expected:R})");
        Assert.True(Math.Abs(actual - expected) <= relTol * Math.Max(Math.Abs(expected), 1e-18),
            $"expected {expected:R}, got {actual:R}");
    }

    private static void AssertInterval((double Fit, double Lower, double Upper) actual,
        double fit, double lower, double upper)
    {
        AssertRelative(fit, actual.Fit);
        AssertRelative(lower, actual.Lower);
        AssertRelative(upper, actual.Upper);
    }

    private static void AssertAllNaN((double Fit, double Lower, double Upper) r) =>
        Assert.True(double.IsNaN(r.Fit) && double.IsNaN(r.Lower) && double.IsNaN(r.Upper),
            $"expected (NaN, NaN, NaN), got ({r.Fit}, {r.Lower}, {r.Upper})");

    // ---------- census, exponential ----------

    [Fact]
    public void Census_Exponential_MatchesManualAndPython()
    {
        var r = new RegressionPvalue(Census, CurveFit.Exponential);
        Assert.Equal(CurveFit.Exponential, r.Curve);
        Assert.Equal(9, r.N);
        AssertRelative(0.01411830638821616, r.Slope);
        AssertRelative(0.01411830638821616, r.CurveM);
        AssertRelative(-8.670083711272705, r.YIntercept);
        AssertRelative(0.0001716447350238849, r.CurveB);
        AssertRelative(0.995723868760261, r.Correlation);
        AssertRelative(0.9914660228189014, r.RSquared);
        AssertRelative(314510828.7531725, r.PredictY(2000));
        AssertRelative(1996.6542706157325, r.PredictX(3e8));
        AssertRelative(1.6765199927928083e-08, r.PValue, 1e-7);
    }

    [Fact]
    public void Census_Exponential_StoresTransformedPoints()
    {
        var r = new RegressionPvalue(Census, CurveFit.Exponential);
        Assert.Equal(Census.Count, r.DataPointsCount());
        for (var i = 0; i < Census.Count; i++)
        {
            Assert.Equal(Census[i].x, r.DataPoints.ElementAt(i).x);
            Assert.Equal(Math.Log(Census[i].y), r.DataPoints.ElementAt(i).y);
        }
    }

    [Fact]
    public void Census_Exponential_Intervals_BackTransformed()
    {
        var r = new RegressionPvalue(Census, CurveFit.Exponential);
        // statsmodels summary_frame at x = 2000 on (x, ln y), then exp of mean / mean_ci_* / obs_ci_*
        AssertInterval(r.MeanResponseInterval(2000), 19.56652906515914, 19.47918565363513, 19.65387247668315);
        AssertInterval(r.CurveMeanResponseInterval(2000),
            314510828.7530216, 288205882.8288267, 343216663.1437638);
        AssertInterval(r.CurvePredictionInterval(2000),
            314510828.7530216, 277304267.1565964, 356709481.67217726);
        AssertRelative(r.PredictY(2000), r.CurveMeanResponseInterval(2000).Fit);
    }

    [Fact]
    public void Regression_Census_TableCtor_MatchesPvalueClass()
    {
        var a = new Regression(Census, CurveFit.Exponential);
        var b = new RegressionPvalue(Census, CurveFit.Exponential);
        Assert.Equal(b.Slope, a.Slope);
        Assert.Equal(b.CurveB, a.CurveB);
        Assert.Equal(b.PredictY(2000), a.PredictY(2000));
    }

    // ---------- power, logarithmic, exponential on a small set ----------

    [Fact]
    public void Small_Power_MatchesStatsmodels()
    {
        var r = new RegressionPvalue(Small, CurveFit.Power);
        AssertRelative(0.9857211901278424, r.CurveM);
        AssertRelative(0.7206988743399818, r.YIntercept);
        AssertRelative(2.055869503133536, r.CurveB);
        AssertRelative(0.9986219301936357, r.Correlation);
        AssertRelative(2.847306056902943e-06, r.PValue, 1e-7);
        AssertRelative(13.996730160178577, r.PredictY(7));
        AssertRelative(4.472350610804163, r.PredictX(9));
        AssertInterval(r.CurveMeanResponseInterval(7), 13.996730160178572, 12.98552217755415, 15.086682884072685);
        AssertInterval(r.CurvePredictionInterval(7), 13.996730160178572, 12.28680666077009, 15.944619345428332);
    }

    [Fact]
    public void Small_Logarithmic_MatchesStatsmodels()
    {
        var r = new RegressionPvalue(Small, CurveFit.Logarithmic);
        AssertRelative(5.491015975207098, r.CurveM);
        AssertRelative(1.0455377483253177, r.CurveB);
        Assert.Equal(r.YIntercept, r.CurveB);
        AssertRelative(0.9646750974417405, r.Correlation);
        AssertRelative(0.0018497330435385735, r.PValue, 1e-7);
        AssertRelative(11.730561463105667, r.PredictY(7));
        AssertRelative(4.2572871800409935, r.PredictX(9));
        AssertInterval(r.CurveMeanResponseInterval(7), 11.730561463105667, 9.559863981014793, 13.901258945196542);
        AssertInterval(r.CurvePredictionInterval(7), 11.730561463105667, 7.958834306026719, 15.502288620184615);
    }

    [Fact]
    public void Small_Exponential_MatchesStatsmodels()
    {
        var r = new RegressionPvalue(Small, CurveFit.Exponential);
        AssertRelative(0.3392507380191048, r.CurveM);
        AssertRelative(1.8481882724875682, r.CurveB);
        AssertRelative(0.9703354348685842, r.Correlation);
        AssertRelative(0.0013069274293528034, r.PValue, 1e-7);
        AssertRelative(19.865032259774278, r.PredictY(7));
        AssertRelative(4.666220446557094, r.PredictX(9));
        AssertInterval(r.CurveMeanResponseInterval(7), 19.865032259774278, 12.578466153789623, 31.372625394629875);
        AssertInterval(r.CurvePredictionInterval(7), 19.865032259774278, 10.158446325273873, 38.84644305301615);
    }

    // ---------- streaming Add / Dec ----------

    [Theory]
    [InlineData(CurveFit.Linear)]
    [InlineData(CurveFit.Exponential)]
    [InlineData(CurveFit.Power)]
    [InlineData(CurveFit.Logarithmic)]
    public void Streaming_AddEqualsTableCtor(CurveFit curve)
    {
        var table = new Regression(Small, curve);
        var stream = new Regression(curve);
        foreach (var (x, y) in Small) stream.Add(x, y);
        Assert.Equal(curve, stream.Curve);
        Assert.Equal(table.N, stream.N);
        Assert.Equal(table.Slope, stream.Slope);
        Assert.Equal(table.YIntercept, stream.YIntercept);
        Assert.Equal(table.Correlation, stream.Correlation);
        Assert.Equal(table.PredictY(7), stream.PredictY(7));
    }

    [Theory]
    [InlineData(CurveFit.Exponential)]
    [InlineData(CurveFit.Power)]
    [InlineData(CurveFit.Logarithmic)]
    public void Streaming_DecRawPoint_RestoresFit(CurveFit curve)
    {
        var without = new Regression(Small.Take(5), curve);
        var r = new Regression(Small, curve);
        r.Dec(Small[5].x, Small[5].y); // raw pair again, like INV 2nd Σ+
        Assert.Equal(without.N, r.N);
        AssertRelative(without.Slope, r.Slope, 1e-12);
        AssertRelative(without.YIntercept, r.YIntercept, 1e-12);
        AssertRelative(without.CurveB, r.CurveB, 1e-12);
        AssertRelative(without.PredictY(7), r.PredictY(7), 1e-12);
    }

    [Fact]
    public void RegressionPvalue_EmptyCurveCtor_StreamsSumsButNoTests()
    {
        var r = new RegressionPvalue(CurveFit.Exponential);
        foreach (var (x, y) in Census) r.Add(x, y);
        AssertRelative(0.0001716447350238849, r.CurveB);
        AssertRelative(314510828.7531725, r.PredictY(2000));
        Assert.Equal(0, r.DataPointsCount()); // documented limitation: Add does not store points
        Assert.True(double.IsNaN(r.PValue));
    }

    [Fact]
    public void Clone_And_Merge_KeepTransform()
    {
        var a = new Regression(Census.Take(4), CurveFit.Exponential);
        var b = new Regression(Census.Skip(4), CurveFit.Exponential);
        var merged = a.Merge(b);
        Assert.Equal(CurveFit.Exponential, merged.Curve);
        AssertRelative(314510828.7531725, merged.PredictY(2000));
        var clone = new Regression(merged);
        Assert.Equal(CurveFit.Exponential, clone.Curve);
        clone.Add(1980, 226545805);
        Assert.Equal(Math.Log(226545805), clone.MaxY);
    }

    // ---------- custom transforms ----------

    [Fact]
    public void Custom_EqualsBuiltInExponential()
    {
        var custom = new CurveTransform(null, Math.Log, inverseY: Math.Exp);
        var c = new RegressionPvalue(Census, custom);
        var e = new RegressionPvalue(Census, CurveFit.Exponential);
        Assert.Equal(CurveFit.Custom, c.Curve);
        Assert.Equal(e.Slope, c.Slope);
        Assert.Equal(e.YIntercept, c.YIntercept);
        Assert.Equal(e.PValue, c.PValue);
        Assert.Equal(e.PredictY(2000), c.PredictY(2000));
        Assert.Equal(e.PredictX(3e8), c.PredictX(3e8));
        Assert.Equal(e.CurveMeanResponseInterval(2000), c.CurveMeanResponseInterval(2000));
        // CurveB is the linearized intercept for a custom transform (the library does not know its form).
        Assert.Equal(e.YIntercept, c.CurveB);
    }

    [Fact]
    public void Custom_EqualsBuiltInPower()
    {
        var custom = new CurveTransform(Math.Log, Math.Log, Math.Exp, Math.Exp);
        var c = new Regression(Small, custom);
        var p = new Regression(Small, CurveFit.Power);
        Assert.Equal(p.Slope, c.Slope);
        Assert.Equal(p.PredictY(7), c.PredictY(7));
        Assert.Equal(p.PredictX(9), c.PredictX(9));
    }

    [Fact]
    public void Custom_WithoutInverse_PredictionsAreNaN()
    {
        var custom = new CurveTransform(Math.Sqrt, Math.Log);
        var r = new RegressionPvalue(Small, custom);
        Assert.False(double.IsNaN(r.Slope));
        Assert.False(double.IsNaN(r.PValue));
        Assert.True(double.IsNaN(r.PredictY(7)));
        Assert.True(double.IsNaN(r.PredictX(9)));
        AssertAllNaN(r.CurveMeanResponseInterval(7));
        AssertAllNaN(r.CurvePredictionInterval(7));
    }

    [Fact]
    public void Custom_DecreasingInverse_IntervalOrdered()
    {
        // y' = 1/y (decreasing): Lower and Upper must still come out ordered.
        var custom = new CurveTransform(null, v => 1 / v, inverseY: v => 1 / v);
        var r = new RegressionPvalue(Small, custom);
        var ci = r.CurveMeanResponseInterval(3.5);
        Assert.True(ci.Lower <= ci.Fit && ci.Fit <= ci.Upper, $"[{ci.Lower}, {ci.Upper}] vs {ci.Fit}");
    }

    [Fact]
    public void CurveFitCustom_InCtor_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Regression(CurveFit.Custom));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RegressionPvalue(Small, (CurveFit)99));
        Assert.Throws<ArgumentNullException>(() => new Regression((CurveTransform)null!));
    }

    // ---------- domain ----------

    [Theory]
    [InlineData(CurveFit.Exponential, 3.0, 0.0)]
    [InlineData(CurveFit.Exponential, 3.0, -1.0)]
    [InlineData(CurveFit.Power, 0.0, 5.0)]
    [InlineData(CurveFit.Power, 3.0, -2.0)]
    [InlineData(CurveFit.Logarithmic, -3.0, 5.0)]
    public void Domain_InvalidPoint_MakesStatsNaN_NoThrow(CurveFit curve, double badX, double badY)
    {
        var data = new List<(double x, double y)>(Small) { (badX, badY) };
        var r = new RegressionPvalue(data, curve);
        Assert.True(double.IsNaN(r.Slope) || double.IsNaN(r.YIntercept));
        Assert.True(double.IsNaN(r.PValue));
        Assert.True(double.IsNaN(r.PredictY(7)));
        Assert.True(double.IsNaN(r.CurveB));
        AssertAllNaN(r.MeanResponseInterval(Math.Log(7)));
        AssertAllNaN(r.CurveMeanResponseInterval(7));
        Assert.True(r.IsNaN);
    }

    [Fact]
    public void Domain_StreamingAddOfInvalidPoint_MakesStatsNaN()
    {
        var r = new Regression(Small, CurveFit.Exponential);
        r.Add(7, 0);
        Assert.True(double.IsNaN(r.Slope));
        Assert.True(double.IsNaN(r.PredictY(7)));
    }

    [Fact]
    public void Domain_PredictOutsideDomain_IsNaN()
    {
        var r = new Regression(Small, CurveFit.Power);
        Assert.True(double.IsNaN(r.PredictY(0)));
        Assert.True(double.IsNaN(r.PredictY(-1)));
        Assert.True(double.IsNaN(r.PredictX(-5)));
        var log = new RegressionPvalue(Small, CurveFit.Logarithmic);
        AssertAllNaN(log.CurveMeanResponseInterval(-1));
    }

    [Fact]
    public void PredictX_ZeroSlope_IsNaN()
    {
        var r = new Regression([(1.0, 2.0), (2.0, 2.0), (3.0, 2.0)], CurveFit.Linear);
        Assert.Equal(0, r.Slope);
        Assert.True(double.IsNaN(r.PredictX(2)));
        Assert.Equal(2, r.PredictY(10));
    }

    // ---------- Linear is the existing behavior ----------

    [Fact]
    public void Linear_IdenticalToExisting()
    {
        var advertising = new List<(double x, double y)>
            { (1000, 101000), (1250, 116000), (1500, 165000), (2000, 209000), (2500, 264000) };
        var old = new RegressionPvalue(advertising);
        var lin = new RegressionPvalue(advertising, CurveFit.Linear);
        Assert.Equal(CurveFit.Linear, old.Curve);
        Assert.Same(CurveTransform.Linear, old.Transform);
        Assert.Equal(old.Slope, lin.Slope);
        Assert.Equal(old.YIntercept, lin.YIntercept);
        Assert.Equal(old.Correlation, lin.Correlation);
        Assert.Equal(old.PValue, lin.PValue);
        Assert.Equal(old.InterceptPValue, lin.InterceptPValue);
        Assert.Equal(old.ConfidenceInterval(), lin.ConfidenceInterval());
        Assert.Equal(old.MeanResponseInterval(4750), lin.MeanResponseInterval(4750));
        Assert.Equal(old.MeanResponseInterval(4750), lin.CurveMeanResponseInterval(4750));
        Assert.Equal(old.PredictionInterval(4750), lin.CurvePredictionInterval(4750));
        Assert.Equal(old.ToString(), lin.ToString());
        Assert.Equal(old.YIntercept, old.CurveB);
        // Manual p. 5-6: y'(4750) = 514672.4138, and x' for that y comes back to 4750.
        AssertRelative(514672.4137931034, old.PredictY(4750));
        AssertRelative(4750, old.PredictX(514672.4137931034));
    }

    [Fact]
    public void Ids_WithCurve_KeepIdsAndTransform()
    {
        var withIds = Census.Select((p, i) => ($"c{i}", p.x, p.y)).ToList();
        var r = new RegressionPvalue(withIds, CurveFit.Exponential);
        Assert.Equal("c0", r.IdPoints.First());
        Assert.Equal(Census.Count, r.IdPoints.Count());
        AssertRelative(314510828.7531725, r.PredictY(2000));
    }
}
