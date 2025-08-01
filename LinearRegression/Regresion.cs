namespace LinearRegression;

/// <summary>
///     2D Regression Generator (Bi-variate)
///     Lineage from 1978 TI-58 and TI-59 calculators.  Ported to C in 1982, ported to Perl 3 in 1987.
///     Parameter names are more 'traditional' than newer code and am not going to upgrade them at this time.
///     Copyright (c) Jillian England, 2001, 2002, 2003, 2008, 2012, 2016, 2018, 2019, 2022
/// </summary>
/// <remarks>
///     I am a 2D Regression generator
///     I can add pairs of x, y data to myself
///     I can return the following properties of that data
///     Two dimensional statistical data
///     sy  = sum y
///     sy2 = sum y^2
///     n
///     sx  = sum x
///     sx2 = sum x**2
///     sxy = sum x * y
///     mean x = mx = sx/n
///     mean y = my = sy/n
///     standard deviation x = StdDevX = sqr((sx2 - (sx^2 /n)) / (n-1) )
///     standard deviation y = StdDevY= sqr((sy2 - (sy^2 /n)) / (n-1) )
///     variance x = VarianceX = sx2 / n - mx^2
///     variance y = VarianceY = sy2 / n - my^2
///     Use N weighting for population studies
///     and N-1 for sample studies
///     Slope = m = sxy - (sx*sy)/n / sx2 - sx^2 /n
///     yIntercept = b = (sy - m*sx) / n
///     correlation coefficient = R = (m qx) /qy
/// </remarks>
[Serializable]
public class Regression
{
    public Regression(List<(double x, double y)> dataPoints)
    {
        foreach (var (x, y) in dataPoints) Add(x, y);
    }

    /// <summary>
    ///     Initialize a new regression, all data set to zero with no samples.
    /// </summary>
    public Regression()
    {
        Sy =
            Sy2 =
                N =
                    Sx =
                        Sx2 =
                            Sxy = 0;

        MinX = double.MaxValue;
        MinY = double.MaxValue;

        MaxX = double.MinValue;
        MaxY = double.MinValue;
    }

    /// <summary>
    ///     Initialize a new regression with initial x & y
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public Regression(double x, double y) : this()
    {
        Add(x, y);
    }

    /// <summary>
    ///     Clone a regression in a new instance
    /// </summary>
    /// <param name="cloneMe"></param>
    public Regression(Regression cloneMe)
    {
        Sy += cloneMe.Sy;
        Sy2 += cloneMe.Sy2;
        N += cloneMe.N;
        Sx += cloneMe.Sx;
        Sx2 += cloneMe.Sx2;
        Sxy += cloneMe.Sxy;

        MinX = cloneMe.MinX;
        MinY = cloneMe.MinY;

        MaxX = cloneMe.MaxX;
        MaxY = cloneMe.MaxY;
    }

    public double N { get; private set; }


    /// <summary>
    ///     Sum y
    /// </summary>
    public double Sy { get; set; }

    /// <summary>
    ///     Sum y**2
    /// </summary>
    public double Sy2 { get; set; }

    /// <summary>
    ///     Sum of all X's.
    /// </summary>
    public double Sx { get; set; }

    /// <summary>
    ///     Sum of all X's squared.
    /// </summary>
    public double Sx2 { get; set; }

    /// <summary>
    ///     Sum of all (X * Y)'s.
    /// </summary>
    public double Sxy { get; set; }

    /// <summary>
    ///     Maximum Value of X (note; this is not reversed in Dec method)
    /// </summary>
    public double MaxX { get; set; }

    /// <summary>
    ///     Minimum value of X (note; this is not reversed in Dec method)
    /// </summary>
    public double MinX { get; set; }

    /// <summary>
    ///     Maximum Value of Y (nte; this is not reversed in Dec method)
    /// </summary>
    public double MaxY { get; set; }

    /// <summary>
    ///     Minimum value of Y (note; this is not reversed in Dec method)
    /// </summary>
    public double MinY { get; set; }

    public bool IsNaN => double.IsNaN(StdDevX)
                         || double.IsNaN(StdDevY)
                         || double.IsNaN(Sx)
                         || double.IsNaN(Sy)
                         || double.IsPositiveInfinity(Slope)
                         || double.IsNaN(YIntercept);

    /// <summary>
    ///     Merge two Regressions into a new Regression instance.
    /// </summary>
    /// <param name="other">The Regression to merge with.</param>
    /// <returns>A new Regression representing the combined data.</returns>
    public Regression Merge(Regression other)
    {
        // if (other == null) throw new ArgumentNullException(nameof(other));
        ArgumentException.ThrowIfNullOrEmpty(nameof(other));

        return new Regression
        {
            Sy = Sy + other.Sy,
            Sy2 = Sy2 + other.Sy2,
            N = N + other.N,
            Sx = Sx + other.Sx,
            Sx2 = Sx2 + other.Sx2,
            Sxy = Sxy + other.Sxy,
            MaxX = Math.Max(MaxX, other.MaxX),
            MinX = Math.Min(MinX, other.MinX),
            MaxY = Math.Max(MaxY, other.MaxY),
            MinY = Math.Min(MinY, other.MinY)
        };
    }

    /// <summary>
    ///     Merge two Regressions, DEPRECATED, use Merge(Regression other) instead.`
    /// </summary>
    /// <param name="other"></param>
    public void Add(Regression other)
    {
        Sy += other.Sy;
        Sy2 += other.Sy2;
        N += other.N;
        Sx += other.Sx;
        Sx2 += other.Sx2;
        Sxy += other.Sxy;

        MaxX = Math.Max(MaxX, other.MaxX);
        MinX = Math.Min(MinX, other.MinX);

        MaxY = Math.Max(MaxY, other.MaxY);
        MinY = Math.Min(MinY, other.MinY);
    }

    /// <summary>
    ///     Add data point.
    /// </summary>
    /// <param name="x">X value of data.</param>
    /// <param name="y">Y value of data.</param>
    public void Add(double x, double y)
    {
        Sy += y;
        Sy2 += y * y;
        N++;
        Sx += x;
        Sx2 += x * x;
        Sxy += x * y;

        MaxX = Math.Max(MaxX, x);
        MinX = Math.Min(MinX, x);

        MaxY = Math.Max(MaxY, y);
        MinY = Math.Min(MinY, y);
    }

    /// <summary>
    ///     Add a data point.
    /// </summary>
    /// <param name="x">X value of data, y assumes "number of samples" +1.</param>
    public void Add(double x)
    {
        Add(x, N + 1);
    }

    /// <summary>
    ///     Remove a previous sample pair.  Must have x and y values.
    /// </summary>
    /// <param name="x">Previous X value of data.</param>
    /// <param name="y">Previous Y value of data.</param>
    public void Dec(double x, double y)
    {
        Sy -= y;
        Sy2 -= y * y;
        N--;
        Sx -= x;
        Sx2 -= x * x;
        Sxy -= x * y;
    }

    /// <summary>
    ///     Only removes previous sample added without Y value.
    /// </summary>
    /// <param name="x">The previous data point.  Assumes y = "number of samples" -1.</param>
    public void Dec(double x)
    {
        Dec(x, N - 1);
    }


    /// <summary>
    ///     mean x = sum(x) / n
    /// </summary>
    /// <returns>Mean of x</returns>
    public double MeanX => N > 0 ? Sx / N : 0;

    /// <summary>
    ///     mean y = sum(y) / n
    /// </summary>
    /// <returns>Mean of y</returns>
    public double MeanY => N > 0 ? Sy / N : 0;

    /// <summary>
    ///     Standard Deviation of X (requires at least 2 samples)
    /// </summary>
    /// <returns>Standard Deviation of X</returns>
    public double StdDevX
    {
        get
        {
            if (!(N > 1)) return double.NaN;

            // throw new InvalidOperationException(
            //"There must be more than one sample to find the Standard Deviation.");

            return Math.Sqrt(Sx2 - Sx * Sx / N) / (N - 1);
        }
    }

    /// <summary>
    ///     Standard Deviation of Y (requires at least 2 samples)
    /// </summary>
    /// <returns>Standard Deviation of Y</returns>
    public double StdDevY
    {
        get
        {
            if (!(N > 1)) return double.NaN;
            //throw new InvalidOperationException(
            //    "There must be more than one sample to find the Standard Deviation.");

            return Math.Sqrt(Sy2 - Sy * Sy / N) / (N - 1);
        }
    }

    /// <summary>
    ///     Calculates the variance of X.
    /// </summary>
    /// <returns>The unweighted variance of X, calculated as Sx2 / N - (MeanX)².</returns>
    /// <exception cref="DivideByZeroException">Thrown when the number of samples (N) is less than or equal to 0.</exception>
    public double VarianceX
    {
        get
        {
            if (!(N > 0)) throw new DivideByZeroException("Number of samples needs to be greater than 0.");

            return Sx2 / N - MeanX * MeanX;
        }
    }

    /// <summary>
    ///     Calculates the variance of Y.
    /// </summary>
    /// <returns>The unweighted variance of Y, calculated as Sy2 / N - (MeanY)².</returns>
    /// <exception cref="DivideByZeroException">Thrown when the number of samples (N) is less than or equal to 0.</exception>
    public double VarianceY
    {
        get
        {
            if (N > 0)
                return Sy2 / N - MeanY * MeanY;

            throw new DivideByZeroException("Number of samples needs to be greater than 0.");
        }
    }

    /// <summary>
    ///     Calculates the slope of the linear regression line.
    /// </summary>
    /// <returns>
    ///     The slope (m) of the regression line, calculated as (Sxy - Sx*Sy/N) / (Sx2 - Sx²/N). Returns double.NaN if N
    ///     &lt; 2 or the denominator is zero.
    /// </returns>
    //public double SlopeI()
    //{
    //    if (N < 2) return double.NaN; // Insufficient data

    //    var numerator = Sxy - Sx * Sy / N;
    //    var denominator = Sx2 - Sx * Sx / N;
    //    return denominator != 0 ? numerator / denominator : double.NaN;
    //}

    public double Slope
    {
        get
        {
            if (N < 2) return double.NaN; // Insufficient data

            var numerator = Sxy - Sx * Sy / N;
            var denominator = Sx2 - Sx * Sx / N;
            return denominator != 0 ? numerator / denominator : double.NaN;
        }
    }

    /// <summary>
    ///     Calculates the y-intercept of the linear regression line.
    /// </summary>
    /// <returns>The y-intercept (b) of the regression line, calculated as (Sy - m*Sx) / N.</returns>
    public double YIntercept =>
        // If something needs to be thrown it will happen
        // in Slope().
        (Sy - Slope * Sx) / N;

    /// <summary>
    ///     Calculates the Pearson correlation coefficient between X and Y.
    /// </summary>
    /// <returns>
    ///     The correlation coefficient (R), ranging from -1 to 1. Returns double.NaN if Qy is zero or the slope is
    ///     infinite.
    /// </returns>
    public double Correlation
    {
        get
        {
            var qy = StdDevY;
            if (qy == 0 || double.IsInfinity(Slope)) return double.NaN;
            return Slope * StdDevX / qy;
        }
    }

    /// <summary>
    ///     Calculates the coefficient of determination (R²) for the linear regression.
    /// </summary>
    /// <returns>
    ///     The R² value, ranging from 0 to 1, representing the proportion of variance in Y explained by X. Returns
    ///     double.NaN if the correlation coefficient is invalid.
    /// </returns>
    public double RSquared
    {
        get
        {
            var r = Correlation;
            if (double.IsNaN(r)) return double.NaN;
            return r * r;
        }
    }

    public override string ToString()
    {
        var isInfinity = double.IsPositiveInfinity(Slope);
        var result = isInfinity
            ? $"NaN - {N}"
            : $"Slope: {Slope:F2} R^2: {RSquared:f3} N={N} StdDev: (x{StdDevX:F3} y{StdDevY:F3}) Variance: (x{VarianceX:F3} y{VarianceY:F3}) Y-intercept: {YIntercept:F3}  Cor: {Correlation:F4} " +
              $"Mean: (x{MeanX:F2} y{MeanY:F2}) MinMax: x({MinX:0.##} <-> {MaxX:0.##}) y({MinY:0.####} <-> {MaxY:0.####}) isNaN:{IsNaN}";

        return result;
    }
}