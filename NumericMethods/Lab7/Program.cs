using NumericMethods.Core.PartialDiffEquation;
using NumericMethods.Core.PartialDiffEquation.Elliptical;
using System;
using System.Collections.Generic;

using ILNumerics;
using static ILNumerics.ILMath;
using static ILNumerics.Globals;
using System.Linq;

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
			//var max = 0.0d;
			var ans = 0.0d;

			for (int i = 0; i < grid.GetLength(0); i++)
			{
				for (int j = 0; j < grid.GetLength(1); j++)
				{
					//max = Math.Max(max, grid[i, j]);
					ans += grid[i, j] * grid[i, j];
				}
			}

			return ans / (grid.GetLength(0) * grid.GetLength(1));
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
				XStepCount = 80,
				YStepCount = 80,
				Solver = SolverType.Zeidel,
				Eps = 0.00000001d
			};

			Array<double> first = Array.Empty<double>();
			Array<double> errNonLog = Array.Empty<double>();
			var errors = new double[0, 0];

			for (int i = 4; i <= 80; i += 3)
			{
				@params.XStepCount = i;
				@params.YStepCount = i;
				var method = new EllipticalFiniteDifference(conditions, equation, @params);
				var result = method.Solve();
				errors = method.FindError(result, (x, y) => Math.Exp(y) * Math.Sin(x));
				var maxError = Math.Log(FindMax(errors));
				first = first.Append(maxError).ToArray();
				errNonLog = errNonLog.Append(FindMax(errors)).ToArray();
			}

			errNonLog = errNonLog.T;
			first = first.T;

			Console.ReadKey();

		}
	}
}
