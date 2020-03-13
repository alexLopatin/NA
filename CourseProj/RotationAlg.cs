using System;
using System.Collections.Generic;
using System.Text;

namespace NMCP
{
    class RotationAlg
    {
        private Matrix A;
        private double eps;
        private Matrix eigenVectorsRaw;
        public RotationAlg(Matrix inMat, double eps)
        {
            A = inMat;
            this.eps = eps;
            eigenVectorsRaw = Matrix.Identity(A.Rows);
        }
        private void Rotate()
        {
            int i, j = 0;
            (i, j) = A.FindMax((i, j) => i != j);
            double fi = 0.5 * Math.Atan(2 * A[i, j] / (A[i, i] - A[j, j]));
            SumRows(A, i, j, fi);
            SumColumns(A, i, j, fi);
            SumColumns(eigenVectorsRaw, i, j, fi);
        }
        private bool IsEnough()
        {
            double sum = 0;
            for (int i = 0; i < A.Rows; i++)
                for (int j = i + 1; j < A.Columns; j++)
                    sum += A[i, j] * A[i, j];
            sum = Math.Sqrt(sum);
            return sum < eps;
        }
        private void SumRows(Matrix m, int i, int j, double fi)
        {
            var iRow = m.RowReference(i);
            var jRow = m.RowReference(j);
            double[] tmp = new double[iRow.Length];
            iRow.CopyTo(tmp, 0);
            double sinFi = Math.Sin(fi);
            double cosFi = Math.Cos(fi);
            for (int k = 0; k < m.Columns; k++)
                iRow[k] = iRow[k] * cosFi + jRow[k] * sinFi;
            for (int k = 0; k < m.Columns; k++)
                jRow[k] = -tmp[k] * sinFi + jRow[k] * cosFi;
        }
        private void SumColumns(Matrix m, int i, int j, double fi)
        {
            var tmp = m.Column(i);
            double sinFi = Math.Sin(fi);
            double cosFi = Math.Cos(fi);
            for (int k = 0; k < A.Columns; k++)
                m[k, i] = m[k, i] * cosFi + m[k, j] * sinFi;
            for (int k = 0; k < A.Columns; k++)
                m[k, j] = -tmp[k] * sinFi + m[k, j] * cosFi;
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
                    for (int j = 0; j < eigenVectorsRaw.Columns; j++)
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
