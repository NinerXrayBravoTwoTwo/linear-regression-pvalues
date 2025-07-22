using MathNet.Numerics.Distributions;

namespace LinearRegression;

public partial class RegressionPvalue
{
    /// <summary>
    /// Calculates the confidence interval for the mean of X or Y values.
    /// </summary>
    /// <param name="useY">If true, computes CI for the mean of Y (response variable); if false, for the mean of X (predictor variable).</param>
    /// <param name="confidenceLevel">The confidence level for the interval, between 0 and 1 (default: 0.95 for 95% CI).</param>
    /// <returns>A tuple containing the mean and margin of error for reporting like 'Mean ± MOE'.</returns>
    /// <exception cref="InvalidOperationException">Thrown if there are fewer than 2 data points.</exception>
    public (double Mean, double MarginOfError) MarginOfError(bool useY = false, double confidenceLevel = 0.95)
    {
        if (DataPoints.Count < 2)
            throw new InvalidOperationException("At least 2 data points are required to compute the mean CI.");

        if (_isDataContainsNan || confidenceLevel <= 0 || confidenceLevel >= 1)
            return (double.NaN, double.NaN);

        // Calculate mean and standard deviation
        var mean = useY ? MeanY() : MeanX();
        var stdDev = useY ? Qy() : Qx();

        // Calculate standard error
        var seMean = stdDev / Math.Sqrt(N);

        // Handle zero standard deviation
        if (stdDev == 0)
            return (mean, 0.0); // CI collapses to the mean

        // Get critical t-value
        var degreesOfFreedom = N - 1;
        var tDistribution = new StudentT(0, 1, degreesOfFreedom);
        var alpha = 1 - confidenceLevel;
        var tCritical = tDistribution.InverseCumulativeDistribution(1 - alpha / 2);

        // Calculate margin of error
        var marginOfError = tCritical * seMean;

        return (mean, marginOfError);
    }
}