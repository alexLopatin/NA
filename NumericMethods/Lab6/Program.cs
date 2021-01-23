using NumericMethods.Core.PartialDiffEquation;
using NumericMethods.Core.PartialDiffEquation.Hyperbolic;
using System;
using System.Collections.Generic;

namespace Lab6
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
			var conditions = new HyperbolicBoundaryConditions()
			{
				FirstConditionParameters = new[] { .0d, 1.0d },
				SecondConditionParameters = new[] { .0d, 1.0d },
				InitialCondition = (x, t) => Math.Cos(x) * Math.Exp(-x),
				DerivativeCondition = (x, t) => -Math.Cos(x) * Math.Exp(-x),
				FirstCondition = (x, t) => Math.Cos(2 * t) * Math.Exp(-t),
				SecondCondition = (x, t) => 0
			};

			var @params = new FiniteDifferenceParams()
			{
				SpaceBoundLeft = 0,
				SpaceBoundRight = Math.PI / 2,
				TimeLimit = 1d,
				SpaceStepCount = 10,
				TimeStepCount = 400,
				ApproximationType = BoundaryApproximationType.FirstDegreeTwoPoints
			};

			var method = new HyperbolicExplicitFiniteDifference(conditions, @params);

			var result = method.Solve(new[] { 2.0 }, new[] { 1.0d, 2.0d, -3.0d }, (x, t) => 0);

			var errors = method.FindError((x, t) => Math.Cos(x) * Math.Cos(2 * t) * Math.Exp(-x - t));

			var maxError = FindMax(errors);
			var median = FindMedian(errors);

			Console.WriteLine($"Max error: {maxError}; median error: {median}");
			Console.ReadKey();
		}
	}
}
