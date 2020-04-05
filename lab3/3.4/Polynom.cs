using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace ConsoleApp1
{
    class Polynom : IEnumerable<KeyValuePair<int, double>>
    {
        Dictionary<int, double> coefs = new Dictionary<int, double>();
        public Polynom() { }
        public Polynom(double[] coefs)
        {
            for (int i = 0; i < coefs.Length; i++)
                this[i] = coefs[i];
        }
        public double this[int pow]
        {
            get
            {
                if (coefs.ContainsKey(pow))
                    return coefs[pow];
                else
                    return 0;
            }
            set { coefs[pow] = value; }
        }
        public IEnumerator<KeyValuePair<int, double>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<int, double>>)coefs).GetEnumerator();
        }
        public override string ToString()
        {
            string res = "";
            foreach (var pair in coefs)
                res += pair.Value.ToString("0.0000", CultureInfo.InvariantCulture) + ((pair.Key > 0) ? (" * x^" + pair.Key.ToString()) : ("")) + " + ";
            return res.Remove(res.Length - 3);
        }
        public double GetValue(double x)
        {
            double res = 0;
            foreach (var pair in coefs)
                res += Math.Pow(x, pair.Key) * pair.Value;
            return res;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<int, double>>)coefs).GetEnumerator();
        }
        public static Polynom operator *(Polynom a, Polynom b)
        {
            Polynom res = new Polynom();
            foreach (var pairA in a)
                foreach (var pairB in b)
                    res[pairA.Key + pairB.Key] += pairA.Value * pairB.Value;
            return res;
        }
        public static Polynom operator +(Polynom a, Polynom b)
        {
            Polynom res = new Polynom();
            foreach (var pairA in a)
                res[pairA.Key] = pairA.Value;
            foreach (var pairB in b)
                res[pairB.Key] += pairB.Value;
            return res;
        }
        public static Polynom operator *(double a, Polynom b)
        {
            Polynom res = new Polynom();
            foreach (var pairB in b)
                res[pairB.Key] = a * pairB.Value;
            return res;
        }
        public static Polynom operator *(Polynom b, double a)
        {
            Polynom res = new Polynom();
            foreach (var pairB in b)
                res[pairB.Key] = a * pairB.Value;
            return res;
        }
        public static Polynom operator /(Polynom b, double a)
        {
            Polynom res = new Polynom();
            foreach (var pairB in b)
                res[pairB.Key] = pairB.Value / a;
            return res;
        }
    }
}
