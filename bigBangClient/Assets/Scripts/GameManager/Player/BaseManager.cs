using Babu;

namespace BigBang
{
    public class BaseManager
    {
        public BaseManager()
        {
            ServerNotificationCenter.Instance.Register(this);
        }
    }
}
