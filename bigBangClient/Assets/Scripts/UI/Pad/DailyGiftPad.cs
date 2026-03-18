using System.Collections.Generic;
using BigBang.Animation;
using deVoid.UIFramework;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Babu;
using GameConfig;
using Utils.GameItem;
using System.Linq;
using GameConfig.Config;
using Utils;
using DG.Tweening;

namespace BigBang.UI
{

    public class DailyGiftPad : MonoBehaviour, IActivity
    {
        [SerializeField] private DailyGiftAdapter adapter;
        [SerializeField] private TMP_Text timeText;
        private ActivityData activityData;

        protected void OnEnable()
        {
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
            EventManager.Instance.Register(EventID.RefreshWindow, OnServerPushRefresh);
        }

        protected void OnDisable()
        {
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
            EventManager.Instance.Unregister(EventID.RefreshWindow, OnServerPushRefresh);
        }

        private void RefreshLeftTime()
        {
            long leftTime = activityData.EndTime - Utils.DataConvUtil.ServerTime;
            timeText.text = "活动结束：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
        }

        private void OnServerPushRefresh(object[] objects)
        {
            if ((int)objects[0] != activityData.cfg.Id) return;
            refreshData();
        }
        private void refreshData()
        {
            List<DailyGiftItemData> dailyGiftItemDataList = new();
            DailyGiftItemData dailyGiftItemDataFree = new();
            dailyGiftItemDataFree.isFree = true;
            dailyGiftItemDataFree.activityData = activityData;
            dailyGiftItemDataList.Add(dailyGiftItemDataFree);
            List<GiftShopConfig> giftShopConfigList = Configs.GiftShop.GetConfigList().Where(cfg => cfg.Type == activityData.cfg.Id).OrderBy(cfg => cfg.Id).ToList();
            foreach (GiftShopConfig giftShopConfig in giftShopConfigList)
            {
                DailyGiftItemData dailyGiftItemData = new();
                dailyGiftItemData.isFree = false;
                dailyGiftItemData.activityData = activityData;
                dailyGiftItemData.giftShopConfig = giftShopConfig;
                if (activityData.payData.BuyRecordDict.ContainsKey(giftShopConfig.Id))
                {
                    dailyGiftItemData.buyRecord = activityData.payData.BuyRecordDict[giftShopConfig.Id];
                }
                else
                {
                    dailyGiftItemData.buyRecord = new();
                }
                dailyGiftItemDataList.Add(dailyGiftItemData);
            }
            adapter.SetData(dailyGiftItemDataList);
        }

        public void LoadActivity(ActivityData activityData)
        {
            this.activityData = activityData;
            refreshData();
        }
    }
}