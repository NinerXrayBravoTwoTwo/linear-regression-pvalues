namespace LinearRegression;

// Curve-fit constructors and back-transformed intervals for RegressionPvalue. See RegressionCurve.cs and
// docs/MATH.md, "Curve fits".
public partial class RegressionPvalue
{
    /// <summary>
    ///     Initialize an empty instance that fits <paramref name="curve" />. Like <see cref="RegressionPvalue()" />,
    ///     it has no stored points, so the tests are NaN; <see cref="Regression.Add(double, double)" /> (raw x, y)
    ///     updates only the running sums (<see cref="Regression.Slope" />, <see cref="Regression.CurveB" />,
    ///     <see cref="Regression.PredictY" /> and so on).
    /// </summary>
    /// <param name="curve">Linear, Exponential, Power or Logarithmic.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="curve" /> is <see cref="CurveFit.Custom" />
    ///     or undefined.</exception>
    public RegressionPvalue(CurveFit curve) : this(CurveTransform.For(curve))
    {
    }

    /// <summary>
    ///     Initialize an empty instance that applies <paramref name="transform" />. See
    ///     <see cref="RegressionPvalue(CurveFit)" />.
    /// </summary>
    /// <param name="transform">The transforms to apply (built-in or custom).</param>
    public RegressionPvalue(CurveTransform transform) : base(transform)
    {
        DataPoints = [];
        IdPoints = [];
    }

    /// <summary>
    ///     Initialize an instance that fits <paramref name="curve" /> to the raw (x, y) points.
    ///     <see cref="DataPoints" /> holds the <b>transformed</b> points (for example (x, ln y) for an exponential
    ///     fit), so the residuals, p-values, standard errors and intervals all describe the same linearized fit.
    /// </summary>
    /// <param name="dataPoints">Raw (x, y) pairs, in original units.</param>
    /// <param name="curve">Linear, Exponential, Power or Logarithmic. Linear gives the same results as
    ///     <see cref="RegressionPvalue(List{ValueTuple{double, double}})" />.</param>
    /// <remarks>A point outside the transform's domain (ln of a value ≤ 0) becomes NaN, so the tests return NaN,
    ///     the same as NaN in the data.</remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="curve" /> is <see cref="CurveFit.Custom" />
    ///     or undefined.</exception>
    public RegressionPvalue(List<(double x, double y)> dataPoints, CurveFit curve)
        : this(dataPoints, CurveTransform.For(curve))
    {
    }

    /// <summary>
    ///     Initialize an instance that applies <paramref name="transform" /> to each raw (x, y) point. See
    ///     <see cref="RegressionPvalue(List{ValueTuple{double, double}}, CurveFit)" />.
    /// </summary>
    /// <param name="dataPoints">Raw (x, y) pairs, in original units.</param>
    /// <param name="transform">The transforms to apply (built-in or custom).</param>
    public RegressionPvalue(List<(double x, double y)> dataPoints, CurveTransform transform) : base(transform)
    {
        var points = dataPoints.Select(p => transform.Apply(p.x, p.y)).ToArray();
        foreach (var (x, y) in dataPoints) Add(x, y); // Add applies the transform
        DataPoints = points;
        IdPoints = [];
        IsDataContainsNan = points.Any(p => double.IsNaN(p.x) || double.IsNaN(p.y));
        IsDataContainsInf = points.Any(p => double.IsInfinity(p.x) || double.IsInfinity(p.y));
    }

    /// <summary>
    ///     Initialize an instance with ids that fits <paramref name="curve" /> to the raw (x, y) points. See
    ///     <see cref="RegressionPvalue(List{ValueTuple{double, double}}, CurveFit)" />.
    /// </summary>
    /// <param name="dataPoints">(id, x, y) triples, x and y in original units. Ids need not be unique.</param>
    /// <param name="curve">Linear, Exponential, Power or Logarithmic.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="curve" /> is <see cref="CurveFit.Custom" />
    ///     or undefined.</exception>
    public RegressionPvalue(List<(string id, double x, double y)> dataPoints, CurveFit curve)
        : this(dataPoints, CurveTransform.For(curve))
    {
    }

    /// <summary>
    ///     Initialize an instance with ids that applies <paramref name="transform" /> to each raw (x, y) point.
    ///     See <see cref="RegressionPvalue(List{ValueTuple{double, double}}, CurveFit)" />.
    /// </summary>
    /// <param name="dataPoints">(id, x, y) triples, x and y in original units. Ids need not be unique.</param>
    /// <param name="transform">The transforms to apply (built-in or custom).</param>
    public RegressionPvalue(List<(string id, double x, double y)> dataPoints, CurveTransform transform)
        : this(dataPoints.Select(p => (p.x, p.y)).ToList(), transform)
    {
        IdPoints = dataPoints.Select(p => p.id).ToArray();
    }

    /// <summary>
    ///     <see cref="MeanResponseInterval" /> converted back to original units: the band around the fitted
    ///     curve at raw <paramref name="x" />. Computed on the linearized fit at f(x), then each of Fit, Lower and
    ///     Upper is passed through the inverse y transform.
    /// </summary>
    /// <param name="x">Raw x, in original units.</param>
    /// <param name="confidenceLevel">Confidence level, strictly between 0 and 1 (default 0.95).</param>
    /// <returns>
    ///     (Fit, Lower, Upper) in original units; Fit equals <see cref="Regression.PredictY" />. All NaN in the
    ///     same cases as <see cref="MeanResponseInterval" />, when x is outside the transform's domain, or when
    ///     a custom y transform has no inverse. Never throws.
    /// </returns>
    /// <remarks>
    ///     Valid because the inverse transform is monotone: the probability that the linearized band covers the
    ///     true line carries over unchanged. For the exponential and power fits the band is for the median of y
    ///     at x (e^ of the mean of ln y), not the mean, and it is not symmetric around Fit. For a decreasing
    ///     custom inverse, Lower and Upper are swapped so Lower ≤ Upper.
    /// </remarks>
    public (double Fit, double Lower, double Upper) CurveMeanResponseInterval(double x, double confidenceLevel = 0.95) =>
        BackTransform(MeanResponseInterval(Transform.X(x), confidenceLevel));

    /// <summary>
    ///     <see cref="PredictionInterval" /> converted back to original units: the interval for a single new y at
    ///     raw <paramref name="x" />. See <see cref="CurveMeanResponseInterval" /> for how it is computed.
    /// </summary>
    /// <param name="x">Raw x, in original units.</param>
    /// <param name="confidenceLevel">Confidence level, strictly between 0 and 1 (default 0.95).</param>
    /// <returns>
    ///     (Fit, Lower, Upper) in original units. All NaN in the same cases as
    ///     <see cref="CurveMeanResponseInterval" />. Never throws.
    /// </returns>
    /// <remarks>
    ///     A prediction interval back-transforms exactly under a monotone inverse: the chance that a new ln y
    ///     falls in the linearized interval equals the chance that the new y falls in the back-transformed one.
    /// </remarks>
    public (double Fit, double Lower, double Upper) CurvePredictionInterval(double x, double confidenceLevel = 0.95) =>
        BackTransform(PredictionInterval(Transform.X(x), confidenceLevel));

    private (double Fit, double Lower, double Upper) BackTransform((double Fit, double Lower, double Upper) linear)
    {
        var inverse = Transform.InverseY;
        if (inverse is null || double.IsNaN(linear.Fit))
            return (double.NaN, double.NaN, double.NaN);

        var fit = inverse(linear.Fit);
        var a = inverse(linear.Lower);
        var b = inverse(linear.Upper);
        if (!double.IsFinite(fit) || !double.IsFinite(a) || !double.IsFinite(b))
            return (double.NaN, double.NaN, double.NaN);

        return (fit, Math.Min(a, b), Math.Max(a, b));
    }
}
