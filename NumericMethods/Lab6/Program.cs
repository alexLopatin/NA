using NumericMethods.Core.PartialDiffEquation;
using NumericMethods.Core.PartialDiffEquation.Hyperbolic;
using System;
using System.Collections.Generic;

using ILNumerics;
using static ILNumerics.ILMath;
using static ILNumerics.Globals;
using System.Linq;

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
				SpaceStepCount = 200,
				TimeStepCount = 600,
				InitialApproximation = InitialApproximationType.FirstDegree,
				BoundaryApproximation = BoundaryApproximationType.SecondDegreeTwoPoints
			};
			Array<double> FirstDegree = Array.Empty<double>();
			Array<double> SecondDegree = Array.Empty<double>();
			Array<double> errNonLog = Array.Empty<double>();

			for (int i = 100; i <= 1000; i += 100)
			{
				@params.BoundaryApproximation = BoundaryApproximationType.SecondDegreeTwoPoints;
				@params.SpaceStepCount = i;
				@params.TimeStepCount = i;
				var method = new HyperbolicExplicitFiniteDifference(conditions, equation, @params);
				var result = method.Solve();
				var errors = method.FindError((x, t) => Math.Cos(t) * Math.Exp(2 * x));
				var maxError = FindMax(errors);
				SecondDegree = SecondDegree.Append(Math.Log(maxError)).ToArray();
				errNonLog = errNonLog.Append(maxError).ToArray();
			}

			for (int i = 100; i <= 1000; i += 100)
			{
				@params.BoundaryApproximation = BoundaryApproximationType.SecondDegreeTwoPoints;
				@params.InitialApproximation = InitialApproximationType.SecondDegree;
				@params.SpaceStepCount = i;
				@params.TimeStepCount = i;
				var method = new HyperbolicExplicitFiniteDifference(conditions, equation, @params);
				var result = method.Solve();
				var errors = method.FindError((x, t) => Math.Cos(t) * Math.Exp(2 * x));
				var maxError = Math.Log(FindMax(errors));
				FirstDegree = FirstDegree.Append(maxError).ToArray();
			}

			var allDegrees = ILMath.horzcat(FirstDegree, SecondDegree).T;
			errNonLog = errNonLog.T;
			Console.ReadKey();
		}
	}
}
