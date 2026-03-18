using System;
using BigBang.Battle;
using UnityEditor;
using UnityEngine;
using Utils;

public class Battle2BallPrefabCreator
{

    private static string battle2BallPrefabPath = "Assets/LocalAsset/Lanqiu/_Resources/Prefabs/Battle2Ball.prefab";

    private static string prefabChineseName = "战斗2球prefab";
    [MenuItem("工具/3D/战斗2/处理Battle2Ball.prefab")]
    public static void ProcessBattle2BallPrefab()
    {
        try
        {

            Debug.Log("开始处理" + prefabChineseName);

            //加载Prefab
            GameObject battle2BallGameObject = AssetDatabase.LoadAssetAtPath(battle2BallPrefabPath, typeof(GameObject)) as GameObject;
            if (battle2BallGameObject == null)
            {
                Debug.LogError("找不到" + prefabChineseName);
                Debug.LogError("battle2BallPrefabPath = " + battle2BallPrefabPath);
                return;
            }

            //设置层级
            battle2BallGameObject.SetLayerInThisAndAllChild(Layers.Battle2);

            //添加脚本
            if (battle2BallGameObject.GetComponent<Battle2Ball>() == null)
            {
                battle2BallGameObject.AddComponent<Battle2Ball>();
            }

            //保存修改到硬盘
            AssetDatabase.SaveAssets();

            Debug.Log(prefabChineseName + "处理完成");
        }
        catch (Exception ex)
        {
            Debug.Log(prefabChineseName + "处理出错");
            Debug.LogError(ex);
        }
    }
}