using System;
using System.Linq;
using NumericMethods.Core.LinearAlgebra;

namespace NumericMethods.Core.PartialDiffEquation.Parabolic
{
	public class CrankNikolsonMethod
	{
		private readonly ParabolicBoundaryConditions _conditions;
		private readonly FiniteDifferenceParams _params;
		private readonly ParabolicEquationParams _equation;

		private readonly double _weight;

		private double[,] _grid;

		public CrankNikolsonMethod(
			ParabolicBoundaryConditions conditions,
			ParabolicEquationParams equation,
			FiniteDifferenceParams @params,
			double weight = 0.5d)
		{
			_conditions = conditions;
			_params = @params;
			_equation = equation;
			_weight = weight;

			_grid = new double[_params.SpaceStepCount + 1, _params.TimeStepCount + 1];
			InitializeGrid();
		}

		private void InitializeGrid()
		{
			for (int i = 0; i <= _params.SpaceStepCount; i++)
			{
				var x = GetSpaceCoordinate(i);
				_grid[i, 0] = _conditions.InitialCondition(x, 0);
			}
		}

		private double GetSpaceCoordinate(int i)
		{
			return (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount * i + _params.SpaceBoundLeft;
		}

		private double GetTimeCoordinate(int i)
		{
			return _params.TimeLimit / _params.TimeStepCount * i;
		}

		public double[,] Solve()
		{
			var a = _equation.a;
			var b = _equation.b;
			var c = _equation.c;

			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			var matrix = new Matrix(_params.SpaceStepCount + 1, _params.SpaceStepCount + 1);

			for (int i = 0; i <= _params.SpaceStepCount - 1; i++)
			{
				matrix[i, i] = (2 * a / (h * h) - c) * _weight + 1 / tau;
				matrix[i, i + 1] = -(a / (h * h) + b / (2 * h)) * _weight;
				matrix[i + 1, i] = -(a / (h * h) - b / (2 * h)) * _weight;
			}

			for (int k = 1; k <= _params.TimeStepCount; k++)
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

			var d = new double[_params.SpaceStepCount + 1];

			for (int j = 1; j < N; j++)
			{
				d[j] = _grid[j, k - 1] / tau
					+ _equation.f(GetSpaceCoordinate(j), GetTimeCoordinate(k - 1))
					+ GetExplicitPart(j, k);
			}

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
					matrix[N, N - 1] = -2 * gamma / h;
					matrix[N, N] = delta + 3 * gamma / (2 * h);

					sim = matrix[N, N - 2] / matrix[N - 1, N - 2];
					matrix[N, N] -= sim * matrix[N - 1, N];
					matrix[N, N - 1] -= sim * matrix[N - 1, N - 1];
					matrix[N, N - 2] -= sim * matrix[N - 1, N - 2];
					d[N] -= sim * d[N - 1];

					break;
				case BoundaryApproximationType.SecondDegreeTwoPoints:
					var lam0 = (-2 * a / h - h / tau + c * h) / (2 * a - b * h);
					var ro0 = (h * _grid[0, k - 1] / tau
						+ _equation.f(0, GetTimeCoordinate(k)) * h)
						/ (2 * a - b * h);

					var lam1 = (2 * a / h + h / tau - c * h) / (2 * a + b * h);
					var ro1 = (-2 * a * _grid[_params.SpaceStepCount - 1, k] / h
						- 1 * h * _grid[_params.SpaceStepCount, k - 1] / tau
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

		private double GetExplicitPart(int j, int k)
		{
			var a = _equation.a;
			var b = _equation.b;
			var c = _equation.c;

			var h = (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount;

			return ((a / (h * h)) * _grid[j + 1, k - 1]
					+ -2 * (a / (h * h)) * _grid[j, k - 1]
					+ (a / (h * h)) * _grid[j - 1, k - 1]
					+ b * (_grid[j + 1, k - 1] - _grid[j - 1, k - 1]) / (2 * h)
					+ c * _grid[j, k - 1]) * (1 - _weight);
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
