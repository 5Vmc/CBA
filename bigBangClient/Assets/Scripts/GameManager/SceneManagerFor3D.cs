using System;
using UnityEngine;
using YooAsset;

namespace BigBang
{
    public class SceneManagerFor3D
    {
        public static AssetOperationHandle handle;
        public static GameObject assetInstance;

        public static void LoadAddressableSceneAdditive(Action loadCompleteAction = null)
        {
            handle = YooAssets.LoadAssetSync<GameObject>(ResourcePath.LanqiuPath + "ChallengeManager.prefab");
            assetInstance = handle.InstantiateSync();
            loadCompleteAction?.Invoke();
        }

        public static void UnloadAddressableScene()
        {
            GameObject.Destroy(assetInstance);
            handle.Release();
        }
    }
}
