using System;
using System.Linq;
using NumericMethods.Core.LinearAlgebra;

namespace NumericMethods.Core.PartialDiffEquation.Elliptical
{
	public class EllipticalFiniteDifference
	{
		private readonly double[,] _grid;

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

			_grid = new double[_params.XStepCount + 1, _params.YStepCount + 1];
		}

		private double GetXCoordinate(int i)
		{
			return (_params.XBoundRight - _params.XBoundLeft) / _params.XStepCount * i + _params.XBoundLeft;
		}

		private double GetYCoordinate(int i)
		{
			return (_params.YBoundRight - _params.YBoundLeft) / _params.YStepCount * i + _params.YBoundLeft;
		}

		private int Linearize(int i, int j)
		{
			return i + j * (_params.XStepCount + 1);
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

			var matrix = new Matrix(dim, dim);
			var d = new Matrix(dim, 1);

			for (int j = 1; j < _params.YStepCount; j++)
			{
				var N = _params.XStepCount;
				var f0 = _conditions.InitialConditions[0](0, GetYCoordinate(j));
				var fn = _conditions.InitialConditions[1](0, GetYCoordinate(j));

				var alpha = _conditions.ConditionParameters[0, 0];
				var betta = _conditions.ConditionParameters[0, 1];

				var gamma = _conditions.ConditionParameters[1, 0];
				var delta = _conditions.ConditionParameters[1, 1];

				switch (_params.BoundaryApproximation)
				{
					case BoundaryApproximationType.FirstDegreeTwoPoints:
						d[Linearize(0, j)] = f0;
						d[Linearize(N, j)] = fn;

						matrix[Linearize(0, j), Linearize(0, j)] = betta - alpha / h1;
						matrix[Linearize(0, j), Linearize(1, j)] = alpha / h1;

						matrix[Linearize(N, j), Linearize(N - 1, j)] = -gamma / h1;
						matrix[Linearize(N, j), Linearize(N, j)] = delta + gamma / h1;

						break;
					case BoundaryApproximationType.SecondDegreeThreePoints:
						d[Linearize(0, j)] = f0;
						d[Linearize(N, j)] = fn;

						matrix[Linearize(0, j), Linearize(0, j)] = betta - 3 * alpha / (2 * h1);
						matrix[Linearize(0, j), Linearize(1, j)] = 2 * alpha / h1;
						matrix[Linearize(0, j), Linearize(2, j)] = (-alpha / (2 * h1));

						matrix[Linearize(N, j), Linearize(N - 2, j)] = gamma / (2 * h1);
						matrix[Linearize(N, j), Linearize(N - 1, j)] = -2 * gamma / h1;
						matrix[Linearize(N, j), Linearize(N, j)] = delta + 3 * gamma / (2 * h1);

						break;
				}
			}

			for (int i = 0; i <= _params.XStepCount; i++)
			{
				var N = _params.YStepCount;
				var f0 = _conditions.InitialConditions[2](GetXCoordinate(i), 0);
				var fn = _conditions.InitialConditions[3](GetXCoordinate(i), 0);

				var alpha = _conditions.ConditionParameters[2, 0];
				var betta = _conditions.ConditionParameters[2, 1];

				var gamma = _conditions.ConditionParameters[3, 0];
				var delta = _conditions.ConditionParameters[3, 1];

				switch (_params.BoundaryApproximation)
				{
					case BoundaryApproximationType.FirstDegreeTwoPoints:
						d[Linearize(i, 0)] = f0;
						d[Linearize(i, N)] = fn;

						matrix[Linearize(i, 0), Linearize(i, 0)] = betta - alpha / h2;
						matrix[Linearize(i, 0), Linearize(i, 1)] = alpha / h2;

						matrix[Linearize(i, N), Linearize(i, N - 1)] = -gamma / h2;
						matrix[Linearize(i, N), Linearize(i, N)] = delta + gamma / h2;

						break;
					case BoundaryApproximationType.SecondDegreeThreePoints:
						d[Linearize(i, 0)] = f0;
						d[Linearize(i, N)] = fn;

						matrix[Linearize(i, 0), Linearize(i, 0)] = betta - 3 * alpha / (2 * h2);
						matrix[Linearize(i, 0), Linearize(i, 1)] = 2 * alpha / h2;
						matrix[Linearize(i, 0), Linearize(i, 2)] = (-alpha / (2 * h2));

						matrix[Linearize(i, N), Linearize(i, N - 2)] = gamma / (2 * h2);
						matrix[Linearize(i, N), Linearize(i, N - 1)] = -2 * gamma / h2;
						matrix[Linearize(i, N), Linearize(i, N)] = delta + 3 * gamma / (2 * h2);
						break;
				}
			}

			for (int i = 1; i < _params.XStepCount; i++)
			{
				for (int j = 1; j < _params.YStepCount; j++)
				{
					matrix[Linearize(i, j), Linearize(i, j)] = -2 / (h1 * h1) - 2 / (h2 * h2) - c;
					matrix[Linearize(i, j), Linearize(i - 1, j)] = 1 / (h1 * h1) + a / h1;
					matrix[Linearize(i, j), Linearize(i + 1, j)] = 1 / (h1 * h1) - a / h1;
					matrix[Linearize(i, j), Linearize(i, j - 1)] = 1 / (h2 * h2) + b / h2;
					matrix[Linearize(i, j), Linearize(i, j + 1)] = 1 / (h2 * h2) - b / h2;

					d[Linearize(i, j)] = _equation.f(GetXCoordinate(i), GetYCoordinate(j));
				}
			}

			var solver = new IterationSolver(matrix, d, _params.Eps);

			switch (_params.Solver)
			{
				case SolverType.Libman:
					solver.Solve();
					break;
				case SolverType.Zeidel:
					solver.ZeidelSolve();
					break;
				case SolverType.OverRelaxation:
					solver.OverRelaxation();
					break;
			}

			//solver.X.Log("x");

			for (int i = 0; i < _params.XStepCount + 1; i++)
			{
				for (int j = 0; j < _params.YStepCount + 1; j++)
				{
					_grid[i, j] = solver.X[Linearize(i, j)];
				}
			}

			return _grid;
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

		public double[,] FindError(Func<double, double, double> u)
		{
			var errors = _grid.Clone() as double[,];

			for (int k = 0; k <= _params.YStepCount; k++)
			{
				for (int j = 0; j <= _params.XStepCount; j++)
				{
					errors[j, k] = Math.Abs(_grid[j, k] - u(GetXCoordinate(j), GetYCoordinate(k)));
				}
			}

			return errors;
		}
	}
}
