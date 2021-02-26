using System;
using System.Linq;
using NumericMethods.Core.LinearAlgebra;

namespace NumericMethods.Core.PartialDiffEquation.Elliptical
{
	public class EllipticalFiniteDifference
	{
		private readonly EllipticalBoundaryConditions _conditions;
		private readonly EllipticalFiniteDifferenceParams _params;
		private readonly EllipticalEquationParams _equation;

		public EllipticalFiniteDifference(
			EllipticalBoundaryConditions conditions,
			EllipticalEquationParams equation,
			EllipticalFiniteDifferenceParams @params)
		{
			_params = @params;
			_conditions = conditions;
			_equation = equation;
		}

		private double GetXCoordinate(int i)
		{
			return (_params.XBoundRight - _params.XBoundLeft) / _params.XStepCount * i + _params.XBoundLeft;
		}

		private double GetYCoordinate(int i)
		{
			return (_params.YBoundRight - _params.YBoundLeft) / _params.YStepCount * i + _params.YBoundLeft;
		}

		public double[,] Solve()
		{
			var a = _equation.a;
			var b = _equation.b;
			var c = _equation.c;

			var h1 = (_params.XBoundRight - _params.XBoundLeft) / _params.XStepCount;
			var h2 = (_params.YBoundRight - _params.YBoundLeft) / _params.YStepCount;

			var alpha0 = _conditions.ConditionParameters[0, 0];
			var betta0 = _conditions.ConditionParameters[0, 1];

			var gamma0 = _conditions.ConditionParameters[1, 0];
			var delta0 = _conditions.ConditionParameters[1, 1];

			var alpha1 = _conditions.ConditionParameters[2, 0];
			var betta1 = _conditions.ConditionParameters[2, 1];

			var gamma1 = _conditions.ConditionParameters[3, 0];
			var delta1 = _conditions.ConditionParameters[3, 1];

			var dim = (_params.XStepCount + 1) * (_params.YStepCount + 1);

			var prevGrid = new double[_params.XStepCount + 1, _params.YStepCount + 1];
			var nextGrid = new double[_params.XStepCount + 1, _params.YStepCount + 1];

			var hx2 = h1 * h1;
			var hy2 = h2 * h2;

			if (alpha0 < _params.Eps)
			{
				for (int j = 0; j <= _params.YStepCount; j++)
				{
					prevGrid[0, j] = _conditions.InitialConditions[0](0, GetYCoordinate(j)) / betta0;
					nextGrid[0, j] = prevGrid[0, j];
				}
			}

			if (gamma0 < _params.Eps)
			{
				for (int j = 0; j <= _params.YStepCount; j++)
				{
					prevGrid[_params.XStepCount, j] = _conditions.InitialConditions[1](0, GetYCoordinate(j)) / delta0;
					nextGrid[_params.XStepCount, j] = prevGrid[_params.XStepCount, j];
				}
			}

			if (alpha1 < _params.Eps)
			{
				for (int i = 0; i <= _params.XStepCount; i++)
				{
					prevGrid[i, 0] = _conditions.InitialConditions[2](GetXCoordinate(i), 0) / betta1;
					nextGrid[i, 0] = prevGrid[i, 0];
				}
			}

			if (gamma1 < _params.Eps)
			{
				for (int i = 0; i <= _params.XStepCount; i++)
				{
					prevGrid[i, _params.YStepCount] = _conditions.InitialConditions[3](GetXCoordinate(i), 0) / delta1;
					nextGrid[i, _params.YStepCount] = prevGrid[i, _params.YStepCount];
				}
			}

			var it = 0;
			var w = 1.2d;	// magic

			do
			{
				if (alpha0 > _params.Eps)
				{
					for (int j = 1; j < _params.YStepCount; j++)
					{
						var f0 = _conditions.InitialConditions[0](GetXCoordinate(0), GetYCoordinate(j));

						nextGrid[0, j] =
							((prevGrid[1, j] + alpha0 * prevGrid[1, j] + 2 * h1 / alpha0 * (betta0 * prevGrid[0, j] - f0)) / hx2
							+ (prevGrid[0, j + 1] + prevGrid[0, j - 1]) / hy2)
							/ (2 / hx2 + 2 / hy2);
					}
				}

				if (alpha1 > _params.Eps)
				{
					for (int i = 1; i < _params.XStepCount; i++)
					{
						var f0 = _conditions.InitialConditions[2](GetXCoordinate(i), 0);

						nextGrid[i, 0] =
							((prevGrid[i + 1, 0] + prevGrid[i - 1, 0]) / hx2
							+ (prevGrid[i, 1] + alpha1 * prevGrid[i, 1] + 2 * h2 / alpha1 * (betta1 * prevGrid[i, 0] - f0)) / hy2)
							/ (2 / hx2 + 2 / hy2);
					}
				}

				for (int i = 1; i < _params.XStepCount; i++)
				{
					for (int j = 1; j < _params.YStepCount; j++)
					{
						switch (_params.Solver)
						{
							case SolverType.Liebmann:
								nextGrid[i, j] =
									((prevGrid[i + 1, j] + prevGrid[i - 1, j]) / hx2
									+ (prevGrid[i, j + 1] + prevGrid[i, j - 1]) / hy2)
									/ (2 / hx2 + 2 / hy2);
								break;
							case SolverType.Zeidel:
								nextGrid[i, j] =
									((prevGrid[i + 1, j] + nextGrid[i - 1, j]) / hx2
									+ (prevGrid[i, j + 1] + nextGrid[i, j - 1]) / hy2)
									/ (2 / hx2 + 2 / hy2);
								break;
							case SolverType.OverRelaxation:
								nextGrid[i, j] +=
									w * (((prevGrid[i + 1, j] + nextGrid[i - 1, j]) / hx2
										+ (prevGrid[i, j + 1] + nextGrid[i, j - 1]) / hy2)
										/ (2 / hx2 + 2 / hy2) - nextGrid[i, j]);
								break;
						}
					}
				}

				if (gamma0 > _params.Eps)
				{
					for (int j = 1; j < _params.YStepCount; j++)
					{
						var N = _params.XStepCount;
						var fn = _conditions.InitialConditions[1](_params.XBoundRight, GetYCoordinate(j));

						nextGrid[N, j] =
							((gamma0 * prevGrid[N - 1, j] + 2 * h1 / gamma0 * (fn - delta0 * prevGrid[N, j]) + prevGrid[N - 1, j]) / hx2
							+ (prevGrid[N, j + 1] + prevGrid[N, j - 1]) / hy2)
							/ (2 / hx2 + 2 / hy2);
					}
				}

				if (gamma1 > _params.Eps)
				{
					for (int i = 1; i < _params.XStepCount; i++)
					{
						var N = _params.YStepCount;
						var fn = _conditions.InitialConditions[3](GetXCoordinate(i), 0);

						nextGrid[i, N] =
							((prevGrid[i + 1, N] + prevGrid[i - 1, N]) / hx2
							+ (gamma1 * prevGrid[i, N - 1] + 2 * h2 / gamma1 * (fn - delta1 * prevGrid[i, N]) + prevGrid[i, N - 1]) / hy2)
							/ (2 / hx2 + 2 / hy2);
					}
				}

				var tmp = prevGrid;
				prevGrid = nextGrid;
				nextGrid = tmp;
				it++;
			}
			while (Norm(prevGrid, nextGrid) > _params.Eps && it < 50000);

			Console.WriteLine($"iterations: {it}");

			return nextGrid;
		}

		static double Norm(double[,] a, double[,] b)
		{
			var max = 0.0d;

			for (int i = 1; i < a.GetLength(0) - 1; i++)
			{
				for (int j = 1; j < a.GetLength(1) - 1; j++)
				{
					max = Math.Max(max, Math.Abs(a[i, j] - b[i, j]));
				}
			}

			return max;
		}

		public double[,] FindError(double[,] grid, Func<double, double, double> u)
		{
			var errors = new double[_params.XStepCount + 1, _params.YStepCount + 1];

			for (int k = 0; k <= _params.YStepCount; k++)
			{
				for (int j = 0; j <= _params.XStepCount; j++)
				{
					errors[j, k] = Math.Abs(grid[j, k] - u(GetXCoordinate(j), GetYCoordinate(k)));
				}
			}

			return errors;
		}
	}
}
