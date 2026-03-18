using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameConfig.Config;
using GameConfig;
using static BigBang.AllStarManager;
using Utils.GameItem;

namespace BigBang.UI
{
    public class AllStarHelpUI : AWindowController
    {
        #region 初始化与监听
        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClickClose;
            toggleGroup.OnValueChanged += OnToggleChanged;
        }
        protected override void RemoveListeners()
        {
            closeBtn.OnClick -= OnClickClose;
            toggleGroup.OnValueChanged -= OnToggleChanged;
        }
        [SerializeField] private ScrollRect rewardScrollView = null;
        [SerializeField] private ScrollRect ruleScrollView = null;
        protected override void OnPropertiesSet()
        {
            ruleScrollView.enabled = false;
            rewardScrollView.enabled = false;
            SetReward();
            toggleGroup.Switch(1);
            UnityTimer.Timer.Register(this.gameObject, 0.2f, () =>
            {
                ruleScrollView.verticalNormalizedPosition = 1f;
                ruleScrollView.enabled = true;
                rewardScrollView.verticalNormalizedPosition = 1f;
                rewardScrollView.enabled = true;
            });
        }
        [SerializeField] private BabuButton closeBtn = null;
        private void OnClickClose(BabuButton _)
        {
            UIController.Instance.CloseWindow<AllStarHelpUI>();
        }
        #endregion

        #region 设置奖励

        private void SetReward()
        {
            Set2();
            Set3();
        }
        [SerializeField] private TMP_Text tipText2 = null;
        private readonly string tipText2Str = "选择支持的阵营后，每日可于活动主界面领取每日参与奖。<color=#9D2A2D>奖励：{0}</color>";
        private void Set2()
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.AllStarHome);
            List<GameItem> gameItemList = GameItemUtils.CreateGameItems(activityData.cfg.DailyGift).ToList();
            tipText2.text = tipText2Str.SafeFormat(GameItemUtils.GetNameCountStr(gameItemList));
        }
        [SerializeField] List<InventoryItem> inventoryItemListWin = new();
        [SerializeField] List<InventoryItem> inventoryItemListLose = new();
        [SerializeField] private TMP_Text tipText3 = null;
        private readonly string tipText3Str = "结算领奖阶段，可领取阵营胜负奖励。\n胜方阵营奖励：<color=#9D2A2D>{0}</color>\n胜方阵营奖励：<color=#9D2A2D>{1}</color>";
        private void Set3()
        {
            AllStarRewardConfig allStarRewardConfigWin = Configs.AllStarReward.GetConfigList().FirstOrDefault(cfg => cfg.Type == 1 && cfg.Option == 1);
            AllStarRewardConfig allStarRewardConfigLose = Configs.AllStarReward.GetConfigList().FirstOrDefault(cfg => cfg.Type == 1 && cfg.Option == 2);
            List<GameItem> gameItemListWin = GameItemUtils.CreateGameItems(allStarRewardConfigWin.Rewards).ToList();
            List<GameItem> gameItemListLose = GameItemUtils.CreateGameItems(allStarRewardConfigLose.Rewards).ToList();
            GameItemUtils.SetRewards(inventoryItemListWin, gameItemListWin);
            GameItemUtils.SetRewards(inventoryItemListLose, gameItemListLose);
            tipText3.text = tipText3Str.SafeFormat(GameItemUtils.GetNameCountStr(gameItemListWin), GameItemUtils.GetNameCountStr(gameItemListLose));
        }

        #endregion

        #region 底部页签

        [SerializeField] private BabuToggleGroup toggleGroup;
        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            int selectIndex = toggleGroup.EnableIndex;
            ruleScrollView.verticalNormalizedPosition = 1f;
            rewardScrollView.verticalNormalizedPosition = 1f;
            ruleScrollView.gameObject.SetActive(selectIndex == 1);
            rewardScrollView.gameObject.SetActive(selectIndex == 0);
        }

        #endregion

    }
}