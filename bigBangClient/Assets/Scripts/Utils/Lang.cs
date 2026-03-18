using System.Collections.Generic;
using System.Text;
using GameConfig;

namespace Utils
{
    public class Lang
    {
        public static string Get(LangID id)
        {
            var cfg = Configs.Lang.GetConfig((int) id);
            if (cfg == null) return "";
            string str = cfg.Content;
            str = str.Replace("\\n", "\n");
            return str;
        }


        public static string Get(LangID id, params string[] replaceList)
        {
            StringBuilder lang = new StringBuilder(Get(id));
            if (replaceList.Length % 2 != 0) return lang.ToString();
            for (int i = 0; i < replaceList.Length; i += 2)
            {
                lang.Replace(replaceList[i], replaceList[i + 1]);
            }

            return lang.ToString();
        }

        public static string Get(LangID id, Dictionary<string, string> replace)
        {
            var lang = Get(id);
            foreach (var item in replace)
            {
                lang = lang.Replace(item.Key, item.Value);
            }

            return lang;
        }

        public static string Error(int errorID)
        {
            var cfg = Configs.Lang.GetConfig(errorID);
            if (cfg == null) return "";
            return cfg.Content;
        }

        public static string Error(ErrorID errorID)
        {
            return Error((int) errorID);
        }
    }
}