using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;

// 自动引用组件
// 将暴露在检查器（Inspector)面板上的字段添加对应的引用
// 需要保证字段的名称和要引用的物体名称一致就行（不区分大小写）
// 优先级:仅当自动的值为空时才会自动引用
// 如果字段已经被赋值,且自动引用的目标值和当前值不匹配,也不进行自动引用
//
// 支持List<>类型的自动引用
// 例如:
// -ScrollView
// ----Viewport
// --------Content
// ------------Item         ->包含组件Image
// ------------Item(1)      ->包含组件Image
// ------------Item(2)      ->包含组件Image
// ------------Item(3)      ->包含组件Image
//
// [SerializeField] private List<Image> content;
// 保证List字段名称与元素父物体名称相同即可

public class AutoReference : MonoBehaviour
{
    /// <summary>
    /// 设置引用
    /// </summary>
    /// <param name="script">目标脚本</param>
    /// <returns>返回修改个数</returns>
    public static int SetReference(UnityObject script)
    {
        int refCount = 0;
        var type = script.GetType();
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
              .Where(item => Attribute.IsDefined(item, typeof(SerializeField)));
        foreach (var field in fields)
        {
            bool isGameObject = field.FieldType.IsEquivalentTo(typeof(GameObject));
            // 如果是GameObject类型，获得Transform类型
            var fieldType = isGameObject ? typeof(Transform) : field.FieldType;
            // 如果是数组类型
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var list = field.GetValue(script) as IList;
                if (!(list == null || list.Count == 0)) continue;

                Undo.RecordObject(script, field.Name);

                // 初始化空数组
                field.SetValue(script, Activator.CreateInstance(fieldType));
                // 获得数组
                list = field.GetValue(script) as IList;
                list.Clear();
                // 获得元素组的父节点
                var content = (type.GetMethod("GetComponentsInChildren", new Type[] { })
                    ?.MakeGenericMethod(typeof(Transform))
                    .Invoke(script, new object[] { }) as Component[])
                    .FirstOrDefault(item => item.name.Trim('@').ToUpper() == field.Name.ToUpper()) as Transform;
                if (content == null) continue;
                // 泛型类型
                var genericType = fieldType.GetGenericArguments().First();
                refCount++;
                for (int i = 0; i < content.childCount; i++)
                {
                    if (genericType.IsEquivalentTo(typeof(GameObject)))
                    {
                        list.Add(content.GetChild(i).gameObject);
                    }
                    else
                    {
                        // 给数组添加元素
                        list.Add(content.GetChild(i).GetComponent(genericType.Name));
                    }
                }
                continue;
            }
            if (!fieldType.IsSubclassOf(typeof(Component))) continue;
            var method = type.GetMethod("GetComponentsInChildren", new Type[] { }).MakeGenericMethod(fieldType);
            var components = method.Invoke(script, new object[] { }) as Component[];
            var target = components.FirstOrDefault(item =>
            {
                return field.Name.ToUpper() == (item.name.Trim('@').ToUpper());
            });
            if (target == null) continue;

            Undo.RecordObject(script, field.Name);

            var sourceValue = field.GetValue(script);
            if (!(sourceValue == null || (sourceValue as UnityObject) == null)) continue;
            refCount++;
            field.SetValue(script, isGameObject ? target.gameObject as UnityObject : target);
        }
        EditorUtility.SetDirty(script);
        return refCount;
    }
}