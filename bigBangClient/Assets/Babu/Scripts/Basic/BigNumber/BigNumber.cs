using System;

namespace Babu.BigNumber
{
    partial class BigNumberConst
    {
        public const int NUMBER_UNIT_INTERVAL = 1000;
        public const int NUMBER_UNIT_10_POW = 3;
    }
    public class BigNumber
    {
        public double Value { get; set; }
        public int UnitId { get; set; }
        
        public BigNumber()
        {
            Value = 0;
            UnitId = 0;
        }

        public BigNumber(int value)
        {
            Value = value;
            UnitId = 0;
            Format();
        }

        public BigNumber(double value)
        {
            Value = value;
            UnitId = 0;
            Format();
        }

        public BigNumber(double value, int unitId)
        {
            Value = value;
            UnitId = unitId;
            Format();
        }

        public void Format()
        {
            if (Value > 0)
            {
                while (Value >= BigNumberConst.NUMBER_UNIT_INTERVAL)
                {
                    Value /= BigNumberConst.NUMBER_UNIT_INTERVAL;
                    UnitId++;
                }

                while (Value * BigNumberConst.NUMBER_UNIT_INTERVAL < 1)
                {
                    Value *= BigNumberConst.NUMBER_UNIT_INTERVAL;
                    UnitId--;
                }

                while (Value < 1)
                {
                    Value *= BigNumberConst.NUMBER_UNIT_INTERVAL;
                    UnitId--;
                }
            }
            else if (Value < 0)
            {
                while (Value <= -BigNumberConst.NUMBER_UNIT_INTERVAL)
                {
                    Value /= BigNumberConst.NUMBER_UNIT_INTERVAL;
                    UnitId++;
                }

                while (Value * BigNumberConst.NUMBER_UNIT_INTERVAL > -1)
                {
                    Value *= BigNumberConst.NUMBER_UNIT_INTERVAL;
                    UnitId--;
                }

                while (Value > -1)
                {
                    Value *= BigNumberConst.NUMBER_UNIT_INTERVAL;
                    UnitId--;
                }
            }
            else
            {
                Value = 0;
                UnitId = 0;
            }
        }

        public double ToDouble()
        {
            var c = Value;
            var u = UnitId;
            while (u > 0)
            {
                c *= BigNumberConst.NUMBER_UNIT_INTERVAL;
                u--;
            }

            while (u < 0)
            {
                c /= BigNumberConst.NUMBER_UNIT_INTERVAL;
                u++;
            }

            return c;
        }
        
        public static implicit operator BigNumber(double value)
        {
            return new BigNumber(value);
        }

        public static implicit operator BigNumber(int value)
        {
            return new BigNumber(value);
        }
        
        #region Overriding +

        private static BigNumber AdditionNotSameUnit(BigNumber max, BigNumber min)
        {
            //精度
            BigNumber ret = new BigNumber();
            var dis = max.UnitId - min.UnitId;
            if (dis >= 5) return max;
            double minValue = min.Value;
            while (dis > 0)
            {
                minValue /= BigNumberConst.NUMBER_UNIT_INTERVAL;
                dis--;
            }

            ret.UnitId = max.UnitId;
            ret.Value = max.Value + minValue;
            ret.Format();
            return ret;
        }

        public static BigNumber operator +(BigNumber a, BigNumber b)
        {
            if (a.UnitId == b.UnitId)
            {
                BigNumber ret = new BigNumber(a.Value + b.Value, a.UnitId);
                ret.Format();
                return ret;
            }
            else if (a.UnitId > b.UnitId)
            {
                return AdditionNotSameUnit(a, b);
            }
            else
            {
                return AdditionNotSameUnit(b, a);
            }
        }

        public static BigNumber operator +(BigNumber a, int b)
        {
            BigNumber n = new BigNumber(b);
            return a + n;
        }

        public static BigNumber operator +(BigNumber a, double b)
        {
            BigNumber n = new BigNumber(b);
            return a + n;
        }

        #endregion

        #region Overriding -

        public static BigNumber operator -(BigNumber a, int b)
        {
            BigNumber n = new BigNumber(b);
            return a - n;
        }

        public static BigNumber operator -(BigNumber a, double b)
        {
            BigNumber n = new BigNumber(b);
            return a - n;
        }

        public static BigNumber operator -(BigNumber a, BigNumber b)
        {
            BigNumber tmp = new BigNumber(-b.Value, b.UnitId);
            return a + tmp;
        }

        #endregion

        #region Overriding *

        public static BigNumber operator *(BigNumber a, int b)
        {
            BigNumber n = new BigNumber(b);
            return a * n;
        }

        public static BigNumber operator *(BigNumber a, double b)
        {
            BigNumber n = new BigNumber(b);
            return a * n;
        }

        public static BigNumber operator *(BigNumber a, BigNumber b)
        {
            BigNumber ret = new BigNumber(a.Value, a.UnitId);
            ret.Value *= b.Value;
            ret.UnitId += b.UnitId;
            ret.Format();
            return ret;
        }

        #endregion

        #region Overriding /

        public static BigNumber operator /(BigNumber a, int b)
        {
            BigNumber n = new BigNumber(b);
            return a / n;
        }

        public static BigNumber operator /(BigNumber a, double b)
        {
            BigNumber n = new BigNumber(b);
            return a / n;
        }

        public static BigNumber operator /(BigNumber a, BigNumber b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException();
            }
            BigNumber ret = new BigNumber(a.Value, a.UnitId);
            ret.Value /= b.Value;
            ret.UnitId -= b.UnitId;
            ret.Format();
            return ret;
        }

        #endregion

        #region Overriding cmp

        public static bool operator ==(BigNumber a, int b)
        {
            return a == new BigNumber(b);
        }

        public static bool operator ==(BigNumber a, double b)
        {
            return a == new BigNumber(b);
        }

        public static bool operator ==(BigNumber a, BigNumber b)
        {
            if (a is null || b is null) return a is null && b is null;
            a.Format();
            b.Format();
            return a.Value.CompareTo(b.Value) == 0 && b.UnitId == a.UnitId;
        }

        public static bool operator !=(BigNumber a, int b)
        {
            return a != new BigNumber(b);
        }

        public static bool operator !=(BigNumber a, double b)
        {
            return a != new BigNumber(b);
        }
        public static bool operator !=(BigNumber a, BigNumber b)
        {
            return !(a == b);
        }

        public static bool operator >(BigNumber a, int b)
        {
            return a > new BigNumber(b);
        }

        public static bool operator >(BigNumber a, double b)
        {
            return a > new BigNumber(b);
        }

        public static bool operator >(BigNumber a, BigNumber b)
        {
            if (a.UnitId == b.UnitId) return a.Value > b.Value;
            else if (a.Value == 0) return b.Value < 0;
            else if (b.Value == 0) return a.Value > 0;
            else if (a.Value > 0 && b.Value > 0) return a.UnitId > b.UnitId;
            else if (a.Value > 0 && b.Value < 0) return true;
            else if (a.Value < 0 && b.Value > 0) return false;
            else if (a.Value < 0 && b.Value < 0) return a.UnitId < b.UnitId;
            else return false;
        }


        public static bool operator >=(BigNumber a, int b)
        {
            return a >= new BigNumber(b);
        }

        public static bool operator >=(BigNumber a, double b)
        {
            return a >= new BigNumber(b);
        }

        public static bool operator >=(BigNumber a, BigNumber b)
        {
            return !(a < b);
        }

        public static bool operator <(BigNumber a, int b)
        {
            return a < new BigNumber(b);
        }

        public static bool operator <(BigNumber a, double b)
        {
            return a < new BigNumber(b);
        }

        public static bool operator <(BigNumber a, BigNumber b)
        {
            if (a.UnitId == b.UnitId) return a.Value < b.Value;
            else if (a.Value == 0) return b.Value > 0;
            else if (b.Value == 0) return a.Value < 0;
            else if (a.Value > 0 && b.Value > 0) return a.UnitId < b.UnitId;
            else if (a.Value > 0 && b.Value < 0) return false;
            else if (a.Value < 0 && b.Value > 0) return true;
            else if (a.Value < 0 && b.Value < 0) return a.UnitId > b.UnitId;
            else return false;
        }


        public static bool operator <=(BigNumber a, int b)
        {
            return a <= new BigNumber(b);
        }

        public static bool operator <=(BigNumber a, double b)
        {
            return a <= new BigNumber(b);
        }

        public static bool operator <=(BigNumber a, BigNumber b)
        {
            return !(a > b);
        }

        #endregion
        
        public override string ToString()
        {
            return $"{{{Value} {UnitId}}}";
        }

        public override bool Equals(object obj)
        {
            return obj is BigNumber other && this == other;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, UnitId);
        }

        public BigNumber Clone()
        {
            return new BigNumber(this.Value, this.UnitId);
        }
    }
}
