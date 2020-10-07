using System;
using System.Collections.Generic;
using System.Linq;
using NumericMethods.Core;
using NumericMethods.Core.PartialDiffEquation;

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
			var conditions = new BoundaryConditionsThirdDegree()
			{
				FirstConditionParameters = new[] { .0d, 1.0d },
				SecondConditionParameters = new[] { .0d, 1.0d },
				InitialFunc = (x, t) => Math.Sin(2 * Math.PI * x),
				FirstFunc = (x, t) => 0,
				SecondFunc = (x, t) => 0
			};

			var @params = new FiniteDifferenceParams()
			{
				SpaceBoundLeft = 0,
				SpaceBoundRight = 1,
				TimeLimit = 1d,
				SpaceStepCount = 200,
				TimeStepCount = 90000
			};

			//var method = new FiniteDifference(conditions, @params);

			//var result = method.SolveExplicit(new[] { 1.0d, .0d, .0d }, (x, t) => 0);
			var method = new CrankNikolsonMethod(conditions, @params);

			var result = method.Solve(new[] { 1.0d, .0d, .0d }, (x, t) => 0);

			var errors = method.FindError((x, t) => Math.Exp(-t * 4 * Math.PI * Math.PI) * Math.Sin(2 * Math.PI * x));

			var maxError = FindMax(errors);
			var median = FindMedian(errors);

			Console.WriteLine($"Max error: {maxError}; median error: {median}");
			Console.ReadKey();
		}
	}
}
