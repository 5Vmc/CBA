using Babu;
using BigBang.Animation;
using CBA;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.UI
{
    public class HundredHomeUIFight3Pad : MonoBehaviour
    {

        [SerializeField] private BabuButton formationButton = null;
        [SerializeField] private TMP_Text signTimeTipText = null;
        [SerializeField] private LeftTimeComponent leftTimeComponent = null;
        [SerializeField] private ScrollRect scrollView = null;
        [SerializeField] private RectTransform midContent = null;
        [SerializeField] private SkeletonGraphic noWinnerBgSpine = null;
        [SerializeField] private Image winnerNoTeamImage = null;
        [SerializeField] private BabuButton winnerBgImage = null;
        [SerializeField] private ClubIconItem winnerClubIconImage = null;
        [SerializeField] private List<HundredHomeUIFight3Item> hundredHomeUIFight3ItemList = new();
        [SerializeField] private ImageFont titleImageFont = null;
        [SerializeField] private BabuButton helpBtn = null;
        [SerializeField] private Image cupImage = null;

        [SerializeField] private BabuButton guessButton = null;
        [SerializeField] private Image dotNodeImgGuess = null;

        protected void OnEnable()
        {
            helpBtn.OnClick += OnClickHelpButton;
            formationButton.OnClick += OnClickFormationButton;
            winnerBgImage.OnClick += OnClickWinner;
            guessButton.OnClick += OnClickGuessButton;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
            RefreshRedDot(null);
        }
        protected void OnDisable()
        {
            helpBtn.OnClick -= OnClickHelpButton;
            formationButton.OnClick -= OnClickFormationButton;
            winnerBgImage.OnClick -= OnClickWinner;
            guessButton.OnClick -= OnClickGuessButton;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
            EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
            RemoveTimer();
        }
        private void RefreshRedDot(object[] _)
        {
            RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Hundred, "/Guess");
            redDotNode.IsRed(dotNodeImgGuess.transform);
        }

        GetHundredCourseResponse serverData = null;
        public void OnShow(bool needRefreshData = false)
        {
            HundredManager.Instance.GetCourse(HundredManager.Instance.dropdownValue + 1, needRefreshData, (GetHundredCourseResponse getHundredCourseResponse) =>
            {
                this.serverData = getHundredCourseResponse;
                CheckStage();
                Refresh();
            });
        }
        public void CheckStage()
        {
            switch ((HundredProgress)serverData.Stage)
            {
                case HundredProgress.Fight3:
                    break;
                default:
                    EventManager.Instance.Dispatch(EventID.OnHundredStageMismatch);
                    break;
            }
        }
        private void Refresh()
        {
            HundredManager.Instance.SetTitle(titleImageFont, serverData);
            RefreshLeftTime();
            scrollView.gameObject.SetActive(true);
            SetSpider();
            RefreshShowFormationButton();
        }
        private void RefreshShowFormationButton()
        {
            bool isSign = serverData.MyZoneId != 0;
            bool isOut = serverData.IsOut;
            bool isWinnerCome = HundredManager.Instance.IsWinnerCome(serverData, 3);
            formationButton.gameObject.SetActive(isSign && !isOut && !isWinnerCome);
        }

        private void OnClickHelpButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<HundredHelpUI>();
        }

        private void OnClickFormationButton(BabuButton _)
        {
            HundredManager.Instance.GetFormationLeftTime(serverData, 3, out HundredFormationUI.HFType hfType, out int leftime);
            UIController.Instance.OpenWindow<HundredFormationUI>(new HundredFormationUIProperties(HundredProgress.Fight3, hfType, leftime));
        }
        private void RefreshLeftTimeOneSec()
        {
            if (leftTime > 0 && needShowTime)
            {
                leftTime--;
                if (leftTime == 0)
                {
                    OnShow(true);
                    EventManager.Instance.Dispatch(EventID.OnHundredNeedRefreshGuess);
                }
                leftTimeComponent.SetLeftTimeText(leftTime);
            }
        }

        private int leftTime = -1;
        private void SetLeftTime(int leftTime)
        {
            this.leftTime = leftTime;
            leftTimeComponent.SetLeftTimeText(leftTime);
        }

        private bool needShowTime = false;
        private bool isEnd = false;
        private Timer refreshDataTimer = null;
        private HashSet<LeagueCourseItemData> nearFightSet = new();
        private void RefreshLeftTime()
        {
            needShowTime = false;
            nearFightSet.Clear();
            isEnd = false;
            var data1 = serverData.LeagueCourseItemList.Where(item => item.Round == 3).ToList();
            if (data1 != null && data1.Count > 0)
            {
                LeagueCourseItemData leagueCourseItemData = data1[0];
                if (leagueCourseItemData != null && leagueCourseItemData.AwayGoal > -1 && leagueCourseItemData.HomeGoal > -1) isEnd = true;
            }
            RemoveTimer();
            if (!isEnd)
            {
                LeagueCourseItemData nearFight = null;
                int minLefttime = int.MaxValue;
                bool findNextFight = false;
                foreach (LeagueCourseItemData leagueCourseItemData in serverData.LeagueCourseItemList)
                {
                    if (leagueCourseItemData == null) continue;
                    if (leagueCourseItemData.AwayTeam == null) continue;
                    if (leagueCourseItemData.HomeTeam == null) continue;
                    if (leagueCourseItemData.HomeGoal > 0) continue;
                    if (leagueCourseItemData.AwayGoal > 0) continue;
                    if (leagueCourseItemData.Time <= Utils.DataConvUtil.ServerTime) continue;
                    int itemLeftTime = (int)(leagueCourseItemData.Time - Utils.DataConvUtil.ServerTime) + 5;
                    if (itemLeftTime < minLefttime)
                    {
                        minLefttime = itemLeftTime;
                        nearFight = leagueCourseItemData;
                        findNextFight = true;
                        needShowTime = true;
                        signTimeTipText.text = "下场比赛倒计时";
                    }
                }
                if (nearFight != null)
                {
                    foreach (LeagueCourseItemData leagueCourseItemData in serverData.LeagueCourseItemList)
                    {
                        if (leagueCourseItemData.Time == nearFight.Time)
                        {
                            nearFightSet.Add(leagueCourseItemData);
                        }
                    }
                }
                if (findNextFight)
                {
                    SetLeftTime(minLefttime);
                }
                else
                {
                    refreshDataTimer = Timer.Register(this.gameObject, HundredManager.refreshNextBattleTime, () =>
                    {
                        Debug.Log("refreshDataTimer");
                        OnShow(true);
                    }, null, true, true);
                }
            }
            else
            {
                needShowTime = true;
                signTimeTipText.text = "休赛期开始倒计时";
                SetLeftTime((int)(serverData.StageEndTime - Utils.DataConvUtil.ServerTime));
            }
            signTimeTipText.gameObject.SetActive(needShowTime);
            leftTimeComponent.gameObject.SetActive(needShowTime);
        }
        private void RemoveTimer()
        {
            refreshDataTimer?.Cancel();
            refreshDataTimer = null;
        }

        private void SetSpider()
        {
            if (serverData.LeagueCourseItemList.Count != 7)
            {
                Debug.LogWarning("HundredHomeUIFight3Pad , SetSpider , serverData.LeagueCourseItemList.Count = " + serverData.LeagueCourseItemList.Count);
            }
            for (int i = 0; i < 7; i++)
            {
                HundredHomeUIFight3Item hundredHomeUIFight3Item = hundredHomeUIFight3ItemList[i];
                LeagueCourseItemData leagueCourseItemData = (i < serverData.LeagueCourseItemList.Count) ? serverData.LeagueCourseItemList[i] : null;
                hundredHomeUIFight3Item.SetData(leagueCourseItemData, i, nearFightSet.Contains(leagueCourseItemData));
            }
            LeagueCourseItemData winnerData = (6 < serverData.LeagueCourseItemList.Count) ? serverData.LeagueCourseItemList[6] : null;
            bool isEnd = true;
            if (winnerData == null || winnerData.AwayTeam == null || winnerData.AwayTeam == null || winnerData.AwayGoal == -1 || winnerData.HomeGoal == -1)
            {
                isEnd = false;
            }
            noWinnerBgSpine.gameObject.SetActive(!isEnd);
            winnerNoTeamImage.gameObject.SetActive(!isEnd);
            winnerBgImage.gameObject.SetActive(isEnd);
            cupImage.SetAlpha(isEnd ? 0.7f : 1f);
            if (isEnd) winnerClubIconImage.SetIcon(winnerData.AwayGoal > winnerData.HomeGoal ? winnerData.AwayTeam.TeamIcon : winnerData.HomeTeam.TeamIcon);
        }
        private void OnClickWinner(BabuButton button)
        {
            LeagueCourseItemData winnerData = (6 < serverData.LeagueCourseItemList.Count) ? serverData.LeagueCourseItemList[6] : null;
            bool isEnd = true;
            if (winnerData == null || winnerData.AwayTeam == null || winnerData.AwayTeam == null || winnerData.AwayGoal == -1 || winnerData.HomeGoal == -1)
            {
                isEnd = false;
            }
            if (!isEnd) return;
            UIController.Instance.OpenWindow<HundredSingleDetailUI>(new HundredSingleDetailUIProperties(winnerData.FightId, winnerData.AwayGoal > winnerData.HomeGoal, CompitionID.Hundred, winnerData.AwayGoal > winnerData.HomeGoal ? winnerData.AwayTeam.TeamId : winnerData.HomeTeam.TeamId));
        }

        private void OnClickGuessButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<HundredGuessUI>(new HundredGuessUIProperties(false));
        }

    }
}