using System.Collections;
using System.Collections.Generic;
using Babu;
using Babu.SDK;
using BigBang.UI;
using UnityEngine;

namespace BigBang
{
    public class UIUpdateManager : BabuSingleton<UIUpdateManager>
    {

        void Update()
        {
            CheckEsc();
        }

        private void CheckEsc()
        {

            if (Input.GetKeyDown(KeyCode.Escape)) // 返回键
            {
                if (Application.platform != RuntimePlatform.Android) return;

                //if (SDKManagerBeforeHotFix.Instance.quickManager.isChannelHasExitDialog())
                //{
                //    SDKManager.Instance.CloseGame();
                //}
                //else
                //{
                UIController.Instance.OpenWindow<ConfirmationBoxUI>(new ConfirmationBoxUIProperties("确定退出游戏吗？", () =>
                {
                    SDKManager.Instance.CloseGame();
                    LoginManager.Instance.BackToLogin();
                }, null));
                //}
            }

        }
    }
}