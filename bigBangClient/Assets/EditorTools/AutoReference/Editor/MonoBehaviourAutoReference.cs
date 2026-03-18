using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonoBehaviour), true)]
public class MonoBehaviourAutoReference : EditorButton
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(10);
        if (GUILayout.Button("自动引用"))
        {
            Debug.Log("自动引用" + AutoReference.SetReference(target) + "个字段");
        }
        GUILayout.Label("自动引用需要字段名与物体名称相同(不区分大小写)");
        GUILayout.Label("被引用的物体需要激活");
    }
}
