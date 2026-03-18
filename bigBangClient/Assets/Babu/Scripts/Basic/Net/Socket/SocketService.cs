using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Assertions;
using Google.Protobuf;
using System.Buffers;
using System.Threading.Tasks;

namespace Babu
{
    public class SocketService : BabuSingleton<SocketService>
    {
        public class Event
        {
            public static string Disconnected = "Disconnected";
            public static string SendingLimit = "SendingLimit";
        }

        SocketOperate socketOperate = null;

        public Task<bool> Open(string ip, int port, int timeout)
        {
            socketOperate = new();
            return socketOperate.Open(ip, port, timeout);
        }

        public void Close()
        {
            try
            {
                if (socketOperate != null)
                {
                    Debug.Log("Close Close 3");
                    socketOperate.Close();
                    socketOperate = null;
                }
            }
            catch(Exception e)
            {
                Debug.Log(e);
            }
        }

        private void OnDestroy()
        {
            Close();
        }

        public void Call<T>(string methodName, IMessage message, Action<T> callback) where T : IMessage
        {
            socketOperate?.Call<T>(methodName, message, callback);
        }

        public void Send(string methodName, IMessage message)
        {
            socketOperate.Send(methodName, message);
        }

        public Dictionary<string, bool> _ignoreMaskReq = new();
        public void AddIgnoreMaskReq(List<string> methodNameList)
        {
            foreach (string methodName in methodNameList)
            {
                this._ignoreMaskReq.TryAdd(methodName, true);
            }
        }
    }
}
