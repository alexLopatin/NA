using System;

namespace NumericMethods.Core.PartialDiffEquation.Splitting
{
	public class SplittingEquationParams
	{
		//dx
		public double a { get; set; }
		//dx
		public double b { get; set; }
		//u
		public double c { get; set; }
		//free
		public Func<double, double, double> f { get; set; }
	}
}
