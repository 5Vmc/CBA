using Babu.Config;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Babu.Globalization.Editor
{
    class Editor :  UnityEditor.Editor
    {
        static readonly string[] PREFAB_PATHS = new string[]
        {
            "Assets/Prefabs",
            "Assets/Resources/Panels",
        };

        static readonly string OUT_PUT_PATH = "Assets/Resources/Config/ui_chinese.txt";
        static readonly string GLOBALIZATION_CONFIG_PATH = "Config/cfg_globalization";
        static readonly string BLACK_CHINESE_PATH = "Assets/Resources/Config/cfg_black_chinese.txt";

        [MenuItem("Babu/Globalization/ExportUIText")]
        static void ExportUIChineseText()
        {
            HashSet<string> chineseTextes = new HashSet<string>();
            ScanPrefabComponment<Text>((text) =>
            {
                if (StringUtils.HasChinese(text.text))
                {
                    chineseTextes.Add(text.text.Replace("\n", "<br/>").Replace("\r", "<br/>").Trim());
                }
            });

            FileUtils.WriteFile(OUT_PUT_PATH, StringUtils.CollectionToString(chineseTextes, "\n"));
        }

        [MenuItem("Babu/Globalization/PreOprateUIChineseText")]
        static void PreOprateUIChineseText()
        {
            DataTable dataTable = CSVService.LoadTables(GLOBALIZATION_CONFIG_PATH);
            string str = FileUtils.ReadFile(BLACK_CHINESE_PATH);
            string[] blackChinese = str.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            ScanPrefabComponment<Text>((text) =>
            {
                if (StringUtils.HasChinese(text.text))
                {
                    string str = text.text.Replace("\n", "<br/>").Replace("\r", "<br/>").Trim();
                    if (!IsBlackChinese(blackChinese, str))
                    {
                        string index = GetGlobalizationConfigIndex(dataTable, str);
                        if (index != string.Empty)
                        {
                            text.text = "$$" + index;
                            text.gameObject.AddComponent<GlobalizationTextFiller>();
                        }
                    }
                }
            });
        }

        static bool IsBlackChinese(string[] blackChinese, string str)
        {
            foreach (var blackStr in blackChinese)
            {
                if (blackStr == str)
                {
                    return true;
                }
            }
            return false;
        }

        static string GetGlobalizationConfigIndex(DataTable dataTable, string str)
        {
            int rowCount = dataTable.getRowCount();
            for (int i = 0; i < rowCount; ++i)
            {
                DataRow dataRow = dataTable.getDataRow(i);
                if (dataRow.GetString(1) == str)
                {
                    return dataRow.GetString(0);
                }
            }
            return "";
        }

        static void ScanPrefabComponment<T>(Transform transform, Action<T> callback)
        {
            T comp = transform.GetComponent<T>();
            if (comp != null)
            {
                callback(comp);
            }

            for (int i = 0; i < transform.childCount; ++i)
            {
                ScanPrefabComponment(transform.GetChild(i), callback);
            }
        }

        static void ScanPrefabComponment<T>(Action<T> callback)
        {
            for (int i = 0; i < PREFAB_PATHS.Length; ++i)
            {
                DirectoryInfo dir = new DirectoryInfo(PREFAB_PATHS[i]);
                FileInfo[] files = dir.GetFiles("*.prefab", SearchOption.TopDirectoryOnly);

                for (int j = 0; j < files.Length; ++j)
                {
                    string prefabPath = PREFAB_PATHS[i] + "/" + files[j].Name;
                    try
                    {
                        GameObject target = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
                        target = PrefabUtility.InstantiatePrefab(target) as GameObject;
                        ScanPrefabComponment<T>(target.transform, callback);
                        PrefabUtility.SaveAsPrefabAsset(target, prefabPath);
                        GameObject.DestroyImmediate(target);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Analyze prefab: {prefabPath} failed: " + e.Message);
                    }
                }
            }
        }
    }
}
