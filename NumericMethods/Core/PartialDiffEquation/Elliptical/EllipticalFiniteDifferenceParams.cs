namespace NumericMethods.Core.PartialDiffEquation.Elliptical
{
	public class EllipticalFiniteDifferenceParams
	{
		public double XBoundLeft { get; set; }
		public double XBoundRight { get; set; }

		public double YBoundLeft { get; set; }
		public double YBoundRight { get; set; }

		public int XStepCount { get; set; }
		public int YStepCount { get; set; }

		public BoundaryApproximationType BoundaryApproximation { get; set; }

		public SolverType Solver { get; set; }
		public double Eps { get; set; }
	}
}
