using MathNet.Numerics.Distributions;

namespace LinearRegression;

[Serializable]
public partial class RegressionPvalue : Regression
{
    private bool _isDataContainsNan;
    public IEnumerable<(double x, double y)> DataPoints { get; init; }
    public IEnumerable<string> IdPoints { get; init; }

    public RegressionPvalue()
    {
        DataPoints = [];
        IdPoints = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegressionPvalue"/> class with the specified data points.
    /// </summary>
    /// <remarks>If any of the provided data points contain <see cref="double.NaN"/> for either the X or Y
    /// value, the instance will mark the data as containing invalid values.</remarks>
    /// <param name="dataPoints">A list of tuples representing the data points for regression analysis, where each tuple contains an X value and
    /// a Y value.</param>
    public RegressionPvalue(List<(double x, double y)> dataPoints) : base(dataPoints.Select(tuple => (tuple.x, tuple.y)))
    {
        DataPoints = dataPoints.Select(tuple => (tuple.x, tuple.y));
        IdPoints = [];
        _isDataContainsNan = dataPoints.Any(item => item.x is double.NaN || item.y is double.NaN);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegressionPvalue"/> class with the specified data points including a key for each x, y pair.
    /// The internal array is read only, ids are not required to be unique.
    /// </summary>
    /// <remarks>If any of the provided data points contain <see cref="double.NaN"/> for either the X or Y
    /// value, the instance will mark the data as containing invalid values.</remarks>
    /// <param name="dataPoints">A list of tuples representing the data points for regression analysis, where each tuple contains an X value and
    /// a Y value.</param>
    public RegressionPvalue(List<(string id, double x, double y)> dataPoints) : base(dataPoints.Select(tuple => (tuple.x, tuple.y)))
    {
        DataPoints = dataPoints.Select(tuple => (tuple.x, tuple.y));
        IdPoints = dataPoints.Select(tuple => tuple.id);
        _isDataContainsNan = dataPoints.Any(item => item.x is double.NaN || item.y is double.NaN);
    }

    public int DataPointsCount()
    {
        return DataPoints.Count();
    }

    private double _savedPvalue = double.NaN;
    private bool _isSavedPvalue = false;
    /// <summary>
    ///     P-Value for the linear regression slope.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public double PValue
    {
        get
        {
            if (_isSavedPvalue)
                return _savedPvalue;
            
            if (DataPoints.Count() < 3)
                throw new InvalidOperationException("At least 3 data points are required to compute the p-value.");

            if (_isDataContainsNan)
                return double.NaN; // If data contains NaN, return NaN for P-Value

            // Calculate slope and standard error of the slope
            var slope = Slope;
            var varianceX = VarianceX;
            if (varianceX == 0)
                return 1.0; // No variation in X, slope is undefined or always zero, so p-value is 1

            var rss = ResidualSumOfSquares;
            var n = DataPoints.Count();
            var seSlope = Math.Sqrt(rss / (n - 2) / varianceX);

            if (seSlope == 0)
                return slope == 0 ? 1.0 : 0.0; // If standard error is zero, p-value is 1 if slope is zero, else 0

            // Calculate t-statistic
            var t = slope / seSlope;

            // Calculate p-value from t-distribution (two-tailed test)
            double degreesOfFreedom = n - 2;
            var pValue = 2 * (1 - DistributionCdf(Math.Abs(t), degreesOfFreedom));

            if (double.IsNaN(pValue) || pValue < 0 || pValue > 1 || double.IsInfinity(t))
                return 1.0; // Defensive: return 1 for degenerate cases

            _savedPvalue = pValue;
            _isSavedPvalue = true;
            
            return pValue;
        }
    }

    private double ResidualSumOfSquares
    {
        get
        {
            double rss = 0;
            var slope = Slope;
            var intercept = YIntercept;

            // Calculate RSS: sum of (y - predicted_y)^2
            foreach (var (x, y) in DataPoints) // Assume you have a collection of data points
            {
                var predictedY = slope * x + intercept;
                rss += Math.Pow(y - predictedY, 2);
            }

            return rss;
        }
    }

    /// <summary>
    ///     CDF of the t-distribution (for calculating p-value).
    ///     You can replace this with a library function if available.
    /// </summary>
    private static double DistributionCdf(double t, double degreesOfFreedom)
    {
        // Create a t-distribution with specified degrees of freedom
        var tDistribution = new StudentT(0, 1, degreesOfFreedom);

        // Compute the CDF at t
        return tDistribution.CumulativeDistribution(t);
    }

}
