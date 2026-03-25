using System;
using System.Buffers;
using System.Collections.Generic;
using Babu;
using Babu.SDK;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using LightJson;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using YooAsset;
using static BigBang.LoginManager;
using SystemTask = System.Threading.Tasks.Task;

namespace BigBang.UI
{
    public enum LoadMode
    {
        // 异步加载
        Async,
        // 同步加载
        Sync,
        // 按需加载
        Demand
    }


    public class LoadingUIProperties : PanelProperties
    {

    }


    public class LoadingUI : APanelController<LoadingUIProperties>
    {
        [SerializeField] private GameObject HealthTextPanel;
        [SerializeField] private GameObject ProgressPanel;
        [SerializeField] private Image darkImage;
        [SerializeField] private GameObject LoginInputPanel;
        [SerializeField] private GameObject LoginDoingPanel;


        [SerializeField] private Slider progress;
        [SerializeField] private TMP_Text progresText;
        [SerializeField] private GameObject serverPanel;
        [SerializeField] private TMP_Text checkText;
        [SerializeField] private TMP_Text loginText;
        [SerializeField] private TMP_Text connectText;
        [SerializeField] private Image rotateImage;
        [SerializeField] private BabuButton startBtn;
        [SerializeField] private BabuButton retryBtn;
        [SerializeField] private BabuButton ageBtn;
        [SerializeField] private TMP_InputField accountField;
        [SerializeField] private TMP_InputField passwordField;
        [SerializeField] private Toggle readToggle;
        [SerializeField] private BabuButton PABtn;
        [SerializeField] private BabuButton UMBtn;
        [SerializeField] private Image PlayersImage;
        [SerializeField] private List<Image> ServerStateImageList = new();

        // 加载模式
        [SerializeField] private LoadMode loadMode;

        [SerializeField] private BabuButton serverChangeBtn;
        [SerializeField] private TMP_Text serverOfficialName;

        private bool startBtnLock = false;
        private ServerData serverData;

        public ServerData ServerData
        {
            get { return this.serverData; }
            set
            {
                this.serverData = value;
                Player.ServerData = this.serverData;
            }
        }
        protected override void Awake()
        {
            //PlayersImage.transform.localScale = Vector3.one * Utility.Lerp(0.74f, 1f, UIFrame.GetFixScreenLerpT());

            // 设置加载模式
            //loadMode = (LoadMode)PlayerPrefs.GetInt(PlayerPrefsKeys.LoadMode, (int)LoadMode.Sync);
            loadMode = LoadMode.Async;

        }
        protected void AddListenersAfterManagerLoaded()
        {
            startBtn.OnClick += OnStart;
            retryBtn.OnClick += OnRetry;
            ageBtn.OnClick += OnAgePanelShow;
            PABtn.OnClick += OnPrivacyAgreement;
            UMBtn.OnClick += OnUserManual;
            //// 注册登陆成功事件
            //LoginManager.Instance.OnLoginSucceed += OnLoginSucceed;
            //// 注册登陆失败事件
            LoginManager.Instance.OnLoginFailed += OnLoginFailed;
            // 注册游戏开始事件
            LoginManager.Instance.OnEnterGame += OnEnterGame;


            accountField.onValueChanged.AddListener(SaveAccount);
            passwordField.onValueChanged.AddListener(SavePassword);

            serverChangeBtn.OnClick += OnClickChangeServerBtn;

            EventManager.Instance.Register(EventID.OnClickChangeServerBtn, OnClickChangeServerBtn);
            EventManager.Instance.Register(EventID.QUICK_LOGIN_SUCCESS, OnQuickLoginSuccess);
            EventManager.Instance.Register(EventID.QUICK_LOGIN_FAIL, OnQuickLoginFail);
            //EventManager.Instance.Register(EventID.QUICK_INIT_END, OnQuickInitEnd);
        }

        protected override void RemoveListeners()
        {
            startBtn.OnClick -= OnStart;
            retryBtn.OnClick -= OnRetry;
            ageBtn.OnClick -= OnAgePanelShow;
            PABtn.OnClick -= OnPrivacyAgreement;
            UMBtn.OnClick -= OnUserManual;
            // 注销登陆成功事件
            //LoginManager.Instance.OnLoginSucceed -= OnLoginSucceed;
            // 注销登陆失败事件
            LoginManager.Instance.OnLoginFailed -= OnLoginFailed;
            // 注销游戏开始事件
            LoginManager.Instance.OnEnterGame -= OnEnterGame;

            accountField.onValueChanged.RemoveListener(SaveAccount);
            passwordField.onValueChanged.RemoveListener(SavePassword);

            serverChangeBtn.OnClick -= OnClickChangeServerBtn;
            EventManager.Instance.Unregister(EventID.OnClickChangeServerBtn, OnClickChangeServerBtn);
            EventManager.Instance.Unregister(EventID.QUICK_LOGIN_SUCCESS, OnQuickLoginSuccess);
            EventManager.Instance.Unregister(EventID.QUICK_LOGIN_FAIL, OnQuickLoginFail);
            //EventManager.Instance.Unregister(EventID.QUICK_INIT_END, OnQuickInitEnd);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            getDefaultServerTimes = 5;
            HealthTextPanel.SetActive(false);
            ProgressPanel.SetActive(false);
            LoginInputPanel.SetActive(false);
            LoginDoingPanel.SetActive(false);
            darkImage.gameObject.SetActive(false);
            HealthTextPanel.SetAlpha(0);
            ProgressPanel.SetAlpha(0);
            LoginInputPanel.SetAlpha(0);
            LoginDoingPanel.SetAlpha(0);
            darkImage.SetAlpha(0);
            loginText.gameObject.SetActive(true);
            connectText.gameObject.SetActive(false);

            HealthTextPanel.SetActive(true);
            HealthTextPanel.DOFade(1, 0.5f);

            startBtnLock = false;
            if (LoginManager.Instance.IsLoadSuccess)
            {
                LoginManager.Instance.ClearSilenceReLogin();
                AddListenersAfterManagerLoaded();
                GameBackToLoading();
                return;
            }
            // 进度条设置为0
            progress.value = 0;

            UnityTimer.Timer.Register(this.gameObject, 0.001f, () =>
            {
                GameInitialization.Inatance.RemoveFirstBgCanvas();
            });

            SetVersion();

            BeginLoading();


            // readToggle.isOn = PlayerPrefs.GetInt("read_toggle", 0) != 0;

            if (HeartbeatManager.Instance != null)
                HeartbeatManager.Instance.ClearAllSubscribe();
        }

        /// <summary>
        /// 从其他界面回到loading后此方法会被调用
        /// 应清理掉仅触发一次之类的状态
        /// </summary>
        private void GameBackToLoading()
        {
            retryed = true;

            GuideManager.ResetGuideSign();
            Player.ResetStrength();
            Player.ResetLevel();

            loginText.gameObject.SetActive(false);
            connectText.gameObject.SetActive(true);
            LoginDoingPanel.SetActive(true);
            DOTween.Kill(LoginDoingPanel);
            LoginDoingPanel.DOFade(1, 0.1f).AddTo(this.gameObject);

            LoadDefaultServerAsync();
        }

        private void OnQuickLoginSuccess(object[] args)
        {
            UnityTimer.Timer.Register(this.gameObject, 0.3f, () =>
            {
                string userId = (string)args[0];
                if (string.IsNullOrEmpty(LoginManager.Instance.miGuUserId) == false && LoginManager.Instance.miGuUserId != userId)
                {
                    LoginManager.Instance.isNeedCloseClientAfterChangeAccount = true;
                }
                if (LoginManager.Instance.isNeedCloseClientAfterChangeAccount)
                {
                    UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties("切换账号成功，请重新登陆", "账号切换", () =>
                    {
                        SDKManager.Instance.CloseGame();
                        ReturnToLogin();
                    }));
                    return;
                }
                if (LoginManager.Instance.isLoginOut)
                {
                    ReturnToLogin();
                    return;
                }

                LoginManager.Instance.miGuUserId = userId;
                this.LoginGameServer(userId);
                ReportQuickUserInfo(userId);
            });
        }
        private void ReportQuickUserInfo(string userId)
        {

        }

        private void OnQuickLoginFail(object[] objects)
        {
            ReturnToLogin();
        }
        private static void LogOutQuick()
        {
            UnityEngine.Debug.Log("LoadingUI: LogOutQuick");
            SDKManager.Instance.LogOut();
        }

        private void ReturnToLogin()
        {
            Debug.Log("ReturnToLogin");
            startBtnLock = false;

            LoginInputPanel.SetActive(true);
            DOTween.Kill(LoginInputPanel);
            LoginInputPanel.DOFade(1, 0.1f).AddTo(this.gameObject);

            LoginDoingPanel.SetActive(true);
            DOTween.Kill(LoginDoingPanel);
            LoginDoingPanel.DOFade(0, 0.1f).OnComplete(() =>
            {
                LoginDoingPanel.SetActive(false);
            }).AddTo(this.gameObject);
        }

        private int GetLastGameServer()
        {
            int lastServerId = PlayerPrefs.GetInt(PlayerPrefsKeys.LastGameServerSelectorId, -1);
            Debug.Log("LoadDefaultServerAsync , GetLastGameServer lastServerId = " + lastServerId);
            return lastServerId;
        }
        private void OnGameServerChanged(int id)
        {
            Debug.Log("LoadDefaultServerAsync , OnGameServerChanged save server id = " + id);
            PlayerPrefs.SetInt(PlayerPrefsKeys.LastGameServerSelectorId, id);
        }

        private void OnClickChangeServerBtn(BabuButton sender)
        {
            UIController.Instance.OpenWindow<ServerListUI>();
        }

        private void OnClickChangeServerBtn(object[] args)
        {
            UIController.Instance.CloseWindow<ServerListUI>();
            this.ServerData = (ServerData)args[0];
            LoginManager.Instance.ServerData = ServerData;
            this.UpdateServerInfoShow();

            OnGameServerChanged(this.ServerData.Id);
        }


        [PreserveAttribute]
        private void DoNotStrip()
        {
            LoginResponse.Parser.ParseFrom(new ReadOnlySequence<byte>());
        }

        //// 下载更新资源包
        //private async SystemTask DownloadAndUpdateAsset()
        //{
        //    Debug.Log("检查资源包更新");
        //    // 检查是否有资源更新
        //    bool hasUpdate = await AssetManager.Instance.CheckForUpdateAsync();
        //    if (!hasUpdate)
        //    {
        //        Debug.Log("暂无资源包更新");
        //        return;
        //    }
        //    // 获得更新包大小
        //    double downloadSize = await AssetManager.Instance.GetDownloadSizeAsync();
        //    Debug.Log("更新包大小:" + AssetManager.GetAssetSize(downloadSize));
        //    // 下载更新资源包
        //    await AssetManager.Instance.DownloadAssetAsync();
        //}

        private int getDefaultServerTimes = 0;
        private async void BeginLoading()
        {

            getDefaultServerTimes = 0;
            // 开始进度条动画
            await StartProgressAnim();

            Debug.Log("AfterHotFixSdks Start");

            // xc：移到GameIniialization中初始化
            //{
            //    var h = YooAssets.LoadAssetAsync<GameObject>("Prefabs/Launch/AfterHotFix.prefab");
            //    await h.Task;
            //    h.InstantiateSync();
            //}

            Debug.Log("AfterHotFixSdks End");

            AccountServiceManager.Instance.AddAccountService(AccountServiceManager.AccountServiceType.Local, new AccountServiceLocal());

            await TweenSliderAsync(0.2f);

            {
                var h = YooAssets.LoadAssetAsync<GameObject>("Prefabs/Launch/GameManger.prefab");
                await h.Task;
                h.InstantiateSync();
            }

            await TweenSliderAsync(0.3f);

            // 设置场景天空盒
            {
                var h = YooAssets.LoadAssetAsync<Material>("Prefabs/Launch/MainSceneSkyBox.mat");
                h.Completed += _ =>
                {
                    RenderSettings.skybox = h.AssetObject as Material;
                };
            }

            AddListenersAfterManagerLoaded();

            // 加载过滤文本
#if UNITY_WEGBL
            var handle = YooAssets.LoadAssetSync<TextAsset>(ResourcePath.TextPath + AssetNames.NameFilter);
            IllegalCharacter.Init((handle.AssetObject as TextAsset).text);
            handle.Release();
#else
            var handle = YooAssets.LoadAssetAsync<TextAsset>(ResourcePath.TextPath + AssetNames.NameFilter);
            handle.Completed += _ =>
            {
                IllegalCharacter.Init((handle.AssetObject as TextAsset).text);
                handle.Release();
            };
#endif

            await TweenSliderAsync(0.4f);

            switch (loadMode)
            {
                // 异步加载
                case LoadMode.Async:
                    await AsyncLoadMode();
                    break;
                // 同步加载
                case LoadMode.Sync:
                    await SyncLoadMode();
                    break;
                // 按需加载
                case LoadMode.Demand:
                    DemandLoadMode();
                    break;
            }
            LoadDefaultServerAsync();
        }

        private async SystemTask AsyncLoadMode()
        {
            Debug.Log("异步加载模式");
            //List<SystemTask> tasks = new List<SystemTask>();
            //// 加载图集图片
            //var spriteTask = SpriteManager.LoadAsync();
            //tasks.Add(spriteTask);
            // 加载配置表
            //tasks.Add(Configs.LoadAllAsync());
            _ = Configs.LoadAllAsync();
            Debug.Log("开始加载训练场景");
            var h = YooAssets.LoadAssetAsync<GameObject>("Prefabs/3DPrefab/Train/TrainActions.prefab");
            //tasks.Add(h.Task);
            await h.Task;
            //await SystemTask.WhenAll(tasks.ToArray());
            //TriggerManager.Instance.Init();
            //ClassicManager.Instance.Init();
            //HeroManager.Instance.Init();
            //BountyTaskManager.Instance.Init();
            GameObject.Instantiate(h.AssetObject as GameObject);
            //LoadDefaultServerAsync();
            //float loadSpriteTime = spriteTask.Result;
            //PrintLoadInfo(loadSpriteTime, 0);
        }

        private async SystemTask SyncLoadMode()
        {
            Debug.Log("同步加载模式");
            // 加载图集图片
            float loadSpriteTime = SpriteManager.Load();
            await TweenSliderAsync(0.5f);
            // 加载配置表
            Configs.LoadAll();
            TriggerManager.Instance.InitOnce();
            ClassicManager.Instance.InitOnce();
            HeroManager.Instance.InitOnce();
            HundredManager.Instance.InitOnce();
            RedEnvlopeManager.Instance.InitOnce();
            AllStarManager.Instance.InitOnce();
            BountyTaskManager.Instance.InitOnce();
            await TweenSliderAsync(0.6f);
            Debug.Log("加载3D模型场景");
            var h = YooAssets.LoadAssetAsync<GameObject>("Prefabs/3DPrefab/Train/TrainActions.prefab");
            await h.Task;
            GameObject.Instantiate(h.AssetObject as GameObject);
            await TweenSliderAsync(0.8f);
            PrintLoadInfo(loadSpriteTime, 0);
        }

        private void DemandLoadMode()
        {
            Debug.Log("按需加载模式");
        }

        // 显示加载信息
        private void PrintLoadInfo(float spriteTime, float configTime)
        {
            Debug.Log($"图集加载耗时:{spriteTime}s");
            //Debug.Log($"配置表加载耗时:{configTime}s");
        }

        // 点击开始按钮
        private void OnStart(BabuButton sender)
        {
            if (startBtnLock) return;// 防止开始游戏按钮连续多次点击触发相同事件
            startBtnLock = true;

            LoginManager.Instance.isLoginOut = false;

            //// 隐私协议和用户协议交由QuiSDK控制
            // if (readToggle.isOn == false)
            // {
            //     Tips.PopTips("请阅读并同意《隐私协议》和《用户协议》");
            //     startBtnLock = false;
            //     return;
            // }


            //             if (passwordField.text.CompareTo("123456") != 0)
            //             {
            // #if UNITY_EDITOR
            //                 Tips.PopTips("密码是123456");
            // #else
            //                 Tips.PopTips("密码错误");
            // #endif
            //                 return;
            //             }

            LoginInputPanel.SetActive(true);
            DOTween.Kill(LoginInputPanel);
            LoginInputPanel.DOFade(0, 0.1f).OnComplete(() =>
            {
                LoginInputPanel.SetActive(false);
            }).AddTo(this.gameObject);

            loginText.gameObject.SetActive(true);
            connectText.gameObject.SetActive(false);
            LoginDoingPanel.SetActive(true);
            DOTween.Kill(LoginDoingPanel);
            LoginDoingPanel.DOFade(1, 0.1f).AddTo(this.gameObject);

            DOTween.Kill(darkImage);
            darkImage.gameObject.SetActive(true);
            darkImage.DOFade(0, 0.5f).AddTo(this.gameObject);

#if UNITY_EDITOR || UNITY_WEBGL
            this.LoginGameServer(null);
#else
            SDKManager.Instance.DoLogin();
#endif

        }

        private void LoginGameServer(string userId)
        {
            Debug.Log("LoginGameServer 1");
            Timer.Register(this.gameObject, 3, () => { startBtnLock = false; });
            // 关闭套接字连接
            Debug.Log("LoginGameServer 2");
            SocketService.Instance.Close();

            Debug.Log("LoginGameServer 3");
            string accountId = accountField.text;
            if (string.IsNullOrEmpty(userId) == false)
            {
                Debug.Log("LoginGameServer 4");
                accountId = "MiGuPlay" + "|" + userId;
            }
            Debug.Log("LoginGameServer 5");
            LoginManager.Instance.accountId = accountId;
            Debug.Log("LoginGameServer 6");
            AccountServiceManager.Instance.Login(accountId, AccountServiceManager.AccountServiceType.Local, result =>
            {
                Debug.Log("LoginGameServer 7");
                if (!result || LoginManager.Instance.isNeedCloseClientAfterChangeAccount)
                {
                    Debug.Log("LoginGameServer 8");
                    ReturnToLogin();
                    return;
                }
                Debug.Log("LoginGameServer 9");
                // 登陆服务器
                LoginUserInfo loginUserInfo = null;
                if(string.IsNullOrEmpty(userId) == false)
                {
                    loginUserInfo = new(userId);
                }
                LoginServerAsync(loginUserInfo);
                Debug.Log("LoginGameServer 10");
            });
            Debug.Log("LoginGameServer 11");
        }

        //用户协议
        private void OnUserManual(BabuButton obj)
        {
            var url = PolicyConst.GetPolicyUrl();
            Application.OpenURL(url);
        }

        //隐私协议
        private void OnPrivacyAgreement(BabuButton obj)
        {
            var url = PolicyConst.GetPrivacyUrl();
            Application.OpenURL(url);
        }

        private void UpdateServerInfoShow()
        {
            LoginManager.Instance.InitLoginWhenStart();
            serverOfficialName.text = this.ServerData.AliasName + " " + this.ServerData.OfficialName;
            foreach (Image ServerStateImage in ServerStateImageList)
            {
                if (ServerStateImage.name == "ServerStateImage" + (ServerStatus)this.serverData.Status)
                {
                    ServerStateImage.gameObject.SetActive(true);
                }
                else
                {
                    ServerStateImage.gameObject.SetActive(false);
                }
            }
        }
        private bool retryed = false;
        private void OnRetry(BabuButton sender)
        {
            retryBtn.gameObject.SetActive(false);

            loginText.gameObject.SetActive(false);
            connectText.gameObject.SetActive(true);
            LoginDoingPanel.SetActive(true);
            DOTween.Kill(LoginDoingPanel);
            LoginDoingPanel.DOFade(1, 0.1f).AddTo(this.gameObject);

            getDefaultServerTimes = 0;
            LoadDefaultServerAsync();
        }
        private void LoadDefaultServerAsync()
        {
            int lastServer = this.GetLastGameServer();
            string url = $"{ServerConst.SERVER_SELECTOR_URL}?last_server={lastServer}&account_id={AccountServiceManager.Instance.GetAccountId(AccountServiceManager.AccountServiceType.Local)}&game_name={Application.identifier}";
            Debug.Log("LoadDefaultServerAsync , url = " + url);

            UnityHttpServiceFix.Instance.AsyncGet(url, async (bool result, string response) =>
            {
                try
                {
                    getDefaultServerTimes++;
                    if (result == false) throw new Exception("Request Failed");
                    JsonValue json = JsonValue.Parse(response);
                    this.ServerData = new ServerData(json);
                    LoginManager.Instance.ServerData = ServerData;

                    ReportServerInfo();

                    OnGameServerChanged(this.ServerData.Id);

                    Debug.Log($"LoadDefaultServerAsync , Server Info: {this.ServerData.Id},{this.ServerData.Ip}:{this.ServerData.Port}");

                    this.UpdateServerInfoShow();

                    await TweenSliderAsync(0.85f);
                    LoadDefaultServerCompleted();
                }
                catch (Exception ex)
                {
                    retryed = true;
                    if (getDefaultServerTimes <= 5)
                    {
                        UnityTimer.Timer.Register(this.gameObject, 3, () =>
                        {
                            LoadDefaultServerAsync();
                        });
                    }
                    else
                    {
                        retryBtn.gameObject.SetActive(true);

                        LoginDoingPanel.SetActive(true);
                        DOTween.Kill(LoginDoingPanel);
                        LoginDoingPanel.DOFade(0, 0.5f).OnComplete(() =>
                        {
                            LoginDoingPanel.SetActive(false);
                        }).AddTo(this.gameObject);

                        Tips.PopTips("获取服务器失败:" + ex.Message);
                    }
                }
            });
        }
        // 登陆服务器
        private async void LoginServerAsync(LoginUserInfo userInfo)
        {
            try
            {
                ReportServerInfo();
                LoginManager.Instance.IsBackByKickOff = false;
                if (await SocketService.Instance.Open(this.ServerData.Ip, this.ServerData.Port, 2))
                {
                    // 登陆游戏
                    LoginManager.Instance.Login(userInfo);
                }
                else
                {
                    Debug.LogError("Connect To Server Failed!");
                    Tips.PopTips("连接服务器失败");
                    ReturnToLogin();
                }
            }
            catch (Exception ex)
            {
                Tips.PopTips("无法连接服务器");
                //Tips.PopTips($"登陆服务器失败: {this.ServerData.Ip}:{this.ServerData.Port}");
                Debug.LogError($"LoginServerAsync Failed Exception: " + ex.Message);
                ReturnToLogin();
            }

    /*
    try
    {
        //测试外网 112.74.79.48:10017
        //测试内网 10.200.0.102:10010
        //string ip = "112.74.79.48";
       // int port = 10017;
        string ip = "10.200.0.102";
        int port = 10010;
        Debug.Log($"Server Info: {ip}:{port}");
        if (SocketService.Instance.Open(ip, port, 2))
        {
            // 登陆游戏
            LoginManager.Instance.Login();
        }
        else
        {
            Debug.LogError("Connect To Server Failed!");
        }
    }
    catch (Exception ex)
    {
        //Debug.LogError($"Request Game Server Selector Failed: {url}, Exception: " + ex.Message);
        throw ex;
    }*/
}

        private void ReportServerInfo()
        {

        }

        // 登陆失败事件
        private void OnLoginFailed()
        {
            Debug.Log("Login Failed!");
            Tips.PopTips("Login Failed!");
            ReturnToLogin();
        }

        // 游戏开始事件
        private void OnEnterGame()
        {
            // 防止开始游戏按钮连续多次点击触发相同事件


        }

        // 隐私协议
        private void OnAgePanelShow(BabuButton sender)
        {
            UIController.Instance.OpenWindow<UserPrivacyDetailUI>(new UserPrivacyProperties(PrivacyType.Age));
        }



        private int quickRetryTimes = 0;
        private readonly int quickRetryTimesMax = 3;
        private bool calInitQuickAgain = false;
        private void LoadDefaultServerCompleted()
        {
#if UNITY_EDITOR || UNITY_WEBGL
            _ = OnLoadQuickCompleted();
#else
            quickRetryTimes = 0;
            calInitQuickAgain = true;
            _ = CkeckQuickInit();
#endif
        }
        //private void OnQuickInitEnd(object[] args)
        //{
        //    if (calInitQuickAgain == false) return;
        //    _ = CkeckQuickInit();
        //}


        private async SystemTask CkeckQuickInit()
        {
            await OnLoadQuickCompleted();
        }

        // 加载完成事件
        private async SystemTask OnLoadQuickCompleted()
        {

            await TweenSliderAsync(0.9f);
            ProgressPanel.SetActive(true);
            DOTween.Kill(ProgressPanel);
            ProgressPanel.DOFade(0, 0.5f).OnComplete(() =>
            {
                ProgressPanel.SetActive(false);
            }).AddTo(this.gameObject);

            LoginDoingPanel.SetActive(true);
            DOTween.Kill(LoginDoingPanel);
            LoginDoingPanel.DOFade(0, 0.5f).OnComplete(() =>
            {
                LoginDoingPanel.SetActive(false);
            }).AddTo(this.gameObject);

            LoginInputPanel.SetActive(true);
            LoginInputPanel.SetAlpha(0);
            LoginInputPanel.DOFade(1, 0.5f).AddTo(this.gameObject);

            darkImage.gameObject.SetActive(true);
            darkImage.DOFade(0.65f, 3.5f).AddTo(this.gameObject);

            LayoutRebuilder.ForceRebuildLayoutImmediate(serverPanel.GetComponent<RectTransform>());

#if UNITY_EDITOR
            LoadAccount();
            LoadPassword();
            this.accountField.gameObject.SetActive(true);
            this.passwordField.gameObject.SetActive(true);
#else
            this.accountField.gameObject.SetActive(false);
            this.passwordField.gameObject.SetActive(false);
#endif

            AudioManager.Instance.PlayMusic(AudioNames.BGM_TRAINING);

            LoginManager.Instance.IsLoadSuccess = true;

            _ = TweenSliderAsync(1.0f);

            _ = ServerNoticeManager.Instance.GetAndShowServerNotice();
        }

        #region 临时保存账号密码

        private void SaveAccount(string str)
        {
            //local login 用
            StorageManager.Instance.Store("account.txt", str);
        }
        private void LoadAccount()
        {
            string accountStr = StorageManager.Instance.Load("account.txt");
            if (accountStr != null)//第一次进入可能并未保存账号
            {
                accountField.text = accountStr;
            }
            //accountField.text = PlayerPrefs.GetString("accountField.text", "");
        }
        private void SavePassword(string str)
        {
            PlayerPrefs.SetString("passwordField.text", str);
        }
        private void LoadPassword()
        {
            passwordField.text = PlayerPrefs.GetString("passwordField.text", "");
        }

        #endregion

        #region 虚假的进度条

        private async SystemTask TweenSliderAsync(float newProgress)
        {
            bool isSliderTweenEnd = false;
            progress.DOValue(newProgress, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                isSliderTweenEnd = true;
            }).SetUpdate(true).OnUpdate(() =>
            {
                progresText.text = Mathf.RoundToInt(progress.value * 100) + "%";
            }).AddTo(this.gameObject);
            while (isSliderTweenEnd == false)
            {
                await SystemTask.Yield();
            }

        }

        private async SystemTask StartProgressAnim()
        {
            ProgressPanel.SetActive(true);
            ProgressPanel.DOFade(1, 0.5f).AddTo(this.gameObject);
            progresText.text = "0%";
            await TweenSliderAsync(0.1f);
        }

        #endregion

        #region



        [SerializeField] private TextMeshProUGUI versionText;
        private void SetVersion()
        {
#if !UNITY_WEBGL
            var handle = YooAssets.LoadAssetSync<TextAsset>(ResourcePath.TextPath + AssetNames.BundleVersion);
            string bundleCreatTime = (handle.AssetObject as TextAsset).text;
            handle.Release();
            SetVersionInternal(bundleCreatTime);
#else
            var handle = YooAssets.LoadAssetAsync<TextAsset>(ResourcePath.TextPath + AssetNames.BundleVersion);
            handle.Completed += _ =>
            {
                string bundleCreatTime = (handle.AssetObject as TextAsset).text;
                handle.Release();
                SetVersionInternal(bundleCreatTime);
            };
#endif
        }

        void SetVersionInternal(string bundleCreatTime)
        {
            string clientCreatTime = Babu.Environment.GetValue("client_creat_time", "");
            Babu.Environment.bundleCreatTime = bundleCreatTime;
            string versionStr = "客户端版本:{0}\n资源包版本:{1}".SafeFormat(clientCreatTime, bundleCreatTime);
            versionText.text = versionStr;
            Debug.Log(versionStr);
        }

        #endregion

        private void OnLoginout(object[] args)
        {
            // QuickUserInfo qui = (QuickUserInfo)args[0];


        }

    }
}
