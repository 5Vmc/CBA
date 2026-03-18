using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Babu
{
    public class StringUtils
    {
        public static bool HasChinese(string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        public static string CollectionToString<T>(ICollection<T> collection, string delimiter = ",")
        {
            bool first = true;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var str in collection)
            {
                if (first)
                {
                    stringBuilder.Append(str.ToString());
                    first = false;
                }
                else
                {
                    stringBuilder.Append(delimiter).Append(str.ToString());
                }
            }
            return stringBuilder.ToString();
        }

        public static string BytesToString(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public static byte[] StringToBytes(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        public static string SecondToString(long second)
        {
            string str;
            if (second < 60)
            {
                str = second.ToString() + "秒";
            }
            else if (second < 3600)
            {
                str = Mathf.FloorToInt(second / 60).ToString() + "分钟";
            }
            else if (second < 3600 * 24)
            {
                str = Mathf.FloorToInt(second / 3600).ToString() + "小时";
            }
            else
            {
                str = Mathf.FloorToInt(second / 86400).ToString() + "天";
            }
            return str;
        }
    }
}
