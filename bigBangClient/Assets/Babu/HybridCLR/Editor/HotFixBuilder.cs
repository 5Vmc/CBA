using Babu;
using Babu.Editor.Build;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class HotFixBuilder
{

    /// <summary>
    /// Mac上打开文件夹
    /// </summary>
    public static void OpenMacDir(string path)
    {
        ProcessCommand("open", path);
    }
    /// <summary>
    /// Mac执行shell
    /// </summary>
    /// <param name="command">应用路径</param>
    /// <param name="argument">参数</param>
    public static void ProcessCommand(string command, string argument)
    {
        ProcessStartInfo start = new ProcessStartInfo(command);
        start.Arguments = argument;
        start.CreateNoWindow = true;
        start.ErrorDialog = true;
        start.UseShellExecute = true;
        if (start.UseShellExecute)
        {
            start.RedirectStandardOutput = false;
            start.RedirectStandardError = false;
            start.RedirectStandardInput = false;
        }
        else
        {
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;
            start.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
            start.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
        }
        Process p = Process.Start(start);
        if (!start.UseShellExecute)
        {
            Debug.LogFormat("--- output:{0}", p.StandardOutput.ToString());
        }
        p.WaitForExit();
        p.Close();
    }

    /// <summary>
    /// Windows上打开文件夹
    /// </summary>
    public static void OpenWindowsDir(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        path = path.Replace("/", "\\");
        if (!Directory.Exists(path))
        {
            Debug.LogError("No Directory: " + path);
            return;
        }
        System.Diagnostics.Process.Start("explorer.exe", path);
    }



    [MenuItem("热更新/打包")]
    public static void Build()
    {
        Debug.Log("请使用命令行打包");
        Debug.Log(@"参考：/Users/droidhenmini/Documents/bigBang/docs/热更新说明/热更新说明.xlsx");
        //var buildAction = new Action(() =>
        //{
        //    string outputPath = $"{SettingsUtil.ProjectDir}/Build";
        //    var buildOptions = BuildOptions.Development | BuildOptions.ConnectWithProfiler;
        //    string location = outputPath + "/main.apk";
        //    BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
        //    {
        //        scenes = new string[] { "Assets/Scenes/MainScene.unity" },
        //        locationPathName = location,
        //        options = buildOptions,
        //        target = BuildTarget.Android,
        //        targetGroup = BuildTargetGroup.Android,
        //    };
        //    BuildPipeline.BuildPlayer(buildPlayerOptions);
        //});
        //var packAction = new Action(() =>
        //{
        //    AddressableAssetSettings.CleanPlayerContent();
        //    SaveBundleCreateTime();
        //    AddressableAssetSettings.BuildPlayerContent();
        //});
        //BuildAndroid64(buildAction, packAction);
        //AndroidLocalToRemote();
    }

    //[MenuItem("热更新/更新")]
    public static void BuildHotFix()
    {
        string targetPlatform = BuildArgs.GetArgsValue("target_platform", "android");
        if (targetPlatform == "android")
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }
        else if (targetPlatform == "ios")
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        }
        else
        {
            Debug.LogError("BuildHotFix , 不支持的平台");
            return;
        }

        int bundleVersionInt = Builder.CreateBundleVersionInt();
        SaveBundleCreateTime(bundleVersionInt);
        var packAction = new Action(() =>
        {
            AssetDatabase.Refresh();
            Builder.BuildYooAsset(bundleVersionInt);
            //AddressableAssetSettings.BuildPlayerContent();
            //AndroidLocalToRemote(false);
            //OpenHotFixDir();
        });

        BuildAndroid64AB(packAction);
    }

    //public static void AndroidLocalToRemote(bool copyToStreamingAssets)
    //{

    //    //var remoteCatalogName = "catalog";
    //    var remoteCatalogName = "catalog_" + AddressableAssetSettingsDefaultObject.Settings.PlayerBuildVersion;
    //    var aaBuildPath = Path.Combine(Application.dataPath, "..", Addressables.LibraryPath, "aa", "Android");
    //    var remoteBuildPath = Path.Combine(Application.dataPath, "..", "ServerData", "Android");
    //    if (!System.IO.Directory.Exists(remoteBuildPath))
    //    {
    //        System.IO.Directory.CreateDirectory(remoteBuildPath);
    //    }
    //    else
    //    {
    //        //DeleteAllFile(remoteBuildPath);
    //    }
    //    var localCatalogContent = File.ReadAllText(Path.Combine(aaBuildPath, "catalog.json"));

    //    var remoteLoadPath = AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetValueByName(AddressableAssetSettingsDefaultObject.Settings.activeProfileId, "Remote.LoadPath");
    //    remoteLoadPath = remoteLoadPath.TrimEnd('/') + "/";

    //    var content = localCatalogContent.Replace(@"{UnityEngine.AddressableAssets.Addressables.RuntimePath}/Android/", remoteLoadPath);
    //    File.WriteAllText(Path.Combine(remoteBuildPath, remoteCatalogName + ".json"), content);
    //    // catalog hash文件
    //    var bytes = File.ReadAllBytes(Path.Combine(remoteBuildPath, remoteCatalogName + ".json"));
    //    var hash = MD5.Create();
    //    File.WriteAllText(Path.Combine(remoteBuildPath, remoteCatalogName + ".hash"), BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", ""));
    //    // 拷贝Local和unity内置资源包
    //    string fileList = "";// 创建BundleList.txt，表示当前有哪些bundle
    //    foreach (var bundle in Directory.GetFiles(Path.Combine(aaBuildPath, "Android")))
    //    {
    //        var info = new FileInfo(bundle);
    //        if (info.FullName.EndsWith(".bundle") == false) continue;
    //        if (fileList != "") fileList += "|";
    //        fileList += info.Name;//添加bundle的PrimaryKey到BundleList.txt
    //        File.Copy(info.FullName, Path.Combine(remoteBuildPath, info.Name), true);
    //        info.Delete();
    //    }
    //    //添加spriteatlas的bundle的PrimaryKey到BundleList.txt
    //    foreach (var bundle in Directory.GetFiles(remoteBuildPath, "*.bundle", SearchOption.AllDirectories))
    //    {
    //        var info = new FileInfo(bundle);
    //        if (fileList != "") fileList += "|";
    //        if (info.Name.Contains("spriteatlas")) fileList += "spriteatlas_assets_assets/localasset/sprites/atlas/";
    //        fileList += info.Name;
    //    }
    //    File.WriteAllText(Path.Combine(remoteBuildPath, "BundleList.txt"), fileList);
    //    // 复制一份BundleVersion.txt，方便后续查看
    //    File.Copy("Assets/LocalAsset/Texts/BundleVersion.txt", Path.Combine(remoteBuildPath, "BundleVersion.txt"), true);

    //    if (copyToStreamingAssets)
    //    {
    //        CopyDir(remoteBuildPath, Path.Combine(Application.streamingAssetsPath, "AddressableAssetLocalCache"), true);
    //        //CopyDir(Path.Combine(Application.streamingAssetsPath, "AddressableAssetLocalCache", "spriteatlas_assets_assets/localasset/sprites/atlas"), Path.Combine(Application.streamingAssetsPath, "AddressableAssetLocalCache"), false);
    //        //Directory.Delete(Path.Combine(Application.streamingAssetsPath, "AddressableAssetLocalCache", "spriteatlas_assets_assets"), true);
    //    }

    //}
    public static void ClearRemoteBuiltFolder()
    {
        string platform = "";
#if UNITY_ANDROID
        platform = "Android";
#elif UNITY_IOS
        platform = "iOS";
#endif
        var remoteBuildPath = Path.Combine(Application.dataPath, "..", "Bundles", platform);
        if (!System.IO.Directory.Exists(remoteBuildPath))
        {
            System.IO.Directory.CreateDirectory(remoteBuildPath);
        }
        else
        {
            DeleteDirectory(remoteBuildPath);
        }
    }

    /// <summary>
    /// 删除某个文件夹（没有则不删）
    /// </summary>
    /// <param name="fullPath">路径</param>
    /// <param name="rebuild">重建这个文件夹</param>
    /// <returns></returns>
    public static bool DeleteDirectory(string fullPath, bool rebuild = true)
    {
        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, true);
        }
        if (rebuild) System.IO.Directory.CreateDirectory(fullPath);
        return false;
    }
    private static void CopyDir(string srcPath, string aimPath, bool clearBeforeCopy)
    {
        try
        {
            // 检查目标目录是否以目录分割字符结束如果不是则添加
            if (aimPath[aimPath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
            {
                aimPath += System.IO.Path.DirectorySeparatorChar;
            }
            // 判断目标目录是否存在如果不存在则新建
            if (!System.IO.Directory.Exists(aimPath))
            {
                System.IO.Directory.CreateDirectory(aimPath);
            }
            else
            {
                if (clearBeforeCopy)
                {
                    Directory.Delete(aimPath, true);
                    System.IO.Directory.CreateDirectory(aimPath);
                }
            }
            // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
            // 如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法
            // string[] fileList = Directory.GetFiles（srcPath）；
            string[] fileList = System.IO.Directory.GetFileSystemEntries(srcPath);
            // 遍历所有的文件和目录
            foreach (string file in fileList)
            {
                // 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                if (System.IO.Directory.Exists(file))
                {
                    CopyDir(file, aimPath + System.IO.Path.GetFileName(file), clearBeforeCopy);
                }
                // 否则直接Copy文件
                else
                {
                    System.IO.File.Copy(file, aimPath + System.IO.Path.GetFileName(file), true);
                }
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public static void BuildAndroid64(Action buildAction, Action packAction)
    {
//#if !TEST_BUNDLE
//        PrebuildCommand.GenerateAll();
//        CompileDll();

//        //MethodBridgeGeneratorCommand.GenerateMethodBridge();
//        // buildAction?.Invoke();
//        foreach (var item in Directory.GetFiles(InitData.AOTDllsPath)) File.Delete(item);
//#if UNITY_IOS
//        var dlls = Directory.GetFiles(SettingsUtil.GetAssembliesPostIl2CppStripDir(BuildTarget.iOS)).Where(item => item.EndsWith(".dll"));
//#else
//        var dlls = Directory.GetFiles(SettingsUtil.GetAssembliesPostIl2CppStripDir(BuildTarget.Android)).Where(item => item.EndsWith(".dll"));
//#endif
//        foreach (var dll in dlls)
//        {
//            var dllInfo = new FileInfo(dll);
//            if (!InitData.AOTDlls.Contains(dllInfo.Name)) continue;
//            File.Copy(dllInfo.FullName, Path.Combine(InitData.AOTDllsPath, dllInfo.Name + ".bytes"), true);
//        }
//#endif
        // ClearRemoteBuiltFolder();
        AssetDatabase.Refresh();
        packAction?.Invoke();
        buildAction?.Invoke();
    }

    public static void BuildAndroid64AB(Action packAction)
    {
//#if !TEST_BUNDLE
//        // ClearRemoteBuiltFolder();
//        CompileDll();
//#endif
        // ClearRemoteBuiltFolder();
        AssetDatabase.Refresh();
        packAction?.Invoke();
    }

    public static void SaveBundleCreateTime(int bundleVersionInt)
    {
        FileUtils.WriteFile("Assets/LocalAsset/Texts/BundleVersion.txt", bundleVersionInt.ToString());
    }

    public static readonly string uploadUrl = "http://package.win.babuyo.com:8080/";
    [MenuItem("热更新/打开文件服务器网页")]
    public static void OpenUploadPageApk()
    {
        Application.OpenURL(uploadUrl);
    }
    public static async void UploadToCBAFileServer(string folder, string file)
    {
        string path = uploadUrl + folder + "/";
        Debug.Log("start upload file = " + file + " , to path = " + path);
        var responseStr = await UploadToFileServer(path, file);
        Debug.Log("upload response str = " + responseStr);
    }
    public static async Task<string> UploadToFileServer(string uploadUrl, string file)
    {
        var httpClient = new HttpClient();
        httpClient.Timeout = Timeout.InfiniteTimeSpan;
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(File.ReadAllBytes(file)), "file", Path.GetFileName(file));
        var response = await httpClient.PostAsync(uploadUrl, content);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            Debug.LogError("upload file error, file = " + file + ", response code = " + response.StatusCode);
            throw new Exception("upload file error, file = " + file + ", response code = " + response.StatusCode);
        }
        var responseStr = await response.Content.ReadAsStringAsync();
        return responseStr;
    }

}
