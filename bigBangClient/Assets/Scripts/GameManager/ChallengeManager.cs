using Babu;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using UnityEngine;
using Utils;
using YooAsset;

namespace BigBang
{
    public class ChallengeManager : BabuSingleton<ChallengeManager>
    {
        private AssetOperationHandle handle;
        private GameObject assetInstance;

        public void LoadContinentModel(int mapId)
        {
            ChallengeMapConfig challengeMapConfig = Configs.ChallengeMap.GetConfig(mapId);
            if(challengeMapConfig == null)
            {
                Debug.LogError("ChallengeManager , LoadContinentModel , challengeMapConfig == null , mapId = " + mapId.ToString());
                return;
            }
            string prefab = challengeMapConfig.ScenePrefab;
            string path = $"{ResourcePath.LanqiuPath}{prefab}.prefab";
            
            handle = YooAssets.LoadAssetSync<GameObject>(path);
            assetInstance = handle.InstantiateSync(transform);
            ProcessRes();
        }

        public void ReleaseInstance()
        {
            GameObject.Destroy(assetInstance);
            handle.Release();
        }

        /// <summary>
         ///- [ ] 自动代码导入转盘资源
         ///   - [ ] 挂载摄像机初始化脚本
         ///   - [ ] 摄像机Cullingmask
         ///   - [ ] 去掉摄像机上的声音接收器
         ///   - [ ] 设置Xuanzhuan及其所有子节点的layer
         ///   - [ ] 设置动态生成的棋子及其所有子节点的layer（动态生成时设置）
        /// </summary>
        public void ProcessRes()
        {
            Camera camera = assetInstance.transform.Find("Xuanzhuan").Find("Camera").GetComponent<Camera>();
            if(camera == null)
            {
                Debug.LogError("找不到挑战转盘的摄像机");
            }
            camera.gameObject.SetActive(false);
            float cameraFOV = Utility.Lerp(30f, 36f, UIFrame.GetFixScreenLerpT());
            camera.fieldOfView = cameraFOV;

            if (assetInstance.transform.GetComponent<CameraInitializer>() == null)//挂载摄像机初始化脚本
            {
                CameraInitializer cameraInitializer = assetInstance.AddComponent<CameraInitializer>();
                cameraInitializer.RenderCamera = camera;
                cameraInitializer.ID = CameraID.Challenge;
                CameraManager.Instance.Register(CameraID.Challenge, camera);
            }
            assetInstance.SetCullingMaskInThisAndAllChild(Layers.Wheel);//设置光照和摄像机的CullingMask
            if (camera.transform.GetComponent<AudioListener>() != null)//去掉摄像机上的声音接收器
            {
                camera.transform.GetComponent<AudioListener>().enabled = false;
            }
            assetInstance.SetLayerInThisAndAllChild(Layers.Wheel);//设置Xuanzhuan及其所有子节点的layer

        }

    }
}