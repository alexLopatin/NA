using NumericMethods.Core.PartialDiffEquation;
using NumericMethods.Core.PartialDiffEquation.Splitting;
using System;
using System.Collections.Generic;

namespace Lab7
{
	class Program
	{
		static double FindMedian(double[,,] grid)
		{
			List<double> linear = new List<double>();

			for (int i = 0; i < grid.GetLength(0); i++)
			{
				for (int j = 0; j < grid.GetLength(1); j++)
				{
					for (int k = 0; k < grid.GetLength(2); k++)
					{
						linear.Add(grid[i, j, k]);
					}
				}
			}

			linear.Sort();

			return linear[linear.Count / 2];
		}

		static double FindMax(double[,,] grid)
		{
			var max = 0.0d;

			for (int i = 0; i < grid.GetLength(0); i++)
			{
				for (int j = 0; j < grid.GetLength(1); j++)
				{
					for (int k = 0; k < grid.GetLength(2); k++)
					{
						max = Math.Max(max, grid[i, j, k]);
					}
				}
			}

			return max;
		}

		static void Main(string[] args)
		{
			var conditions = new SplittingBoundaryConditions()
			{
				ConditionParameters = new double[4, 2] { { 0, 1 }, { 1, 0 }, { 1, 0 }, { 0, 1 } },
				InitialConditions = new Func<double, double, double, double>[4]
				{
					(x, y, t) => Math.Sinh(y) * Math.Exp(-3 * t),
					(x, y, t) => -2 * Math.Sinh(y) * Math.Exp(-3 * t),
					(x, y, t) => Math.Cos(2 * x) * Math.Exp(-3 * t),
					(x, y, t) => 3.0d / 4 * Math.Cos(2 * x) * Math.Exp(-3 * t)
				},
				ZeroTimeCondition = (x, y, t) => Math.Cos(2 * x) * Math.Sinh(y)
			};
			var equation = new SplittingEquationParams()
			{
				a = 1.0d,
				b = 1.0d,
				f = (x, y, t) => 0
			};
			var @params = new SplittingFiniteDifferenceParams()
			{
				XBoundLeft = 0,
				XBoundRight = Math.PI / 4,
				YBoundLeft = 0,
				YBoundRight = Math.Log(2),
				XStepCount = 160,
				YStepCount = 160,
				TimeLimit = 1d,
				TimeStepCount = 160,
				BoundaryApproximation = BoundaryApproximationType.SecondDegreeThreePoints
			};

			var method = new FractionalStepMethod(conditions, equation, @params);

			var result = method.Solve();

			var errors = method.FindError((x, y, t) => Math.Cos(2 * x) * Math.Sinh(y) * Math.Exp(-3 * t));

			var maxError = FindMax(errors);
			var median = FindMedian(errors);

			Console.WriteLine($"Max error: {maxError}; median error: {median}");
			Console.ReadKey();
		}
	}
}
