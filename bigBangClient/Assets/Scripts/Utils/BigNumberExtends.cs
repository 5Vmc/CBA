using System;
using Babu.BigNumber;
using GameConfig;
using UnityEngine;

namespace Utils
{
    public static class BigNumberExtends
    {
        private static string GetUnitName(this BigNumber number)
        {
            var cfg = Configs.Unit.GetConfig(number.UnitId);
            if (cfg == null) return "";
            return cfg.UnitName;
        }

        public static string ToFormatString(this BigNumber number)
        {
            number.Format();
            if (number.UnitId == 0)
            {
                return $"{Math.Ceiling(number.Value):f0}";
            }
            else
            {
                return $"{number.Value:f3}{number.GetUnitName()}";
            }           
            // return string.Format("{0:#.###} {1}", number.Value, number.GetUnitName(), number.UnitId);
        }

        public static string ToFormatStrengthString(this BigNumber number)
        {
            number.Format();
            if (number.UnitId == 0)
            {
                return $"{Math.Ceiling(number.Value):f0}";
            }
            else
            {
                return $"{number.Value:f0}{number.GetUnitName()}";
            }
        }


    }


}