using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Babu
{
    public enum CBAColor
    {
        White = 0,
        Green = 1,
        Blue = 2,
        Purple = 3,
        Orange = 4,
        Red = 5,
        Gold = 6,
        DarkGold = 7
    }

    public class CBAColorUtil
    {
        private static CBAColorUtil _instance;
        private Dictionary<int, Color> dict;
        private Dictionary<CBAColor, string> hexDict;
        private string[] qualityname = { "白","绿","蓝","紫","橙","红","金","暗金" };
        public static CBAColorUtil Instance
        {
            get
            {
                if (_instance == null) _instance = new CBAColorUtil();
                return _instance;
            }
        }

        private CBAColorUtil() {
            hexDict = new Dictionary<CBAColor, string>();
            hexDict.Add(CBAColor.White, "#FFFFFF");
            hexDict.Add(CBAColor.Green, "#53F143");
            hexDict.Add(CBAColor.Blue, "#44DEEC");
            hexDict.Add(CBAColor.Purple, "#D258FF");
            hexDict.Add(CBAColor.Orange, "#EA8519");
            hexDict.Add(CBAColor.Red, "#F23C29");
            hexDict.Add(CBAColor.Gold, "#FEE17E");
            hexDict.Add(CBAColor.DarkGold, "#FFC766");

            dict = new Dictionary<int, Color>();
            ColorUtility.TryParseHtmlString(hexDict[CBAColor.White], out Color white);
            dict.Add((int)CBAColor.White, white);

            ColorUtility.TryParseHtmlString(hexDict[CBAColor.Green], out Color green);
            dict.Add((int)CBAColor.Green, green);

            ColorUtility.TryParseHtmlString(hexDict[CBAColor.Blue], out Color blue);
            dict.Add((int)CBAColor.Blue, blue);

            ColorUtility.TryParseHtmlString(hexDict[CBAColor.Purple], out Color purple);
            dict.Add((int)CBAColor.Purple, purple);

            ColorUtility.TryParseHtmlString(hexDict[CBAColor.Orange], out Color orange);
            dict.Add((int)CBAColor.Orange, orange);

            ColorUtility.TryParseHtmlString(hexDict[CBAColor.Red], out Color red);
            dict.Add((int)CBAColor.Red, red);

            ColorUtility.TryParseHtmlString(hexDict[CBAColor.Gold], out Color gold);
            dict.Add((int)CBAColor.Gold, gold);

            ColorUtility.TryParseHtmlString(hexDict[CBAColor.DarkGold], out Color darkgold);
            dict.Add((int)CBAColor.DarkGold, darkgold);
        }

        /// <summary>
        /// 通过枚举色取
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public Color GetColor(CBAColor color) {
            return dict[(int)color];
        }

        /// <summary>
        /// 通过品质色取
        /// </summary>
        /// <param name="quality"></param>
        /// <returns></returns>
        public Color GetColor(int quality) {
            return dict[quality];
        }

        /// <summary>
        /// 获取16进制颜色值
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public string GetHexColor(CBAColor color) {
            return hexDict[color];
        }

        public string GetQualityName(int quality) {
            return qualityname[quality];
        }


    }
}