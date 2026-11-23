# Math definitions

The formulas behind `Regression` and `RegressionPvalue`: ordinary least squares on one predictor, $y = a + b\,x$.
They match Excel `LINEST(y, x, TRUE, TRUE)`, R `lm`, statsmodels `OLS` and SciPy `linregress`.

## Sums and means

For $n$ points $(x_i, y_i)$:

$$
\bar x = \frac{1}{n}\sum_{i=1}^{n} x_i, \qquad \bar y = \frac{1}{n}\sum_{i=1}^{n} y_i
$$

$$
S_{xx} = \sum_{i=1}^{n} (x_i - \bar x)^2, \qquad
S_{yy} = \sum_{i=1}^{n} (y_i - \bar y)^2, \qquad
S_{xy} = \sum_{i=1}^{n} (x_i - \bar x)(y_i - \bar y)
$$

The library accumulates these centred sums with Welford's streaming update, measured from the first point
(Chan's pairwise combine is used in `Merge`, and the exact reverse update in `Dec`), so they stay accurate when the data
are far from zero. The raw sums `Sx`, `Sx2` and so on are still kept for the public properties.

**Downdate precision (`Dec`).** Removing a point reverses the Welford update by subtraction. If the removed point
dominated the sums (for example $x = 0$ added to data near $10^6$, where it adds about $10^{12}$ to $S_{xx}$
against a true $S_{xx}$ of about $10^{-3}$), plain doubles keep only about 16 digits of that $10^{12}$, and the
subtraction leaves mostly rounding error (issue #14). The streaming means, $S_{xx}$, $S_{yy}$ and $S_{xy}$ are therefore
held as double-double values (an unevaluated sum of two doubles, about 32 digits) and updated with error-free
TwoSum/FMA products, so an `Add` followed by `Dec` of the same point restores the statistics to full double
precision. The remaining limit: removing a point whose contribution exceeds the remaining $S_{xx}$ by more than
about $10^{16}$ loses digits in proportion. For example, a point at $x = 10^{12}$ against $S_{xx} = 10^{-3}$ keeps about 6 digits.
Add and Dec cost about 3 times as much as with plain doubles (about 70 ns instead of 25 ns per point).

## Descriptive statistics (sample, n − 1)

`VarianceX`, `VarianceY`, `StdDevX` and `StdDevY` use the sample divisor $n - 1$ (Excel `VAR.S` / `STDEV.S`):

$$
s_x^2 = \frac{S_{xx}}{n-1}, \qquad s_x = \sqrt{\frac{S_{xx}}{n-1}}
$$

The data are treated as a sample from a larger population (for example patients in a study), not as the whole
population. When the data are the whole population, use `VarianceXPopulation`, `VarianceYPopulation`,
`StdDevXPopulation` and `StdDevYPopulation` (divisor $n$; Excel `VAR.P` / `STDEV.P`, numpy `ddof=0`). They are 0
for a single point and NaN with no data. The TI-58/59 manual offered the same choice between N and N − 1 weighting
(Texas Instruments, *Personal Programming*, 1977, section V). The regression standard errors, p-values and
confidence intervals have no such option: they always use $\mathrm{df} = n - 2$.

## Fit

$$
b = \frac{S_{xy}}{S_{xx}}, \qquad a = \bar y - b\,\bar x, \qquad
r = \frac{S_{xy}}{\sqrt{S_{xx}\,S_{yy}}}, \qquad R^2 = r^2
$$

Residual sum of squares and residual variance, with $n - 2$ degrees of freedom (two parameters were fitted):

$$
\mathrm{RSS} = \sum_{i=1}^{n} \bigl(y_i - a - b\,x_i\bigr)^2, \qquad
s^2 = \frac{\mathrm{RSS}}{n-2}, \qquad \mathrm{df} = n - 2
$$

$s$ is Excel's `STEYX`, the standard error of the y estimate.

## Slope: standard error, t, p-value, confidence interval

$$
\mathrm{SE}_b = \sqrt{\frac{s^2}{S_{xx}}}, \qquad t_b = \frac{b}{\mathrm{SE}_b}
$$

Two-sided p-value for $H_0: b = 0$, with $F_{\mathrm{df}}$ the Student t CDF:

$$
p_b = 2\,F_{\mathrm{df}}\!\left(-\lvert t_b \rvert\right)
$$

This is mathematically equal to $2\,(1 - F(\lvert t\rvert))$. The library uses the lower-tail form because
$1 - F$ rounds to 0 once $p$ falls below about $10^{-16}$, while $F(-\lvert t\rvert)$ keeps full precision
(for example $p = 1.03\times10^{-20}$ instead of 0). This is Excel's `T.DIST.2T(ABS(t), n-2)`.

Confidence interval at level $1 - \alpha$ (default 0.95), where $t_{1-\alpha/2,\,\mathrm{df}}$ is the t quantile:

$$
b \pm t_{1-\alpha/2,\,n-2}\;\mathrm{SE}_b
$$

Members: `PValue`, `ConfidenceInterval(level)`, `ConfidenceIntervalPlus(level)` (which also returns $\mathrm{SE}_b$).

## Intercept: standard error, t, p-value, confidence interval

$$
\mathrm{SE}_a = \sqrt{s^2\left(\frac{1}{n} + \frac{\bar x^{\,2}}{S_{xx}}\right)}
= \mathrm{SE}_b\,\sqrt{\frac{S_{xx}}{n} + \bar x^{\,2}}
$$

$$
t_a = \frac{a}{\mathrm{SE}_a}, \qquad p_a = 2\,F_{n-2}\!\left(-\lvert t_a \rvert\right), \qquad
a \pm t_{1-\alpha/2,\,n-2}\;\mathrm{SE}_a
$$

Members: `InterceptStandardError`, `InterceptPValue`, `InterceptConfidenceInterval(level)`.
$\mathrm{SE}_a$ is Excel `LINEST` row 2, column 2.

**Extrapolation.** The intercept is the fitted value at $x = 0$. The $\bar x^{\,2}/S_{xx}$ term grows as the data
move away from zero, so $\mathrm{SE}_a$ grows and the intercept test loses power and meaning. For example, with
$\bar x = 10^6$ and $x$ spread over about $\pm 0.03$, $\mathrm{SE}_a \approx 6\times10^5$. The intercept is also
ill-conditioned there: a tiny change in the slope moves it a lot, so expect fewer correct digits. If the value
at a meaningful $x_0$ matters, shift the data ($x - x_0$) before fitting. With $\bar x = 0$, $a = \bar y$ and
$\mathrm{SE}_a = s/\sqrt{n}$.

## Invalid results

Every p-value, standard error and interval is `double.NaN` (never 1, never ∞, never an exception) when:
$n < 3$; any $x$ or $y$ is NaN or ±∞; or all $x$ are equal ($S_{xx} = 0$). A confidence level outside $(0, 1)$
makes only the interval NaN. If the residual is zero (perfect fit), SE is 0, the interval collapses to the
estimate, and p is 0 for a non-zero estimate or NaN for a zero one ($t = 0/0$).

## Worked example (tiny5)

$x = 1, 2, 3, 4, 5$ and $y = 2, 4, 5, 4, 5$, so $n = 5$ and $\mathrm{df} = 3$.

| Quantity | Value |
|---|---|
| $\bar x$, $\bar y$ | 3, 4 |
| $S_{xx}$, $S_{yy}$, $S_{xy}$ | 10, 6, 6 |
| $b = 6/10$, $a = 4 - 0.6 \cdot 3$ | 0.6, 2.2 |
| fitted $\hat y$ | 2.8, 3.4, 4.0, 4.6, 5.2 |
| residuals | −0.8, 0.6, 1.0, −0.6, −0.2 |
| $\mathrm{RSS}$, $s^2 = 2.4/3$, $s$ | 2.4, 0.8, 0.894427 |
| $r$, $R^2$ | 0.774597, 0.6 |
| $\mathrm{SE}_b = \sqrt{0.8/10}$ | 0.282843 |
| $t_b$, $p_b$ | 2.121320, 0.124027 |
| $\mathrm{SE}_a = \sqrt{0.8\,(1/5 + 9/10)} = \sqrt{0.88}$ | 0.938083 |
| check: $\mathrm{SE}_b\sqrt{10/5 + 9} = 0.282843\cdot\sqrt{11}$ | 0.938083 |
| $t_a = 2.2/0.938083$ | 2.345208 |
| $p_a = 2\,F_3(-2.345208)$ | 0.100743 |
| $t_{0.975,\,3}$ | 3.182446 |
| 95% CI for $b$ | [−0.300132, 1.500132] |
| 95% CI for $a$: $2.2 \pm 3.182446 \cdot 0.938083$ | [−0.785399, 5.185399] |
| `VarianceX` $= 10/4$, `StdDevX` | 2.5, 1.581139 |

These match Excel `LINEST`, `STEYX`, `T.DIST.2T` and `STDEV.S`, and statsmodels `OLS`.

## Why Add and Dec exist

`Add` and `Dec` were the main interface of the original calculator program: 1978 algorithms for the TI-58 and
TI-59, later ported to C, then Perl, then C#. Basic linear regression was built into the 58/59, so no extra module
was needed. Data sets were entered by hand, one (x, y) pair at a time:

| Calculator keys | What the manual says | Library |
|---|---|---|
| `xᵢ x:t yᵢ 2nd Σ+` | enters one point (x goes to the T-register); the point number i is displayed | `Add(x, y)` |
| `xᵢ x:t yᵢ INV 2nd Σ+` | "reenter both the undesired x and y values again, but press INV immediately before 2nd Σ+"; N drops by 1 | `Dec(x, y)` |
| `2nd Pgm 1 SBR CLR` | zeroes registers 01-06 and the T-register (with a library module) | `new Regression()` |

The calculator didn't have enough memory to store the pairs, so the interface was limited on purpose. Its whole
state was six data registers of running sums:

| Register | 01 | 02 | 03 | 04 | 05 | 06 |
|---|---|---|---|---|---|---|
| Holds | Σy | Σy² | N | Σx | Σx² | Σxy |
| Library | `Sy` | `Sy2` | `N` | `Sx` | `Sx2` | `Sxy` |

Every statistic was computed from these registers. The same page covers the mean (`2nd x̄`), standard deviation
(`INV 2nd x̄`) and variance (`2nd Op 11`). The manual also notes that "data that has already been so grouped can be
entered directly into these registers"; the library's settable `Sx`, `Sy`, `Sx2`, `Sy2` and `Sxy` do the same.
`Regression` keeps that design: it streams data and never stores the points. Only `RegressionPvalue` keeps them,
because the residuals behind its p-values need them.

Because the running sums are all there is, their precision matters. Each pair is folded in and can later be taken
out again, and that must not wear away the digits of the points that stay. The library therefore keeps centred,
numerically stable sums (see [Sums and means](#sums-and-means)). Issue #14 covers removing a point far from the
rest of the data.

Sources: Texas Instruments, *Personal Programming* (1977), the owner's guide for the TI-58 and TI-59, p. V-33
(section V, "The Details"), and its Statistics section, "Data Entry"; Texas Instruments, *Applied Statistics*
(1977), the manual for the Statistics library module, which covers regression in more depth.
