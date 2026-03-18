using BigBang;
using BigBang.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IngameDebugConsole.Commands
{
    /// <summary>
    /// 控制台命令
    /// </summary>
    public static class ConsoleCommand
    {
#if !RELEASE
        //[ConsoleMethod("guide.reset", "重置新手引导"), UnityEngine.Scripting.Preserve]
        //public static void OnResetGuide()
        //{
        //    Debug.Log("新手引导已重置");
        //    GuideManager.Clear();
        //}

        [ConsoleMethod("guide.finish", "完成所有引导"), UnityEngine.Scripting.Preserve]
        public static void OnFinishGuide()
        {
            Debug.Log("已完成所有新手引导");
            GuideManager.FinishAll();
        }

        [ConsoleMethod("guide.show", "显示新手引导完成状态"), UnityEngine.Scripting.Preserve]
        public static void OnShowGuide()
        {
            var type = typeof(GuideID);
            foreach (var item in type.GetEnumValues())
            {
                var id = (GuideID)item;
                string name = ((GuideID)item).ToString();
                Debug.Log(name + " : " + GuideManager.IsFinished(id));
            }
        }

        [ConsoleMethod("bgm.on", "开启背景音乐"), UnityEngine.Scripting.Preserve]
        public static void OnEnableMusic()
        {
            AudioManager.Instance.EnableMusic();
        }

        [ConsoleMethod("bgm.off", "关闭背景音乐"), UnityEngine.Scripting.Preserve]
        public static void OnDisableMusic()
        {
            AudioManager.Instance.DisableMusic();
        }

        [ConsoleMethod("sound.on", "开启音效"), UnityEngine.Scripting.Preserve]
        public static void OnEnableSound()
        {
            AudioManager.Instance.EnableSound();
        }

        [ConsoleMethod("sound.off", "关闭音效"), UnityEngine.Scripting.Preserve]
        public static void OnDisableSound()
        {
            AudioManager.Instance.DisableSound();
        }

        [ConsoleMethod("bgm.volume", "设置背景音乐音量[0.0-1.0]"), UnityEngine.Scripting.Preserve]
        public static void OnSetBGMVolume(float volume)
        {
            PlayerPrefs.SetFloat(PlayerPrefsKeys.BGM, Mathf.Clamp01(volume));
            AudioManager.Instance.MusicVolume = Mathf.Clamp01(volume);
        }

        [ConsoleMethod("bgm.volume", "显示背景音乐音量"), UnityEngine.Scripting.Preserve]
        public static void OnShowBGMVolume()
        {
            Debug.Log(AudioManager.Instance.MusicVolume);
        }

        [ConsoleMethod("load.mode", "设置加载模式(同步加载=0,异步加载=1,按需加载=3)"), UnityEngine.Scripting.Preserve]
        public static void OnSetLoadMode(int mode)
        {
            if (mode == (int)LoadMode.Sync)
            {
                Debug.Log("加载模式切换为同步加载");
                PlayerPrefs.SetInt(PlayerPrefsKeys.LoadMode, (int)LoadMode.Sync);
            }
            else if (mode == (int)LoadMode.Async)
            {
                Debug.Log("加载模式切换为异步加载");
                PlayerPrefs.SetInt(PlayerPrefsKeys.LoadMode, (int)LoadMode.Async);
            }
            else if (mode == (int)LoadMode.Demand)
            {
                Debug.Log("加载模式切换为按需加载");
                Debug.Log("该模式暂不可用");
                //PlayerPrefs.SetInt(PlayerPrefsKeys.LoadMode, (int)LoadMode.Demand);
            }
            else
            {
                Debug.Log("非法代码");
            }
        }

        [ConsoleMethod("load.mode", "显示当前的加载模式"), UnityEngine.Scripting.Preserve]
        public static void OnShowLoadMode()
        {
            Debug.Log(((LoadMode)PlayerPrefs.GetInt(PlayerPrefsKeys.LoadMode)).ToString());
        }

        [ConsoleMethod("game.version", "显示游戏版本号"), UnityEngine.Scripting.Preserve]
        public static void OnShowGameVersion()
        {
            Debug.Log(Application.version.Replace(".", "") + Babu.Environment.GetValue("minor_version", ""));
        }

        [ConsoleMethod("ping", "发送回显信息"), UnityEngine.Scripting.Preserve]
        public static async void OnPing(string ip)
        {
#if !UNITY_WEBGL
            Ping ping = new Ping(ip);
            var time = DateTime.Now;
            while (!ping.isDone)
            {
                await System.Threading.Tasks.Task.Yield();
                if ((DateTime.Now - time).TotalSeconds >= 4)
                {
                    Debug.Log($"Request from {ip} timed out");
                    return;
                }
            }
            Debug.Log($"Reply from {ip} time={ping.time}ms");
            ping.DestroyPing();
#endif
        }

        [ConsoleMethod("debug.close", "关闭控制台"), UnityEngine.Scripting.Preserve]
        public static void OnCloseDebug()
        {
            UnityEngine.Object.FindObjectOfType<FPSCounter>().gameObject.SetActive(false);
            DebugLogManager.Instance.gameObject.SetActive(false);
        }

#endif
        }
}