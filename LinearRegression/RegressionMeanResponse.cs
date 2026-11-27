using MathNet.Numerics.Distributions;

namespace LinearRegression;

// Intervals for a fitted y at a given x: the mean-response (trend-line) band and the
// wider prediction interval for a single new observation. Same assumptions, degrees of
// freedom (n - 2) and invalid cases as the slope members (PValue, ConfidenceInterval).
// The formulas are in docs/MATH.md.
public partial class RegressionPvalue
{
    /// <summary>
    ///     Confidence interval for the mean response at a given x: the band around the fitted
    ///     line ŷ = a + b x (Excel-style trend-line band).
    ///     ŷ ± t(1 − α/2, n − 2) · s · √(1/n + (x − x̄)² / Sxx), with s² = RSS / (n − 2).
    /// </summary>
    /// <param name="x">The x at which to evaluate the fitted line.</param>
    /// <param name="confidenceLevel">Confidence level, strictly between 0 and 1 (default 0.95).</param>
    /// <returns>
    ///     (Fit, Lower, Upper), where Fit is the fitted ŷ at <paramref name="x"/>. All three are
    ///     <see cref="double.NaN"/> when the interval is invalid: fewer than 3 data points, any
    ///     NaN or infinite value in the data, no variation in x, a non-finite x, or a confidence
    ///     level outside (0, 1). When the residual is zero (perfect fit) the interval collapses
    ///     to (Fit, Fit, Fit). Never throws.
    /// </returns>
    /// <remarks>
    ///     This is the interval for the mean of y at x, not for a single new observation; use
    ///     <see cref="PredictionInterval"/> for that wider interval. Matches statsmodels
    ///     <c>OLS.get_prediction(x).summary_frame</c> columns <c>mean</c>, <c>mean_ci_lower</c>
    ///     and <c>mean_ci_upper</c>. Like the other residual-based results, this uses
    ///     <see cref="DataPoints"/>, so <see cref="Regression.Add(double, double)"/> and
    ///     <see cref="Regression.Dec(double, double)"/> after construction are not reflected.
    /// </remarks>
    public (double Fit, double Lower, double Upper) MeanResponseInterval(double x, double confidenceLevel = 0.95) =>
        FittedYInterval(x, confidenceLevel, includeObservationVariance: false);

    /// <summary>
    ///     Prediction interval for a single new observation at a given x.
    ///     ŷ ± t(1 − α/2, n − 2) · s · √(1 + 1/n + (x − x̄)² / Sxx), with s² = RSS / (n − 2).
    /// </summary>
    /// <param name="x">The x at which to predict a new y.</param>
    /// <param name="confidenceLevel">Confidence level, strictly between 0 and 1 (default 0.95).</param>
    /// <returns>
    ///     (Fit, Lower, Upper), where Fit is the fitted ŷ at <paramref name="x"/>. All three are
    ///     <see cref="double.NaN"/> when the interval is invalid: fewer than 3 data points, any
    ///     NaN or infinite value in the data, no variation in x, a non-finite x, or a confidence
    ///     level outside (0, 1). When the residual is zero (perfect fit) the interval collapses
    ///     to (Fit, Fit, Fit). Never throws.
    /// </returns>
    /// <remarks>
    ///     Wider than <see cref="MeanResponseInterval"/> at the same x, because it includes the
    ///     variance of a new observation (the extra 1 under the square root). Matches statsmodels
    ///     <c>OLS.get_prediction(x).summary_frame</c> columns <c>mean</c>, <c>obs_ci_lower</c>
    ///     and <c>obs_ci_upper</c>. Like the other residual-based results, this uses
    ///     <see cref="DataPoints"/>, so <see cref="Regression.Add(double, double)"/> and
    ///     <see cref="Regression.Dec(double, double)"/> after construction are not reflected.
    /// </remarks>
    public (double Fit, double Lower, double Upper) PredictionInterval(double x, double confidenceLevel = 0.95) =>
        FittedYInterval(x, confidenceLevel, includeObservationVariance: true);

    /// <summary>
    ///     Shared calculation for <see cref="MeanResponseInterval"/> and <see cref="PredictionInterval"/>.
    ///     Fit is mean(y) + slope · (x − mean(x)), the same value as YIntercept + Slope · x, but
    ///     without cancellation when x is far from zero.
    /// </summary>
    private (double Fit, double Lower, double Upper) FittedYInterval(
        double x, double confidenceLevel, bool includeObservationVariance)
    {
        if (!IsSlopeTestDefined() || !double.IsFinite(x) || !(confidenceLevel > 0 && confidenceLevel < 1))
            return (double.NaN, double.NaN, double.NaN);

        var fit = CenterY + Slope * (x - CenterX);
        if (!double.IsFinite(fit))
            return (double.NaN, double.NaN, double.NaN);

        var n = DataPoints.Count();
        var s2 = ResidualSumOfSquares / (n - 2);
        var dx = x - CenterX;
        var se = Math.Sqrt(s2 * ((includeObservationVariance ? 1.0 : 0.0) + 1.0 / n + dx * dx / SumOfSquaresX));
        if (!double.IsFinite(se))
            return (double.NaN, double.NaN, double.NaN);

        if (se == 0)
            return (fit, fit, fit); // zero residual: interval collapses to the fitted value

        double degreesOfFreedom = n - 2;
        var tCritical = new StudentT(0, 1, degreesOfFreedom)
            .InverseCumulativeDistribution(1 - (1 - confidenceLevel) / 2);
        var margin = tCritical * se;
        return (fit, fit - margin, fit + margin);
    }
}
