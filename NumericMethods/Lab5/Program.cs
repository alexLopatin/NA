using System;
using System.Collections.Generic;
using System.Linq;
using NumericMethods.Core;
using NumericMethods.Core.Expressions;
using NumericMethods.Core.Expressions.Helpers;
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
				SecondConditionParameters = new[] { 1.0d, .0d },
				InitialFunc = (x, t) => 0,
				FirstFunc = (x, t) => Math.Sin(t),
				SecondFunc = (x, t) => -Math.Sin(t)
			};

			var @params = new FiniteDifferenceParams()
			{
				SpaceBoundLeft = 0,
				SpaceBoundRight = Math.PI / 2,
				TimeLimit = 1d,
				SpaceStepCount = 200,
				TimeStepCount = 90000
			};

			var expression = new Expression();
			var variables = new List<Variable>()
			{
				new Variable("x", 0),
				new Variable("t", 0)
			};

			expression.FromString("e^(-2*t)*sin(x)", variables);

			var expFunc = expression.GetTwoDimensional();
			Func<double, double, double> f = (x, t) => Math.Exp(-2 * t) * Math.Sin(x);

			var method = new FiniteDifference(conditions, @params);

			//var result = method.SolveExplicit(new[] { 1.0d, .0d, .0d }, (x, t) => 0);
			//var method = new CrankNikolsonMethod(conditions, @params);

			var result = method.SolveExplicit(new[] { 1.0d, .0d, .0d }, (x, t) => Math.Cos(x) * (Math.Cos(t) + Math.Sin(t)));

			var errors = method.FindError((x, t) => Math.Sin(t) * Math.Cos(x));

			var maxError = FindMax(errors);
			var median = FindMedian(errors);

			Console.WriteLine($"Max error: {maxError}; median error: {median}");
			Console.ReadKey();
		}
	}
}
