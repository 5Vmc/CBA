public static class AssetBuilder
{
    //[MenuItem("Build/Addressables/打包")]
    //public static void Build()
    //{
    //    AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilderIndex = 3;
    //    AddressableAssetSettings.BuildPlayerContent();
    //}

    //[MenuItem("Build/Addressables/检查")]
    //public static void Check()
    //{
    //    var contentPath = $"{AddressableAssetSettingsDefaultObject.kDefaultConfigFolder}/{PlatformMappingService.GetPlatformPathSubFolder()}/addressables_content_state.bin";
    //    var changeAsset = ContentUpdateScript.GatherModifiedEntriesWithDependencies(AddressableAssetSettingsDefaultObject.Settings, contentPath);
    //    if (changeAsset.Count <= 0) return;
    //    var settings = AddressableAssetSettingsDefaultObject.Settings;
    //    // 创建一个更新组
    //    var updateGroup = settings.CreateGroup("Update", false, false, true, null);
    //    // 为这个组添加设置
    //    var schema = updateGroup.AddSchema<BundledAssetGroupSchema>();
    //    // 设置打包地址
    //    schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
    //    // 设置加载地址
    //    schema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
    //    // 设置打包模式
    //    schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel;
    //    // 设置AB包命名模式
    //    schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
    //    // 设置成动态包
    //    updateGroup.AddSchema<ContentUpdateGroupSchema>().StaticContent = false;
    //    // 将有变动的资源移动到这个新组中
    //    var result = new HashSet<AddressableAssetEntry>();
    //    foreach (var item in changeAsset)
    //    {
    //        result.Add(item.Key);
    //        foreach (var subItem in item.Value)
    //        {
    //            result.Add(subItem);
    //        }
    //    }
    //    settings.MoveEntries(result.ToList(), updateGroup);
    //}

    //[MenuItem("Build/Addressables/更新")]
    //public static void Update()
    //{
    //    var contentPath = $"{AddressableAssetSettingsDefaultObject.kDefaultConfigFolder}/{PlatformMappingService.GetPlatformPathSubFolder()}/addressables_content_state.bin";
    //    ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, contentPath);
    //}

    //[MenuItem("Build/Addressables/清除")]
    //public static void Clear()
    //{
    //    var path = Path.Combine(Application.streamingAssetsPath, "aa");
    //    if (Directory.Exists(path))
    //    {
    //        Directory.Delete(path, true);
    //    }
    //    AddressableAssetSettings.CleanPlayerContent();
    //    BuildCache.PurgeCache(false);
    //}

    //public static void SetIPAddress(string ip)
    //{
    //    var setting = AddressableAssetSettingsDefaultObject.Settings;
    //    setting.profileSettings.SetValue(setting.activeProfileId, "IPAddress", ip);
    //}

    //public static void SetBuildAddress(string address)
    //{
    //    var setting = AddressableAssetSettingsDefaultObject.Settings;
    //    setting.profileSettings.SetValue(setting.activeProfileId, "BuildAddress", address);
    //}
}
