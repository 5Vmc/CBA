using Google.Protobuf;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Babu
{
    class SocketResponseCenter : BabuSingleton<SocketResponseCenter>
    {
        private class SocketResponseCallbackInfo
        {
            public long StartTime = TimeUtils.NowEx();
            public MethodInfo Method;
            public object Instance;
            public Type Type;
        }
        Dictionary<uint, SocketResponseCallbackInfo> _callbackInfos = new Dictionary<uint, SocketResponseCallbackInfo>();

        public void Register<T>(uint sessionId, Action<T> callback)
        {
            SocketResponseCallbackInfo callbackInfo = new SocketResponseCallbackInfo();
            callbackInfo.Method = typeof(Action<T>).GetMethod("Invoke");
            callbackInfo.Instance = callback;
            callbackInfo.Type = typeof(T);
            _callbackInfos[sessionId] = callbackInfo;
        }

        public void OnResponse(uint sessionId, ReadOnlySequence<byte> response)
        {
            SocketResponseCallbackInfo callbackInfo;
            if (_callbackInfos.TryGetValue(sessionId, out callbackInfo) == false)
            {
                Debug.LogError($"Session Id: {sessionId} Timeout");
                return;
            }

            string callBackInstanceString = callbackInfo.Instance.ToString();
            Debug.Log("收到了服务器返回，callbackInfo.Method.Name：" + callBackInstanceString);

            var parserProperty = callbackInfo.Type.GetProperty("Parser");
            var parserType = parserProperty.PropertyType;
            var parserMethod = parserType.GetMethod("ParseFrom", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ReadOnlySequence<byte>) }, null);
            IMessage responseMsg = parserMethod.Invoke(parserProperty.GetValue(null), new object[1] { response }) as IMessage;

            callbackInfo.Method.Invoke(callbackInfo.Instance, new object[1] { responseMsg });
        }
    }
}
