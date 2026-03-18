
using Protocol;

namespace BigBang
{
    public sealed class ChannelManager
    {
        private static readonly ChannelManager instance = new ChannelManager();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ChannelManager()
        {
        }

        private ChannelManager()
        {
        }

        public static ChannelManager Instance
        {
            get
            {
                return instance;
            }
        }

        private bool _enableAds = false;

        private bool _enableQQ = false;
        private bool _enableMail = false;

        public void SetInfo(ChannelInfo info)
        {
            this._enableAds = info.EnableAds > 0;
            this._enableQQ = info.EnableQq > 0;
            this._enableMail = info.EnableMail > 0;//TODO:需要服务器信息
            UnityEngine.Debug.Log("ChannelManager.SetInfo: info.EnableAds=" + info.EnableAds + ", info.EnableQq=" + info.EnableQq);
        }

        public bool EnableAds
        {
            get { return this._enableAds; }
        }

        public bool EnableQQ
        {
            get { return this._enableQQ; }
        }

        public bool EnableMail
        {
            get { return this._enableMail; }
        }
    }
}