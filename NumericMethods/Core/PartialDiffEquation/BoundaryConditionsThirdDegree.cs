using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.PartialDiffEquation
{
	public class BoundaryConditionsThirdDegree
	{
		public double[] FirstConditionParameters { get; set; } = Array.Empty<double>();
		public double[] SecondConditionParameters { get; set; } = Array.Empty<double>();

		public Func<double, double, double> FirstFunc { get; set; }
		public Func<double, double, double> SecondFunc { get; set; }
		public Func<double, double, double> InitialFunc { get; set; }
	}
}
