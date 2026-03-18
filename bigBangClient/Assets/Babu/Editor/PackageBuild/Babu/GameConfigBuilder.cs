using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Babu.Editor.Build.Babu
{
    // 游戏配置表打包
    class GameConfigBuilder //: IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => BuildOrder.PROCESS_GAME_CONFIG;

        // public void OnPreprocessBuild(BuildReport report)
        // {
        //     BuildUtils.Build(() =>
        //     {
        //         //if (BuildConfigBuilder.Instance.Config.AsJsonObject.ContainsKey("config_path"))
        //         //{
        //         //    string configPath = BuildConfigBuilder.Instance.GetConfig("config_path");
        //         //    EncryptConfigs(configPath);
        //         //}
        //     });
        // }

        // public void OnPostprocessBuild(BuildReport report)
        // {
        //     BuildUtils.Build(() =>
        //     {
        //         //if (BuildConfigBuilder.Instance.Config.AsJsonObject.ContainsKey("config_path"))
        //         //{
        //         //    string configPath = BuildConfigBuilder.Instance.GetConfig("config_path");
        //         //    DecryptConfigs(configPath);
        //         //}
        //     });
        // }

        public static bool EncryptConfigs(string configPath)
        {
            if (Directory.Exists(configPath) == false)
            {
                return false;
            }

            DirectoryInfo dir = new DirectoryInfo(configPath);
            FileInfo[] files = dir.GetFiles("*.csv", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; ++i)
            {
                try
                {
                    Debug.Log("Encrypt Config: " + files[i].FullName);
                    byte[] data = FileUtils.ReadFileBytes(files[i].FullName);
                    byte[] encryptData = DesEncryptor.Encrypt(data);
                    FileUtils.WriteFileBytes(files[i].FullName, encryptData);
                }
                catch (IOException e)
                {
                    Debug.LogError($"EncryptConfigs: {files[i].FullName} failed: {e.Message}");
                    return false;
                }
            }
            return true;
        }

        public static bool DecryptConfigs(string configPath)
        {
            if (Directory.Exists(configPath) == false)
            {
                return false;
            }

            DirectoryInfo dir = new DirectoryInfo(configPath);
            FileInfo[] files = dir.GetFiles("*.csv", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; ++i)
            {
                try
                {
                    byte[] data = FileUtils.ReadFileBytes(files[i].FullName);
                    byte[] decryptData = DesEncryptor.Decrypt(data);
                    FileUtils.WriteFileBytes(files[i].FullName, decryptData);
                }
                catch (IOException e)
                {
                    Debug.LogError($"DecryptConfigs: {files[i].FullName} failed: {e.Message}");
                    return false;
                }
            }
            return true;
        }
    }
}
