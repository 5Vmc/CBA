using System.Collections.Generic;
using UnityEngine;
namespace Utils
{
    public static class PointInRangeUtil
    {
        /// <summary>
        /// 三维向量转换为二维向量（丢弃z）
        /// </summary>
        /// <param name="vec3">三维向量</param>
        /// <returns>二维向量</returns>
        public static Vector2 ToVec2(this Vector3 vec3)
        {
            return new Vector2(vec3.x, vec3.y);
        }

        /// <summary>
        /// 判断目标点是否在多边形内
        /// 射线法，允许凹多边形
        /// 目标点向右水平发射一条射线，与多边形相交点的个数为奇数时，目标点多边形里，反之在多边形外
        /// </summary>
        /// <param name="targetPoint">目标点</param>
        /// <param name="pointList">多边形的节点数组（按顺序）</param>
        /// <param name="listOffset">多边形的节点数组统一偏移量（可空）</param>
        /// <param name="isFlipX">反转横坐标</param>
        /// <returns>点在多边形内部返回true</returns>
        public static bool InRegion(this Vector2 targetPoint, List<Vector2> pointList, Vector2 listOffset = new(), bool isFlipX = false)
        {
            int crossCount = 0;    // 定义变量，统计目标点向右画射线与多边形相交次数
            if (isFlipX == true)
            {
                for (int index = 0; index < pointList.Count; index++)
                {
                    pointList[index] = new Vector2(-pointList[index].x, pointList[index].y);
                }
            }
            for (int i = 0; i < pointList.Count; i++)
            {   //遍历多边形每一个节点
                Vector2 p1;
                Vector2 p2;
                p1 = pointList[i] + listOffset;
                p2 = pointList[(i + 1) % pointList.Count] + listOffset;  // p1是这个节点，p2是下一个节点，两点连线是多边形的一条边
                                                                         // 以下算法是用是先以y轴坐标来判断的
                if (p1.y == p2.y)
                    continue;   //如果这条边是水平的，跳过
                if (targetPoint.y <= Mathf.Min(p1.y, p2.y)) //如果目标点低于这个线段，跳过
                    continue;
                if (targetPoint.y >= Mathf.Max(p1.y, p2.y)) //如果目标点高于这个线段，跳过
                    continue;
                if (targetPoint.x >= Mathf.Max(p1.x, p2.x)) //如果目标点在这个线段右侧，跳过
                    continue;
                //那么下面的情况就是：如果过p1画水平线，过p2画水平线，目标点在这两条线中间
                float x = (targetPoint.y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y) + p1.x;
                // 这段的几何意义是 过目标点，画一条水平线，x是这条线与多边形当前边的交点x坐标
                if (x > targetPoint.x)
                    crossCount++; //如果交点在右边，统计加一。这等于从目标点向右发一条射线（ray），与多边形各边的相交（crossing）次数
            }
            if (isFlipX == true)
            {
                for (int index = 0; index < pointList.Count; index++)
                {
                    pointList[index] = new Vector2(-pointList[index].x, pointList[index].y);
                }
            }
            if (crossCount % 2 == 1)
            {
                return true; //如果是奇数，说明在多边形里
            }
            else
            {
                return false; //否则在多边形外 或 边上
            }
        }
    }
}
