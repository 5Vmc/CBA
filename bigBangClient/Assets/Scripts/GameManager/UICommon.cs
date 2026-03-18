using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.UI;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using UnityEngine;
using Utils;

namespace BigBang
{
    /// <summary>
    /// 存一些 UI 公共数值
    /// </summary>
    public static class UICommon
    {
        public static float TopBarShowY
        {
            get
            {
                return 0;
            }
        }
        public static float TopBarHideY
        {
            get
            {
                return 280;
            }
        }
        //public static bool IsBigTop
        //{
        //    get
        //    {
        //        float hwFixLine = (1280.0f + 82.0f) / 720.0f;
        //        float hwScreen = (float)UIFrame.height / (float)UIFrame.width;
        //        return hwScreen > hwFixLine;
        //    }
        //}
        public static float HomeScreenLerpT
        {
            get
            {
                //float hwFixLine = (1280.0f + 82.0f) / 720.0f;
                //float hw219 = 21.0f / 9.0f;
                //float hwScreen = (float)UIFrame.height / (float)UIFrame.width;
                //float screenT = (hwScreen - hwFixLine) / (hw219 - hwFixLine);
                //return screenT;
                if(Application.isPlaying)
                {
                    return UIFrame.GetFixScreenLerpT();
                }
                else
                {
                    return Utility.GetScreenLerpT();
                }
            }
        }


    }
}