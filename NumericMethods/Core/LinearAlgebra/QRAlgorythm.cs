using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace NumericMethods.Core.LinearAlgebra
{
	class QRAlgorythm
	{
		Matrix A;
		double eps;

		public QRAlgorythm(Matrix a, double eps)
		{
			A = a;
			this.eps = eps;
			isComplex = new bool[a.Rows];
			complices = new Complex[a.Rows];
		}

		bool[] isComplex;
		Complex[] complices;

		public bool Check()
		{
			double res = 0;
			for (int i = 0; i < A.Rows - 1; i++)
				isComplex[i] = (Math.Abs(A[i + 1, i]) > eps);
			bool isAllGoodForComplices = false;
			//проверяем мнимые
			for (int i = 0; i < A.Rows; i++)
				if (isComplex[i])
				{
					double a = A[i, i];
					double b = A[i, i + 1];
					double c = A[i + 1, i + 1];
					double d = A[i + 1, i];
					Complex lambda = new Complex((a + c) / 2, 0);
					lambda += Complex.Sqrt((a + c) * (a + c) / 4 - a * c + b * d);
					if (lambda.Imaginary < eps)
						isComplex[i] = false;
					else
						isAllGoodForComplices = isAllGoodForComplices || Complex.Abs(lambda - complices[i]) > eps;
					complices[i] = lambda;
				}
			//проверяем вещественные корни
			for (int i = 0; i < A.Rows; i++)
				for (int j = 0; j < i; j++)
					if (!isComplex[j] || i != j + 1)
						res += A[i, j] * A[i, j];
			res = Math.Sqrt(res);
			return (res > eps) || isAllGoodForComplices;
		}

		public Complex[] Find()
		{
			A.Log("A");
			while (Check())
			{
				Matrix Q, R;
				(Q, R) = FindQR(A);
				A = R * Q;
				A.Log("A");
			}
			Complex[] res = new Complex[A.Rows];
			for (int i = 0; i < A.Rows; i++)
				if (isComplex[i])
				{
					res[i] = complices[i];
					res[i + 1] = Complex.Conjugate(complices[i]);
					i++;
				}
				else
					res[i] = A[i, i];
			return res;
		}

		private (Matrix, Matrix) FindQR(Matrix mat)
		{
			var copyMat = new Matrix(mat);
			var Q = Matrix.Identity(mat.Columns);
			for (int i = 0; i < mat.Columns - 1; i++)
			{
				Matrix H = Matrix.Identity(mat.Columns);
				Matrix v = new Matrix(mat.Rows, 1);
				for (int j = i; j < mat.Rows; j++)
					v[j] = copyMat[j, i];
				double vi = 0;
				for (int j = i; j < mat.Rows; j++)
					vi += copyMat[j, i] * copyMat[j, i];
				vi = Math.Sign(copyMat[i, i]) * Math.Sqrt(vi);
				v[i] += vi;
				H -= 2 * (v * v.Transpose()) / (double)(v.Transpose() * v);
				copyMat = H * copyMat;
				Q *= H;
			}
			return (Q, copyMat);
		}
	}
}
