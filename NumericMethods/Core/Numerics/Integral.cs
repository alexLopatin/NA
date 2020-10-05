using System;

namespace NumericMethods.Core.Numerics
{
	public class Integral
	{
		private readonly double _left;
		private readonly double _right;
		private readonly Func<double, double> _function;

		public Integral(double left, double right, Func<double, double> function)
		{
			_left = left;
			_right = right;
			_function = function;
		}

		public double Rectangles(double h)
		{
			double res = 0;
			for (double x = _left; x < _right; x += h)
				res += h * _function(x + h / 2);
			return res;
		}

		public double Trapeze(double h)
		{
			double res = 0;
			for (double x = _left; x < _right; x += h)
				res += h * (_function(x) + _function(x + h)) / 2;
			return res;
		}

		public double Simpson(double h)
		{
			double res = 0;
			for (double x = _left; x < _right; x += h)
				res += h * (_function(x) + _function(x + h) + 4 * _function(x + h / 2)) / 6;
			return res;
		}
		public double RungeRomberg(double Fh, double Fkh, double k, double p)
		{
			return (Fh - Fkh) / (Math.Pow(k, p) - 1);
		}
	}
}
