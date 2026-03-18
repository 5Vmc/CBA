using System;

namespace Babu
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ServerNotification : Attribute
    {
        public string NotifyProc;

        public ServerNotification(string notifyProc)
        {
            NotifyProc = notifyProc;
        }
    }
}
