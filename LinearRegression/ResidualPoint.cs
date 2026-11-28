namespace LinearRegression;

/// <summary>
///     One point's regression diagnostics, returned by <see cref="RegressionPvalue.Residuals" /> and
///     <see cref="RegressionPvalue.CurveResiduals" />: the fitted value, the residual, three scaled versions of it
///     and the point's leverage and influence. Formulas are in docs/MATH.md ("Residuals, leverage and Cook's distance").
/// </summary>
/// <param name="X">The point's x as stored in <see cref="RegressionPvalue.DataPoints" /> (for a curve fit,
///     the transformed x′ in <see cref="RegressionPvalue.Residuals" /> and the original x in
///     <see cref="RegressionPvalue.CurveResiduals" />).</param>
/// <param name="Y">The point's y, with the same units as <paramref name="X" />.</param>
/// <param name="Fit">The fitted y on the line at <paramref name="X" />: ŷ = ȳ + m (x − x̄).</param>
/// <param name="Residual">y − ŷ. Positive means the point is above the fitted line. The residuals sum to 0.</param>
/// <param name="Standardized">Residual / s, with s = √(RSS / (n − 2)). A quick scale-free outlier flag.</param>
/// <param name="Studentized">Internally studentized residual: Residual / (s · √(1 − h)). Accounts for the point's
///     own leverage; |value| above about 2 is a common outlier flag. statsmodels <c>resid_studentized_internal</c>.</param>
/// <param name="Leverage">Hat value h = 1/n + (x − x̄)² / Sxx, between 1/n and 1. Large when x is far from the
///     mean of x; values above 2·2/n (that is, 2p/n with p = 2 parameters) are a common flag. statsmodels
///     <c>hat_matrix_diag</c>.</param>
/// <param name="CooksDistance">Cook's distance D = Studentized² · h / (2 · (1 − h)): how much the fit would move
///     if this point were dropped. Values above 4/n are a common flag. statsmodels <c>cooks_distance[0]</c>.</param>
/// <remarks>
///     A field that cannot be computed is <see cref="double.NaN" />; see <see cref="RegressionPvalue.Residuals" />
///     for exactly when. A <c>readonly record struct</c>, so it has value equality, <c>with</c> expressions and
///     deconstruction: <c>var (x, y, fit, res, std, stu, lev, cook) = point;</c>
/// </remarks>
public readonly record struct ResidualPoint(
    double X,
    double Y,
    double Fit,
    double Residual,
    double Standardized,
    double Studentized,
    double Leverage,
    double CooksDistance);
