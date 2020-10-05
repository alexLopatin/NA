using System;
using System.Collections.Generic;
using System.Text;

namespace NumericMethods.Core.LinearAlgebra
{
	class RotationSolver
	{
		private Matrix A;
		private double eps;
		private Matrix eigenVectorsRaw;
		public RotationSolver(Matrix inMat, double eps)
		{
			A = inMat;
			this.eps = eps;
			eigenVectorsRaw = Matrix.Identity(A.Rows);
		}
		public void Rotate()
		{
			Matrix U = Matrix.Identity(A.Rows);

			int i, j = 0;
			(i, j) = A.FindMax((i, j) => i != j);
			double fi = 0.5 * Math.Atan(2 * A[i, j] / (A[i, i] - A[j, j]));

			U[i, i] = Math.Cos(fi);
			U[i, j] = -Math.Sin(fi);
			U[j, j] = Math.Cos(fi);
			U[j, i] = Math.Sin(fi);

			A = U.Transpose() * A * U;
			A.Log("A");

			eigenVectorsRaw = eigenVectorsRaw * U;
		}
		public bool IsEnough()
		{
			double sum = 0;

			for (int i = 0; i < A.Rows; i++)
				for (int j = i + 1; j < A.Columns; j++)
					sum += A[i, j] * A[i, j];

			sum = Math.Sqrt(sum);

			return sum < eps;
		}

		public void Calculate()
		{
			while (!IsEnough())
				Rotate();
		}

		public double[] EigenValues
		{
			get
			{
				double[] res = new double[A.Rows];

				for (int i = 0; i < A.Rows; i++)
					res[i] = A[i, i];

				return res;
			}
		}
		public List<double[]> EigenVectors
		{
			get
			{
				List<double[]> res = new List<double[]>();

				for (int i = 0; i < eigenVectorsRaw.Rows; i++)
				{
					double[] eigenVector = new double[eigenVectorsRaw.Rows];
					for (int j = 0; j < eigenVectorsRaw.Rows; j++)
						eigenVector[j] = eigenVectorsRaw[j, i];
					res.Add(eigenVector);
				}

				return res;
			}
		}
		public string GetRawResult()
		{
			var eigenValues = EigenValues;
			var eigenVectors = EigenVectors;
			var outTxt = "";

			foreach (var eigenValue in eigenValues)
				outTxt += eigenValue.ToString() + " ";

			outTxt += "\n";

			foreach (var eigenVector in eigenVectors)
			{
				for (int i = 0; i < eigenVector.Length; i++)
					outTxt += eigenVector[i].ToString() + " ";
				outTxt += "\n";
			}

			return outTxt;
		}
	}
}
