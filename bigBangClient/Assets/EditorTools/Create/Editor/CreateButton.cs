using BigBang;
using BigBang.UI;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CreateButton
{
    private static MethodInfo m_miGetDefaultResource = null;
    private static MethodInfo m_miPlaceUIElementRoot = null;

    static CreateButton()
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

    static public BabuButton Create(MenuCommand menuCommand)
    {
        GameObject obj = DefaultControls.CreateImage((DefaultControls.Resources)m_miGetDefaultResource.Invoke(null, null));
        var btn = obj.AddComponent<BabuButton>();
        var rect = obj.GetComponent<RectTransform>();
        obj.name = "Babu Button";
        m_miPlaceUIElementRoot.Invoke(null, new object[] { obj, menuCommand });
        btn.image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/LocalAsset/_Sprites/Public/btn1.png");
        btn.image.SetNativeSize();
        return btn;
    }

    [MenuItem("GameObject/UI/CBA/Button", false, 2100)]
    public static void CreateBtn(MenuCommand menuCommand)
    {
        _ = Create(menuCommand);
    }

    [MenuItem("GameObject/UI/CBA/Button - TextMeshPro", false, 2101)]
    public static void CreateTMPBtn(MenuCommand menuCommand)
    {
        _ = Create(menuCommand);
    }

    [MenuItem("GameObject/UI/CBA/Button(被遗弃)", false, 2102)]
    static public void CreateSource(MenuCommand menuCommand)
    {
        GameObject obj = DefaultControls.CreateButton((DefaultControls.Resources)m_miGetDefaultResource.Invoke(null, null));
        m_miPlaceUIElementRoot.Invoke(null, new object[] { obj, menuCommand });
    }
}
