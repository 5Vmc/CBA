using Babu;
using BigBang.Animation;
using CBA;
using Coffee.UIEffects;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using Spine;
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
    public class HundredHomeUIFight1Pad : MonoBehaviour
    {

        [SerializeField] private TMP_Text signTimeTipText = null;
        [SerializeField] private LeftTimeComponent leftTimeComponent = null;
        [SerializeField] private BabuButton formationButton = null;
        [SerializeField] private ScrollRect winLoseScrollView = null;
        [SerializeField] private HorizontalLayoutGroup content = null;
        [SerializeField] private GameObject hundredFight1WinLoseItemPrefab = null;
        [SerializeField] private TMP_Text countNumText = null;
        [SerializeField] private Image countProgressFgImage = null;
        [SerializeField] private RectTransform winPanel = null;
        [SerializeField] private RectTransform losePanel = null;
        [SerializeField] private HundredFight1FightingItem fightingPanel = null;
        [SerializeField] private RectTransform matchingPanel = null;
        [SerializeField] private RectTransform teamPanel = null;//OSA
        [SerializeField] private HundredHomeUIFight1Adapter hundredHomeUIFight1Adapter = null;
        [SerializeField] private ImageFont titleImageFont = null;
        [SerializeField] private ImageFont stageTitleImageFont = null;
        [SerializeField] private BabuButton helpBtn = null;

        private void Awake()
        {

        }
        protected void OnEnable()
        {
            helpBtn.OnClick += OnClickHelpButton;
            formationButton.OnClick += OnClickFormationButton;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
        }
        protected void OnDisable()
        {
            helpBtn.OnClick -= OnClickHelpButton;
            formationButton.OnClick -= OnClickFormationButton;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
            RemoveTimer();
        }

        GetHundredCourseResponse serverData = null;
        public void OnShow(bool needNewData = false)
        {
            HundredManager.Instance.GetCourse(HundredManager.Instance.dropdownValue + 1, needNewData, (GetHundredCourseResponse getHundredCourseResponse) =>
            {
                this.serverData = getHundredCourseResponse;
                CheckStage();
                HundredManager.Instance.SetTitle(titleImageFont, serverData);
                Refresh();
            });
        }
        public void CheckStage()
        {
            switch ((HundredProgress)serverData.Stage)
            {
                case HundredProgress.Fight1:
                    break;
                default:
                    EventManager.Instance.Dispatch(EventID.OnHundredStageMismatch);
                    break;
            }
        }
        private readonly string newTitle = "新星赛区入围赛";
        private readonly string normalTitle = "第{0}赛区入围赛";
        private void Refresh()
        {
            if(serverData.MyZoneId == 9)
            {
                stageTitleImageFont.text = newTitle;
            }
            else
            {
                stageTitleImageFont.text = normalTitle.SafeFormat(serverData.MyZoneId.ToChinese());
            }
            GetFightData();
            SetWinLosePanel();
            SetLeftNum();
            SetUpDown();
            SetOsa();
            RefreshShowFormationButton();
        }
        private void RefreshShowFormationButton()
        {
            bool isOut = serverData.IsOut;
            bool isSign = serverData.MyZoneId != 0;
            formationButton.gameObject.SetActive(isSign && !isOut);
        }

        private void OnClickHelpButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<HundredHelpUI>();
        }

        private void OnClickFormationButton(BabuButton _)
        {
            HundredManager.Instance.GetFormationLeftTimeFight1(serverData, out HundredFormationUI.HFType hfType, out int leftime);
            UIController.Instance.OpenWindow<HundredFormationUI>(new HundredFormationUIProperties(HundredProgress.Fight1, hfType, leftime));
        }

        [HideInInspector] private List<LeagueCourseItemData> fightDataList = new();
        private void GetFightData()
        {
            fightDataList.Clear();
            foreach (LeagueCourseItemData leagueCourseItemData in serverData.LeagueCourseItemList)
            {
                if (leagueCourseItemData.HomeTeam == null || leagueCourseItemData.AwayTeam == null) continue;
                if (leagueCourseItemData.HomeTeam.TeamId == Player.GbId || leagueCourseItemData.AwayTeam.TeamId == Player.GbId)
                {
                    if (leagueCourseItemData.HomeGoal > -1 && leagueCourseItemData.AwayGoal > -1)
                        fightDataList.Add(leagueCourseItemData);
                }
            }
        }
        private void RefreshLeftTimeOneSec()
        {
            if (leftTime > 0)
            {
                leftTime--;
                if (leftTime == 0)
                {
                    signTimeTipText.text = "当前阶段结束";
                }
                leftTimeComponent.SetLeftTimeText(leftTime);
            }
        }
        private void SetLeftTime(int leftTime)
        {
            this.leftTime = leftTime;
            leftTimeComponent.SetLeftTimeText(leftTime);
        }

        [SerializeField] private List<HundredFight1WinLoseItem> winLoseItemList = new();
        private void SetWinLosePanel()//顶部每场胜负
        {
            List<LeagueCourseItemData> fightDataListTop = fightDataList.ToList();
            while (winLoseItemList.Count < fightDataListTop.Count)
            {
                GameObject winLoseItemGameObject = GameObject.Instantiate(hundredFight1WinLoseItemPrefab, content.transform);
                HundredFight1WinLoseItem winLoseItemComponent = winLoseItemGameObject.GetComponent<HundredFight1WinLoseItem>();
                winLoseItemList.Add(winLoseItemComponent);
            }
            for (int i = 0; i < winLoseItemList.Count || i < fightDataListTop.Count; i++)
            {
                HundredFight1WinLoseItem hundredFight1WinLoseItem = winLoseItemList[i];
                hundredFight1WinLoseItem.winImage.gameObject.SetActive(false);
                hundredFight1WinLoseItem.loseImage.gameObject.SetActive(false);
                hundredFight1WinLoseItem.emptyImage.gameObject.SetActive(false);
                bool hasData = i < fightDataListTop.Count;
                if (hasData)
                {
                    LeagueCourseItemData leagueCourseItemData = fightDataListTop[i];
                    bool isWin = HundredManager.Instance.IsFightWin(leagueCourseItemData);
                    hundredFight1WinLoseItem.winImage.gameObject.SetActive(isWin);
                    hundredFight1WinLoseItem.loseImage.gameObject.SetActive(!isWin);
                    hundredFight1WinLoseItem.gameObject.SetActive(true);
                }
                else
                {
                    hundredFight1WinLoseItem.emptyImage.gameObject.SetActive(true);
                    hundredFight1WinLoseItem.gameObject.SetActive(i < 10);
                }
            }
        }

        private void SetLeftNum()//剩余人数和进度条
        {
            int totalNum = serverData.ZoneSignTeamCount[serverData.MyZoneId - 1];
            int outNum = serverData.ZoneOutTeamCount[serverData.MyZoneId - 1];
            int leftNum = totalNum - outNum;
            leftNum = Utility.KeepInRange(leftNum, 64, 9999);
            totalNum = Utility.KeepInRange(totalNum, 64, 9999);
            countNumText.text = "<color=#16DA41>{0}</color>/{1}".SafeFormat(leftNum, totalNum);

            float realProgress = 1 - (float)(totalNum - leftNum) / (float)(totalNum - 64);
            float showProgress = 0.3f + Mathf.Lerp(0f, 0.7f, realProgress);
            countProgressFgImage.fillAmount = showProgress;
        }

        [HideInInspector] private int leftTime = -1;
        private Timer refreshDataTimer = null;
        private void SetUpDown()//是否晋级
        {
            HundredManager.Instance.GetFight1EndAndWin(serverData, out bool isEnd, out bool isDown);

            winPanel.gameObject.SetActive(isEnd && !isDown);
            losePanel.gameObject.SetActive(isEnd && isDown);
            fightingPanel.gameObject.SetActive(false);
            matchingPanel.gameObject.SetActive(false);
            SetOsaSize(isEnd);
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
                    }
                }
                fightingPanel.gameObject.SetActive(findNextFight);
                if (findNextFight)
                {
                    fightingPanel.SetData(nearFight, () =>
                    {
                        OnShow(true);
                    });
                }
                matchingPanel.gameObject.SetActive(!findNextFight);
                if (!findNextFight)
                {
                    refreshDataTimer = Timer.Register(this.gameObject, HundredManager.refreshNextBattleTime, () =>
                    {
                        Debug.Log("refreshDataTimer");
                        OnShow(true);
                    }, null, true, true);
                }
            }

            signTimeTipText.gameObject.SetActive(isEnd);
            leftTimeComponent.gameObject.SetActive(isEnd);
            if (isEnd)
            {
                signTimeTipText.text = "淘汰赛开始倒计时";
                SetLeftTime((int)(serverData.StageEndTime - Utils.DataConvUtil.ServerTime));
            }
        }
        private void RemoveTimer()
        {
            refreshDataTimer?.Cancel();
            refreshDataTimer = null;
        }
        private readonly float osaSmallTop = 379.3298f;
        private readonly float osaBigTop = 405.2113f;
        private void SetOsaSize(bool isSmall)
        {
            teamPanel.SetTop(isSmall ? osaSmallTop : osaBigTop);
        }

        private void SetOsa()
        {
            List<LeagueCourseItemData> fightDataListOsa = fightDataList.ToList();
            fightDataListOsa.Reverse();
            hundredHomeUIFight1Adapter.SetData(fightDataListOsa);
        }

    }
}