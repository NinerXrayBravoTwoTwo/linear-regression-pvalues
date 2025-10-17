namespace RegressionTest
{
    internal class QValueCalculator
    {
        public QValueCalculator()
        {
        }

        public double[] CalculateQValues(double[] pValues)
        {
            if (pValues == null || pValues.Length == 0)
                throw new ArgumentException("pValues cannot be null or empty.", nameof(pValues));

            var m = pValues.Length;
            var qValues = new double[m];
            var sortedIndices = Enumerable.Range(0, m).OrderBy(i => pValues[i]).ToArray();
            var sortedPValues = sortedIndices.Select(i => pValues[i]).ToArray();

            for (var i = m - 1; i >= 0; i--)
            {
                var rank = i + 1;
                qValues[sortedIndices[i]] = sortedPValues[i] * m / rank;
                if (i < m - 1) qValues[sortedIndices[i]] = Math.Min(qValues[sortedIndices[i]], qValues[sortedIndices[i + 1]]);
            }

            return qValues;
        }
    }
}