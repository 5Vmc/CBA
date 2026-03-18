using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;
using System.Buffers;
using System.Threading.Tasks;
using System.Reflection;
#if UNITY_WEBGL
using UnityEngine.Assertions;
using UnityWebSocket;
using System.IO;
#else
using System.Net.Sockets;
#endif

namespace Babu
{
#if !UNITY_WEBGL
    public class SocketOperate
    {
        public class Event
        {
            public static string Disconnected = "Disconnected";
            public static string SendingLimit = "SendingLimit";
        }

        [SerializeField] int tickTime = 15;     // TODO
        public Socket _socket;

        uint _sessionGenerator = 0;

        Dictionary<string, long> _sendingLimit = new Dictionary<string, long>();        // 发送限制
        bool _isSending = false;
        Queue<byte[]> _sendingQueue = new Queue<byte[]>();

        public static string CheckPlayerAchievement = "achievement_module.cs_requestCheckPlayerAchievement";


        public Task<bool> Open(string ip, int port, int timeout)
        {
            if (_socket != null)
            {
                Debug.LogError("SocketOperate , Open , _socket != null , ip = " + ip + " , port = " + port + " , _socket.GetHashCode() = " + _socket.GetHashCode());
            }

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Debug.Log("SocketOperate , Open , ip = " + ip + " , port = " + port + " , _socket.GetHashCode() = " + _socket.GetHashCode());

            //var result = _socket.BeginConnect(ep, null, null);
            //bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
            //if (success)
            //{
            //    Debug.Log($"Connect To Server {ip}:{port} Succ");
            //    _socket.EndConnect(result);
            //    BeginReceive(true, 4);
            //    return true;
            //}
            //else
            //{
            //    Debug.Log($"Connect To Server {ip}:{port} Failed");
            //    Close();
            //    return false;
            //}
            //Debug.Log("_socket.GetHashCode() = " + _socket.GetHashCode() + " , socketConnect");
            _socket.Connect(ip, port);
            //if (_socket.Connected)
            //{
            BeginReceive(true, 4);
            //}
            return System.Threading.Tasks.Task.FromResult(_socket.Connected);
        }

        public void Close()
        {
            if (_socket != null)
            {
                //Debug.Log("_socket.GetHashCode() = " + _socket.GetHashCode() + " , socketClose");
                Debug.Log("SocketOperate , Close , _socket.GetHashCode() = " + _socket.GetHashCode());
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket = null;
                _isSending = false;

            }
        }

        void BeginReceive(bool receiveHead, int length)
        {
            byte[] buffer = new byte[length];
            Receive(receiveHead, buffer, 0, length);
        }

        void Receive(bool receiveHead, byte[] buffer, int offest, int length)
        {
            _socket.BeginReceive(buffer, offest, length, SocketFlags.None, (result) =>
            {

                SocketError socketError;
                int readLength = _socket.EndReceive(result, out socketError);

                //Debug.Log("_socket.GetHashCode() = " + _socket.GetHashCode() + " , socketError = " + socketError);

                if (socketError != SocketError.Success || readLength == 0)
                {
                    EventManager.Instance.Dispatch(Event.Disconnected);
                    Debug.Log("Receive Close 1");
                    MainThreadTaskService.Instance.AddTask(Close);
                    return;
                }

                if (readLength < length)
                {
                    Receive(receiveHead, buffer, offest + readLength, length - readLength);
                }
                else
                {
                    if (receiveHead)
                    {
                        int bodyLength = BitConverter.ToInt32(buffer, 0);
                        bodyLength = IPAddress.NetworkToHostOrder(bodyLength);
                        BeginReceive(false, bodyLength);
                    }
                    else
                    {
                        //Debug.Log("SocketOperate , OnReceiveComplete , _socket.GetHashCode() = " + _socket.GetHashCode());
                        MainThreadTaskService.Instance.AddTask(() => OnReceiveComplete(buffer));
                        BeginReceive(true, 4);
                    }
                }
            }, null);
        }

        void OnReceiveComplete(byte[] buffer)
        {
            try
            {
                MessagePack.MessagePackReader reader = new MessagePack.MessagePackReader(buffer);
                uint sessionId = reader.ReadUInt32();
                string methodName = reader.ReadString();
                ReadOnlySequence<byte>? msgData = reader.ReadBytes();

                if (sessionId == 0)
                {
                    Debug.Log("收到了服务器推送，方法名称：" + methodName);
                    ServerNotificationCenter.Instance.OnServerNotify(methodName, msgData.Value);
                }
                else
                {
                    //Debug.Log("收到了服务器返回，sessionId：" + sessionId);
                    SocketResponseCenter.Instance.OnResponse(sessionId, msgData.Value);

                    EventManager.Instance.Dispatch(EventManager.CanNotHotFixId.NETWORK_CALLBACK, methodName);
                }
            }
            catch (TargetInvocationException e)
            {
                Debug.LogException(e.InnerException);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Call<T>(string methodName, IMessage message, Action<T> callback) where T : IMessage
        {
            Debug.Log("发送网络请求（带回调），方法名称：" + methodName);
            uint sessionId = MakeSessionId();
            if (Send(sessionId, methodName, message) == false)
            {
                return;
            }
            if (!SocketService.Instance._ignoreMaskReq.ContainsKey(methodName))
            {
                EventManager.Instance.Dispatch(EventManager.CanNotHotFixId.NETWORK_SENDING, methodName);
            }
            SocketResponseCenter.Instance.Register<T>(sessionId, callback);
        }

        uint MakeSessionId()
        {
            return ++_sessionGenerator;
        }

        public void Send(string methodName, IMessage message)
        {
            Debug.Log("发送网络请求 ，方法名称：" + methodName);
            Send(0, methodName, message);
        }

        bool Send(uint sessionId, string methodName, IMessage message)
        {
            if (_socket == null) return false;
            if (_socket.Connected == false) return false;
            long now = TimeUtils.NowEx();
            long lastTime;
            if (_sendingLimit.TryGetValue(methodName, out lastTime) && lastTime + 100 > now)
            {
#if UNITY_EDITOR
                Debug.LogWarningFormat("{0} sending limit", methodName);
#else
                Debug.LogFormat("{0} sending limit", methodName);
#endif
                EventManager.Instance.Dispatch(Event.SendingLimit);
                return false;
            }

            _sendingLimit[methodName] = now;

            object[] sendData = new object[3];
            sendData[0] = sessionId;
            sendData[1] = methodName;
            sendData[2] = message.ToByteArray();

            byte[] data = MessagePack.MessagePackSerializer.Serialize(sendData);
            Send(data);
            return true;
        }

        void Send(byte[] data)
        {
            if (data.Length > short.MaxValue)
            {
                // TODO：提示
                return;
            }

            short length = IPAddress.NetworkToHostOrder((short)data.Length);
            byte[] lengthData = BitConverter.GetBytes(length);
            lock (this)
            {
                _sendingQueue.Enqueue(lengthData);
                _sendingQueue.Enqueue(data);

                if (_isSending)
                {
                    return;
                }

                _isSending = true;
            }

            SendNext(PickNextSendData());
        }

        byte[] PickNextSendData()
        {
            lock (this)
            {
                if (_sendingQueue.Count > 0)
                {
                    return _sendingQueue.Dequeue();
                }

                _isSending = false;
                return null;
            }
        }

        void SendNext(byte[] data)
        {
            if (data == null)
            {
                return;
            }

            Send(data, 0);
        }

        void Send(byte[] data, int offest)
        {
            int lastSize = data.Length - offest;
            if (_socket == null) return;
            _socket.BeginSend(data, offest, lastSize, SocketFlags.None, (result) =>
            {
                SocketError socketError;
                int sendLength = _socket.EndSend(result, out socketError);
                if (socketError != SocketError.Success || sendLength == 0)
                {
                    EventManager.Instance.Dispatch(Event.Disconnected);
                    Debug.Log("Send Close 2");
                    MainThreadTaskService.Instance.AddTask(Close);
                    return;
                }

                if (sendLength == lastSize)
                {
                    SendNext(PickNextSendData());
                }
                else
                {
                    Send(data, offest + sendLength);
                }
            }, null);
        }
    }
#else
    public class SocketOperate
    {
        public class Event
        {
            public static string Disconnected = "Disconnected";
            public static string SendingLimit = "SendingLimit";
        }

        [SerializeField] int tickTime = 15;     // TODO
        WebSocket _socket;
        TaskCompletionSource<bool> _connectTcs;
        MemoryStream _receiveBuffer = new MemoryStream();
        bool _receiveHead = false;
        int _length;

        uint _sessionGenerator = 0;

        Dictionary<string, long> _sendingLimit = new Dictionary<string, long>();        // 发送限制
        bool _isSending = false;
        Queue<byte[]> _sendingQueue = new Queue<byte[]>();

        public static string CheckPlayerAchievement = "achievement_module.cs_requestCheckPlayerAchievement";
        Dictionary<string, bool> _ignoreMaskReq = new Dictionary<string, bool>(){
            {/*ProcID.*/CheckPlayerAchievement, true}
        };

        public Task<bool> Open(string ip, int port, int timeout)
        {
            Assert.IsTrue(_socket == null);
#if RELEASE
            ip = $"wss://{ip}";
#else
            ip = $"ws://{ip}";
#endif
            //ip = "ws://10.0.0.102";
            //port = 9999;

            _socket = new WebSocket($"{ip}:{port}");
            //var result = _socket.BeginConnect(ep, null, null);
            //bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
            //if (success)
            //{
            //    Debug.Log($"Connect To Server {ip}:{port} Succ");
            //    _socket.EndConnect(result);
            //    BeginReceive(true, 4);
            //    return true;
            //}
            //else
            //{
            //    Debug.Log($"Connect To Server {ip}:{port} Failed");
            //    Close();
            //    return false;
            //}

            _socket.OnOpen += OnOpen;
            _socket.OnClose += OnClose;
            _socket.OnMessage += OnMessage;
            _socket.OnError += OnError;

            _connectTcs = new TaskCompletionSource<bool>();

            _receiveHead = true;
            _length = 4;

            _socket.ConnectAsync();

            return _connectTcs.Task;
        }

        void OnOpen(object sender, OpenEventArgs args)
        {
            Debug.Log("Connect succ");
            _connectTcs.SetResult(true);
            _connectTcs = null;
        }

        void OnClose(object sender, CloseEventArgs args)
        {
            EventManager.Instance.Dispatch(Event.Disconnected);
            Close();
        }

        void OnMessage(object sender, MessageEventArgs args)
        {
            _receiveBuffer.Write(args.RawData);
            _receiveBuffer.Position = 0;
            Receive(_receiveHead, _length);
        }

        void OnError(object sender, UnityWebSocket.ErrorEventArgs args)
        {
            Debug.LogError("Error: " + args.Message + " " + args.GetType());
            if (_connectTcs != null)
            {
                _connectTcs.SetResult(false);
                _connectTcs = null;
            }
        }

        public void Close()
        {
            if (_socket != null)
            {
                _socket.CloseAsync();
                _socket = null;
                _isSending = false;
            }
        }

        private void OnDestroy()
        {
            Close();
        }

        void Receive(bool receiveHead, int length)
        {
            _receiveHead = receiveHead;
            _length = length;
            if (_receiveBuffer.Length - _receiveBuffer.Position < length)
            {
                if (_receiveBuffer.Position != 0)
                {
                    if (_receiveBuffer.Length - _receiveBuffer.Position > 0)
                    {
                        byte[] arr = new byte[_receiveBuffer.Length - _receiveBuffer.Position];
                        _receiveBuffer.Read(arr, 0, (int)(_receiveBuffer.Length - _receiveBuffer.Position));
                        _receiveBuffer.SetLength(0);
                        _receiveBuffer.Write(arr, 0, arr.Length);
                    }
                    else
                    {
                        _receiveBuffer.SetLength(0);
                        _receiveBuffer.Position = 0;
                    }
                }
                else
                {
                    _receiveBuffer.Position = _receiveBuffer.Length;
                }
            }
            else
            {
                var arr = new byte[length];
                _receiveBuffer.Read(arr, 0, arr.Length);

                if (receiveHead)
                {
                    int bodyLength = BitConverter.ToInt32(arr, 0);
                    bodyLength = IPAddress.NetworkToHostOrder(bodyLength);
                    Receive(false, bodyLength);
                }
                else
                {
                    OnReceiveComplete(arr);
                    Receive(true, 4);
                }
            }
        }

        void OnReceiveComplete(byte[] buffer)
        {
            MessagePack.MessagePackReader reader = new MessagePack.MessagePackReader(buffer);
            uint sessionId = reader.ReadUInt32();
            string methodName = reader.ReadString();
            ReadOnlySequence<byte>? msgData = reader.ReadBytes();

            if (sessionId == 0)
            {
                Debug.Log("收到了服务器推送，方法名称：" + methodName);
                ServerNotificationCenter.Instance.OnServerNotify(methodName, msgData.Value);
            }
            else
            {
                //Debug.Log("收到了服务器返回，sessionId：" + sessionId);
                SocketResponseCenter.Instance.OnResponse(sessionId, msgData.Value);

                EventManager.Instance.Dispatch(EventManager.CanNotHotFixId.NETWORK_CALLBACK, methodName);
            }
        }

        public void Call<T>(string methodName, IMessage message, Action<T> callback) where T : IMessage
        {
            Debug.Log("发送网络请求（带回调），方法名称：" + methodName);
            uint sessionId = MakeSessionId();
            if (Send(sessionId, methodName, message) == false)
            {
                return;
            }
            if (!SocketService.Instance._ignoreMaskReq.ContainsKey(methodName))
            {
                EventManager.Instance.Dispatch(EventManager.CanNotHotFixId.NETWORK_SENDING, methodName);
            }
            SocketResponseCenter.Instance.Register<T>(sessionId, callback);
        }

        uint MakeSessionId()
        {
            return ++_sessionGenerator;
        }

        public void Send(string methodName, IMessage message)
        {
            Debug.Log("发送网络请求 ，方法名称：" + methodName);
            Send(0, methodName, message);
        }

        bool Send(uint sessionId, string methodName, IMessage message)
        {
            long now = TimeUtils.NowEx();
            long lastTime;
            if (_sendingLimit.TryGetValue(methodName, out lastTime) && lastTime + 100 > now)
            {
                Console.WriteLine("{0} sending limit", methodName);
                EventManager.Instance.Dispatch(Event.SendingLimit);
                return false;
            }

            _sendingLimit[methodName] = now;

            object[] sendData = new object[3];
            sendData[0] = sessionId;
            sendData[1] = methodName;
            sendData[2] = message.ToByteArray();

            byte[] data = MessagePack.MessagePackSerializer.Serialize(sendData);
            Send(data);
            return true;
        }

        void Send(byte[] data)
        {
            if (data.Length > short.MaxValue)
            {
                // TODO：提示
                return;
            }

            short length = IPAddress.NetworkToHostOrder((short)data.Length);
            byte[] lengthData = BitConverter.GetBytes(length);
            lock (this)
            {
                _sendingQueue.Enqueue(lengthData);
                _sendingQueue.Enqueue(data);

                if (_isSending)
                {
                    return;
                }

                _isSending = true;
            }

            SendNext(PickNextSendData());
        }

        byte[] PickNextSendData()
        {
            lock (this)
            {
                if (_sendingQueue.Count > 0)
                {
                    return _sendingQueue.Dequeue();
                }

                _isSending = false;
                return null;
            }
        }

        void SendNext(byte[] data)
        {
            if (data == null)
            {
                return;
            }

            _socket.SendAsync(data);
            SendNext(PickNextSendData());
        }

        public void AddIgnoreMaskReq(List<string> methodNameList)
        {
            foreach (string methodName in methodNameList)
            {
                this._ignoreMaskReq.TryAdd(methodName, true);
            }
        }
    }
#endif
}
