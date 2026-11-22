using LinearRegression;

namespace RegressionTest;

/// <summary>
///     Tiny p-values keep their precision: p is computed as 2 * CDF(-|t|), not 2 * (1 - CDF(|t|)),
///     which rounds anything below about 1e-16 to 0. Reference values: SciPy <c>scipy.stats.linregress</c>,
///     or mpmath at 50 digits where noted.
/// </summary>
public class SmallPValuePrecisionTests
{
    // y = 2x + 3 + 0.5 * noise, x ~ N(0, 1), n = 30 (System.Random seed 1). t = 25.05, df = 28.
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

    private static readonly double[] X10 = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

    private static void AssertRelative(double expected, double actual, double relTol = 1e-6)
    {
        Assert.False(double.IsNaN(actual));
        Assert.True(Math.Abs(actual - expected) <= relTol * Math.Abs(expected),
            $"expected {expected:R}, got {actual:R}");
    }

    [Fact]
    public void Strong30_TinyPValue_MatchesScipy()
    {
        var r = new RegressionPvalue(Strong30);

        Assert.Equal(1.8782653555905284, r.Slope, 9);
        Assert.True(r.PValue > 0, "p must not round to 0");
        AssertRelative(1.0294132291954983e-20, r.PValue);
    }

    [Fact]
    public void Strong30_ConfidenceIntervalPlus_ReportsSameTinyPValue()
    {
        var r = new RegressionPvalue(Strong30);
        AssertRelative(1.0294132291954983e-20, r.ConfidenceIntervalPlus().PValue);
    }

    [Fact]
    public void NearPerfectFit_VeryTinyPValue_MatchesScipy()
    {
        double[] y = [2, 4, 6, 8, 10.0001, 12, 14, 16, 18, 20];
        var r = new RegressionPvalue(X10.Zip(y).ToList());

        // Reference from mpmath at 50 digits (t = 542516.49, df = 8). SciPy's linregress gives 1.49245e-43 here,
        // off by 3e-5 relative from cancellation in its raw-sum formulas; this library's centred sums are closer.
        AssertRelative(1.4925003880152055e-43, r.PValue, 1e-8);
    }

    [Fact]
    public void FlatSlope_PValueNearOne_MatchesScipy()
    {
        double[] y = [5, 3, 6, 2, 7, 4, 4, 6, 3, 5];
        var r = new RegressionPvalue(X10.Zip(y).ToList());

        AssertRelative(0.9239332723098314, r.PValue, 1e-9);
    }

    [Fact]
    public void MidRangePValue_MatchesScipy()
    {
        double[] y = [3, 1, 4, 1, 5, 9, 2, 6, 5, 3];
        var r = new RegressionPvalue(X10.Zip(y).ToList());

        AssertRelative(0.345071047733112, r.PValue, 1e-9);
    }
}
