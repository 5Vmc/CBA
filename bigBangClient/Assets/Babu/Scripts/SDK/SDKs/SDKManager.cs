using System;
using UnityEngine;

namespace Babu.SDK
{
    public class SDKManager : SequentialTaskExecutor
    {

        public static SDKManager Instance;

        void Awake()
        {
            Instance = this;
        }

        public void DoLogin()
        {
            MiGuPlayManager.Instance.Login();
        }

        public void CloseGame()
        {
            Application.Quit();
        }

        public void LogOut()
        {
            MiGuPlayManager.Instance.LogOut();
        }
    }
}
