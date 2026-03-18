using Babu;
using UnityEngine;
using YooAsset;
using Task = System.Threading.Tasks.Task;
public class YooAssetInitializer
{

    public static Task Initialize()
    {
#if UNITY_EDITOR
        return YooAssetInitializer.InitAssetManagerInEditorMode();
#else
        return YooAssetInitializer.InitAssetManagerInOnlineMode();
#endif
    }

    private static Task InitAssetManagerInEditorMode()
    {
        Debug.Log("初始化资源管理器（编辑器）");
        var resourceCreateParam = new YooAssets.EditorSimulateModeParameters();
        resourceCreateParam.LocationServices = new DefaultLocationServices("Assets/LocalAsset");
        return YooAssets.InitializeAsync(resourceCreateParam).Task;
    }

    private static Task InitAssetManagerInOnlineMode()
    {
        Debug.Log("初始化资源管理器（在线）: " + RemoteLoadPath.LoadPath);
        var resourceCreateParam = new YooAssets.HostPlayModeParameters();
        resourceCreateParam.LocationServices = new DefaultLocationServices("Assets/LocalAsset");
        resourceCreateParam.DefaultHostServer = RemoteLoadPath.LoadPath;
        resourceCreateParam.FallbackHostServer = RemoteLoadPath.LoadPath;
        resourceCreateParam.ClearCacheWhenDirty = false;
        return YooAssets.InitializeAsync(resourceCreateParam).Task;
    }
}