

using System.Security.Policy;
/**
* 定义服务器地址
*/

namespace BigBang
{
    public class ServerConst
    {
        /// <summary>
        /// 是否开放购买，需要部分界面prefab调整配合
        /// </summary>
        public static readonly bool OPEN_BUY = true;

        public const string URL_MIGU_NFT_QUERY = "http://223.111.8.56:16666/query/";//NFT查询地址
        public const string URL_SURVEY = "https://www.wjx.cn/vm/h9RvdFX.aspx"; //问卷调查

        public static readonly string SERVER_SELECTOR_URL = $"{SERVER_HOST}/cba/server_selector.php";//选择上次登录的服务器
        public static readonly string SERVER_LIST_URL = $"{SERVER_HOST}/cba/server_list.php";//获取服务器列表
        public static readonly string PVP_BATTLE_URL = $"{SERVER_BATTLE_HOST}/cba/get_pvp_battle.php";//获取战斗回放
        public static readonly string SERVER_NOTICE_URL = $"{SERVER_HOST}/hotfix/CbaServerNotice.json";//获取服务器通知

#if MiGuNft
        public const string ServerHostOnline = "http://gateway.migunft.hzboyoutech.com";//登录服务器数据正式地址
        public const string ServerBattleHostOnline = "http://battle.migunft.hzboyoutech.com";//战斗回放数据正式地址
#else
        public const string ServerHostOnline = "https://cba.gateway.ximiplay.com";//登录服务器数据正式地址
        public const string ServerBattleHostOnline = "https://cba.playback.ximiplay.com";//战斗回放数据正式地址
#endif

        public const string ServerHostTest = "http://47.99.113.240";//登录服务器数据测试地址
        public const string ServerBattleHostTest = "http://47.99.113.240";//战斗回放数据测试地址

        public static string SERVER_HOST//登录服务器数据获取
        {
            get
            {
                string UsedServerHostUrl = UnityEngine.PlayerPrefs.GetString("ServerHostUrl", "");
                if (UsedServerHostUrl == "")
                {
#if !RELEASE
                    UsedServerHostUrl = ServerHostTest;
#else
                    UsedServerHostUrl = ServerHostOnline;
#endif
                    UnityEngine.PlayerPrefs.SetString("ServerHostUrl", UsedServerHostUrl);
                }

                UnityEngine.Debug.Log("UsedServerHostUrl = " + UsedServerHostUrl);
                return UsedServerHostUrl;
            }
        }

        public static string SERVER_BATTLE_HOST//战斗回放数据获取
        {
            get
            {
                string UsedServerBattleHostUrl = UnityEngine.PlayerPrefs.GetString("ServerBattleHostUrl", "");
                if (UsedServerBattleHostUrl == "")
                {
#if !RELEASE
                    UsedServerBattleHostUrl = ServerBattleHostTest;
#else
                    UsedServerBattleHostUrl = ServerBattleHostOnline;
#endif
                    UnityEngine.PlayerPrefs.SetString("ServerBattleHostUrl", UsedServerBattleHostUrl);
                }

                UnityEngine.Debug.Log("UsedServerBattleHostUrl = " + UsedServerBattleHostUrl);
                return UsedServerBattleHostUrl;
            }
        }

        public const string LOG_KEY = "CBA202403";
        public const string CbaLogServerUrl = "https://cbalog.ximiplay.com/logging.php";

    }
}