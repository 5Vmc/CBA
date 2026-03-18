using System.Linq;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class DailyChargeRewardItem : MonoBehaviour
    {
        [SerializeField] private Image dayBgImageNormal = null;
        [SerializeField] private TMP_Text dayTextNormal = null;
        [SerializeField] private Image dayBgImageNow = null;
        [SerializeField] private TMP_Text dayTextNow = null;
        [SerializeField] private InventoryItem inventoryItem = null;
        [SerializeField] private Image rewardGetImage = null;
        [SerializeField] private Image lockImage = null;

        public ActivityPayDailyRewardConfig activityPayDailyRewardConfig;
        public void SetData(ActivityPayDailyRewardConfig activityPayDailyRewardConfig)
        {
            this.activityPayDailyRewardConfig = activityPayDailyRewardConfig;

            SetRewardItem();
        }

        private void SetRewardItem()
        {
            GameItem gameItem = GameItemUtils.CreateGameItem(activityPayDailyRewardConfig.Rewards);
            inventoryItem.SetData(gameItem);
        }

        private ActivityPayInfoData activityPayInfoData;
        private bool isTodayChargeEnough = false;
        public bool hasGetReward = false;
        public bool isLock = false;
        public void RefreshInfo(ActivityPayInfoData activityPayInfoData, bool isTodayChargeEnough)
        {
            this.activityPayInfoData = activityPayInfoData;
            this.isTodayChargeEnough = isTodayChargeEnough;
            RefreshDayNowItem();
            RefreshReward();
        }
        private void RefreshDayNowItem()
        {
            bool isNowDay = activityPayInfoData.Days + 1 + (isTodayChargeEnough ? -1 : 0) == activityPayDailyRewardConfig.Option;
            dayBgImageNormal.gameObject.SetActive(!isNowDay);
            dayBgImageNow.gameObject.SetActive(isNowDay);
        }
        private void RefreshReward()
        {
            hasGetReward = activityPayInfoData.HasReceive(activityPayDailyRewardConfig.Id);
            isLock = activityPayInfoData.Days < activityPayDailyRewardConfig.Option;
            if (isLock)
            {
                inventoryItem.SetBlack(true);
                rewardGetImage.gameObject.SetActive(false);
                lockImage.gameObject.SetActive(true);
                return;
            }
            if (hasGetReward)
            {
                inventoryItem.SetBlack(true);
                rewardGetImage.gameObject.SetActive(true);
                lockImage.gameObject.SetActive(false);
                return;
            }
            inventoryItem.SetBlack(false);
            rewardGetImage.gameObject.SetActive(false);
            lockImage.gameObject.SetActive(false);
        }

    }
}