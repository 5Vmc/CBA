using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang;
using BigBang.Animation;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using static BigBang.AllStarManager;
using static BigBang.SpriteNames;

public class AllStarHomePad : MonoBehaviour, IActivity
{
    #region 初始化
    [SerializeField] private BabuButton helpButton = null;
    [SerializeField] private BabuButton giftButton = null;
    [SerializeField] private BabuButton rankButton = null;
    [SerializeField] private BabuButton recruitButton = null;
    [SerializeField] private BabuButton northPlayerButton = null;
    [SerializeField] private BabuButton southPlayerButton = null;
    [SerializeField] private BabuButton northSupportButton = null;
    [SerializeField] private BabuButton southSupportButton = null;
    [SerializeField] private BabuButton formationButton = null;
    [SerializeField] private BabuButton refreshButton = null;
    [SerializeField] private TMP_Text endTipText = null;
    [SerializeField] private AllStarHomeProgressItem progressItem = null;
    [SerializeField] private Image northHideSignImage = null;
    [SerializeField] private Image southHideSignImage = null;
    [SerializeField] private ImageFont leftImageFont = null;
    [SerializeField] private ImageFont rightImageFont = null;

    private void OnEnable()
    {
        helpButton.OnClick += OnClickHelpButton;
        giftButton.OnClick += OnClickGiftButton;
        rankButton.OnClick += OnClickRankButton;
        recruitButton.OnClick += OnClickRecruitButton;
        northPlayerButton.OnClick += OnClickNorthPlayerButton;
        southPlayerButton.OnClick += OnClickSouthPlayerButton;
        northSupportButton.OnClick += OnClickNorthSupportButton;
        southSupportButton.OnClick += OnClickSouthSupportButton;
        formationButton.OnClick += OnClickFormationButton;
        refreshButton.OnClick += OnClickRefreshButton;
        dailyRewardButton.OnClick += OnClickDailyRewardButton;
        EventManager.Instance.Register(EventID.RefreshAllStarHomePad, RefreshAllStarHomePad);
        SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
    }

    private void OnDisable()
    {
        helpButton.OnClick -= OnClickHelpButton;
        giftButton.OnClick -= OnClickGiftButton;
        rankButton.OnClick -= OnClickRankButton;
        recruitButton.OnClick -= OnClickRecruitButton;
        northPlayerButton.OnClick -= OnClickNorthPlayerButton;
        southPlayerButton.OnClick -= OnClickSouthPlayerButton;
        northSupportButton.OnClick -= OnClickNorthSupportButton;
        southSupportButton.OnClick -= OnClickSouthSupportButton;
        formationButton.OnClick -= OnClickFormationButton;
        refreshButton.OnClick -= OnClickRefreshButton;
        dailyRewardButton.OnClick -= OnClickDailyRewardButton;
        EventManager.Instance.Unregister(EventID.RefreshAllStarHomePad, RefreshAllStarHomePad);
        SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
    }
    private ActivityData activityData = null;
    public void LoadActivity(ActivityData _data)
    {
        activityData = _data;
        HideAll();
        AllStarManager.Instance.GetServerData(() =>
        {
            RefreshUI();
            if (AllStarManager.Instance.IsNeedShowEnd)
            {
                UIController.Instance.OpenWindow<AllStarEndUI>();
            }
        });
    }
    private void RefreshAllStarHomePad(object[] _)
    {
        HideAll();
        RefreshUI();
    }
    #endregion

    #region 按钮回调

    private void OnClickHelpButton(BabuButton button)
    {
        UIController.Instance.OpenWindow<AllStarHelpUI>();
    }
    private void OnClickGiftButton(BabuButton button)
    {
        switch (stage)
        {
            case Stage.NotOpen:
                Tips.PopTips("活动未开启");
                break;
            case Stage.CanSign:
                Tips.PopTips("请先选择阵营");
                break;
            case Stage.Playing:
            case Stage.Ending:
                UIController.Instance.ShowPanel<AllStarCombatRewardUI>();
                break;
            case Stage.Closed:
                Tips.PopTips("活动已结束");
                break;
            default:
                break;
        }
    }
    private void OnClickRankButton(BabuButton button)
    {
        UIController.Instance.ShowPanel<AllStarRankUI>();
    }
    private void OnClickRecruitButton(BabuButton button)
    {
        Area area = Area.North;
        if (AllStarManager.Instance.serverData != null && AllStarManager.Instance.serverData.Area != 0)
        {
            area = (Area)AllStarManager.Instance.serverData.Area;
        }
        else
        {
            area = Utility.GetRandomBool() ? Area.North : Area.South;
        }
        UIController.Instance.ShowPanel<RecruitUI>(new RecruitUIProperties(RecruitUI.SubUIID.AllStar, area));
    }
    private void OnClickNorthPlayerButton(BabuButton button)
    {
        UIController.Instance.ShowPanel<AllStarPosterUI>(new AllStarPosterUIProperties(Area.North));
    }
    private void OnClickSouthPlayerButton(BabuButton button)
    {
        UIController.Instance.ShowPanel<AllStarPosterUI>(new AllStarPosterUIProperties(Area.South));
    }
    private void OnClickNorthSupportButton(BabuButton button)
    {
        UIController.Instance.OpenWindow<AllStarSelectConfirmUI>(new AllStarSelectConfirmUIProperties(Area.North));
    }
    private void OnClickSouthSupportButton(BabuButton button)
    {
        UIController.Instance.OpenWindow<AllStarSelectConfirmUI>(new AllStarSelectConfirmUIProperties(Area.South));
    }
    private void OnClickFormationButton(BabuButton button)
    {
        UIController.Instance.ShowPanel<AllStarFormationUI>();
    }
    private void OnClickRefreshButton(BabuButton button)
    {
        AllStarManager.Instance.SyncAllStarData();
    }
    private void OnClickDailyRewardButton(BabuButton button)
    {
        switch (stage)
        {
            case Stage.NotOpen:
                Tips.PopTips("活动未开启");
                return;
            case Stage.CanSign:
                Tips.PopTips("请先选择阵营");
                return;
            case Stage.Playing:
            case Stage.Ending:
                break;
            case Stage.Closed:
                Tips.PopTips("活动已结束");
                return;
            default:
                break;
        }
        bool hasGet = ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(activityData.cfg.Id);
        if (hasGet)
        {
            Tips.PopTips("今日奖励已领取，请明日再来");
        }
        else
        {
            NetworkManager.Instance.ReceiveDailyGift(activityData.cfg.Id, (resp) =>
            {
                if (resp.ReceiveSucceed == false)
                {
                    Tips.PopTips("领取失败");
                    Debug.LogWarningFormat("AllStarHomePad , OnClickDailyRewardButton , resp.ReceiveSucceed == false , activityData.cfg.Id = {0}", activityData.cfg.Id);
                    return;
                }
                if (ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(activityData.cfg.Id) == false)
                    ActivityController.Instance.dailyGiftReceivedActivityIdSet.Add(activityData.cfg.Id);
                var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(activityData.cfg.DailyGift).ToList());
                UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                ActivityController.Instance.RefreshRedDot(activityData);
                EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
                RefreshDailyGiftButtonState();
            });
        }
    }

    #endregion

    #region 界面刷新

    [SerializeField] private Image mineSouthImage = null;
    [SerializeField] private Image mineNorthImage = null;
    private void HideAll()
    {
        mineSouthImage.gameObject.SetActive(false);
        mineNorthImage.gameObject.SetActive(false);
        northSupportButton.gameObject.SetActive(false);
        southSupportButton.gameObject.SetActive(false);
        formationButton.gameObject.SetActive(false);
        refreshButton.gameObject.SetActive(false);
        endTipText.gameObject.SetActive(false);
        northHideSignImage.gameObject.SetActive(true);
        southHideSignImage.gameObject.SetActive(true);
        leftImageFont.gameObject.SetActive(false);
        rightImageFont.gameObject.SetActive(false);
        progressItem.SetData(0, 0);
        progressItem.StopLightMove();
        HideTime();
        allStarHomeUIAnim.Init();
    }
    Stage stage = Stage.NotOpen;
    [SerializeField] private Image dotNodeImgFormation = null;
    [SerializeField] private AllStarHomeUIAnim allStarHomeUIAnim = null;
    private void RefreshUI()
    {
        stage = AllStarManager.Instance.GetStage();
        switch (stage)
        {
            case Stage.NotOpen:
                endTipText.gameObject.SetActive(true);
                endTipText.text = "活动即将开启";
                break;
            case Stage.Closed:
                endTipText.gameObject.SetActive(true);
                endTipText.text = "活动已结束";
                break;
            case Stage.Ending:
                endTipText.gameObject.SetActive(true);
                endTipText.text = "比拼已结束, 活动将于3月11日结束";
                RefreshCombat();
                RefreshBg();
                break;
            case Stage.CanSign:
                northSupportButton.gameObject.SetActive(true);
                southSupportButton.gameObject.SetActive(true);
                RefreshCombat();
                RefreshBg();
                break;
            case Stage.Playing:
                RefreshRefreshButtonShow();
                formationButton.gameObject.SetActive(true);
                dotNodeImgFormation.gameObject.SetActive(AllStarManager.Instance.savedTotalNowCombatInServer <= 0);
                RefreshCombat();
                RefreshBg();
                break;
            default:
                break;
        }
        RefreshTime();
        RefreshDailyGiftButtonState();
        RefreshredDot();
        allStarHomeUIAnim.PlayEnter();
    }
    [SerializeField] private Image dotNodeImg = null;
    private void RefreshredDot()
    {
        dotNodeImg.gameObject.SetActive(AllStarManager.Instance.IsCombatRewardCanGet());
    }
    private void RefreshRefreshButtonShow()
    {
        int newCombat = AllStarManager.Instance.GetNewTotalCombat();
        bool refreshButtonActive = AllStarManager.Instance.savedTotalNowCombatInServer < newCombat;
        refreshButton.gameObject.SetActive(refreshButtonActive);
    }
    private void RefreshBg()
    {
        if (AllStarManager.Instance.serverData == null || AllStarManager.Instance.serverData.Area == 0)
        {
            mineNorthImage.gameObject.SetActive(false);
            mineSouthImage.gameObject.SetActive(false);
            return;
        }
        bool isNorth = AllStarManager.Instance.serverData.Area == (int)Area.North;
        bool isSouth = AllStarManager.Instance.serverData.Area == (int)Area.South;
        mineNorthImage.gameObject.SetActive(isNorth);
        mineSouthImage.gameObject.SetActive(isSouth);
    }
    private void RefreshCombat()
    {
        if (AllStarManager.Instance.IsCombatNeedHide) return;
        leftImageFont.gameObject.SetActive(true);
        rightImageFont.gameObject.SetActive(true);
        northHideSignImage.gameObject.SetActive(false);
        southHideSignImage.gameObject.SetActive(false);
        progressItem.SetData(AllStarManager.Instance.serverData.South, AllStarManager.Instance.serverData.North);
        if (stage == Stage.CanSign || stage == Stage.Playing)
        {
            progressItem.StartLightMove();
        }
        else
        {
            progressItem.StopLightMove();
        }
    }

    [SerializeField] private BabuButton dailyRewardButton = null;
    [SerializeField] private Image iconOpenImage = null;
    [SerializeField] private Image iconCloseImage = null;
    [SerializeField] private LoopAnim loopAnim = null;
    [SerializeField] private Image dotNodeImgDaily = null;
    private void RefreshDailyGiftButtonState()
    {
        bool isStageRight = stage == Stage.Playing || stage == Stage.Ending;
        bool isSignd = AllStarManager.Instance.serverData != null && AllStarManager.Instance.serverData.Area != 0;
        bool hasGet = ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(activityData.cfg.Id);
        bool canOpenDailyGift = isStageRight && isSignd && !hasGet;
        iconOpenImage.gameObject.SetActive(hasGet);
        iconCloseImage.gameObject.SetActive(!hasGet);
        dotNodeImgDaily.gameObject.SetActive(canOpenDailyGift);
        if (canOpenDailyGift)
        {
            loopAnim.LockShake();
        }
        else
        {
            loopAnim.ClearLockShake();
        }
    }

    private bool needRefreshTime = false;
    private void HideTime()
    {
        timeBar.gameObject.SetActive(false);
        needRefreshTime = false;
    }
    private void RefreshLeftTimeOneSec()
    {
        if (needRefreshTime) RefreshTime();
    }
    [SerializeField] private RectTransform timeBar = null;
    [SerializeField] private HorizontalLayoutGroup timeLayout = null;
    [SerializeField] private TMP_Text leftTimeText = null;
    private void RefreshTime()
    {
        if (stage == Stage.CanSign || stage == Stage.Playing)
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.AllStarHome);
            long endTime = activityData.EndTime;
            long leftTime = endTime - Utils.DataConvUtil.ServerTime;
            if (leftTime <= 0)
            {
                HideTime();
            }
            else
            {
                needRefreshTime = true;
                timeBar.gameObject.SetActive(true);
                leftTimeText.text = TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime);
                LayoutRebuilder.ForceRebuildLayoutImmediate(leftTimeText.transform as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(timeLayout.transform as RectTransform);
            }
        }
        else
        {
            HideTime();
        }
    }

    #endregion


}
