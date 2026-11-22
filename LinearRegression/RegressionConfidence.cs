using MathNet.Numerics.Distributions;

namespace LinearRegression;

public partial class RegressionPvalue
{
    /// <summary>
    ///     Confidence interval for the linear regression slope.
    /// </summary>
    /// <param name="confidenceLevel">Confidence level, strictly between 0 and 1 (default 0.95).</param>
    /// <returns>
    ///     (Lower, Upper). Both are <see cref="double.NaN" /> when the interval is invalid: fewer than 3 data
    ///     points, any NaN or infinite value, no variation in x, or a confidence level outside (0, 1).
    ///     When the residual is zero (perfect fit, including constant y) the interval collapses to (Slope, Slope).
    ///     Never throws.
    /// </returns>
    public (double Lower, double Upper) ConfidenceInterval(double confidenceLevel = 0.95)
    {
        var r = ConfidenceIntervalPlus(confidenceLevel);
        return (r.Lower, r.Upper);
    }

    /// <summary>
    ///     Confidence interval for the slope together with the slope, its standard error and the p-value.
    /// </summary>
    /// <param name="confidenceLevel">Confidence level, strictly between 0 and 1 (default 0.95).</param>
    /// <returns>
    ///     (Lower, Upper, Slope, StandardError, PValue). When the slope test is invalid (fewer than 3 points,
    ///     any NaN or infinite value, no variation in x) every element is <see cref="double.NaN" />. An invalid
    ///     confidence level makes only Lower and Upper NaN. PValue is the same value as <see cref="PValue" />.
    ///     Never throws.
    /// </returns>
    public (double Lower, double Upper, double Slope, double StandardError, double PValue) ConfidenceIntervalPlus(
        double confidenceLevel = 0.95)
    {
        if (!IsSlopeTestDefined())
            return (double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);

        var slope = Slope;
        var n = DataPoints.Count();
        var seSlope = SlopeStandardError(SumOfSquaresX, n);
        var pValue = PValue;

        if (double.IsNaN(seSlope) || double.IsInfinity(seSlope))
            return (double.NaN, double.NaN, slope, double.NaN, pValue);

        if (!(confidenceLevel > 0 && confidenceLevel < 1))
            return (double.NaN, double.NaN, slope, seSlope, pValue);

        if (seSlope == 0)
            return (slope, slope, slope, 0.0, pValue); // zero residual: interval collapses to the slope

        // Critical t-value for the confidence level (two-tailed)
        var degreesOfFreedom = n - 2;
        var tDistribution = new StudentT(0, 1, degreesOfFreedom);
        var alpha = 1 - confidenceLevel;
        var tCritical = tDistribution.InverseCumulativeDistribution(1 - alpha / 2);

        var marginOfError = tCritical * seSlope;
        return (slope - marginOfError, slope + marginOfError, slope, seSlope, pValue);
    }
}
