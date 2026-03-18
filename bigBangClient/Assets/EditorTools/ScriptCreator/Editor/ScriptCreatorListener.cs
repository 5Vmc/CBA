using System.Collections.Generic;
using UnityEditor;

public class ScriptCreatorListener : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        List<string> list = new List<string>();
        list.AddRange(importedAsset);
        list.AddRange(deletedAssets);
        list.AddRange(movedAssets);
        // 如果图集文件发生改变
        if (list.Exists(item => item.EndsWith("AtlasNames.cs")) || list.Exists(item => item.EndsWith(".spriteatlas")))
        {
            // 更新AtlasNames.cs脚本文件
            AtlasNamesCreator.CreateOrUpdateScript();
        }
#if UNITY_EDITOR_WIN
        // 如果cfg_lang.csv或LangID.cs文件发生改变
        if (list.Exists(item => item.EndsWith("cfg_lang.csv") || item.EndsWith("LangID.cs")))
        {
            // 更新LangID.cs脚本文件
            LangIDCreator.CreateOrUpdateScript();
        }
#endif
        // 如果音频文件发生改变
        if (list.Exists(item => item.EndsWith("AudioNames.cs")) || list.Exists(item => item.EndsWith(".mp3") || item.EndsWith(".ogg") || item.EndsWith(".wav")))
        {
            // 更新AudioNames.cs脚本文件
            AudioNamesCreator.CreateOrUpdateScript();
        }
        // 创建或更新Tags脚本
        TagsCreator.CreateOrUpdateScript();
        // 创建或更新Layers脚本
        LayersCreator.CreateOrUpdateScript();


    }
}
