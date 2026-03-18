using System;
using UnityEditor;
using UnityEngine;

public class ProcessAllPrefab
{
    private static string prefabChineseName = "所有预制体";
    [MenuItem("工具/3D/处理所有预制体", false, 2000)]
    public static void StartProcessAllPrefab()
    {
        try
        {
            Debug.Log("开始处理" + prefabChineseName);

            TrainPrefabCreator.ProcessTrainPrefab();
            Battle2CardPrefabCreator.ProcessBattle2CardPrefab();
            Battle2BallPrefabCreator.ProcessBattle2BallPrefab();
            Battle2PrefabCreator.ProcessBattle2Prefab();

            Debug.Log(prefabChineseName + "处理完成");
        }
        catch (Exception ex)
        {
            Debug.Log(prefabChineseName + "处理出错");
            Debug.LogError(ex);
        }
    }
}