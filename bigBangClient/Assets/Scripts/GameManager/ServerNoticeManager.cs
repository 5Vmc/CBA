using Babu;
using BigBang.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static BigBang.ServerNoticeData;
using Task = System.Threading.Tasks.Task;

namespace BigBang
{

    /// <summary>
    /// 服务器通知数据
    /// 由json文件解析而来
    /// </summary>
    public class ServerNoticeData
    {

        public class NoticeOneData
        {
            public string titleText = "";
            public string topText = "";
            public string midText = "";
            public string bottomText = "";
        }

        public long noticeJsonVersion = -1;

        public List<NoticeOneData> noticeList = new();
    }

    /// <summary>
    /// 服务器通知管理类
    /// </summary>
    public class ServerNoticeManager : BabuSingleton<ServerNoticeManager>
    {
        private int getNoticeTimes = 0;

        private ServerNoticeData serverNoticeData = null;
        private bool isGetServerNoticeFinish = false;

        /// <summary>
        /// 获取并显示服务器公告
        /// </summary>
        async public Task GetAndShowServerNotice()
        {
            await LoadServerNoticeAsync();
        }
        private async Task LoadServerNoticeAsync()
        {
            try
            {
                getNoticeTimes++;
                string url = ServerConst.SERVER_NOTICE_URL;
                UnityWebRequest unityWebRequest = UnityWebRequest.Get(url);
                unityWebRequest.timeout = 3;
                await unityWebRequest.SendWebRequest();
                if (unityWebRequest.result != UnityWebRequest.Result.Success) throw new Exception("ServerNoticeManager , LoadServerNoticeAsync , unityWebRequest.result != UnityWebRequest.Result.Success");
                string resultStr = unityWebRequest.downloadHandler.text;
                if (string.IsNullOrEmpty(resultStr)) throw new Exception("ServerNoticeManager , LoadServerNoticeAsync , string.IsNullOrEmpty(resultStr)");
                Debug.Log(resultStr);
                serverNoticeData = JsonConvert.DeserializeObject<ServerNoticeData>(resultStr);
                if (serverNoticeData == null) throw new Exception("ServerNoticeManager , LoadServerNoticeAsync , serverNoticeData == null");
                isGetServerNoticeFinish = true;
                AfterLoadServerNoticeSuccess();
            }
            catch (Exception ex)
            {
                if (getNoticeTimes <= 5)
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    await LoadServerNoticeAsync();
                }
                else
                {
                    Debug.Log("获取服务器公告失败:" + ex.Message);
                }
            }

        }
        private void AfterLoadServerNoticeSuccess()
        {
            if(serverNoticeData == null)
            {
                return;
            }
            if (serverNoticeData.noticeList.Count == 0)
            {
                return;
            }
            NoticeOneData noticeOneData = serverNoticeData.noticeList[0];
            UIController.Instance.OpenWindow<NoticeDetailWindow>(new NoticeDetailWindowProperties(noticeOneData));
        }


    }
}