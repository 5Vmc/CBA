using System;
using UnityEngine;

// 忽略空引用警告
[AttributeUsage(AttributeTargets.Field|AttributeTargets.Class)]
public class IgnoreNullWarningAttribute : PropertyAttribute
{
}