using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System;
using System.Collections.Generic;
using GameConfig.Config;
using Utils.GameItem;
using Protocol;
using GameItem = Utils.GameItem.GameItem;
using Babu;
using GameConfig;

namespace BigBang.UI
{
    public class ChristmasTaskUI : AWindowController
    {
        [SerializeField] private Button closeBtn = null;
        [SerializeField] private TMP_Text batteryNumText = null;
        [SerializeField] private List<ChristmasTaskItem> christmasTaskItemList = null;
        [SerializeField] private BabuButton tipPanel = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            EventManager.Instance.Register(EventID.OnRefreshGoods, RefreshBatteryCount);
            EventManager.Instance.Register(EventID.OnFestivalTaskDataChange, RefreshItemList);
            tipPanel.OnClick += OnClickTipPanel;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            EventManager.Instance.Unregister(EventID.OnRefreshGoods, RefreshBatteryCount);
            EventManager.Instance.Unregister(EventID.OnFestivalTaskDataChange, RefreshItemList);
            tipPanel.OnClick -= OnClickTipPanel;
        }

        private void OnClickTipPanel(BabuButton button)
        {
            GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, festivalBoxConfig.KeyId, 0);
            UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(gameItem));
        }

        private void OnClose()
        {
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);

            UIController.Instance.CloseWindow<ChristmasTaskUI>();
        }

        protected override void OnPropertiesSet()
        {
            RefreshActivityData();
            RefreshBatteryCount(null);
            RefreshItemList(null);
        }

        private ActivityData activityData = null;
        private FestivalBoxConfig festivalBoxConfig = null;
        private void RefreshActivityData()
        {
            if (ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.ChristmasTask) == false)
            {
                Debug.LogWarning("ChristmasTaskUI , RefreshActivityData , ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.ChristmasTask) == false");
                return;
            }
            activityData = ActivityController.Instance.OnlineActivityDic[ActivityID.ChristmasTask];
            festivalBoxConfig = Configs.FestivalBox.GetConfig(ActivityID.ChristmasTree);
            if (festivalBoxConfig == null)
            {
                Debug.LogError("ChristmasTaskUI , RefreshTreeItem , festivalBoxConfig == null , ActivityID.ChristmasTree = {0}".SafeFormat(ActivityID.ChristmasTree));
                return;
            }
        }

        private void RefreshBatteryCount(object[] _)
        {
            GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, festivalBoxConfig.KeyId, 0);
            batteryNumText.text = gameItem.GetPlayerCount().ToString();
        }

        private void RefreshItemList(object[] _)
        {
            List<FestivalTaskInfo> festivalTaskInfoList = ActivityController.Instance.GetFestivalTaskInfoList(festivalBoxConfig.FestivalTaskActivity);
            for (int i = 0; i < 4; i++)
            {
                ChristmasTaskItem christmasTaskItem = christmasTaskItemList[i];
                if (i == 0)
                {
                    christmasTaskItem.SetData(activityData, null, 0);
                    christmasTaskItem.gameObject.SetActive(true);
                }
                else
                {
                    if (i - 1 >= festivalTaskInfoList.Count)
                    {
                        christmasTaskItem.gameObject.SetActive(false);
                        Debug.LogWarning("ChristmasTaskUI , RefreshItemList , i - 1 >= festivalTaskInfoList.Count , i - 1 = {0} , festivalTaskInfoList.Count = {1}".SafeFormat(i - 1, festivalTaskInfoList.Count));
                        continue;
                    }
                    FestivalTaskInfo festivalTaskInfo = festivalTaskInfoList[i - 1];
                    christmasTaskItem.SetData(activityData, festivalTaskInfo, i);
                    christmasTaskItem.gameObject.SetActive(true);
                }
            }
        }


    }
}