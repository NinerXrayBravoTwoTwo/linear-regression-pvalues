using LinearRegression;

namespace RegressionTest;

/// <summary>
///     Residuals and diagnostics (<see cref="RegressionPvalue.Residuals"/>, <see cref="RegressionPvalue.CurveResiduals"/>).
///     References: statsmodels 0.15.0 OLS results.resid, get_influence().resid_studentized_internal,
///     hat_matrix_diag and cooks_distance[0]; the far-from-zero set uses exact rational arithmetic.
///     Expected arrays come from a generator script kept outside the repository.
/// </summary>
public class ResidualsTests
{
    // TI Applied Statistics p. 5-6 advertising example.
    private static readonly List<(double x, double y)> Advertising =
        [(1000, 101000), (1250, 116000), (1500, 165000), (2000, 209000), (2500, 264000)];

    private static readonly List<(double x, double y)> Tiny5 = [(1, 2), (2, 4), (3, 5), (4, 4), (5, 5)];

    // y = 0.3x + noise, n = 20 (System.Random seed 2). Same set as InterceptStatsTests.Weak20.
    private static readonly List<(double x, double y)> Weak20 =
    [
        (-1.415191229889354, 0.17531380199090957), (-0.16751028375272065, -1.746026389550866),
        (0.7121923018076856, 1.8887459136989762), (-0.12296229265750719, -0.21717385047294377),
        (0.1432886305172531, 1.417284165317748), (-0.21974880651632664, -0.1707249619527373),
        (0.8159749980848928, 1.5496652020859256), (-0.16341166668061965, 2.212156756880563),
        (0.4488757425151228, 0.6698487091604215), (-0.057433200791628156, 1.2513616965294119),
        (-0.038952388113004964, -0.7933603287320926), (-2.3963397274489457, -0.9068799571812663),
        (-0.9115688874220362, -1.3181513337421322), (0.3090452735604671, -1.1328541655399171),
        (-0.360194321814743, -0.23211066604864036), (1.0309504078848344, -0.5928595954838725),
        (0.0929456006312321, 2.274103141443451), (-0.5958119770020796, 1.1168350950388455),
        (-0.8157851897162739, -0.12678754379615637), (0.1911443421341591, -0.8601262918228889)
    ];

    // One vertical outlier (x = 4) and one high-leverage point (x = 20).
    private static readonly List<(double x, double y)> Outlier =
        [(1, 1.1), (2, 2.3), (3, 2.9), (4, 11.0), (5, 5.1), (6, 5.8), (7, 7.2), (20, 9.0)];

    // TI Applied Statistics p. 5-11: US census 1890-1970.
    private static readonly List<(double x, double y)> Census =
    [
        (1890, 62947714), (1900, 75994575), (1910, 91972266), (1920, 105710620), (1930, 122775046),
        (1940, 131669275), (1950, 150697361), (1960, 179323175), (1970, 203235298)
    ];

    private static readonly List<(double x, double y)> Small =
        [(1, 2.1), (2, 3.9), (3, 6.2), (4, 7.8), (5, 10.5), (6, 11.9)];

    private static readonly List<(double x, double y)> FarFromZero =
        Enumerable.Range(0, 8).Select(k => (1e9 + k, new[] { 3.1, 4.9, 7.2, 8.8, 11.3, 12.9, 15.4, 16.7 }[k])).ToList();

    private const double Tol = 1e-9;

    /// <summary>Largest relative difference seen against the references; printed by the report test.</summary>
    private static double _maxDiff;

    /// <summary>Largest absolute difference seen (unitless fields; residuals and fits are in the data's units).</summary>
    private static double _maxAbsDiff;

    private static void Close(double expected, double actual, string what, double tol = Tol)
    {
        Assert.False(double.IsNaN(actual), $"{what}: unexpected NaN (expected {expected:R})");
        var diff = Math.Abs(actual - expected) / Math.Max(Math.Abs(expected), 1.0);
        if (tol <= Tol)
            lock (typeof(ResidualsTests))
            {
                _maxDiff = Math.Max(_maxDiff, diff);
                if (what.StartsWith("Standardized") || what.StartsWith("Studentized") || what.StartsWith("Leverage") || what.StartsWith("CooksDistance"))
                    _maxAbsDiff = Math.Max(_maxAbsDiff, Math.Abs(actual - expected));
            }
        Assert.True(diff <= tol, $"{what}: expected {expected:R}, got {actual:R}");
    }

    private static void AssertAllNaN(ResidualPoint p)
    {
        Assert.True(double.IsNaN(p.Fit) && double.IsNaN(p.Residual) && double.IsNaN(p.Standardized)
                    && double.IsNaN(p.Studentized) && double.IsNaN(p.Leverage) && double.IsNaN(p.CooksDistance), p.ToString());
    }

    private static void CheckLinear(List<(double x, double y)> data, double[] fit, double[] resid, double[] stand,
        double[] stud, double[] lev, double[] cook)
    {
        var rs = new RegressionPvalue(data).Residuals();
        Assert.Equal(data.Count, rs.Count);
        for (var i = 0; i < data.Count; i++)
        {
            Assert.Equal(data[i].x, rs[i].X);
            Assert.Equal(data[i].y, rs[i].Y);
            Close(fit[i], rs[i].Fit, $"Fit[{i}]");
            Close(resid[i], rs[i].Residual, $"Residual[{i}]");
            Close(stand[i], rs[i].Standardized, $"Standardized[{i}]");
            Close(stud[i], rs[i].Studentized, $"Studentized[{i}]");
            Close(lev[i], rs[i].Leverage, $"Leverage[{i}]");
            Close(cook[i], rs[i].CooksDistance, $"CooksDistance[{i}]");
        }
    }

    // ---------- statsmodels, linear ----------

    [Fact]
    public void Advertising_MatchesStatsmodels() =>
        CheckLinear(Advertising, Ref.AdvertisingFit, Ref.AdvertisingResid, Ref.AdvertisingStand, Ref.AdvertisingStud,
            Ref.AdvertisingLev, Ref.AdvertisingCook);

    [Fact]
    public void Tiny5_MatchesStatsmodels() =>
        CheckLinear(Tiny5, Ref.Tiny5Fit, Ref.Tiny5Resid, Ref.Tiny5Stand, Ref.Tiny5Stud, Ref.Tiny5Lev, Ref.Tiny5Cook);

    [Fact]
    public void Weak20_MatchesStatsmodels() =>
        CheckLinear(Weak20, Ref.Weak20Fit, Ref.Weak20Resid, Ref.Weak20Stand, Ref.Weak20Stud, Ref.Weak20Lev, Ref.Weak20Cook);

    [Fact]
    public void Outlier_MatchesStatsmodels() =>
        CheckLinear(Outlier, Ref.OutlierFit, Ref.OutlierResid, Ref.OutlierStand, Ref.OutlierStud, Ref.OutlierLev,
            Ref.OutlierCook);

    [Fact]
    public void Advertising_ValuesFromTheManualFit()
    {
        // Manual p. 5-6: y' = -11922.41379 + 110.862069 x.
        var rs = new RegressionPvalue(Advertising).Residuals();
        Close(-11922.41379 + 110.862069 * 1000, rs[0].Fit, "Fit[0]", 1e-8);
        Close(101000 - (-11922.41379 + 110.862069 * 1000), rs[0].Residual, "Residual[0]", 1e-6);
    }

    // ---------- identities ----------

    [Fact]
    public void Identities_ResidualsSumToZero_LeverageSumsToTwo_StandardizedSquaresSumToDf()
    {
        foreach (var data in new[] { Advertising, Tiny5, Weak20, Outlier })
        {
            var rs = new RegressionPvalue(data).Residuals();
            var scale = data.Max(p => Math.Abs(p.y));
            Assert.True(Math.Abs(rs.Sum(r => r.Residual)) < 1e-10 * scale);
            Close(2.0, rs.Sum(r => r.Leverage), "sum of leverage");
            Close(data.Count - 2, rs.Sum(r => r.Standardized * r.Standardized), "sum of standardized^2");
            Assert.All(rs, r => Assert.InRange(r.Leverage, 1.0 / data.Count - 1e-15, 1.0));
        }
    }

    [Fact]
    public void Residuals_AgreeWithThePValue()
    {
        // Rebuild the slope p-value from the residuals: s from Standardized, t = m / (s / sqrt(Sxx)).
        var r = new RegressionPvalue(Weak20);
        var rs = r.Residuals();
        var s = Math.Sqrt(rs.Sum(p => p.Residual * p.Residual) / (Weak20.Count - 2));
        Close(s, rs[0].Residual / rs[0].Standardized, "s");
        var xBar = Weak20.Average(p => p.x);
        var sxx = Weak20.Sum(p => (p.x - xBar) * (p.x - xBar));
        var t = r.Slope / (s / Math.Sqrt(sxx));
        var p2 = 2 * MathNet.Numerics.Distributions.StudentT.CDF(0, 1, Weak20.Count - 2, -Math.Abs(t));
        Close(r.PValue, p2, "p-value", 1e-9);
    }

    [Fact]
    public void Residuals_FitMatchesMeanResponseInterval()
    {
        var r = new RegressionPvalue(Weak20);
        var rs = r.Residuals();
        for (var i = 0; i < rs.Count; i++)
            Assert.Equal(r.MeanResponseInterval(rs[i].X).Fit, rs[i].Fit);
    }

    [Fact]
    public void Outlier_FlagsTheOutlierAndTheLeveragePoint()
    {
        var rs = new RegressionPvalue(Outlier).Residuals();
        Assert.True(Math.Abs(rs[3].Studentized) > 2, "vertical outlier at x = 4");
        Assert.True(rs[7].Leverage > 4.0 / Outlier.Count, "x = 20 is a high-leverage point");
        Assert.Equal(rs.Max(p => p.CooksDistance), rs[7].CooksDistance); // the far point is the most influential
    }

    [Fact]
    public void Order_FollowsDataPoints()
    {
        var shuffled = Weak20.AsEnumerable().Reverse().ToList();
        var a = new RegressionPvalue(Weak20).Residuals();
        var b = new RegressionPvalue(shuffled).Residuals();
        for (var i = 0; i < a.Count; i++)
        {
            Assert.Equal(b[a.Count - 1 - i].X, a[i].X);
            Close(a[i].Residual, b[a.Count - 1 - i].Residual, "residual order", 1e-12);
        }
    }

    [Fact]
    public void IdConstructor_SameResidualsAsPlain()
    {
        var withIds = new RegressionPvalue(Tiny5.Select((p, i) => ($"id{i}", p.x, p.y)).ToList()).Residuals();
        Assert.Equal(new RegressionPvalue(Tiny5).Residuals(), withIds);
    }

    [Fact]
    public void FarFromZero_MatchesExactArithmetic()
    {
        var rs = new RegressionPvalue(FarFromZero).Residuals();
        for (var i = 0; i < rs.Count; i++)
        {
            Close(Ref.FarResid[i], rs[i].Residual, $"far residual[{i}]", 1e-6);
            Close(Ref.FarLev[i], rs[i].Leverage, $"far leverage[{i}]", 1e-9);
            Close(Ref.FarStud[i], rs[i].Studentized, $"far studentized[{i}]", 1e-6);
        }
    }

    [Fact]
    public void ReadonlyRecordStruct_HasValueEquality()
    {
        var a = new RegressionPvalue(Tiny5).Residuals();
        var b = new RegressionPvalue(Tiny5).Residuals();
        Assert.Equal(a[2], b[2]);
        var (x, y, fit, res, std, stu, lev, cook) = a[2];
        Assert.Equal(3, x);
        Assert.Equal(5, y);
        Assert.Equal(a[2].Fit, fit);
        Assert.Equal(a[2].CooksDistance, cook);
        _ = (res, std, stu, lev);
    }

    // ---------- degenerate cases ----------

    [Fact]
    public void NoPoints_EmptyList() => Assert.Empty(new RegressionPvalue().Residuals());

    [Fact]
    public void FewerThanThreePoints_AllNaNExceptXY()
    {
        var rs = new RegressionPvalue([(1, 1), (2, 3)]).Residuals();
        Assert.Equal(2, rs.Count);
        Assert.Equal((1.0, 1.0), (rs[0].X, rs[0].Y));
        Assert.Equal((2.0, 3.0), (rs[1].X, rs[1].Y));
        Assert.All(rs, AssertAllNaN);
    }

    [Fact]
    public void AllXEqual_AllNaN()
    {
        var rs = new RegressionPvalue([(2, 1), (2, 3), (2, 7)]).Residuals();
        Assert.Equal(3, rs.Count);
        Assert.All(rs, AssertAllNaN);
        Assert.All(rs, p => Assert.Equal(2.0, p.X));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NanOrInfinityInData_AllNaN(double bad)
    {
        var inY = new RegressionPvalue([(1, 1), (2, bad), (3, 4), (4, 4.5)]).Residuals();
        Assert.Equal(4, inY.Count);
        Assert.All(inY, AssertAllNaN);
        Assert.Equal(1.0, inY[0].Y);
        var inX = new RegressionPvalue([(1, 1), (bad, 2), (3, 4), (4, 4.5)]).Residuals();
        Assert.All(inX, AssertAllNaN);
    }

    [Fact]
    public void PerfectFit_ResidualZero_ScaledFieldsNaN_LeverageDefined()
    {
        var rs = new RegressionPvalue([(1, 2), (2, 4), (3, 6), (4, 8)]).Residuals();
        Assert.All(rs, p =>
        {
            Assert.Equal(0.0, p.Residual);
            Assert.True(double.IsNaN(p.Standardized) && double.IsNaN(p.Studentized) && double.IsNaN(p.CooksDistance));
            Assert.False(double.IsNaN(p.Fit));
            Assert.False(double.IsNaN(p.Leverage));
        });
        Close(0.7, rs[0].Leverage, "leverage", 1e-12); // 1/4 + 2.25/5
    }

    [Fact]
    public void AllYEqual_ResidualZero_ScaledFieldsNaN()
    {
        var rs = new RegressionPvalue([(1, 5), (2, 5), (3, 5)]).Residuals();
        Assert.All(rs, p =>
        {
            Assert.Equal(0.0, p.Residual);
            Assert.Equal(5.0, p.Fit);
            Assert.True(double.IsNaN(p.Standardized) && double.IsNaN(p.CooksDistance));
        });
    }

    [Fact]
    public void LeverageOne_StudentizedAndCooksNaN_OthersDefined()
    {
        // x = 1, 1, 1, 5: the last point alone fixes the line (h = 1, residual 0).
        var rs = new RegressionPvalue([(1, 1), (1, 2), (1, 4), (5, 9)]).Residuals();
        Assert.Equal(1.0, rs[3].Leverage);
        Assert.Equal(0.0, rs[3].Residual, 12);
        Assert.True(double.IsNaN(rs[3].Studentized) && double.IsNaN(rs[3].CooksDistance));
        Assert.False(double.IsNaN(rs[3].Standardized));
        for (var i = 0; i < 3; i++)
        {
            Assert.False(double.IsNaN(rs[i].Studentized), $"studentized[{i}]");
            Assert.False(double.IsNaN(rs[i].CooksDistance), $"cook[{i}]");
            Close(1.0 / 3.0, rs[i].Leverage, "leverage", 1e-12);
        }
    }

    // ---------- curve fits ----------

    private static void CheckCurve(List<(double x, double y)> raw, CurveFit curve, double[] fit, double[] resid,
        double[] stand, double[] stud, double[] lev, double[] cook)
    {
        var r = new RegressionPvalue(raw, curve);
        var cr = r.CurveResiduals();
        var lin = r.Residuals();
        Assert.Equal(raw.Count, cr.Count);
        for (var i = 0; i < raw.Count; i++)
        {
            Close(raw[i].x, cr[i].X, $"X[{i}]", 1e-12);
            Close(raw[i].y, cr[i].Y, $"Y[{i}]", 1e-12);
            Close(fit[i], cr[i].Fit, $"Fit[{i}]");
            Close(resid[i], cr[i].Residual, $"Residual[{i}]");
            Close(r.PredictY(raw[i].x), cr[i].Fit, $"PredictY[{i}]", 1e-12);
            // diagnostics are those of the linearized fit, unchanged
            Close(stand[i], cr[i].Standardized, $"Standardized[{i}]");
            Close(stud[i], cr[i].Studentized, $"Studentized[{i}]");
            Close(lev[i], cr[i].Leverage, $"Leverage[{i}]");
            Close(cook[i], cr[i].CooksDistance, $"CooksDistance[{i}]");
            Assert.Equal(lin[i].Standardized, cr[i].Standardized);
            Assert.Equal(lin[i].Studentized, cr[i].Studentized);
            Assert.Equal(lin[i].Leverage, cr[i].Leverage);
            Assert.Equal(lin[i].CooksDistance, cr[i].CooksDistance);
        }
    }

    [Fact]
    public void Census_Exponential_OriginalUnits() =>
        CheckCurve(Census, CurveFit.Exponential, Ref.CensusExpOrigFit, Ref.CensusExpOrigResid, Ref.CensusExpStand,
            Ref.CensusExpStud, Ref.CensusExpLev, Ref.CensusExpCook);

    [Fact]
    public void Census_Exponential_LinearizedResidualsMatchStatsmodelsOnLnY()
    {
        var rs = new RegressionPvalue(Census, CurveFit.Exponential).Residuals();
        for (var i = 0; i < Census.Count; i++)
        {
            Close(Math.Log(Census[i].y), rs[i].Y, $"ln y[{i}]", 1e-14);
            Close(Ref.CensusExpFit[i], rs[i].Fit, $"ln fit[{i}]");
            Close(Ref.CensusExpResid[i], rs[i].Residual, $"ln residual[{i}]");
        }
    }

    [Fact]
    public void SmallPower_OriginalUnits() =>
        CheckCurve(Small, CurveFit.Power, Ref.SmallPowerOrigFit, Ref.SmallPowerOrigResid, Ref.SmallPowerStand,
            Ref.SmallPowerStud, Ref.SmallPowerLev, Ref.SmallPowerCook);

    [Fact]
    public void Census_Exponential_Prediction2000_UsesTheSameFit()
    {
        var r = new RegressionPvalue(Census, CurveFit.Exponential);
        // The fit passes through the residuals' fitted values: extrapolating to 1970 reproduces point 8.
        Close(r.PredictY(1970), r.CurveResiduals()[8].Fit, "Fit at 1970", 1e-12);
    }

    [Fact]
    public void LinearCurve_EqualsPlainResiduals() =>
        Assert.Equal(new RegressionPvalue(Weak20).Residuals(), new RegressionPvalue(Weak20, CurveFit.Linear).CurveResiduals());

    [Fact]
    public void PlainInstance_CurveResiduals_EqualsResiduals() =>
        Assert.Equal(new RegressionPvalue(Tiny5).Residuals(), new RegressionPvalue(Tiny5).CurveResiduals());

    [Fact]
    public void Logarithmic_OriginalUnits_XIsRaw()
    {
        var r = new RegressionPvalue(Small, CurveFit.Logarithmic);
        var cr = r.CurveResiduals();
        for (var i = 0; i < Small.Count; i++)
        {
            Close(Small[i].x, cr[i].X, "X", 1e-12);
            Close(Small[i].y, cr[i].Y, "Y", 1e-12);
            Assert.Equal(r.Residuals()[i].Residual, cr[i].Residual, 12); // identity y transform
        }
    }

    [Fact]
    public void OutOfDomainPoint_AllNaN()
    {
        // ln of a non-positive y: the fit is undefined, as for the p-value.
        var rs = new RegressionPvalue([(1, 2), (2, 4), (3, -1), (4, 9)], CurveFit.Exponential).CurveResiduals();
        Assert.Equal(4, rs.Count);
        Assert.All(rs, AssertAllNaN);
    }

    [Fact]
    public void Custom_WithoutInverseY_FitAndResidualNaN_DiagnosticsKept()
    {
        var t = new CurveTransform(null, Math.Log);
        var r = new RegressionPvalue(Census, t);
        var cr = r.CurveResiduals();
        var lin = r.Residuals();
        for (var i = 0; i < cr.Count; i++)
        {
            Assert.Equal(Census[i].x, cr[i].X); // x has no transform: identity inverse
            Assert.True(double.IsNaN(cr[i].Y) && double.IsNaN(cr[i].Fit) && double.IsNaN(cr[i].Residual));
            Assert.Equal(lin[i].Studentized, cr[i].Studentized);
            Assert.Equal(lin[i].CooksDistance, cr[i].CooksDistance);
        }
    }

    [Fact]
    public void Custom_WithoutInverseX_XNaN_RestKept()
    {
        var t = new CurveTransform(Math.Log, null);
        var cr = new RegressionPvalue(Small, t).CurveResiduals();
        Assert.All(cr, p =>
        {
            Assert.True(double.IsNaN(p.X));
            Assert.False(double.IsNaN(p.Fit));
            Assert.False(double.IsNaN(p.Residual));
        });
    }

    [Fact]
    public void Custom_WithInverses_EqualsBuiltInExponential()
    {
        var t = new CurveTransform(null, Math.Log, null, Math.Exp);
        Assert.Equal(new RegressionPvalue(Census, CurveFit.Exponential).CurveResiduals(),
            new RegressionPvalue(Census, t).CurveResiduals());
    }

    [Fact]
    public void ReportMaxDifference()
    {
        // Runs the statsmodels comparisons once more and writes the largest relative difference to the test output file.
        Advertising_MatchesStatsmodels();
        Tiny5_MatchesStatsmodels();
        Weak20_MatchesStatsmodels();
        Outlier_MatchesStatsmodels();
        Census_Exponential_OriginalUnits();
        SmallPower_OriginalUnits();
        Assert.True(_maxDiff < Tol);
        var path = Environment.GetEnvironmentVariable("RESIDUALS_MAXDIFF_FILE");
        if (!string.IsNullOrEmpty(path)) File.AppendAllText(path, $"relative {_maxDiff:R} absolute-unitless {_maxAbsDiff:R}\n");
    }
}

/// <summary>Reference values from /workspace/residuals_ref.py (statsmodels 0.15.0 and exact arithmetic).</summary>
internal static class Ref
{
// Advertising
    public static readonly double[] AdvertisingFit = [98939.65517241375, 126655.1724137931, 154370.6896551724, 209801.72413793107, 265232.75862068974];
    public static readonly double[] AdvertisingResid = [2060.3448275862465, -10655.172413793101, 10629.310344827594, -801.7241379310726, -1232.7586206897395];
    public static readonly double[] AdvertisingStand = [0.2338271294003119, -1.209248250789874, 1.2063131822199968, -0.09098712566623272, -0.13990493516421024];
    public static readonly double[] AdvertisingStud = [0.3278671358499484, -1.4561277181125414, 1.3619726048839074, -0.1075647306538275, -0.2546994499103948];
    public static readonly double[] AdvertisingLev = [0.4913793103448275, 0.3103448275862068, 0.21551724137931033, 0.28448275862068984, 0.6982758620689659];
    public static readonly double[] AdvertisingCook = [0.05192644872809801, 0.47706928457751807, 0.2548034857766835, 0.002300094290727744, 0.07506595132224689];
// Tiny5
    public static readonly double[] Tiny5Fit = [2.8, 3.4, 4.0, 4.6, 5.199999999999999];
    public static readonly double[] Tiny5Resid = [-0.7999999999999998, 0.6000000000000001, 1.0, -0.5999999999999996, -0.1999999999999993];
    public static readonly double[] Tiny5Stand = [-0.8944271909999159, 0.6708203932499371, 1.1180339887498951, -0.6708203932499366, -0.22360679774997821];
    public static readonly double[] Tiny5Stud = [-1.4142135623730945, 0.8017837257372733, 1.2500000000000004, -0.8017837257372727, -0.3535533905932726];
    public static readonly double[] Tiny5Lev = [0.5999999999999996, 0.2999999999999998, 0.19999999999999993, 0.3000000000000001, 0.6000000000000001];
    public static readonly double[] Tiny5Cook = [1.4999999999999967, 0.13775510204081626, 0.19531250000000006, 0.13775510204081623, 0.09374999999999943];
// Weak20
    public static readonly double[] Weak20Fit = [-0.36232781176601514, 0.22693417644520314, 0.6424052118946032, 0.2479735598587785, 0.373720088920424, 0.202262664164588, 0.6914203051679207, 0.22886989506966482, 0.5180445412134298, 0.27892201914088, 0.28765024445880677, -0.8257103104670099, -0.124474133108183, 0.45200459694413253, 0.1359322422435411, 0.7929501376042354, 0.3499437910014167, 0.024653370522957185, -0.07923685323860735, 0.39632166175197114];
    public static readonly double[] Weak20Resid = [0.5376416137569247, -1.9729605659960692, 1.2463407018043728, -0.46514741033172224, 1.043564076397324, -0.3729876261173253, 0.8582448969180049, 1.983286861810898, 0.1518041679469917, 0.9724396773885319, -1.0810105731908994, -0.08116964671425642, -1.193677200633949, -1.5848587624840498, -0.36804290829218145, -1.3858097330881078, 1.9241593504420342, 1.0921817245158882, -0.047550690557549014, -1.25644795357486];
    public static readonly double[] Weak20Stand = [0.44342163960298486, -1.6272055336876203, 1.027923477939512, -0.38363181358879356, 0.8606828079273021, -0.3076227369545561, 0.7078402222494908, 1.635722179170548, 0.12520097278045744, 0.8020227324843431, -0.8915669258614886, -0.06694492559927341, -0.9844890870019445, -1.3071173306119175, -0.3035445651216914, -1.1429510073257216, 1.58695657515858, 0.9007803686271357, -0.03921758404068659, -1.03625946614666];
    public static readonly double[] Weak20Stud = [0.4896231516009952, -1.6694830372563974, 1.0937203536847564, -0.3936472568621994, 0.8870756633428869, -0.31564112089697394, 0.7602819933879295, 1.6782274257069885, 0.13074538620425075, 0.8233733841588777, -0.9154940684938314, -0.09167786634834188, -1.0352952625948448, -1.3553391029847535, -0.3119012446615838, -1.2569477462677976, 1.6334497983098721, 0.9315142165102719, -0.04098982588044428, -1.0696154274576088];
    public static readonly double[] Weak20Lev = [0.1798186741794252, 0.05000612887507918, 0.1166984702007865, 0.05023804035409287, 0.058620087869416235, 0.05016162962954656, 0.13319568383777833, 0.05001344969068162, 0.08301409116486486, 0.05118900698858321, 0.051588461998440895, 0.4667801210421996, 0.09573991865014551, 0.0698923659169659, 0.05286757033537019, 0.17316132519836352, 0.05611626391303333, 0.06489829105777643, 0.08460291570376927, 0.06139750339368056];
    public static readonly double[] Weak20Cook = [0.026279603520820823, 0.07335613789707536, 0.07902031800894677, 0.004098287133235956, 0.024500382916179612, 0.0026307470121137333, 0.0444107901865282, 0.07413802064818421, 0.0007737700300806767, 0.018287765727421895, 0.02279485456724377, 0.0036787901853549154, 0.056741152485810346, 0.06901801651164043, 0.002715081463502168, 0.16543773292078254, 0.07931436173496953, 0.030110875917662563, 7.764222224397222e-05, 0.037419185294488434];
// Outlier
    public static readonly double[] OutlierFit = [3.909126984126982, 4.237301587301585, 4.565476190476188, 4.8936507936507905, 5.221825396825394, 5.549999999999997, 5.8781746031746, 10.144444444444439];
    public static readonly double[] OutlierResid = [-2.8091269841269817, -1.9373015873015849, -1.6654761904761881, 6.1063492063492095, -0.12182539682539417, 0.25000000000000266, 1.3218253968254006, -1.1444444444444386];
    public static readonly double[] OutlierStand = [-0.929176956520257, -0.6408026418606997, -0.5508907595021213, 2.019801526618694, -0.04029627428333281, 0.08269268012540877, 0.4372211388852928, -0.3785487134629764];
    public static readonly double[] OutlierStud = [-1.054934074533503, -0.7113411091883018, -0.6013268169828858, 2.179114327521513, -0.04317654649571606, 0.0884021935495344, 0.46847256148936073, -1.2140567914135847];
    public static readonly double[] OutlierLev = [0.2242063492063491, 0.18849206349206335, 0.16071428571428564, 0.14087301587301576, 0.1289682539682539, 0.12499999999999992, 0.1289682539682539, 0.9027777777777775];
    public static readonly double[] OutlierCook = [0.16081343591066485, 0.05876599815894633, 0.03462069646175447, 0.3893144190774558, 0.00013801129940123853, 0.0005582105588835239, 0.01624752295722799, 6.843264502180984];
// CensusExp
    public static readonly double[] CensusExpFit = [18.013515362456054, 18.154698426338157, 18.295881490220253, 18.43706455410235, 18.578247617984452, 18.719430681866548, 18.86061374574865, 19.001796809630747, 19.142979873512843];
    public static readonly double[] CensusExpResid = [-0.0557003591961589, -0.008525912214285114, 0.04111614282417264, 0.039151364727935345, 0.04761672657416938, -0.023626837738898843, -0.02982959391773221, 0.0029033732414021074, -0.013104904300266895];
    public static readonly double[] CensusExpStand = [-1.45248604100927, -0.22232834144763558, 1.0721766317868298, 1.0209415446231147, 1.2416909274133663, -0.6161118618299348, -0.7778597732538302, 0.07571062675062983, -0.3417337130244692];
    public static readonly double[] CensusExpStud = [-1.8413621362391428, -0.2586456985881489, 1.182419905735448, 1.093168988739163, 1.3170121123677068, -0.6596992594668715, -0.8578408188521842, 0.08807796531453646, -0.4332265523202068];
    public static readonly double[] CensusExpLev = [0.37777777777777644, 0.26111111111111107, 0.17777777777777715, 0.12777777777777777, 0.11111111111111112, 0.12777777777777777, 0.17777777777777715, 0.26111111111111285, 0.37777777777778];
    public static readonly double[] CensusExpCook = [1.0292936925924592, 0.011820252171853847, 0.1511477657815589, 0.08753319768357673, 0.10840755650770309, 0.03187793502435091, 0.07955576978259273, 0.0013707263713368108, 0.05697587813927373];
    public static readonly double[] CensusExpOrigFit = [66553411.41718468, 76645268.00931785, 88267407.83273014, 101651876.07618952, 117065904.20544198, 134817245.45020652, 155260319.34015882, 178803287.97038922, 205916205.27958414];
    public static readonly double[] CensusExpOrigResid = [-3605697.4171846807, -650693.0093178451, 3704858.1672698557, 4058743.923810482, 5709141.794558018, -3147970.450206518, -4562958.34015882, 519887.02961078286, -2680907.2795841396];
// SmallPower
    public static readonly double[] SmallPowerFit = [0.7206988743399818, 1.4039487380952895, 1.8036242870149837, 2.0871986018505972, 2.3071559288213934, 2.4868741507702916];
    public static readonly double[] SmallPowerResid = [0.021238470389395525, -0.04297218495968891, 0.020925005036062316, -0.03307486815505101, 0.044219328342084196, -0.010335750652807896];
    public static readonly double[] SmallPowerStand = [0.553393895229093, -1.1196919733551542, 0.5452261783587179, -0.861805477839466, 1.1521877944589902, -0.2693104168523312];
    public static readonly double[] SmallPowerStud = [1.0354011716715674, -1.28503930191702, 0.597266053349932, -0.966496649568381, 1.3640245342143569, -0.3439194320055077];
    public static readonly double[] SmallPowerLev = [0.7143386899049681, 0.24078582331800033, 0.16666861914861497, 0.20490724747063868, 0.28648652924412743, 0.38681309091365157];
    public static readonly double[] SmallPowerCook = [1.3404174033355467, 0.2618601604708749, 0.03567317533140785, 0.1203677755732827, 0.37352218148996574, 0.03730711990735911];
    public static readonly double[] SmallPowerOrigFit = [2.055869503133536, 4.071244546511283, 6.071612903647711, 8.06229779284841, 10.045812980316938, 12.023633253388311];
    public static readonly double[] SmallPowerOrigResid = [0.044130496866463975, -0.1712445465112835, 0.12838709635228884, -0.26229779284841026, 0.4541870196830615, -0.1236332533883111];
// FarFromZero (exact rational arithmetic)
    public static readonly double[] FarResid = [0.03333333333333333, -0.15833333333333333, 0.15, -0.24166666666666667, 0.26666666666666666, -0.125, 0.38333333333333336, -0.30833333333333335];
    public static readonly double[] FarLev = [0.4166666666666667, 0.27380952380952384, 0.17857142857142858, 0.13095238095238096, 0.13095238095238096, 0.17857142857142858, 0.27380952380952384, 0.4166666666666667];
    public static readonly double[] FarStud = [0.1619331455322264, -0.6893860391628892, 0.6140754858344113, -0.9618566869501143, 1.0613591028415055, -0.5117295715286762, 1.6690398842891003, -1.4978815961730942];
}
