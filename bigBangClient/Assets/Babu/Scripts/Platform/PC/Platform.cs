#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
using UnityEngine;

namespace Babu
{
    public class Platform
    {
        public static void RequireIDFA()
        {

        }

        public static int GetIDFAAuthorizationStatus()
        {
            return 0;
        }

        public static string GetIDFA()
        {
            return "null";
        }

        public static void JumpToIDFASetting()
        {
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
            return Application.dataPath;
        }

        public static string GetSystemLanguage()
        {
            return "zh-cn";
        }

        public static string GetPackageName()
        {
            return "unknown";
        }

        public static string GetCountryCode()
        {
            return "CN";
        }

        public static string GetSimCountryCode()
        {
            return "CN";
        }

        public static string GetCountryName()
        {
            return "中国";
        }

        public static string GetDeviceId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }
    }
}
#endif
