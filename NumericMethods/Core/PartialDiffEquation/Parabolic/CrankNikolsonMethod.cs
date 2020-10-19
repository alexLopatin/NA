using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.PartialDiffEquation.Parabolic
{
	public class CrankNikolsonMethod
	{
		private readonly ParabolicImplicitFiniteDifference _finiteDifference;
		private readonly FiniteDifferenceParams _params;

		private readonly double _weight;

		//govno
		private double[,] _grid;

		public CrankNikolsonMethod(
			BoundaryConditionsThirdDegree conditions,
			FiniteDifferenceParams @params,
			double weight = 0.5d)
		{
			_finiteDifference = new ParabolicImplicitFiniteDifference(conditions, @params);
			_params = @params;
			_weight = weight;
		}

		public double[,] Solve(double[] coefs, Func<double, double, double> f)
		{
			var @explicit = _finiteDifference.Solve(coefs, f);
			var @implicit = _finiteDifference.Solve(coefs, f);

			_grid = @explicit.Clone() as double[,];

			for(int i = 0; i < _params.SpaceStepCount; i++)
			{
				for (int j = 0; j < _params.TimeStepCount; j++)
				{
					_grid[i, j] = _weight * @implicit[i, j] + (1 - _weight) * @explicit[i, j];
				}
			}

			return _grid;
		}

		private double GetSpaceCoordinate(int i)
		{
			return (_params.SpaceBoundRight - _params.SpaceBoundLeft) / _params.SpaceStepCount * i + _params.SpaceBoundLeft;
		}

		private double GetTimeCoordinate(int i)
		{
			return _params.TimeLimit / _params.TimeStepCount * i;
		}

		public double[,] FindError(Func<double, double, double> u)
		{
			var errors = _grid.Clone() as double[,];

			for (int k = 0; k < _params.TimeStepCount; k++)
			{
				for (int j = 0; j < _params.SpaceStepCount; j++)
				{
					errors[j, k] = Math.Abs(_grid[j, k] - u(GetSpaceCoordinate(j), GetTimeCoordinate(k)));
				}
			}

			return errors;
		}
	}
}
