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
using GameConfig;
using GameConfig.Config;
using static BigBang.DragonBoatFestivalManager;

namespace BigBang.UI
{
    public class DragonBoatFestivalProgressUI : AWindowController
    {
        [SerializeField] private BabuButton closeButton = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeButton.OnClick += OnClickCloseButton;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeButton.OnClick -= OnClickCloseButton;
        }


        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
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
            else if (stage == Stage.Closed)
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
            UIController.Instance.CloseWindow<DragonBoatFestivalProgressUI>();
        }

        [SerializeField] private DragonBoatFestivalProgressAdapter dragonBoatFestivalProgressAdapter = null;
        private void RefreshTaskList()
        {
            List<DragonBoatRewardConfig> dragonBoatRewardConfigList = Configs.DragonBoatReward.GetConfigList().Where(t => t.Type == 2).ToList();
            if (dragonBoatRewardConfigList == null || dragonBoatRewardConfigList.Count <= 0)
            {
                Debug.LogWarning("DragonBoatFestivalProgressUI , RefreshTaskList ,  dragonBoatRewardConfigList.Count <= 0 , here(t => t.Type == 2) ");
            }
            dragonBoatFestivalProgressAdapter.SetData(dragonBoatRewardConfigList);
        }

    }
}