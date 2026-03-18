using System.Collections.Generic;
using System.Text;

namespace Babu
{
    public class Utils
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }

        public static V GetOrDefault<K, V>(Dictionary<K, V> dic, K key, V defaultValue)
        {
            V ret;
            if (dic.TryGetValue(key, out ret))
            {
                return ret;
            }
            return defaultValue;
        }

        public static int VersionToInt(string versionStr)
        {
            int intVersion = 0;
            string[] versionList = versionStr.Split('.');
            for (int i = 0; i < versionList.Length; ++i)
            {
                intVersion = intVersion * 100 + int.Parse(versionList[i]);
            }
            return intVersion;
        }

        public static string ListToString<T>(ICollection<T> list)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var str in list)
            {
                stringBuilder.Append(str).Append(",");
            }
            string returnStr = stringBuilder.ToString();
            if (returnStr.Length > 0)
            {
                return returnStr.Substring(0, returnStr.Length - 1);
            }
            return returnStr;
        }

        /// <summary>
        /// 对比两个dictionary是否相等，注意引用类型的不要用这个方法来比较
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dict1"></param>
        /// <param name="dict2"></param>
        /// <returns></returns>
        public static bool DictEquals<T1,T2>(Dictionary<T1, T2> dict1, Dictionary<T1,T2> dict2)
        {
            if (dict1.Keys.Count != dict2.Keys.Count)
                return false; // Different number of items

            foreach (var key in dict1.Keys)
            {
                T2 bValue;
                if (!dict2.TryGetValue(key, out bValue))
                    return false; // key missing in b
                if (!Equals(dict1[key], bValue))
                    return false; // value is different
            }
            return true;
        }
    }
}