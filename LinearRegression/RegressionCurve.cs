namespace LinearRegression;

// Curve fits after TI ST-12 (Applied Statistics, 1977): exponential, power and logarithmic curves fitted as a
// straight line on transformed points, with the results converted back to original units. See docs/MATH.md.
public partial class Regression
{
    private CurveTransform _transform = CurveTransform.Linear;

    /// <summary>
    ///     Initialize an empty regression that fits <paramref name="curve" />. Pass raw x and y to
    ///     <see cref="Add(double, double)" /> and <see cref="Dec(double, double)" />; the transform is applied there.
    /// </summary>
    /// <param name="curve">Linear, Exponential, Power or Logarithmic.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="curve" /> is <see cref="CurveFit.Custom" />
    ///     or undefined.</exception>
    public Regression(CurveFit curve) : this(CurveTransform.For(curve))
    {
    }

    /// <summary>
    ///     Initialize an empty regression that applies <paramref name="transform" /> (built-in or custom) to every
    ///     point passed to <see cref="Add(double, double)" /> and <see cref="Dec(double, double)" />.
    /// </summary>
    /// <param name="transform">The transforms to apply.</param>
    public Regression(CurveTransform transform) : this()
    {
        ArgumentNullException.ThrowIfNull(transform);
        _transform = transform;
    }

    /// <summary>
    ///     Initialize a regression that fits <paramref name="curve" /> to the raw (x, y) points.
    /// </summary>
    /// <param name="dataPoints">Raw (x, y) pairs, in original units.</param>
    /// <param name="curve">Linear, Exponential, Power or Logarithmic.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="curve" /> is <see cref="CurveFit.Custom" />
    ///     or undefined.</exception>
    public Regression(IEnumerable<(double x, double y)> dataPoints, CurveFit curve)
        : this(dataPoints, CurveTransform.For(curve))
    {
    }

    /// <summary>
    ///     Initialize a regression that applies <paramref name="transform" /> to each raw (x, y) point.
    /// </summary>
    /// <param name="dataPoints">Raw (x, y) pairs, in original units.</param>
    /// <param name="transform">The transforms to apply (built-in or custom).</param>
    public Regression(IEnumerable<(double x, double y)> dataPoints, CurveTransform transform) : this(transform)
    {
        foreach (var (x, y) in dataPoints) Add(x, y);
    }

    /// <summary>
    ///     The curve being fitted. <see cref="CurveFit.Linear" /> unless a curve constructor was used.
    /// </summary>
    /// <remarks>
    ///     With any other curve, the existing statistics (<see cref="Slope" />, <see cref="YIntercept" />,
    ///     <see cref="Correlation" />, <see cref="RSquared" />, means, variances, sums, min/max and, on
    ///     <see cref="RegressionPvalue" />, the p-values and intervals) describe the <b>linearized</b> fit on the
    ///     transformed points, for example (x, ln y) for <see cref="CurveFit.Exponential" />. Use
    ///     <see cref="CurveB" />, <see cref="CurveM" />, <see cref="PredictY" /> and <see cref="PredictX" /> for
    ///     results in original units.
    /// </remarks>
    public CurveFit Curve => _transform.Curve;

    /// <summary>
    ///     The transforms applied to each point (<see cref="CurveTransform.Linear" /> when no curve was chosen).
    /// </summary>
    public CurveTransform Transform => _transform;

    /// <summary>
    ///     The coefficient b of the curve in its original form: e^<see cref="YIntercept" /> for
    ///     <see cref="CurveFit.Exponential" /> (y = b·e^(m·x)) and <see cref="CurveFit.Power" /> (y = b·x^m);
    ///     <see cref="YIntercept" /> for <see cref="CurveFit.Linear" />, <see cref="CurveFit.Logarithmic" />
    ///     (y = b + m·ln x) and <see cref="CurveFit.Custom" /> (g(y) = b + m·f(x)).
    /// </summary>
    /// <returns>b, or <see cref="double.NaN" /> when the fit is undefined. Never throws.</returns>
    public double CurveB => Curve is CurveFit.Exponential or CurveFit.Power ? Math.Exp(YIntercept) : YIntercept;

    /// <summary>
    ///     The coefficient m of the curve: the growth rate of y = b·e^(m·x), the exponent of y = b·x^m, or the
    ///     slope of the line for the other curves. Always equal to <see cref="Slope" />.
    /// </summary>
    public double CurveM => Slope;

    /// <summary>
    ///     Predicted y at <paramref name="x" />, in original units: g⁻¹(a + m·f(x)), where a and m are the
    ///     linearized intercept and slope. For example b·e^(m·x) for an exponential fit. Like TI's x→y′ key.
    /// </summary>
    /// <param name="x">Raw x, in original units.</param>
    /// <returns>
    ///     The predicted y, or <see cref="double.NaN" /> when the fit is undefined (fewer than 2 points, no
    ///     variation in x, NaN in the data), x is outside the transform's domain (for example x ≤ 0 for a power
    ///     fit), a custom y transform has no inverse, or the result is not finite. Never throws.
    /// </returns>
    /// <remarks>
    ///     For the exponential and power fits this is the back-transform of a log-space least-squares fit, which
    ///     is not the least-squares prediction in y (see docs/MATH.md).
    /// </remarks>
    public double PredictY(double x)
    {
        var inverse = _transform.InverseY;
        if (inverse is null) return double.NaN;
        var linear = PredictLinearY(_transform.X(x));
        return Finite(inverse(linear));
    }

    /// <summary>
    ///     The x at which the fitted curve reaches <paramref name="y" />, in original units:
    ///     f⁻¹(x̄′ + (g(y) − ȳ′) / m). Like TI's y→x′ key.
    /// </summary>
    /// <param name="y">Raw y, in original units.</param>
    /// <returns>
    ///     The x, or <see cref="double.NaN" /> when the fit is undefined, the slope is 0, y is outside the
    ///     transform's domain, a custom x transform has no inverse, or the result is not finite. Never throws.
    /// </returns>
    public double PredictX(double y)
    {
        var inverse = _transform.InverseX;
        if (inverse is null) return double.NaN;
        var slope = Slope;
        if (slope == 0 || !double.IsFinite(slope)) return double.NaN;
        var linear = CenterX + (_transform.Y(y) - CenterY) / slope;
        return Finite(inverse(linear));
    }

    /// <summary>
    ///     Fitted value of the linearized line at a transformed x′: ȳ′ + m(x′ − x̄′), the same as
    ///     YIntercept + Slope·x′ without cancellation when x′ is far from zero.
    /// </summary>
    private double PredictLinearY(double xTransformed)
    {
        var slope = Slope;
        if (!double.IsFinite(slope) || !double.IsFinite(xTransformed)) return double.NaN;
        return CenterY + slope * (xTransformed - CenterX);
    }

    private static double Finite(double v) => double.IsFinite(v) ? v : double.NaN;
}
