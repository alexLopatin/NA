namespace NumericMethods.Core.Common
{
	public struct Point
	{
		public double X;
		public double Y;

		public Point(double x, double y) => (X, Y) = (x, y);
		public static implicit operator Point((double, double) point) => new Point(point.Item1, point.Item2);
	}
}
