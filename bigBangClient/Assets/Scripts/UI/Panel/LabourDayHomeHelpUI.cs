using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System;
using System.Collections.Generic;
using Utils.GameItem;
using GameConfig.Config;
using GameConfig;
using System.Linq;

namespace BigBang.UI
{
    public class LabourDayHomeHelpUI : AWindowController
    {
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private List<GameObject> panelList = new();

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
            toggleGroup.OnValueChanged += OnToggleChanged;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
            toggleGroup.OnValueChanged -= OnToggleChanged;
        }

        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            int selectIndex = toggleGroup.EnableIndex;
            for (int i = 0; i < panelList.Count; i++)
            {
                panelList[i].SetActive(i == selectIndex);
            }
            if (selectIndex == 1)
            {
                List<GameItem> gameItemList = getRewardList();
                SetReward(gameItemList);
            }
        }

        [SerializeField] private GameObject rewardHistoryItemPrefab = null;
        [SerializeField] private RectTransform rewardHistoryItemRoot = null;
        private List<RewardHistoryItem> rewardHistoryItemList = new();
        private List<GameItem> getRewardList()
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.LabourDayHome);
            int activityId = activityData.cfg.Id;
            List<FestivalTravelConfig> festivalTravelConfigList = Configs.FestivalTravel.GetConfigList().Where(f => f.ActivityId == activityId).ToList();
            List<GameItem> gameItemList = new List<GameItem>();
            foreach (FestivalTravelConfig festivalTravelConfig in festivalTravelConfigList)
            {
                GameItem gameItem = GameItemUtils.CreateGameItem(festivalTravelConfig.Reward);
                if (festivalTravelConfig.Order > LabourDayManager.Instance.serverOrder) gameItem.Count = 0;
                gameItemList.Add(gameItem);
                gameItemList = GameItemUtils.MergeGameItemList(gameItemList);
            }
            gameItemList.OrderBy(g => g.Count > 0);
            return gameItemList;
        }
        private void SetReward(List<GameItem> gameItemList)
        {
            int needItem = gameItemList.Count - rewardHistoryItemList.Count;
            if (needItem > 0)
            {
                for (int i = 0; i < needItem; i++)
                {
                    GameObject itemGo = GameObject.Instantiate(rewardHistoryItemPrefab, rewardHistoryItemRoot);
                    RewardHistoryItem item = itemGo.GetComponent<RewardHistoryItem>();
                    rewardHistoryItemList.Add(item);
                }
            }
            for (int i = 0; i < Mathf.Max(gameItemList.Count, rewardHistoryItemList.Count); i++)
            {
                RewardHistoryItem rewardHistoryItem = rewardHistoryItemList[i];
                if (i >= gameItemList.Count)
                {
                    rewardHistoryItem.gameObject.SetActive(false);
                    continue;
                }
                rewardHistoryItem.gameObject.SetActive(true);
                rewardHistoryItem.SetData(gameItemList[i]);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(rewardHistoryItemRoot as RectTransform);
        }


        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            toggleGroup.Switch(0);
        }

        private void OnClose(BabuButton _)
        {
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);

            UIController.Instance.CloseWindow<LabourDayHomeHelpUI>();
        }
    }
}