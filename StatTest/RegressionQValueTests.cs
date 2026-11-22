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
    public void QValue_InvalidFamilyEntries_AreExcludedFromRankingAndM()
    {
        var r = Sample();

        // NaN / out-of-range entries are dropped, so these equal the clean-family results above.
        Assert.Equal(r.QValue([0.1]), r.QValue([0.1, double.NaN]), 12);
        Assert.Equal(0.11215822040549586, r.QValue([0.5, 0.9, double.NaN, 1.5, -0.1]), 12);
        Assert.Equal(ThisP, r.QValue([double.NaN, double.NaN]), 12);
    }

    [Fact]
    public void QValue_FamilyWithNaN_MatchesStatsmodelsOnValidSubset()
    {
        // statsmodels fdr_bh on the non-NaN subset [0.01, 0.04, p, 0.20, 0.5] -> q(p) = 0.06666666666666667
        var r = Sample();
        Assert.Equal(0.06666666666666667, r.QValue([0.01, double.NaN, 0.04, r.PValue, double.NaN, 0.20, 0.5]), 12);
    }

    [Fact]
    public void BenjaminiHochberg_NaNEntriesGetNaN_RestMatchStatsmodels()
    {
        double[] family = [0.01, double.NaN, 0.04, 0.03, double.NaN, 0.20, 0.04, 0.5, double.NaN, 0.001];
        // statsmodels multipletests(p[~isnan(p)], method="fdr_bh"), NaN where p is NaN
        double[] expected = [0.035, double.NaN, 0.056, 0.056, double.NaN, 0.23333333333333336, 0.056, 0.5, double.NaN, 0.007];

        var q = RegressionQValue.BenjaminiHochberg(family);

        Assert.Equal(expected.Length, q.Length);
        for (var i = 0; i < expected.Length; i++)
            if (double.IsNaN(expected[i])) Assert.True(double.IsNaN(q[i]), $"index {i}");
            else Assert.Equal(expected[i], q[i], 12);
    }

    [Fact]
    public void BenjaminiHochberg_AgreesWithInstanceQValue()
    {
        var r = Sample();
        double[] family = [0.01, 0.02, 0.05, 0.10, 0.20, 0.03, 0.5, r.PValue];

        Assert.Equal(r.QValue(family), RegressionQValue.BenjaminiHochberg(family)[^1], 12);
        Assert.Empty(RegressionQValue.BenjaminiHochberg([]));
    }

    [Fact]
    public void QValue_TooFewPoints_ReturnsNaN()
    {
        var tooFew = new RegressionQValue([(1.0, 2.0), (2.0, 3.0)]);
        Assert.True(double.IsNaN(tooFew.QValue([0.1])));
    }

    [Fact]
    public void QValue_EmptyFamily_EqualsPValue()
    {
        // m = 1 (only this regression), so q = p
        Assert.Equal(ThisP, Sample().QValue(Array.Empty<double>()), 12);
    }

    [Fact]
    public void QValue_DataWithNaN_ReturnsNaN()
    {
        var r = new RegressionQValue([(1.0, 2.0), (2.0, double.NaN), (3.0, 5.0)]);
        Assert.True(double.IsNaN(r.QValue([0.1])));
    }

    [Fact]
    public void QValue_NoVariationInX_ReturnsNaN()
    {
        var r = new RegressionQValue([(2.0, 1.0), (2.0, 2.0), (2.0, 3.0)]);
        Assert.True(double.IsNaN(r.QValue([0.01, 0.2])));
    }

    [Fact]
    public void QValue_IdConstructor()
    {
        var r = new RegressionQValue([("a", 1.0, 2.0), ("b", 2.0, 3.0), ("c", 3.0, 5.0), ("d", 4.0, 4.0), ("e", 5.0, 6.0)]);

        Assert.Equal(5, r.IdPoints.Count());
        Assert.Equal(ThisP, r.QValue([r.PValue]), 12);
    }
}
