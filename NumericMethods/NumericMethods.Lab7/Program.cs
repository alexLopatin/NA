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
				ConditionParameters = new double[4,2] { { 1, 0 }, { 1, -1 }, { 0, 1 }, { 0, 1 } },
				InitialConditions = new Func<double, double, double>[4]
				{
					(x, y) => Math.Cos(y),
					(x, y) => 0,
					(x, y) => x,
					(x, y) => 0
				}
			};
			var equation = new EllipticalEquationParams()
			{
				a = .0d,
				b = .0d,
				c = -1.0d,
				f = (x, t) => 0
			};
			var @params = new EllipticalFiniteDifferenceParams()
			{
				XBoundLeft = 0,
				XBoundRight = 1d,
				YBoundLeft = 0,
				YBoundRight = Math.PI / 2,
				XStepCount = 15,
				YStepCount = 15,
				Solver = SolverType.Libman,
				Eps = 0.0001d,
				BoundaryApproximation = BoundaryApproximationType.SecondDegreeThreePoints
			};

			var method = new EllipticalFiniteDifference(conditions, equation, @params);

			var result = method.Solve();

			var errors = method.FindError((x, y) => x * Math.Cos(y));

			var maxError = FindMax(errors);
			var median = FindMedian(errors);

			Console.WriteLine($"Max error: {maxError}; median error: {median}");
			Console.ReadKey();
		}
	}
}
