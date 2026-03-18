using System.Text;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

public class AtlasNamesCreator
{
    // 图集文件路径
    private static readonly string atlasPath = Path.Combine(Application.dataPath, "LocalAsset", "Sprites/Atlas");
    // 模板文件路径
    private static readonly string templatePath = Path.Combine(Application.dataPath, "EditorTools", "ScriptCreator", "Templates", "AtlasNamesTemplate.txt");
    // 脚本生成路径
    private static readonly string scriptPath = Path.Combine(Application.dataPath, "Scripts", "GameConst", "AtlasNames.cs");
    // 文件类型
    private static readonly string fileType = ".spriteatlas";

    public static void CreateOrUpdateScript()
    {
        Debug.Log("文件更新:AtlasNames.cs");
        var files = Directory.GetFiles(atlasPath);
        List<string> atlasNames = new List<string>();
        var template = File.ReadAllText(templatePath);
        foreach (var item in files)
        {
            if (item.EndsWith(fileType))
            {
                // 忽略文件名中的空格
                atlasNames.Add(Path.GetFileNameWithoutExtension(item).Replace(" ", ""));
            }
        }
        StringBuilder property = new StringBuilder();
        atlasNames.ForEach(item => property.Append($"public const string {item} = \"{item}\";\r\n        "));
        var output = template.Replace("#PROPERTY#", property.ToString().TrimEnd('\r', '\n', ' '));
        File.WriteAllText(scriptPath, output);
        AssetDatabase.Refresh();
    }
}