using MathNet.Numerics.Distributions;

namespace LinearRegression;

public partial class RegressionPvalue
{
    /// <summary>
    /// Calculates the 95% confidence interval for the linear regression slope.
    /// </summary>
    /// <returns>A tuple containing the lower and upper bounds of the 95% CI.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there are insufficient data points.</exception>
    public (double Lower, double Upper) ConfidenceInterval(double confidenceLevel = 0.95)
    {
        if (DataPoints.Count < 3)
            throw new InvalidOperationException(
                "At least 3 data points are required to compute the confidence interval.");

        if (_isDataContainsNan)
            return (double.NaN, double.NaN); // Return NaN for invalid data

        // Calculate slope and standard error (same as in PValue)
        var slope = Slope();
        var varianceX = Qx2();
        if (varianceX == 0)
            return (double.NaN, double.NaN); // No variation in X, CI is undefined

        var rss = ResidualSumOfSquares();
        var n = DataPoints.Count;
        var seSlope = Math.Sqrt(rss / (n - 2) / varianceX);

        if (seSlope == 0)
            return (slope, slope); // If SE is zero, CI collapses to the slope

        // Get critical t-value for the confidence level (two-tailed)
        var degreesOfFreedom = n - 2;
        var tDistribution = new StudentT(0, 1, degreesOfFreedom);
        var alpha = 1 - confidenceLevel;
        var tCritical = tDistribution.InverseCumulativeDistribution(1 - alpha / 2); // PPF for two-tailed CI

        // Calculate margin of error and CI bounds
        var marginOfError = tCritical * seSlope;
        var lowerBound = slope - marginOfError;
        var upperBound = slope + marginOfError;

        return (lowerBound, upperBound);
    }

    /// <summary>
    /// Calculates the 95% confidence interval for the linear regression slope, along with additional statistics.
    ///  This method extends the basic confidence interval calculation to include the slope, standard error, and p-value.
    ///  More computationally expensive than the basic ConfidenceInterval method.
    /// </summary>
    /// <param name="confidenceLevel">The confidence level for the interval (default: 0.95).</param>
    /// <returns>A tuple containing the lower bound, upper bound, slope, standard error, and p-value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there are insufficient data points.</exception>
    public (double Lower, double Upper, double Slope, double StandardError, double PValue) ConfidenceIntervalPlus(
        double confidenceLevel = 0.95)
    {
        if (DataPoints.Count < 3)
            throw new InvalidOperationException(
                "At least 3 data points are required to compute the confidence interval.");

        if (_isDataContainsNan)
            return (double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);

        // Calculate slope, p-value, and standard error (reuse existing logic)
        var slope = Slope();
        var varianceX = Qx2();
        if (varianceX == 0)
            return (double.NaN, double.NaN, slope, double.NaN, 1.0);

        var rss = ResidualSumOfSquares();
        var n = DataPoints.Count;
        var seSlope = Math.Sqrt(rss / (n - 2) / varianceX);

        if (seSlope == 0)
            return (slope, slope, slope, 0.0, slope == 0 ? 1.0 : 0.0);

        // Calculate p-value (using existing method for consistency)
        var pValue = PValue();

        // Get critical t-value for the confidence level
        var degreesOfFreedom = n - 2;
        var tDistribution = new StudentT(0, 1, degreesOfFreedom);
        var alpha = 1 - confidenceLevel;
        var tCritical = tDistribution.InverseCumulativeDistribution(1 - alpha / 2);

        // Calculate CI bounds
        var marginOfError = tCritical * seSlope;
        var lowerBound = slope - marginOfError;
        var upperBound = slope + marginOfError;

        return (lowerBound, upperBound, slope, seSlope, pValue);
    }
}