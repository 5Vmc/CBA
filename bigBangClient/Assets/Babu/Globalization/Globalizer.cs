using Babu.Config;
using System.Collections.Generic;
using UnityEngine;

namespace Babu.Globalization
{
    public class Globalizer : BabuSingleton<Globalizer>
    {
        public enum LanguageType
        {
            None,
            English,
            Chinese,
            TraditionalChinese,
            Korean,
            Japanese,

            MAX
        }

        private List<Language> _supportLanguageList = new List<Language>();
        private Language _curLanguage;

        static readonly string SUPPORT_LANGUAGE_CONFIG_PATH = "config/cfg_support_lang";
        static readonly string GLOBALIZATION_CONFIG_PATH = "config/cfg_globalization";

        private DataTable _globalizationConfigTable;

        private IConfigLoader _configLoader = new CSVConfigLoader();

        public override void Awake()
        {
            base.Awake();

            //LoadSupportLanguage();

            //// 是否主动设置过语言
            //int language = PlayerPrefs.GetInt("language", 0);
            //if (language == 0)
            //{
            //    SetLanguageFromSystem();
            //}
            //else if (language >= (int)LanguageType.MAX)
            //{
            //    PlayerPrefs.DeleteKey("language");
            //}
            //else
            //{
            //    SetLanguage((LanguageType)language);
            //}
        }

        void LoadSupportLanguage()
        {
            DataTable dataTable = _configLoader.LoadTable(SUPPORT_LANGUAGE_CONFIG_PATH);

            if (dataTable != null)
            {
                for (int i = 0; i < dataTable.getRowCount(); ++i)
                {
                    DataRow dataRow = dataTable.getDataRow(i);
                    string ready = dataRow.GetString("ready");
                    if (ready == "1")
                    {
                        Language language = new Language();
                        language.Type = (LanguageType)dataRow.GetInt32("id");
                        language.Suffix = dataRow.GetString("suffix");
                        language.Desc = dataRow.GetString("desc");
                        language.Default = dataRow.GetString("default") == "1";

                        _supportLanguageList.Add(language);
                    }
                }
            }
           
            if (_supportLanguageList.Count == 0)
            {
                Language language = new Language();
                language.Type = LanguageType.English;
                language.Suffix = "_en";
                language.Desc = "English";
                language.Default = true;
                _supportLanguageList.Add(language);
            }
        }

        Language GetDefaultLanguage()
        {
            foreach (var lang in _supportLanguageList)
            {
                if (lang.Default)
                {
                    return lang;
                }
            }
            return _supportLanguageList[0];
        }

        void SetLanguage(LanguageType languageType)
        {
            foreach (var lang in _supportLanguageList)
            {
                if (lang.Type == languageType)
                {
                    _curLanguage = lang;
                }
            }

            if (_curLanguage == null)
            {
                _curLanguage = GetDefaultLanguage();
            }
        }

        void SetLanguageFromSystem()
        {
            string systemLanguage = Platform.GetSystemLanguage();
            systemLanguage = systemLanguage.ToLower();
            Debug.Log("System Language: " + systemLanguage);
            Debug.Log("Country Code1: " + Platform.GetSimCountryCode());
            Debug.Log("Country Code2: " + Platform.GetCountryCode());

            if (systemLanguage.StartsWith("en"))
            {
                SetLanguage(LanguageType.English);
            }
            else if (systemLanguage.StartsWith("zh"))
            {
                var tags = systemLanguage.Split('-');
                if (tags.Length < 2)
                {
                    SetLanguage(LanguageType.Chinese);
                    return;
                }
                if (tags[1] == "hans")
                {
                    SetLanguage(LanguageType.Chinese);
                }
                else if (tags[1] == "hant")
                {
                    SetLanguage(LanguageType.TraditionalChinese);
                }
                else
                {
                    SetLanguage(LanguageType.Chinese);
                }
            }
            else
            {
                SetLanguage(GetDefaultLanguage().Type);
            }
        }

        DataTable GetGlobalizationConfigTable()
        {
            if (_globalizationConfigTable == null)
            {
                //_globalizationConfigTable = _configLoader.LoadTable(GLOBALIZATION_CONFIG_PATH);
                _globalizationConfigTable = new DataTable();
            }
            return _globalizationConfigTable;
        }

        // 获取国际化后的文字;
        public string GetGlobalizationText(string text)
        {
            if (text.StartsWith("$$") == false)
            {
                return text;
            }

            int index;
            if (int.TryParse(text.Substring(2), out index) == false)
            {
                return text;
            }

            DataTable dataTable = GetGlobalizationConfigTable();
            if (index > dataTable.getRowCount())
            {
                return text;
            }

            string fieldName = "text" + _curLanguage.Suffix;
            int fieldIndex = dataTable.filedIndex(fieldName);
            if (fieldIndex == -1)
            {
                return text;
            }

            return dataTable.getDataRow(index - 1).GetString(fieldIndex);
        }

        // 获取当前语言后缀
        public string GetCurLanguageSuffix()
        {
            return _curLanguage == null ? "_en" : _curLanguage.Suffix;
        }

        // 获取当前语言类型
        public LanguageType GetCurLanguageType()
        {
            return _curLanguage == null ? LanguageType.English : _curLanguage.Type;
        }

        public bool IsInternationalVersion()
        {
#if UNITY_EDITOR
            return false;
#else
            string channelId = Environment.GetValue<string>("channel_id", "unknown");
            if (channelId.Contains("google_play") || channelId.Contains("oversea"))
            {
                return true;
            }
            return false;
#endif
        }
    }
}
