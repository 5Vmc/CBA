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
    public class HundredHomeUIFight2Pad : MonoBehaviour
    {

        [SerializeField] private BabuButton formationButton = null;
        [SerializeField] private TMP_Text signTimeTipText = null;
        [SerializeField] private LeftTimeComponent leftTimeComponent = null;
        [SerializeField] private SpiderMap64 spiderMap64 = null;
        [SerializeField] private ImageFont titleImageFont = null;
        [SerializeField] private BabuButton helpBtn = null;

        [SerializeField] private BabuButton dropdownOpenButton = null;
        [SerializeField] private TMP_Text dropdownOpenLabel = null;
        [SerializeField] private Image dropdownOpenBall = null;
        [SerializeField] private GameObject dropdownPrefab = null;
        [SerializeField] private BabuButton dropdownCancleButton = null;
        [SerializeField] private RectTransform dropdownList = null;

        [SerializeField] private Color mineTopColorText = new();
        [SerializeField] private Color mineDownColorText = new();
        [SerializeField] private Color otherTopColorText = new();
        [SerializeField] private Color otherDownColorText = new();

        [SerializeField] private Color mineTopColorImage = new();
        [SerializeField] private Color mineDownColorImage = new();
        [SerializeField] private Color otherTopColorImage = new();
        [SerializeField] private Color otherDownColorImage = new();

        [SerializeField] private Color normalItemColorBg = new();
        [SerializeField] private Color selectlItemColorBg = new();

        [SerializeField] private BabuButton guessButton = null;
        [SerializeField] private Image dotNodeImgGuess = null;

        protected void OnEnable()
        {
            InitDropdownBox();
            helpBtn.OnClick += OnClickHelpButton;
            formationButton.OnClick += OnClickFormationButton;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
            spiderMap64.OnClickItem += OnClickSpiderItem;
            spiderMap64.OnClickDetail += OnClickSpiderDetail;
            spiderMap64.NeedShowDetail += NeedShowDetail;
            dropdownOpenButton.OnClick += OnClickDropOpen;
            dropdownCancleButton.OnClick += OnClickDropClose;
            dropdownList.gameObject.SetActive(false);
            dropdownCancleButton.gameObject.SetActive(false);
            guessButton.OnClick += OnClickGuessButton;
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
            EventManager.Instance.Register(EventID.AfterHundredGuessSupport, AfterHundredGuessSupport);
            RefreshRedDot(null);
        }
        protected void OnDisable()
        {
            helpBtn.OnClick -= OnClickHelpButton;
            formationButton.OnClick -= OnClickFormationButton;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
            spiderMap64.OnClickItem -= OnClickSpiderItem;
            spiderMap64.OnClickDetail -= OnClickSpiderDetail;
            spiderMap64.NeedShowDetail -= NeedShowDetail;
            dropdownOpenButton.OnClick -= OnClickDropOpen;
            dropdownCancleButton.OnClick -= OnClickDropClose;
            guessButton.OnClick -= OnClickGuessButton;
            EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
            EventManager.Instance.Unregister(EventID.AfterHundredGuessSupport, AfterHundredGuessSupport);
            RemoveTimer();
        }
        private void RefreshRedDot(object[] _)
        {
            RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Hundred, "/Guess");
            redDotNode.IsRed(dotNodeImgGuess.transform);
        }
        private void AfterHundredGuessSupport(object[] args)
        {
            SupportCourseData supportCourseData = args[0] as SupportCourseData;
            bool isSupport = HundredManager.Instance.IsSupported(serverData, supportCourseData);
            if (!isSupport)
            {
                serverData.HundredSupportCourses.Add(supportCourseData);
                RefreshSpider();
            }
        }

        #region dropdown

        private bool isInitdropdown = false;
        private List<HundredFight2DropdownItem> dropdownItemList = new();
        private readonly string newTitle = "新星赛区";
        private readonly string normalTitle = "第{0}赛区";
        private void InitDropdownBox()
        {
            if (isInitdropdown) return;
            isInitdropdown = true;
            dropdownItemList.Clear();
            for (int i = 0; i < 9; i++)
            {
                GameObject dropItemGo = GameObject.Instantiate(dropdownPrefab, dropdownList);
                dropItemGo.SetActive(true);
                HundredFight2DropdownItem item = dropItemGo.GetComponent<HundredFight2DropdownItem>();
                if (i + 1 == 9)
                {
                    item.itemLabel.text = newTitle;
                }
                else
                {
                    item.itemLabel.text = normalTitle.SafeFormat((i + 1).ToChinese());
                }
                item.index = i;
                item.dropdownPrefab.OnClick += OnClickDropDownItem;
                dropdownItemList.Add(item);
            }
        }
        private void OnClickDropOpen(BabuButton itemButton)
        {
            dropdownList.gameObject.SetActive(true);
            dropdownCancleButton.gameObject.SetActive(true);
        }
        private void OnClickDropClose(BabuButton itemButton)
        {
            dropdownList.gameObject.SetActive(false);
            dropdownCancleButton.gameObject.SetActive(false);
        }
        private void OnClickDropDownItem(BabuButton itemButton)
        {
            HundredFight2DropdownItem item = itemButton.GetComponent<HundredFight2DropdownItem>();
            HundredManager.Instance.dropdownValue = item.index;
            OnDropdownValueChanged(item.index);
            dropdownList.gameObject.SetActive(false);
            dropdownCancleButton.gameObject.SetActive(false);
        }
        private void OnDropdownValueChanged(int selectIndex)
        {
            RefreshNowSelect();
        }

        private void RefreshDropdown()
        {
            if (HundredManager.Instance.dropdownValue + 1 == 9)
            {
                dropdownOpenLabel.text = newTitle;
            }
            else
            {
                dropdownOpenLabel.text = normalTitle.SafeFormat((HundredManager.Instance.dropdownValue + 1).ToChinese());
            }
            Debug.Log("RefreshDropdown , dropdownValue = " + HundredManager.Instance.dropdownValue);
            bool isTopMine = HundredManager.Instance.dropdownValue == HundredManager.Instance.MyZoneId - 1;
            dropdownOpenLabel.color = isTopMine ? mineTopColorText : otherTopColorText;
            dropdownOpenBall.color = isTopMine ? mineTopColorImage : otherTopColorImage;
            for (int i = 0; i < 9; i++)
            {
                HundredFight2DropdownItem item = dropdownItemList[i];
                bool isMine = i == HundredManager.Instance.MyZoneId - 1;
                bool isSelect = HundredManager.Instance.dropdownValue == i;
                item.itemBackground.color = isSelect ? selectlItemColorBg : normalItemColorBg;
                item.itemLabel.color = isMine ? mineDownColorText : otherDownColorText;
                item.itemFireImage.color = isMine ? mineDownColorImage : otherDownColorImage;
            }
        }

        #endregion

        private void OnClickSpiderDetail(CupScoreboardPadItem cupScoreboardPadItem)
        {
            var dataRoundList = serverData.LeagueCourseItemList.Where(item => item.Round == cupScoreboardPadItem.lineId - 1).ToList();
            LeagueCourseItemData leagueCourseItemData = dataRoundList[cupScoreboardPadItem.index];
            UIController.Instance.OpenWindow<HundredTeamDetailUI>(new HundredTeamDetailUIProperties(leagueCourseItemData, HundredProgress.Fight2));
        }
        private bool NeedShowDetail(CupScoreboardPadItem cupScoreboardPadItem)
        {
            var dataRoundList = serverData.LeagueCourseItemList.Where(item => item.Round == cupScoreboardPadItem.lineId - 1).ToList();
            if (dataRoundList == null || dataRoundList.Count < 0) return false;
            if (cupScoreboardPadItem.index >= dataRoundList.Count) return false;
            return true;
        }

        private void OnClickSpiderItem(CupScoreboardPadItem item)
        {
            UIController.Instance.OpenWindow<HundredSingleDetailUI>(new HundredSingleDetailUIProperties(item.fightId, item.isAway, CompitionID.Hundred, item.dataProvider.ClubID));
        }

        public void RefreshNowSelect(bool needRefreshData = true)
        {
            InitDropdownBox();
            RefreshDropdown();
            HundredManager.Instance.GetCourse(HundredManager.Instance.dropdownValue + 1, needRefreshData, (GetHundredCourseResponse getHundredCourseResponse) =>
            {
                this.serverData = getHundredCourseResponse;
                CheckStage();
                Refresh(HundredManager.Instance.dropdownValue);
            });
        }
        public void RefreshNowSelect(int PlayoffZoneId)
        {
            Debug.Log("RefreshNowSelect , PlayoffZoneId = {0} , dropdownValue = {1}".SafeFormat(PlayoffZoneId, HundredManager.Instance.dropdownValue));
            RefreshNowSelect(PlayoffZoneId != HundredManager.Instance.dropdownValue + 1);
        }

        GetHundredCourseResponse serverData = null;
        public void OnShow(bool needRefreshData = false)
        {
            InitDropdownBox();
            RefreshDropdown();
            dropdownList.gameObject.SetActive(false);
            dropdownCancleButton.gameObject.SetActive(false);
            HundredManager.Instance.GetCourse(HundredManager.Instance.dropdownValue + 1, needRefreshData, (GetHundredCourseResponse getHundredCourseResponse) =>
            {
                this.serverData = getHundredCourseResponse;
                CheckStage();
                Refresh(getHundredCourseResponse.PlayoffZoneId - 1);
            });
        }
        public void CheckStage()
        {
            switch ((HundredProgress)serverData.Stage)
            {
                case HundredProgress.Fight2:
                    break;
                default:
                    EventManager.Instance.Dispatch(EventID.OnHundredStageMismatch);
                    break;
            }
        }
        private void Refresh(int zoneIndex)
        {
            HundredManager.Instance.dropdownValue = zoneIndex;
            Debug.Log("Refresh , dropdownValue = " + HundredManager.Instance.dropdownValue);
            HundredManager.Instance.SetTitle(titleImageFont, serverData);
            spiderMap64.gameObject.SetActive(true);
            RefreshSpider();
            RefreshLeftTime();
            RefreshShowFormationButton();
            RefreshDropdown();
        }

        private void RefreshShowFormationButton()
        {
            bool isSign = serverData.MyZoneId != 0;
            formationButton.gameObject.SetActive(isSign && serverData.IsOut == false);
        }

        private void OnClickHelpButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<HundredHelpUI>();
        }

        private void OnClickFormationButton(BabuButton _)
        {
            HundredManager.Instance.GetFormationLeftTime(serverData, 6, out HundredFormationUI.HFType hfType, out int leftime);
            UIController.Instance.OpenWindow<HundredFormationUI>(new HundredFormationUIProperties(HundredProgress.Fight2, hfType, leftime));
        }
        private void RefreshLeftTimeOneSec()
        {
            if (leftTime > 0 && needShowTime)
            {
                leftTime--;
                if (leftTime == 0)
                {
                    RefreshNowSelect();
                    EventManager.Instance.Dispatch(EventID.OnHundredNeedRefreshGuess);
                }
                leftTimeComponent.SetLeftTimeText(leftTime);
            }
        }

        private int leftTime = 0;
        private void SetLeftTime(int leftTime)
        {
            this.leftTime = leftTime;
            leftTimeComponent.SetLeftTimeText(leftTime);
        }

        private void RefreshSpider()
        {
            spiderMap64.SetShowDetailButton(true);

            var list = serverData.LeagueCourseItemList.OrderBy(item => item.CourseId);
            var data32 = list.Where(item => item.Round == 1).ToList();
            var data16 = list.Where(item => item.Round == 2).ToList();
            var data8 = list.Where(item => item.Round == 3).ToList();
            var data4 = list.Where(item => item.Round == 4).ToList();
            var data2 = list.Where(item => item.Round == 5).ToList();
            var data1 = list.Where(item => item.Round == 6).ToList();
            var dataProvider = new SpiderMap64Data(data32, data16, data8, data4, data2, data1);

            spiderMap64.SetData(dataProvider, serverData);
        }
        private bool needShowTime = false;
        private bool isEnd = false;
        private Timer refreshDataTimer = null;
        private void RefreshLeftTime()
        {
            needShowTime = false;

            isEnd = false;
            var data1 = serverData.LeagueCourseItemList.Where(item => item.Round == 6).ToList();
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
                        signTimeTipText.text = "距离下场比赛剩余";
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
                        RefreshNowSelect();
                    }, null, true, true);
                }
            }
            else
            {
                needShowTime = true;
                signTimeTipText.text = "冠军赛开始倒计时";
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

        private void OnClickGuessButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<HundredGuessUI>(new HundredGuessUIProperties(false));
        }


    }
}