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
				FirstConditionParameters = new[] { 1.0d, -2.0d },
				SecondConditionParameters = new[] { 1.0d, -2.0d },
				InitialCondition = (x, t) => Math.Exp(2 * x),
				DerivativeCondition = (x, t) => 0,
				FirstCondition = (x, t) => 0,
				SecondCondition = (x, t) => 0,
			};
			var equation = new HyperbolicEquationParams()
			{
				a = 1.0d,
				b = .0d,
				c = -5.0d,
				d = .0d,
				f = (x, t) => 0
			};
			var @params = new HyperbolicFiniteDifferenceParams()
			{
				SpaceBoundLeft = 0,
				SpaceBoundRight = 1d,
				TimeLimit = 1d,
				SpaceStepCount = 400,
				TimeStepCount = 1000,
				InitialApproximation = InitialApproximationType.SecondDegree,
				BoundaryApproximation = BoundaryApproximationType.SecondDegreeThreePoints
			};

			var method = new HyperbolicImplicitFiniteDifference(conditions, equation, @params);

			var result = method.Solve();

			var errors = method.FindError((x, t) => Math.Cos(t) * Math.Exp(2 * x));

			var maxError = FindMax(errors);
			var median = FindMedian(errors);

			Console.WriteLine($"Max error: {maxError}; median error: {median}");
			Console.ReadKey();
		}
	}
}
