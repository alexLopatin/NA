using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static double a;
        static double b;
        static Func<double, double> f;
        static double Rectangles(double h)
        {
            double res = 0;
            for (double x = a; x < b; x += h)
                res += h * f(x + h / 2);
            return res;
        }
        static double Trapeze(double h)
        {
            double res = 0;
            for (double x = a; x < b; x += h)
                res += h * (f(x) + f(x + h)) / 2;
            return res;
        }
        static double Simpson(double h)
        {
            double res = 0;
            for (double x = a; x < b; x += h)
                res += h * (f(x) + f(x + h) + 4 * f(x + h / 2)) / 6;
            return res;
        }
        static double RungeRomberg(double Fh, double Fkh, double k, double p)
        {
            return (Fh - Fkh) / (Math.Pow(k, p) - 1);
        }
        static void Main(string[] args)
        {
            a = 0;
            b = 2;
            f = (x) => x / (x * x * x * x + 81);
            double h = 0.25d;
            double exact = 1 / 18 * Math.Atan(4 / 9);
            Console.WriteLine("Rectangles method: {0}", Rectangles(h));
            Console.WriteLine("Exact Error: {0}", Math.Abs(Rectangles(h) - exact));
            Console.WriteLine("Error: {0}", RungeRomberg(Rectangles(h), Rectangles(2 * h), 2, 4));
            Console.WriteLine();

            Console.WriteLine("Trapeze method: {0}", Trapeze(h));
            Console.WriteLine("Exact Error: {0}", Math.Abs(Trapeze(h) - exact));
            Console.WriteLine("Error: {0}", RungeRomberg(Trapeze(h), Trapeze(2 * h), 2, 4));
            Console.WriteLine();

            Console.WriteLine("Simpson method: {0}", Simpson(h));
            Console.WriteLine("Exact Error: {0}", Math.Abs(Simpson(h) - exact));
            Console.WriteLine("Error: {0}", RungeRomberg(Simpson(h), Simpson(2 * h), 2, 4));
            Console.WriteLine();
        }
    }
}