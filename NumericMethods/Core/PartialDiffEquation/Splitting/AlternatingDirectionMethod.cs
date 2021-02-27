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
			firstMatrix[0, 0] = 1;
			firstMatrix[0, 1] = 0;
			firstMatrix[_params.XStepCount, _params.XStepCount] = 1;
			firstMatrix[_params.XStepCount, _params.XStepCount - 1] = 0;

			for (int i = 0; i < _params.YStepCount; i++)
			{
				secondMatrix[i, i] = -2 * b / (hy * hy) - 2 / tau;
				secondMatrix[i, i + 1] = b / (hy * hy);
				secondMatrix[i + 1, i] = b / (hy * hy);
			}
			secondMatrix[0, 0] = 1;
			secondMatrix[0, 1] = 0;
			secondMatrix[_params.YStepCount, _params.YStepCount] = 1;
			secondMatrix[_params.YStepCount, _params.YStepCount - 1] = 0;

			for (int k = 1; k <= _params.TimeStepCount; k++)
			{
				var halfStep = k - 0.5d;

				// первый шаг считаем k + 1/2 слой
				for (int i = 0; i <= _params.XStepCount; i++)
				{
					halfStepLayer[i, 0] = _conditions.InitialConditions[2](GetXCoordinate(i), GetYCoordinate(0), GetTimeCoordinate(halfStep));
					halfStepLayer[i, _params.YStepCount] = _conditions.InitialConditions[3](GetXCoordinate(i), GetYCoordinate(_params.YStepCount), GetTimeCoordinate(halfStep));
				}

				for (int j = 1; j < _params.YStepCount; j++)
				{
					SetFirstD(firstD, j, k - 1);

					var newLayer = firstMatrix.SolveTridiagonal(firstD);

					for (int i = 0; i <= _params.XStepCount; i++)
					{
						halfStepLayer[i, j] = newLayer[i];
					}
				}

				//тут второй шаг считаем k + 1 слой
				for (int j = 0; j <= _params.YStepCount; j++)
				{
					_grid[0, j, k] = _conditions.InitialConditions[0](GetXCoordinate(0), GetYCoordinate(j), GetTimeCoordinate(k));
					_grid[_params.XStepCount, j, k] = _conditions.InitialConditions[1](GetXCoordinate(_params.XStepCount), GetYCoordinate(j), GetTimeCoordinate(k));
				}

				for (int i = 1; i < _params.XStepCount; i++)
				{
					SetSecondD(secondD, halfStepLayer, i, k - 1);

					var newLayer = secondMatrix.SolveTridiagonal(secondD);

					for (int j = 0; j <= _params.YStepCount; j++)
					{
						_grid[i, j, k] = newLayer[j];
					}
				}
			}

			return _grid.Clone() as double[,,];
		}

		private void SetFirstD(double[] d, int j, int k)
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
		}

		private void SetSecondD(double[] d, double[,] halfStep, int i, int k)
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
