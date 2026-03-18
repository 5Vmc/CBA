using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TagsCreator
{
    // 模板文件路径
    private static readonly string templatePath = Path.Combine(Application.dataPath, "EditorTools", "ScriptCreator", "Templates", "TagsTemplate.txt");
    // 脚本生成路径
    private static readonly string scriptPath = Path.Combine(Application.dataPath, "Scripts", "GameConst", "Tags.cs");

    public static void CreateOrUpdateScript()
    {
        string template = File.ReadAllText(templatePath);
        StringBuilder content = new StringBuilder();
        foreach (var tag in UnityEditorInternal.InternalEditorUtility.tags)
        {
            content.Append($"    public const string {tag.Replace(" ", "")} = \"{tag}\";\r\n");
        }
        string output = template.Replace("#PROPERTIES#", content.ToString().TrimEnd('\r', '\n'));
        File.WriteAllText(scriptPath, output);
        AssetDatabase.Refresh();
    }
}