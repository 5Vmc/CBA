using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System.Linq;
using DG.Tweening;
using System.Collections.Generic;
using BigBang.Battle;
using UnityTimer;
using Spine.Unity;
using GameConfig;
using Babu.Client.Fsm;
using System;
using Spine;
using GameConfig.Config;

namespace BigBang.UI
{

    public class HomeUIProperties : PanelProperties
    {
        public bool isOpenByBottomTab = false;
        public HomeUIProperties(bool isOpenByBottomTab)
        {
            this.isOpenByBottomTab = isOpenByBottomTab;
        }
    }

    public class HomeUI : APanelController<HomeUIProperties>
    {
        #region 初始化
        //[SerializeField] private PageView activetyPageView;
        protected override void AddListeners()
        {
            base.AddListeners();

            //EventManager.Instance.Register(EventID.OnHomeUIRedDotReady, RefreshRedDot);

            surveyBtn.onClick.AddListener(OnClickSurvey);

            //backpackButton.onClick.AddListener(OnInventoryBtn);
            //achievementBtn.onClick.AddListener(OnAchievementBtn);
            //emailBtn.onClick.AddListener(OnEmailBtn);
            //activityBtn.onClick.AddListener(OnActivityBtn);
            //festivalGiftBtn.onClick.AddListener(OnFestivalGift);
            //christmasButton.onClick.AddListener(OnChristmasButton);
            //newYearHomeButton.onClick.AddListener(OnNewYearHomeButton);
            //newYearSignButton.onClick.AddListener(OnNewYearSignButton);

            //EventManager.Instance.Register(EventID.OnRefreshEmail, RefreshEmailRedDot);
            //EventManager.Instance.Register(EventID.OnTeamlevelUp, RefreshFuncLock);
            //EventManager.Instance.Register(EventID.RefreshBigBangUIRedDot, RefreshTrainRedDot);

            //activetyPageView.OnStartScroll += OnStartScroll;
            //activetyPageView.OnResetCount += OnResetActivityCount;

            //challengeBtn.OnClick += OnChallengeBtn;
            //arenaBtn.OnClick += OnArenaBtn;
            //leagueBtn.OnClick += OnLeagueBtn;
            //storyBtn.OnClick += OnStoryBtn;
            //hundredBtn.OnClick += OnHundredBtn;
            //trainBtn.OnClick += OnTrainBtn;
            //stuntBtn.OnClick += OnStuntBtn;
            //taskBtn.OnClick += OnTaskBtn;
            //practiceBtn.OnClick += OnPricticeBtn;
            //nftBtn.OnClick += OnClickNftBtn;
            //mainTaskBtn.OnClick += OnMainTaskBtn;
            //workBtn.OnClick += OnWorkBtn;
            //timeGiftBtn.onClick.AddListener(OnTimeGift);


            DevelopBtn.onClick.AddListener(OnDevelopBtn);

            homeTopButtonMore.OnClick += OnClickMore;
            EventManager.Instance.Register(EventID.OnHundredGetMineInfo, OnHundredGetMineInfo);
            SecondUpdateManager.Instance.RegistAction(RefreshArenaInfo);
            SecondUpdateManager.Instance.RegistAction(RefreshStageAndTime);
            EventManager.Instance.Register(EventID.OnArenaGetNewInfo, OnArenaGetNewInfo);
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
            jumpGuideButton.OnClick += OnClickJumpGuideButton;
            normalTimePanelButton.OnClick += OnClickNormalTimePanelButton;
            allStarPanelNorthButton.OnClick += OnClickAllStarPanelNorthButton;
            allStarPanelSouthButton.OnClick += OnClickAllStarPanelSouthButton;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            //EventManager.Instance.Unregister(EventID.OnHomeUIRedDotReady, RefreshRedDot);
            surveyBtn.onClick.RemoveListener(OnClickSurvey);

            //backpackButton.onClick.RemoveListener(OnInventoryBtn);
            //achievementBtn.onClick.RemoveListener(OnAchievementBtn);
            //emailBtn.onClick.RemoveListener(OnEmailBtn);
            //activityBtn.onClick.RemoveListener(OnActivityBtn);
            //festivalGiftBtn.onClick.RemoveListener(OnFestivalGift);
            //christmasButton.onClick.RemoveListener(OnChristmasButton);
            //newYearHomeButton.onClick.RemoveListener(OnNewYearHomeButton);
            //newYearSignButton.onClick.RemoveListener(OnNewYearSignButton);

            //EventManager.Instance.Unregister(EventID.OnRefreshEmail, RefreshEmailRedDot);
            //EventManager.Instance.Unregister(EventID.OnTeamlevelUp, RefreshFuncLock);
            //EventManager.Instance.Unregister(EventID.RefreshBigBangUIRedDot, RefreshTrainRedDot);

            //activetyPageView.OnStartScroll -= OnStartScroll;
            //activetyPageView.OnResetCount -= OnResetActivityCount;

            //challengeBtn.OnClick -= OnChallengeBtn;
            //arenaBtn.OnClick -= OnArenaBtn;
            //leagueBtn.OnClick -= OnLeagueBtn;
            //storyBtn.OnClick -= OnStoryBtn;
            //hundredBtn.OnClick -= OnHundredBtn;
            //trainBtn.OnClick -= OnTrainBtn;
            //stuntBtn.OnClick -= OnStuntBtn;
            //taskBtn.OnClick -= OnTaskBtn;
            //practiceBtn.OnClick -= OnPricticeBtn;
            //nftBtn.OnClick -= OnClickNftBtn;
            //mainTaskBtn.OnClick -= OnMainTaskBtn;
            //workBtn.OnClick -= OnWorkBtn;
            //timeGiftBtn.onClick.RemoveListener(OnTimeGift);

            DevelopBtn.onClick.RemoveListener(OnDevelopBtn);

            homeTopButtonMore.OnClick -= OnClickMore;
            EventManager.Instance?.Unregister(EventID.OnHundredGetMineInfo, OnHundredGetMineInfo);
            SecondUpdateManager.Instance?.UnRegistAction(RefreshArenaInfo);
            SecondUpdateManager.Instance?.UnRegistAction(RefreshStageAndTime);
            EventManager.Instance?.Unregister(EventID.OnArenaGetNewInfo, OnArenaGetNewInfo);
            EventManager.Instance?.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
            jumpGuideButton.OnClick -= OnClickJumpGuideButton;
            normalTimePanelButton.OnClick -= OnClickNormalTimePanelButton;
            allStarPanelNorthButton.OnClick -= OnClickAllStarPanelNorthButton;
            allStarPanelSouthButton.OnClick -= OnClickAllStarPanelSouthButton;
        }

        [SerializeField] private HomeUIAnim homeAnim;
        [SerializeField] private HomeUIGuide homeGuide;
        protected override void OnPropertiesSet()
        {
            AudioManager.Instance.PlayMusic(AudioNames.BGM_HOME);
            SafeInvok(SetScreenFix);
            Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.Unknown;
            SafeInvok(CheckCreatePlayer);
            //guide.CheckGuide();// 检测新手引导
            bool isOpenByBottomTab = Properties != null && Properties.isOpenByBottomTab;
            if (isOpenByBottomTab) Properties.isOpenByBottomTab = false;
            SafeInvok(() => { homeAnim.PlayEnter(isOpenByBottomTab); });
            //RefreshActivityCenter();
            //RefreshBottomBountyTask();
            //GetRecentMatch();
            //RefreshFuncLock();
            SafeInvok(() => { Player.CardManager.SkillController.CheckRedDot(); });
            SafeInvok(() => { Player.CardManager.CheckRedDot(0, true); });
            //SafeInvok(() => { EventManager.Instance.Dispatch(EventID.OnRefreshNavigationUIRedDot); });

            //RefreshEmailRedDot();
            //RefreshAllRedDot();
            SafeInvok(() => { if (!LoginManager.Instance.isDoingSilenceReLogin) homeGuide.CheckGuide(); });

            //timeGiftBtn.gameObject.SetActive(TimeGiftController.Instance.HasTimeGift);
            //festivalGiftBtn.gameObject.SetActive(ActivityController.Instance.IsActivityOpen(ActivityID.NationalDayLogin));
            //christmasButton.gameObject.SetActive(ActivityController.Instance.IsActivityOpen(ActivityID.ChristmasTree));
            //newYearHomeButton.gameObject.SetActive(ActivityController.Instance.IsActivityOpen(ActivityID.NewYearChallenge));
            //newYearSignButton.gameObject.SetActive(ActivityController.Instance.IsActivityOpen(ActivityID.NewYearSign));
            //Player.TimeGiftTrans = timeGiftBtn.transform;//TODO

            SafeInvok(RefreshFirstChargeBtn);
            SafeInvok(RefreshNoviceTaskBtn);
            SafeInvok(() => { ShowMore(false); });
            SafeInvok(MoveButton);

            SafeInvok(RefreshArenaInfo);
            SafeInvok(RefreshHundredInfo);
            SafeInvok(RefreshRecruitInfo);
            SafeInvok(() =>
            {
                PlayoffFinalsGuessManager.Instance.GetCourseData();
            });
            SafeInvok(() =>
            {
                DragonBoatFestivalManager.Instance.GetCourseData();
            });
            SafeInvok(() =>
            {
                if (HundredManager.Instance.nowCourse == null || (DataConvUtil.ServerDateTime - HundredManager.Instance.nowCourseDateTime).Seconds > 600)
                {
                    HundredManager.Instance.GetCourse(0, true);
                }
            });
            SafeInvok(() => { Player.ActivityManager.RefreshOnlineActivity(); });
            SafeInvok(RefreshTopBtnLock);
            SafeInvok(() => { EventManager.Instance.Dispatch(EventID.RefreshUIRedDot); });
            SafeInvok(RefreshJumpGuideButtonState);
        }

        private void SafeInvok(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        //private void GetRecentMatch()
        //{
        //    try
        //    {
        //        Player.PVPManager.GetRecentlyMatch(() =>
        //        {
        //            if (Player.PVPManager.resp != null && Player.PVPManager.resp.SeasonId != 0)
        //            {
        //                txtMatchIndex.text = string.Format("第{0}届", Player.PVPManager.resp.SeasonId);
        //            }
        //            else
        //            {
        //                txtMatchIndex.text = "";
        //            }
        //        });
        //    }
        //    catch
        //    {

        //    }
        //}

        #endregion

        //#if USER_DEBUG
        //        private void Update()
        //        {
        //            SetScreenFix();
        //        }
        //#endif

        #region 主界面适配，当前适配了16:10及以上（常见的16:9-21:9为最佳范围）
        //[SerializeField] private RectTransform homeUITrans;
        //[SerializeField] private RectTransform centerTrans;
        //[SerializeField] private RectTransform activityTrans;
        //[SerializeField] private RectTransform line1Trans;
        //private float centerTransY219 = -10f;
        //private float centerTransY169 = -27f;
        //private float activityTransHeight219 = 344f;
        //private float activityTransHeight169 = 150f;
        //private float activityTransHeightMin = 100f;
        //private float activityTransHeightMax = 344f;
        //private float line1TransHeight219 = 268f;
        //private float line1TransHeight169 = 200f;
        //private float line1TransHeightMin = 130f;
        //private float line1TransHeightMax = 268f;
        //[SerializeField] private HorizontalLayoutGroup rightButtonPanelLayout = null;
        [SerializeField] private RectTransform centerRect = null;
        private void SetScreenFix()
        {
            //#if UNITY_WEBGL
            //            //rightButtonPanelLayout.childAlignment = TextAnchor.UpperLeft;
            //#endif
            //            float hw169 = 16.0f / 9.0f;
            //            float hw219 = 21.0f / 9.0f;
            //            float hwScreen = homeUITrans.rect.height / homeUITrans.rect.width;
            //            float screenT = (hwScreen - hw169) / (hw219 - hw169);
            //            centerTrans.SetLocalPositionY(GetLimitFloat(centerTransY169, centerTransY219, centerTransY169, centerTransY219, screenT));
            //            activityTrans.SetSizeDeltaHeight(GetLimitFloat(activityTransHeightMin, activityTransHeightMax, activityTransHeight169, activityTransHeight219, screenT));
            //            line1Trans.SetSizeDeltaHeight(GetLimitFloat(line1TransHeightMin, line1TransHeightMax, line1TransHeight169, line1TransHeight219, screenT));
            //            LayoutRebuilder.ForceRebuildLayoutImmediate(centerTrans);

            //float fixPosY = 0;
            //if (UICommon.IsBigTop)
            //{
            //    fixPosY = Mathf.Lerp(-163.6f, -47.93079f, UICommon.HomeScreenLerpT);
            //}
            //else
            //{
            //    fixPosY = -131.53f;
            //}
            //centerRect.SetAnchoredPositionY(fixPosY);
        }

        //private float GetLimitFloat(float min, float max, float value169, float value219, float t)
        //{
        //    float targetValue = Utility.Lerp(value169, value219, t);
        //    targetValue = Utility.KeepInRange(targetValue, min, max);
        //    return targetValue;
        //}

        #endregion

        #region 活动

        //[SerializeField] private HomeUICenterItem homeUICenterItem;
        //private void RefreshActivityCenter()
        //{
        //    homeUICenterItem.LoadActivity();
        //    //todo://lh暂时显示，调试完恢复
        //    //timeGiftBtn.gameObject.SetActive(TimeGiftController.Instance.HasGift);
        //}

        //[SerializeField] private HomeActivityTabPoint homeActivityTabPointPrefab;
        //private List<HomeActivityTabPoint> homeActivityTabPointList = new();
        //private void OnResetActivityCount(int count)
        //{
        //    if (homeActivityTabPointList.Count < count)
        //    {
        //        int needPointCount = count - homeActivityTabPointList.Count;
        //        homeActivityTabPointPrefab.gameObject.SetActive(true);
        //        for (int i = 0; i < needPointCount; i++)
        //        {
        //            HomeActivityTabPoint homeActivityTabPoint = GameObject.Instantiate<HomeActivityTabPoint>(homeActivityTabPointPrefab, homeActivityTabPointPrefab.transform.parent);
        //            homeActivityTabPointList.Add(homeActivityTabPoint);
        //        }
        //        homeActivityTabPointPrefab.gameObject.SetActive(false);
        //    }
        //    for (int i = 0; i < homeActivityTabPointList.Count; i++)
        //    {
        //        homeActivityTabPointList[i].gameObject.SetActive(i < count);
        //    }
        //}

        //private void OnStartScroll(int index)
        //{
        //    for (int i = 0; i < homeActivityTabPointList.Count; i++)
        //    {
        //        homeActivityTabPointList[i].SetLight(i == index);
        //    }
        //}

        #endregion

        #region 个人信息
        [SerializeField] private Button surveyBtn;//调查问卷
        private void OnClickSurvey()
        {
            Application.OpenURL(ServerConst.URL_SURVEY);
        }

        #endregion

        #region 顶部按钮

        //[SerializeField] private Button backpackButton; //背包
        //[SerializeField] private Button achievementBtn; //成就
        //[SerializeField] private Button emailBtn; //邮件
        //[SerializeField] private Button activityBtn; //活动
        //[SerializeField] private Button timeGiftBtn; //礼包集合
        //[SerializeField] private Button festivalGiftBtn; //礼包集合
        //[SerializeField] private Button christmasButton = null;//圣诞树
        //[SerializeField] private Button newYearHomeButton = null;//元旦入口
        //[SerializeField] private Button newYearSignButton = null;//跨年入口

        //[SerializeField] private List<Transform> RedDotList; //红点控件

        //private void RefreshAllRedDot()
        //{
        //    //RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVE, "");
        //    //node.IsRed(RedDotList[0]);
        //    //node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FB, "");
        //    //node.IsRed(RedDotList[1]);
        //    //node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicArena, "");
        //    //node.IsRed(RedDotList[2]);
        //    //if (TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicPVP, false))
        //    //{
        //    //    node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVP, "");
        //    //    node.IsRed(RedDotList[3]);
        //    //}

        //    //node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Train, "");
        //    //node.IsRed(RedDotList[4]);
        //    //node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_SkillTrain, "");
        //    //node.IsRed(RedDotList[5]);
        //    //node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Task, "");
        //    //node.IsRed(RedDotList[6]);
        //    ////这里比较特殊，体力小红点是根据当前时间来确定的。
        //    ////没有单独做个定时器来检测时间，所以在首页重新打开的时候来刷新。
        //    //Player.ActivityManager.RefreshChallengeRedDot();
        //    //node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Games, "");
        //    //node.IsRed(RedDotList[7]);
        //    //node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Career, "");
        //    //node.IsRed(RedDotList[8]);
        //    //node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Bag, "");
        //    //node.IsRed(RedDotList[9]);
        //    //node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Activity1, "");
        //    //node.IsRed(RedDotList[10]);
        //    //node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Achieve, "");
        //    //node.IsRed(RedDotList[11]);
        //    //node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Festival, "");
        //    //node.IsRed(RedDotList[12]);

        //    //RefreshChristmasRedDot(null);
        //    //RefreshNewYearRedDot(null);
        //    //RefreshNewYearSignRedDot(null);
        //    //RefreshHundredRedDot(null);
        //}
        //private void RefreshRedDot(object[] args)
        //{
        //    //string path = (string)args[0];
        //    //int index = (int)args[1];
        //    //RedDotNode node;
        //    //node = RedDotManager.Instance.ConfirmNode(path, "");
        //    ////有些没开启的功能，服务端还是推回来了，这里客户端做拦截去屏蔽小红点。
        //    //if (index == 3)
        //    //{
        //    //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicPVP, false))
        //    //    {
        //    //        return;
        //    //    }
        //    //}
        //    //node.IsRed(RedDotList[index]);

        //    //RefreshChristmasRedDot(args);
        //    //RefreshNewYearRedDot(args);
        //    //RefreshNewYearSignRedDot(args);
        //    //RefreshHundredRedDot(args);
        //}

        //private void OnInventoryBtn()
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    UIController.Instance.ShowPanel<InventoryUI>(new InventoryUIProperties(InventoryUI.SubUIID.Inventory));
        //}
        //private void OnAchievementBtn()
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    UIController.Instance.ShowPanel<AchievementUI>();
        //}
        //private void OnEmailBtn()
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    UIController.Instance.ShowPanel<MailBoxUI>();
        //}

        //private void OnActivityBtn()
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties());
        //}

        //private void OnTimeGift()
        //{
        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Activity)) return;
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    UIController.Instance.ShowPanel<TimeGiftCollectionUI>(new TimeGiftCollectionUIProperties());
        //}

        //private void OnFestivalGift()
        //{
        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Activity)) return;
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    UIController.Instance.ShowPanel<ActivityFestivalMainUI>(new ActivityFestivalMainUIProperties());
        //}

        //private void OnChristmasButton()
        //{
        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Activity)) return;
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    UIController.Instance.ShowPanel<ChristmasTreeUI>();
        //}

        //private void OnNewYearHomeButton()
        //{
        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Activity)) return;
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    UIController.Instance.ShowPanel<NewYearHomeUI>();
        //}

        //private void OnNewYearSignButton()
        //{
        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Activity)) return;
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    UIController.Instance.ShowPanel<NewYearSignUI>();
        //}

        #endregion

        #region 中间按钮

        //[SerializeField] private BabuButton challengeBtn;//经典赛
        //[SerializeField] private BabuButton arenaBtn;//排位赛
        //[SerializeField] private BabuButton leagueBtn;//赛事
        //[SerializeField] private BabuButton storyBtn;//剧情
        //[SerializeField] private BabuButton hundredBtn = null;//百分大战
        //[SerializeField] private BabuButton trainBtn;//训练
        //[SerializeField] private BabuButton stuntBtn;//特技
        //[SerializeField] private BabuButton taskBtn;//任务
        //[SerializeField] private BabuButton practiceBtn;//球技练习
        //[SerializeField] private BabuButton nftBtn;//藏品
        //[SerializeField] private BabuButton mainTaskBtn;//生涯
        //[SerializeField] private BabuButton workBtn;//悬赏
        //[SerializeField] private TMP_Text txtMatchIndex;

        //private void OnChallengeBtn(BabuButton sender)
        //{
        //    //if (Player.TrainManager.GetUnlockCount() < 4)
        //    //{
        //    //    Tips.PopTips(LangID.UnlockChallengeTxt);
        //    //    return;
        //    //}
        //    //if (GuideManager.IsFinished(GuideID.Trigger_Challenge_1) && !GuideManager.IsFinished(GuideID.Trigger_Challenge_2))
        //    //{
        //    //    GuideManager.Finish(GuideID.Trigger_Challenge_2);
        //    //}
        //    //FightPoint.PopTips(200);
        //    //return;
        //    //Tips.PopTips("3333333333");
        //    //return;

        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    ClassicManager.Instance.OpenClassicMapUI();
        //}
        //private void OnArenaBtn(BabuButton sender)
        //{
        //    //bool isUnlock = Player.TrainManager.GetUnlockCount() >= 10;
        //    //if (!isUnlock)
        //    //{
        //    //    Tips.PopError(ErrorID.NoTenTrainCanUnclockArena);
        //    //    return;
        //    //}
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);

        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicArena)) return;
        //    UIController.Instance.ShowPanel<ArenaUI>();
        //}
        //private void OnLeagueBtn(BabuButton sender)
        //{
        //    //bool isUnlock = Player.TrainManager.GetUnlockCount() >= 10;
        //    //if (!isUnlock)
        //    //{
        //    //    //解锁十个训练项目后开启赛事
        //    //    Tips.PopError(ErrorID.NoTenTrainCanUnclock);
        //    //    return;
        //    //}

        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicPVP)) return;
        //    UIController.Instance.ShowPanel<MatchHomeUI>(new MatchHomeUIProperites());
        //}
        //private void OnStoryBtn(BabuButton sender)
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicHero)) return;
        //    UIController.Instance.ShowPanel<FBMainUI>();
        //}
        //private void OnHundredBtn(BabuButton sender)
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Hundred)) return;
        //    HundredManager.Instance.OpenHundredHome();
        //}
        //private void OnTrainBtn(BabuButton sender)
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.BigBang)) return;

        //    FsmManager.Instance.ChangeToState<StateTrain>(new StateCommonUserData()
        //    {
        //        OpenUIAction = async () =>
        //        {
        //            await UIController.Instance.ShowPanel<TrainUI>(new TrainUIPanelProperties(TrainUI.SubUIID.Regular));
        //        }
        //    });
        //}
        //private void OnStuntBtn(BabuButton sender)
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.CardSkill)) return;

        //    Player.FightManager.LoginSuccess();
        //    //UIController.Instance.ShowPanel<CardUI>(new CardUIProperties(CardUI.SubUIID.SkillList));
        //    UIController.Instance.ShowPanel<SkillUI>();
        //}
        //private void OnTaskBtn(BabuButton sender)
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Task)) return;

        //    FsmManager.Instance.ChangeToState<StateTask>(new StateCommonUserData()
        //    {
        //        OpenUIAction = async () =>
        //        {
        //            await UIController.Instance.ShowPanel<TaskUI>(new TaskUIProperties(TaskUI.SubUIID.Daily));
        //        }
        //    });
        //}
        //private void OnPricticeBtn(BabuButton sender)
        //{

        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Games)) return;
        //    UIController.Instance.ShowPanel<TinyFunMainUI>();
        //}
        //private void OnClickNftBtn(BabuButton sender)
        //{

        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.BlockChain)) return;
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);

        //    FsmManager.Instance.ChangeToState<StateNft>(new StateCommonUserData()
        //    {
        //        OpenUIAction = async () =>
        //        {
        //            await UIController.Instance.ShowPanel<NFTChinaUI>();
        //        }
        //    });
        //}
        //private void OnMainTaskBtn(BabuButton sender)
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Career)) return;

        //    //if (Player.TaskManager.NormalTasks.Tasks.Count == 0)
        //    //{
        //    //    Tips.PopError(ErrorID.UnlockRequirements);
        //    //    return;
        //    //}
        //    FsmManager.Instance.ChangeToState<StateMainTask>(new StateCommonUserData()
        //    {
        //        OpenUIAction = async () =>
        //        {
        //            await UIController.Instance.ShowPanel<MainTaskUI>();
        //        }
        //    });
        //}
        //private void OnWorkBtn(BabuButton sender)
        //{
        //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Task_Bounty)) return;
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);

        //    FsmManager.Instance.ChangeToState<StateTask>(new StateCommonUserData()
        //    {
        //        OpenUIAction = async () =>
        //        {
        //            await UIController.Instance.ShowPanel<TaskUI>(new TaskUIProperties(TaskUI.SubUIID.Bounty));
        //        }
        //    });
        //}

        #endregion

        #region 悬赏任务

        //[SerializeField] public TMP_Text bountyTaskInfo;
        //[SerializeField] public List<Image> bountyRewardsIconList;

        //private void RefreshBottomBountyTask()
        //{
        //    var taskList = BountyTaskManager.Instance.GetBountyTaskDataList().FindAll(p => p.isLock == false);
        //    var readyTask = taskList.FirstOrDefault(p => p.IsStart == false);
        //    if (taskList.Count == 0 || readyTask == null)
        //    {
        //        bountyTaskInfo.text = "今日悬赏任务已完成";
        //    }
        //    else
        //    {
        //        var rewardsList = taskList.FindAll(p => p.IsStart == true && p.IsFinish == true);
        //        //最多显示3个奖励icon
        //        for (var index = 0; index < 3; index++)
        //        {
        //            if (index + 1 < rewardsList.Count)
        //            {
        //                bountyRewardsIconList[index].gameObject.SetActive(true);
        //            }
        //            else
        //            {
        //                bountyRewardsIconList[index].gameObject.SetActive(false);
        //            }
        //        }
        //        bountyTaskInfo.text = readyTask.bountyTaskConfig.Desc;
        //    }
        //}

        #endregion

        #region 红点

        //[SerializeField] private Image emailRedDot;//邮件红点
        //private void RefreshEmailRedDot(object[] args = null)
        //{
        //    emailRedDot.gameObject.SetActive(Player.EmailManager.HasRedDot);
        //}

        //private void RefreshTrainRedDot(object[] _)
        //{
        //    RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Train, "");
        //    //node.IsRed(RedDotList[4]);
        //}

        //[SerializeField] private Image christmasRedDot = null;
        //private void RefreshChristmasRedDot(object[] _)
        //{
        //    RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Christmas, "");
        //    node.IsRed(christmasRedDot.transform);
        //}

        //[SerializeField] private Image newYearRedDot = null;
        //private void RefreshNewYearRedDot(object[] _)
        //{
        //    RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_NewYear, "");
        //    node.IsRed(newYearRedDot.transform);
        //}

        //[SerializeField] private Image newYearSignRedDot = null;
        //private void RefreshNewYearSignRedDot(object[] _)
        //{
        //    RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_NewYearSign, "");
        //    node.IsRed(newYearSignRedDot.transform);
        //}

        //[SerializeField] private Image hundredRedDot = null;
        //private void RefreshHundredRedDot(object[] _)
        //{
        //    RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Hundred, "");
        //    node.IsRed(hundredRedDot.transform);
        //}
        private void RefreshRedDot(object[] args)
        {
            //string path = (string)args[0];
            //int index = (int)args[1];
            //RedDotNode node;
            //node = RedDotManager.Instance.ConfirmNode(path, "");
            ////有些没开启的功能，服务端还是推回来了，这里客户端做拦截去屏蔽小红点。
            //if (index == 3)
            //{
            //    if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicPVP, false))
            //    {
            //        return;
            //    }
            //}
            //node.IsRed(RedDotList[index]);

            //RefreshChristmasRedDot(args);
            //RefreshNewYearRedDot(args);
            //RefreshNewYearSignRedDot(args);
            //RefreshHundredRedDot(args);

            RefreshRecruitRedDot();
            RefreshWelfareRedDot();
            RefreshMoreRedDot();
            RefreshShopRedDot();
        }
        private bool IsActivityHasRedDot(List<ActivityClientType> wantShowTypeList)
        {


            List<ActivityToggleData> toggleDataList = ActivityController.Instance.GetActivityToggleDataList(wantShowTypeList);
            foreach (ActivityToggleData activityToggleData in toggleDataList)
            {
                if (ActivityController.Instance.HasRedDot(activityToggleData.activityConfig.Id)) return true;
            }
            return false;
        }
        #region 福利红点
        [SerializeField] private Image welfareDotNodeImg = null;
        private void RefreshWelfareRedDot()
        {
            bool isRed = IsActivityHasRedDot(new() { ActivityClientType.Sign7Day, ActivityClientType.Sign30Day, ActivityClientType.EnergyCenter });
            welfareDotNodeImg.gameObject.SetActive(isRed);
        }
        #endregion

        #region 更多红点
        [SerializeField] private Image moreDotNodeImg = null;
        private void RefreshMoreRedDot()
        {
            bool isRed = false;

            foreach (HomeTopButton homeTopButton in rightButtonPanelLayout2.GetChildren<HomeTopButton>())
            {
                homeTopButton.RefreshRedDot(null);
                isRed |= homeTopButton.dotNodeImg.gameObject.activeSelf;
            }

            moreDotNodeImg.gameObject.SetActive(isRed);
        }
        #endregion

        #region 商店红点
        [SerializeField] private Image shopDotNodeImg = null;
        private void RefreshShopRedDot()
        {
            bool isRed = false;
            bool isMonthCardRed = Player.ActivityManager.GetIsMonthCardRedDot();
            isRed = isMonthCardRed;
            shopDotNodeImg.gameObject.SetActive(isRed);
        }
        #endregion

        #endregion

        #region 引导

        //[SerializeField] private HomeUIGuide guide;//引导组件

        [SerializeField] private Image whiteBg;//新建角色进来后白光消失图片
        private void CheckCreatePlayer()
        {
            whiteBg.gameObject.SetActive(CreatePlayerUI.IsCreate);
            if (CreatePlayerUI.IsCreate)
            {
                CreatePlayerUI.IsCreate = false;
                whiteBg.color = new Color(1, 1, 1, 1);
                whiteBg.DOFade(0, 0.5f).OnComplete(() =>
                {
                    whiteBg.gameObject.SetActive(false);
                });
            }
        }

        #endregion

        #region 功能锁定


        //[SerializeField] private Color funcLockPageColor;
        //[SerializeField] private Color funcUnLockPageColor;

        //[SerializeField] private Image pageImageHero = null;
        //[SerializeField] private TMP_Text lockTextHero = null;
        //[SerializeField] private Image pageImageArena = null;
        //[SerializeField] private TMP_Text lockTextArena = null;
        //[SerializeField] private Image pageImagePvp = null;
        //[SerializeField] private TMP_Text lockTextPvp = null;
        //[SerializeField] private Image pageImageTrain = null;
        //[SerializeField] private TMP_Text lockTextTrain = null;
        //[SerializeField] private Image pageImageStunt = null;
        //[SerializeField] private TMP_Text lockTextStunt = null;
        //[SerializeField] private Image pageImageTask = null;
        //[SerializeField] private TMP_Text lockTextTask = null;
        //[SerializeField] private Image pageImageBounty = null;
        //[SerializeField] private TMP_Text lockTextBounty = null;
        //[SerializeField] private Image pageImageHundred = null;
        //[SerializeField] private TMP_Text lockTextHundred = null;

        //private void RefreshFuncLock(object[] _)
        //{
        //    RefreshFuncLock();
        //}

        //private void RefreshFuncLock()
        //{
        //    RefreshFuncLock(pageImageHundred, lockTextHundred, TriggerModuleType.Hundred);
        //    RefreshFuncLock(pageImageHero, lockTextHero, TriggerModuleType.ClassicHero);
        //    RefreshFuncLock(pageImageArena, lockTextArena, TriggerModuleType.ClassicArena);
        //    RefreshFuncLock(pageImagePvp, lockTextPvp, TriggerModuleType.ClassicPVP);
        //    RefreshFuncLock(pageImageTrain, lockTextTrain, TriggerModuleType.BigBang);
        //    RefreshFuncLock(pageImageStunt, lockTextStunt, TriggerModuleType.CardSkill);
        //    RefreshFuncLock(pageImageTask, lockTextTask, TriggerModuleType.Task);
        //    RefreshFuncLock(pageImageBounty, lockTextBounty, TriggerModuleType.Task_Bounty);
        //    bool isOpenBounty = TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Task_Bounty, false);
        //    if (!isOpenBounty) bountyTaskInfo.text = "悬赏任务";
        //}
        //private void RefreshFuncLock(Image pageImage, TMP_Text lockText, TriggerModuleType triggerModuleType)
        //{
        //    bool isOpen = TriggerManager.Instance.CheckModuleOpen(triggerModuleType, false);
        //    pageImage.color = isOpen ? funcUnLockPageColor : funcLockPageColor;
        //    lockText.gameObject.SetActive(!isOpen);
        //    if (!isOpen) lockText.text = TriggerManager.Instance.GetShortLockTipStr(triggerModuleType);
        //}

        #endregion

        #region 调试

        [SerializeField] private Button DevelopBtn;

        private void OnDevelopBtn()
        {
#if UNITY_EDITOR
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            UIController.Instance.OpenWindow<DevelopUI>();
#endif
        }

        #endregion

        #region 顶部按钮

        [SerializeField] private RectTransform leftButtonPanelLayout = null;
        [SerializeField] private RectTransform rightButtonPanelLayout = null;
        [SerializeField] private RectTransform rightButtonPanelLayout2 = null;
        [SerializeField] private HomeTopButton homeTopButtonMore = null;
        private void OnClickMore()
        {
            ShowMore(!rightButtonPanelLayout2.gameObject.activeSelf);
        }
        private void ShowMore(bool isShow)
        {
            rightButtonPanelLayout2.gameObject.SetActive(isShow);
        }

        private void RefreshTopBtnLock()
        {
            List<HomeTopButton> allTopBtnList = new();
            allTopBtnList.AddRange(leftButtonPanelLayout.GetChildren<HomeTopButton>());
            allTopBtnList.AddRange(rightButtonPanelLayout.GetChildren<HomeTopButton>());
            allTopBtnList.AddRange(rightButtonPanelLayout2.GetChildren<HomeTopButton>());
            foreach (var item in allTopBtnList)
            {
                item.RefreshLock(null);
            }
        }

        private void MoveButton()
        {
            List<HomeTopButton> canMoveList = new();
            canMoveList.AddRange(rightButtonPanelLayout.GetChildren<HomeTopButton>().Where(item => !item.doNotMove).ToList());
            canMoveList.AddRange(rightButtonPanelLayout2.GetChildren<HomeTopButton>().Where(item => !item.doNotMove).ToList());
            foreach (var item in canMoveList)
            {
                item.transform.SetParent(rightButtonPanelLayout.parent);
                item.RefreshLock(null);
            }
            int maxTop = 6;
            int nowTop = 0;
            for (int i = 0; i < canMoveList.Count; i++)
            {
                HomeTopButton homeTopButton = canMoveList[i];
                if (nowTop < maxTop)
                {
                    homeTopButton.transform.SetParent(rightButtonPanelLayout);
                    if (homeTopButton.gameObject.activeSelf == true)
                    {
                        nowTop++;
                    }
                }
                else
                {
                    homeTopButton.transform.SetParent(rightButtonPanelLayout2);
                }
            }
            homeTopButtonMore.transform.SetAsLastSibling();
        }

        [SerializeField] private BabuButton homeTopButtonFirstCharge = null;
        private void RefreshFirstChargeBtn()
        {
            bool isActivityOpen = ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.FirstPay);
            if (!isActivityOpen)
            {
                homeTopButtonFirstCharge.gameObject.SetActive(false);
                return;
            }
            ActivityData activityData = ActivityController.Instance.OnlineActivityDic[ActivityID.FirstPay];
            List<ActivityPayRewardConfig> tmp = Configs.ActivityPayReward.GetConfigList().FindAll(p => p.ActivityId == activityData.cfg.Id);
            if (tmp.Count == activityData.payData.ReceiveSet.Count)
            {
                //奖励领完了，不需要首页再展示
                homeTopButtonFirstCharge.gameObject.SetActive(false);
                return;
            }
            homeTopButtonFirstCharge.gameObject.SetActive(true);
        }

        [SerializeField] private BabuButton homeTopButtonNoviceTask = null;
        private void RefreshNoviceTaskBtn()
        {
            homeTopButtonNoviceTask.gameObject.SetActive(Player.NoviceTaskManager.IsOpen);
        }

        #endregion

        #region 竞技场信息

        private void OnArenaGetNewInfo(object[] _)
        {
            RefreshArenaInfo();
        }

        private readonly string ArenaSStageStr = "第<color=#FFE50B>{0}</color>名 重置倒计时 {1}";
        private readonly string ArenaOtherStageStr = "重置倒计时 {0}";
        [SerializeField] private Image arenaStageTipBgImage = null;
        [SerializeField] private TMP_Text arenaStageTipText = null;
        private void RefreshArenaInfo()
        {
            bool isOpen = TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicArena, false);
            if (!isOpen || Player.BattleManager.newArenaInfo == null)
            {
                arenaStageTipBgImage.gameObject.SetActive(false);
                arenaStageTipText.text = "";
                return;
            }
            int leftTime = (int)(Player.BattleManager.newArenaInfo.EndTime - Utils.DataConvUtil.ServerTime);
            if (leftTime <= 0)
            {
                arenaStageTipBgImage.gameObject.SetActive(false);
                arenaStageTipText.text = "";
                return;
            }

            int nowSatge = Player.BattleManager.newArenaInfo.ArenaStage;
            bool isStageS = nowSatge == 9;
            bool isTimeBiggerThenOneHour = leftTime >= 3600;
            string timeStr = isTimeBiggerThenOneHour ? TimeUtils.FormatLeftTimeWithDayCn(leftTime) : TimeUtils.FormatLeftTime(leftTime);
            if (isStageS)
            {
                arenaStageTipBgImage.gameObject.SetActive(true);
                arenaStageTipText.text = ArenaSStageStr.SafeFormat(Player.BattleManager.newArenaInfo.ArenaRank, timeStr);
            }
            else
            {
                arenaStageTipBgImage.gameObject.SetActive(true);
                arenaStageTipText.text = ArenaOtherStageStr.SafeFormat(timeStr);
            }
        }

        #endregion

        #region 百分大战信息

        [SerializeField] private Image hundredTipImage = null;
        [SerializeField] private TMP_Text hundredTipText = null;

        [SerializeField] private HorizontalLayoutGroup hundredStageTimeLayout = null;
        [SerializeField] private TMP_Text hundredStageTipText = null;
        [SerializeField] private TMP_Text hundredTimeTipText = null;

        [SerializeField] private HorizontalLayoutGroup winLoseLayout = null;
        [SerializeField] private List<HundredFight1WinLoseItem> hundredFight1WinLoseItemList = null;

        private void OnHundredGetMineInfo(object[] _)
        {
            RefreshHundredInfo();
        }

        //private readonly string WaitStr = "休赛期";
        private readonly string SignStrNew = "已报名新星赛区";
        private readonly string SignStrNormal = "已报名第{0}赛区";
        private readonly string NotSignStr = "尚未报名";
        //private readonly string Fight1Str = "入围赛";//使用圆圈
        private readonly string Fight1WinStr = "已晋级";
        private readonly string Fight1LoseStr = "未能晋级";
        private readonly string Fight2Str = "成绩:<color=#FF9A09>赛区</color><color=#FFE50B>{0}</color>";
        private readonly string Fight3Str = "成绩:<color=#FF9A09>决赛</color><color=#FFE50B>{0}</color>";

        private void RefreshHundredInfo()
        {
            bool isOpen = TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicArena, false);
            if (!isOpen || HundredManager.Instance.nowCourse == null || HundredManager.Instance.nowCourse.Stage == 0)
            {
                HideHundredInfo();
                return;
            }
            int leftTime = HundredManager.Instance.nowCourse.StageEndTime - (int)Utils.DataConvUtil.ServerTime;
            if (leftTime <= 0)
            {
                HideHundredInfo();
                return;
            }
            RefreshStageAndTime();
            HundredProgress hundredProgress = (HundredProgress)HundredManager.Instance.nowCourse.Stage;
            switch (hundredProgress)
            {
                case HundredProgress.Wait:
                    {
                        hundredTipImage.gameObject.SetActive(false);
                        winLoseLayout.gameObject.SetActive(false);
                    }
                    break;
                case HundredProgress.Sign:
                    {
                        hundredTipImage.gameObject.SetActive(true);
                        winLoseLayout.gameObject.SetActive(false);
                        if (HundredManager.Instance.nowCourse.MyZoneId > 0)
                        {
                            if (HundredManager.Instance.nowCourse.MyZoneId == 9)
                            {
                                hundredTipText.text = SignStrNew;
                            }
                            else
                            {
                                hundredTipText.text = SignStrNormal.SafeFormat(HundredManager.Instance.nowCourse.MyZoneId.ToChinese());
                            }
                        }
                        else
                        {
                            hundredTipText.text = NotSignStr;
                        }
                    }
                    break;
                case HundredProgress.Fight1:
                    {
                        if (HundredManager.Instance.isMeInNowCourse() == false)
                        {
                            hundredTipImage.gameObject.SetActive(false);
                            winLoseLayout.gameObject.SetActive(false);
                            break;
                        }
                        HundredManager.Instance.GetFight1EndAndWin(HundredManager.Instance.nowCourse, out bool isEnd, out bool isDown);
                        hundredTipImage.gameObject.SetActive(isEnd);
                        winLoseLayout.gameObject.SetActive(!isEnd);
                        if (isEnd)
                        {
                            hundredTipText.text = isDown ? Fight1LoseStr : Fight1WinStr;
                        }
                        else
                        {
                            RefreshWinLose();
                        }
                    }
                    break;
                case HundredProgress.Fight2:
                    {
                        if (HundredManager.Instance.isMeInNowCourse() == false)
                        {
                            hundredTipImage.gameObject.SetActive(false);
                            winLoseLayout.gameObject.SetActive(false);
                            break;
                        }
                        string str = HundredManager.Instance.GetRoundPlayerTitle(true, HundredManager.Instance.GetMyMaxRound(true));
                        if (string.IsNullOrWhiteSpace(str))
                        {
                            hundredTipImage.gameObject.SetActive(false);
                            winLoseLayout.gameObject.SetActive(false);
                            break;
                        }
                        hundredTipImage.gameObject.SetActive(true);
                        winLoseLayout.gameObject.SetActive(false);
                        hundredTipText.text = Fight2Str.SafeFormat(str);
                    }
                    break;
                case HundredProgress.Fight3:
                    {
                        if (HundredManager.Instance.isMeInNowCourse() == false)
                        {
                            hundredTipImage.gameObject.SetActive(false);
                            winLoseLayout.gameObject.SetActive(false);
                            break;
                        }
                        string str = HundredManager.Instance.GetRoundPlayerTitle(false, HundredManager.Instance.GetMyMaxRound(false));
                        if (string.IsNullOrWhiteSpace(str))
                        {
                            hundredTipImage.gameObject.SetActive(false);
                            winLoseLayout.gameObject.SetActive(false);
                            break;
                        }
                        hundredTipImage.gameObject.SetActive(true);
                        winLoseLayout.gameObject.SetActive(false);
                        hundredTipText.text = Fight3Str.SafeFormat(str);
                    }
                    break;
            }
        }
        [SerializeField] private Image hundredStageTimeBgImage = null;
        private void RefreshStageAndTime()
        {
            bool isOpen = TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicArena, false);
            if (!isOpen || HundredManager.Instance.nowCourse == null || HundredManager.Instance.nowCourse.Stage == 0)
            {
                hundredStageTimeBgImage.gameObject.SetActive(false);
                hundredStageTimeLayout.gameObject.SetActive(false);
                return;
            }
            int leftTime = HundredManager.Instance.nowCourse.StageEndTime - (int)Utils.DataConvUtil.ServerTime;
            if (leftTime < 0)
            {
                hundredStageTimeBgImage.gameObject.SetActive(false);
                hundredStageTimeLayout.gameObject.SetActive(false);
                return;
            }
            bool isTimeBiggerThenOneHour = leftTime >= 3600;
            string timeStr = isTimeBiggerThenOneHour ? TimeUtils.FormatLeftTimeWithDayCn(leftTime) : TimeUtils.FormatLeftTime(leftTime);
            HundredProgress hundredProgress = (HundredProgress)HundredManager.Instance.nowCourse.Stage;
            hundredStageTimeBgImage.gameObject.SetActive(true);
            hundredStageTimeLayout.gameObject.SetActive(true);
            hundredStageTipText.text = HundredManager.Instance.GetStageName(hundredProgress);
            hundredTimeTipText.text = timeStr;
            LayoutRebuilder.ForceRebuildLayoutImmediate(hundredStageTipText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(hundredTimeTipText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(hundredStageTimeLayout.transform as RectTransform);
        }
        private void RefreshWinLose()
        {
            List<LeagueCourseItemData> fightDataList = new();
            foreach (LeagueCourseItemData leagueCourseItemData in HundredManager.Instance.nowCourse.LeagueCourseItemList)
            {
                if (leagueCourseItemData.HomeTeam == null || leagueCourseItemData.AwayTeam == null) continue;
                if (leagueCourseItemData.HomeTeam.TeamId == Player.GbId || leagueCourseItemData.AwayTeam.TeamId == Player.GbId)
                {
                    if (leagueCourseItemData.HomeGoal > -1 && leagueCourseItemData.AwayGoal > -1)
                        fightDataList.Add(leagueCourseItemData);
                }
            }
            fightDataList.Reverse();
            for (int i = 0; i < 5; i++)
            {
                HundredFight1WinLoseItem hundredFight1WinLoseItem = hundredFight1WinLoseItemList[i];
                bool hasData = i < fightDataList.Count;
                if (hasData)
                {
                    LeagueCourseItemData leagueCourseItemData = fightDataList[i];
                    bool isWin = HundredManager.Instance.IsFightWin(leagueCourseItemData);
                    hundredFight1WinLoseItem.winImage.gameObject.SetActive(isWin);
                    hundredFight1WinLoseItem.loseImage.gameObject.SetActive(!isWin);
                    hundredFight1WinLoseItem.emptyImage.gameObject.SetActive(false);
                }
                else
                {
                    hundredFight1WinLoseItem.winImage.gameObject.SetActive(false);
                    hundredFight1WinLoseItem.loseImage.gameObject.SetActive(false);
                    hundredFight1WinLoseItem.emptyImage.gameObject.SetActive(false);
                }
            }
        }
        private void HideHundredInfo()
        {
            hundredTipImage.gameObject.SetActive(false);
            hundredStageTimeBgImage.gameObject.SetActive(false);
            hundredStageTimeLayout.gameObject.SetActive(false);
            winLoseLayout.gameObject.SetActive(false);
        }

        #endregion

        #region 招募信息

        [SerializeField] private List<Image> recruitDotNodeImgList = new();
        private void RefreshRecruitRedDot()
        {
            bool isTimeRed = false;
            ActivityData activityData = ActivityController.Instance.FindTimeRecruitActivity;
            if (activityData != null)
            {
                RedDotNode timeNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + activityData.cfg.Id);
                isTimeRed |= timeNode.IsRed(null);
                RedDotNode timeNodeRec = RedDotManager.Instance.ConfirmNode(PanelNodePath.Recruit, "/" + activityData.cfg.Param1);
                isTimeRed |= timeNodeRec.IsRed(null);
            }
            else
            {
                isTimeRed = false;
            }

            bool isNormalRed = false;
            RedDotNode normalNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Recruit, "/1");
            isNormalRed = normalNode.IsRed(null);

            bool isAllStarRed = false;
            ActivityData allStarTimeRecruitActivityDataNorth = ActivityController.Instance.FindAllStar2024NorthTimeRecruit;
            ActivityData allStarTimeRecruitActivityDataSouth = ActivityController.Instance.FindAllStar2024SouthTimeRecruit;
            bool isAllStarOpen = allStarTimeRecruitActivityDataNorth != null && allStarTimeRecruitActivityDataSouth != null;
            if (isAllStarOpen)
            {
                RedDotNode allStarNorthNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + allStarTimeRecruitActivityDataNorth.cfg.Id);
                isAllStarRed |= allStarNorthNode.IsRed(null);
                RedDotNode allStarSouthNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + allStarTimeRecruitActivityDataSouth.cfg.Id);
                isAllStarRed |= allStarSouthNode.IsRed(null);
                RedDotNode allStarNorthNodeRec = RedDotManager.Instance.ConfirmNode(PanelNodePath.Recruit, "/" + allStarTimeRecruitActivityDataNorth.cfg.Param1);
                isAllStarRed |= allStarNorthNodeRec.IsRed(null);
                RedDotNode allStarSouthNodeRec = RedDotManager.Instance.ConfirmNode(PanelNodePath.Recruit, "/" + allStarTimeRecruitActivityDataSouth.cfg.Param1);
                isAllStarRed |= allStarSouthNodeRec.IsRed(null);
            }

            bool isRed = isTimeRed || isNormalRed || isAllStarRed;
            foreach (var item in recruitDotNodeImgList)
            {
                item.gameObject.SetActive(isRed);
            }
        }

        [SerializeField] private RectTransform normalTimePanel = null;
        [SerializeField] private RectTransform allStarPageView = null;

        [SerializeField] private Image recruitBgImageNormal = null;
        [SerializeField] private Image recruitBgImageTime = null;
        [SerializeField] private Image recruitFgImage = null;
        [SerializeField] private Image recruitNameBgImage = null;
        [SerializeField] private Image recruitUpBgImage = null;
        [SerializeField] private TMP_Text recruitUpNameText = null;
        [SerializeField] private Image recruitBorderImageNormal = null;
        [SerializeField] private Image recruitBorderImageTime = null;

        [SerializeField] private Image recruitFgImageNorth = null;
        [SerializeField] private TMP_Text recruitUpNameTextNorth = null;
        [SerializeField] private Image recruitFgImageSouth = null;
        [SerializeField] private TMP_Text recruitUpNameTextSouth = null;

        private async void RefreshRecruitInfo()
        {
            ActivityData allStarTimeRecruitActivityDataNorth = ActivityController.Instance.FindAllStar2024NorthTimeRecruit;
            ActivityData allStarTimeRecruitActivityDataSouth = ActivityController.Instance.FindAllStar2024SouthTimeRecruit;
            bool isAllStarOpen = allStarTimeRecruitActivityDataNorth != null && allStarTimeRecruitActivityDataSouth != null;


            bool isModuleOpen = TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Recruit, false);
            ActivityData activityDataTime = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.TimeRecruit);
            bool isHasActivityTime = activityDataTime != null;
            bool isInGuide = GuideManager.IsStarterGuide;
            bool isNormal = (!isHasActivityTime && !isAllStarOpen) || !isModuleOpen || isInGuide;

            if (isNormal)
            {
                normalTimePanel.gameObject.SetActive(true);
                allStarPageView.gameObject.SetActive(false);

                recruitBgImageNormal.gameObject.SetActive(isNormal);
                recruitBgImageTime.gameObject.SetActive(!isNormal);
                recruitFgImage.gameObject.SetActive(!isNormal);
                recruitNameBgImage.gameObject.SetActive(!isNormal);
                recruitUpBgImage.gameObject.SetActive(!isNormal);
                recruitBorderImageNormal.gameObject.SetActive(isNormal);
                recruitBorderImageTime.gameObject.SetActive(!isNormal);
            }
            else
            {
                if (isAllStarOpen)
                {
                    normalTimePanel.gameObject.SetActive(false);
                    allStarPageView.gameObject.SetActive(true);

                    {
                        int cardId = CardId.ZhaoRui;
                        int.TryParse(allStarTimeRecruitActivityDataNorth.cfg.Param2, out cardId);
                        CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(cardId);
                        if (cardModelConfig == null)
                        {
                            Debug.LogWarningFormat("HomeUI , RefreshRecruitInfo , cardModelConfig is null , allStarTimeRecruitActivityDataNorth.cfg.Id = {0} , cardId = {1}", allStarTimeRecruitActivityDataNorth.cfg.Id, cardId);
                            return;
                        }
                        recruitUpNameTextNorth.text = cardModelConfig.Name;
                        recruitFgImageNorth.sprite = await SpriteProxy.GetActivityRecruitHomeSprite(cardModelConfig.Id.ToString());
                    }

                    {
                        int cardId = CardId.HuMingXuan;
                        int.TryParse(allStarTimeRecruitActivityDataSouth.cfg.Param2, out cardId);
                        CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(cardId);
                        if (cardModelConfig == null)
                        {
                            Debug.LogWarningFormat("HomeUI , RefreshRecruitInfo , cardModelConfig is null , allStarTimeRecruitActivityDataSouth.cfg.Id = {0} , cardId = {1}", allStarTimeRecruitActivityDataSouth.cfg.Id, cardId);
                            return;
                        }
                        recruitUpNameTextSouth.text = cardModelConfig.Name;
                        recruitFgImageSouth.sprite = await SpriteProxy.GetActivityRecruitHomeSprite(cardModelConfig.Id.ToString());
                    }
                }
                else
                {
                    normalTimePanel.gameObject.SetActive(true);
                    allStarPageView.gameObject.SetActive(false);

                    recruitBgImageNormal.gameObject.SetActive(isNormal);
                    recruitBgImageTime.gameObject.SetActive(!isNormal);
                    recruitFgImage.gameObject.SetActive(!isNormal);
                    recruitNameBgImage.gameObject.SetActive(!isNormal);
                    recruitUpBgImage.gameObject.SetActive(!isNormal);
                    recruitBorderImageNormal.gameObject.SetActive(isNormal);
                    recruitBorderImageTime.gameObject.SetActive(!isNormal);

                    int cardId = 104004;
                    int.TryParse(activityDataTime.cfg.Param2, out cardId);
                    CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(cardId);
                    if (cardModelConfig == null)
                    {
                        Debug.LogWarningFormat("HomeUI , RefreshRecruitInfo , cardModelConfig is null , activityData.cfg.Id = {0} , cardId = {1}", activityDataTime.cfg.Id, cardId);
                        return;
                    }
                    recruitUpNameText.text = cardModelConfig.Name;
                    recruitFgImage.sprite = await SpriteProxy.GetActivityRecruitHomeSprite(cardModelConfig.Id.ToString());
                }
            }
        }

        [SerializeField] private BabuButton normalTimePanelButton = null;
        [SerializeField] private BabuButton allStarPanelNorthButton = null;
        [SerializeField] private BabuButton allStarPanelSouthButton = null;

        private void OnClickNormalTimePanelButton(BabuButton _)
        {
            bool isModuleOpen = TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Recruit, true);
            if (!isModuleOpen) return;
            TriggerManager.Instance.JumpPanel(TriggerModuleType.Recruit);
        }
        private void OnClickAllStarPanelNorthButton(BabuButton _)
        {
            bool isModuleOpen = TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Recruit, true);
            if (!isModuleOpen) return;
            TriggerManager.Instance.JumpPanel((int)TriggerModuleType.Recruit_AllStar, false, (int)AllStarManager.Area.North);
        }
        private void OnClickAllStarPanelSouthButton(BabuButton _)
        {
            bool isModuleOpen = TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Recruit, true);
            if (!isModuleOpen) return;
            TriggerManager.Instance.JumpPanel((int)TriggerModuleType.Recruit_AllStar, false, (int)AllStarManager.Area.South);
        }

        #endregion

        #region 跳过引导

        [SerializeField] private BabuButton jumpGuideButton = null;
        private void RefreshJumpGuideButtonState()
        {
#if !UNITY_EDITOR
            jumpGuideButton.gameObject.SetActive(false);
            return;
#endif
            jumpGuideButton.gameObject.SetActive(false);
            //jumpGuideButton.gameObject.SetActive(GuideManager.InForceGuide);
        }
        private void OnClickJumpGuideButton(BabuButton _)
        {
            GuideManager.Finish(GuideID.starterGuide);
            homeGuide.enabled = false;
            RefreshJumpGuideButtonState();
        }

        #endregion

    }
}
