using LinearRegression;

namespace RegressionTest;

/// <summary>
///     Checks results against independently computed reference values.
///     Reference values were computed with SciPy 1.x (<c>scipy.stats.linregress</c>,
///     <c>scipy.stats.t</c>, <c>numpy.std(ddof=1)</c>, <c>scipy.stats.false_discovery_control</c>).
/// </summary>
public class ReferenceValueTests
{
    private const int Precision = 9;

    // Small 5-point dataset used in the README and the existing tests.
    private static readonly List<(double x, double y)> Small =
    [
        (1.0, 2.0), (2.0, 3.0), (3.0, 5.0), (4.0, 4.0), (5.0, 6.0)
    ];

    // Anscombe's quartet, data set I (Anscombe, 1973, The American Statistician 27(1)).
    private static readonly List<(double x, double y)> AnscombeI =
    [
        (10, 8.04), (8, 6.95), (13, 7.58), (9, 8.81), (11, 8.33), (14, 9.96),
        (6, 7.24), (4, 4.26), (12, 10.84), (7, 4.82), (5, 5.68)
    ];

    [Fact]
    public void SmallDataset_BasicStatistics()
    {
        var r = new RegressionPvalue(Small);

        Assert.Equal(0.9, r.Slope, Precision);
        Assert.Equal(1.3, r.YIntercept, Precision);
        Assert.Equal(0.9, r.Correlation, Precision);
        Assert.Equal(0.81, r.RSquared, Precision);
        Assert.Equal(3.0, r.MeanX, Precision);
        Assert.Equal(4.0, r.MeanY, Precision);
    }

    [Fact]
    public void SmallDataset_SampleStandardDeviation()
    {
        var r = new RegressionPvalue(Small);

        // numpy.std(x, ddof=1)
        Assert.Equal(1.5811388300841898, r.StdDevX, Precision);
        Assert.Equal(1.5811388300841898, r.StdDevY, Precision);
    }

    [Fact]
    public void SmallDataset_PopulationVariance()
    {
        var r = new RegressionPvalue(Small);

        // numpy.var(x, ddof=0)
        Assert.Equal(2.0, r.VarianceX, Precision);
        Assert.Equal(2.0, r.VarianceY, Precision);
    }

    [Fact]
    public void SmallDataset_PValue()
    {
        var r = new RegressionPvalue(Small);
        Assert.Equal(0.03738607346849862, r.PValue, Precision);
    }

    [Fact]
    public void SmallDataset_ConfidenceInterval95()
    {
        var r = new RegressionPvalue(Small);
        var (lower, upper) = r.ConfidenceInterval();

        Assert.Equal(0.09910190986563006, lower, Precision);
        Assert.Equal(1.70089809013437, upper, Precision);
    }

    [Fact]
    public void SmallDataset_ConfidenceInterval85()
    {
        var r = new RegressionPvalue(Small);
        var (lower, upper) = r.ConfidenceInterval(0.85);

        Assert.Equal(0.4157235063723285, lower, Precision);
        Assert.Equal(1.3842764936276715, upper, Precision);
    }

    [Fact]
    public void SmallDataset_ConfidenceIntervalPlus_StandardError()
    {
        var r = new RegressionPvalue(Small);
        var result = r.ConfidenceIntervalPlus();

        Assert.Equal(0.9, result.Slope, Precision);
        Assert.Equal(0.25166114784235827, result.StandardError, Precision);
        Assert.Equal(0.03738607346849862, result.PValue, Precision);
    }

    [Fact]
    public void SmallDataset_MarginOfError()
    {
        var r = new RegressionPvalue(Small);

        var moeX = r.MarginOfError();
        Assert.Equal(3.0, moeX.Mean, Precision);
        Assert.Equal(1.9632431614775572, moeX.MarginOfError, Precision);

        var moeY = r.MarginOfError(true);
        Assert.Equal(4.0, moeY.Mean, Precision);
        Assert.Equal(1.9632431614775572, moeY.MarginOfError, Precision);
    }

    [Fact]
    public void AnscombeI_MatchesPublishedValues()
    {
        var r = new RegressionPvalue(AnscombeI);

        Assert.Equal(11, r.N);
        Assert.Equal(0.5000909090909091, r.Slope, Precision);
        Assert.Equal(3.0000909090909103, r.YIntercept, Precision);
        Assert.Equal(0.8164205163448398, r.Correlation, Precision);
        Assert.Equal(3.3166247903554, r.StdDevX, Precision);
        Assert.Equal(2.031568135925815, r.StdDevY, Precision);
        Assert.Equal(0.002169628873078799, r.PValue, Precision);

        var plus = r.ConfidenceIntervalPlus();
        Assert.Equal(0.1179055005956341, plus.StandardError, Precision);
        Assert.Equal(0.23337013638518744, plus.Lower, Precision);
        Assert.Equal(0.7668116817966308, plus.Upper, Precision);
    }
}
