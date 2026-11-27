using LinearRegression;

namespace RegressionTest;

/// <summary>
///     Mean-response and prediction intervals (<see cref="RegressionPvalue.MeanResponseInterval"/>,
///     <see cref="RegressionPvalue.PredictionInterval"/>).
///     References: statsmodels OLS.get_prediction(x).summary_frame(alpha) columns mean, mean_ci_*
///     and obs_ci_* (statsmodels 0.15.0).
/// </summary>
public class MeanResponseIntervalTests
{
    // TI Applied Statistics p. 5-6 advertising example.
    private static readonly List<(double x, double y)> Advertising =
    [
        (1000, 101000),
        (1250, 116000),
        (1500, 165000),
        (2000, 209000),
        (2500, 264000)
    ];

    // y = 0.3x + noise, n = 20 (System.Random seed 2). Same set as InterceptStatsTests.Weak20.
    private static readonly List<(double x, double y)> Weak20 =
    [
        (-1.415191229889354, 0.17531380199090957),
        (-0.16751028375272065, -1.746026389550866),
        (0.7121923018076856, 1.8887459136989762),
        (-0.12296229265750719, -0.21717385047294377),
        (0.1432886305172531, 1.417284165317748),
        (-0.21974880651632664, -0.1707249619527373),
        (0.8159749980848928, 1.5496652020859256),
        (-0.16341166668061965, 2.212156756880563),
        (0.4488757425151228, 0.6698487091604215),
        (-0.057433200791628156, 1.2513616965294119),
        (-0.038952388113004964, -0.7933603287320926),
        (-2.3963397274489457, -0.9068799571812663),
        (-0.9115688874220362, -1.3181513337421322),
        (0.3090452735604671, -1.1328541655399171),
        (-0.360194321814743, -0.23211066604864036),
        (1.0309504078848344, -0.5928595954838725),
        (0.0929456006312321, 2.274103141443451),
        (-0.5958119770020796, 1.1168350950388455),
        (-0.8157851897162739, -0.12678754379615637),
        (0.1911443421341591, -0.8601262918228889)
    ];

    private static readonly List<(double x, double y)> Tiny5 = [(1, 2), (2, 4), (3, 5), (4, 4), (5, 5)];

    private static readonly double Weak20XBar = Weak20.Average(p => p.x);

    private const double RelTol = 1e-9;

    private static void AssertRelative(double expected, double actual, double relTol = RelTol)
    {
        Assert.False(double.IsNaN(actual), $"unexpected NaN (expected {expected:R})");
        Assert.True(Math.Abs(actual - expected) <= relTol * Math.Max(Math.Abs(expected), 1e-18),
            $"expected {expected:R}, got {actual:R}");
    }

    private static void AssertInterval(
        (double Fit, double Lower, double Upper) actual,
        double fit, double lower, double upper,
        double relTol = RelTol)
    {
        AssertRelative(fit, actual.Fit, relTol);
        AssertRelative(lower, actual.Lower, relTol);
        AssertRelative(upper, actual.Upper, relTol);
        Assert.True(actual.Lower <= actual.Fit && actual.Fit <= actual.Upper,
            $"interval does not contain Fit: [{actual.Lower}, {actual.Upper}] vs {actual.Fit}");
    }

    private static void AssertAllNaN((double Fit, double Lower, double Upper) r)
    {
        Assert.True(double.IsNaN(r.Fit) && double.IsNaN(r.Lower) && double.IsNaN(r.Upper),
            $"expected (NaN, NaN, NaN), got ({r.Fit}, {r.Lower}, {r.Upper})");
    }

    [Fact]
    public void Advertising_At4750_MatchesStatsmodels_95And90()
    {
        var r = new RegressionPvalue(Advertising);
        // statsmodels OLS.get_prediction(4750).summary_frame, columns mean / mean_ci_* / obs_ci_*
        AssertInterval(r.MeanResponseInterval(4750),
            514672.4137931037, 441400.1356220408, 587944.6919641666);
        AssertInterval(r.MeanResponseInterval(4750, 0.90),
            514672.4137931037, 460488.84439678205, 568855.9831894254);
        AssertInterval(r.PredictionInterval(4750),
            514672.4137931037, 436217.5219124269, 593127.3056737805);
        AssertInterval(r.PredictionInterval(4750, 0.90),
            514672.4137931037, 456656.3921160763, 572688.4354701311);
    }

    [Fact]
    public void Advertising_AtXBar_MatchesStatsmodels_95And90()
    {
        var r = new RegressionPvalue(Advertising);
        const double xbar = 1650.0;
        Assert.Equal(xbar, r.MeanX, 12);
        AssertInterval(r.MeanResponseInterval(xbar),
            171000.0, 158459.31970241596, 183540.6802975841);
        AssertInterval(r.MeanResponseInterval(xbar, 0.90),
            171000.0, 161726.38604746453, 180273.61395253552);
        AssertInterval(r.PredictionInterval(xbar),
            171000.0, 140281.73224354477, 201718.2677564553);
        AssertInterval(r.PredictionInterval(xbar, 0.90),
            171000.0, 148284.3777447334, 193715.62225526664);
    }

    [Fact]
    public void Weak20_AtXBarAndAway_MatchesStatsmodels_95And90()
    {
        var r = new RegressionPvalue(Weak20);
        var xbar = Weak20XBar;
        AssertInterval(r.MeanResponseInterval(xbar),
            0.2229129698911369, -0.34668827766222493, 0.7925142174444988);
        AssertInterval(r.MeanResponseInterval(xbar, 0.90),
            0.2229129698911369, -0.24722574765637895, 0.6930516874386528);
        AssertInterval(r.PredictionInterval(xbar),
            0.2229129698911369, -2.3873278629634243, 2.8331538027456977);
        AssertInterval(r.PredictionInterval(xbar, 0.90),
            0.2229129698911369, -1.9315332903998181, 2.377359230182092);

        AssertInterval(r.MeanResponseInterval(2.0),
            1.250618495122392, -0.4587893916821273, 2.9600263819269115);
        AssertInterval(r.MeanResponseInterval(2.0, 0.90),
            1.250618495122392, -0.16029630732911904, 2.661533297573903);
        AssertInterval(r.PredictionInterval(2.0),
            1.250618495122392, -1.817114698582109, 4.318351688826893);
        AssertInterval(r.PredictionInterval(2.0, 0.90),
            1.250618495122392, -1.2814338057831887, 3.782670796027973);
    }

    [Fact]
    public void Tiny5_AtXBar_MatchesStatsmodels()
    {
        var r = new RegressionPvalue(Tiny5);
        AssertInterval(r.MeanResponseInterval(3.0),
            4.0, 2.727021477886517, 5.272978522113483);
        AssertInterval(r.PredictionInterval(3.0),
            4.0, 0.8818521672997344, 7.118147832700266);
    }

    [Fact]
    public void Tiny5_AtZero_MeanResponseMatchesInterceptConfidenceInterval()
    {
        var r = new RegressionPvalue(Tiny5);
        var mean = r.MeanResponseInterval(0);
        var intercept = r.InterceptConfidenceInterval();
        AssertRelative(r.YIntercept, mean.Fit);
        AssertRelative(intercept.Lower, mean.Lower);
        AssertRelative(intercept.Upper, mean.Upper);
        // statsmodels / MATH.md worked example: 2.2 ± 3.182446 * 0.938083
        AssertInterval(mean, 2.2, -0.7853992610189073, 5.185399261018908);
    }

    [Fact]
    public void DefaultLevel_Is95()
    {
        var r = new RegressionPvalue(Advertising);
        var a = r.MeanResponseInterval(1650);
        var b = r.MeanResponseInterval(1650, 0.95);
        Assert.Equal(a, b);
        Assert.Equal(r.PredictionInterval(1650), r.PredictionInterval(1650, 0.95));
    }

    [Fact]
    public void BandIsNarrowestAtXBar()
    {
        var r = new RegressionPvalue(Advertising);
        const double xbar = 1650.0;
        var widthAtMean = Width(r.MeanResponseInterval(xbar));
        foreach (var x in new[] { 1000.0, 1250.0, 2000.0, 2500.0, 4750.0, 0.0, 3000.0 })
            Assert.True(Width(r.MeanResponseInterval(x)) > widthAtMean,
                $"mean-response band at x={x} was not wider than at x̄");

        var predAtMean = Width(r.PredictionInterval(xbar));
        foreach (var x in new[] { 1000.0, 2500.0, 4750.0 })
            Assert.True(Width(r.PredictionInterval(x)) > predAtMean,
                $"prediction interval at x={x} was not wider than at x̄");
    }

    [Fact]
    public void PredictionInterval_IsWiderThanMeanResponseInterval()
    {
        var r = new RegressionPvalue(Advertising);
        foreach (var x in new[] { 1650.0, 1000.0, 4750.0 })
        {
            foreach (var level in new[] { 0.95, 0.90 })
            {
                var mean = r.MeanResponseInterval(x, level);
                var pred = r.PredictionInterval(x, level);
                AssertRelative(mean.Fit, pred.Fit);
                Assert.True(Width(pred) > Width(mean),
                    $"prediction interval was not wider than the mean-response band at x={x}, level={level}");
            }
        }
    }

    [Fact]
    public void PerfectFit_IntervalCollapsesToFit()
    {
        // y = 2x + 3, zero residual. Matches slope/intercept CI: interval collapses to the estimate.
        var r = new RegressionPvalue([(1, 5), (2, 7), (3, 9), (4, 11)]);
        foreach (var x in new[] { 0.0, 2.5, 10.0 })
        {
            var fit = 3.0 + 2.0 * x;
            var mean = r.MeanResponseInterval(x);
            var pred = r.PredictionInterval(x);
            Assert.Equal((fit, fit, fit), mean);
            Assert.Equal((fit, fit, fit), pred);
        }
    }

    [Fact]
    public void ConstantY_IntervalCollapsesToY()
    {
        // Slope 0, zero residual: PValue is NaN (t = 0/0), but the CI collapses, as for the slope.
        var r = new RegressionPvalue([(1, 7), (2, 7), (3, 7), (4, 7)]);
        Assert.True(double.IsNaN(r.PValue));
        var mean = r.MeanResponseInterval(2.5);
        Assert.Equal((7.0, 7.0, 7.0), mean);
        Assert.Equal((7.0, 7.0, 7.0), r.PredictionInterval(2.5));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void FewerThanThreePoints_AllNaN(int n)
    {
        var r = new RegressionPvalue(Advertising.Take(n).ToList());
        AssertAllNaN(r.MeanResponseInterval(1650));
        AssertAllNaN(r.PredictionInterval(1650));
    }

    [Fact]
    public void AllXEqual_AllNaN()
    {
        var r = new RegressionPvalue([(2, 1), (2, 2), (2, 4), (2, 3)]);
        AssertAllNaN(r.MeanResponseInterval(2));
        AssertAllNaN(r.PredictionInterval(2));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonFiniteX_AllNaN(double x)
    {
        var r = new RegressionPvalue(Advertising);
        AssertAllNaN(r.MeanResponseInterval(x));
        AssertAllNaN(r.PredictionInterval(x));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(1.5)]
    [InlineData(-0.1)]
    [InlineData(double.NaN)]
    public void InvalidConfidenceLevel_AllNaN(double level)
    {
        var r = new RegressionPvalue(Advertising);
        AssertAllNaN(r.MeanResponseInterval(1650, level));
        AssertAllNaN(r.PredictionInterval(1650, level));
    }

    [Fact]
    public void NaNInData_AllNaN()
    {
        var r = new RegressionPvalue([(1, 2), (2, double.NaN), (3, 5), (4, 4)]);
        AssertAllNaN(r.MeanResponseInterval(2));
        AssertAllNaN(r.PredictionInterval(2));
    }

    [Fact]
    public void InfiniteInData_AllNaN()
    {
        var r = new RegressionPvalue([(1, 2), (2, double.PositiveInfinity), (3, 5), (4, 4)]);
        AssertAllNaN(r.MeanResponseInterval(2));
        AssertAllNaN(r.PredictionInterval(2));
    }

    private static double Width((double Fit, double Lower, double Upper) i) => i.Upper - i.Lower;
}
