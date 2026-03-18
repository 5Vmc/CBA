using System.Text;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

public class AudioNamesCreator
{
    // 音频文件文件路径
    private static readonly string audioPath = Path.Combine(Application.dataPath, "LocalAsset", "Audios");
    // 模板文件路径
    private static readonly string templatePath = Path.Combine(Application.dataPath, "EditorTools", "ScriptCreator", "Templates", "AudioNamesTemplate.txt");
    // 脚本生成路径
    private static readonly string scriptPath = Path.Combine(Application.dataPath, "Scripts", "GameConst", "AudioNames.cs");

    public static void CreateOrUpdateScript()
    {
        Debug.Log("文件更新:AudioNames.cs");
        var files = Directory.GetFiles(audioPath);
        List<FileInfo> audioNames = new List<FileInfo>();
        var template = File.ReadAllText(templatePath);
        foreach (var item in files)
        {
            if (!item.EndsWith(".meta"))
            {
                audioNames.Add(new FileInfo(item));
            }
        }
        StringBuilder property = new StringBuilder();
        audioNames.ForEach(item => property.Append($"public const string {Path.GetFileNameWithoutExtension(item.FullName.ToUpper()).Replace(" ", "")} = \"{item.Name}\";\r\n        "));
        var output = template.Replace("#PROPERTY#", property.ToString().TrimEnd('\r', '\n', ' '));
        File.WriteAllText(scriptPath, output);
        AssetDatabase.Refresh();
    }
}