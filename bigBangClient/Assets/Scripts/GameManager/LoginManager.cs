using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using Babu.Client.Fsm;
using Babu.SDK;
using BigBang.UI;
using GameConfig;
using Protocol;
using UnityEngine;
using UnityTimer;
using Utils;

namespace BigBang
{
    enum NetworkCallStatus
    {
        None,
        SENDING,
        CALLBACK
    }
    public class LoginManager : Singleton<LoginManager>
    {
        /// <summary>
        /// 不需要转圈的接口
        /// 通常界面刷新类的接口不需要转圈，按钮交互类的需要有转圈
        /// </summary>
        private readonly List<string> NoNetMaskList = new List<string>
        {
            ProcID.Heart, //这个肯定不要转圈
            ProcID.SyncTrainEvents,
            ProcID.CheckPlayerAchievement,
            ProcID.SaveFormation,
            ProcID.SetGoodsAsOld,
            ProcID.CollectTaskReward,
            ProcID.ReviseName,
            ProcID.GetNFTGoods,
            ProcID.GetMainUIMatch,
            ProcID.GetRankList,

            ProcID.GetHundredCourse,
            ProcID.GetHundredHof,
            ProcID.GetLeagueData,
            ProcID.GetLeagueChampionRank,
            ProcID.GetLeagueHistory,
            ProcID.GetChallengeMapData,
            ProcID.ReceiveCompitionReward,

            ProcID.LikeRedPacket,
            ProcID.GetRedPacketInfo,
            ProcID.GetRedPacketMarquees,
            ProcID.GetRedPacketLogs,

            ProcID.GetHundredSupport,
            ProcID.GetCourseTeamData,
            ProcID.GetFinalsGuessInfo,
            ProcID.GetDragonBoatInfo,
        };

        public bool IsBackByKickOff = false;

        /// <summary>
        /// Loading界面的资源进度条走完，可以进行登录时为true
        /// </summary>
        public bool IsLoadSuccess = false;

        // 登陆失败事件
        public event System.Action OnLoginFailed;
        // 游戏开始事件
        public event System.Action OnEnterGame;
        /// <summary>
        /// 冷启动标识
        /// </summary>
        public bool ColdStartWithGiftWindow = false;

        private bool initial = false;
        private NetworkCallStatus netStatus = NetworkCallStatus.None;

        public class LoginUserInfo
        {
            public string userId = "";
            public string channel = "MiGuPlay";

            public LoginUserInfo(string userId)
            {
                this.userId = userId;
            }
        }
        public void Login(LoginUserInfo userInfo)
        {
            if (LoginManager.Instance.isLoginOut)
            {
                LoginManager.Instance.BackToLogin();
                return;
            }
            ServerNotifyManager.Instance.Init();
            NetworkManager.Instance.Login(userInfo, LoginCallBack);
        }

        private void LoginCallBack(LoginResponse response)
        {
            if (LoginManager.Instance.isLoginOut)
            {
                LoginManager.Instance.BackToLogin();
                return;
            }
            if (response.Session.Length == 0)
            {
                MiGuPlayManager.Instance.ReportCPLoginResult(false);
                OnLoginFailed?.Invoke();
                if (isDoingSilenceReLogin) BackToLogin();
            }
            else
            {
                if (response.NeedUpdate)
                {
                    MiGuPlayManager.Instance.ReportCPLoginResult(false);
                    if (isDoingSilenceReLogin) ClearSilenceReLogin();
                    UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties("需要更新游戏客户端", Lang.Get(LangID.ConfirmTxt), () =>
                    {
                        SDKManager.Instance.CloseGame();
                    }));
                    return;
                }
                MiGuPlayManager.Instance.ReportCPLoginResult(true);
                Babu.Environment.SetValue("session", response.Session);
                DataConvUtil.SetServerTime(response.ServerTime);
                ChannelManager.Instance.SetInfo(response.ChannelInfo);
                //初始化模板表
                GameManager.InitManager();
                //取回所有球员数据
                NetworkManager.Instance.FetchAllPlayer(LoginManager.Instance.AfterFetchAllPlayersCallBack);

                //if (isDoingSilenceReLogin) NetworkManager.Instance.FetchAllPlayer(AfterFetchAllPlayersCallBack);
            }
        }

        public void AfterFetchAllPlayersCallBack(FetchAllPlayersResponse response)
        {
            if (isDoingSilenceReLoginByCreatePlayer)
            {
                isDoingSilenceReLoginByCreatePlayer = false;
                silenceReLoginByCreatePlayerCallback?.Invoke();
                return;
            }
            if (LoginManager.Instance.isLoginOut)
            {
                LoginManager.Instance.BackToLogin();
                return;
            }
            if (response.Players.Count == 0)
            {
#if !UNITY_WEBGL
                UIController.Instance.OpenWindow<PlayMovieUI>(new PlayMovieUIProperties("FirstEnterMovieVideoH265", () =>
                {
                    UIController.Instance.ShowPanel<Guide1UI>();
                }));
#else
                UIController.Instance.ShowPanel<Guide1UI>();
#endif
            }
            else
            {
                BasicPlayerInfoNotify basicPlayerInfoNotify = response.Players.First();
                Player.GbId = basicPlayerInfoNotify.Gbid;
                Player.Name = basicPlayerInfoNotify.Name;
                Player.UpLevel = basicPlayerInfoNotify.Level;
                Player.UpStrengh = basicPlayerInfoNotify.Strength;
                Player.UpCreateTime = basicPlayerInfoNotify.CreateTime;
                // 直接进入游戏
                EnterGame(response.Players.First().Gbid);
            }
        }

        public void EnterGame(string gbid)
        {
            MainThreadTaskService.Instance.StartCoroutine(EnterGameAsync(gbid));
        }

        IEnumerator EnterGameAsync(string gbid)
        {
            Player.GbId = gbid;
            var startTime = DateTime.Now;
            while (Configs.LeftCount > 0)
            {
                yield return null;
            }

            Debug.Log($"Wait Time: {(DateTime.Now - startTime).TotalMilliseconds}");
            RedDotManager.Instance.InitRedTree();
            TriggerManager.Instance.InitOnce();
            ClassicManager.Instance.InitOnce();
            HeroManager.Instance.InitOnce();
            HundredManager.Instance.InitOnce();
            RedEnvlopeManager.Instance.InitOnce();
            AllStarManager.Instance.InitOnce();
            BountyTaskManager.Instance.InitOnce();

            NetworkManager.Instance.EnterGame(gbid, EnterGameCallBack);

            ByteDanceManager.Instance.ReportLogin();
        }

        public void EnterGameCallBack(EnterGameResponse response)
        {
            if (!response.Result)
            {
                Debug.Log("开始游戏失败");
                return;
            }

            if (LoginManager.Instance.isLoginOut)
            {
                LoginManager.Instance.BackToLogin();
                return;
            }

            AllStarManager.Instance.ProcessUnPack();

            HeartbeatManager.Instance.ClearAllSubscribe();

            HeartbeatManager.Instance.Subscribe(new IncomeHeartbeat(GameConst.AddExpMinUnitSecond));
            HeartbeatManager.Instance.Subscribe(new SyncTrainHeartbeat(GameConst.SyncTrainEventUnitSecond));
            HeartbeatManager.Instance.Subscribe(new SyncHandshakeHeartbeat(GameConst.SyncHandshakeUnitSecond));

            NetworkStatusListener.Instance.SetCloseFin(true);

            if (isDoingSilenceReLogin) AfterReloginSuccess();
            OnEnterGame?.Invoke();
            //取完数据后初始化
            Player.LoginSuccess();

            HundredManager.Instance.CheckHundredRedDotWhenEnterGame();
            FsmManager.Instance.ChangeToState<StateHome>(new StateCommonUserData()
            {
                OpenUIAction = async () =>
                {
                    //准备启动就要弹的各种礼包窗口，不能挪到其他地方，例如unpack，在服务器启动的时候有可能被推送多次。
                    ActivityController.Instance.PrepareStartWindow();

                    await UIController.Instance.ShowPanel<HomeUI>(new HomeUIProperties(true));
                    //处理登录后的弹窗逻辑，打开需要登录就弹的窗口

                    isBeforeLoadingEnd = false;

                    GuideManager.UpdatePopwindowFlag();
                    if (UIController.Instance.PopwindowFlag) UIController.Instance.OpenAllHideScreens();

                    this.InitWhenEnterGame();
                }
            });
        }

        private void InitWhenEnterGame()
        {
            if (this.initial == true) return;
            this.initial = true;

            EventManager.Instance.Register(EventID.NETWORK_CALLBACK, OnNetWorkCallback);
            EventManager.Instance.Register(EventID.NETWORK_SENDING, OnNetWorkSending);
            EventManager.Instance.Register(EventID.HEART_BEAT_OVERTIME, OnHeartBeatOvertime);

            SocketService.Instance.AddIgnoreMaskReq(NoNetMaskList);
        }

        private bool isLoginInited = false;
        public void InitLoginWhenStart()
        {
            if (this.isLoginInited == true) return;
            this.isLoginInited = true;

            EventManager.Instance.Register(EventID.QUICK_SWITCH_ACCOUNT, OnSwitchAccount);
            EventManager.Instance.Register(EventID.QUICK_LOGIN_OUT, OnLoginout);
        }

        /// <summary>
        /// Loading界面完成后关闭，进入游戏后为false
        /// </summary>
        public bool isBeforeLoadingEnd = true;

        private void OnHeartBeatOvertime(object[] args)
        {
            if (isBeforeLoadingEnd == true) return;

            UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(Lang.Get(LangID.NetworkConnectionTimeout), Lang.Get(LangID.ConfirmTxt), () =>
            {
                BackToLogin();
            }));
        }
        private void OnNetWorkCallback(object[] args)
        {
            string methodName = args[0] as string;

            netStatus = NetworkCallStatus.CALLBACK;

            UIController.Instance.CloseWindow<NetworkMaskUI>(false);

            Debug.Log("---->OnNetWorkCallback");

        }
        private void OnNetWorkSending(object[] args)
        {
            string methodName = args[0] as string;

            netStatus = NetworkCallStatus.SENDING;
            // UIController.Instance.OpenWindow<NetworkMaskUI>();
            Timer.RegisterWithNoBindGameObject(0.15f, CheckNetwrokDelay);
        }
        private void CheckNetwrokDelay()
        {
            Debug.Log("---->CheckNetwrokDelay set ");
            if (NetworkCallStatus.SENDING == netStatus)
            {
                Debug.Log("---->CheckNetwrokDelay call");
                UIController.Instance.OpenWindow<NetworkMaskUI>();
            }
        }

        public bool isNeedCloseClientAfterChangeAccount = false;

        private void OnSwitchAccount(object[] args)
        {
            //QuickUserInfo qui = (QuickUserInfo)args[0];
            Debug.Log("LoginManager ,  OnSwitchAccount");
            BackToLogin();
            isNeedCloseClientAfterChangeAccount = true;
            if (LoginManager.Instance.isNeedCloseClientAfterChangeAccount)
            {
                UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties("切换账号成功，请重新登陆", "账号切换", () =>
                {
                    SDKManager.Instance.CloseGame();
                }));
                return;
            }

            //GuideManager.UpdatePopwindowFlag();
        }

        public bool isLoginOut = false;
        private void OnLoginout(object[] args)
        {
            // QuickUserInfo qui = (QuickUserInfo)args[0];
            Debug.Log("LoginManager ,  OnLoginout");
            isLoginOut = true;
            BackToLogin();
        }

        #region 默默重连

        public string miGuUserId = "";
        public ServerData ServerData = null;

        public bool isCheckingSilenceReLoginHeart = false;
        bool isGetHeart = false;
        Timer waitTimer = null;
        public void CheckSilenceReLogin()
        {
            Debug.Log("CheckSilenceReLogin");
            if (isBeforeLoadingEnd || isCheckingSilenceReLoginHeart || isDoingSilenceReLogin) return;
            //if (Application.internetReachability == NetworkReachability.NotReachable)
            //{
            //    BackToLogin();
            //    return;
            //}

            isGetHeart = false;
            isCheckingSilenceReLoginHeart = true;
            waitTimer = UnityTimer.Timer.RegisterWithNoBindGameObject(0.7f, WaitHeart);
            NetworkManager.Instance.SyncHandshakeHeartbeat((resp) =>
            {
                isGetHeart = true;
                isCheckingSilenceReLoginHeart = false;
                GuideManager.UpdatePopwindowFlag();
            });
            GuideManager.UpdatePopwindowFlag();
        }
        private void WaitHeart()
        {
            if (isGetHeart == false)
            {
                DoSilenceReLogin();
            }
        }

        public bool isDoingSilenceReLogin = false;
        public string accountId = "";
        public bool isDoingSilenceReLoginByCreatePlayer = false;
        public Action silenceReLoginByCreatePlayerCallback = null;
        public void DoSilenceReLogin()
        {
            if (LoginManager.Instance.isNeedCloseClientAfterChangeAccount) return;
            if (LoginManager.Instance.isLoginOut) return;
            Debug.Log("DoSilenceReLogin");
            isDoingSilenceReLogin = true;
            GuideManager.UpdatePopwindowFlag();
            if (waitTimer != null) UnityTimer.Timer.Cancel(waitTimer);
            waitTimer = null;
            UIController.Instance.CloseAllPanelAndWindow();
            if (LoginManager.Instance.isDoingSilenceReLoginByCreatePlayer == false)
            {
                UIController.Instance.ShowPanel<HomeUI>();
            }
            UIController.Instance.OpenWindow<NetworkMaskUI>();

            HeartbeatManager.Instance.ClearAllSubscribe();
            NetworkStatusListener.Instance.SetCloseFin(false);
            SocketService.Instance.Close();

            if (ServerData == null)
            {
                BackToLogin();
                return;
            }

            AccountServiceManager.Instance.Login(accountId, AccountServiceManager.AccountServiceType.Local, async result =>
            {
                if (!result || isLoginOut || isNeedCloseClientAfterChangeAccount)
                {
                    BackToLogin();
                    return;
                }
                try
                {
                    //UnityTimer.Timer.Register(this.gameObject, 0.5f, () =>
                    //{
                    if (await SocketService.Instance.Open(this.ServerData.Ip, this.ServerData.Port, 2))
                    {
                        if (!result || isLoginOut || isNeedCloseClientAfterChangeAccount)
                        {
                            BackToLogin();
                            return;
                        }
                        LoginUserInfo loginUserInfo = new(miGuUserId);
                        LoginManager.Instance.Login(loginUserInfo);
                    }
                    else
                    {
                        BackToLogin();
                        return;
                    }
                    //});

                }
                catch (Exception)
                {
                    BackToLogin();
                    return;
                }
            });

        }
        public void ClearSilenceReLogin()
        {
            Debug.Log("ClearSilenceReLogin");
            isDoingSilenceReLogin = false;
            isDoingSilenceReLoginByCreatePlayer = false;
            if (waitTimer != null) UnityTimer.Timer.Cancel(waitTimer);
            waitTimer = null;
            isCheckingSilenceReLoginHeart = false;
            isGetHeart = false;
            GuideManager.UpdatePopwindowFlag();
        }

        public void AfterReloginSuccess()
        {
            Debug.Log("AfterReloginSuccess");
            ClearSilenceReLogin();
            UIController.Instance.CloseAllPanelAndWindow();
            TouchManager.Instance.EnableTouch();
        }

        public void BackToLogin()
        {
            Debug.Log("BackToLogin");
            isBeforeLoadingEnd = true;
            GuideManager.UpdatePopwindowFlag();
            ClearSilenceReLogin();
            HeartbeatManager.Instance.ClearAllSubscribe();
            NetworkStatusListener.Instance.SetCloseFin(false);
            SocketService.Instance.Close();
            UIController.Instance.CloseAllPanelAndWindow();
            FsmManager.Instance.ChangeToState<StateLoading>();
        }


        #endregion
    }
}
