using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static double y0;
        static double yD0;
        static double a;
        static double b;
        static Func<double, double, double, double> f;
        static Func<double, double, double, double> g;
        static Func<double, double> p;
        static Func<double, double> q;
        static Func<double, double> ExactF;
        static (double[], double[]) RungeCutta(double h)
        {
            double[] z = new double[(int)Math.Round((b - a) / h) + 1];
            double[] y = new double[(int)Math.Round((b - a) / h) + 1];
            z[0] = yD0;
            y[0] = y0;
            double x = a;
            for (int i = 1; i <= (b - a) / h; i++, x += h)
            {
                double K1 = h * f(x, y[i - 1], z[i - 1]);
                double L1 = h * g(x, y[i - 1], z[i - 1]);
                double K2 = h * f(x + h / 2, y[i - 1] + K1 / 2, z[i - 1] + L1 / 2);
                double L2 = h * g(x + h / 2, y[i - 1] + K1 / 2, z[i - 1] + L1 / 2);
                double K3 = h * f(x + h / 2, y[i - 1] + K2 / 2, z[i - 1] + L2 / 2);
                double L3 = h * g(x + h / 2, y[i - 1] + K2 / 2, z[i - 1] + L2 / 2);
                double K4 = h * f(x + h, y[i - 1] + K3, z[i - 1] + L3);
                double L4 = h * g(x + h, y[i - 1] + K3, z[i - 1] + L3);

                double dy = (K1 + 2 * K2 + 2 * K3 + K4) / 6;
                double dz = (L1 + 2 * L2 + 2 * L3 + L4) / 6;
                y[i] = y[i - 1] + dy;
                z[i] = z[i - 1] + dz;
            }
            return (y, z);
        }
        static void RungeRomberg(double[] yh, double[] y2h, double p)
        {
            Console.WriteLine("Runge-Romberg error:");
            for (int i = 0; i < y2h.Length; i++)
            {
                double error = (yh[i * 2] - y2h[i]) / (Math.Pow(2, p) - 1);
                Console.WriteLine(error);
            }
            Console.WriteLine();
        }
        static void Error(double[] yC, string name, double h = 0.1d)
        {
            double[] y = new double[1 + (int)((b - a) / h)];
            for (int i = 0; i < 1 + (int)((b - a) / h); i++)
                y[i] = ExactF(i * h);
            double[] e = new double[1 + (int)((b - a) / h)];
            Console.WriteLine("{0} Error:", name);
            for (int i = 0; i < 1 + (int)((b - a) / h); i++)
                e[i] = Math.Abs(y[i] - yC[i]);
            for (int i = 0; i < 1 + (int)((b - a) / h); i++)
                Console.WriteLine(e[i]);
        }
        static double[] Shooting(double h, double n0, double n1, double eps)
        {
            List<double> n = new List<double>();
            List<double> fi = new List<double>();
            n.Add(n0);
            n.Add(n1);
            y0 = n0;
            double[] y, z;
            (y, z) = RungeCutta(h);
            fi.Add(z[z.Length - 1] - y[y.Length - 1] - 1);
            y0 = n1;
            (y, z) = RungeCutta(h);
            fi.Add(z[z.Length - 1] - y[y.Length - 1] - 1);
            while (Math.Abs(fi.Last()) > eps)
            {
                n.Add(n[n.Count - 1] - (n[n.Count - 1] - n[n.Count - 2]) * fi[n.Count - 1] / (fi[n.Count - 1] - fi[n.Count - 2]));
                y0 = n[n.Count - 1];
                (y, z) = RungeCutta(h);
                fi.Add(z[z.Length - 1] - y[y.Length - 1] - 1);
            }
            return y;
        }
        static double[] FiniteDifference(double h)
        {
            Matrix mat = new Matrix((int)((b - a) / h + 1), (int)((b - a) / h) + 1);

            double[] x = new double[(int)((b - a) / h) + 1];
            x[0] = a + h;
            for (int i = 1; i < x.Length; i++)
                x[i] = x[i - 1] + h;
            for (int i = 0; i < mat.Columns; i++)
                mat[i, i] = -2 + h * h * q(x[i]);
            for (int i = 0; i < mat.Columns - 1; i++)
                mat[i, i + 1] = 1 + h * p(x[i]) / 2;
            for (int i = 1; i < mat.Columns; i++)
                mat[i, i - 1] = 1 - h * p(x[i]) / 2;
            double[] d = new double[mat.Rows];
            d[0] = 0;
            d[mat.Rows - 1] = -h;
            mat[mat.Rows - 1, mat.Rows - 1] = h - 1;
            mat[mat.Rows - 1, mat.Rows - 2] = 1;
            mat.Log("mat");
            double[] y = mat.SolveTridiagonal(d);
            double[] res = new double[mat.Rows];
            Array.Copy(y, 0, res, 1, mat.Rows - 1);
            return res;
        }

        static void Main(string[] args)
        {
            double h = 0.1d;
            a = 0;
            b = 1;
            yD0 = 1;
            f = (x, y, z) => z;
            g = (x, y, z) => (2 * z + y * Math.Exp(x)) / (Math.Exp(x) + 1);
            ExactF = (x) => Math.Exp(x) - 1;
            p = (x) => (-2 / (Math.Exp(x) + 1));
            q = (x) => (-Math.Exp(x) / (Math.Exp(x) + 1));

            var y = Shooting(h, 1.0d, 0.8d, 0.001d);
            Error(y, "Shooting");
            RungeRomberg(y, Shooting(2 * h, 1.0d, 0.8d, 0.001d), 4);
            h /= 4;
            y = FiniteDifference(h);
            Error(y, "Finite Difference", h);
            RungeRomberg(y, FiniteDifference(2 * h), 1);

        }
    }
}