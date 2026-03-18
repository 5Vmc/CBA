using Babu;
using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using YooAsset;
using Task = System.Threading.Tasks.Task;

public class GameInitialization : MonoBehaviour
{

    [SerializeField] private Slider progress;
    [SerializeField] private Text descTxt;

    [SerializeField] public Canvas FirstBgCanvas;
    //[SerializeField] public Image PlayersImage;

    [SerializeField] private RectTransform updateProgressPanel = null;//更新进度条
    [SerializeField] private RectTransform updateClientPanel = null;//显示重新下载客户端
    [SerializeField] private Button openUrlButton = null;
    [SerializeField] private RectTransform updateClientRetryPanel = null;//重试获取更新地址
    [SerializeField] private Button updateClientRetryButton = null;
    [SerializeField] private RectTransform updateWaitPanel = null;//等待转圈
    [SerializeField] public GameObject tempEventSystemGo = null;//临时的事件系统go

    [SerializeField] public GameObject BeforeHotFixSdks;

    [SerializeField] public ConfirmationPolicyPanel confirmationPolicyPanel;

    private void Awake()
    {
        _instance = this;

        //PlayersImage.transform.localScale = Vector3.one * Lerp(0.74f, 1f, GetScreenLerpT());
#if UNITY_ANDROID || UNITY_EDITOR
        Invoke("CheckPolicy", 0.01f);
#else
        Invoke("DoUpdate", 0.01f);
#endif
    }

    private void CheckPolicy()
    {
#if JumpPolicy
        PlayerPrefs.SetInt("read_toggle", 1);
        DoUpdate();
#else
        if (PlayerPrefs.GetInt("read_toggle", 0) == 0)
        {
            confirmationPolicyPanel.beforeConfirmAnimCallBack = () =>
            {
                updateWaitPanel.gameObject.SetActive(true);
                PlayerPrefs.SetInt("read_toggle", 1);
                DoUpdate();
            };
            confirmationPolicyPanel.afterConfirmAnimCallBack = () =>
            {
                this.gameObject.SetActive(false);
            };
            confirmationPolicyPanel.gameObject.SetActive(true);
        }
        else
        {
            DoUpdate();
        }
#endif
    }

    private readonly int retryMaxTimes = 5;
    private int retryNowTimes = 0;
    public static bool canContinueDoing = true;
    async private void DoUpdate()
    {
#if !UNITY_EDITOR
#if UNITY_WEBGL
        Application.targetFrameRate = 45;//小游戏限制到 45 帧
#else
        Application.targetFrameRate = 60;
#endif
#endif

        SetLogLevel();
        Babu.Environment.LoadEnvironment();
        ReportEnvironment();

#if UNITY_IOS
        //ios发起 ATT 弹窗
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            int curStatus = ATTAuth.GetAppTrackingAuthorizationStatus();
            if (curStatus == 0)
            {
                ATTAuth.RequestTrackingAuthorizationWithCompletionHandler((status) =>
                {
                    Debug.Log("ATT status :" + status);
                });
            }
        }
#endif

        Debug.Log("BeforeHotFixSdks Start");
        GameObject.Instantiate(BeforeHotFixSdks);
        Debug.Log("BeforeHotFixSdks End");

        bool isFullRes = Babu.Environment.GetValue("full_res", true);
#if UNITY_EDITOR
        isFullRes = true;
#endif

        Input.multiTouchEnabled = false;
        DOTween.SetTweensCapacity(1250, 200);

        canContinueDoing = true;
        retryNowTimes = 0;



        if (!isFullRes)
        {
            updateClientRetryButton.onClick.AddListener(OnUpdateClientRetryButton);
            openUrlButton.onClick.AddListener(OnOpenUrlButton);
            await LoadResAddress();
            await YooAssetInitializer.Initialize();
            Version nowVersion = new Version(Babu.Environment.GetValue<string>("major_version", "0.0.0"));
            Version minVersion = new Version(cbaBundleResVersionData.clientMinVersion);
            if (nowVersion < minVersion)
            {
                updateWaitPanel.gameObject.SetActive(false);
                updateClientPanel.gameObject.SetActive(true);
                bool hasUpdateClientUrl = string.IsNullOrWhiteSpace(cbaBundleResVersionData.downloadClientUrl) == false;
                openUrlButton.gameObject.SetActive(hasUpdateClientUrl);
                return;
            }

            updateWaitPanel.gameObject.SetActive(false);
            updateClientRetryPanel.gameObject.SetActive(false);
            updateProgressPanel.gameObject.SetActive(true);
            await StartInit();
            while (canContinueDoing == false)
            {
                canContinueDoing = true;
                retryNowTimes++;
                if (retryNowTimes > retryMaxTimes)
                {
                    descTxt.text = "配置文件加载失败，请尝试重新进入游戏";
                    break;
                }
                else
                {
                    descTxt.text = "配置文件加载失败，稍后重新尝试（" + retryNowTimes + "）";
                }
                await Task.Delay(3000);
                await StartInit();
            }
        }
        else
        {
            await YooAssetInitializer.Initialize();

            //await LoadDLLs();
        }

#if !RELEASE
        // 实例化控制台
        YooAssets.LoadAssetAsync<GameObject>("Prefabs/Launch/IngameDebugConsole.prefab").Completed += (h) =>
        {
            h.InstantiateSync();
        };

        // 实例化FPS分析
        YooAssets.LoadAssetAsync<GameObject>("Prefabs/Launch/FPS Counter.prefab").Completed += (h) =>
        {
            h.InstantiateSync();
        };
#endif

        if (canContinueDoing == true)
        {
            Destroy(tempEventSystemGo);
            // 实例化UI框架
            {
                var h = YooAssets.LoadAssetAsync<GameObject>("Prefabs/Launch/UIFrame.prefab");
                await h.Task;
                h.InstantiateSync();
            }

            {
                var h = YooAssets.LoadAssetAsync<GameObject>("Prefabs/Launch/AfterHotFix.prefab");
                await h.Task;
                h.InstantiateSync();
            }

            {
                var h = YooAssets.LoadAssetAsync<GameObject>("Prefabs/Launch/AudioManager.prefab");
                await h.Task;
                h.InstantiateSync();
            }

            {
                var h = YooAssets.LoadAssetAsync<GameObject>("Prefabs/Launch/Entry.prefab");
                await h.Task;
                h.InstantiateSync();
            }
        }
    }

    private void ReportEnvironment()
    {

    }

    private void SetLogLevel()
    {
#if !UNITY_WEBGL

#if RELEASE && !UNITY_EDITOR
        // 开启SDK的日志打印，发布版本请务必关闭
        Debug.unityLogger.filterLogType = LogType.Warning;
#else
        Debug.unityLogger.filterLogType = LogType.Log;
#endif

#endif
    }

    private async Task LoadResAddress()
    {
        updateWaitPanel.gameObject.SetActive(true);
        updateClientRetryPanel.gameObject.SetActive(false);
        getResAddressTimes = 0;
        isGetResAddressFinish = false;
        await LoadResAddressAsync();
        if (isGetResAddressFinish == false)
        {
            updateWaitPanel.gameObject.SetActive(false);
            updateClientRetryPanel.gameObject.SetActive(true);
        }
        else
        {
            string majorVersion = Babu.Environment.GetValue("major_version", "").ToString();
            Debug.Log("majorVersion = " + majorVersion);
            string bundleUrl = cbaBundleResVersionData.GetBundleUrl(majorVersion);
            Debug.Log("bundleUrl = " + bundleUrl);
            //将bundleUrl设置为更新地址
            RemoteLoadPath.SetLoadPath(bundleUrl);
        }

        while (true)
        {
            if (isGetResAddressFinish)
            {
                break;
            }
            await Task.Delay(100);
        }
    }
    private async void OnUpdateClientRetryButton()
    {
        await LoadResAddress();
    }
    private void OnOpenUrlButton()
    {
        Application.OpenURL(cbaBundleResVersionData.downloadClientUrl);
    }

    private bool isGetResAddressFinish = false;
    private int getResAddressTimes = 0;
    private CbaBundleResVersionData cbaBundleResVersionData = null;

    // 热更新使用，表明客户端版本和配置文件版本的对应关系，详见“/bigBang/docs/热更新说明/热更新说明.xlsx”
    public const string CbaBundleResVersionJsonUrlAndroidOnline = "https://cba.gateway.ximiplay.com/hotfix/CbaBundleResVersionAndroid.json";//Android外网正式地址
    public const string CbaBundleResVersionJsonUrlAndroidTest = "https://cba.gateway.ximiplay.com/hotfix/CbaBundleResVersionAndroidTest.json";//Android测试热更地址
    public const string CbaBundleResVersionJsonUrlIosOnline = "https://cba.gateway.ximiplay.com/hotfix/CbaBundleResVersionIos.json";//Ios外网正式地址
    public const string CbaBundleResVersionJsonUrlIosTest = "https://cba.gateway.ximiplay.com/hotfix/CbaBundleResVersionIosTest.json";//Ios测试热更地址
    public const string CbaBundleResVersionJsonUrlAndroidPrelaunch = "https://cba.gateway.ximiplay.com/hotfix/CbaBundleResVersionAndroidPreOnline.json";//Android预发布热更地址
    public const string CbaBundleResVersionJsonUrlIosPrelaunch = "https://cba.gateway.ximiplay.com/hotfix/CbaBundleResVersionIosPreOnline.json";//Ios预发布热更地址

    public static string BUNDLE_JSON
    {
        get
        {
            string UsedCbaBundleResVersionJsonUrl = UnityEngine.PlayerPrefs.GetString("CbaBundleResVersionJsonUrl", "");
            if (UsedCbaBundleResVersionJsonUrl == "")
            {
#if !RELEASE
#if UNITY_IOS
                string defaultCbaBundleResVersionJsonUrl = CbaBundleResVersionJsonUrlIosTest;
#else
                string defaultCbaBundleResVersionJsonUrl = CbaBundleResVersionJsonUrlAndroidTest;
#endif
#else
#if UNITY_IOS
                string defaultCbaBundleResVersionJsonUrl = CbaBundleResVersionJsonUrlIosOnline;
#else
                string defaultCbaBundleResVersionJsonUrl = CbaBundleResVersionJsonUrlAndroidOnline;
#endif
#endif
                UsedCbaBundleResVersionJsonUrl = defaultCbaBundleResVersionJsonUrl;
                UnityEngine.PlayerPrefs.SetString("CbaBundleResVersionJsonUrl", UsedCbaBundleResVersionJsonUrl);
            }
            return UsedCbaBundleResVersionJsonUrl;
        }
    }

    private async Task LoadResAddressAsync()
    {
        try
        {
            getResAddressTimes++;

            Debug.Log("LoadResAddressAsync , BUNDLE_JSON = " + BUNDLE_JSON);
            UnityWebRequest unityWebRequest = UnityWebRequest.Get(BUNDLE_JSON);
            unityWebRequest.timeout = 3;
            await unityWebRequest.SendWebRequest();
            if (unityWebRequest.result != UnityWebRequest.Result.Success) throw new Exception("LoadResAddressAsync , unityWebRequest.result = " + unityWebRequest.result);
            string resultStr = unityWebRequest.downloadHandler.text;
            if (string.IsNullOrEmpty(resultStr)) throw new Exception("LoadResAddressAsync , string.IsNullOrEmpty(resultStr)");
            Debug.Log(resultStr);
            //cbaBundleResVersionData = JsonUtility.FromJson<CbaBundleResVersionData>(resultStr);//Unity自带的这种不能处理List<T>类型
            cbaBundleResVersionData = JsonConvert.DeserializeObject<CbaBundleResVersionData>(resultStr);
            if (cbaBundleResVersionData == null) throw new Exception("LoadResAddressAsync , cbaBundleResVersionData == null");
            isGetResAddressFinish = true;

        }
        catch (Exception ex)
        {
            if (getResAddressTimes <= 5)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                await LoadResAddressAsync();
            }
            else
            {
                Debug.LogErrorFormat("配置文件加载({0})\n失败:" + ex.Message, BUNDLE_JSON);
            }
        }

    }

    async private Task StartInit()
    {

//#if UNITY_ANDROID
//        Debug.Log("检查配置文件");
//        descTxt.text = "资源文件加载中";
//#endif
//#if UNITY_IOS
//        descTxt.text = "配置文件加载中";
//#endif

//        // 检查是否有配置文件更新
//        bool hasUpdate = await CheckForUpdateAsync();
//        if (hasUpdate)
//        {
//            // 获得更新大小
//#if UNITY_ANDROID
//            descTxt.text = "正在检查资源文件...";
//#endif
//            double downloadSize = await GetDownloadSizeAsync();
//#if UNITY_ANDROID
//            Debug.Log("资源文件大小:" + GetAssetSize(downloadSize));
//            descTxt.text = "资源文件大小: " + GetAssetSize(downloadSize);
//#endif
//            // 下载更新配置文件
//            await DownloadAssetAsync();
//        }
//        else
//        {
//#if UNITY_ANDROID
//            descTxt.text = "暂无资源文件";
//            Debug.Log("暂无资源文件");
//#endif
//            progress.DOValue(1, 0.1f);
//            await Task.Delay(100);
//        }
//#if UNITY_ANDROID
//        descTxt.text = "资源文件解压中，此过程不消耗流量";
//#endif
//        await Task.Delay(10);

//        if (canContinueDoing == true)
//        {
//            await LoadDLLs();
//        }
    }

    async private Task LoadDLLs()
    {
//#if !UNITY_EDITOR && !UNITY_WEBGL
//        //加载更新Dll
//        try
//        {
//            var h = YooAssets.LoadAssetAsync<TextAsset>(hotFixInitData.HotFixDllKey);
//            await h.Task;
//            var hotFixDll = h.AssetObject as TextAsset;
//            var aotDlls = new List<TextAsset>();
//            foreach (var key in hotFixInitData.AOTDllKeys)
//            {
//                aotDlls.Add(await Resources.LoadAsync<TextAsset>(key) as TextAsset);
//            }
//            HotFixManager.Init(hotFixDll, aotDlls);
//        }
//        catch (Exception ex)
//        {
//            canContinueDoing = false;
//            Debug.LogError("Load Dlls Error");
//            Debug.LogError(ex);
//            return;
//        }
//#endif
    }

    private static GameInitialization _instance = null;
    public static GameInitialization Inatance
    {
        get { return _instance; }
    }

    public void RemoveFirstBgCanvas()
    {
        if (FirstBgCanvas == null) return;
        GameObject.Destroy(FirstBgCanvas.gameObject);
        FirstBgCanvas = null;
    }

    /// <summary>
    /// 线性插值
    /// Unity自带的插值函数限定了t在0到1区间，使用此函数允许突破范围
    /// </summary>
    /// <param name="from">t=0时的值</param>
    /// <param name="to">t=1时的值</param>
    /// <param name="t">0-1对应from-to</param>
    /// <returns>插值结果</returns>
    public float Lerp(float from, float to, float t)
    {
        return from + (to - from) * t;
    }
    /// <summary>
    /// 获取Lerp用的T值
    /// 16:9为0，21:9为1，可能会超过0和1
    /// </summary>
    public float GetScreenLerpT()
    {
        float hw169 = 16.0f / 9.0f;
        float hw219 = 21.0f / 9.0f;
        float hwScreen = (float)Screen.height / (float)Screen.width;
        float screenT = (hwScreen - hw169) / (hw219 - hw169);
        return screenT;
    }


    #region 热更新

    private static List<string> catalogs;
    private static List<object> keys = new List<object>();
    private static double sumBytes = 0;
    private static double downloadedBytes = 0;
    public static Action OnDownloadCompleted;

    //检查是否有配置文件更新
    public static async Task<bool> CheckForUpdateAsync()
    {
        try
        {
            var nowStaticVersion = PlayerPrefs.GetInt("Version") == 0 ? YooAssets.GetResourceVersion() : PlayerPrefs.GetInt("Version");
            var staticVersionOperation = YooAssets.UpdateStaticVersionAsync();
            await staticVersionOperation.Task;

            if (staticVersionOperation.Status != EOperationStatus.Succeed)
            {
                Debug.LogError("UpdateStaticVersionAsync Error: " + staticVersionOperation.Error + "\n RemoteLoadPath.LoadPath: " + RemoteLoadPath.LoadPath);
                return false;
            }

            if (staticVersionOperation.ResourceVersion > nowStaticVersion)
            {
                Debug.Log($"发现新配置：{nowStaticVersion} -> {staticVersionOperation.ResourceVersion}");
                var manifestOperation = YooAssets.UpdateManifestAsync(staticVersionOperation.ResourceVersion);
                await manifestOperation.Task;

                if (manifestOperation.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError("UpdateManifestAsync Error: " + manifestOperation.Error + "\n RemoteLoadPath.LoadPath: " + RemoteLoadPath.LoadPath);
                    return false;
                }
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            canContinueDoing = false;
            Debug.LogError("CheckForUpdateAsync Error");
            Debug.LogError(ex);
            return false;
        }
    }


    //获得更新大小
    public static async Task<double> GetDownloadSizeAsync()
    {


        try
        {
            var downloadingMaxNum = int.MaxValue;
            var failedTryAgain = int.MaxValue;
            DownloaderOperation downloader = YooAssets.CreatePatchDownloader(downloadingMaxNum, failedTryAgain);
            return downloader.TotalDownloadBytes;
        }
        catch (Exception ex)
        {
            canContinueDoing = false;
            Debug.LogError("GetDownloadSizeAsync Error");
            Debug.LogError(ex);
            return -1;
        }
    }

    //下载配置文件
    public async Task DownloadAssetAsync()
    {
        try
        {
            downloadedBytes = 0;
            progress.value = 0;

            var downloadingMaxNum = int.MaxValue;
            var failedTryAgain = int.MaxValue;
            DownloaderOperation downloader = YooAssets.CreatePatchDownloader(downloadingMaxNum, failedTryAgain);

            // 注册回调方法
            downloader.OnDownloadProgressCallback = (int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes) =>
            {
                progress.DOValue((float)currentDownloadBytes / (float)totalDownloadBytes, 0.1f);
#if UNITY_ANDROID
                descTxt.text = "正在下载中：" + GetAssetSize(currentDownloadBytes) + "/" + GetAssetSize(totalDownloadBytes);
#endif
            };
            downloader.OnDownloadErrorCallback = (string fileName, string error) =>
            {
#if UNITY_ANDROID
                Debug.Log("资源文件 " + fileName + " 加载失败");
#endif
            };

            downloader.BeginDownload();
            await downloader.Task;

            //设置进度条（针对没有配置文件更新的情况）
            progress.DOValue(1, 0.1f);
        }
        catch (Exception ex)
        {
            canContinueDoing = false;
            Debug.LogError("DownloadAssetAsync Error");
            Debug.LogError(ex);
            return;
        }
    }

    //删除本地所有配置文件缓存
    public static void ClearCache()
    {
        Caching.ClearCache();
    }

    //转换
    public static string GetAssetSize(double sumByte)
    {
        //保留2位小数
        if (sumByte < 1000)
        {
            return Math.Round(sumByte, 2) + "B";
        }
        else if (sumByte < 1000000)
        {
            return Math.Round(sumByte / 1000, 2) + "KB";
        }
        else if (sumByte < 1000000000)
        {
            return Math.Round(sumByte / 1000000, 2) + "MB";
        }
        else
        {
            return Math.Round(sumByte / 1000000000, 2) + "GB";
        }
    }

    #endregion


}

public class UnityWebRequestAwaiter : INotifyCompletion
{
    private UnityWebRequestAsyncOperation asyncOp;
    private Action continuation;

    public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
    {
        this.asyncOp = asyncOp;
        asyncOp.completed += OnRequestCompleted;
    }

    public bool IsCompleted { get { return asyncOp.isDone; } }

    public void GetResult() { }

    public void OnCompleted(Action continuation)
    {
        this.continuation = continuation;
    }

    private void OnRequestCompleted(AsyncOperation obj)
    {
        continuation();
    }
}

public static class UnityWebRequestExtensionMethods
{
    public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
    {
        return new UnityWebRequestAwaiter(asyncOp);
    }
}

public class ResourceRequestAwaiter : INotifyCompletion
{
    private ResourceRequest asyncOp;
    private Action continuation;

    public ResourceRequestAwaiter(ResourceRequest asyncOp)
    {
        this.asyncOp = asyncOp;
        asyncOp.completed += OnRequestCompleted;
    }

    public bool IsCompleted { get { return asyncOp.isDone; } }

    public object GetResult() { return asyncOp.asset; }

    public void OnCompleted(Action continuation)
    {
        this.continuation = continuation;
    }

    private void OnRequestCompleted(AsyncOperation obj)
    {
        continuation();
    }
}

public static class ResourceRequestExtensionMethods
{
    public static ResourceRequestAwaiter GetAwaiter(this ResourceRequest asyncOp)
    {
        return new ResourceRequestAwaiter(asyncOp);
    }
}