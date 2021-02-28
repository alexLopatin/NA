using System;
using System.Linq;
using NumericMethods.Core.LinearAlgebra;
using NumericMethods.Core.Numerics;

namespace NumericMethods.Core.PartialDiffEquation.Splitting
{
	public class AlternatingDirectionMethod
	{
		private readonly double[,,] _grid;

		private readonly SplittingBoundaryConditions _conditions;
		private readonly SplittingFiniteDifferenceParams _params;
		private readonly SplittingEquationParams _equation;

		public AlternatingDirectionMethod(
			SplittingBoundaryConditions conditions,
			SplittingEquationParams equation,
			SplittingFiniteDifferenceParams @params)
		{
			_params = @params;
			_conditions = conditions;
			_equation = equation;

			_grid = new double[_params.XStepCount + 1, _params.YStepCount + 1, _params.TimeStepCount + 1];
			InitializeGrid();
		}

		private double GetXCoordinate(int i)
		{
			return (_params.XBoundRight - _params.XBoundLeft) / _params.XStepCount * i + _params.XBoundLeft;
		}

		private double GetYCoordinate(int i)
		{
			return (_params.YBoundRight - _params.YBoundLeft) / _params.YStepCount * i + _params.YBoundLeft;
		}

		private double GetTimeCoordinate(double i)
		{
			return _params.TimeLimit / _params.TimeStepCount * i;
		}

		private void InitializeGrid()
		{
			for (int i = 0; i <= _params.XStepCount; i++)
			{
				for (int j = 0; j <= _params.YStepCount; j++)
				{
					_grid[i, j, 0] = _conditions.ZeroTimeCondition(GetXCoordinate(i), GetYCoordinate(j), 0);
				}
			}
		}

		public double[,,] Solve()
		{
			var a = _equation.a;
			var b = _equation.b;

			var hx = (_params.XBoundRight - _params.XBoundLeft) / _params.XStepCount;
			var hy = (_params.YBoundRight - _params.YBoundLeft) / _params.YStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			var alpha0 = _conditions.ConditionParameters[0, 0];
			var betta0 = _conditions.ConditionParameters[0, 1];

			var gamma0 = _conditions.ConditionParameters[1, 0];
			var delta0 = _conditions.ConditionParameters[1, 1];

			var alpha1 = _conditions.ConditionParameters[2, 0];
			var betta1 = _conditions.ConditionParameters[2, 1];

			var gamma1 = _conditions.ConditionParameters[3, 0];
			var delta1 = _conditions.ConditionParameters[3, 1];

			var firstMatrix = new Matrix(_params.XStepCount + 1, _params.XStepCount + 1);
			var firstD = new double[_params.XStepCount + 1];
			var halfStepLayer = new double[_params.XStepCount + 1, _params.YStepCount + 1];

			var secondMatrix = new Matrix(_params.YStepCount + 1, _params.YStepCount + 1);
			var secondD = new double[_params.YStepCount + 1];

			for (int i = 0; i < _params.XStepCount; i++)
			{
				firstMatrix[i, i] = -2 * a / (hx * hx) - 2 / tau;
				firstMatrix[i, i + 1] = a / (hx * hx);
				firstMatrix[i + 1, i] = a / (hx * hx);
			}

			for (int i = 0; i < _params.YStepCount; i++)
			{
				secondMatrix[i, i] = -2 * b / (hy * hy) - 2 / tau;
				secondMatrix[i, i + 1] = b / (hy * hy);
				secondMatrix[i + 1, i] = b / (hy * hy);
			}

			switch (_params.BoundaryApproximation)
			{
				case BoundaryApproximationType.FirstDegreeTwoPoints:
					var N = _params.XStepCount;
					firstMatrix[0, 0] = betta0 - alpha0 / hx;
					firstMatrix[0, 1] = alpha0 / hx;

					firstMatrix[N, N - 1] = -gamma0 / hx;
					firstMatrix[N, N] = delta0 + gamma0 / hx;

					N = _params.YStepCount;
					secondMatrix[0, 0] = betta1 - alpha1 / hy;
					secondMatrix[0, 1] = alpha1 / hx;

					secondMatrix[N, N - 1] = -gamma1 / hy;
					secondMatrix[N, N] = delta1 + gamma1 / hy;

					break;
				case BoundaryApproximationType.SecondDegreeThreePoints:
					N = _params.XStepCount;
					firstMatrix[0, 0] = betta0 - 3 * alpha0 / (2 * hx);
					firstMatrix[0, 1] = 2 * alpha0 / hx;
					firstMatrix[0, 2] = (-alpha0 / (2 * hx));

					var sim = firstMatrix[0, 2] / firstMatrix[1, 2];
					firstMatrix[0, 0] -= sim * firstMatrix[1, 0];
					firstMatrix[0, 1] -= sim * firstMatrix[1, 1];
					firstMatrix[0, 2] -= sim * firstMatrix[1, 2];
					//d[0] -= sim * d[1];

					firstMatrix[N, N - 2] = gamma0 / (2 * hx);
					firstMatrix[N, N - 1] = -2 * gamma0 / hx;
					firstMatrix[N, N] = delta0 + 3 * gamma0 / (2 * hx);

					sim = firstMatrix[N, N - 2] / firstMatrix[N - 1, N - 2];
					firstMatrix[N, N] -= sim * firstMatrix[N - 1, N];
					firstMatrix[N, N - 1] -= sim * firstMatrix[N - 1, N - 1];
					firstMatrix[N, N - 2] -= sim * firstMatrix[N - 1, N - 2];
					//d[N] -= sim * d[N - 1];

					N = _params.YStepCount;
					secondMatrix[0, 0] = betta1 - 3 * alpha1 / (2 * hy);
					secondMatrix[0, 1] = 2 * alpha1 / hy;
					secondMatrix[0, 2] = (-alpha1 / (2 * hy));

					sim = secondMatrix[0, 2] / secondMatrix[1, 2];
					secondMatrix[0, 0] -= sim * secondMatrix[1, 0];
					secondMatrix[0, 1] -= sim * secondMatrix[1, 1];
					secondMatrix[0, 2] -= sim * secondMatrix[1, 2];
					//d[0] -= sim * d[1];

					secondMatrix[N, N - 2] = gamma1 / (2 * hy);
					secondMatrix[N, N - 1] = -2 * gamma1 / hy;
					secondMatrix[N, N] = delta1 + 3 * gamma1 / (2 * hy);

					sim = secondMatrix[N, N - 2] / secondMatrix[N - 1, N - 2];
					secondMatrix[N, N] -= sim * secondMatrix[N - 1, N];
					secondMatrix[N, N - 1] -= sim * secondMatrix[N - 1, N - 1];
					secondMatrix[N, N - 2] -= sim * secondMatrix[N - 1, N - 2];
					//d[N] -= sim * d[N - 1];

					break;
				case BoundaryApproximationType.SecondDegreeTwoPoints:
					throw new NotImplementedException();
			}

			for (int k = 1; k <= _params.TimeStepCount; k++)
			{
				var halfStep = k - 0.5d;

				// первый шаг считаем k + 1/2 слой
				for (int j = 1; j < _params.YStepCount; j++)
				{
					SetFirstD(firstD, firstMatrix, j, k - 1);

					var newLayer = firstMatrix.SolveTridiagonal(firstD);

					for (int i = 0; i <= _params.XStepCount; i++)
					{
						halfStepLayer[i, j] = newLayer[i];
					}
				}

				for (int i = 0; i <= _params.XStepCount; i++)
				{
					var f0 = _conditions.InitialConditions[2](GetXCoordinate(i), GetYCoordinate(0), GetTimeCoordinate(halfStep));
					var fn = _conditions.InitialConditions[3](GetXCoordinate(i), GetYCoordinate(_params.YStepCount), GetTimeCoordinate(halfStep));

					switch (_params.BoundaryApproximation)
					{
						case BoundaryApproximationType.FirstDegreeTwoPoints:
							halfStepLayer[i, 0] = -(alpha1 / hy) / (betta1 - alpha1 / hy) * halfStepLayer[i, 1] +
								f0 / (betta1 - alpha1 / hy);
							halfStepLayer[i, _params.YStepCount] = (gamma1 / hy) / (delta1 + gamma1 / hy) * halfStepLayer[i, _params.YStepCount - 1] +
								fn / (delta1 + gamma1 / hy);
							break;
						case BoundaryApproximationType.SecondDegreeThreePoints:
							halfStepLayer[i, 0] = 1 / (-3 * alpha1 / (2 * hy) + betta1) *
								(f0 + alpha1 / (2 * hy) * (halfStepLayer[i, 2] - 4 * halfStepLayer[i, 1]));
							halfStepLayer[i, _params.YStepCount] = 1 / (3 * gamma1 / (2 * hy) + delta1) *
								(fn + gamma1 / (2 * hy) * (4 * halfStepLayer[i, _params.YStepCount - 1] - halfStepLayer[i, _params.YStepCount - 2]));
							break;
					}
				}

				//тут второй шаг считаем k + 1 слой
				for (int i = 1; i < _params.XStepCount; i++)
				{
					SetSecondD(secondD, secondMatrix, halfStepLayer, i, k - 1);

					var newLayer = secondMatrix.SolveTridiagonal(secondD);

					for (int j = 0; j <= _params.YStepCount; j++)
					{
						_grid[i, j, k] = newLayer[j];
					}
				}

				for (int j = 0; j <= _params.YStepCount; j++)
				{
					var f0 = _conditions.InitialConditions[0](GetXCoordinate(0), GetYCoordinate(j), GetTimeCoordinate(k));
					var fn = _conditions.InitialConditions[1](GetXCoordinate(_params.XStepCount), GetYCoordinate(j), GetTimeCoordinate(k));

					switch (_params.BoundaryApproximation)
					{
						case BoundaryApproximationType.FirstDegreeTwoPoints:
							_grid[0, j, k] = -(alpha0 / hx) / (betta0 - alpha0 / hx) * _grid[1, j, k] +
								f0 / (betta0 - alpha0 / hx);
							_grid[_params.XStepCount, j, k] = (gamma0 / hx) / (delta0 + gamma0 / hx) * _grid[_params.XStepCount - 1, j, k] +
								fn / (delta0 + gamma0 / hx);
							break;
						case BoundaryApproximationType.SecondDegreeThreePoints:
							_grid[0, j, k] = 1 / (-3 * alpha0 / (2 * hx) + betta0) *
								(f0 + alpha0 / (2 * hx) * (_grid[2, j, k] - 4 * _grid[1, j, k]));
							_grid[_params.XStepCount, j, k] = 1 / (3 * gamma0 / (2 * hx) + delta0) *
								(fn + gamma0 / (2 * hx) * (4 * _grid[_params.XStepCount - 1, j, k] - _grid[_params.XStepCount - 2, j, k]));
							break;
					}
				}
			}

			return _grid.Clone() as double[,,];
		}

		private void SetFirstD(double[] d, Matrix firstMatrix, int j, int k)
		{
			var a = _equation.a;
			var b = _equation.b;
			var c = _equation.c;

			var hx = (_params.XBoundRight - _params.XBoundLeft) / _params.XStepCount;
			var hy = (_params.YBoundRight - _params.YBoundLeft) / _params.YStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			d[0] = _conditions.InitialConditions[0](GetXCoordinate(0), GetYCoordinate(j), GetTimeCoordinate(k + 0.5d));
			d[_params.XStepCount] = _conditions.InitialConditions[1](GetXCoordinate(_params.XStepCount), GetYCoordinate(j), GetTimeCoordinate(k + 0.5d));

			for (int i = 1; i < _params.XStepCount; i++)
			{
				d[i] = -(2 * _grid[i, j, k] / tau
					+ b / (hy * hy) * (_grid[i, j + 1, k] - 2 * _grid[i, j, k] + _grid[i, j - 1, k])
					+ _equation.f(GetXCoordinate(i), GetYCoordinate(j), GetTimeCoordinate(k)));
			}

			var N = _params.XStepCount;
			var sim = firstMatrix[0, 2] / firstMatrix[1, 2];
			d[0] -= sim * d[1];
			sim = firstMatrix[N, N - 2] / firstMatrix[N - 1, N - 2];
			d[N] -= sim * d[N - 1];
		}

		private void SetSecondD(double[] d, Matrix secondMatrix, double[,] halfStep, int i, int k)
		{
			var a = _equation.a;
			var b = _equation.b;
			var c = _equation.c;

			var hx = (_params.XBoundRight - _params.XBoundLeft) / _params.XStepCount;
			var hy = (_params.YBoundRight - _params.YBoundLeft) / _params.YStepCount;
			var tau = _params.TimeLimit / _params.TimeStepCount;

			d[0] = _conditions.InitialConditions[2](GetXCoordinate(i), 0, GetTimeCoordinate(k + 1));
			d[_params.YStepCount] = _conditions.InitialConditions[3](GetXCoordinate(i), 0, GetTimeCoordinate(k + 1));

			for (int j = 1; j < _params.YStepCount; j++)
			{
				d[j] = -(2 * halfStep[i, j] / tau
					+ a / (hx * hx) * (halfStep[i + 1, j] - 2 * halfStep[i, j] + halfStep[i - 1, j])
					+ _equation.f(GetXCoordinate(i), GetYCoordinate(j), GetTimeCoordinate(k + 0.5d)));
			}

			var N = _params.YStepCount;
			var sim = secondMatrix[0, 2] / secondMatrix[1, 2];
			d[0] -= sim * d[1];
			sim = secondMatrix[N, N - 2] / secondMatrix[N - 1, N - 2];
			d[N] -= sim * d[N - 1];
		}

		public double[,,] FindError(Func<double, double, double, double> u)
		{
			var errors = _grid.Clone() as double[,,];

			for (int k = 0; k <= _params.TimeStepCount; k++)
			{
				for (int i = 0; i <= _params.XStepCount; i++)
				{
					for (int j = 0; j <= _params.YStepCount; j++)
					{
						errors[i, j, k] = Math.Abs(_grid[i, j, k] - u(GetXCoordinate(i), GetYCoordinate(j), GetTimeCoordinate(k)));
					}
				}
			}

			return errors;
		}
	}
}
