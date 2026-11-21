using LinearRegression;

namespace RegressionTest;

/// <summary>
///     Tests for the experimental <see cref="RegressionQValue" /> (Benjamini-Hochberg q-values).
///     Reference values: statsmodels <c>multipletests(p, method="fdr_bh")</c>, which agrees with
///     R <c>p.adjust(p, "BH")</c> and SciPy <c>false_discovery_control</c>.
/// </summary>
public class RegressionQValueTests
{
    // README 5-point dataset; SciPy linregress slope p-value = 0.03738607346849862.
    private const double ThisP = 0.03738607346849862;

    private static RegressionQValue Sample() =>
        new([(1.0, 2.0), (2.0, 3.0), (3.0, 5.0), (4.0, 4.0), (5.0, 6.0)]);

    [Fact]
    public void Sample_PValueMatchesReference()
    {
        Assert.Equal(ThisP, Sample().PValue, 12);
    }

    // Bug 7: the step-up only compared against the next rank, not every higher rank.
    [Fact]
    public void QValue_StepUpTakesMinimumOverAllHigherRanks()
    {
        // sorted: p, 0.04, 0.045, 0.05, 0.9 (m = 5); p*m/k = 0.187, 0.1, 0.075, 0.0625, 0.9
        // statsmodels fdr_bh: 0.0625  (old code: 0.10)
        var r = Sample();
        Assert.Equal(0.0625, r.QValue([r.PValue, 0.04, 0.045, 0.05, 0.9]), 12);
    }

    [Fact]
    public void QValue_MiddleRank()
    {
        // family 0.01, p, 0.045, 0.05 -> statsmodels fdr_bh for p: 0.05  (old code: 0.06)
        var r = Sample();
        Assert.Equal(0.05, r.QValue([0.01, r.PValue, 0.045, 0.05]), 12);
    }

    [Fact]
    public void QValue_Ties()
    {
        // family p, p, 0.2 -> statsmodels fdr_bh: 0.05607911020274793
        var r = Sample();
        Assert.Equal(0.05607911020274793, r.QValue([r.PValue, r.PValue, 0.2]), 12);
    }

    [Fact]
    public void QValue_SingleTest_EqualsPValue()
    {
        var r = Sample();
        Assert.Equal(ThisP, r.QValue([r.PValue]), 12);
    }

    [Fact]
    public void QValue_OwnPValueMissing_IsAddedToFamily()
    {
        // family becomes p, 0.5, 0.9 (m = 3) -> statsmodels fdr_bh: 0.11215822040549586
        var r = Sample();
        Assert.Equal(0.11215822040549586, r.QValue([0.5, 0.9]), 12);
    }

    [Fact]
    public void QValue_AgreesWithTestReferenceCalculator()
    {
        var r = Sample();
        double[] family = [0.01, 0.02, 0.05, 0.10, 0.20, 0.03, 0.5, r.PValue];
        var reference = new QValueCalculator().CalculateQValues(family);

        Assert.Equal(reference[^1], r.QValue(family), 12);
    }

    [Fact]
    public void QValue_FromRegressionList_MatchesPValueList()
    {
        var a = Sample();
        var b = new RegressionQValue([(1.0, 5.0), (2.0, 1.0), (3.0, 4.0), (4.0, 2.0), (5.0, 3.0)]);

        var fromList = a.QValue(new List<RegressionPvalue> { a, b });

        Assert.Equal(a.QValue([a.PValue, b.PValue]), fromList, 12);
        Assert.InRange(fromList, a.PValue, 1.0);
    }

    [Fact]
    public void QValue_InvalidPValues_ReturnNaN()
    {
        var r = Sample();

        Assert.True(double.IsNaN(r.QValue([0.1, double.NaN])));
        Assert.True(double.IsNaN(r.QValue([0.1, 1.5])));
        Assert.True(double.IsNaN(r.QValue([-0.1])));
    }

    [Fact]
    public void QValue_Throws_WhenTooFewPointsOrEmptyFamily()
    {
        var tooFew = new RegressionQValue([(1.0, 2.0), (2.0, 3.0)]);
        Assert.Throws<InvalidOperationException>(() => tooFew.QValue([0.1]));

        Assert.Throws<InvalidOperationException>(() => Sample().QValue(Array.Empty<double>()));
    }

    [Fact]
    public void QValue_DataWithNaN_ReturnsNaN()
    {
        var r = new RegressionQValue([(1.0, 2.0), (2.0, double.NaN), (3.0, 5.0)]);
        Assert.True(double.IsNaN(r.QValue([0.1])));
    }

    [Fact]
    public void QValue_IdConstructor()
    {
        var r = new RegressionQValue([("a", 1.0, 2.0), ("b", 2.0, 3.0), ("c", 3.0, 5.0), ("d", 4.0, 4.0), ("e", 5.0, 6.0)]);

        Assert.Equal(5, r.IdPoints.Count());
        Assert.Equal(ThisP, r.QValue([r.PValue]), 12);
    }
}
