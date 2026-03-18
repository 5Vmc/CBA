using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System;
using System.Collections.Generic;
using Protocol;
using System.Linq;
using Babu;
using static BigBang.DragonBoatFestivalManager;

namespace BigBang.UI
{
    public class DragonBoatFestivalHomeTaskUI : AWindowController
    {
        [SerializeField] private BabuButton closeButton = null;
        //[SerializeField] private ScrollRect tipScrollView = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeButton.OnClick += OnClickCloseButton;
            EventManager.Instance.Register(EventID.OnFestivalTaskDataChange, OnFestivalTaskDataChange);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeButton.OnClick -= OnClickCloseButton;
            EventManager.Instance.Unregister(EventID.OnFestivalTaskDataChange, OnFestivalTaskDataChange);
        }


        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            //tipScrollView.enabled = false;
            //UnityTimer.Timer.Register(this.gameObject, 0.2f, () =>
            //{
            //    tipScrollView.enabled = true;
            //    tipScrollView.ScroolToTop(0);
            //});
            HideAll();
            RefreshPanel();
        }

        [SerializeField] private RectTransform normalPanel = null;
        [SerializeField] private RectTransform waitTeamPanel = null;
        [SerializeField] private RectTransform closedPanel = null;
        private void HideAll()
        {
            normalPanel.gameObject.SetActive(false);
            waitTeamPanel.gameObject.SetActive(false);
            closedPanel.gameObject.SetActive(false);
        }
        private void RefreshPanel()
        {
            Stage stage = DragonBoatFestivalManager.Instance.GetStage();
            if (stage == Stage.NotOpen || stage == Stage.CanSelectTeam)
            {
                waitTeamPanel.gameObject.SetActive(true);
            }
            else if (stage == Stage.Ending || stage == Stage.Closed)
            {
                closedPanel.gameObject.SetActive(true);
            }
            else
            {
                normalPanel.gameObject.SetActive(true);
                RefreshTaskList();
            }
        }

        private void OnClickCloseButton(BabuButton _)
        {
            UIController.Instance.CloseWindow<DragonBoatFestivalHomeTaskUI>();
        }

        private void OnFestivalTaskDataChange(object[] args)
        {
            RefreshTaskList();
        }
        [SerializeField] private DragonBoatFestivalHomeTaskAdapter dragonBoatFestivalHomeTaskAdapter = null;
        private void RefreshTaskList()
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalTask);
            if (activityData == null) return;
            List<FestivalTaskInfo> festivalTaskInfoList = ActivityController.Instance.GetFestivalTaskInfoList(activityData.cfg.Id).OrderBy(a => a.Id).ToList();
            if (festivalTaskInfoList == null || festivalTaskInfoList.Count <= 0)
            {
                Debug.LogWarning("DragonBoatFestivalHomeTaskUI , RefreshTaskList ,  festivalTaskInfoList.Count <= 0 , ActivityID.NewYearTask = " + ActivityID.NewYearTask);
            }
            dragonBoatFestivalHomeTaskAdapter.SetData(festivalTaskInfoList);
        }
    }
}