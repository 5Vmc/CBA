using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;

public static class FindNullReference
{
    // 排除属于该命名空间下的组件的空引用检查
    private static List<string> ignoreNamespaces = new List<string>()
    {
        "UnityEngine.UI",
    };

    // 忽略组件的空引用检查
    public static List<Type> ignoreComponents = new List<Type>()
    {
        typeof(Coffee.UIExtensions.UIParticle),
    };

    public static IEnumerable<(string, UnityEngine.Object)> Find()
    {
        AssetDatabase.Refresh();
        var paths = AssetDatabase.GetAllAssetPaths().Where(item => item.StartsWith("Assets/") && item.EndsWith(".prefab"));
        int fieldCount = 0;
        HashSet<string> scriptSet = new HashSet<string>();
        HashSet<string> prefabSet = new HashSet<string>();
        foreach (var path in paths)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var components = asset.GetComponents(typeof(Component));
            foreach (var component in components)
            {
                if (component == null) continue;

                var type = component.GetType();
                // 排除组件空引用检查
                if (ignoreNamespaces.Exists(item => type.Namespace == item)) continue;
                if (ignoreComponents.Exists(item => type == item)) continue;
                if (Attribute.IsDefined(type, typeof(IgnoreNullWarningAttribute))) continue;
                // 筛选字段
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(item =>
                    {
                        // 排除忽略项
                        if (Attribute.IsDefined(item, typeof(IgnoreNullWarningAttribute))) return false;
                        // 排除不显示再面板上的
                        if (Attribute.IsDefined(item, typeof(HideInInspector))) return false;
                        // 排除数组集合
                        if (item.FieldType.GetInterfaces().ToList().Exists(item => item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IEnumerable<>))) return false;
                        // 筛选在面板上的
                        if (Attribute.IsDefined(item, typeof(SerializeField))) return true;
                        return false;
                    });
                // 统计空字段个数
                foreach (var field in fields)
                {
                    var fieldValue = field.GetValue(component);
                    // 排除值类型
                    if (fieldValue != null && fieldValue.GetType().IsValueType) continue;
                    // 组件块
                    if (Attribute.IsDefined(field.FieldType, typeof(CheckNullAttribute)))
                    {
                        var subFields = fieldValue.GetType().GetFields().Where(item => Attribute.IsDefined(item, typeof(SerializeField)));
                        foreach (var subField in subFields)
                        {
                            var subFieldValue = subField.GetValue(fieldValue);
                            if ((subField.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) && (subFieldValue == null || subFieldValue as UnityEngine.Object == null)) ||
                       subFieldValue is UnityEngine.Object && subFieldValue as UnityEngine.Object == null)
                            {
                                // 统计预制体个数
                                prefabSet.Add(component.name);
                                // 统计脚本个数
                                scriptSet.Add(type.Name);
                                // 统计字段个数
                                fieldCount++;
                                yield return ($"预制体:<color=#4C7CFA>{component.name}</color> 组件:<color=green>{type.Name}</color> 属性:<color=yellow>{subField.Name}</color> = <color=red>None</color> (<color=#4C7CFA>{field.FieldType.Name}:{subField.FieldType.Name}</color>)", component.gameObject);
                            }
                        }
                        continue;
                    }
                    // 包含类型的检查,Unity类型判空
                    if ((field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)) && (fieldValue == null || fieldValue as UnityEngine.Object == null)) ||
                        fieldValue is UnityEngine.Object && fieldValue as UnityEngine.Object == null)
                    {
                        // 统计预制体个数
                        prefabSet.Add(component.name);
                        // 统计脚本个数
                        scriptSet.Add(type.Name);
                        // 统计字段个数
                        fieldCount++;
                        yield return ($"预制体:<color=#4C7CFA>{component.name}</color> 组件:<color=green>{type.Name}</color> 属性:<color=yellow>{field.Name}</color> = <color=red>None</color> (<color=#4C7CFA>{field.FieldType.Name}</color>)", component.gameObject);
                    }
                }
            }
        }
        if (fieldCount > 0 || scriptSet.Count > 0 || prefabSet.Count > 0)
        {
            Debug.Log($"共计:<color=#4C7CFA>{fieldCount}</color>个空引用,涉及脚本:<color=#4C7CFA>{scriptSet.Count}</color>个,涉及预制体:<color=#4C7CFA>{prefabSet.Count}</color>个");
        }
    }
}
