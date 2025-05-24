using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MetabolicStat.StatMath;

public class PValueStat : Statistic
{
    internal List<(double x, double y)> dataPoints = new();

    public int DataPointsCount() 
    {
        return dataPoints.Count;
    }

    #region Add methods with data points
    // Changed the method name to avoid conflict with the base class method
    public void AddDataPoint(double x, double y)
    {
        dataPoints.Add((x, y));
        this.Add(x, y);

    }
    #endregion

    /// <summary>
    ///  P-Value for the linear regression slope.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public double PValue()
    {
        if (N <= 2)
            throw new InvalidOperationException("P-value requires at least 3 data points.");

        // Calculate slope and standard error of the slope
        double slope = Slope();
        double varianceX = VarianceX();
        double rss = ResidualSumOfSquares();
        double seSlope = Math.Sqrt(rss / (N - 2) / varianceX);

        // Calculate t-statistic
        double tStatistic = slope / seSlope;

        // Calculate p-value from t-distribution (two-tailed test)
        double degreesOfFreedom = N - 2;
        return 2 * (1 - TDistributionCdf(Math.Abs(tStatistic), degreesOfFreedom));
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
        // Approximation for t-distribution CDF
        // Replace this with a more accurate method or library call
        throw new NotImplementedException("Use a library for t-distribution CDF or implement this function.");
    }
}
