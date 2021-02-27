using System;

namespace NumericMethods.Core.PartialDiffEquation.Elliptical
{
	public class SplittingEquationParams
	{
		//dx2
		public double a { get; set; }
		//dy2
		public double b { get; set; }
		//free
		public Func<double, double, double, double> f { get; set; }
	}
}
