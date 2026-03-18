using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.Animation;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class DailyChargePad : MonoBehaviour, IActivity
    {

        #region 初始化

        [SerializeField] private TMP_Text timeText = null;
        [SerializeField] private Image bigBoxLightImage = null;
        [SerializeField] private BabuButton bigBoxCloseButton = null;
        [SerializeField] private BabuButton bigBoxOpenButton = null;
        [SerializeField] private Image freeBoxBgImageLight = null;
        [SerializeField] private TMP_Text freeTipText = null;
        [SerializeField] private BabuButton freeBoxButton = null;
        [SerializeField] private RectTransform buttonStarParticle = null;
        [SerializeField] private TMP_Text totalDayText = null;
        [SerializeField] private Image progressFgImage = null;
        [SerializeField] private HorizontalLayoutGroup rewardLayout = null;
        [SerializeField] private BabuButton gotoChargeButton = null;
        [SerializeField] private TMP_Text chargeNeedInfoText = null;
        [SerializeField] private BabuButton getRewardButton = null;
        [SerializeField] private TMP_Text finishTipText = null;

        protected void OnEnable()
        {
            EventManager.Instance.Register(EventID.RefreshWindow, OnServerPushRefresh);
            bigBoxCloseButton.OnClick += OnClickBigBoxCloseButton;
            bigBoxOpenButton.OnClick += OnClickBigBoxOpenButton;
            freeBoxButton.OnClick += OnClickFreeBoxButton;
            gotoChargeButton.OnClick += OnClickGotoChargeButton;
            getRewardButton.OnClick += OnClickGetRewardButton;
            closeTipButton.OnClick += OnClickCloseTipButton;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
        }

        protected void OnDisable()
        {
            EventManager.Instance.Unregister(EventID.RefreshWindow, OnServerPushRefresh);
            bigBoxCloseButton.OnClick -= OnClickBigBoxCloseButton;
            bigBoxOpenButton.OnClick -= OnClickBigBoxOpenButton;
            freeBoxButton.OnClick -= OnClickFreeBoxButton;
            gotoChargeButton.OnClick -= OnClickGotoChargeButton;
            getRewardButton.OnClick -= OnClickGetRewardButton;
            closeTipButton.OnClick -= OnClickCloseTipButton;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
        }

        [SerializeField] private List<DailyChargeRewardItem> rewardItemList = null;
        private ActivityData activityData;
        public void LoadActivity(ActivityData activityData)
        {
            this.activityData = activityData;

            GetBoxConfig();
            SetRewards();
            OnClickCloseTipButton(null);
            RefreshInfo();
        }
        private ActivityPayDailyRewardConfig bigBoxConfig;
        private List<ActivityPayDailyRewardConfig> normalBoxConfigList = new();
        private void GetBoxConfig()
        {
            List<ActivityPayDailyRewardConfig> rewardConfigList = Configs.ActivityPayDailyReward.GetConfigList().Where(config => config.ActivityId == activityData.cfg.Id).ToList();
            bigBoxConfig = rewardConfigList.FirstOrDefault(config => config.Type == 1);
            normalBoxConfigList = rewardConfigList.Where(config => config.Type == 0).OrderBy(config => config.Option).ToList();
        }
        private void SetRewards()
        {
            for (int i = 0; i < rewardItemList.Count; i++)
            {
                DailyChargeRewardItem dailyChargeRewardItem = rewardItemList[i];
                ActivityPayDailyRewardConfig activityPayDailyRewardConfig = normalBoxConfigList[i];
                dailyChargeRewardItem.SetData(activityPayDailyRewardConfig);
            }
        }

        #endregion

        #region 刷新

        private void OnServerPushRefresh(object[] objects)
        {
            if ((int)objects[0] != activityData.cfg.Id) return;
            RefreshInfo();
        }
        private bool isTodayChargeEnough = false;
        private void RefreshInfo()
        {
            isTodayChargeEnough = activityData.payData.TodayPay >= activityData.cfg.Param1;

            RefreshLeftTime();
            RefreshFreeBox();
            RefreshBigBox();
            RefreshTotalDay();
            RefreshRewards();
            RefreshProgress();
        }

        private void RefreshLeftTime()
        {
            long leftTime = activityData.EndTime - Utils.DataConvUtil.ServerTime;
            timeText.text = "活动结束：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
        }
        private void RefreshFreeBox()
        {
            bool hasGetFreeBox = ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(activityData.cfg.Id);
            freeTipText.text = hasGetFreeBox ? "已领取" : "每日免费";
            freeBoxBgImageLight.gameObject.SetActive(!hasGetFreeBox);
            if (hasGetFreeBox)
            {
                freeBoxButton.gameObject.DOKill();
            }
            else
            {
                freeBoxButton.gameObject.DOShake();
            }
        }
        private void RefreshBigBox()
        {
            bigBoxCloseButton.gameObject.DOKill();
            bigBoxCloseButton.gameObject.SetActive(false);
            bigBoxOpenButton.gameObject.SetActive(false);
            bigBoxLightImage.gameObject.SetActive(false);
            bool hasGetBigBox = activityData.payData.HasReceive(bigBoxConfig.Id);
            bigBoxOpenButton.gameObject.SetActive(hasGetBigBox);
            bigBoxCloseButton.gameObject.SetActive(!hasGetBigBox);
            if (hasGetBigBox) return;
            bool isBoxCanGet = activityData.payData.Days >= normalBoxConfigList.Count;
            bigBoxCloseButton.gameObject.SetActive(true);
            if (isBoxCanGet)
            {
                bigBoxLightImage.gameObject.SetActive(true);
                bigBoxCloseButton.gameObject.DOShake();
            }
        }
        private void RefreshTotalDay()
        {
            totalDayText.text = "已累计{0}天".SafeFormat(activityData.payData.Days);
        }
        List<ActivityPayDailyRewardConfig> canGetRewardConfigList = new();
        private void RefreshRewards()
        {
            canGetRewardConfigList.Clear();
            bool isAllNormalRewardGet = true;
            foreach (DailyChargeRewardItem rewardItem in rewardItemList)
            {
                rewardItem.RefreshInfo(activityData.payData, isTodayChargeEnough);
                if (rewardItem.isLock == false && rewardItem.hasGetReward == false) canGetRewardConfigList.Add(rewardItem.activityPayDailyRewardConfig);
                if (rewardItem.hasGetReward == false) isAllNormalRewardGet = false;
            }
            bool isCanGet = canGetRewardConfigList.Count > 0;
            gotoChargeButton.gameObject.SetActive(false);
            chargeNeedInfoText.gameObject.SetActive(false);
            getRewardButton.gameObject.SetActive(false);
            finishTipText.gameObject.SetActive(false);
            if (isCanGet)
            {
                getRewardButton.gameObject.SetActive(true);
                return;
            }
            if (isTodayChargeEnough)
            {
                finishTipText.gameObject.SetActive(true);
                return;
            }
            if (isAllNormalRewardGet == true)
            {
                return;
            }
            gotoChargeButton.gameObject.SetActive(true);
            chargeNeedInfoText.gameObject.SetActive(true);
            chargeNeedInfoText.text = "再充值{0}元可领取".SafeFormat(activityData.cfg.Param1 - activityData.payData.TodayPay);
        }
        private void RefreshProgress()
        {
            progressFgImage.fillAmount = activityData.payData.Days / (float)rewardItemList.Count;
        }

        #endregion

        #region 按钮
        private void OnClickBigBoxCloseButton(BabuButton _)
        {
            bool isBoxCanGet = activityData.payData.Days >= normalBoxConfigList.Count;
            if (isBoxCanGet)
            {
                ActivityController.Instance.GetRewards(activityData.cfg.Id, bigBoxConfig.Id, () =>
                {
                    activityData.payData.AddReceive(bigBoxConfig.Id);
                    var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(bigBoxConfig.Rewards).ToList());
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                });
            }
            else
            {
                SetBigBoxContent();
            }
        }

        private const int MAX_REWARD_ITEM = 3;// 最大奖励数量
        [SerializeField] private Image bigBoxObtain = null;
        [SerializeField] private List<InventoryItem> bigBoxInventoryItemList = new();
        [SerializeField] private BabuButton closeTipButton = null;
        private void SetBigBoxContent()
        {
            closeTipButton.gameObject.SetActive(true);
            bigBoxObtain.gameObject.SetActive(true);
            var gameItems = GameItemUtils.CreateGameItems(bigBoxConfig.Rewards).ToArray();
            for (int i = 0; i < MAX_REWARD_ITEM; i++)
            {
                if (i < gameItems.Length)
                {
                    bigBoxInventoryItemList[i].gameObject.SetActive(true);
                    bigBoxInventoryItemList[i].SetData(gameItems[i]);
                }
                else
                {
                    bigBoxInventoryItemList[i].gameObject.SetActive(false);
                }
            }
        }
        private void OnClickCloseTipButton(BabuButton _)
        {
            closeTipButton.gameObject.SetActive(false);
            bigBoxObtain.gameObject.SetActive(false);
        }

        private void OnClickBigBoxOpenButton(BabuButton _)
        {
            Tips.PopTips("奖励已领取");
        }
        private void OnClickFreeBoxButton(BabuButton _)
        {
            if (ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(activityData.cfg.Id))
            {
                Tips.PopTips("该奖励已被领取，请明日再来");
                return;
            }
            NetworkManager.Instance.ReceiveDailyGift(activityData.cfg.Id, (resp) =>
            {
                if (resp.ReceiveSucceed == false)
                {
                    Tips.PopTips("领取失败");
                    Debug.LogWarningFormat("DailyGiftItem , OnClickGetButton , resp.ReceiveSucceed == false , activityData.cfg.Id = {0}", activityData.cfg.Id);
                    return;
                }
                ActivityController.Instance.dailyGiftReceivedActivityIdSet.Add(activityData.cfg.Id);
                var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(activityData.cfg.DailyGift).ToList());
                UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                ActivityController.Instance.RefreshRedDot(activityData);
                EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
            });
            return;
        }
        private void OnClickGotoChargeButton(BabuButton _)
        {
            TriggerManager.Instance.JumpPanel(TriggerModuleType.Shop_diamond);
        }
        private void OnClickGetRewardButton(BabuButton _)
        {
            List<int> canGetRewardIdList = new();
            List<GameItem> canGetGameItemList = new();
            foreach (var config in canGetRewardConfigList)
            {
                canGetRewardIdList.Add(config.Id);
                GameItem gameItem = GameItemUtils.CreateGameItem(config.Rewards);
                canGetGameItemList.Add(gameItem);
            }
            ActivityController.Instance.GetRewards(activityData.cfg.Id, canGetRewardIdList, () =>
            {
                activityData.payData.AddReceive(canGetRewardIdList);
                var properties = new InventoryObtainedUIProperties(canGetGameItemList);
                UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
            });
        }

        #endregion

    }
}