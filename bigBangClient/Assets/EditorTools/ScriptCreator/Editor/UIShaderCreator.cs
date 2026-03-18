using System.IO;
using UnityEditor;
using UnityEngine;

public class UIShaderCreator
{
    [MenuItem("Assets/Create/Shader/UI Shader", false, 0)]
    public static void CreateShader()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(Path.Combine(Application.dataPath, "EditorTools", "ScriptCreator", "Templates", "UIShaderTemplate.txt"), "NewUIShader.shader");
    }
}
