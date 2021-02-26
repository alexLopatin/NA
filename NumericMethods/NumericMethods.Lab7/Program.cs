using NumericMethods.Core.PartialDiffEquation;
using NumericMethods.Core.PartialDiffEquation.Elliptical;
using System;
using System.Collections.Generic;

namespace Lab7
{
	class Program
	{
		static double FindMedian(double[,] grid)
		{
			List<double> linear = new List<double>();

			for (int i = 0; i < grid.GetLength(0); i++)
			{
				for (int j = 0; j < grid.GetLength(1); j++)
				{
					linear.Add(grid[i, j]);
				}
			}

			linear.Sort();

			return linear[linear.Count / 2];
		}

		static double FindMax(double[,] grid)
		{
			var max = 0.0d;

			for (int i = 0; i < grid.GetLength(0); i++)
			{
				for (int j = 0; j < grid.GetLength(1); j++)
				{
					max = Math.Max(max, grid[i, j]);
				}
			}

			return max;
		}

		static void Main(string[] args)
		{
			var conditions = new EllipticalBoundaryConditions()
			{
				ConditionParameters = new double[4,2] { { 1, 0 }, { 1, 0 }, { 0, 1 }, { 0, 1 } },
				InitialConditions = new Func<double, double, double>[4]
				{
					(x, y) => Math.Exp(y),
					(x, y) => -Math.Exp(y),
					(x, y) => Math.Sin(x),
					(x, y) => Math.E * Math.Sin(x)
				}
			};
			var equation = new EllipticalEquationParams()
			{
				a = .0d,
				b = .0d,
				c = .0d,
				f = (x, y) => 0
			};
			var @params = new EllipticalFiniteDifferenceParams()
			{
				XBoundLeft = 0,
				XBoundRight = Math.PI,
				YBoundLeft = 0,
				YBoundRight = 1d,
				XStepCount = 20,
				YStepCount = 20,
				Solver = SolverType.Liebmann,
				Eps = 0.00001d
			};

			var method = new EllipticalFiniteDifference(conditions, equation, @params);

			var result = method.Solve();

			var errors = method.FindError(result, (x, y) => Math.Exp(y) * Math.Sin(x));

			var maxError = FindMax(errors);
			var median = FindMedian(errors);

			Console.WriteLine($"Max error: {maxError}; median error: {median}");
			Console.ReadKey();
		}
	}
}
