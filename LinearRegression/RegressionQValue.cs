namespace LinearRegression
{
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
        /// <param name="pValues">A list of P-values from multiple regression tests, including this regression's P-value.</param>
        /// <returns>The Q-value for this regression's slope, or double.NaN if invalid.</returns>
        /// <exception cref="InvalidOperationException">Thrown if there are insufficient data points or P-values.</exception>
        public double QValue(IReadOnlyList<double> pValues)
        {
            if (DataPoints.Count() < 3)
                throw new InvalidOperationException("At least 3 data points are required to compute the Q-value.");

            if (IsDataContainsNan || pValues.Any(p => double.IsNaN(p) || p < 0 || p > 1))
                return double.NaN;

            if (pValues.Count < 1)
                throw new InvalidOperationException("At least one P-value is required to compute the Q-value.");

            // Get the P-value for this regression
            var currentPValue = PValue;
            if (double.IsNaN(currentPValue))
                return double.NaN;

            // Ensure the current P-value is included in the list
            var pValueList = pValues.Where(p => !double.IsNaN(p)).ToList();
            if (!pValueList.Contains(currentPValue))
                pValueList.Add(currentPValue);

            // Sort P-values in ascending order and assign ranks
            var sortedPValues = pValueList.OrderBy(p => p).ToList();
            var rank = sortedPValues.IndexOf(currentPValue) + 1; // 1-based rank
            var m = sortedPValues.Count; // Total number of tests

            // Benjamini-Hochberg step-up: q(rank) = min over k >= rank of p(k) * m / k,
            // i.e. a cumulative minimum taken from the top rank (m) down to this rank.
            var qValue = double.PositiveInfinity;
            for (var k = m; k >= rank; k--)
                qValue = Math.Min(qValue, sortedPValues[k - 1] * m / k);

            qValue = Math.Min(1.0, Math.Max(0.0, qValue)); // Clamp to [0, 1]

            if (double.IsNaN(qValue) || double.IsInfinity(qValue))
                qValue = 1.0; // Defensive: return 1 for degenerate cases

            return qValue;
        }

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
