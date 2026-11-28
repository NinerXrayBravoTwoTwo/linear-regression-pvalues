namespace LinearRegression;

// Residuals and regression diagnostics (leverage, studentized residuals, Cook's distance) for each stored point.
// Same data (DataPoints), degrees of freedom (n - 2) and invalid cases as the slope members (PValue,
// ConfidenceInterval, MeanResponseInterval). The formulas are in docs/MATH.md.
public partial class RegressionPvalue
{
    // Leverage within this of 1 is treated as exactly 1 (the point alone determines the line): rounding can put
    // a mathematically exact 1 a few ulps either side, and 1 - h would then be noise.
    private const double LeverageOneTolerance = 1e-12;

    /// <summary>
    ///     Residuals and diagnostics for every point in <see cref="DataPoints" />, in the same order, ready for a
    ///     residual-versus-x plot and for flagging outliers and influential points.
    /// </summary>
    /// <returns>
    ///     One <see cref="ResidualPoint" /> per stored point. With a defined fit (at least 3 points, all finite,
    ///     x varies; the same condition as <see cref="PValue" />) every field is computed, except:
    ///     <list type="bullet">
    ///         <item>perfect fit (RSS = 0, so s = 0): Residual is 0 but Standardized, Studentized and
    ///         <see cref="ResidualPoint.CooksDistance" /> are NaN (0/0);</item>
    ///         <item>leverage of 1 (one point alone fixes the line): Studentized and Cook's distance are NaN.</item>
    ///     </list>
    ///     When the fit is not defined (fewer than 3 points, any NaN or infinite value in the data, or all x equal)
    ///     the list still has one entry per point, with X and Y as stored and every other field NaN. An instance with
    ///     no stored points returns an empty list. Never throws.
    /// </returns>
    /// <remarks>
    ///     <para>Fit is ȳ + m·(x − x̄), the same value as <see cref="MeanResponseInterval" />, and the residual is
    ///     computed from centred values so that the squared residuals add up exactly to the RSS behind the p-value.</para>
    ///     <para>Like the other residual-based results this uses <see cref="DataPoints" /> together with the
    ///     fitted line, so <see cref="Regression.Add(double, double)" /> and <see cref="Regression.Dec(double, double)" />
    ///     after construction (which change the running sums but not <see cref="DataPoints" />) make it
    ///     inconsistent: build a new instance instead. For a curve fit, <see cref="DataPoints" /> holds the transformed points,
    ///     so these are residuals of the linearized fit (for example ln y for an exponential fit); use
    ///     <see cref="CurveResiduals" /> for residuals in original units.</para>
    ///     <para>Matches statsmodels <c>OLS.resid</c> and <c>get_influence()</c>: <c>resid_studentized_internal</c>,
    ///     <c>hat_matrix_diag</c> and <c>cooks_distance[0]</c>.</para>
    /// </remarks>
    public IReadOnlyList<ResidualPoint> Residuals()
    {
        var points = DataPoints.ToArray();
        var result = new ResidualPoint[points.Length];
        var undefined = !IsSlopeTestDefined();
        var n = points.Length;

        double slope = 0, centerX = 0, centerY = 0, sxx = 0, s = 0;
        if (!undefined)
        {
            slope = Slope;
            centerX = CenterX;
            centerY = CenterY;
            sxx = SumOfSquaresX;
            s = Math.Sqrt(ResidualSumOfSquares / (n - 2));
            undefined = !double.IsFinite(slope) || !double.IsFinite(s);
        }

        for (var i = 0; i < n; i++)
        {
            var (x, y) = points[i];
            if (undefined)
            {
                result[i] = new ResidualPoint(x, y, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);
                continue;
            }

            var dx = DeviationX(x);
            var fit = centerY + slope * (x - centerX);
            var residual = DeviationY(y) - slope * dx; // same value as y - fit, without cancellation far from zero
            var leverage = 1.0 / n + dx * dx / sxx;
            if (leverage > 1 - LeverageOneTolerance)
                leverage = leverage < 1 + LeverageOneTolerance ? 1.0 : double.NaN;

            var standardized = double.NaN;
            var studentized = double.NaN;
            var cooks = double.NaN;
            if (s > 0)
            {
                standardized = residual / s;
                if (leverage < 1) // NaN leverage fails this test too
                {
                    studentized = residual / (s * Math.Sqrt(1 - leverage));
                    cooks = studentized * studentized * leverage / (2 * (1 - leverage));
                }
            }

            result[i] = new ResidualPoint(x, y, fit, residual, standardized, studentized, leverage, cooks);
        }

        return result;
    }

    /// <summary>
    ///     <see cref="Residuals" /> for a curve fit, with X, Y, Fit and Residual in <b>original units</b>:
    ///     X and Y are the raw point (inverse transforms of the stored x′ and y′), Fit = <see cref="Regression.PredictY" />
    ///     at that x, and Residual = y − Fit.
    /// </summary>
    /// <returns>
    ///     One <see cref="ResidualPoint" /> per stored point, in <see cref="DataPoints" /> order.
    ///     <para>Standardized, Studentized, Leverage and <see cref="ResidualPoint.CooksDistance" /> are copied unchanged
    ///     from <see cref="Residuals" />, so they describe the <b>linearized</b> fit (for example ln y against x for
    ///     an exponential fit): they are unitless, and an outlier in log space is the useful notion for a log-space
    ///     fit. They are not the standardized residuals of the original-unit Residual in this record.</para>
    ///     <para>NaN: all fields (X and Y as stored) when the fit is undefined, exactly as in <see cref="Residuals" />;
    ///     X when the transform has no <see cref="CurveTransform.InverseX" />; Y, Fit and Residual when it has no
    ///     <see cref="CurveTransform.InverseY" />; and Fit and Residual when the inverse of the fitted value is not
    ///     finite. The diagnostics in the last group are still reported. Never throws.</para>
    /// </returns>
    /// <remarks>
    ///     For the exponential and power fits the residual is y − b·e^(m·x) (or b·x^m): the back-transformed fit is
    ///     the median curve of a log-space least-squares fit, so these residuals do not sum to 0, and are larger for
    ///     larger y (see docs/MATH.md). A point whose transformed value is NaN (outside the transform's domain, for
    ///     example y ≤ 0 for an exponential fit) has already made the fit undefined. With
    ///     <see cref="CurveFit.Linear" /> the result equals <see cref="Residuals" />.
    /// </remarks>
    public IReadOnlyList<ResidualPoint> CurveResiduals()
    {
        var linear = Residuals();
        if (Curve == CurveFit.Linear) return linear; // identity transforms: original units are the linearized units
        var result = new ResidualPoint[linear.Count];
        var inverseX = Transform.InverseX;
        var inverseY = Transform.InverseY;

        for (var i = 0; i < linear.Count; i++)
        {
            var p = linear[i];
            if (double.IsNaN(p.Fit))
            {
                result[i] = p; // undefined fit: all NaN, X and Y as stored
                continue;
            }

            var x = inverseX is null ? double.NaN : Finite(inverseX(p.X));
            var y = inverseY is null ? double.NaN : Finite(inverseY(p.Y));
            var fit = inverseY is null ? double.NaN : Finite(inverseY(p.Fit)); // = PredictY(x) without the x round trip
            var residual = y - fit; // NaN if either is NaN
            result[i] = p with { X = x, Y = y, Fit = fit, Residual = residual };
        }

        return result;
    }

    private static double Finite(double v) => double.IsFinite(v) ? v : double.NaN;
}
