using BigBang;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ude;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityObject = UnityEngine.Object;

public static class DeveloperMenu
{
    [MenuItem("开发者/返回加载界面")]
    private static void BackToLoadingUI()
    {
        LoginManager.Instance.BackToLogin();
    }

    [MenuItem("开发者/完成引导")]
    private static void FinishGuide()
    {
        GuideManager.FinishAll(false);
    }

    [MenuItem("开发者/查找空引用")]
    private static void FindNull()
    {
        foreach (var item in FindNullReference.Find())
        {
            Debug.Log(item.Item1, item.Item2);
        }
    }

    [MenuItem("开发者/检查代码文件")]
    private static void CheckCodeFile()
    {
        var rule = new Regex("= *$?\".*[\u4e00-\u9fa5]+.*\"");
        var paths = AssetDatabase.GetAllAssetPaths().Where(item => item.StartsWith("Assets/Scripts") && item.EndsWith(".cs"));
        // 中文直接赋值
        foreach (var path in paths)
        {
            var content = File.ReadAllText(Path.Combine(Application.dataPath, "..", path));
            if (rule.IsMatch(content))
            {
                var script = AssetDatabase.LoadAssetAtPath(path, typeof(UnityObject));
                Debug.Log("<color=yellow>中文直接赋值</color> " + path.Replace(script.name + ".cs", "<color=blue>" + script.name + ".cs</color>"), script);
            }
        }

        // 文件编码不是UTF-8
        foreach (var path in paths)
        {
            var script = AssetDatabase.LoadAssetAtPath(path, typeof(UnityObject));
            using var reader = File.OpenRead(Path.Combine(Application.dataPath, "..", path));
            var detector = new CharsetDetector();
            detector.Feed(reader);
            detector.DataEnd();
            if (detector.Charset != null)
            {
                if (!(detector.Charset == "UTF-8" || detector.Charset == "ASCII"))
                {
                    Debug.Log("<color=red>文件编码不是UTF-8</color> " + path.Replace(script.name + ".cs", "<color=blue>" + script.name + ".cs</color>"), script);
                }
            }
        }
    }

    [MenuItem("开发者/获得字符集")]
    private static void GetCharacterSet()
    {
        Dictionary<char, int> set = new Dictionary<char, int>();
        // 过滤条件
        string filter = "Assets/LocalAsset";
        // 预制体内文本
        foreach (var path in AssetDatabase.GetAllAssetPaths().Where(item => item.StartsWith(filter) && item.EndsWith(".prefab")))
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (var tmp in asset.GetComponentsInChildren<TMPro.TMP_Text>())
            {
                foreach (var item in tmp.text)
                {
                    if (!set.ContainsKey(item))
                    {
                        set.Add(item, 0);
                    }
                    set[item]++;
                }
            }
            foreach (var txt in asset.GetComponentsInChildren<Text>())
            {
                foreach (var item in txt.text)
                {
                    if (!set.ContainsKey(item))
                    {
                        set.Add(item, 0);
                    }
                    set[item]++;
                }
            }
        }
        // 忽略文本
        string ignore = "NameFilter.txt";
        // 配置表内文本
        foreach (var path in AssetDatabase.GetAllAssetPaths()
            .Where(item => item.StartsWith(filter) && !item.EndsWith(ignore) && (item.EndsWith(".csv") || item.EndsWith(".txt"))))
        {
            foreach (var item in File.ReadAllText(Path.Combine(Application.dataPath, "..", path)))
            {
                if (!set.ContainsKey(item))
                {
                    set.Add(item, 0);
                }
                set[item]++;
            }
        }
        // 保存结果
        StringBuilder builder = new StringBuilder();
        // 排除换行,回车
        foreach (var item in set.Where(item => item.Key != '\r' && item.Key != '\n').OrderByDescending(item => item.Value).Take(1000))
        {
            builder.Append(item.Key);
        }
        File.WriteAllText(Path.Combine(Application.dataPath, "character_set.txt"), builder.ToString());
        AssetDatabase.Refresh();
        Debug.Log("字符数:" + set.Count);
        Debug.Log("文件生成在:Assets/character_set.txt", AssetDatabase.LoadAssetAtPath("Assets/character_set.txt", typeof(TextAsset)));
    }

    [MenuItem("开发者/美术资源/查找不含Alpha通道的图片")]
    private static void ExportImageWithoutAloha()
    {
        // 过滤条件
        string filter = "Assets/LocalAsset";
        foreach (var path in AssetDatabase.GetAllAssetPaths().Where(item => item.StartsWith(filter) && (item.EndsWith(".png") || item.EndsWith("jpg"))))
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer.DoesSourceTextureHaveAlpha())
            {
                var asset = AssetDatabase.LoadAssetAtPath(path, typeof(Texture));
                Debug.Log(path, asset);
            }
        }
    }

    [MenuItem("开发者/美术资源/查找大尺寸UI图片")]
    private static void FindLargeSizeImage()
    {
        // 过滤条件
        string filter = "Assets/LocalAsset/Sprites";
        foreach (var path in AssetDatabase.GetAllAssetPaths().Where(item => item.StartsWith(filter) && (item.EndsWith(".png") || item.EndsWith("jpg"))))
        {
            var asset = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (asset.width * asset.height > 720 * 720)
            {
                Debug.Log(path, asset);
            }
        }
    }
}
