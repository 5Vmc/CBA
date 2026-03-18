#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEngine;

namespace Babu
{
    public class Platform
    {
        [DllImport("__Internal")]
        public static extern void RequireIDFA();

        [DllImport("__Internal")]
        public static extern int GetIDFAAuthorizationStatus();

        [DllImport("__Internal")]
        public static extern string GetIDFA();

        [DllImport("__Internal")]
        public static extern void JumpToIDFASetting();

        [DllImport("__Internal")]
        public static extern string GetSystemLanguage();

        [DllImport("__Internal")]
        public static extern string GetSimCountryCode();

        [DllImport("__Internal")]
        public static extern string GetCountryCode();

        [DllImport("__Internal")]
        public static extern string GetCountryName();

        public static void QuitApp()
        {
            SDK.SDKManager.Instance.CloseGame();
        }

        public static void RequestExternalStoragePermission()
        {
        }

        public static bool HasExternalStoragePermission()
        {
            return false;
        }

        public static string GetExternalStorageDirectory()
        {
            return Application.persistentDataPath;
        }

        public static string GetPackageName()
        {
            return "unknown";
        }

        public static string GetDeviceId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }
    }
}
#endif