using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 创建Raycast Target默认为false的图片
/// 该方法会覆盖系统自带方法
/// </summary>
[InitializeOnLoad]
public class CreateImage
{
    private static MethodInfo m_miGetDefaultResource = null;
    private static MethodInfo m_miPlaceUIElementRoot = null;

    static CreateImage()
    {
        Initialize();
    }

    private static void Initialize()
    {
        Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        Assembly uiEditorAssembly = null;
        foreach (Assembly assembly in allAssemblies)
        {
            AssemblyName assemblyName = assembly.GetName();
            if ("UnityEditor.UI" == assemblyName.Name)
            {
                uiEditorAssembly = assembly;
                break;
            }
        }
        if (null == uiEditorAssembly)
        {
            return;
        }

        Type menuOptionType = uiEditorAssembly.GetType("UnityEditor.UI.MenuOptions");
        m_miGetDefaultResource = menuOptionType.GetMethod("GetStandardResources", BindingFlags.NonPublic | BindingFlags.Static);
        m_miPlaceUIElementRoot = menuOptionType.GetMethod("PlaceUIElementRoot", BindingFlags.NonPublic | BindingFlags.Static);
    }

    [MenuItem("GameObject/UI/Image", false, 2000)]
    static public void Create(MenuCommand menuCommand)
    {
        GameObject obj = DefaultControls.CreateImage((DefaultControls.Resources)m_miGetDefaultResource.Invoke(null, null));
        m_miPlaceUIElementRoot.Invoke(null, new object[] { obj, menuCommand });
        Image image = obj.GetComponent<Image>();
        image.raycastTarget = false;
    }
}
