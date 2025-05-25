
using MathNet.Numerics.Distributions;
using StatMath; // Ensure you have MathNet.Numerics installed for statistical functions


namespace LinearRegression;

public class PValueStat : Regression
{
    internal List<(double x, double y)> dataPoints = new();
    bool bDataContainsNan = false;

    public int DataPointsCount()
    {
        return dataPoints.Count;
    }

    #region Add methods with data points
    // Changed the method name to avoid conflict with the base class method
    public void AddDataPoint(double x, double y)
    {
        dataPoints.Add((x, y));
        Add(x, y);
        if(double.IsNaN(x) || double.IsNaN(y))
        {
            bDataContainsNan = true; // Track if any data point is NaN
        }

    }

    public void AddDataPoint(double x)
    {
        double y = N + 1;
        Add(x, y);       
    }
    #endregion

    /// <summary>
    ///  P-Value for the linear regression slope.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public double PValue()
    {
        if (dataPoints.Count < 3)
            throw new InvalidOperationException("At least 3 data points are required to compute the p-value.");

        if (bDataContainsNan)
            return double.NaN; // If data contains NaN, return NaN for P-Value

        // Calculate slope and standard error of the slope
        double slope = Slope();
        double varianceX = VarianceX();
        if (varianceX == 0)
            return 1.0; // No variation in X, slope is undefined or always zero, so p-value is 1

        double rss = ResidualSumOfSquares();
        int n = dataPoints.Count;
        double seSlope = Math.Sqrt(rss / (N - 2) / varianceX);

        if (seSlope == 0)
            return slope == 0 ? 1.0 : 0.0; // If standard error is zero, p-value is 1 if slope is zero, else 0

        // Calculate t-statistic
        double t = slope / seSlope;

        // Calculate p-value from t-distribution (two-tailed test)
        double degreesOfFreedom = n - 2;
        double pValue = 2 * (1 - TDistributionCdf(Math.Abs(t), degreesOfFreedom));

        if (double.IsNaN(pValue) || pValue < 0 || pValue > 1 || double.IsInfinity(t))
            return 1.0; // Defensive: return 1 for degenerate cases

        return pValue;
    }

    private double ResidualSumOfSquares()
    {
        double rss = 0;
        double slope = Slope();
        double intercept = YIntercept();

        // Calculate RSS: sum of (y - predicted_y)^2
        foreach (var (x, y) in dataPoints) // Assume you have a collection of data points
        {
            double predictedY = slope * x + intercept;
            rss += Math.Pow(y - predictedY, 2);
        }

        return rss;
    }

    private double VarianceX()
    {
        return Sx2 / N - Math.Pow(Sx / N, 2);
    }

    /// <summary>
    /// CDF of the t-distribution (for calculating p-value).
    /// You can replace this with a library function if available.
    /// </summary>
    private double TDistributionCdf(double t, double degreesOfFreedom)
    {
        // Create a t-distribution with specified degrees of freedom
        var tDistribution = new StudentT(0, 1, degreesOfFreedom);

        // Compute the CDF at t
        return tDistribution.CumulativeDistribution(t);
    }
}
