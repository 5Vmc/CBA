// Staggart Creations http://staggart.xyz
// Copyright protected under Unity asset store EULA

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

public class SelectionHistoryWindow : EditorWindow
{
    public static string windowName = " Selection History";
    [MenuItem("Window/Selection History")]
    public static void Init()
    {
        SelectionHistoryWindow window = GetWindow<SelectionHistoryWindow>();
        //Options
        window.autoRepaintOnSceneChange = true;
        window.titleContent.image = EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_UnityEditor.SceneHierarchyWindow" : "UnityEditor.SceneHierarchyWindow").image;
        window.titleContent.text = windowName;
        window.wantsMouseMove = true;

        //Show
        window.Show();
    }

    // 资源打开事件
    [OnOpenAssetAttribute]
    private static bool OnOpenAsset(int instanceID, int line)
    {
        if (!HasOpenInstances<SelectionHistoryWindow>()) return false;
        var window = GetWindow<SelectionHistoryWindow>();
        var obj = window.selectionHistory.Find(item => item.GetInstanceID() == instanceID);
        if (obj == null) return false;
        window.selectionHistory.Remove(obj);
        window.selectionHistory.Insert(0, obj);
        return false;
    }

    private AnimBool settingAnimation;
    private bool settingExpanded;
    private AnimBool clearAnimation;
    private bool historyVisible = true;
    private static bool muteRecording;
    private int selectedIndex = -1;
    private new bool hasFocus;

    public List<Object> selectionHistory = new List<Object>();

    private string iconPrefix => EditorGUIUtility.isProSkin ? "d_" : "";

    public static bool RecordHierarchy
    {
        get { return EditorPrefs.GetBool(PlayerSettings.productName + "_SH_RecordHierachy", false); }
        set { EditorPrefs.SetBool(PlayerSettings.productName + "_SH_RecordHierachy", value); }
    }

    public static bool RecordProject
    {
        get { return EditorPrefs.GetBool(PlayerSettings.productName + "_SH_RecordProject", true); }
        set { EditorPrefs.SetBool(PlayerSettings.productName + "_SH_RecordProject", value); }
    }

    public static int MaxHistorySize
    {
        get { return EditorPrefs.GetInt(PlayerSettings.productName + "_SH_MaxHistorySize", 50); }
        set { EditorPrefs.SetInt(PlayerSettings.productName + "_SH_MaxHistorySize", value); }
    }

    public static bool ExceptFolder
    {
        get { return EditorPrefs.GetBool(PlayerSettings.productName + "_SH_ExceptFolder", true); }
        set { EditorPrefs.SetBool(PlayerSettings.productName + "_SH_ExceptFolder", value); }
    }

    public static bool ExceptScript
    {
        get { return EditorPrefs.GetBool(PlayerSettings.productName + "_SH_ExceptScript", true); }
        set { EditorPrefs.SetBool(PlayerSettings.productName + "_SH_ExceptScript", value); }
    }


    public static bool ExceptShader
    {
        get { return EditorPrefs.GetBool(PlayerSettings.productName + "_SH_ExceptShader", true); }
        set { EditorPrefs.SetBool(PlayerSettings.productName + "_SH_ExceptShader", value); }
    }

    public static bool ExceptAssembly
    {
        get { return EditorPrefs.GetBool(PlayerSettings.productName + "_SH_ExceptAssembly", true); }
        set { EditorPrefs.SetBool(PlayerSettings.productName + "_SH_ExceptAssembly", value); }
    }

    public static bool ExceptTexture2D
    {
        get { return EditorPrefs.GetBool(PlayerSettings.productName + "_SH_ExceptTexture2D", false); }
        set { EditorPrefs.SetBool(PlayerSettings.productName + "_SH_ExceptTexture2D", value); }
    }

    public static bool ExceptTextAsset
    {
        get { return EditorPrefs.GetBool(PlayerSettings.productName + "_SH_ExceptTextAsset", false); }
        set { EditorPrefs.SetBool(PlayerSettings.productName + "_SH_ExceptTextAsset", value); }
    }

    // 选择改变事件
    private void OnSelectionChange()
    {
        this.Repaint();

        if (muteRecording || !Selection.activeObject) return;
        AddToHistory();
    }

    private void OnFocus()
    {
        //Items have have been deleted and should be removed from history
        selectionHistory = selectionHistory.Where(x => x != null).ToList();

        hasFocus = true;
    }

    private void OnLostFocus()
    {
        hasFocus = false;
    }

    private void OnInspectorUpdate() //10 fps
    {
        if (hasFocus) Repaint();
    }

    private void AddToHistory()
    {
        var selectionType = Selection.activeObject.GetType();
        // 排除文件夹
        if (ExceptFolder && selectionType == typeof(DefaultAsset)) return;
        // 排除脚本
        if (ExceptScript && selectionType == typeof(MonoScript)) return;
        // 排除Shader
        if (ExceptShader && selectionType == typeof(Shader)) return;
        // 排除程序集
        if (ExceptAssembly && selectionType == typeof(UnityEditorInternal.AssemblyDefinitionAsset)) return;
        // 排除图片
        if (ExceptTexture2D && selectionType == typeof(Texture2D)) return;
        // 排除文本文件
        if (ExceptTextAsset && selectionType == typeof(TextAsset)) return;

        if (EditorUtility.IsPersistent(Selection.activeObject) && !RecordProject) return;
        if (EditorUtility.IsPersistent(Selection.activeObject) == false && !RecordHierarchy) return;

        // 如果不在历史记录中，则插入到历史表中
        if (!selectionHistory.Contains(Selection.activeObject))
        {
            selectionHistory.Insert(0, Selection.activeObject);
        }

        //Trim end
        if (selectionHistory.Count - 1 == MaxHistorySize) selectionHistory.RemoveAt(selectionHistory.Count - 1);
    }

    private void OnEnable()
    {
#if !UNITY_2019_1_OR_NEWER
        SceneView.onSceneGUIDelegate += ListenForNavigationInput;
#else
        SceneView.duringSceneGui += ListenForNavigationInput;
#endif

        settingAnimation = new AnimBool(false);
        settingAnimation.valueChanged.AddListener(this.Repaint);
        settingAnimation.speed = 4f;
        clearAnimation = new AnimBool(false);
        clearAnimation.valueChanged.AddListener(this.Repaint);
        clearAnimation.speed = settingAnimation.speed;
    }

    private void OnDisable()
    {
#if !UNITY_2019_1_OR_NEWER
        SceneView.onSceneGUIDelegate -= ListenForNavigationInput;
#else
        SceneView.duringSceneGui -= ListenForNavigationInput;
#endif
    }

    private void ListenForNavigationInput(SceneView sceneView)
    {
        if (Event.current.type == EventType.KeyDown && Event.current.isKey && Event.current.keyCode == KeyCode.LeftBracket)
        {
            SelectPrevious();
        }
        if (Event.current.type == EventType.KeyDown && Event.current.isKey && Event.current.keyCode == KeyCode.RightBracket)
        {
            SelectNext();
        }
    }

    private void SetSelection(Object target, int index)
    {
        muteRecording = true;
        Selection.activeObject = target;
        EditorGUIUtility.PingObject(target);
        muteRecording = false;
    }

    private void SelectPrevious()
    {
        selectedIndex--;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, selectionHistory.Count - 1);

        SetSelection(selectionHistory[selectedIndex], selectedIndex);
    }

    private void SelectNext()
    {
        selectedIndex++;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, selectionHistory.Count - 1);

        SetSelection(selectionHistory[selectedIndex], selectedIndex);
    }

    private Vector2 scrollPos;

    private void OnGUI()
    {
        hasFocus = hasFocus || (Event.current.type == EventType.MouseMove);

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(selectionHistory.Count == 0))
            {
                using (new EditorGUI.DisabledScope(selectedIndex == selectionHistory.Count - 1))
                {
                    if (GUILayout.Button(
                        new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "back@2x").image,
                            "Select previous (Left bracket key)"), EditorStyles.miniButtonLeft, GUILayout.Height(20f),
                        GUILayout.Width(30f)))
                    {
                        SelectNext();
                    }
                }

                using (new EditorGUI.DisabledScope(selectedIndex == 0))
                {
                    if (GUILayout.Button(
                        new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "forward@2x").image,
                            "Select next (Right bracket key)"), EditorStyles.miniButtonRight, GUILayout.Height(20),
                        GUILayout.Width(30f)))
                    {
                        SelectPrevious();
                    }
                }

                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "TreeEditor.Trash").image, "清空"), EditorStyles.miniButton))
                {
                    historyVisible = false;
                }
            }

            GUILayout.FlexibleSpace();

            settingExpanded = GUILayout.Toggle(settingExpanded, new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "Settings").image, "Edit settings"), EditorStyles.miniButtonMid);
            settingAnimation.target = settingExpanded;
        }

        if (EditorGUILayout.BeginFadeGroup(settingAnimation.faded))
        {
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Record", EditorStyles.boldLabel, GUILayout.Width(100f));
                RecordHierarchy = EditorGUILayout.ToggleLeft("Hierarchy", RecordHierarchy, GUILayout.MaxWidth(80f));
                RecordProject = EditorGUILayout.ToggleLeft("Project window", RecordProject);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("History size", EditorStyles.boldLabel, GUILayout.Width(100f));
                MaxHistorySize = EditorGUILayout.IntField(MaxHistorySize, GUILayout.MaxWidth(40f));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Except", EditorStyles.boldLabel, GUILayout.Width(100f));
                ExceptFolder = EditorGUILayout.ToggleLeft("Folder", ExceptFolder, GUILayout.MaxWidth(80f));
                ExceptScript = EditorGUILayout.ToggleLeft("Script", ExceptScript, GUILayout.MaxWidth(80f));
                ExceptShader = EditorGUILayout.ToggleLeft("Shader", ExceptShader, GUILayout.MaxWidth(80f));
                ExceptAssembly = EditorGUILayout.ToggleLeft("Assembly", ExceptAssembly, GUILayout.MaxWidth(80f));
                ExceptTexture2D = EditorGUILayout.ToggleLeft("Texture2D", ExceptTexture2D, GUILayout.MaxWidth(80f));
                ExceptTextAsset = EditorGUILayout.ToggleLeft("TextAsset", ExceptTextAsset, GUILayout.MaxWidth(80f));
            }

            EditorGUILayout.Space();
        }
        EditorGUILayout.EndFadeGroup();

        clearAnimation.target = !historyVisible;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox, GUILayout.MaxHeight(this.maxSize.y - 20f));
        {
            EditorGUILayout.BeginFadeGroup(1f - clearAnimation.faded);

            var prevColor = GUI.color;
            var prevBgColor = GUI.backgroundColor;
            Object deleteItem = null;
            for (int i = 0; i < selectionHistory.Count; i++)
            {
                if (selectionHistory[i] == null) continue;

                var rect = EditorGUILayout.BeginHorizontal();

                GUI.color = i % 2 == 0 ? Color.grey * (EditorGUIUtility.isProSkin ? 1f : 1.7f) : Color.grey * (EditorGUIUtility.isProSkin ? 1.05f : 1.66f);

                //Hover color
                if (rect.Contains(Event.current.mousePosition) || Selection.activeObject == (selectionHistory[i]))
                {
                    GUI.color = EditorGUIUtility.isProSkin ? Color.grey * 1.1f : Color.grey * 1.5f;
                }

                //Selection outline
                if (Selection.activeObject == (selectionHistory[i]))
                {
                    Rect outline = rect;
                    outline.x -= 1;
                    outline.y -= 1;
                    outline.width += 2;
                    outline.height += 2;
                    EditorGUI.DrawRect(outline, EditorGUIUtility.isProSkin ? Color.gray * 1.5f : Color.gray);
                }

                //Background
                EditorGUI.DrawRect(rect, GUI.color);

                GUI.color = prevColor;
                GUI.backgroundColor = prevBgColor;
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "ViewToolOrbit").image, "查看"), GUILayout.Width(30), GUILayout.Height(25)))
                {
                    AssetDatabase.OpenAsset(selectionHistory[i]);
                }
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "TreeEditor.Trash").image, "删除"), GUILayout.Width(30), GUILayout.Height(25)))
                {
                    deleteItem = selectionHistory[i];
                }
                if (GUILayout.Button(new GUIContent(" " + selectionHistory[i].name, EditorGUIUtility.ObjectContent(selectionHistory[i], selectionHistory[i].GetType()).image), EditorStyles.label, GUILayout.MaxHeight(25f)))
                {
                    SetSelection(selectionHistory[i], i);
                }
                EditorGUILayout.EndHorizontal();
            }
            if (deleteItem != null)
            {
                selectionHistory.Remove(deleteItem);
            }
            EditorGUILayout.EndFadeGroup();
        }
        EditorGUILayout.EndScrollView();

        //Once the list is collapse, clear the collection
        if (clearAnimation.faded == 1f) selectionHistory.Clear();
        //Reset
        if (selectionHistory.Count == 0) historyVisible = true;
    }
}