using UnityEngine;
using UnityEditor;
using System.IO;

public class UICreator
{
    [MenuItem("Assets/创建UI(Window)", false, 0)]
    public static void CreateWindow()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Path.Combine(Application.dataPath, "EditorTools", "ScriptCreator", "Templates", "WindowTemplate.txt"), "NewWindow.cs");
    }

    [MenuItem("Assets/创建UI(Panel)", false, 0)]
    public static void CreatePanel()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Path.Combine(Application.dataPath, "EditorTools", "ScriptCreator", "Templates", "PanelTemplate.txt"), "NewPanel.cs");
    }
}
