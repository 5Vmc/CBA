using Babu;
using BigBang.Animation;
using CBA;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Google.Protobuf.Collections;
using Protocol;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class HundredHomeUIHistoryPad : MonoBehaviour
    {
        [SerializeField] private List<HundredHomeUIHistoryItem> historyItemList = null;
        [SerializeField] private BabuButton leftButton = null;
        [SerializeField] private BabuButton rightButton = null;
        [SerializeField] private DarkLightItem leftDarkLightItem = null;
        [SerializeField] private DarkLightItem rightDarkLightItem = null;
        [SerializeField] private LeftTimeComponent leftTimeComponent = null;
        [SerializeField] private RectTransform bottom = null;
        [SerializeField] private BabuButton helpBtn = null;
        [SerializeField] private ImageFont titleImageFont = null;
        [SerializeField] private BabuButton guessBtn = null;

        protected void OnEnable()
        {
            leftButton.OnClick += OnClickLeftButton;
            rightButton.OnClick += OnClickRightButton;
            helpBtn.OnClick += OnClickHelpButton;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
            guessBtn.OnClick += OnClickGuessButton;
        }

        protected void OnDisable()
        {
            leftButton.OnClick -= OnClickLeftButton;
            rightButton.OnClick -= OnClickRightButton;
            helpBtn.OnClick -= OnClickHelpButton;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
            guessBtn.OnClick -= OnClickGuessButton;
        }

        private void OnClickGuessButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<HundredGuessUI>(new HundredGuessUIProperties(true));
        }

        RepeatedField<string> seasonTitleList = new();
        int index = 0;
        public void OnShow()
        {
            for (int i = 0; i < 8; i++)
            {
                HundredHomeUIHistoryItem hundredHomeUIHistoryItem = historyItemList[i];
                hundredHomeUIHistoryItem.gameObject.SetActive(false);
            }
            bottom.gameObject.SetActive(false);
            leftDarkLightItem.SetLight(false);
            rightDarkLightItem.SetLight(false);
            HundredManager.Instance.GetCourse(0, false, (GetHundredCourseResponse getHundredCourseResponse) =>
            {
                seasonTitleList.Clear();
                seasonTitleList.AddRange(getHundredCourseResponse.SeasonTitles);
                index = seasonTitleList.Count - 2;
                UnityEngine.PlayerPrefs.SetString(PlayerPrefsKeys.HundredShowHistory + Player.GbId, seasonTitleList[index]);
                RefreshButtonState();
                ShowYearSeason();
                bool isWait = (HundredProgress)getHundredCourseResponse.Stage == HundredProgress.Wait;
                if (isWait)
                {
                    leftTime = getHundredCourseResponse.StageEndTime - (int)Utils.DataConvUtil.ServerTime;
                    bottom.gameObject.SetActive(leftTime > 0);
                    SetLeftTime(leftTime);
                }
                else
                {
                    bottom.gameObject.SetActive(false);
                }
            });
        }

        public void ShowYearSeason()
        {
            if (index < 0 || index > seasonTitleList.Count - 2)
            {
                Debug.LogWarning("HundredHomeUIHistoryPad , ShowYearSeason , index < 0 || index > seasonTitleList.Count - 2 , index = " + index + " , seasonTitleList.Count = " + seasonTitleList.Count);
                return;
            }
            HundredManager.Instance.GetYearAndSession(seasonTitleList[index], out int year, out int session);
            if (year == 0 || session == 0)
            {
                titleImageFont.text = "";
                return;
            }
            titleImageFont.text = "{0}第{1}届".SafeFormat(year, session.ToChinese());
            HundredManager.Instance.GetHistory(seasonTitleList[index], (List<CourseTeamData> dataList) =>
            {
                for (int i = 0; i < 8; i++)
                {
                    HundredHomeUIHistoryItem hundredHomeUIHistoryItem = historyItemList[i];
                    CourseTeamData courseTeamData = null;
                    if (i < dataList.Count)
                    {
                        courseTeamData = dataList[i];
                    }
                    hundredHomeUIHistoryItem.SetData(courseTeamData);
                }
                PlayEnterAnim();
            });
        }
        private Sequence seq = null;
        private void PlayEnterAnim()
        {
            seq?.Kill();
            seq = DOTween.Sequence();
            seq.AddTo(this.gameObject);
            for (int i = 0; i < 8; i++)
            {
                HundredHomeUIHistoryItem hundredHomeUIHistoryItem = historyItemList[i];
                hundredHomeUIHistoryItem.transform.SetLocalScale(0);
                seq.Insert(i * 0.1f, hundredHomeUIHistoryItem.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
            }
        }

        private void OnClickLeftButton(BabuButton _)
        {
            if (index - 1 < 0)
            {
                Tips.PopTips("没有更早的记录啦");
                return;
            }
            index = Utility.KeepInRange(index - 1, 0, seasonTitleList.Count - 2);
            RefreshButtonState();
            ShowYearSeason();
        }
        private void OnClickRightButton(BabuButton _)
        {
            if (index + 1 > seasonTitleList.Count - 2)
            {
                Tips.PopTips("没有更晚的记录啦");
                return;
            }
            index = Utility.KeepInRange(index + 1, 0, seasonTitleList.Count - 2);
            RefreshButtonState();
            ShowYearSeason();
        }
        private void RefreshButtonState()
        {
            bool hasData = seasonTitleList != null;
            leftDarkLightItem.SetLight(index > 0 && hasData);
            rightDarkLightItem.SetLight(index < seasonTitleList.Count - 2 && hasData);
        }
        private void OnClickHelpButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<HundredHelpUI>();
        }

        private void RefreshLeftTimeOneSec()
        {
            if (leftTime > 0)
            {
                leftTime--;
                if (leftTime == 0)
                {
                    bottom.gameObject.SetActive(false);
                }
                RefreshTimeText();
            }
        }

        private int leftTime = 0;
        private void SetLeftTime(int leftTime)
        {
            this.leftTime = leftTime;
            RefreshTimeText();
        }
        [SerializeField] private TMP_Text dayTimeTipText = null;
        private void RefreshTimeText()
        {
            int time = leftTime;
            int daySec = 24 * 60 * 60;
            dayTimeTipText.gameObject.SetActive(time > daySec);
            if (leftTime >= daySec)
            {
                dayTimeTipText.text = "{0}天".SafeFormat(time / daySec);
                time -= (time / daySec) * daySec;
            }
            leftTimeComponent.SetLeftTimeText(time);
        }


    }
}