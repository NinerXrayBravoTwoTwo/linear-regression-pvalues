namespace LinearRegression
{
    public class RegressionQValue : RegressionPvalue
    {
        //private bool _isSavedQvalue;
        //private double _savedQvalue;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegressionQvalue"/> class with the specified data points.
        /// Experimental class to compute Q-values for regression slopes.
        /// </summary>
        /// <param name="dataPoints">A list of tuples representing the data points for regression analysis.</param>
        public RegressionQValue(List<(double x, double y)> dataPoints) : base(dataPoints)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegressionQvalue"/> class with the specified data points including IDs.
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
            //if (_isSavedQvalue)
            //    return _savedQvalue;

            if (DataPoints.Count() < 3)
                throw new InvalidOperationException("At least 3 data points are required to compute the Q-value.");

            if (_isDataContainsNan || pValues.Any(p => double.IsNaN(p) || p < 0 || p > 1))
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

            // Calculate Q-value using Benjamini-Hochberg: q = p * m / rank
            var qValue = currentPValue * m / rank;

            // Ensure Q-value is between 0 and 1 (monotonicity correction)
            if (rank < m)
            {
                var nextQValue = sortedPValues[rank] * m / (rank + 1); // Q-value for the next rank
                qValue = Math.Min(qValue, nextQValue);
            }

            qValue = Math.Min(1.0, Math.Max(0.0, qValue)); // Clamp to [0, 1]

            if (double.IsNaN(qValue) || double.IsInfinity(qValue))
                qValue = 1.0; // Defensive: return 1 for degenerate cases

            //_savedQvalue = qValue;
            //_isSavedQvalue = true;

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
