using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using Babu.SDK;
using BigBang;
using BigBang.UI;
using Coffee.UIEffects;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

public class NewYearGiftItem : MonoBehaviour
{
    [SerializeField] private RectTransform newYearGiftItem = null;
    [SerializeField] private Image bgImage = null;
    [SerializeField] private Image bgSpecialImage = null;
    [SerializeField] private BabuButton getBtn = null;
    [SerializeField] private TMP_Text getText = null;
    [SerializeField] private BabuButton waitBtn = null;
    [SerializeField] private TMP_Text waitText = null;
    [SerializeField] private TMP_Text titleText = null;
    [SerializeField] private TMP_Text limitText = null;
    [SerializeField] private List<InventoryItem> inventoryItemList = null;
    [SerializeField] private UIShiny getBtnUIShiny = null;

    [SerializeField] private Color normalTitleColor = new();
    [SerializeField] private Color specialTitleColor = new();

    private void OnEnable()
    {
        waitBtn.OnClick += OnClickWaitBtn;
        getBtn.OnClick += OnClickGetBtn;
        EventManager.Instance.Register(EventID.OnRefreshGiftShop, OnRefreshGiftShop);
    }
    private void OnDisable()
    {
        waitBtn.OnClick -= OnClickWaitBtn;
        getBtn.OnClick -= OnClickGetBtn;
        EventManager.Instance.Unregister(EventID.OnRefreshGiftShop, OnRefreshGiftShop);
    }
    private void OnRefreshGiftShop(object[] objs)
    {
        if (giftShopConfig == null) return;
        if (objs != null && objs.Length > 0)
        {
            int itemId = (int)(objs[0]);
            if (itemId == giftShopConfig.Id) RefreshDataNormal();
        }
    }

    public GiftShopConfig giftShopConfig = null;
    public void SetData(GiftShopConfig giftShopConfig)
    {
        this.giftShopConfig = giftShopConfig;
        activityData = null;
        RefreshDataNormal();
    }

    public ActivityData activityData = null;
    public void SetData(ActivityData ActivityData)
    {
        this.activityData = ActivityData;
        giftShopConfig = null;
        RefreshDataActivity();
    }

    public void SetBg(bool isNormal)
    {
        if (bgSpecialImage == null) return;
        bgImage.gameObject.SetActive(isNormal);
        bgSpecialImage.gameObject.SetActive(!isNormal);
        titleText.color = isNormal ? normalTitleColor : specialTitleColor;
    }

    private void RefreshDataNormal()
    {
        if (giftShopConfig == null) return;
        int boughtCountDaily = Player.ShopManager.BuyCount.FirstOrDefault(item => item.Key == giftShopConfig.Id).Value;
        int boughtCountTotal = Player.ShopManager.SumCount.FirstOrDefault(item => item.Key == giftShopConfig.Id).Value;
        bool isLimitedByDaily = giftShopConfig.DailyLimit > 0 && boughtCountDaily >= giftShopConfig.DailyLimit;
        bool isLimitedByAll = giftShopConfig.Limit > 0 && boughtCountTotal >= giftShopConfig.Limit;
        if (isLimitedByDaily)
        {
            limitText.text = "今日限购:{0}/{1}".SafeFormat(boughtCountDaily, giftShopConfig.DailyLimit);
        }
        else if (isLimitedByAll)
        {
            limitText.text = "限购:{0}/{1}".SafeFormat(boughtCountTotal, giftShopConfig.Limit);
        }
        else
        {
            if (giftShopConfig.DailyLimit > 0)
            {
                limitText.text = "今日限购:{0}/{1}".SafeFormat(boughtCountDaily, giftShopConfig.DailyLimit);
            }
            else if (giftShopConfig.Limit > 0)
            {
                limitText.text = "限购:{0}/{1}".SafeFormat(boughtCountTotal, giftShopConfig.Limit);
            }
            else
            {
                limitText.text = "";
            }
        }
        bool hasGet = isLimitedByDaily || isLimitedByAll;
        titleText.text = giftShopConfig.Name;
        SetRewards(giftShopConfig.Content);
        getBtn.gameObject.SetActive(!hasGet);
        waitBtn.gameObject.SetActive(hasGet);
        getBtnUIShiny.enabled = false;
        SetBtnText("{0}元".SafeFormat(giftShopConfig.Rmb));
    }

    private void RefreshDataActivity()
    {
        if (activityData == null) return;
        bool hasGet = ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(activityData.cfg.Id);
        limitText.text = "今日限购:{0}/1".SafeFormat(hasGet ? "1" : "0");
        titleText.text = activityData.cfg.DailyGiftDesc;
        SetRewards(activityData.cfg.DailyGift);
        getBtn.gameObject.SetActive(!hasGet);
        waitBtn.gameObject.SetActive(hasGet);
        getBtnUIShiny.enabled = true;
        SetBtnText("免费");
    }

    private void SetBtnText(string btnStr)
    {
        getText.text = btnStr;
        waitText.text = btnStr;
    }
    private void SetRewards(string rewardStr)
    {
        List<GameItem> gameItemList = GameItemUtils.CreateGameItems(rewardStr).ToList();
        if (gameItemList.Count == 0 || gameItemList[0] == null)
        {
            Debug.LogError("NewYearGiftItem , SetRewards , gameItemList.Count == 0 , rewardStr = {0}".SafeFormat(rewardStr));
            return;
        }
        for (int i = 0; i < inventoryItemList.Count; i++)
        {
            InventoryItem inventoryItem = inventoryItemList[i];
            if (i < gameItemList.Count)
            {
                GameItem gameItem = gameItemList[i];
                inventoryItem.SetData(gameItem);
                inventoryItem.gameObject.SetActive(true);
            }
            else
            {
                inventoryItem.gameObject.SetActive(false);
            }
        }
    }


    private void OnClickWaitBtn(BabuButton _)
    {
        Tips.PopTips("购买次数已达上限");
    }
    private void OnClickGetBtn(BabuButton _)
    {
        if (activityData != null)
        {
            NetworkManager.Instance.ReceiveDailyGift(activityData.cfg.Id, (resp) =>
            {
                if (resp.ReceiveSucceed == false)
                {
                    Tips.PopTips("领取失败");
                    Debug.LogWarningFormat("NewYearGiftItem , OnClickGetBtn , resp.ReceiveSucceed == false , activityData.cfg.Id = {0}", activityData.cfg.Id);
                    return;
                }
                ActivityController.Instance.dailyGiftReceivedActivityIdSet.Add(activityData.cfg.Id);
                var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(activityData.cfg.DailyGift).ToList());
                UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                ActivityController.Instance.RefreshRedDot(activityData);
                Player.ShopManager.CheckRedDot();
                EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
                //EventManager.Instance.Dispatch(EventID.OnRefreshNavigationUIRedDot);
                RefreshDataActivity();
            });
        }
        if (giftShopConfig != null)
        {
            PurchaseInfo info = DataConvUtil.NewPurchase(giftShopConfig.ProductId, giftShopConfig.Name, giftShopConfig.Rmb, giftShopConfig.Id);
#if USER_DEBUG && UNITY_EDITOR
            PurchaseUtil.TestBuyInEditor(info.ShopItemId);
#else
            PurchaseServiceManager.Instance.Purchase(info);
#endif
        }
    }
}
