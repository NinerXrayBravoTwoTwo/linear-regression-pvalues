using LinearRegression;

namespace RegressionTest;

/// <summary>
///     Intercept standard error, p-value and confidence interval (<see cref="RegressionPvalue.InterceptStandardError" />,
///     <see cref="RegressionPvalue.InterceptPValue" />, <see cref="RegressionPvalue.InterceptConfidenceInterval" />).
///     References: Excel LINEST / statsmodels OLS for tiny5; mpmath at 60 digits for the generated datasets
///     (statsmodels and SciPy lose about 8 digits of the intercept on offset50, where mean x = 1e6).
/// </summary>
public class InterceptStatsTests
{
    private static readonly List<(double x, double y)> Tiny5 = [(1, 2), (2, 4), (3, 5), (4, 4), (5, 5)];

    // y = 0.3x + noise, n = 20 (System.Random seed 2).
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

    // y = 2x + 3 + 0.5 * noise, n = 30 (System.Random seed 1).
    private static readonly List<(double x, double y)> Strong30 =
    [
        (0.5803915748180694, 4.236689813259252),
        (-1.3352939917151108, 0.7681169303179591),
        (-0.28902596877490006, 2.4234405737220164),
        (0.876620458492026, 4.327304139274071),
        (-0.6103399049737027, 1.5465382631178624),
        (2.036806164396545, 6.855219036296495),
        (0.8668215588430228, 4.421003841517179),
        (1.4073035275627626, 5.682343533890949),
        (1.5445756420317325, 6.626547918579105),
        (1.8278553076279676, 6.062281667283408),
        (0.7976271493657279, 4.015250884798359),
        (-0.48219841041842476, 1.2944077944076307),
        (-0.45332875215013935, 2.3897058904475017),
        (-0.33045838674266437, 2.62017021807552),
        (2.105422354371346, 5.866212056663933),
        (1.3660805981807493, 5.7878316962304845),
        (-0.16096946103056214, 2.458833563279544),
        (2.495029689064, 8.155908332635718),
        (0.19906761108446894, 3.9073198541252605),
        (0.15424706565413618, 4.180366195521232),
        (-0.5016117735012225, 1.8201138569729438),
        (-0.7162591716102867, 2.047697236759551),
        (0.7186050485366887, 4.583398197039449),
        (0.42548480384192805, 4.007784387292211),
        (-2.0089233124271906, -0.7141737648492401),
        (-0.35839244300312456, 1.8980701027480287),
        (-0.4401993172173737, 2.2128109547681176),
        (-0.7244060328004294, 1.9282459927331943),
        (0.3226528934672192, 2.9564714335995226),
        (-1.673309754427866, -0.6542652179302193)
    ];

    // x = 1e6 + 0.01 * noise, y = 5(x - 1e6) + 100 + 0.05 * noise, n = 50 (System.Random seed 3).
    internal static readonly List<(double x, double y)> Offset50 =
    [
        (999999.9973037917, 100.0183405193206),
        (1000000.0054174113, 100.06296422438186),
        (999999.9932510661, 99.98780555515017),
        (1000000.0151328976, 100.09269973140938),
        (1000000.0031550784, 99.93244085896043),
        (999999.9939819372, 100.02129935687054),
        (999999.9692567439, 99.78881496605874),
        (999999.9902986175, 99.95318633857669),
        (1000000.0108974497, 100.11437142865623),
        (999999.989257275, 99.89390943962135),
        (999999.9826964068, 99.84143116519101),
        (999999.9893201266, 99.89955894439221),
        (999999.9920510427, 100.03634563930825),
        (1000000.0145732786, 100.05041451893663),
        (1000000.0014242331, 100.03389258030168),
        (999999.9957496339, 100.01752293916626),
        (999999.9997732508, 99.96986282742087),
        (1000000.0034045811, 100.00109171982908),
        (1000000.0294222124, 100.10974788156584),
        (1000000.0125331426, 100.00001543997158),
        (1000000.0045346041, 100.02381059751443),
        (1000000.0026813934, 99.96875393770046),
        (1000000.0078441256, 100.03010934059392),
        (1000000.0000994753, 100.12639987464532),
        (1000000.0274330868, 100.14474299804306),
        (999999.9992233353, 99.9959629755007),
        (1000000.0067000225, 100.0278096926345),
        (1000000.007294376, 99.987631912936),
        (999999.9960660742, 99.97717963014327),
        (1000000.0052893516, 100.04222327338604),
        (1000000.014967075, 100.04472971436927),
        (999999.9935763713, 99.96923836854195),
        (999999.9956834891, 99.9854968730625),
        (999999.9939227487, 99.93317391993925),
        (1000000.0119029436, 100.03269155966416),
        (999999.999671409, 100.08501335745983),
        (999999.99852138, 99.9369657395146),
        (999999.9915379747, 99.98844555206746),
        (999999.9898274638, 99.95586953683217),
        (1000000.003213699, 99.94052255146282),
        (1000000.0060970811, 100.0759972696078),
        (1000000.0035891727, 100.07838877125138),
        (999999.9953360644, 99.97178756876164),
        (999999.9924688188, 100.03261468735131),
        (1000000.0036656886, 100.05282149390179),
        (1000000.0041270382, 100.02746375420016),
        (999999.9966379412, 99.9455458344709),
        (1000000.0078732478, 100.04549492328648),
        (1000000.0044112453, 100.01192319799972),
        (1000000.0043639281, 100.05647453345982)
    ];

    // (intercept, SE, p, 95% lower, 95% upper); intercept/SE/p from mpmath, t critical from scipy.stats.t.ppf.
    private static readonly Dictionary<string, (double A, double Se, double P, double Lo, double Hi)> Reference = new()
    {
        ["weak20"] = (0.3060469040664712, 0.27813107031740963, 0.28566983898870063, -0.27828479163920466, 0.8903785997721472),
        ["strong30"] = (2.9787311095841518, 0.08549633281887638, 1.3252924739267544e-24, 2.8035998108406623, 3.153862408327641),
        ["offset50"] = (-5074396.861544821, 632823.4163298304, 2.0637184244488735e-10, -6346773.617856088, -3802020.1052335547),
    };

    private static List<(double x, double y)> Data(string name) => name switch
    {
        "weak20" => Weak20,
        "strong30" => Strong30,
        "offset50" => Offset50,
        _ => throw new ArgumentException(name)
    };

    private static void AssertRelative(double expected, double actual, double relTol)
    {
        Assert.False(double.IsNaN(actual), "unexpected NaN");
        Assert.True(Math.Abs(actual - expected) <= relTol * Math.Abs(expected),
            $"expected {expected:R}, got {actual:R}");
    }

    [Fact]
    public void Tiny5_MatchesExcelLinestAndStatsmodels()
    {
        var r = new RegressionPvalue(Tiny5);

        Assert.Equal(2.2, r.YIntercept, 12);
        AssertRelative(0.9380831519646859, r.InterceptStandardError, 1e-12); // sqrt(0.88), LINEST row 2 col 2
        AssertRelative(2.3452078799117149, r.YIntercept / r.InterceptStandardError, 1e-12);
        AssertRelative(0.10074345608541982, r.InterceptPValue, 1e-9);

        var (lo, hi) = r.InterceptConfidenceInterval();
        AssertRelative(-0.7853992610189073, lo, 1e-9);
        AssertRelative(5.185399261018908, hi, 1e-9);

        var (lo90, hi90) = r.InterceptConfidenceInterval(0.90);
        AssertRelative(-0.007650588637332145, lo90, 1e-7);
        AssertRelative(4.407650588637333, hi90, 1e-9);
    }

    [Theory]
    [InlineData("weak20", 1e-9)]
    [InlineData("strong30", 1e-9)]
    [InlineData("offset50", 1e-7)] // mean x = 1e6: the intercept is a long extrapolation, see docs/MATH.md
    public void GeneratedDatasets_MatchHighPrecisionReference(string name, double relTol)
    {
        var r = new RegressionPvalue(Data(name));
        var e = Reference[name];

        AssertRelative(e.A, r.YIntercept, relTol);
        AssertRelative(e.Se, r.InterceptStandardError, relTol);
        AssertRelative(e.P, r.InterceptPValue, relTol * 100);
        var (lo, hi) = r.InterceptConfidenceInterval();
        AssertRelative(e.Lo, lo, relTol);
        AssertRelative(e.Hi, hi, relTol);
    }

    [Fact]
    public void SecondForm_SlopeSeTimesSqrt_Agrees()
    {
        // SE_a = SE_b * sqrt(Sxx / n + mean(x)^2)
        var r = new RegressionPvalue(Tiny5);
        var seB = r.ConfidenceIntervalPlus().StandardError;
        var sxx = r.VarianceX * (r.N - 1);
        AssertRelative(seB * Math.Sqrt(sxx / r.N + r.MeanX * r.MeanX), r.InterceptStandardError, 1e-12);
    }

    [Fact]
    public void ShiftingXToZeroMean_InterceptSeEqualsSeOfMeanY()
    {
        // With mean x = 0 the intercept is mean y, so SE_a = s / sqrt(n).
        var centred = Tiny5.Select(p => (p.x - 3.0, p.y)).ToList();
        var r = new RegressionPvalue(centred);
        Assert.Equal(4.0, r.YIntercept, 12);
        AssertRelative(Math.Sqrt(0.8 / 5), r.InterceptStandardError, 1e-12);
    }

    public static TheoryData<List<(double x, double y)>> InvalidData => new()
    {
        new List<(double x, double y)>(),
        new List<(double x, double y)> { (1, 2), (2, 3) },                       // n < 3
        new List<(double x, double y)> { (2, 1), (2, 2), (2, 3) },               // no variation in x
        new List<(double x, double y)> { (1, 1), (2, double.NaN), (3, 3) },      // NaN
        new List<(double x, double y)> { (1, 1), (2, 2), (double.PositiveInfinity, 3) } // infinity
    };

    [Theory]
    [MemberData(nameof(InvalidData))]
    public void InvalidData_AllNaN_NeverThrows(List<(double x, double y)> data)
    {
        var r = new RegressionPvalue(data);
        Assert.True(double.IsNaN(r.InterceptStandardError));
        Assert.True(double.IsNaN(r.InterceptPValue));
        var (lo, hi) = r.InterceptConfidenceInterval();
        Assert.True(double.IsNaN(lo) && double.IsNaN(hi));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(-0.5)]
    [InlineData(double.NaN)]
    public void InvalidConfidenceLevel_IntervalNaN(double level)
    {
        var (lo, hi) = new RegressionPvalue(Tiny5).InterceptConfidenceInterval(level);
        Assert.True(double.IsNaN(lo) && double.IsNaN(hi));
    }

    [Fact]
    public void PerfectFit_NonZeroIntercept_PZeroAndCollapsedInterval()
    {
        var r = new RegressionPvalue([(1, 5), (2, 7), (3, 9), (4, 11)]); // y = 2x + 3
        Assert.Equal(0.0, r.InterceptStandardError, 12);
        Assert.Equal(0.0, r.InterceptPValue);
        var (lo, hi) = r.InterceptConfidenceInterval();
        Assert.Equal(3.0, lo, 12);
        Assert.Equal(3.0, hi, 12);
    }

    [Fact]
    public void PerfectFit_ZeroIntercept_PValueNaN()
    {
        var r = new RegressionPvalue([(1, 2), (2, 4), (3, 6), (4, 8)]); // y = 2x, t = 0/0
        Assert.True(double.IsNaN(r.InterceptPValue));
    }

    [Theory]
    [InlineData("weak20")]
    [InlineData("strong30")]
    [InlineData("offset50")]
    public void MergeAndDec_StreamingState_GiveSameInterceptSe(string name)
    {
        // The intercept SE depends on n, mean x, Sxx and RSS = Syy - Cxy^2 / Sxx. Rebuild it from the public
        // properties of a merged (and of an add-then-Dec) streaming Regression and compare with the single pass.
        var data = Data(name);
        var single = new RegressionPvalue(data);
        var half = data.Count / 2;
        var merged = new Regression(data.Take(half)).Merge(new Regression(data.Skip(half)));

        // Extra point within the data's range (removing a far outlier from x ~ 1e6 data loses digits in any
        // subtract-style update, so that is not a fair equality test).
        var (ex, ey) = (data[0].x + 0.5 * (data[1].x - data[0].x), data[0].y);
        var withExtra = new Regression(data);
        withExtra.Add(ex, ey);
        withExtra.Dec(ex, ey);

        foreach (var m in new[] { merged, withExtra })
        {
            Assert.Equal(single.N, m.N);
            AssertRelative(single.YIntercept, m.YIntercept, 1e-9);
            var n = m.N;
            var sxx = m.VarianceX * (n - 1);
            var rss = (n - 1) * (m.VarianceY - m.Slope * m.Slope * m.VarianceX);
            var se = Math.Sqrt(rss / (n - 2) * (1 / n + m.MeanX * m.MeanX / sxx));
            AssertRelative(single.InterceptStandardError, se, 1e-6);
        }
    }

    [Fact]
    public void PointOrder_DoesNotChangeInterceptStats()
    {
        var a = new RegressionPvalue(Offset50);
        var b = new RegressionPvalue(Enumerable.Reverse(Offset50).ToList());
        AssertRelative(a.InterceptStandardError, b.InterceptStandardError, 1e-9);
        AssertRelative(a.InterceptPValue, b.InterceptPValue, 1e-7);
    }
}
