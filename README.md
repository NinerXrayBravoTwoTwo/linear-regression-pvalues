# License

This project is licensed under the MIT License. See the LICENSE file for details.

# Linear Regression Library

A robust and lightweight C# library for performing 2D (bivariate) linear regression analysis. This project calculates statistical properties such as slope, y-intercept, correlation coefficient, and p-value for paired x, y data. It is designed for technical users, including data scientists, statisticians, and developers working on statistical modeling.

## Overview

The library provides a `Regression` class for computing 2D regression statistics and a `PValueStat` class for calculating the p-value of the regression slope. It supports adding, removing, and merging data points, with robust handling of edge cases like insufficient data or NaN values.

### Lineage
- Originated from 1978 TI-58 and TI-59 calculator algorithms.
- Ported to C in 1982, then to Perl 3 in 1987.
- Modernized in C# with updates in 2001, 2002, 2003, 2008, 2012, 2016, 2018, 2019, 2022, 2025
- Copyright � Jillian England, 2001�2022.

## Features

- **Data Management**:
  - Add individual (x, y) data points or merge multiple regression datasets.
  - Remove data points (decrement) with support for both explicit y-values and implicit y (based on sample count).
  - Track minimum and maximum x and y values.
- **Statistical Calculations**:
  - Sum of x, y, x�, y�, and x*y.
  - Mean of x and y.
  - Standard deviation and variance for x and y (with N or N-1 weighting for population or sample studies).
  - Slope, y-intercept, and correlation coefficient.
  - P-value for the regression slope (via `PValueStat` class).
- **Robustness**:
  - Handles edge cases (e.g., insufficient samples, division by zero, NaN, or infinite values).
  - Returns `double.NaN` for invalid computations instead of throwing exceptions in most cases.
- **Serialization**:
  - The `Regression` class is marked `[Serializable]` for persistence.

## Installation

The library is a C# project that can be included in your .NET solution:

1. Clone or download the repository.
2. Add the project or source files (`Regression.cs`, `PValue.cs`) to your solution.
3. Ensure the `MathNet.Numerics` NuGet package is installed for `PValueStat` (for t-distribution calculations).

```bash
dotnet add package MathNet.Numerics
```
## License

This project is licensed under the [Affero General Public License (AGPL-3.0)](./LICENSE). See the `LICENSE` file for full details.

## Contact

Jillian England  
Email: jill.england@comcast.net

-X: @Firefox_XB9R
-FB: Jillian England Seattle
-Any omissions or errors in this software specification are unintentional and should be reported for correction. 
