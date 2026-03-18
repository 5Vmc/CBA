using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;

public class LayersCreator 
{
    // 模板文件路径
    private static readonly string templatePath = Path.Combine(Application.dataPath, "EditorTools", "ScriptCreator", "Templates", "LayersTemplate.txt");
    // 脚本生成路径
    private static readonly string scriptPath = Path.Combine(Application.dataPath, "Scripts", "GameConst", "Layers.cs");

    public static void CreateOrUpdateScript()
    {
        string template = File.ReadAllText(templatePath);
        StringBuilder content = new StringBuilder();
        foreach (var layer in UnityEditorInternal.InternalEditorUtility.layers)
        {
            content.Append($"    public static int {layer.Replace(" ", "")} => LayerMask.NameToLayer(\"{layer}\");\r\n");
        }
        string output = template.Replace("#PROPERTIES#", content.ToString().TrimEnd('\r', '\n'));
        File.WriteAllText(scriptPath, output);
        AssetDatabase.Refresh();
    }
}