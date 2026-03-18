using System;

namespace Babu.BigNumber
{
    public class BigNumberMath
    {
        public static BigNumber Pow(int x, int n)
        {
            return Pow(new BigNumber(x), n);
        }

        public static BigNumber Pow(double x, int n)
        {
            return Pow(new BigNumber(x), n);
        }

        public static BigNumber Pow(BigNumber x, int n)
        {
            BigNumber ret = new BigNumber(1);
            BigNumber tmp = new BigNumber(x.Value, x.UnitId);
            while (n != 0)
            {
                if ((n & 1) == 1)
                {
                    ret *= tmp;
                }

                n >>= 1;
                tmp *= tmp;
            }

            return ret;
        }

        public static double Log10(BigNumber x)
        {
            if (x == 0) return 0;
            x.Format();
            return x.UnitId * BigNumberConst.NUMBER_UNIT_10_POW + Math.Log10(x.Value);
        }

        public static BigNumber Sqrt(BigNumber x)
        {
            double value = x.Value;
            if (value < 0) return 0;
            int powCount = x.UnitId * BigNumberConst.NUMBER_UNIT_10_POW;
            if (powCount % 2 == 1 || powCount % 2 == -1)
            {
                value *= 10;
            }

            int retPowCount = (int) Math.Floor(powCount / 2.0);
            double retValue = Math.Sqrt(value);

            int diffPow = retPowCount % 3;
            retValue *= Math.Pow(10, diffPow);
            retPowCount -= diffPow;
            var ret = new BigNumber(retValue, retPowCount / BigNumberConst.NUMBER_UNIT_10_POW);
            return ret;
        }
    }
}