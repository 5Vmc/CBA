using System;
using BigBang;
using UnityEditor;
using UnityEngine;
using Utils;

public class TrainPrefabCreator
{

    private static int[] trainLayerArray = { Layers.TrainShenti, Layers.TrainToulan, Layers.TrainKoulan, Layers.TrainKongqiu, Layers.TrainChuanqiu, Layers.TrainFangshou, Layers.TrainGaimao, Layers.TrainLanban, Layers.TrainQiangduan, Layers.TrainWending };//每个训练的prefab层级
    private static string[] stadiumThingNameArray = { "BG", /*"lanqiuchang",*/ "Directional Light", "Directional Light (1)" };//共用部分，放在Stadium层
    private static string[] trainNameArray = { "TrainShenti", "TrainToulan", "TrainKoulan", "TrainKongqiu", "TrainChuanqiu", "TrainFangshou", "TrainGaimao", "TrainLanban", "TrainQiangduan", "TrainWending" };//每个训练的prefab名字，也是Layer的名字，也是CameraID的名字
    private static string[] lightNameArray = { "Directional Light", "Directional Light (1)" };//光照名
    private static string trainActionsPrefabPath = "Assets/LocalAsset/Lanqiu/_Resources/Prefabs/TrainActions.prefab";

    [MenuItem("工具/3D/训练/处理TrainActions.prefab")]
    public static void ProcessTrainPrefab()
    {
        try
        {
            Debug.Log("开始处理训练prefab");

            GameObject trainActionsGameObject = AssetDatabase.LoadAssetAtPath(trainActionsPrefabPath, typeof(GameObject)) as GameObject;
            if (trainActionsGameObject == null)
            {
                Debug.LogError("找不到训练Prefab");
                Debug.LogError("trainActionsPrefabPath = " + trainActionsPrefabPath);
                return;
            }
            Transform trainActionsTrans = trainActionsGameObject.transform;

            foreach (string stadiumThingName in stadiumThingNameArray)//设置公用部分的层级
            {
                Transform stadiumThingTrans = trainActionsTrans.Find(stadiumThingName);
                stadiumThingTrans.gameObject.SetLayerInThisAndAllChild(Layers.Stadium);//共用部分，放在Stadium层
            }
            for (int i = 0; i < trainNameArray.Length; i++)//设置每个训练prefab
            {
                string trainName = trainNameArray[i];//每个训练的prefab放在对应的层
                Transform trainThingTrans = trainActionsTrans.Find(trainName);
                trainThingTrans.gameObject.SetActive(true);
                //Debug.LogWarning(trainName + "  " + trainThingTrans.gameObject.activeSelf);
                int trainLayer = trainLayerArray[i];
                trainThingTrans.gameObject.SetLayerInThisAndAllChild(trainLayer);

                Transform trainCameraTrans = trainThingTrans.Find("Main Camera");//设置这个训练的摄像机拍摄当前prefab和公用部分
                Camera trainCamera = trainCameraTrans.GetComponent<Camera>();
                trainCamera.cullingMask = 0;
                trainCamera.cullingMask |= (1 << Layers.Stadium);
                trainCamera.cullingMask |= (1 << trainLayer);

                AudioListener audioListener = trainCameraTrans.GetComponent<AudioListener>();//去掉摄像机上的声音接收器
                if (audioListener != null)
                {
                    audioListener.enabled = false;
                }

                if (trainThingTrans.GetComponent<CameraInitializer>() == null)//挂载摄像机初始化脚本
                {
                    CameraInitializer cameraInitializer = trainThingTrans.gameObject.AddComponent<CameraInitializer>();
                    cameraInitializer.RenderCamera = trainCamera;
                    cameraInitializer.ID = (CameraID)Enum.Parse(typeof(CameraID), trainName);
                    CameraManager.Instance.Register(cameraInitializer.ID, trainCamera);
                }

                trainCameraTrans.gameObject.SetActive(false);
            }
            foreach (string lightName in lightNameArray)//设置光源照亮哪些层级
            {
                Transform lightTrans = trainActionsTrans.Find(lightName);
                Light light = lightTrans.GetComponent<Light>();
                light.cullingMask = 0;
                light.cullingMask |= (1 << Layers.Stadium);
                foreach (int trainLayer in trainLayerArray)
                {
                    light.cullingMask |= (1 << trainLayer);
                }
            }


            AssetDatabase.SaveAssets();

            Debug.Log("训练prefab处理完成");
        }
        catch (Exception ex)
        {
            Debug.Log("训练prefab处理出错");
            Debug.LogError(ex);
        }
    }
}