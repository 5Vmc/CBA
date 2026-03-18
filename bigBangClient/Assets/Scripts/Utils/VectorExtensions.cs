using UnityEngine;

public static class VectorExtensions
{
    /// <summary>
    /// 将向量按指定轴旋转指定角度
    /// </summary>
    /// <param name="source">要旋转的向量</param>
    /// <param name="axis">旋转轴</param>
    /// <param name="angle">旋转角度</param>
    /// <returns>旋转后的向量</returns>
    public static Vector3 Rotate(this Vector3 source, Vector3 axis, float angle)
    {
        var quaternion = Quaternion.AngleAxis(angle, axis);
        return quaternion * source;
    }

    /// <summary>
    /// 按顺时针旋转指定角度
    /// </summary>
    /// <param name="source">要旋转的向量</param>
    /// <param name="angle">旋转角度</param>
    /// <returns>旋转后的向量</returns>
    public static Vector2 Rotate(this Vector2 source, float angle)
    {
        var quaternion = Quaternion.AngleAxis(angle, Vector3.back);
        return quaternion * source;
    }

    /// <summary>
    /// 判断当前向量是否位于2个向量所形成扇形区域内(0°~180°)
    /// </summary>
    /// <param name="source">源向量</param>
    /// <param name="v1">向量1</param>
    /// <param name="v2">向量2</param>
    /// <returns>如果位于扇形区域内返回true,否则返回false</returns>
    public static bool Between(this Vector2 source, Vector2 v1, Vector2 v2)
    {
        if (v1.ClockwiseAngle(v2) > v2.ClockwiseAngle(v1)) (v1, v2) = (v2, v1);

        float angle1 = v1.ClockwiseAngle(source);
        float angle2 = source.ClockwiseAngle(v2);
        float angle3 = v1.ClockwiseAngle(v2);
        return (angle1 < angle3 && angle2 < angle3);
    }

    /// <summary>
    /// 源向量按顺时针方向旋转到目标向量所需要旋转的角度
    /// </summary>
    /// <param name="source">源向量</param>
    /// <param name="targer">目标向量</param>
    /// <returns>按顺时针方向的旋转角度(取值范围0~360)</returns>
    public static float ClockwiseAngle(this Vector2 source, Vector2 targer)
    {
        var angle = Vector2.SignedAngle(targer, source);
        angle = angle > 0 ? angle : 360 + angle;
        return angle;
    }
}