using System;
using System.Linq;
using NumericMethods.Core.LinearAlgebra;
using NumericMethods.Core.Numerics;

namespace NumericMethods.Core.PartialDiffEquation.Hyperbolic
{
	public class HyperbolicImplicitFiniteDifference
	{
		private readonly double[,] _grid;

		private readonly HyperbolicBoundaryConditions _conditions;
		private readonly HyperbolicFiniteDifferenceParams _params;
		private readonly HyperbolicEquationParams _equation;

		public HyperbolicImplicitFiniteDifference(
			HyperbolicBoundaryConditions conditions,
			HyperbolicEquationParams equation,
			HyperbolicFiniteDifferenceParams @params)
		{
			_params = @params;
			_conditions = conditions;
			_equation = equation;

			_grid = new double[_params.SpaceStepCount + 1, _params.TimeStepCount + 1];
			InitializeGrid();
		}

		private double GetSpaceCoordinate(int i)
		{
			return (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount * i + _params.SpaceBoundLeft;
		}

		private double GetTimeCoordinate(int i)
		{
			return _params.TimeLimit / _params.TimeStepCount * i;
		}

		private void InitializeGrid()
		{
			var d = _equation.d;
			var a = _equation.a;
			var b = _equation.b;
			var c = _equation.c;

			var tau = _params.TimeLimit / _params.TimeStepCount;
			Func<double, double> p1 = (x) => _conditions.InitialCondition(x, 0);
			Func<double, double> p2 = (x) => _conditions.DerivativeCondition(x, 0);
			var firstDerivative = Derivative.FindByFunction(p1, 1, _params.SpaceBoundLeft, _params.SpaceBoundRight, _params.SpaceStepCount);
			var secondDerivative = Derivative.FindByFunction(p1, 2, _params.SpaceBoundLeft, _params.SpaceBoundRight, _params.SpaceStepCount);

			for (int i = 0; i <= _params.SpaceStepCount; i++)
			{
				var x = GetSpaceCoordinate(i);
				_grid[i, 0] = p1(x);

				switch (_params.InitialApproximation)
				{
					case InitialApproximationType.FirstDegree:
						_grid[i, 1] = p1(x) + p2(x) * tau;
						break;
					case InitialApproximationType.SecondDegree:
						_grid[i, 1] = p1(x) + p2(x) * tau
							+ tau * tau / 2 * (a * secondDerivative[i] + b * firstDerivative[i] + c * p1(x) + _equation.f(x, 0) - d * p2(x));
						break;
				}
			}
		}

		public double[,] Solve()
		{
			var g = _equation.d;
			var a = _equation.a;
			var b = _equation.b;
			var c = _equation.c;

			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			var matrix = new Matrix(_params.SpaceStepCount + 1, _params.SpaceStepCount + 1);

			for (int i = 0; i < _params.SpaceStepCount; i++)
			{
				matrix[i, i] = 2 * a / (h * h) + 1 / (tau * tau) + g / (2 * tau) - c;
				matrix[i, i + 1] = -(a / (h * h) + b / (2 * h));
				matrix[i + 1, i] = -(a / (h * h) - b / (2 * h));
			}

			for (int k = 2; k <= _params.TimeStepCount; k++)
			{
				var d = SetBorderGrid(k, matrix);

				var newLayer = matrix.SolveTridiagonal(d);

				for (int i = 0; i <= _params.SpaceStepCount; i++)
				{
					_grid[i, k] = newLayer[i];
				}
			}

			return _grid.Clone() as double[,];
		}

		private double[] SetBorderGrid(int k, Matrix matrix)
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
		}

		public double[,] FindError(Func<double, double, double> u)
		{
			var errors = _grid.Clone() as double[,];

			for (int k = 0; k <= _params.TimeStepCount; k++)
			{
				for (int j = 0; j <= _params.SpaceStepCount; j++)
				{
					errors[j, k] = Math.Abs(_grid[j, k] - u(GetSpaceCoordinate(j), GetTimeCoordinate(k)));
				}
			}

			return errors;
		}
	}
}
