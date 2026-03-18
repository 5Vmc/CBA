using System.Collections.Generic;
using System.Linq;
using Babu;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class SeasonPassPad : MonoBehaviour, IActivity
    {

        #region 初始化

        protected void OnEnable()
        {
            EventManager.Instance.Register(EventID.RefreshWindow, OnServerPushRefresh);
            rewardPreviewButton.OnClick += OnClickRewardPreviewButton;
            buySeasonPassButton.OnClick += OnClickBuySeasonPassButton;
            seasonPassItemAdapter.ScrollPositionChanged += OnSeasonPassItemAdapterScroll;
            EventManager.Instance.Register(EventID.OnSeasonPassItemSetData, OnSeasonPassItemSetData);
            EventManager.Instance.Register(EventID.OnSeasonPassItemGetReward, OnSeasonPassItemGetReward);
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
        }

        protected void OnDisable()
        {
            EventManager.Instance.Unregister(EventID.RefreshWindow, OnServerPushRefresh);
            rewardPreviewButton.OnClick -= OnClickRewardPreviewButton;
            buySeasonPassButton.OnClick -= OnClickBuySeasonPassButton;
            seasonPassItemAdapter.ScrollPositionChanged -= OnSeasonPassItemAdapterScroll;
            EventManager.Instance.Unregister(EventID.OnSeasonPassItemSetData, OnSeasonPassItemSetData);
            EventManager.Instance.Unregister(EventID.OnSeasonPassItemGetReward, OnSeasonPassItemGetReward);
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
            ClearFinger();
        }

        [SerializeField] private List<DailyChargeRewardItem> rewardItemList = null;
        [SerializeField] private TMP_Text txtLeftTime = null;

        private void RefreshLeftTime()
        {
            long leftTime = activityData.EndTime - Utils.DataConvUtil.ServerTime;
            txtLeftTime.text = "活动结束：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
        }

        private ActivityData activityData;
        public void LoadActivity(ActivityData activityData)
        {
            ClearFinger();
            seasonPassItemAdapter.InitAnim();
            this.activityData = activityData;
            GetFingerIndex();
            SetOsaData();
            RefreshInfo();
            ScrollToBest();
            UnityTimer.Timer.Register(this.gameObject, 0.05f, () =>
            {
                RefreshFingerPos();
            });
        }

        [SerializeField] SeasonPassItemAdapter seasonPassItemAdapter = null;
        List<ActivityBattlePassRewardConfig> specialConfigList = new();
        List<ActivityBattlePassRewardConfig> activityBattlePassRewardConfigList = null;
        ActivityBattlePassRewardConfig ActivityBattlePassRewardConfigEnd = null;
        private void SetOsaData()
        {
            activityBattlePassRewardConfigList = Configs.ActivityBattlePassReward.GetConfigList().Where(cfg => cfg.ActivityId == activityData.cfg.Id).OrderBy(cfg => cfg.Id).ToList();
            specialConfigList = activityBattlePassRewardConfigList.Where(cfg => !string.IsNullOrEmpty(cfg.RewardsStep)).OrderBy(cfg => cfg.Id).ToList();
            ActivityBattlePassRewardConfigEnd = activityBattlePassRewardConfigList[^1];
            activityBattlePassRewardConfigList.RemoveAt(activityBattlePassRewardConfigList.Count - 1);
            seasonPassItemAdapter.SetData(activityBattlePassRewardConfigList, activityData);
        }
        [SerializeField] SeasonPassItem bottomSeasonPassItem = null;
        private void OnSeasonPassItemAdapterScroll(double _)
        {
            if (seasonPassItemAdapter.VisibleItemsCount <= 0) return;
            SeasonPassItem endSeasonPassItem = seasonPassItemAdapter.GetItemViewsHolder(seasonPassItemAdapter.VisibleItemsCount - 1).item;
            bool isFind = false;
            foreach (ActivityBattlePassRewardConfig specialConfig in specialConfigList)
            {
                if (endSeasonPassItem.config.Id < specialConfig.Id)
                {
                    isFind = true;
                    if (bottomSeasonPassItem.config != specialConfig)
                    {
                        bottomSeasonPassItem.SetData(specialConfig, -1);
                        if (activityData != null) bottomSeasonPassItem.RefreshState(activityData);
                    }
                    break;
                }
            }
            if (!isFind)
            {
                if (bottomSeasonPassItem.config != ActivityBattlePassRewardConfigEnd)
                {
                    bottomSeasonPassItem.SetData(ActivityBattlePassRewardConfigEnd, -1);
                    if (activityData != null) bottomSeasonPassItem.RefreshState(activityData);
                }
            }
            RefreshFingerPos();
        }

        #endregion

        #region 刷新

        private void OnServerPushRefresh(object[] objects)
        {
            if ((int)objects[0] != activityData.cfg.Id) return;
            RefreshInfo();
        }

        private void RefreshInfo()
        {
            RefreshLevelAndProgress();
            seasonPassItemAdapter.RefreshInfo(activityData);
            bottomSeasonPassItem.RefreshState(activityData);
            RefreshBuyState();
            seasonPassItemAdapter.PlayAnim();
        }
        private void ScrollToBest()
        {
            int targetIndex = -1;
            var RewardsConfigList = activityBattlePassRewardConfigList;
            for (int i = 0; i < RewardsConfigList.Count; i++)
            {
                var config = RewardsConfigList[i];
                int taskPoint = activityData.payData.TaskPoint;
                bool isLockByPurchase = !activityData.payData.hasBuy;
                bool isLockByLevel = taskPoint < config.Option;
                bool freeHasRecieve = activityData.payData.HasReceive(config.Id * 10 + 1);
                bool freeHasGoods = string.IsNullOrEmpty(config.Rewards1) == false;
                bool freeCanGet = !isLockByLevel && !freeHasRecieve && freeHasGoods;
                bool payHasRecieve = activityData.payData.HasReceive(config.Id * 10 + 2);
                bool payHasGoods = string.IsNullOrEmpty(config.Rewards2) == false || string.IsNullOrEmpty(config.RewardsStep) == false;
                bool payCanGet = !isLockByLevel && !payHasRecieve && !isLockByPurchase && payHasGoods;
                if (freeCanGet || payCanGet || isLockByLevel)
                {
                    targetIndex = i;
                    break;
                }
            }
            if (targetIndex == -1)
            {
                targetIndex = Utility.KeepInRange(level - 1, 0, activityBattlePassRewardConfigList.Count - 1);
            }
            targetIndex = Utility.KeepInRange(targetIndex, 0, activityBattlePassRewardConfigList.Count - 1);
            seasonPassItemAdapter.ScrollTo(targetIndex);
        }

        [SerializeField] private TMP_Text hasBuySeasonPassText = null;
        [SerializeField] private Image payLockImage = null;
        [SerializeField] private TMP_Text purchaseCostText = null;
        private void RefreshBuyState()
        {
            purchaseCostText.gameObject.SetActive(!activityData.payData.hasBuy);
            hasBuySeasonPassText.gameObject.SetActive(activityData.payData.hasBuy);
            buySeasonPassButton.gameObject.SetActive(!activityData.payData.hasBuy);
            payLockImage.gameObject.SetActive(!activityData.payData.hasBuy);
            if (!activityData.payData.hasBuy)
            {
                GiftShopConfig giftShopConfig = Configs.GiftShop.GetConfigList().FirstOrDefault(cfg => cfg.Type == activityData.cfg.Id);
                purchaseCostText.text = "{0}元".SafeFormat(giftShopConfig.Rmb);
            }
        }

        [SerializeField] private TMP_Text nowLevelText = null;
        [SerializeField] private Image progressFgImage = null;
        [SerializeField] private TMP_Text progressNumText = null;
        int level = 0;
        private void RefreshLevelAndProgress()
        {
            level = 0;
            List<ActivityBattlePassRewardConfig> activityBattlePassRewardConfigList = Configs.ActivityBattlePassReward.GetConfigList().Where(cfg => cfg.ActivityId == activityData.cfg.Id).OrderBy(cfg => cfg.Id).ToList();
            ActivityBattlePassRewardConfig activityBattlePassRewardConfigNow = null;
            ActivityBattlePassRewardConfig activityBattlePassRewardConfigNext = null;
            for (int i = 0; i < activityBattlePassRewardConfigList.Count; i++)
            {
                ActivityBattlePassRewardConfig activityBattlePassRewardConfig = activityBattlePassRewardConfigList[i];
                if (activityData.payData.TaskPoint >= activityBattlePassRewardConfig.Option)
                {
                    level = activityBattlePassRewardConfig.Id % 100;
                    activityBattlePassRewardConfigNow = activityBattlePassRewardConfig;
                    if (i < activityBattlePassRewardConfigList.Count - 1)
                    {
                        activityBattlePassRewardConfigNext = activityBattlePassRewardConfigList[i + 1];
                    }
                }
                else
                {
                    break;
                }
            }
            if (level == 0)
            {
                activityBattlePassRewardConfigNext = activityBattlePassRewardConfigList[0];
                nowLevelText.text = "0";
                int targetPoint = activityBattlePassRewardConfigNext.Option;
                float progress = activityData.payData.TaskPoint / (float)targetPoint;
                progressFgImage.fillAmount = progress;
                progressNumText.text = "{0}/{1}".SafeFormat(activityData.payData.TaskPoint, targetPoint);
            }
            else
            {
                bool isMax = activityBattlePassRewardConfigNext == activityBattlePassRewardConfigList[^1] && activityData.payData.TaskPoint >= activityBattlePassRewardConfigNext.Option;
                if (isMax)
                {
                    nowLevelText.text = (activityBattlePassRewardConfigNext.Id % 100).ToString();
                    int endPoint = activityBattlePassRewardConfigNext.Option;
                    progressFgImage.fillAmount = 1.0f;
                    int lastPoint = activityBattlePassRewardConfigList[^2].Option;
                    progressNumText.text = "{0}/{1}".SafeFormat(endPoint - lastPoint, endPoint - lastPoint);
                }
                else
                {
                    nowLevelText.text = level.ToString();
                    int targetPoint = activityBattlePassRewardConfigNext.Option;
                    int nowTaskPoint = activityData.payData.TaskPoint - activityBattlePassRewardConfigNow.Option;
                    int nowTargetPoint = targetPoint - activityBattlePassRewardConfigNow.Option;
                    float progress = nowTaskPoint / (float)nowTargetPoint;
                    progressFgImage.fillAmount = progress;
                    progressNumText.text = "{0}/{1}".SafeFormat(nowTaskPoint, nowTargetPoint);
                }
            }
        }

        #endregion

        #region 按钮

        [SerializeField] private BabuButton rewardPreviewButton = null;
        [SerializeField] private BabuButton buySeasonPassButton = null;
        private void OnClickRewardPreviewButton(BabuButton _)
        {
            SeasonPassPreviewUIProperties seasonPassPreviewUIProperties = new SeasonPassPreviewUIProperties(this.activityData);
            seasonPassPreviewUIProperties.MagicTargetTrans = rewardPreviewButton.transform;
            UIController.Instance.OpenWindow<SeasonPassPreviewUI>(seasonPassPreviewUIProperties);
        }

        private void OnClickBuySeasonPassButton(BabuButton _)
        {
            ActivityController.Instance.PurchaseSeasonPass(this.activityData);
        }

        #endregion

        #region 手指提示领取



        private int battlePassRewardListCount = 0;
        private int firstCanGetIndex = -1;
        bool freeCanGet = false;
        bool payCanGet = false;
        private void GetFingerIndex()
        {
            int index = -1;
            var RewardsConfigList = Configs.ActivityBattlePassReward.GetConfigList().FindAll(p => p.ActivityId == activityData.cfg.Id);
            battlePassRewardListCount = RewardsConfigList.Count;
            for (int i = 0; i < RewardsConfigList.Count; i++)
            {
                var config = RewardsConfigList[i];
                int taskPoint = activityData.payData.TaskPoint;
                bool isLockByPurchase = !activityData.payData.hasBuy;
                bool isLockByLevel = taskPoint < config.Option;
                bool freeHasRecieve = activityData.payData.HasReceive(config.Id * 10 + 1);
                bool freeHasGoods = string.IsNullOrEmpty(config.Rewards1) == false;
                freeCanGet = !isLockByLevel && !freeHasRecieve && freeHasGoods;
                bool payHasRecieve = activityData.payData.HasReceive(config.Id * 10 + 2);
                bool payHasGoods = string.IsNullOrEmpty(config.Rewards2) == false || string.IsNullOrEmpty(config.RewardsStep) == false;
                payCanGet = !isLockByLevel && !payHasRecieve && !isLockByPurchase && payHasGoods;
                if (freeCanGet || payCanGet)
                {
                    index = i;
                    break;
                }
                if (isLockByLevel) break;
            }
            firstCanGetIndex = index;
        }

        SeasonPassItem fingerSeasonPassItem = null;
        private void OnSeasonPassItemSetData(object[] args)
        {
            SeasonPassItem seasonPassItem = args[0] as SeasonPassItem;
            if (seasonPassItem == fingerSeasonPassItem && seasonPassItem.index != firstCanGetIndex)
            {
                fingerSeasonPassItem = null;
            }
            if (seasonPassItem.index == firstCanGetIndex)
            {
                fingerSeasonPassItem = seasonPassItem;
            }
        }

        private void OnSeasonPassItemGetReward(object[] args)
        {
            // SeasonPassItem seasonPassItem = args[0] as SeasonPassItem;
            ClearFinger();
        }

        [SerializeField] private RectTransform fingerPanel = null;
        [SerializeField] private RectTransform bottomFingerPanel = null;
        private void RefreshFingerPos()
        {
            if (fingerSeasonPassItem == null || firstCanGetIndex < 0 || fingerSeasonPassItem.gameObject.activeSelf == false)
            {
                fingerPanel.gameObject.SetActive(false);
            }
            else
            {
                fingerPanel.gameObject.SetActive(true);
                if (freeCanGet)
                {
                    fingerPanel.localPosition = Utility.ConvertLocalPosition(fingerSeasonPassItem.freeFingerPos.parent, fingerSeasonPassItem.freeFingerPos.localPosition, fingerPanel.parent);
                }
                else
                {
                    fingerPanel.localPosition = Utility.ConvertLocalPosition(fingerSeasonPassItem.payFingerPos.parent, fingerSeasonPassItem.payFingerPos.localPosition, fingerPanel.parent);
                }
            }

            if (firstCanGetIndex < 0 || bottomSeasonPassItem == null || bottomSeasonPassItem.config == null || firstCanGetIndex != bottomSeasonPassItem.config.Id % 100 - 1)
            {
                bottomFingerPanel.gameObject.SetActive(false);
            }
            else
            {
                bottomFingerPanel.gameObject.SetActive(true);
                if (bottomSeasonPassItem.freeCanGet)
                {
                    bottomFingerPanel.localPosition = Utility.ConvertLocalPosition(bottomSeasonPassItem.freeFingerPos.parent, bottomSeasonPassItem.freeFingerPos.localPosition, bottomFingerPanel.parent);
                }
                else
                {
                    bottomFingerPanel.localPosition = Utility.ConvertLocalPosition(bottomSeasonPassItem.payFingerPos.parent, bottomSeasonPassItem.payFingerPos.localPosition, bottomFingerPanel.parent);
                }
            }
        }

        private void ClearFinger()
        {
            bottomFingerPanel.gameObject.SetActive(false);
            fingerPanel.gameObject.SetActive(false);
            fingerSeasonPassItem = null;
            firstCanGetIndex = -2;
        }


        #endregion

    }
}