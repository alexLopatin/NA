namespace NumericMethods.Core.PartialDiffEquation.Splitting
{
	public class SplittingFiniteDifferenceParams
	{
		public double XBoundLeft { get; set; }
		public double XBoundRight { get; set; }

		public double YBoundLeft { get; set; }
		public double YBoundRight { get; set; }

		public double TimeLimit { get; set; }

		public int XStepCount { get; set; }
		public int YStepCount { get; set; }
		public int TimeStepCount { get; set; }

		public BoundaryApproximationType BoundaryApproximation { get; set; }
	}
}
