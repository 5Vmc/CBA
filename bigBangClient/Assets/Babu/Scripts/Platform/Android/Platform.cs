#if UNITY_ANDROID && !UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Android;

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
            AndroidJavaClass jc = new AndroidJavaClass("com.babu.PlatformTool");
            jc.CallStatic("requestExternalStoragePermission");
        }

        public static bool HasExternalStoragePermission()
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.babu.PlatformTool");
            return jc.CallStatic<bool>("hasExternalStoragePermission");
        }

        public static string GetExternalStorageDirectory()
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.babu.PlatformTool");
            return jc.CallStatic<string>("getExternalStorageDirectory");
        }

        public static string GetSystemLanguage()
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.babu.PlatformTool");
            return jc.CallStatic<string>("getSystemLanguage");
        }

        public static string GetPackageName()
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.babu.PlatformTool");
            return jc.CallStatic<string>("getPackageName");
        }

        public static string GetCountryCode()
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.babu.PlatformTool");
            return jc.CallStatic<string>("getCountryCode");
        }

        public static string GetSimCountryCode()
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.babu.PlatformTool");
            return jc.CallStatic<string>("getSimCountryCode");
        }

        public static string GetCountryName()
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.babu.PlatformTool");
            return jc.CallStatic<string>("getCountryName");
        }

        public static string GetDeviceId()
        {
            UnityEngine.Debug.Log("-------------------------- GetDeviceId 1");
            return SystemInfo.deviceUniqueIdentifier;
        }
    }
}
#endif