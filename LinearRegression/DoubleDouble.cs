namespace LinearRegression;

/// <summary>
///     Unevaluated sum hi + lo of two doubles (about 32 significant digits), used for the streaming moments so a
///     Welford downdate (<see cref="Regression.Dec(double, double)" />) can remove a point exactly enough even when that
///     point dominated the sums. Error-free transforms: Knuth TwoSum and an FMA-based TwoProduct.
/// </summary>
[Serializable]
internal readonly struct DoubleDouble
{
    public readonly double Hi;
    public readonly double Lo;

    public DoubleDouble(double hi, double lo = 0)
    {
        Hi = hi;
        Lo = lo;
    }

    public static readonly DoubleDouble Zero = new(0);

    /// <summary>Nearest double to hi + lo.</summary>
    public double Value => Hi + Lo;

    public static implicit operator DoubleDouble(double d) => new(d);

    private static DoubleDouble TwoSum(double a, double b)
    {
        var s = a + b;
        var bb = s - a;
        var err = (a - (s - bb)) + (b - bb);
        return new DoubleDouble(s, err);
    }

    private static DoubleDouble QuickTwoSum(double a, double b)
    {
        var s = a + b;
        return new DoubleDouble(s, b - (s - a));
    }

    private static DoubleDouble TwoProduct(double a, double b)
    {
        var p = a * b;
        return new DoubleDouble(p, Math.FusedMultiplyAdd(a, b, -p));
    }

    /// <summary>Exact difference a - b of two doubles.</summary>
    public static DoubleDouble Difference(double a, double b) => TwoSum(a, -b);

    public static DoubleDouble operator +(DoubleDouble a, DoubleDouble b)
    {
        var s = TwoSum(a.Hi, b.Hi);
        var t = TwoSum(a.Lo, b.Lo);
        var hi = s.Hi;
        var lo = s.Lo + t.Hi;
        var u = QuickTwoSum(hi, lo);
        return QuickTwoSum(u.Hi, u.Lo + t.Lo);
    }

    public static DoubleDouble operator -(DoubleDouble a) => new(-a.Hi, -a.Lo);

    public static DoubleDouble operator -(DoubleDouble a, DoubleDouble b) => a + -b;

    public static DoubleDouble operator *(DoubleDouble a, DoubleDouble b)
    {
        var p = TwoProduct(a.Hi, b.Hi);
        var lo = p.Lo + (a.Hi * b.Lo + a.Lo * b.Hi);
        return QuickTwoSum(p.Hi, lo);
    }

    public static DoubleDouble operator /(DoubleDouble a, double b)
    {
        var q1 = a.Hi / b;
        var r = a - TwoProduct(q1, b);
        var q2 = r.Hi / b;
        r -= TwoProduct(q2, b);
        var q3 = r.Hi / b;
        var q = QuickTwoSum(q1, q2);
        return q + q3;
    }
}
