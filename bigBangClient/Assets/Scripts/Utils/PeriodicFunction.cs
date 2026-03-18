using UnityEngine;

/// <summary>
///  周期函数
///  主要用于制作循环变化的表现效果
///  变化范围从0到1再到0
/// </summary>
public static class PeriodicFunction
{
    /// <summary>
    /// 线性函数
    /// 在0和1直接线性变化
    /// </summary>
    /// <param name="value"></param>
    /// <returns>0到1范围内的值</returns>
    public static float Linear(float value)
    {
        value -= (int)value;
        return value < 0.5f ? value : (1 - value);
    }

    /// <summary>
    /// 经过平移和伸缩变化的三角函数
    /// 在0和1之间按三角函数的规律变化
    /// </summary>
    /// <param name="value">任意值</param>
    /// <returns>0到1范围内的值</returns>
    public static float Trigonometric(float value)
    {
        value -= (int)value;
        return (1 - Mathf.Cos(2 * Mathf.PI * value)) / 2;
    }

    /// <summary>
    /// 半圆函数
    /// </summary>
    /// <param name="value">任意值</param>
    /// <returns>0到1范围内的值</returns>
    public static float Semicircle(float value)
    {
        value -= (int)value;
        return Mathf.Sqrt(1 - Mathf.Pow(value - 0.5f, 2));
    }

    /// <summary>
    /// 突变函数
    /// 在区间前半段返回0,后半段返回1
    /// </summary>
    /// <param name="value">任意值</param>
    /// <returns>非0即1的值</returns>
    public static float Abrupt(float value)
    {
        value -= (int)value;
        return value < 0.5f ? 0 : 1;
    }
}