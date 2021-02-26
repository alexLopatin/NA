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

			for (int i = 0; i <= _params.XStepCount; i++)
			{
				prevGrid[i, 0] = _conditions.InitialConditions[2](GetXCoordinate(i), GetYCoordinate(0));
				nextGrid[i, 0] = _conditions.InitialConditions[2](GetXCoordinate(i), GetYCoordinate(0));

				prevGrid[i, _params.YStepCount] = _conditions.InitialConditions[3](GetXCoordinate(i), _params.YBoundRight);
				nextGrid[i, _params.YStepCount] = _conditions.InitialConditions[3](GetXCoordinate(i), _params.YBoundRight);
			}

			for (int j = 1; j < _params.YStepCount; j++)
			{
				var N = _params.XStepCount;
				var f0 = _conditions.InitialConditions[0](GetXCoordinate(0), GetYCoordinate(j));
				var fn = _conditions.InitialConditions[1](_params.XBoundRight, GetYCoordinate(j));

				prevGrid[0, j] = f0 - Math.Cos(GetXCoordinate(0)) * Math.Exp(GetYCoordinate(j));
				prevGrid[_params.XStepCount, j] = fn - Math.Cos(_params.XBoundRight) * Math.Exp(GetYCoordinate(j));

				nextGrid[0, j] = f0 - Math.Cos(GetXCoordinate(0)) * Math.Exp(GetYCoordinate(j));
				nextGrid[_params.XStepCount, j] = fn - Math.Cos(_params.XBoundRight) * Math.Exp(GetYCoordinate(j));

				/*prevGrid[0, j] = -(alpha0 / (N * h1)) / (betta0 - alpha0 / (N * h1)) * fn +
						f0 / (betta0 - alpha0 / (N * h1));
				prevGrid[_params.XStepCount, j] = (gamma0 / (N * h1)) / (delta0 + gamma0 / (N * h1)) * f0 +
						fn / (delta0 + gamma0 / (N * h1));

				nextGrid[0, j] = -(alpha0 / (N * h1)) / (betta0 - alpha0 / (N * h1)) * fn +
						f0 / (betta0 - alpha0 / (N * h1));
				nextGrid[_params.XStepCount, j] = (gamma0 / (N * h1)) / (delta0 + gamma0 / (N * h1)) * f0 +
						fn / (delta0 + gamma0 / (N * h1));*/
			}

			/*for (int i = 0; i <= _params.XStepCount; i++)
			{
				var f0 = _conditions.InitialConditions[2](i, 0);
				var fn = _conditions.InitialConditions[3](i, 0);

				prevGrid[i, 0] = -(alpha1 / h2) / (betta1 - alpha1 / h2) * prevGrid[i, 1] +
						f0 / (betta1 - alpha1 / h2);
				prevGrid[i, _params.YStepCount] = (gamma1 / h2) / (delta1 + gamma1 / h2) * prevGrid[i, _params.YStepCount - 1] +
						fn / (delta1 + gamma1 / h2);

				nextGrid[i, 0] = -(alpha1 / h2) / (betta1 - alpha1 / h2) * prevGrid[i, 1] +
						f0 / (betta1 - alpha1 / h2);
				nextGrid[i, _params.YStepCount] = (gamma1 / h2) / (delta1 + gamma1 / h2) * prevGrid[i, _params.YStepCount - 1] +
						fn / (delta1 + gamma1 / h2);

				for (int j = 0; j <= _params.YStepCount; j++)
				{
					//prevGrid[i, j] = (1 - (double)j / _params.YStepCount) * f0 + (double)j / _params.YStepCount * fn;
					//nextGrid[i, j] = prevGrid[i, j];
				}
			}

			for (int j = 1; j < _params.YStepCount; j++)
			{
				var f0 = _conditions.InitialConditions[0](0, j);
				var fn = _conditions.InitialConditions[1](0, j);

				var g1 = (1 - 1.0d / _params.XStepCount) * f0 + 1.0d / _params.XStepCount * fn;
				var gn = (1 - 1.0d / _params.XStepCount) * fn + 1.0d / _params.XStepCount * f0;

				prevGrid[0, j] = -(alpha0 / h1) / (betta0 - alpha0 / h1) * 0 +
						f0 / (betta0 - alpha0 / h1);
				prevGrid[_params.XStepCount, j] = (gamma0 / h1) / (delta0 + gamma0 / h1) * 0 +
						fn / (delta0 + gamma0 / h1);

				nextGrid[0, j] = -(alpha0 / h1) / (betta0 - alpha0 / h1) * 0 +
						f0 / (betta0 - alpha0 / h1);
				nextGrid[_params.XStepCount, j] = (gamma0 / h1) / (delta0 + gamma0 / h1) * 0 +
						fn / (delta0 + gamma0 / h1);
			}*/

			var it = 0;
			var w = 1.2d;

			do
			{
				for (int i = 1; i < _params.XStepCount; i++)
				{
					for (int j = 1; j < _params.YStepCount; j++)
					{
						switch (_params.Solver)
						{
							case SolverType.Libman:
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

				for (int j = 1; j < _params.YStepCount; j++)
				{
					var f0 = _conditions.InitialConditions[0](0, j);
					var fn = _conditions.InitialConditions[1](0, j);

					//nextGrid[0, j] = -(alpha0 / h1) / (betta0 - alpha0 / h1) * prevGrid[1, j] +
					//		f0 / (betta0 - alpha0 / h1);
					//nextGrid[0, j] = (-f0 + betta0 * nextGrid[0, 1]) / (alpha0 / h1) - nextGrid[0, 0];
					//nextGrid[_params.XStepCount, j] = (-fn + delta0 * nextGrid[_params.XStepCount, 0]) / (gamma0 / h1) - nextGrid[_params.XStepCount - 1, 0];
					//nextGrid[_params.XStepCount, j] = (gamma0 / h1) / (delta0 + gamma0 / h1) * prevGrid[_params.XStepCount - 1, j] +
					//		fn / (delta0 + gamma0 / h1);
				}

				/*for (int i = 1; i < _params.XStepCount; i++)
				{
					var f0 = _conditions.InitialConditions[2](i, 0);
					var fn = _conditions.InitialConditions[3](i, 0);

					nextGrid[i, 0] = -(alpha1 / h2) / (betta1 - alpha1 / h2) * prevGrid[i, 1] +
							f0 / (betta1 - alpha1 / h2);
					nextGrid[i, _params.YStepCount] = (gamma1 / h2) / (delta1 + gamma1 / h2) * prevGrid[i, _params.YStepCount - 1] +
							fn / (delta1 + gamma1 / h2);
				}*/

				var tmp = prevGrid;
				prevGrid = nextGrid;
				nextGrid = tmp;
				it++;
			}
			while (Norm(prevGrid, nextGrid) > _params.Eps);

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

		/*private double[] SetBorderGrid(int k, Matrix matrix)
		{
			var g = _equation.d;
			var a = _equation.a;
			var b = _equation.b;
			var c = _equation.c;

			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			var alpha = _conditions.FirstConditionParameters[0];
			var betta = _conditions.FirstConditionParameters[1];

			var gamma = _conditions.SecondConditionParameters[0];
			var delta = _conditions.SecondConditionParameters[1];

			var f0 = _conditions.FirstCondition(0, GetTimeCoordinate(k));
			var fn = _conditions.SecondCondition(0, GetTimeCoordinate(k));

			var N = _params.SpaceStepCount;

			var d = Enumerable
					.Range(0, _params.SpaceStepCount + 1)
					.Select(i =>
						(2 * _grid[i, k - 1] - _grid[i, k - 2]) / (tau * tau)
						+ g * _grid[i, k - 2] / (2 * tau)
						+ _equation.f(GetSpaceCoordinate(i), GetTimeCoordinate(k)))
					.ToArray();

			switch (_params.BoundaryApproximation)
			{
				case BoundaryApproximationType.FirstDegreeTwoPoints:
					d[0] = f0;
					d[N] = fn;

					matrix[0, 0] = betta - alpha / h;
					matrix[0, 1] = alpha / h;

					matrix[N, N - 1] = -gamma / h;
					matrix[N, N] = delta + gamma / h;

					break;
				case BoundaryApproximationType.SecondDegreeThreePoints:
					d[0] = f0;
					d[N] = fn;

					matrix[0, 0] = betta - 3 * alpha / (2 * h);
					matrix[0, 1] = 2 * alpha / h;
					matrix[0, 2] = (-alpha / (2 * h));

					var sim = matrix[0, 2] / matrix[1, 2];
					matrix[0, 0] -= sim * matrix[1, 0];
					matrix[0, 1] -= sim * matrix[1, 1];
					matrix[0, 2] -= sim * matrix[1, 2];
					d[0] -= sim * d[1];

					matrix[N, N - 2] = gamma / (2 * h);
					matrix[N, N - 1] = -2  *gamma / h;
					matrix[N, N] = delta + 3 * gamma / (2 * h);

					sim = matrix[N, N - 2] / matrix[N - 1, N - 2];
					matrix[N, N] -= sim * matrix[N - 1, N];
					matrix[N, N - 1] -= sim * matrix[N - 1, N - 1];
					matrix[N, N - 2] -= sim * matrix[N - 1, N - 2];
					d[N] -= sim * d[N - 1];

					break;
				case BoundaryApproximationType.SecondDegreeTwoPoints:
					var lam0 = (-2 * a / h - h / (tau * tau) - g * h / tau + c * h) / (2 * a - b * h);
					var ro0 = (- h / (tau * tau) * (_grid[0, k - 2] - 2 * _grid[0, k - 1])
						+ g * h * _grid[0, k - 1] / tau
						+ _equation.f(0, GetTimeCoordinate(k)) * h)
						/ (2 * a - b * h);

					var lam1 = (2 * a / h + h / (tau * tau) + g * h / tau - c * h) / (2 * a + b * h);
					var ro1 = (-2 * a * _grid[_params.SpaceStepCount - 1, k] / h
						+ h / (tau * tau) * (_grid[_params.SpaceStepCount, k - 2] - 2 * _grid[_params.SpaceStepCount, k - 1])
						- g * h * _grid[_params.SpaceStepCount, k - 1] / tau
						+ _equation.f(GetSpaceCoordinate(_params.SpaceStepCount), GetTimeCoordinate(k)) * h)
						/ (2 * a + b * h);

					d[0] = f0 - alpha * ro0;
					d[N] = fn - gamma * ro1;

					matrix[0, 0] = alpha * lam0 + betta;
					matrix[0, 1] = alpha * 2 * a / (h * (2 * a - b * h));

					matrix[N, N - 1] = -gamma * 2 * a / (h * (2 * a + b * h));
					matrix[N, N] = gamma * lam1 + delta;

					break;
			}

			return d;
		}*/

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
