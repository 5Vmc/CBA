using BigBang;
using System.Collections.Generic;

namespace Utils
{

    public class ConfigUtil
    {
        public static bool CompareByStr(int left, string compareStr, int right)
        {
            if (compareStr == ">=") return left >= right;
            if (compareStr == ">") return left > right;
            if (compareStr == "<=") return left <= right;
            if (compareStr == "<") return left < right;
            if (compareStr == "=") return left == right;
            if (compareStr == "!=") return left != right;
            UnityEngine.Debug.LogWarning("ConfigUtil , compareByStr , compareStr = " + compareStr);
            return left > right;
        }


    }
}
