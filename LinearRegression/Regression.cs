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
///     variance x = VarianceX = Sxx / (n-1)   (sample, n-1)
///     variance y = VarianceY = Syy / (n-1)
///     Slope = m = Sxy / Sxx
///     yIntercept = b = mean y - m * mean x
///     correlation coefficient = R = (m StdDevX) / StdDevY
///     Numerics: the centred sums Sxx = sum((x - mean x)^2), Syy and the co-moment Cxy = sum((x - mean x)(y - mean y))
///     are accumulated with Welford's streaming update (reverse update in Dec, Chan et al. pairwise combine in Merge),
///     so results stay accurate for data far from zero (e.g. x = 1e9 + small).
///     The streaming moments are double-double (about 32 digits), so Dec can remove a point that dominated the sums
///     (e.g. one far from the rest of the data) without losing the remaining ones (issue #14). The raw sums Sx, Sy, Sx2, Sy2, Sxy are
///     still accumulated alongside for the public properties; the textbook raw-sum formulas are used only as a
///     fallback (see <see cref="IsStreamingValid" />).
/// </remarks>
[Serializable]
public class Regression
{
    public Regression(IEnumerable<(double x, double y)> dataPoints) : this()
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

        _streamingValid = true;
    }

    /// <summary>
    ///     Initialize a new regression with initial x and y
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
        CopyStateFrom(cloneMe);
    }

    public double N { get; private set; }

    // Raw sums, kept for the public Sx/Sy/Sx2/Sy2/Sxy properties (and the fallback formulas).
    private double _sx, _sy, _sx2, _sy2, _sxy;

    // Streaming (Welford) state, kept relative to a reference point (_kx, _ky) = the first point added
    // ("shifted data"), so the running means stay small and do not lose digits when the data are far from zero:
    // means of (x - _kx) and (y - _ky), centred sums of squares and the centred co-moment.
    // The moments are double-double (about 32 digits) so Dec can remove a point that dominated them (for example
    // one far from the rest of the data) without cancelling away the remaining sums; see docs/MATH.md.
    private double _kx, _ky;
    private DoubleDouble _meanX, _meanY, _m2X, _m2Y, _cXY;
    private bool _streamingValid;

    /// <summary>
    ///     Sum y. Setting it directly (or any other raw sum) switches this instance to the raw-sum formulas,
    ///     because the streaming state can no longer be kept consistent.
    /// </summary>
    public double Sy { get => _sy; set { _sy = value; _streamingValid = false; } }

    /// <summary>
    ///     Sum y**2. See <see cref="Sy" /> about setting it.
    /// </summary>
    public double Sy2 { get => _sy2; set { _sy2 = value; _streamingValid = false; } }

    /// <summary>
    ///     Sum of all X's. See <see cref="Sy" /> about setting it.
    /// </summary>
    public double Sx { get => _sx; set { _sx = value; _streamingValid = false; } }

    /// <summary>
    ///     Sum of all X's squared. See <see cref="Sy" /> about setting it.
    /// </summary>
    public double Sx2 { get => _sx2; set { _sx2 = value; _streamingValid = false; } }

    /// <summary>
    ///     Sum of all (X * Y)'s. See <see cref="Sy" /> about setting it.
    /// </summary>
    public double Sxy { get => _sxy; set { _sxy = value; _streamingValid = false; } }

    /// <summary>
    ///     True when the numerically stable streaming (Welford) state is in use. False after a NaN or infinite
    ///     value was added or removed, after N went negative through <see cref="Dec(double, double)" />, or after
    ///     a raw sum was set directly; the statistics then use the raw-sum formulas, which is also what gives
    ///     NaN/infinity the same propagation as before.
    /// </summary>
    protected bool IsStreamingValid => _streamingValid;

    /// <summary>
    ///     Centred sum of squares of x, Sxx = sum((x - mean x)^2). Streaming (Welford) when valid, else Sx2 - Sx^2/N.
    /// </summary>
    protected double CenteredSumSquaresX =>
        _streamingValid ? _m2X.Value : N > 0 ? _sx2 - _sx * _sx / N : 0;

    /// <summary>
    ///     Centred sum of squares of y, Syy = sum((y - mean y)^2). Streaming when valid, else Sy2 - Sy^2/N.
    /// </summary>
    protected double CenteredSumSquaresY =>
        _streamingValid ? _m2Y.Value : N > 0 ? _sy2 - _sy * _sy / N : 0;

    /// <summary>
    ///     Centred co-moment, Cxy = sum((x - mean x)(y - mean y)). Streaming when valid, else Sxy - Sx*Sy/N.
    /// </summary>
    protected double CenteredCoMoment =>
        _streamingValid ? _cXY.Value : N > 0 ? _sxy - _sx * _sy / N : 0;

    /// <summary>
    ///     Mean of x used for centring: the streaming mean when valid, else Sx/N.
    /// </summary>
    protected double CenterX => _streamingValid ? (_kx + _meanX).Value : N > 0 ? _sx / N : 0;

    /// <summary>
    ///     Mean of y used for centring: the streaming mean when valid, else Sy/N.
    /// </summary>
    protected double CenterY => _streamingValid ? (_ky + _meanY).Value : N > 0 ? _sy / N : 0;

    /// <summary>
    ///     x minus the mean of x, computed as (x - reference) - shifted mean to avoid cancellation.
    /// </summary>
    protected double DeviationX(double x) => _streamingValid ? (DoubleDouble.Difference(x, _kx) - _meanX).Value : x - CenterX;

    /// <summary>
    ///     y minus the mean of y, computed as (y - reference) - shifted mean to avoid cancellation.
    /// </summary>
    protected double DeviationY(double y) => _streamingValid ? (DoubleDouble.Difference(y, _ky) - _meanY).Value : y - CenterY;

    private void CopyStateFrom(Regression o)
    {
        N = o.N;
        _sx = o._sx; _sy = o._sy; _sx2 = o._sx2; _sy2 = o._sy2; _sxy = o._sxy;
        _kx = o._kx; _ky = o._ky;
        _meanX = o._meanX; _meanY = o._meanY; _m2X = o._m2X; _m2Y = o._m2Y; _cXY = o._cXY;
        _streamingValid = o._streamingValid;
        MinX = o.MinX; MinY = o.MinY; MaxX = o.MaxX; MaxY = o.MaxY;
    }

    private void ResetStreaming()
    {
        _kx = _ky = 0;
        _meanX = _meanY = _m2X = _m2Y = _cXY = DoubleDouble.Zero;
    }

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
    /// <remarks>
    ///     The streaming state is combined with the pairwise update of Chan, Golub and LeVeque (1979), so the
    ///     result equals (to rounding) a single pass over both data sets.
    /// </remarks>
    public Regression Merge(Regression other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var a = this;
        var b = other;
        var r = new Regression
        {
            N = a.N + b.N,
            _sx = a._sx + b._sx,
            _sy = a._sy + b._sy,
            _sx2 = a._sx2 + b._sx2,
            _sy2 = a._sy2 + b._sy2,
            _sxy = a._sxy + b._sxy,
            MaxX = Math.Max(a.MaxX, b.MaxX),
            MinX = Math.Min(a.MinX, b.MinX),
            MaxY = Math.Max(a.MaxY, b.MaxY),
            MinY = Math.Min(a.MinY, b.MinY),
            _streamingValid = a._streamingValid && b._streamingValid && a.N >= 0 && b.N >= 0
        };

        if (!r._streamingValid) return r;

        static void CopyStreaming(Regression from, Regression to)
        {
            to._kx = from._kx; to._ky = from._ky;
            to._meanX = from._meanX; to._meanY = from._meanY;
            to._m2X = from._m2X; to._m2Y = from._m2Y; to._cXY = from._cXY;
        }

        if (a.N == 0) { CopyStreaming(b, r); return r; }
        if (b.N == 0) { CopyStreaming(a, r); return r; }

        // Work in a's reference frame: difference of the two means = (kb - ka) + (mean'b - mean'a).
        var n = r.N;
        r._kx = a._kx;
        r._ky = a._ky;
        var dx = DoubleDouble.Difference(b._kx, a._kx) + (b._meanX - a._meanX);
        var dy = DoubleDouble.Difference(b._ky, a._ky) + (b._meanY - a._meanY);
        var w = new DoubleDouble(a.N) * b.N / n;
        r._meanX = a._meanX + dx * b.N / n;
        r._meanY = a._meanY + dy * b.N / n;
        r._m2X = a._m2X + b._m2X + dx * dx * w;
        r._m2Y = a._m2Y + b._m2Y + dy * dy * w;
        r._cXY = a._cXY + b._cXY + dx * dy * w;
        return r;
    }

    /// <summary>
    ///     Add data point.
    /// </summary>
    /// <param name="x">X value of data.</param>
    /// <param name="y">Y value of data.</param>
    public void Add(double x, double y)
    {
        _sy += y;
        _sy2 += y * y;
        N++;
        _sx += x;
        _sx2 += x * x;
        _sxy += x * y;

        if (_streamingValid)
        {
            if (double.IsFinite(x) && double.IsFinite(y) && N >= 1)
            {
                if (N == 1)
                {
                    // First point: it becomes the reference; shifted mean and centred sums are all 0.
                    _kx = x;
                    _ky = y;
                }
                else
                {
                    // Welford on the shifted values: update means, then the centred sums with the
                    // deviations from the old and the new mean.
                    var sx = DoubleDouble.Difference(x, _kx);
                    var sy = DoubleDouble.Difference(y, _ky);
                    var dx = sx - _meanX;
                    var dy = sy - _meanY;
                    _meanX += dx / N;
                    _meanY += dy / N;
                    _m2X += dx * (sx - _meanX);
                    _m2Y += dy * (sy - _meanY);
                    _cXY += dx * (sy - _meanY);
                }
            }
            else
            {
                _streamingValid = false; // NaN/infinity: fall back to the raw sums so NaN propagates as before
            }
        }

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
    /// <remarks>
    ///     The streaming state is reversed exactly as Welford's update was applied. Removing a pair that was never
    ///     added gives meaningless results, as before.
    /// </remarks>
    public void Dec(double x, double y)
    {
        _sy -= y;
        _sy2 -= y * y;
        N--;
        _sx -= x;
        _sx2 -= x * x;
        _sxy -= x * y;

        if (!_streamingValid) return;

        if (!double.IsFinite(x) || !double.IsFinite(y) || N < 0)
        {
            _streamingValid = false;
            return;
        }

        if (N == 0)
        {
            ResetStreaming();
            return;
        }

        // Reverse Welford on the shifted values: recover the previous means, then remove this point's contribution.
        var sx = DoubleDouble.Difference(x, _kx);
        var sy = DoubleDouble.Difference(y, _ky);
        var dxNew = sx - _meanX; // deviation from the mean that included the point
        var dyNew = sy - _meanY;
        _meanX -= dxNew / N;
        _meanY -= dyNew / N;
        var dxOld = sx - _meanX; // deviation from the mean without the point
        _m2X -= dxOld * dxNew;
        _m2Y -= (sy - _meanY) * dyNew;
        _cXY -= dxOld * dyNew;
    }

    /// <summary>
    ///     Only removes previous sample added without Y value (the last such sample, whose y was N).
    /// </summary>
    /// <param name="x">The previous data point.  Assumes y = "number of samples" -1.</param>
    public void Dec(double x)
    {
        Dec(x, N);
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
    ///     Sample (n−1) standard deviation of X: √<see cref="VarianceX" />, the same as numpy <c>std(x, ddof=1)</c>.
    /// </summary>
    /// <returns>The sample standard deviation of X, or <see cref="double.NaN" /> when N &lt; 2. Never throws.</returns>
    public double StdDevX => Math.Sqrt(VarianceX);

    /// <summary>
    ///     Sample (n−1) standard deviation of Y: √<see cref="VarianceY" />, the same as numpy <c>std(y, ddof=1)</c>.
    /// </summary>
    /// <returns>The sample standard deviation of Y, or <see cref="double.NaN" /> when N &lt; 2. Never throws.</returns>
    public double StdDevY => Math.Sqrt(VarianceY);

    /// <summary>
    ///     Sample (n−1) variance of X: Sxx / (N − 1), the same as numpy <c>var(x, ddof=1)</c>. Sxx is accumulated
    ///     with Welford's update, so this stays accurate when x is far from zero.
    /// </summary>
    /// <returns>
    ///     The sample variance of X; exactly 0 when every x is equal; <see cref="double.NaN" /> when N &lt; 2 or the
    ///     data contains NaN or infinity. Never throws.
    /// </returns>
    public double VarianceX => SampleVariance(CenteredSumSquaresX, MinX, MaxX);

    /// <summary>
    ///     Sample (n−1) variance of Y: Syy / (N − 1), the same as numpy <c>var(y, ddof=1)</c>. Syy is accumulated
    ///     with Welford's update, so this stays accurate when y is far from zero.
    /// </summary>
    /// <returns>
    ///     The sample variance of Y; exactly 0 when every y is equal; <see cref="double.NaN" /> when N &lt; 2 or the
    ///     data contains NaN or infinity. Never throws.
    /// </returns>
    public double VarianceY => SampleVariance(CenteredSumSquaresY, MinY, MaxY);

    /// <summary>
    ///     Population (N) variance of X: Sxx / N, the same as Excel <c>VAR.P</c> and numpy <c>var(x, ddof=0)</c>.
    ///     Use it when the data are the whole population rather than a sample; <see cref="VarianceX" /> (n − 1) is
    ///     the default. Regression standard errors, p-values and intervals always use n − 2 and are not affected.
    /// </summary>
    /// <returns>
    ///     The population variance of X; exactly 0 when every x is equal (including N = 1); <see cref="double.NaN" />
    ///     when N &lt; 1 or the data contains NaN or infinity. Never throws.
    /// </returns>
    public double VarianceXPopulation => PopulationVariance(CenteredSumSquaresX, MinX, MaxX);

    /// <summary>
    ///     Population (N) variance of Y: Syy / N, the same as Excel <c>VAR.P</c> and numpy <c>var(y, ddof=0)</c>.
    ///     See <see cref="VarianceXPopulation" />.
    /// </summary>
    /// <returns>
    ///     The population variance of Y; exactly 0 when every y is equal (including N = 1); <see cref="double.NaN" />
    ///     when N &lt; 1 or the data contains NaN or infinity. Never throws.
    /// </returns>
    public double VarianceYPopulation => PopulationVariance(CenteredSumSquaresY, MinY, MaxY);

    /// <summary>
    ///     Population (N) standard deviation of X: √<see cref="VarianceXPopulation" />, the same as Excel
    ///     <c>STDEV.P</c> and numpy <c>std(x, ddof=0)</c>.
    /// </summary>
    /// <returns>The population standard deviation of X, or <see cref="double.NaN" /> when N &lt; 1. Never throws.</returns>
    public double StdDevXPopulation => Math.Sqrt(VarianceXPopulation);

    /// <summary>
    ///     Population (N) standard deviation of Y: √<see cref="VarianceYPopulation" />, the same as Excel
    ///     <c>STDEV.P</c> and numpy <c>std(y, ddof=0)</c>.
    /// </summary>
    /// <returns>The population standard deviation of Y, or <see cref="double.NaN" /> when N &lt; 1. Never throws.</returns>
    public double StdDevYPopulation => Math.Sqrt(VarianceYPopulation);

    private double PopulationVariance(double centeredSumSquares, double min, double max)
    {
        if (N < 1) return double.NaN; // undefined with no data

        if (min == max) return 0.0; // all values equal (or N = 1)

        var variance = centeredSumSquares / N;
        return variance < 0 ? 0.0 : variance; // a tiny negative can only come from rounding; NaN passes through
    }

    private double SampleVariance(double centeredSumSquares, double min, double max)
    {
        if (N < 2) return double.NaN; // undefined for fewer than 2 samples

        if (min == max) return 0.0; // all values equal; avoids rounding noise such as 1.7e-18 for x = 0.1 repeated

        var variance = centeredSumSquares / (N - 1);
        return variance < 0 ? 0.0 : variance; // a tiny negative can only come from rounding; NaN passes through
    }

    /// <summary>
    ///     Calculates the slope of the linear regression line.
    /// </summary>
    /// <returns>
    ///     The slope (m) of the regression line, Cxy / Sxx (centred co-moment over centred sum of squares of x,
    ///     both accumulated with Welford's update). Returns double.NaN if N
    ///     &lt; 2 or x has no variation (every x equal, <see cref="MinX" /> == <see cref="MaxX" />, or a zero
    ///     denominator).
    /// </returns>
    public double Slope
    {
        get
        {
            if (N < 2) return double.NaN; // Insufficient data

            // All x identical: the slope is undefined. Checked explicitly because rounding can leave
            // Sx2 - Sx^2/N as a tiny non-zero number (e.g. x = 0.1 repeated) and yield a garbage slope.
            if (MinX == MaxX) return double.NaN;

            var numerator = CenteredCoMoment;
            var denominator = CenteredSumSquaresX;
            return denominator != 0 ? numerator / denominator : double.NaN;
        }
    }

    /// <summary>
    ///     Calculates the y-intercept of the linear regression line.
    /// </summary>
    /// <returns>The y-intercept (b) of the regression line, mean y − m · mean x.</returns>
    public double YIntercept =>
        // Slope is NaN when N < 2 or x has no variation; that NaN propagates here.
        _streamingValid ? CenterY - Slope * CenterX : (Sy - Slope * Sx) / N;

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

    /// <summary>
    ///     One-line summary of the regression. Never throws: statistics that are undefined for the current N
    ///     print as NaN, and MinMax prints "n/a" for an empty instance.
    /// </summary>
    public override string ToString()
    {
        var isInfinity = double.IsPositiveInfinity(Slope);
        var minMax = N == 0
            ? "x(n/a) y(n/a)"
            : $"x({MinX:0.##} <-> {MaxX:0.##}) y({MinY:0.####} <-> {MaxY:0.####})";
        var result = isInfinity
            ? $"NaN - {N}"
            : $"Slope: {Slope:F2} R^2: {RSquared:f3} N={N} StdDev: (x{StdDevX:F3} y{StdDevY:F3}) Variance: (x{VarianceX:F3} y{VarianceY:F3}) Y-intercept: {YIntercept:F3}  Cor: {Correlation:F4} " +
              $"Mean: (x{MeanX:F2} y{MeanY:F2}) MinMax: {minMax} isNaN:{IsNaN}";

        return result;
    }
}