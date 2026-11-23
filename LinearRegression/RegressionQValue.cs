namespace LinearRegression
{
    /// <summary>
    ///     Experimental. A <see cref="RegressionPvalue" /> that can also report the Benjamini-Hochberg q-value
    ///     (false discovery rate adjusted p-value) of its slope within a family of tests, for example many regressions
    ///     from one data dredge. For a whole batch, the static <see cref="BenjaminiHochberg" /> method returns every q-value
    ///     at once. Invalid (NaN) p-values are left out of the ranking and of m.
    /// </summary>
    public class RegressionQValue : RegressionPvalue
    {
        // Q-values are not cached (unlike PValue): a q-value depends on the family of p-values
        // passed in, so a cached value would be wrong for the next call with a different family.

        /// <summary>
        /// Initializes a new instance of the <see cref="RegressionQValue"/> class with the specified data points.
        /// Experimental class to compute Q-values for regression slopes.
        /// </summary>
        /// <param name="dataPoints">A list of tuples representing the data points for regression analysis.</param>
        public RegressionQValue(List<(double x, double y)> dataPoints) : base(dataPoints)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegressionQValue"/> class with the specified data points including IDs.
        /// </summary>
        /// <param name="dataPoints">A list of tuples with an ID, X value, and Y value.</param>
        public RegressionQValue(List<(string id, double x, double y)> dataPoints) : base(dataPoints)
        {
        }

        /// <summary>
        /// Calculates the Q-value (FDR-adjusted P-value) for the regression slope using the Benjamini-Hochberg procedure.
        /// </summary>
        /// <param name="pValues">
        /// P-values of the whole family of tests, normally including this regression's own <see cref="RegressionPvalue.PValue" />
        /// (it is added if missing). Invalid entries (NaN, or outside [0, 1]) are excluded from the ranking and from m,
        /// as in statsmodels <c>fdr_bh</c> applied to the valid subset.
        /// </param>
        /// <returns>
        /// The q-value in [0, 1], or <see cref="double.NaN" /> when this regression's own p-value is NaN (see
        /// <see cref="RegressionPvalue.PValue" />). Never throws.
        /// </returns>
        public double QValue(IReadOnlyList<double> pValues)
        {
            var currentPValue = PValue;
            if (!IsValidPValue(currentPValue))
                return double.NaN;

            var family = pValues.Where(IsValidPValue).ToList();
            if (!family.Contains(currentPValue))
                family.Add(currentPValue);

            family.Sort();
            var rank = family.IndexOf(currentPValue) + 1; // 1-based; first of any ties
            var m = family.Count;

            // Benjamini-Hochberg step-up: q(rank) = min over k >= rank of p(k) * m / k,
            // i.e. a cumulative minimum taken from the top rank (m) down to this rank.
            var qValue = double.PositiveInfinity;
            for (var k = m; k >= rank; k--)
                qValue = Math.Min(qValue, family[k - 1] * m / k);

            return Math.Min(1.0, qValue);
        }

        /// <summary>
        /// Benjamini-Hochberg q-values for a whole family in one pass (O(m log m)), aligned with the input.
        /// Use this rather than calling <see cref="QValue(IReadOnlyList{double})" /> once per regression when the
        /// family is large. Invalid entries (NaN, or outside [0, 1]) get a NaN q-value and are excluded from the
        /// ranking and from m.
        /// </summary>
        /// <param name="pValues">The family of p-values.</param>
        /// <returns>An array of q-values, one per input p-value, in the same order.</returns>
        public static double[] BenjaminiHochberg(IReadOnlyList<double> pValues)
        {
            var q = new double[pValues.Count];
            Array.Fill(q, double.NaN);

            var valid = Enumerable.Range(0, pValues.Count)
                .Where(i => IsValidPValue(pValues[i]))
                .OrderBy(i => pValues[i])
                .ToArray();
            var m = valid.Length;

            var running = double.PositiveInfinity;
            for (var k = m; k >= 1; k--)
            {
                var index = valid[k - 1];
                running = Math.Min(running, pValues[index] * m / k);
                q[index] = Math.Min(1.0, running);
            }

            return q;
        }

        private static bool IsValidPValue(double p) => p >= 0 && p <= 1; // false for NaN

        /// <summary>
        /// Calculates the Q-value using a list of RegressionPvalue instances.
        /// </summary>
        /// <param name="regressions">A list of RegressionPvalue instances, including this instance.</param>
        /// <returns>The Q-value for this regression's slope.</returns>
        public double QValue(IReadOnlyList<RegressionPvalue> regressions)
        {
            var pValues = regressions.Select(r => r.PValue).ToList();
            return QValue(pValues);
        }
    }
}
