using MathNet.Numerics.Distributions;

namespace LinearRegression;

/// <summary>
///     Inference for the y-intercept: standard error, two-tailed p-value and confidence interval.
///     Same assumptions, degrees of freedom (n - 2) and invalid cases as the slope members
///     (<see cref="PValue" />, <see cref="ConfidenceInterval" />). The formulas are in docs/MATH.md.
/// </summary>
public partial class RegressionPvalue
{
    /// <summary>
    ///     Standard error of the y-intercept: sqrt( s^2 * (1/n + mean(x)^2 / Sxx) ), with s^2 = RSS / (n - 2) and
    ///     Sxx = sum((x - mean x)^2). Equals Excel LINEST(y, x, TRUE, TRUE) row 2, column 2.
    /// </summary>
    /// <value>
    ///     A value &gt;= 0, or <see cref="double.NaN" /> when invalid: fewer than 3 data points, any NaN or
    ///     infinite value, or no variation in x. Zero for a perfect fit (zero residual). Never throws.
    ///     The intercept is an extrapolation to x = 0, so this grows as mean(x) moves away from 0.
    /// </value>
    public double InterceptStandardError
    {
        get
        {
            if (!IsSlopeTestDefined())
                return double.NaN;

            var n = DataPoints.Count();
            var sxx = SumOfSquaresX;
            var meanX = CenterX;
            var s2 = ResidualSumOfSquares / (n - 2);
            var se = Math.Sqrt(s2 * (1.0 / n + meanX * meanX / sxx));
            return double.IsFinite(se) ? se : double.NaN;
        }
    }

    /// <summary>
    ///     Two-tailed p-value for the null hypothesis intercept = 0 (Student t, n - 2 degrees of freedom):
    ///     p = 2 * CDF(-|t|) with t = <see cref="Regression.YIntercept" /> / <see cref="InterceptStandardError" />.
    /// </summary>
    /// <value>
    ///     <para>A value in [0, 1], or <see cref="double.NaN" /> when invalid: fewer than 3 data points, any NaN
    ///     or infinite value, no variation in x, or a zero standard error with a zero intercept (t = 0/0).</para>
    ///     <para>A perfect fit (zero residual) with a non-zero intercept returns 0. Never returns 1 or infinity as
    ///     a stand-in and never throws.</para>
    /// </value>
    public double InterceptPValue
    {
        get
        {
            var se = InterceptStandardError;
            if (double.IsNaN(se))
                return double.NaN;

            var intercept = YIntercept;
            if (!double.IsFinite(intercept))
                return double.NaN;

            if (se == 0)
                return intercept == 0 ? double.NaN : 0.0; // perfect fit: p = 0 is valid unless t = 0/0

            var t = intercept / se;
            if (!double.IsFinite(t))
                return double.NaN;

            double degreesOfFreedom = DataPoints.Count() - 2;
            var p = 2 * new StudentT(0, 1, degreesOfFreedom).CumulativeDistribution(-Math.Abs(t));
            return double.IsNaN(p) ? double.NaN : Math.Clamp(p, 0.0, 1.0);
        }
    }

    /// <summary>
    ///     Confidence interval for the y-intercept: intercept ± t(1 - alpha/2, n - 2) * <see cref="InterceptStandardError" />.
    /// </summary>
    /// <param name="confidenceLevel">Confidence level, strictly between 0 and 1 (default 0.95).</param>
    /// <returns>
    ///     (Lower, Upper). Both are <see cref="double.NaN" /> when the interval is invalid: fewer than 3 data
    ///     points, any NaN or infinite value, no variation in x, or a confidence level outside (0, 1).
    ///     When the residual is zero (perfect fit) the interval collapses to (YIntercept, YIntercept).
    ///     Never throws.
    /// </returns>
    public (double Lower, double Upper) InterceptConfidenceInterval(double confidenceLevel = 0.95)
    {
        var se = InterceptStandardError;
        var intercept = YIntercept;
        if (double.IsNaN(se) || !double.IsFinite(intercept) || !(confidenceLevel > 0 && confidenceLevel < 1))
            return (double.NaN, double.NaN);

        if (se == 0)
            return (intercept, intercept);

        double degreesOfFreedom = DataPoints.Count() - 2;
        var tCritical = new StudentT(0, 1, degreesOfFreedom).InverseCumulativeDistribution(1 - (1 - confidenceLevel) / 2);
        var margin = tCritical * se;
        return (intercept - margin, intercept + margin);
    }
}
