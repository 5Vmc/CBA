using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(MonoBehaviour), true)]
public class EditorButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var mono = target as MonoBehaviour;
        var methods = mono.GetType()
        .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        .Where(item => Attribute.IsDefined(item, typeof(EditorButtonAttribute)));
        foreach (var info in methods)
        {
            var attribute = info.GetCustomAttribute<EditorButtonAttribute>();
            if (info.GetParameters().Length == 0 && GUILayout.Button(attribute.Name))
            {
                var method = info as MethodInfo;
                if (!attribute.Play)
                {
                    method.Invoke(mono, null);
                }
                else if (Application.isPlaying)
                {
                    method.Invoke(mono, null);
                }
                else
                {
                    Debug.Log("仅在运行模式下生效，如需在编辑器模式下生效，请设置play属性为false");
                }
            }
        }
    }
}