using System;

namespace NumericMethods.Core.PartialDiffEquation.Elliptical
{
	public class EllipticalEquationParams
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
