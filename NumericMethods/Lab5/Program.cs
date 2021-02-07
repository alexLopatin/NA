using System;
using System.Collections.Generic;
using System.Linq;
using NumericMethods.Core;
using NumericMethods.Core.Expressions;
using NumericMethods.Core.Expressions.Helpers;
using NumericMethods.Core.PartialDiffEquation;
using NumericMethods.Core.PartialDiffEquation.Parabolic;

namespace Lab5
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
			var conditions = new ParabolicBoundaryConditions()
			{
				FirstConditionParameters = new[] { 1.0d, 1.0d },
				SecondConditionParameters = new[] { 1.0d, 1.0d },
				InitialCondition = (x, t) => Math.Sin(x),
				FirstCondition = (x, t) => Math.Exp(-2 * t) * (Math.Sin(t) + Math.Cos(t)),
				SecondCondition = (x, t) => -Math.Exp(-2 * t) * (Math.Sin(t) + Math.Cos(t))
			};
			var equation = new ParabolicEquationParams()
			{
				a = 1.0d,
				b = 1.0d,
				c = -1.0d,
				f = (x, t) => 0
			};
			var @params = new ParabolicFiniteDifferenceParams()
			{
				SpaceBoundLeft = 0,
				SpaceBoundRight = Math.PI,
				TimeLimit = 1d,
				SpaceStepCount = 20,
				TimeStepCount = 400,
				BoundaryApproximation = BoundaryApproximationType.SecondDegreeThreePoints
			};

			var method = new ParabolicExplicitFiniteDifference(conditions, equation, @params);

			var result = method.Solve();

			var errors = method.FindError((x, t) => Math.Exp(-2 * t) * Math.Sin(x + t));

			var maxError = FindMax(errors);
			var median = FindMedian(errors);

			Console.WriteLine($"Max error: {maxError}; median error: {median}");
			Console.ReadKey();
		}
	}
}
