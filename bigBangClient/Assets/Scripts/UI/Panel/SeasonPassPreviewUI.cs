using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;
using GameConfig.Config;
using System;
using GameConfig;
using System.Linq;
using Utils.GameItem;
using Babu;

namespace BigBang.UI
{
    public class SeasonPassPreviewUIProperties : WindowProperties
    {
        public ActivityData activityData;
        public SeasonPassPreviewUIProperties(ActivityData activityData)
        {
            this.activityData = activityData;
        }
    }
    public class SeasonPassPreviewUI : AWindowController<SeasonPassPreviewUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private GameItemGridAdapter adapter;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            EventManager.Instance.Register(EventID.RefreshWindow, OnServerPushRefresh);
            buySeasonPassButton.OnClick += OnClickBuySeasonPassButton;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            EventManager.Instance.Unregister(EventID.RefreshWindow, OnServerPushRefresh);
            buySeasonPassButton.OnClick -= OnClickBuySeasonPassButton;
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            int activityId = Properties.activityData.cfg.Id;
            List<ActivityBattlePassRewardConfig> activityBattlePassRewardConfigList = Configs.ActivityBattlePassReward.GetConfigList().Where(cfg => cfg.ActivityId == activityId).OrderBy(cfg => cfg.Id).ToList();
            List<GameItem> gameItemList = new();
            foreach (ActivityBattlePassRewardConfig activityBattlePassRewardConfig in activityBattlePassRewardConfigList)
            {
                if (string.IsNullOrEmpty(activityBattlePassRewardConfig.Rewards2) == false) gameItemList.Add(GameItemUtils.CreateGameItem(activityBattlePassRewardConfig.Rewards2));
                if (string.IsNullOrEmpty(activityBattlePassRewardConfig.RewardsStep) == false) gameItemList.Add(GameItemUtils.CreateGameItem(activityBattlePassRewardConfig.RewardsStep));
            }
            adapter.SetData(gameItemList);
            RefreshInfo();
        }

        private void OnServerPushRefresh(object[] objects)
        {
            if ((int)objects[0] != Properties.activityData.cfg.Id) return;
            RefreshInfo();
        }
        [SerializeField] private TMP_Text costText = null;
        [SerializeField] private TMP_Text unlockTipText = null;
        private void RefreshInfo()
        {
            if (!Properties.activityData.payData.hasBuy)
            {
                GiftShopConfig giftShopConfig = Configs.GiftShop.GetConfigList().FirstOrDefault(cfg => cfg.Type == Properties.activityData.cfg.Id);
                costText.text = "￥{0}".SafeFormat(giftShopConfig.Rmb);
            }
            buySeasonPassButton.gameObject.SetActive(!Properties.activityData.payData.hasBuy);
            unlockTipText.gameObject.SetActive(Properties.activityData.payData.hasBuy);
        }

        private void OnClose()
        {
            UIController.Instance.CloseWindow<SeasonPassPreviewUI>();
        }

        [SerializeField] private BabuButton buySeasonPassButton = null;
        private void OnClickBuySeasonPassButton(BabuButton _)
        {
            ActivityController.Instance.PurchaseSeasonPass(Properties.activityData);
        }
    }
}