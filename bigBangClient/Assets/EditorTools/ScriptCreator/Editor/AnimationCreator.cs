using UnityEngine;
using UnityEditor;
using System.IO;

public class AnimationCreator
{
    [MenuItem("Assets/创建UI(Animation)", false, 0)]
    public static void CreateWindow()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Path.Combine(Application.dataPath, "EditorTools", "ScriptCreator", "Templates", "AnimationTemplate.txt"), "NewAnim.cs");
    }
}
