using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang;
using BigBang.Animation;
using BigBang.UI;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using static BigBang.PlayoffFinalsGuessManager;
using static BigBang.SpriteNames;
using GameItem = Utils.GameItem.GameItem;
using RewardType = BigBang.PlayoffFinalsGuessManager.RewardType;

public class PlayoffFinalsGuessHomePad : MonoBehaviour, IActivity
{
    #region 初始化

    private void OnEnable()
    {
        SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
        helpButton.OnClick += OnClickHelpButton;
        selectLeftTeamButton.OnClick += OnClickSelectLeftTeamButton;
        selectRightTeamButton.OnClick += OnClickSelectRightTeamButton;
        EventManager.Instance.Register(EventID.RefreshPlayoffFinalsGuessUI, RefreshPlayoffFinalsGuessHomePad);
        selectMVPButton.OnClick += OnClickSelectMVPButton;
        selectedMVPButton.OnClick += OnClickSelectedMVPButton;
        getEndRewardButton.OnClick += OnClickGetEndRewardButton;
        endDailyRewardButtonCanGet.OnClick += OnClickEndDailyRewardButtonCanGet;
        endDailyRewardButtonHasGot.OnClick += OnClickEndDailyRewardButtonHasGot;

    }

    private void OnDisable()
    {
        SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
        helpButton.OnClick -= OnClickHelpButton;
        selectLeftTeamButton.OnClick -= OnClickSelectLeftTeamButton;
        selectRightTeamButton.OnClick -= OnClickSelectRightTeamButton;
        EventManager.Instance.Unregister(EventID.RefreshPlayoffFinalsGuessUI, RefreshPlayoffFinalsGuessHomePad);
        selectMVPButton.OnClick -= OnClickSelectMVPButton;
        selectedMVPButton.OnClick -= OnClickSelectedMVPButton;
        getEndRewardButton.OnClick -= OnClickGetEndRewardButton;
        endDailyRewardButtonCanGet.OnClick -= OnClickEndDailyRewardButtonCanGet;
        endDailyRewardButtonHasGot.OnClick -= OnClickEndDailyRewardButtonHasGot;
    }
    private ActivityData activityData = null;
    public void LoadActivity(ActivityData _data)
    {
        activityData = _data;
        HideAll();
        loadingDataAnim.Init();
        playoffFinalsGuessHomePadAnim.Init();
        loadingDataAnim.PlayEnter();
        PlayoffFinalsGuessManager.Instance.GetCourseData(() =>
        {
            RefreshUI();
            loadingDataAnim.PlayExit();
            playoffFinalsGuessHomePadAnim.PlayEnter(() =>
            {

            });
        });
    }
    private void RefreshPlayoffFinalsGuessHomePad(object[] _)
    {
        HideAll();
        RefreshUI();
    }
    #endregion

    #region 按钮回调

    [SerializeField] private BabuButton helpButton = null;
    private void OnClickHelpButton(BabuButton _)
    {
        UIController.Instance.OpenWindow<PlayoffFinalsGuessHelpUI>();
    }

    [SerializeField] private BabuButton selectLeftTeamButton = null;
    private void OnClickSelectLeftTeamButton(BabuButton _)
    {
        OnClickSelectTeamButton(Team.Left);
    }
    [SerializeField] private BabuButton selectRightTeamButton = null;
    private void OnClickSelectRightTeamButton(BabuButton _)
    {
        OnClickSelectTeamButton(Team.Right);
    }
    private void OnClickSelectTeamButton(Team team)
    {
        if (PlayoffFinalsGuessManager.Instance.isTeamSelected) return;
        string selectTeamName = Configs.FinalsGuessTeam.GetConfig((int)team).Name;
        UIController.Instance.OpenWindow<ConfirmationBoxUI>(new ConfirmationBoxUIProperties("选择后无法更改，确认预测 {0} 获得最终胜利吗？".SafeFormat(selectTeamName), () =>
        {
            int selectStopTime = Configs.FinalsGuessCourse.GetConfig(PlayoffFinalsGuessManager.Instance.selectStopCourseId).MatchTime;
            long leftTime = selectStopTime - Utils.DataConvUtil.ServerTime;
            if (leftTime <= 0)
            {
                Tips.PopError("已过可预测时间");
            }
            else
            {
                PlayoffFinalsGuessManager.Instance.GuessChampion((int)team);
            }
        }));
    }

    [SerializeField] private BabuButton selectMVPButton = null;
    private void OnClickSelectMVPButton(BabuButton _)
    {
        UIController.Instance.OpenWindow<PlayoffFinalsGuessMVPUI>(new PlayoffFinalsGuessMVPUIProperties((Team)PlayoffFinalsGuessManager.Instance.teamGuessData.Guess));
    }

    [SerializeField] private BabuButton selectedMVPButton = null;
    private void OnClickSelectedMVPButton(BabuButton _)
    {
        UIController.Instance.OpenWindow<PlayoffFinalsGuessHomeHistoryUI>();
    }

    #endregion

    #region 界面刷新
    [SerializeField] private RectTransform timePanel = null;
    [SerializeField] private RectTransform vSPanel = null;
    [SerializeField] private RectTransform namePanel = null;
    [SerializeField] private RectTransform titlePanel = null;
    [SerializeField] private RectTransform selectTeamPanel = null;
    [SerializeField] private RectTransform selectMVPPanel = null;
    [SerializeField] private RectTransform selectedMVPPanel = null;
    [SerializeField] private RectTransform notSelectedMVPPanel = null;
    [SerializeField] private RectTransform loadingPanel = null;
    [SerializeField] private TMP_Text notOpenText = null;
    [SerializeField] private TMP_Text closedText = null;
    [SerializeField] private Image leftTeamImageBoforeSelect = null;
    [SerializeField] private Image leftTeamImageNotSelected = null;
    [SerializeField] private Image leftTeamImageHasSelected = null;
    [SerializeField] private Image rightTeamImageBeforeSelect = null;
    [SerializeField] private Image rightTeamImageNotSelected = null;
    [SerializeField] private Image rightTeamImageHasSelected = null;
    private void HideAll()
    {
        playingPanel.gameObject.SetActive(false);
        endingPanel.gameObject.SetActive(false);

        timePanel.gameObject.SetActive(false);
        timeRoot.gameObject.SetActive(false);
        vSPanel.gameObject.SetActive(false);
        namePanel.gameObject.SetActive(false);
        titlePanel.gameObject.SetActive(true);
        selectTeamPanel.gameObject.SetActive(false);
        selectMVPPanel.gameObject.SetActive(false);
        selectedMVPPanel.gameObject.SetActive(false);
        notSelectedMVPPanel.gameObject.SetActive(false);
        loadingPanel.gameObject.SetActive(true);
        notOpenText.gameObject.SetActive(false);
        closedText.gameObject.SetActive(false);
        leftTeamImageBoforeSelect.gameObject.SetActive(false);
        leftTeamImageNotSelected.gameObject.SetActive(false);
        leftTeamImageHasSelected.gameObject.SetActive(false);
        rightTeamImageBeforeSelect.gameObject.SetActive(false);
        rightTeamImageNotSelected.gameObject.SetActive(false);
        rightTeamImageHasSelected.gameObject.SetActive(false);
    }
    Stage stage = Stage.NotOpen;
    [SerializeField] private PlayoffFinalsGuessHomePadAnim playoffFinalsGuessHomePadAnim = null;
    [SerializeField] private LoadingDataAnim loadingDataAnim = null;
    private void RefreshUI()
    {
        stage = PlayoffFinalsGuessManager.Instance.GetStage();
        timeRoot.gameObject.SetActive(stage == Stage.CanSelectMVP || stage == Stage.CanSelectTeam);
        switch (stage)
        {
            case Stage.CanSelectTeam:
                playingPanel.gameObject.SetActive(true);
                timePanel.gameObject.SetActive(true);
                vSPanel.gameObject.SetActive(true);
                namePanel.gameObject.SetActive(true);
                titlePanel.gameObject.SetActive(true);
                selectTeamPanel.gameObject.SetActive(true);
                leftTeamImageBoforeSelect.gameObject.SetActive(true);
                rightTeamImageBeforeSelect.gameObject.SetActive(true);
                RefreshProgress();
                break;
            case Stage.CanSelectMVP:
                playingPanel.gameObject.SetActive(true);
                timePanel.gameObject.SetActive(true);
                vSPanel.gameObject.SetActive(true);
                namePanel.gameObject.SetActive(true);
                titlePanel.gameObject.SetActive(true);
                selectMVPPanel.gameObject.SetActive(true);
                leftTeamImageNotSelected.gameObject.SetActive(PlayoffFinalsGuessManager.Instance.teamGuessData.Guess != (int)Team.Left);
                rightTeamImageNotSelected.gameObject.SetActive(PlayoffFinalsGuessManager.Instance.teamGuessData.Guess != (int)Team.Right);
                leftTeamImageHasSelected.gameObject.SetActive(PlayoffFinalsGuessManager.Instance.teamGuessData.Guess == (int)Team.Left);
                rightTeamImageHasSelected.gameObject.SetActive(PlayoffFinalsGuessManager.Instance.teamGuessData.Guess == (int)Team.Right);
                RefreshProgress();
                break;
            case Stage.NormalPlaying:
                playingPanel.gameObject.SetActive(true);
                timePanel.gameObject.SetActive(true);
                vSPanel.gameObject.SetActive(true);
                namePanel.gameObject.SetActive(true);
                titlePanel.gameObject.SetActive(true);
                selectedMVPPanel.gameObject.SetActive(PlayoffFinalsGuessManager.Instance.isTeamSelected || PlayoffFinalsGuessManager.Instance.isMVPSelected);
                notSelectedMVPPanel.gameObject.SetActive(!PlayoffFinalsGuessManager.Instance.isTeamSelected && !PlayoffFinalsGuessManager.Instance.isMVPSelected);
                if (PlayoffFinalsGuessManager.Instance.isTeamSelected)
                {
                    leftTeamImageNotSelected.gameObject.SetActive(PlayoffFinalsGuessManager.Instance.teamGuessData.Guess != (int)Team.Left);
                    rightTeamImageNotSelected.gameObject.SetActive(PlayoffFinalsGuessManager.Instance.teamGuessData.Guess != (int)Team.Right);
                    leftTeamImageHasSelected.gameObject.SetActive(PlayoffFinalsGuessManager.Instance.teamGuessData.Guess == (int)Team.Left);
                    rightTeamImageHasSelected.gameObject.SetActive(PlayoffFinalsGuessManager.Instance.teamGuessData.Guess == (int)Team.Right);
                }
                else
                {
                    leftTeamImageBoforeSelect.gameObject.SetActive(true);
                    rightTeamImageBeforeSelect.gameObject.SetActive(true);
                }
                RefreshProgress();
                break;
            case Stage.Ending:
                endingPanel.gameObject.SetActive(true);
                titlePanel.gameObject.SetActive(true);
                RefreshStageEndingUI();
                break;
            case Stage.Closed:
                titlePanel.gameObject.SetActive(true);
                closedText.gameObject.SetActive(true);
                break;
            default://Stage.NotOpen
                titlePanel.gameObject.SetActive(true);
                notOpenText.gameObject.SetActive(true);
                break;
        }
        RefreshTime();
    }

    private bool needRefreshTime = false;
    private void HideTime()
    {
        timeRoot.gameObject.SetActive(false);
        needRefreshTime = false;
    }
    private void RefreshLeftTimeOneSec()
    {
        if (needRefreshTime) RefreshTime();
        RefreshEndTime();
    }
    [SerializeField] private RectTransform timeRoot = null;
    [SerializeField] private TMP_Text gussTimeText = null;
    private void RefreshTime()
    {
        if (stage == Stage.CanSelectMVP || stage == Stage.CanSelectTeam)
        {
            int selectStopTime = Configs.FinalsGuessCourse.GetConfig(PlayoffFinalsGuessManager.Instance.selectStopCourseId).MatchTime;
            long leftTime = selectStopTime - Utils.DataConvUtil.ServerTime;
            if (leftTime <= 0)
            {
                HideTime();
            }
            else
            {
                needRefreshTime = true;
                timeRoot.gameObject.SetActive(true);
                gussTimeText.text = TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime);
            }
        }
        else
        {
            HideTime();
        }
    }

    [SerializeField] private PlayoffFinalsGuessHomePadProgressItem progressItem = null;
    private void RefreshProgress()
    {
        if (PlayoffFinalsGuessManager.Instance.courseData == null) return;
        progressItem.SetData(PlayoffFinalsGuessManager.Instance.team1Support, PlayoffFinalsGuessManager.Instance.team2Support);
    }

    #endregion

    #region 结算后界面

    [SerializeField] private RectTransform playingPanel = null;
    [SerializeField] private RectTransform endingPanel = null;
    [SerializeField] private Image endTeamIconImage = null;
    [SerializeField] private Image endTeamPlayerImage = null;
    [SerializeField] private TMP_Text endTeamNameText = null;
    [SerializeField] private Image endMVPPlayerImage = null;
    [SerializeField] private TMP_Text endMVPNameText = null;
    [SerializeField] private RectTransform getEndReweardPanel = null;
    [SerializeField] private TMP_Text endRewardTipText = null;
    [SerializeField] private HorizontalLayoutGroup endRewardLayout = null;
    [SerializeField] private List<InventoryItem> inventoryItemList = null;
    [SerializeField] private BabuButton getEndRewardButton = null;
    [SerializeField] private TMP_Text endNotSelectedTipText = null;
    [SerializeField] private TMP_Text activityLeftTimeText = null;
    [SerializeField] private BabuButton endDailyRewardButtonCanGet = null;
    [SerializeField] private BabuButton endDailyRewardButtonHasGot = null;

    private List<GameItem> gameItemList = new();
    private async void RefreshStageEndingUI()
    {
        Team winTeam = PlayoffFinalsGuessManager.Instance.team1WinCount > PlayoffFinalsGuessManager.Instance.team2WinCount ? Team.Left : Team.Right;
        FinalsGuessTeamConfig finalsGuessTeamConfig = Configs.FinalsGuessTeam.GetConfig((int)winTeam);
        endTeamIconImage.sprite = await SpriteProxy.GetPlayoffFinalsGuessEndTeamLogoSprite(finalsGuessTeamConfig.Icon);
        endTeamPlayerImage.sprite = await SpriteProxy.GetPlayoffFinalsGuessEndTeamPlayerSprite(finalsGuessTeamConfig.Icon);
        endTeamNameText.text = "恭喜 {0} 获得冠军".SafeFormat(finalsGuessTeamConfig.Name);
        FinalsGuessPlayerConfig finalsGuessPlayerConfig = Configs.FinalsGuessPlayer.GetConfig(PlayoffFinalsGuessManager.Instance.courseData.MvpPlayerId);
        if (finalsGuessPlayerConfig == null)
        {
            Debug.LogWarningFormat("PlayoffFinalsGuessHomePad , RefreshStageEndingUI , finalsGuessPlayerConfig == null , PlayoffFinalsGuessManager.Instance.courseData.MvpPlayerId = {0}", PlayoffFinalsGuessManager.Instance.courseData.MvpPlayerId);
            return;
        }
        else
        {
            endMVPPlayerImage.sprite = await SpriteProxy.GetPlayoffFinalsGuessMVPPlayerSprite(finalsGuessPlayerConfig.Icon);
            endMVPNameText.text = finalsGuessPlayerConfig.Name;
        }
        bool isGuess = PlayoffFinalsGuessManager.Instance.isMVPSelected || PlayoffFinalsGuessManager.Instance.isTeamSelected;
        getEndReweardPanel.gameObject.SetActive(isGuess);
        endNotSelectedTipText.gameObject.SetActive(!isGuess);
        gameItemList.Clear();
        if (isGuess)
        {
            if (PlayoffFinalsGuessManager.Instance.isEndRewardCanGet)
            {
                endRewardTipText.text = "本次预测已结束，请领取您的预测奖励。";
                getEndRewardButton.gameObject.SetActive(true);
            }
            else
            {
                endRewardTipText.text = "本次预测已结束，预测奖励已领取。";
                getEndRewardButton.gameObject.SetActive(false);
            }
            if (PlayoffFinalsGuessManager.Instance.isTeamSelected)
            {
                FinalsGuessRewardConfig finalsGuessRewardConfig = Configs.FinalsGuessReward.GetConfig((int)RewardType.Champion);
                string rewardStr = PlayoffFinalsGuessManager.Instance.IsGuessTeamGuessWin ? finalsGuessRewardConfig.SuccessReward : finalsGuessRewardConfig.FailReward;
                List<GameItem> gameItemTeamList = GameItemUtils.CreateGameItems(rewardStr).ToList();
                gameItemList.AddRange(gameItemTeamList);
            }
            if (PlayoffFinalsGuessManager.Instance.isMVPSelected)
            {
                FinalsGuessRewardConfig finalsGuessRewardConfig = Configs.FinalsGuessReward.GetConfig((int)RewardType.MVP);
                string rewardStr = PlayoffFinalsGuessManager.Instance.isGuessMvpGuessWin ? finalsGuessRewardConfig.SuccessReward : finalsGuessRewardConfig.FailReward;
                List<GameItem> gameItemMVPList = GameItemUtils.CreateGameItems(rewardStr).ToList();
                gameItemList.AddRange(gameItemMVPList);
            }
            GameItemUtils.SetRewards(inventoryItemList, gameItemList);
        }
        ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.PlayoffFinalsGuessHome);
        bool isCanGetDailyReward = !ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(activityData.cfg.Id);
        endDailyRewardButtonCanGet.gameObject.SetActive(isCanGetDailyReward);
        endDailyRewardButtonHasGot.gameObject.SetActive(!isCanGetDailyReward);
        RefreshEndTime();
    }

    private void OnClickGetEndRewardButton(BabuButton _)
    {
        PlayoffFinalsGuessManager.Instance.ReceiveEndReward(() =>
        {
            RefreshStageEndingUI();
            var properties = new InventoryObtainedUIProperties(gameItemList);
            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
        });
    }
    private void OnClickEndDailyRewardButtonCanGet(BabuButton _)
    {
        ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.PlayoffFinalsGuessHome);
        NetworkManager.Instance.ReceiveDailyGift(activityData.cfg.Id, (resp) =>
        {
            if (resp.ReceiveSucceed == false)
            {
                Tips.PopTips("领取失败");
                Debug.LogWarningFormat("PlayoffFinalsGuessHomePad , OnClickEndDailyRewardButtonCanGet , resp.ReceiveSucceed == false , activityData.cfg.Id = {0}", activityData.cfg.Id);
                return;
            }
            if (ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(activityData.cfg.Id) == false)
                ActivityController.Instance.dailyGiftReceivedActivityIdSet.Add(activityData.cfg.Id);
            var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(activityData.cfg.DailyGift).ToList());
            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
            RefreshStageEndingUI();
        });
    }
    private void OnClickEndDailyRewardButtonHasGot(BabuButton _)
    {
        Tips.PopTips("奖励领取，请明日再来");
    }
    private void RefreshEndTime()
    {
        if (stage != Stage.Ending) return;
        long leftTime = activityData.EndTime - Utils.DataConvUtil.ServerTime;
        activityLeftTimeText.text = "活动剩余时间：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
    }

    #endregion


}
