using System;
using System.Collections.Generic;
using System.Text;
using Babu;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace BigBang
{
    /// <summary>
    /// 向 CBA 日志服务器记录Log
    /// 各种打点需求，见飞书文档：
    /// https://s5qjlc6dj6.feishu.cn/wiki/No0RwgP3wiIRDdk8UgXcbLhlnfb
    /// </summary>
    public class CbaLogManager : BabuSingleton<CbaLogManager>
    {

        public bool isCbaLogEnable
        {
            get
            {
//#if UNITY_EDITOR || UNITY_WEBGL
                return false;
//#endif
//                return true;
            }
        }

        private Sequence secondUpdateSequence = null;
        public override void Awake()
        {
            base.Awake();
            if (!isCbaLogEnable) return;
            UnityTimer.Timer.Register(this.gameObject, 0.5f, () =>
            {
                secondUpdateSequence = DOTween.Sequence();
                secondUpdateSequence.AppendInterval(1.0f);
                secondUpdateSequence.AppendCallback(CallUpdateAction);
                secondUpdateSequence.SetLoops(-1);
            });
        }

        private readonly string dataPointStr = ",";
        private readonly string dataStartStr = "data=";
        private readonly string signStartStr = "&sign=";
        private Queue<string> logStrList = new();
        public void AddLog(int type, params object[] others)
        {
            if (!isCbaLogEnable) return;
            try
            {
                if (logStrList.Count >= 100)
                {
                    UnityEngine.Debug.Log("too much log");
                    return;
                }
                StringBuilder stringBuilderData = new();
                stringBuilderData.Append(type);
                stringBuilderData.Append(dataPointStr);
                stringBuilderData.Append(Player.GbId);
                stringBuilderData.Append(dataPointStr);
                stringBuilderData.Append(DataConvUtil.ServerTime);
                if (others != null && others.Length > 0)
                {
                    for (int i = 0; i < others.Length; i++)
                    {
                        stringBuilderData.Append(dataPointStr);
                        stringBuilderData.Append(others[i]);
                    }
                }
                string data = stringBuilderData.ToString();
                string sign = CodecUtils.Hex(CodecUtils.Md5(Encoding.UTF8.GetBytes(data + ServerConst.LOG_KEY)));
                StringBuilder stringBuilderLog = new();
                stringBuilderLog.Append(dataStartStr);
                stringBuilderLog.Append(data);
                stringBuilderLog.Append(signStartStr);
                stringBuilderLog.Append(sign);
                // Debug.LogWarning(stringBuilderLog.ToString());
                logStrList.Enqueue(stringBuilderLog.ToString());
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex);
            }
        }

        private void CallUpdateAction()
        {
            try
            {
                if (logStrList.Count == 0) return;
                PostToLogServer(logStrList.Dequeue());
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex);
            }
        }

        private readonly string contentType = "application/x-www-form-urlencoded";
        private void PostToLogServer(string content)
        {
            //UnityEngine.Debug.Log("post log , content = " + content);
            //UnityHttpServiceFix.Instance.AsyncPost(ServerConst.CbaLogServerUrl, contentType, content, (bool result, string response) => { }, 2);
        }

        public void LogUpdateGoods(List<Protocol.Goods> dataGoods)
        {
            try
            {
                foreach (Protocol.Goods goods in dataGoods)
                {
                    int id = goods.Id;
                    int oldNum = Player.PackageManager.GetGoodsNumber(id);
                    int newNum = goods.Count;
                    CbaLogManager.Instance.AddLog(1007, id, oldNum, newNum, nowNetMethodName, nowNetTargetName);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        private string nowNetMethodName = "";
        private string nowNetTargetName = "";
        /// <summary>
        /// 有服务器请求
        /// </summary>
        /// <param name="methodName">Action.Method.Name字符串，如<DoRecruit>b__0</param>
        /// <param name="targetName">Action.Target.ToString字符串，如BigBang.RecruitController+<>c__DisplayClass15_0</param>
        public void NetworkCallIn(string methodName, string targetName)
        {
            try
            {
                // Debug.LogWarning("NetworkCallIn 1 , methodName = {0} , targetName = {1}".SafeFormat(methodName, targetName));
                methodName = PraseMethodName(methodName);
                if (IgnoreNetMethodName.Contains(methodName)) return;
                targetName = PraseTargetName(targetName);
                nowNetMethodName = methodName;
                nowNetTargetName = targetName;
                // Debug.LogWarning("NetworkCallIn 2 , methodName = {0} , targetName = {1}".SafeFormat(methodName, targetName));
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        /// <summary>
        /// 有服务器回复请求
        /// </summary>
        public void NetworkCallOut(string methodName, string targetName)
        {
            try
            {
                // Debug.Log("NetworkCallOut , methodName = {0} , targetName = {1}".SafeFormat(methodName, targetName));
                methodName = PraseMethodName(methodName);
                if (IgnoreNetMethodName.Contains(methodName)) return;
                nowNetMethodName = "";
                nowNetTargetName = "";
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        private readonly List<string> IgnoreNetMethodName = new() { "OnHeartbeat" };
        /// <summary>
        /// 处理Action.Method.Name字符串
        /// </summary>
        /// <param name="methodName">如<DoRecruit>b__0</param>
        /// <returns>如DoRecruit</returns>
        private string PraseMethodName(string methodName)
        {
            int leftIndex = methodName.IndexOf('<');
            if (leftIndex == -1) return methodName;
            int rightIndex = methodName.IndexOf('>', leftIndex);
            if (leftIndex == -1) return methodName;
            return methodName.Substring(leftIndex + 1, rightIndex - leftIndex - 1);
        }
        /// <summary>
        /// 处理Action.Target.ToString字符串
        /// </summary>
        /// <param name="methodName">如BigBang.RecruitController+<>c__DisplayClass15_0</param>
        /// <returns>如RecruitController</returns>
        private string PraseTargetName(string targetName)
        {
            if (targetName.Contains("(Clone)"))
            {
                return targetName.Substring(0, targetName.IndexOf("(Clone)"));
            }
            int leftIndex = targetName.IndexOf('.');
            int rightIndex = targetName.IndexOf('+');
            if (rightIndex == -1) rightIndex = targetName.Length;
            return targetName.Substring(leftIndex + 1, rightIndex - leftIndex - 1);
        }

    }
}