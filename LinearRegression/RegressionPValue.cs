using MathNet.Numerics.Distributions;

namespace LinearRegression;

[Serializable]
public partial class RegressionPvalue : Regression
{
    protected bool IsDataContainsNan;
    protected bool IsDataContainsInf;
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
        DataPoints = dataPoints.Select(tuple => (tuple.x, tuple.y)).ToArray(); // copy: caller may change the list later
        IdPoints = [];
        IsDataContainsNan = dataPoints.Any(item => item.x is double.NaN || item.y is double.NaN);
        IsDataContainsInf = dataPoints.Any(item => double.IsInfinity(item.x) || double.IsInfinity(item.y));
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
        DataPoints = dataPoints.Select(tuple => (tuple.x, tuple.y)).ToArray(); // copy: caller may change the list later
        IdPoints = dataPoints.Select(tuple => tuple.id).ToArray();
        IsDataContainsNan = dataPoints.Any(item => item.x is double.NaN || item.y is double.NaN);
        IsDataContainsInf = dataPoints.Any(item => double.IsInfinity(item.x) || double.IsInfinity(item.y));
    }

    public int DataPointsCount()
    {
        return DataPoints.Count();
    }

    /// <summary>
    ///     The <see cref="Regression.ToString" /> summary followed by the slope p-value, or "n/a" when the
    ///     p-value is invalid (<see cref="double.NaN" />; see <see cref="PValue" />).
    /// </summary>
    public override string ToString()
    {
        var p = PValue;
        var pValue = double.IsNaN(p) ? "n/a" : p.ToString("G4");
        return $"{base.ToString()} PValue: {pValue}";
    }

    private double _savedPvalue = double.NaN;
    private bool _isSavedPvalue = false;

    /// <summary>
    ///     Two-tailed p-value for the null hypothesis slope = 0 (Student t, n - 2 degrees of freedom).
    /// </summary>
    /// <value>
    ///     <para>A value in [0, 1], or <see cref="double.NaN" /> when the test is invalid. NaN is the only
    ///     "invalid" marker: this property never returns 1 or infinity as a stand-in, and never throws, so
    ///     invalid results can be filtered with <see cref="double.IsNaN" /> in large batches.</para>
    ///     <para>Invalid (NaN) when: fewer than 3 data points; any x or y is NaN or infinite; x has no
    ///     variation (all x equal); y has no variation (all y equal: slope 0 with zero residual, t = 0/0).</para>
    ///     <para>Valid edge case: a perfect fit with a non-zero slope (zero residual) returns 0.</para>
    /// </value>
    public double PValue
    {
        get
        {
            if (_isSavedPvalue)
                return _savedPvalue;

            _savedPvalue = ComputePValue();
            _isSavedPvalue = true;
            return _savedPvalue;
        }
    }

    private double ComputePValue()
    {
        if (!IsSlopeTestDefined())
            return double.NaN;

        if (MinY == MaxY)
            return double.NaN; // all y equal: slope 0, residual 0, t = 0/0

        var slope = Slope;
        var n = DataPoints.Count();
        var seSlope = SlopeStandardError(SumOfSquaresX, n);

        if (seSlope == 0)
            return slope == 0 ? double.NaN : 0.0; // perfect fit with non-zero slope: p = 0 is valid

        var t = slope / seSlope;
        if (double.IsNaN(t) || double.IsInfinity(t))
            return double.NaN;

        // Two-tailed p-value from the t distribution
        double degreesOfFreedom = n - 2;
        var pValue = 2 * (1 - DistributionCdf(Math.Abs(t), degreesOfFreedom));

        if (double.IsNaN(pValue))
            return double.NaN;

        return Math.Clamp(pValue, 0.0, 1.0); // only absorbs floating-point rounding
    }

    /// <summary>
    ///     True when the slope t-test is defined: at least 3 points, all finite, and x varies.
    ///     Shared by <see cref="PValue" />, <see cref="ConfidenceInterval" /> and <see cref="ConfidenceIntervalPlus" />.
    /// </summary>
    private bool IsSlopeTestDefined() =>
        DataPoints.Count() >= 3
        && !IsDataContainsNan
        && !IsDataContainsInf
        && MinX != MaxX
        && SumOfSquaresX > 0;

    /// <summary>
    ///     Corrected sum of squares of x: Sxx = sum((x - mean x)^2), accumulated with Welford's update
    ///     (see <see cref="Regression.CenteredSumSquaresX" />).
    /// </summary>
    private double SumOfSquaresX => CenteredSumSquaresX;

    /// <summary>
    ///     Standard error of the slope: sqrt( (RSS / (n - 2)) / Sxx ).
    ///     (Previously divided by VarianceX = Sxx / n, which inflated the SE by sqrt(n).)
    /// </summary>
    private double SlopeStandardError(double sxx, int n) => Math.Sqrt(ResidualSumOfSquares / (n - 2) / sxx);

    private double ResidualSumOfSquares
    {
        get
        {
            double rss = 0;
            var slope = Slope;

            // RSS = sum of (y - predicted y)^2, computed from centred values: residual = (y - mean y) - m (x - mean x).
            // Same value as y - (m x + b), but without the cancellation of m x + b when x is far from zero.
            foreach (var (x, y) in DataPoints)
            {
                var residual = DeviationY(y) - slope * DeviationX(x);
                rss += residual * residual;
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
