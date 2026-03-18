using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using Babu.SDK;
using BigBang.Battle;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using Utils.GameItem;
using static BigBang.ClassicManager;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang.UI
{
    public class TestServerUI : AWindowController
    {

        protected override void AddListeners()
        {
            base.AddListeners();

            closeBtn.OnClick += OnClickCloseBtn;

            setQuickTestBtn.OnClick += OnClickSetQuickTestBtn;
            setQuickOnlineBtn.OnClick += OnClickSetQuickOnlineBtn;
            setQuickPrelaunchBtn.OnClick += OnClickSetQuickPrelaunchBtn;
            setMoreBtn.OnClick += OnClickSetMoreBtn;

            setTestBtnHotfix.OnClick += OnClickSetTestBtnHotfix;
            setOnlineBtnHotfix.OnClick += OnClickSetOnlineBtnHotfix;
            setPrelaunchBtnHotfix.OnClick += OnClickSetPrelaunchBtnHotfix;
            setEditBtnHotfix.OnClick += OnClickSetEditBtnHotfix;

            setTestBtnServer.OnClick += OnClickSetTestBtnServer;
            setOnlineBtnServer.OnClick += OnClickSetOnlineBtnServer;
            setEditBtnServerHost.OnClick += OnClickSetEditBtnServerHost;
            setEditBtnServerBattleHost.OnClick += OnClickSetEditBtnServerBattleHost;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();

            closeBtn.OnClick -= OnClickCloseBtn;

            setQuickTestBtn.OnClick -= OnClickSetQuickTestBtn;
            setQuickOnlineBtn.OnClick -= OnClickSetQuickOnlineBtn;
            setQuickPrelaunchBtn.OnClick -= OnClickSetQuickPrelaunchBtn;
            setPrelaunchBtnHotfix.OnClick -= OnClickSetPrelaunchBtnHotfix;
            setMoreBtn.OnClick -= OnClickSetMoreBtn;

            setTestBtnHotfix.OnClick -= OnClickSetTestBtnHotfix;
            setOnlineBtnHotfix.OnClick -= OnClickSetOnlineBtnHotfix;
            setEditBtnHotfix.OnClick -= OnClickSetEditBtnHotfix;

            setTestBtnServer.OnClick -= OnClickSetTestBtnServer;
            setOnlineBtnServer.OnClick -= OnClickSetOnlineBtnServer;
            setEditBtnServerHost.OnClick -= OnClickSetEditBtnServerHost;
            setEditBtnServerBattleHost.OnClick -= OnClickSetEditBtnServerBattleHost;
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            RefreshInfo();
            ReadUsedCbaBundleResVersionJsonUrl();
            ReadUsedServerHost();
            ShowMore(false);
        }

        [SerializeField] public BabuButton closeBtn = null;

        [SerializeField] private BabuButton setTestBtnHotfix = null;
        [SerializeField] private BabuButton setOnlineBtnHotfix = null;
        [SerializeField] private BabuButton setPrelaunchBtnHotfix = null;
        [SerializeField] private TMP_InputField urlInputFieldHotfix = null;
        [SerializeField] private BabuButton setEditBtnHotfix = null;

        [SerializeField] private BabuButton setTestBtnServer = null;
        [SerializeField] private BabuButton setOnlineBtnServer = null;
        [SerializeField] private TMP_InputField urlInputFieldServerHost = null;
        [SerializeField] private BabuButton setEditBtnServerHost = null;
        [SerializeField] private TMP_InputField urlInputFieldServerBattleHost = null;
        [SerializeField] private BabuButton setEditBtnServerBattleHost = null;

        [SerializeField] private TMP_Text mainVersionText = null;
        [SerializeField] private TMP_Text platfomText = null;
        [SerializeField] private TMP_Text clientVersionText = null;
        [SerializeField] private TMP_Text bundleVersionText = null;
        [SerializeField] private TMP_Text serverIdText = null;
        [SerializeField] private TMP_Text serverNameText = null;
        [SerializeField] private TMP_Text serverIpText = null;


        private void OnClickCloseBtn(BabuButton button)
        {
            this.gameObject.SetActive(false);
        }

        private void RefreshInfo()
        {
            mainVersionText.text = "主版本号：" + Babu.Environment.GetValue("major_version", "").ToString();
#if UNITY_ANDROID
            platfomText.text = "平台：安卓";
#endif
#if UNITY_IOS
        platfomText.text = "平台：IOS";
#endif
#if UNITY_WEBGL
        platfomText.text = "平台：WEBGL";
#endif
            clientVersionText.text = "客户端版本号：" + Babu.Environment.GetValue("client_creat_time", "").ToString();
            bundleVersionText.text = "资源版本号：" + Babu.Environment.bundleCreatTime;
            serverIdText.text = "服务器 ID：" + LoginManager.Instance.ServerData.Id.ToString();
            serverNameText.text = "服务器名：" + LoginManager.Instance.ServerData.OfficialName.ToString() + " " + LoginManager.Instance.ServerData.AliasName.ToString();
            serverIpText.text = "服务器IP：" + LoginManager.Instance.ServerData.Ip.ToString() + ":" + LoginManager.Instance.ServerData.Port.ToString();
        }

        #region 快速设置

        [SerializeField] private Image hotfixPanel = null;
        [SerializeField] private Image serverPanel = null;
        [SerializeField] private Image quickSetPanel = null;

        [SerializeField] private BabuButton setQuickTestBtn = null;
        [SerializeField] private BabuButton setQuickOnlineBtn = null;
        [SerializeField] private BabuButton setQuickPrelaunchBtn = null;
        [SerializeField] private BabuButton setMoreBtn = null;

        private void OnClickSetQuickTestBtn(BabuButton button)
        {
            OnClickSetTestBtnHotfix(null);
            OnClickSetTestBtnServer(null);
            Tips.PopTips("测试环境，重启后生效");
        }

        private void OnClickSetQuickOnlineBtn(BabuButton button)
        {
            OnClickSetOnlineBtnHotfix(null);
            OnClickSetOnlineBtnServer(null);
            Tips.PopTips("正式环境，重启后生效");
        }

        private void OnClickSetQuickPrelaunchBtn(BabuButton button)
        {
            OnClickSetPrelaunchBtnHotfix(null);
            OnClickSetTestBtnServer(null);
            Tips.PopTips("预发布环境，重启后生效");
        }

        private void OnClickSetMoreBtn(BabuButton button)
        {
            ShowMore(true);
        }
        private void ShowMore(bool showMore)
        {
            hotfixPanel.gameObject.SetActive(showMore);
            serverPanel.gameObject.SetActive(showMore);
            quickSetPanel.gameObject.SetActive(!showMore);
        }

        #endregion

        #region 热更新设置
        private void OnClickSetTestBtnHotfix(BabuButton button)
        {
#if UNITY_IOS
            string CbaBundleResVersionJsonUrlTest = GameInitialization.CbaBundleResVersionJsonUrlIosTest;
#else
            string CbaBundleResVersionJsonUrlTest = GameInitialization.CbaBundleResVersionJsonUrlAndroidTest;
#endif
            urlInputFieldHotfix.text = CbaBundleResVersionJsonUrlTest;
            SetCbaBundleResVersionJsonUrl(CbaBundleResVersionJsonUrlTest);
        }
        private void OnClickSetOnlineBtnHotfix(BabuButton button)
        {
#if UNITY_IOS
            string CbaBundleResVersionJsonUrlOnline = GameInitialization.CbaBundleResVersionJsonUrlIosOnline;
#else
            string CbaBundleResVersionJsonUrlOnline = GameInitialization.CbaBundleResVersionJsonUrlAndroidOnline;
#endif
            urlInputFieldHotfix.text = CbaBundleResVersionJsonUrlOnline;
            SetCbaBundleResVersionJsonUrl(CbaBundleResVersionJsonUrlOnline);
        }
        private void OnClickSetPrelaunchBtnHotfix(BabuButton button)
        {
#if UNITY_IOS
            string CbaBundleResVersionJsonUrlPrelaunch = GameInitialization.CbaBundleResVersionJsonUrlIosPrelaunch;
#else
            string CbaBundleResVersionJsonUrlPrelaunch = GameInitialization.CbaBundleResVersionJsonUrlAndroidPrelaunch;
#endif
            urlInputFieldHotfix.text = CbaBundleResVersionJsonUrlPrelaunch;
            SetCbaBundleResVersionJsonUrl(CbaBundleResVersionJsonUrlPrelaunch);
        }
        private void OnClickSetEditBtnHotfix(BabuButton button)
        {
            string CbaBundleResVersionJsonUrlEdit = urlInputFieldHotfix.text;
            SetCbaBundleResVersionJsonUrl(CbaBundleResVersionJsonUrlEdit);
        }
        private void SetCbaBundleResVersionJsonUrl(string url)
        {
            UnityEngine.PlayerPrefs.SetString("CbaBundleResVersionJsonUrl", url);
            Tips.PopTips("设置了新的热更新地址，重启后生效");
        }
        private void ReadUsedCbaBundleResVersionJsonUrl()
        {
            string UsedCbaBundleResVersionJsonUrl = UnityEngine.PlayerPrefs.GetString("CbaBundleResVersionJsonUrl", "");
            urlInputFieldHotfix.text = UsedCbaBundleResVersionJsonUrl;
        }
        #endregion

        #region 服务器列表设置

        private void OnClickSetTestBtnServer(BabuButton button)
        {
            urlInputFieldServerHost.text = ServerConst.ServerHostTest;
            UnityEngine.PlayerPrefs.SetString("ServerHostUrl", ServerConst.ServerHostTest);
            urlInputFieldServerBattleHost.text = ServerConst.ServerBattleHostTest;
            UnityEngine.PlayerPrefs.SetString("ServerBattleHostUrl", ServerConst.ServerBattleHostTest);
            Tips.PopTips("设置了测试地址，重启后生效");
        }
        private void OnClickSetOnlineBtnServer(BabuButton button)
        {
            urlInputFieldServerHost.text = ServerConst.ServerHostOnline;
            UnityEngine.PlayerPrefs.SetString("ServerHostUrl", ServerConst.ServerHostOnline);
            urlInputFieldServerBattleHost.text = ServerConst.ServerBattleHostOnline;
            UnityEngine.PlayerPrefs.SetString("ServerBattleHostUrl", ServerConst.ServerBattleHostOnline);
            Tips.PopTips("设置了正式地址，重启后生效");
        }
        private void OnClickSetEditBtnServerHost(BabuButton button)
        {
            string ServerHostEdit = urlInputFieldServerHost.text;
            UnityEngine.PlayerPrefs.SetString("ServerHostUrl", ServerHostEdit);
            Tips.PopTips("设置了新的服务器列表地址，重启后生效");
        }
        private void OnClickSetEditBtnServerBattleHost(BabuButton button)
        {
            string ServerBattleHostEdit = urlInputFieldServerBattleHost.text;
            UnityEngine.PlayerPrefs.SetString("ServerBattleHostUrl", ServerBattleHostEdit);
            Tips.PopTips("设置了新的战斗回放地址，重启后生效");
        }
        private void ReadUsedServerHost()
        {
            string UsedServerHost = UnityEngine.PlayerPrefs.GetString("ServerHostUrl", "");
            urlInputFieldServerHost.text = UsedServerHost;
            string UsedServerBattleHost = UnityEngine.PlayerPrefs.GetString("ServerBattleHostUrl", "");
            urlInputFieldServerBattleHost.text = UsedServerBattleHost;
        }
        #endregion
    }
}
