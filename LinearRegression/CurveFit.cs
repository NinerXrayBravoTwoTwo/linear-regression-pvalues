namespace LinearRegression;

/// <summary>
///     The curve a regression fits, after the TI-58/59 <i>Applied Statistics</i> (1977) program ST-12 Bivariate
///     Data Transforms. Each built-in curve becomes a straight line after transforming x, y or both, and the usual
///     least-squares line is then fitted to the transformed points. See docs/MATH.md, "Curve fits".
/// </summary>
public enum CurveFit
{
    /// <summary>Straight line y = b + m·x. No transform; this is the default and the behavior of every
    ///     constructor that takes no curve.</summary>
    Linear = 0,

    /// <summary>Exponential y = b·e^(m·x), fitted as ln y = ln b + m·x on the points (x, ln y).
    ///     Needs y &gt; 0.</summary>
    Exponential = 1,

    /// <summary>Power y = b·x^m, fitted as ln y = ln b + m·ln x on the points (ln x, ln y).
    ///     Needs x &gt; 0 and y &gt; 0.</summary>
    Power = 2,

    /// <summary>Logarithmic y = b + m·ln x, fitted on the points (ln x, y). Needs x &gt; 0.</summary>
    Logarithmic = 3,

    /// <summary>Caller-supplied transforms (a <see cref="CurveTransform" /> built with its public
    ///     constructor), like TI's user-defined f(x) and g(y) under labels A′ and B′. Reported by
    ///     <see cref="Regression.Curve" />; not accepted by the constructors that take a <see cref="CurveFit" />.</summary>
    Custom = 4
}

/// <summary>
///     The pair of transforms x′ = f(x), y′ = g(y) applied to every point before it reaches the running sums,
///     with optional inverses used to convert predictions back to original units. Use the built-in
///     <see cref="Exponential" />, <see cref="Power" /> and <see cref="Logarithmic" /> instances (or
///     <see cref="For" />), or the public constructor for your own transforms.
/// </summary>
/// <remarks>
///     Domain: the built-in curves use ln, which is only defined for values &gt; 0. A value ≤ 0 (or NaN)
///     transforms to <see cref="double.NaN" />, so that point makes the affected statistics NaN, exactly like a
///     NaN in the data today. Nothing throws.
/// </remarks>
[Serializable]
public sealed class CurveTransform
{
    private static double Identity(double v) => v;

    /// <summary>Natural log that returns NaN (not −∞) for 0 and NaN for negative values.</summary>
    private static double Ln(double v) => v > 0 ? Math.Log(v) : double.NaN;

    private CurveTransform(CurveFit curve, Func<double, double> x, Func<double, double> y,
        Func<double, double> inverseX, Func<double, double> inverseY)
    {
        Curve = curve;
        X = x;
        Y = y;
        InverseX = inverseX;
        InverseY = inverseY;
        IsIdentity = curve == CurveFit.Linear;
    }

    /// <summary>
    ///     Custom transforms, like TI's user-defined f(x) and g(y). Each point (x, y) is entered as
    ///     (<paramref name="x" />(x), <paramref name="y" />(y)).
    /// </summary>
    /// <param name="x">Transform for x, f(x). Null means no transform (identity, with identity inverse).</param>
    /// <param name="y">Transform for y, g(y). Null means no transform (identity, with identity inverse).</param>
    /// <param name="inverseX">Inverse of <paramref name="x" />, used by <see cref="Regression.PredictX" />.
    ///     When null (and <paramref name="x" /> is not null), <see cref="Regression.PredictX" /> returns NaN.</param>
    /// <param name="inverseY">Inverse of <paramref name="y" />, used by <see cref="Regression.PredictY" /> and
    ///     the curve intervals. When null (and <paramref name="y" /> is not null), those return NaN. It must be
    ///     monotone for the back-transformed intervals to be valid.</param>
    /// <remarks>
    ///     A transform may return NaN or ±∞ for values outside its domain; that point then makes the statistics NaN.
    ///     The functions should be pure (same output for the same input), because <see cref="Regression.Dec(double, double)" />
    ///     re-applies them to remove a point.
    /// </remarks>
    public CurveTransform(Func<double, double>? x, Func<double, double>? y,
        Func<double, double>? inverseX = null, Func<double, double>? inverseY = null)
    {
        Curve = CurveFit.Custom;
        X = x ?? Identity;
        Y = y ?? Identity;
        InverseX = x is null ? Identity : inverseX;
        InverseY = y is null ? Identity : inverseY;
        IsIdentity = false;
    }

    /// <summary>No transform: y = b + m·x.</summary>
    public static CurveTransform Linear { get; } = new(CurveFit.Linear, Identity, Identity, Identity, Identity);

    /// <summary>y = b·e^(m·x): points (x, ln y); predictions back-transformed with exp.</summary>
    public static CurveTransform Exponential { get; } = new(CurveFit.Exponential, Identity, Ln, Identity, Math.Exp);

    /// <summary>y = b·x^m: points (ln x, ln y); predictions back-transformed with exp.</summary>
    public static CurveTransform Power { get; } = new(CurveFit.Power, Ln, Ln, Math.Exp, Math.Exp);

    /// <summary>y = b + m·ln x: points (ln x, y); x predictions back-transformed with exp.</summary>
    public static CurveTransform Logarithmic { get; } = new(CurveFit.Logarithmic, Ln, Identity, Math.Exp, Identity);

    /// <summary>The built-in transform for a <see cref="CurveFit" />.</summary>
    /// <param name="curve">Linear, Exponential, Power or Logarithmic.</param>
    /// <returns>The shared built-in instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="curve" /> is <see cref="CurveFit.Custom" />
    ///     or not a defined value; use the public constructor for custom transforms.</exception>
    public static CurveTransform For(CurveFit curve) => curve switch
    {
        CurveFit.Linear => Linear,
        CurveFit.Exponential => Exponential,
        CurveFit.Power => Power,
        CurveFit.Logarithmic => Logarithmic,
        _ => throw new ArgumentOutOfRangeException(nameof(curve), curve,
            "Use the CurveTransform constructor for custom transforms.")
    };

    /// <summary>Which curve this is; <see cref="CurveFit.Custom" /> for caller-supplied transforms.</summary>
    public CurveFit Curve { get; }

    /// <summary>Transform applied to x before fitting, f(x).</summary>
    public Func<double, double> X { get; }

    /// <summary>Transform applied to y before fitting, g(y).</summary>
    public Func<double, double> Y { get; }

    /// <summary>Inverse of <see cref="X" />, or null when not supplied.</summary>
    public Func<double, double>? InverseX { get; }

    /// <summary>Inverse of <see cref="Y" />, or null when not supplied.</summary>
    public Func<double, double>? InverseY { get; }

    /// <summary>True only for <see cref="Linear" />; Add and Dec then skip the transform entirely.</summary>
    internal bool IsIdentity { get; }

    /// <summary>Transform one raw point.</summary>
    /// <param name="x">Raw x.</param>
    /// <param name="y">Raw y.</param>
    /// <returns>(f(x), g(y)).</returns>
    public (double x, double y) Apply(double x, double y) => (X(x), Y(y));
}
