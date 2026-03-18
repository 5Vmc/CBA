using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 改变游戏运行速度
/// </summary>
public class ChangeTimeScale
{

    [MenuItem("游戏控制/TimeScale 0.1")]
    private static void TimeScale01()
    {
        SetTimeScale(0.1f);
    }
    [MenuItem("游戏控制/TimeScale 0.5")]
    private static void TimeScale05()
    {
        SetTimeScale(0.5f);
    }
    [MenuItem("游戏控制/TimeScale 1")]
    private static void TimeScale1()
    {
        SetTimeScale(1);
    }
    [MenuItem("游戏控制/TimeScale 5")]
    private static void TimeScale5()
    {
        SetTimeScale(5);
    }
    [MenuItem("游戏控制/TimeScale 20")]
    private static void TimeScale20()
    {
        SetTimeScale(20);
    }

    private static void SetTimeScale(float timeScale)
    {
        UnityEngine.Time.timeScale = timeScale;
        Debug.Log("游戏速度更改为" + timeScale);
    }
}
