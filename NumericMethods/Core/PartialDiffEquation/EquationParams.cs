using System;

namespace NumericMethods.Core.PartialDiffEquation
{
	public class EquationParams
	{
		//d^2 u
		public double a { get; set; }
		//d^1 u
		public double b { get; set; }
		//u
		public double c { get; set; }
		//free
		public Func<double, double, double> f { get; set; }
	}
}
