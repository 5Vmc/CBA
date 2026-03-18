using UnityEditor;
using UnityEngine;

public class SearchForSimilarImageWindow : EditorWindow
{
    public static string windowName = "相似图片";

    private const int MAX_SHOW_COUNT = 200;

    private static bool SHOW_ALL = false;

    private Vector2 scrollPos;

    [MenuItem("开发者/美术资源/查找相似图片")]
    public static void Init()
    {
        SearchForSimilarImageWindow window = GetWindow<SearchForSimilarImageWindow>();

        window.autoRepaintOnSceneChange = true;
        window.titleContent.text = windowName;
        window.wantsMouseMove = true;

        window.Show();
    }

    private void OnGUI()
    {
        GUIStyle similarityTxtStyle = new GUIStyle();
        GUIStyle nameTxtStyle = new GUIStyle();

        similarityTxtStyle.fontSize = 30;
        similarityTxtStyle.alignment = TextAnchor.MiddleCenter;
        similarityTxtStyle.normal.textColor = GUI.color;

        nameTxtStyle.alignment = TextAnchor.UpperCenter;
        nameTxtStyle.normal.textColor = GUI.color;

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope())
            {
                if (SearchForSimilarImage.Result.Count > MAX_SHOW_COUNT)
                {
                    if (!SHOW_ALL)
                    {
                        GUILayout.Label(MAX_SHOW_COUNT + "/" + SearchForSimilarImage.Result.Count);
                        if (GUILayout.Button("全部显示"))
                        {
                            SHOW_ALL = true;
                        }
                    }
                    else
                    {
                        GUILayout.Label(SearchForSimilarImage.Result.Count.ToString());
                        if (GUILayout.Button("折叠"))
                        {
                            SHOW_ALL = false;
                        }
                    }
                }
                else
                {
                    GUILayout.Label(SearchForSimilarImage.Result.Count.ToString());
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Refresh").image, "刷新"), EditorStyles.miniButtonMid))
            {
                SearchForSimilarImage.Search();
            }
        }


        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox, GUILayout.MaxHeight(maxSize.y - 20f));
        var prevColor = GUI.color;
        var prevBgColor = GUI.backgroundColor;
        var result = SearchForSimilarImage.Result;
        int miss = 0;
        for (int i = 0; i < result.Count; i++)
        {

            if (!SHOW_ALL && i > MAX_SHOW_COUNT) break;

            var asset1 = result[i].Img1.Asset;
            var asset2 = result[i].Img2.Asset;
            if (asset1 == null || asset2 == null)
            {
                miss++;
                continue;
            }

            var rect = EditorGUILayout.BeginHorizontal();

            GUI.color = (i - miss) % 2 == 0 ? Color.grey * (EditorGUIUtility.isProSkin ? 1f : 1.7f) : Color.grey * (EditorGUIUtility.isProSkin ? 1.05f : 1.66f);

            EditorGUI.DrawRect(rect, GUI.color);

            GUI.color = prevColor;
            GUI.backgroundColor = prevBgColor;

            EditorGUILayout.BeginVertical();
            if (GUILayout.Button(asset1, GUILayout.Width(100), GUILayout.Height(100)))
            {
                EditorGUIUtility.PingObject(asset1);
            }
            GUILayout.Label(asset1.name, nameTxtStyle, GUILayout.Width(100), GUILayout.Height(20));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            if (GUILayout.Button(asset2, GUILayout.Width(100), GUILayout.Height(100)))
            {
                EditorGUIUtility.PingObject(asset2);
            }
            GUILayout.Label(asset2.name, nameTxtStyle, GUILayout.Width(100), GUILayout.Height(20));
            EditorGUILayout.EndVertical();

            GUILayout.Label(((int)(result[i].Similarity * 100)).ToString() + "%", similarityTxtStyle, GUILayout.Width(100), GUILayout.Height(120));
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void OnDestroy()
    {
        SearchForSimilarImage.Clear();
    }
}