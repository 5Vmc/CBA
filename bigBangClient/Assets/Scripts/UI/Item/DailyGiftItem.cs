
//SpriteProxy.GetInvetoryQuality(cfg.Quality);

using Babu;
using Babu.SDK;
using GameConfig;
using GameConfig.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class DailyGiftItemData
    {
        public bool isFree = false;
        public GiftShopConfig giftShopConfig = null;
        public BuyRecord buyRecord = new();
        public ActivityData activityData = null;
    }

    public class DailyGiftItem : MonoBehaviour
    {
        [SerializeField] private Image bgImage = null;
        [SerializeField] private Image titleBgImage = null;
        [SerializeField] private BabuButton getButton = null;
        [SerializeField] private BabuButton chargeButton = null;
        [SerializeField] private TMP_Text chargeButtonText = null;
        [SerializeField] private TMP_Text titleText = null;
        [SerializeField] private TMP_Text limitText = null;
        [SerializeField] private TMP_Text descText = null;
        [SerializeField] private List<InventoryItem> rewardsList;
        [SerializeField] private Image hasGetImage = null;
        [SerializeField] private TMP_Text txtRebate;
        [SerializeField] private Image imgRebate;

        private DailyGiftItemData dailyGiftItemData;
        protected void OnEnable()
        {
            getButton.OnClick += OnClickGetButton;
            chargeButton.OnClick += OnClickChargeButton;
        }

        protected void OnDisable()
        {
            getButton.OnClick -= OnClickGetButton;
            chargeButton.OnClick -= OnClickChargeButton;
        }

        private void OnClickGetButton(BabuButton sender)
        {
            if (dailyGiftItemData.isFree)
            {
                if (ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(dailyGiftItemData.activityData.cfg.Id))
                {
                    Tips.PopTips("该奖励已被领取，请明日再来");
                    return;
                }
                NetworkManager.Instance.ReceiveDailyGift(dailyGiftItemData.activityData.cfg.Id, (resp) =>
                {
                    if (resp.ReceiveSucceed == false)
                    {
                        Tips.PopTips("领取失败");
                        Debug.LogWarningFormat("DailyGiftItem , OnClickGetButton , resp.ReceiveSucceed == false , dailyGiftItemData.activityData.cfg.Id = {0}", dailyGiftItemData.activityData.cfg.Id);
                        return;
                    }
                    ActivityController.Instance.dailyGiftReceivedActivityIdSet.Add(dailyGiftItemData.activityData.cfg.Id);
                    var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(dailyGiftItemData.activityData.cfg.DailyGift).ToList());
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                    ActivityController.Instance.RefreshRedDot(dailyGiftItemData.activityData);
                    EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
                });
                return;
            }

            NetworkManager.Instance.ReceivePayMicroReward(dailyGiftItemData.activityData.cfg.Id, dailyGiftItemData.giftShopConfig.Id, (resp) =>
            {
                if (resp.ReceiveSucceed == false)
                {
                    Tips.PopTips("领取失败");
                    Debug.LogWarningFormat("DailyGiftItem , OnClickGetButton , resp.ReceiveSucceed == false , dailyGiftItemData.activityData.cfg.Id = {0} , dailyGiftItemData.giftShopConfig.Id = {1}", dailyGiftItemData.activityData.cfg.Id, dailyGiftItemData.giftShopConfig.Id);
                    return;
                }
                List<GameItem> gameItemsList = GameItemUtils.CreateGameItems(dailyGiftItemData.giftShopConfig.Content).ToList();
                for (int i = 1; i < dailyGiftItemData.buyRecord.receiveCount; i++)
                {
                    gameItemsList.AddRange(gameItemsList);
                }
                dailyGiftItemData.buyRecord.receiveCount = 0;
                var properties = new InventoryObtainedUIProperties(gameItemsList);
                UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                if (dailyGiftItemData.activityData.payData.BuyRecordDict.ContainsKey(dailyGiftItemData.giftShopConfig.Id))
                {
                    dailyGiftItemData.activityData.payData.BuyRecordDict[dailyGiftItemData.giftShopConfig.Id].receiveCount = 0;
                }
                else
                {
                    Debug.LogWarningFormat("DailyGiftItem , OnClickGetButton , dailyGiftItemData.activityData.payData.BuyRecordDict.ContainsKey(dailyGiftItemData.giftShopConfig.Id) == false , dailyGiftItemData.activityData.cfg.Id = {0} , dailyGiftItemData.giftShopConfig.Id = {1}", dailyGiftItemData.activityData.cfg.Id, dailyGiftItemData.giftShopConfig.Id);
                }
                EventManager.Instance.Dispatch(EventID.RefreshWindow, dailyGiftItemData.activityData.cfg.Id);
                ActivityController.Instance.RefreshRedDot(dailyGiftItemData.activityData);
            });
        }
        private void OnClickChargeButton(BabuButton sender)
        {
            bool isLimitByDay = dailyGiftItemData.giftShopConfig.Limit == 0;
            int limitCount = isLimitByDay ? dailyGiftItemData.giftShopConfig.DailyLimit : dailyGiftItemData.giftShopConfig.Limit;
            int nowLimit = dailyGiftItemData.buyRecord.payCount;
            if (nowLimit >= limitCount)
            {
                Tips.PopTips("已达到最大限购数量");
                return;
            }
            PurchaseInfo info = DataConvUtil.NewPurchase(dailyGiftItemData.giftShopConfig.ProductId, dailyGiftItemData.giftShopConfig.Name, dailyGiftItemData.giftShopConfig.Rmb, dailyGiftItemData.giftShopConfig.Id);
#if USER_DEBUG && UNITY_EDITOR
            PurchaseUtil.TestBuyInEditor(info.ShopItemId);
#else
            PurchaseServiceManager.Instance.Purchase(info);
#endif
        }

        public void SetData(DailyGiftItemData dailyGiftItemData, int itemIndex)
        {
            this.dailyGiftItemData = dailyGiftItemData;

            

            if (dailyGiftItemData.isFree)
            {
                limitText.text = "";
                titleText.text = dailyGiftItemData.activityData.cfg.DailyGiftDesc;
                descText.text = "";
                SpriteManager.GetSprite(AtlasNames.Activity, "bgdays", s => titleBgImage.sprite = s);
                ColorUtility.TryParseHtmlString("#fdfad4", out Color color);
                titleText.color = color;
                bool isFreeGet = ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(dailyGiftItemData.activityData.cfg.Id);
                getButton.gameObject.SetActive(!isFreeGet);
                hasGetImage.gameObject.SetActive(isFreeGet);
                chargeButton.gameObject.SetActive(false);
                SetInventory(dailyGiftItemData.activityData.cfg.DailyGift);
                imgRebate.gameObject.SetActive(false);
                return;
            }
            else
            {
                if (dailyGiftItemData.giftShopConfig.Rebate != 0)
                {
                    imgRebate.gameObject.SetActive(true);
                    txtRebate.text = dailyGiftItemData.giftShopConfig.Rebate.ToString() + "%";
                }
                else
                {
                    imgRebate.gameObject.SetActive(false);
                }

                hasGetImage.gameObject.SetActive(false);
                SpriteManager.GetSprite(AtlasNames.Activity, "bgblue", s => titleBgImage.sprite = s);
                ColorUtility.TryParseHtmlString("#184448", out Color color);
                titleText.color = color;
                SetInventory(dailyGiftItemData.giftShopConfig.Content);
            }

            bool isLimitByDay = dailyGiftItemData.giftShopConfig.Limit == 0;
            int limitCount = isLimitByDay ? dailyGiftItemData.giftShopConfig.DailyLimit : dailyGiftItemData.giftShopConfig.Limit;
            int nowLimit = dailyGiftItemData.buyRecord.payCount;
            bool canGet = dailyGiftItemData.buyRecord.receiveCount > 0;

            limitText.text = (isLimitByDay ? "每日" : "") + "限购（{0}/{1}）".SafeFormat(Utility.KeepInRange(nowLimit, 0, limitCount), limitCount);
            titleText.text = dailyGiftItemData.giftShopConfig.Name;
            descText.text = dailyGiftItemData.giftShopConfig.Subtitle;
            chargeButtonText.text = "{0}元".SafeFormat(dailyGiftItemData.giftShopConfig.Rmb);

            if (canGet)
            {
                getButton.gameObject.SetActive(true);
                chargeButton.gameObject.SetActive(false);
                return;
            }
            getButton.gameObject.SetActive(false);
            chargeButton.gameObject.SetActive(true);
        }
        private void SetInventory(string rewardStr)
        {
            List<GameItem> rewardsItems = new();
            if (!string.IsNullOrEmpty(rewardStr)) rewardsItems = GameItemUtils.CreateGameItems(rewardStr).ToList();
            int rewardsItemsCount = rewardsItems.Count;

            for (int index = 0; index < 4; index++)
            {
                if (rewardsItemsCount <= index)
                {
                    rewardsList[index].gameObject.SetActive(false);
                }
                else
                {
                    rewardsList[index].gameObject.SetActive(true);
                    rewardsList[index].SetData(rewardsItems[index]);
                }
            }
        }
    }
}
