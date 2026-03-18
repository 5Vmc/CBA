using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class HundredHomeUIProperties : PanelProperties
    {
        public bool isOutOpen = true;
        public HundredHomeUIProperties(bool isOutOpen)
        {
            this.isOutOpen = isOutOpen;
        }
    }

    public class HundredHomeUI : APanelController<HundredHomeUIProperties>
    {
        #region 初始化
        public enum SubUIID
        {
            History = 0,
            Match = 1,
        }

        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClose;
            bottomToggleGroup.OnValueChanged += OnToggleChanged;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
            EventManager.Instance.Register(EventID.OnHundredStageMismatch, OnHundredStageMismatch);
        }
        protected override void RemoveListeners()
        {
            closeBtn.OnClick -= OnClose;
            bottomToggleGroup.OnValueChanged -= OnToggleChanged;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
            EventManager.Instance.Unregister(EventID.OnHundredStageMismatch, OnHundredStageMismatch);
        }

        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private BabuToggleGroup bottomToggleGroup = null;
        [SerializeField] private HundredHomeUIHistoryPad hundredHomeUIHistoryPad = null;
        [SerializeField] private RectTransform matchPanel = null;
        [SerializeField] private HundredHomeUISignPad hundredHomeUISignPad = null;
        [SerializeField] private HundredHomeUIFight1Pad hundredHomeUIFight1Pad = null;
        [SerializeField] private HundredHomeUIFight2Pad hundredHomeUIFight2Pad = null;
        [SerializeField] private HundredHomeUIFight3Pad hundredHomeUIFight3Pad = null;

        int leftRefreshTime = -1;
        protected override void OnPropertiesSet()
        {
            if (Properties.isOutOpen) HundredManager.Instance.dropdownValue = -1;
            RefreshUI(!Properties.isOutOpen);
        }
        private void RefreshLeftTimeOneSec()
        {
            if (leftRefreshTime < 0) return;
            leftRefreshTime--;
            if (leftRefreshTime == 0)
            {
                Properties.isOutOpen = true;
                RefreshUI(true);
            }
        }
        private void OnHundredStageMismatch(object[] args)
        {
            Properties.isOutOpen = true;
            RefreshUI(true);
        }
        [SerializeField] private Toggle historyToggle = null;
        private void RefreshUI(bool useNewData = false)
        {
            AudioManager.Instance.PlaySound(AudioNames.ENT_DETAILS_SHOW_UP);

            SubUIID subUIID = SubUIID.Match;


            HundredManager.Instance.GetCourse(HundredManager.Instance.dropdownValue + 1, useNewData, (GetHundredCourseResponse getHundredCourseResponse) =>
            {
                if ((HundredProgress)getHundredCourseResponse.Stage == HundredProgress.NotOpen)
                {
                    Tips.PopTips("活动未开启");
                    UIController.Instance.HidePanel<HundredHomeUI>();
                    return;
                }

                leftRefreshTime = (int)(getHundredCourseResponse.StageEndTime - Utils.DataConvUtil.ServerTime) + 10;

                historyToggle.gameObject.SetActive(getHundredCourseResponse.SeasonTitles.Count >= 2);
                if (getHundredCourseResponse.Stage == (int)HundredProgress.Wait)
                {
                    HundredManager.Instance.GetYearAndSession(getHundredCourseResponse.SeasonTitles[getHundredCourseResponse.SeasonId - 1], out int year, out int session);
                    if (UnityEngine.PlayerPrefs.GetString(PlayerPrefsKeys.HundredShowHistory + Player.GbId, "") != "{0},{1}".SafeFormat(year, session))
                    {
                        if (getHundredCourseResponse.SeasonTitles.Count >= 2)
                        {
                            subUIID = SubUIID.History;
                        }
                    }
                }

                bottomToggleGroup.Switch((int)subUIID);
                if (getHundredCourseResponse.Stage == (int)HundredProgress.Fight2 || getHundredCourseResponse.Stage == (int)HundredProgress.Fight3)
                {
                    EventManager.Instance.Dispatch(EventID.OnHundredNeedRefreshGuess);
                }
                else
                {
                    EventManager.Instance.Dispatch(EventID.OnHundredNeedCloseGuess);
                }
            });
        }
        #endregion

        #region 关闭界面
        private void OnClose(BabuButton _)
        {
            UIController.Instance.HidePanel<HundredHomeUI>();
        }
        #endregion

        #region 切换页签

        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            int selectedIndex = bottomToggleGroup.EnableIndex;
            ShowPad((SubUIID)selectedIndex);
        }
        private void ShowPad(SubUIID padIndex)
        {
            HideAllPad();
            switch (padIndex)
            {
                case SubUIID.History: OnShowHistory(); break;
                case SubUIID.Match: OnShowMatch(); break;
            }
        }
        private void HideAllPad()
        {
            hundredHomeUIHistoryPad.gameObject.SetActive(false);
            matchPanel.gameObject.SetActive(false);
            hundredHomeUISignPad.gameObject.SetActive(false);
            hundredHomeUIFight1Pad.gameObject.SetActive(false);
            hundredHomeUIFight2Pad.gameObject.SetActive(false);
            hundredHomeUIFight3Pad.gameObject.SetActive(false);
        }

        private void OnShowHistory()
        {
            hundredHomeUIHistoryPad.gameObject.SetActive(true);
            hundredHomeUIHistoryPad.OnShow();
        }
        private void OnShowMatch()
        {
            HundredManager.Instance.GetCourse(HundredManager.Instance.dropdownValue + 1, false, (GetHundredCourseResponse getHundredCourseResponse) =>
            {
                matchPanel.gameObject.SetActive(true);
                switch ((HundredProgress)getHundredCourseResponse.Stage)
                {
                    case HundredProgress.Sign:
                    case HundredProgress.Wait:
                        hundredHomeUISignPad.gameObject.SetActive(true);
                        hundredHomeUISignPad.OnShow(false);
                        break;
                    case HundredProgress.Fight1:
                        if (getHundredCourseResponse.MyZoneId > 0)
                        {
                            hundredHomeUIFight1Pad.gameObject.SetActive(true);
                            hundredHomeUIFight1Pad.OnShow();
                        }
                        else
                        {
                            hundredHomeUISignPad.gameObject.SetActive(true);
                            hundredHomeUISignPad.OnShow(false);
                        }
                        break;
                    case HundredProgress.Fight2:
                        hundredHomeUIFight2Pad.gameObject.SetActive(true);
                        if (Properties.isOutOpen)
                        {
                            hundredHomeUIFight2Pad.OnShow();
                        }
                        else
                        {
                            hundredHomeUIFight2Pad.RefreshNowSelect(getHundredCourseResponse.PlayoffZoneId);
                        }
                        break;
                    case HundredProgress.Fight3:
                        hundredHomeUIFight3Pad.gameObject.SetActive(true);
                        hundredHomeUIFight3Pad.OnShow();
                        break;
                }
                Properties.isOutOpen = false;
            });
        }

        #endregion
    }
}
