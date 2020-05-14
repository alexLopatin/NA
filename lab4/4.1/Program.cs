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
        static Func<double, double> ExactF;
        static double[] RungeCutta(double h)
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
                double K2 = h * f(x + h / 2, y[i - 1] + K1/2, z[i - 1] + L1 / 2);
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
            return y;
        }
        static (double[], double[], double[], double[]) RungeCuttaF(double h)
        {
            double[] z = new double[(int)Math.Round((b - a) / h) + 1];
            double[] y = new double[(int)Math.Round((b - a) / h) + 1];
            double[] f0 = new double[(int)Math.Round((b - a) / h)];
            double[] g0 = new double[(int)Math.Round((b - a) / h)];
            z[0] = yD0;
            y[0] = y0;
            double x = a;
            g0[0] = g(x, y[0], z[0]);
            f0[0] = f(x, y[0], z[0]);
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
                f0[i] = f(x, y[i - 1], z[i - 1]);
                g0[i] = g(x, y[i - 1], z[i - 1]);
            }
            return (f0, g0, y, z);
        }
        static double[] AdvancedEuler(double h)
        {
            double[] z = new double[(int)Math.Round((b - a) / h) + 1];
            double[] y = new double[(int)Math.Round((b - a) / h) + 1];
            z[0] = yD0;
            y[0] = y0;
            double x = a + h;
            for (int i = 1; i <= (b - a) / h; i++, x += h)
            {
                double zm = z[i - 1] + h / 2 * g(x, y[i - 1], z[i - 1]);
                z[i] = z[i - 1] + h * g(x + h / 2, y[i - 1], zm);
                y[i] = y[i - 1] + h * f(x, y[i], z[i]);
            }
            return y;
        }
        static double[] Euler(double h)
        {
            double[] z = new double[(int)Math.Round((b - a) / h) + 1];
            double[] y = new double[(int)Math.Round((b - a) / h) + 1];
            z[0] = yD0;
            y[0] = y0;
            double x = a;
            for (int i = 1; i <= (b - a) / h; i++, x += h)
            {
                z[i] = z[i - 1] + h * g(x, y[i - 1], z[i-1]);
                y[i] = y[i - 1] + h * f(x, y[i], z[i]);
            }
            return y;
        }
        static double[] Adams(double h, double[] f0, double[] g0, double[] z0, double[] y0)
        {
            double[] z = new double[(int)Math.Round((b - a) / h) + 1];
            double[] y = new double[(int)Math.Round((b - a) / h) + 1];
            double[] F = new double[(int)Math.Round((b - a) / h) + 1];
            double[] G = new double[(int)Math.Round((b - a) / h) + 1];
            Array.Copy(f0, 0, F, 0, f0.Length);
            Array.Copy(g0, 0, G, 0, g0.Length);
            Array.Copy(z0, 0, z, 0, z0.Length);
            Array.Copy(y0, 0, y, 0, y0.Length);
            double x = a + 3*h;
            for (int i = 4; i <= (b - a) / h; i++, x += h)
            {
                z[i] = z[i - 1] + h /24 * (55* G[i - 1] -59*G[i-2] +37* G[i - 3]-9* G[i - 4]);
                y[i] = y[i - 1] + h / 24 * (55 * F[i - 1] - 59 * F[i - 2] + 37 * F[i - 3] - 9 * F[i - 4]);
                F[i] = f(x, y[i-1], z[i-1]);
                G[i] = g(x, y[i - 1], z[i - 1]);
            }
            return y;
        }

        static void RungeRomberg(double[] yh, double[] y2h, double p)
        {
            Console.WriteLine("Runge-Romberg error:");
            for(int i = 0; i < y2h.Length; i++)
            {
                double error = (yh[i * 2] - y2h[i]) / (Math.Pow(2, p) - 1);
                Console.WriteLine(error);
            }
            Console.WriteLine();
        }

        static void Error(double[] yC, string name)
        {
            double[] y = new double[11];
            for (int i = 10; i <= 20; i++)
                y[i - 10] = ExactF(i / 10d);
            double[] e = new double[11];
            Console.WriteLine("{0} Error:", name);
            for (int i = 0; i <= 10; i++)
                e[i] = Math.Abs(y[i] - yC[i]);
            for (int i = 0; i <= 10; i++)
                Console.WriteLine(e[i]);
        }

        static void Main(string[] args)
        {
            double h = 0.1d;
            a = 1;
            b = 2d;
            y0 = 1;
            yD0 = 1;
            f = (x, y, z) => z;
            g = (x, y, z) => -z/x;
            ExactF = (x) => 1 + Math.Log(x);

            Error(Euler(h), "Euler");
            RungeRomberg(Euler(h), Euler(2 * h), 1);

            Error(AdvancedEuler(h), "Advanced Euler");
            RungeRomberg(AdvancedEuler(h), AdvancedEuler(2 * h), 2);

            Error(RungeCutta(h), "Runge-Cutta");
            RungeRomberg(RungeCutta(h), RungeCutta(2 * h), 4);

            {
                b = a + 4 * h;
                double[] f0, g0, y0, z0;
                (f0,g0, y0, z0)= RungeCuttaF(h);
                b = 2d;
                Error(Adams(h, f0, g0, z0, y0), "Adams");
                RungeRomberg(Adams(h, f0, g0, z0, y0), Adams(2 * h, f0, g0, z0, y0), 4);
            }
        }
    }
}