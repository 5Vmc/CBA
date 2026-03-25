using System;
using System.Collections.Generic;
using UnityEngine;

namespace Babu
{
    public class EventManager : BabuSingleton<EventManager>
    {
        /// <summary>
        /// 不可进行热更的消息 ID
        /// 通常用于非热更部分发消息通知热更新部分
        /// </summary>
        public static class CanNotHotFixId
        {
            //Quick初始化结束（可能成功或者失败）
            //public static string QUICK_INIT_END = "QUICK_INIT_END";
            //Quick登录成功
            public static string QUICK_LOGIN_SUCCESS = "QUICK_LOGIN_SUCCESS";
            //Quick登录失败
            public static string QUICK_LOGIN_FAIL = "QUICK_LOGIN_FAIL";
            //Quick 切换账号
            public static string QUICK_SWITCH_ACCOUNT = "QUICK_SWITCH_ACCOUNT";
            //Quick 注销账号
            public static string QUICK_LOGIN_OUT = "QUICK_LOGIN_OUT";

            public static string NETWORK_SENDING = "NETWORK_SENDING";
            public static string NETWORK_CALLBACK = "NETWORK_CALLBACK";

            //开始发起充值
            public static string CHARGE_START = "CHARGE_START";
            //充值失败
            public static string CHARGE_FAIL = "CHARGE_FAIL";
            //充值成功
            public static string CHARGE_SUCCESS = "CHARGE_SUCCESS";

            public static string AVOID_GAME = "AVOID_GAME";
        }

        public delegate void EventCallback(object[] args);

        private Dictionary<string, EventCallback> _eventMap = new Dictionary<string, EventCallback>();
        private List<KeyValuePair<string, object[]>> _triggedEventList = new List<KeyValuePair<string, object[]>>();
        private List<KeyValuePair<string, object[]>> _triggedEventListTemp = new List<KeyValuePair<string, object[]>>();

        public override void OnDestroy()
        {
            base.OnDestroy();
            _triggedEventList.Clear();
            _eventMap.Clear();
        }

        public void Register(string eve, EventCallback onEvent)
        {
            if (_eventMap.ContainsKey(eve))
            {
                _eventMap[eve] += onEvent;
            }
            else
            {
                _eventMap[eve] = onEvent;
            }
        }

        public void Unregister(string eve, EventCallback onEvent)
        {
            if (_eventMap.ContainsKey(eve))
            {
                _eventMap[eve] -= onEvent;
                if (_eventMap[eve] == null)
                {
                    _eventMap.Remove(eve);
                }
            }
        }

        void Update()
        {
            lock (_triggedEventList)
            {
                Utils.Swap(ref _triggedEventList, ref _triggedEventListTemp);
                _triggedEventList.Clear();
            }

            foreach (var kv in _triggedEventListTemp)
            {
                string eve = kv.Key;
                object[] args = kv.Value;

                try
                {
                    if (_eventMap.ContainsKey(eve))
                    {
                        _eventMap[eve](args);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Event: {eve} Catch Exception: {e.Message}, StackTrace: {e.StackTrace}");
                }
            }
        }

        public void Dispatch(string eve, params object[] args)
        {
            lock (_triggedEventList)
            {
                _triggedEventList.Add(new KeyValuePair<string, object[]>(eve, args));
            }
        }

        public void Dispatch(object[] args)
        {
            // TODO：临时兼容方案，后续删除
            object[] newArgs = new object[args.Length - 1];
            Array.Copy(args, 1, newArgs, 0, newArgs.Length);
            Dispatch(args[0] as string, newArgs);
        }
    }
}
