using Google.Protobuf;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Babu
{
    public class ServerNotificationCenter : BabuSingleton<ServerNotificationCenter>
    {
        struct NotificationMethodInfo
        {
            public object Instance;
            public MethodInfo MethodInfo;
            public ServerNotification NotificationInfo;
        }

        Dictionary<string, NotificationMethodInfo> _notificationMethods = new Dictionary<string, NotificationMethodInfo>();

        public void Register(object handler)
        {
            Type type = handler.GetType();
            foreach (var methodInfo in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var attribute = methodInfo.GetCustomAttributes(typeof(ServerNotification), false).FirstOrDefault();
                if (attribute != null)
                {
                    ServerNotification serverNotification = attribute as ServerNotification;
                    _notificationMethods[serverNotification.NotifyProc] = new NotificationMethodInfo
                    {
                        Instance = handler,
                        MethodInfo = methodInfo,
                        NotificationInfo = serverNotification
                    };
                }
            }
        }

        public void OnServerNotify(string proc, ReadOnlySequence<byte> notifyData)
        {
            NotificationMethodInfo notificationMethodInfo;
            if (!_notificationMethods.TryGetValue(proc, out notificationMethodInfo))
            {
                Debug.LogError("Invalid Proc: " + proc);
                return;
            }
            Type messageType = notificationMethodInfo.MethodInfo.GetParameters()[0].ParameterType;
            var parserProperty = messageType.GetProperty("Parser");
            var parserType = parserProperty.PropertyType;
            var parserMethod = parserType.GetMethod("ParseFrom", new Type[] { typeof(ReadOnlySequence<byte>) });
            IMessage notifyMessage = parserMethod.Invoke(parserProperty.GetValue(null), new object[1] { notifyData }) as IMessage;
            notificationMethodInfo.MethodInfo.Invoke(notificationMethodInfo.Instance, new object[] { notifyMessage });
            //Debug.LogWarning("-------proc:" + proc + "----" + DateTime.Now.Millisecond);//推送在两个消息之间，消息间过多推送可能会导致转圈
        }
    }
}
