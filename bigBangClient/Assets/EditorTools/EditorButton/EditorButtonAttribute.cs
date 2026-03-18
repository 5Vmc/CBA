using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
public class EditorButtonAttribute : PropertyAttribute
{
    public string Name { get; private set; }
    public bool Play { get; private set; }

    /// <summary>
    /// 在检查器面板上生成该函数功能的按钮
    /// 函数必须是无参函数
    /// </summary>
    /// <param name="name">按钮名称</param>
    /// <param name="play">是否仅在运行模式下启用</param>
    public EditorButtonAttribute(string name, bool play = true)
    {
        Name = name;
        Play = play;
    }
}