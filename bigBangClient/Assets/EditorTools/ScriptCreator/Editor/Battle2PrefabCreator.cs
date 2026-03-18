using System;
using BigBang;
using UnityEditor;
using UnityEngine;
using Utils;

public class Battle2PrefabCreator
{
    private static string[] lightNameArray = { "Directional Light", "Directional Light (1)" };//光照名
    private static string battle2PrefabPath = "Assets/LocalAsset/Lanqiu/_Resources/Prefabs/Zhandou.prefab";
    private static string prefabChineseName = "战斗2prefab";
    [MenuItem("工具/3D/战斗2/处理Zhandou.prefab")]
    public static void ProcessBattle2Prefab()
    {
        try
        {

            Debug.Log("开始处理" + prefabChineseName);

            //加载Prefab
            GameObject battle2GameObject = AssetDatabase.LoadAssetAtPath(battle2PrefabPath, typeof(GameObject)) as GameObject;
            if (battle2GameObject == null)
            {
                Debug.LogError("找不到" + prefabChineseName);
                Debug.LogError("battle2PrefabPath = " + battle2PrefabPath);
                return;
            }
            Transform battle2Trans = battle2GameObject.transform;

            //设置层级
            battle2GameObject.SetLayerInThisAndAllChild(Layers.Battle2);

            //设置相机拍摄哪些物体
            Transform battle2CameraTrans = battle2Trans.Find("Main Camera");
            Camera battle2Camera = battle2CameraTrans.GetComponent<Camera>();
            battle2Camera.cullingMask = 0;
            battle2Camera.cullingMask |= (1 << Layers.Battle2);
            battle2Camera.clearFlags = CameraClearFlags.SolidColor;
            battle2Camera.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0);

            //去掉摄像机上的声音接收器
            AudioListener audioListener = battle2Camera.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = false;
            }

            //挂载摄像机初始化脚本
            if (battle2Trans.GetComponent<CameraInitializer>() == null)
            {
                CameraInitializer cameraInitializer = battle2GameObject.AddComponent<CameraInitializer>();
                cameraInitializer.RenderCamera = battle2Camera;
                cameraInitializer.ID = CameraID.Battle2;
                CameraManager.Instance.Register(cameraInitializer.ID, battle2Camera);
            }
            battle2CameraTrans.gameObject.SetActive(false);

            //设置光源照亮哪些层级
            Transform lightTrans = battle2Trans.Find("Directional Light");
            Light light = lightTrans.GetComponent<Light>();
            light.cullingMask = 0;
            light.cullingMask |= (1 << Layers.Battle2);

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