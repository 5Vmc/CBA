using Babu;
using BigBang;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.GameItem;

public class PlayerShopManager
{
    public void Init()
    {
        BuyCount.Clear();
        WeekCount.Clear();
        ArenaShopCount.Clear();
        BuyEnergyCount = 0;
        MonthCost = 0;
        FirstCharge = false;
        MonthCard1Days = 0;
        MonthCard2Days = 0;
        IsGetMonthCard1 = false;
        IsGetMonthCard2 = false;
        RegOnce();
    }
    bool isRegOnce = false;
    private void RegOnce()
    {
        if (isRegOnce == true) return;
        isRegOnce = true;
        EventManager.Instance.Register(EventID.CHARGE_START, OnChargeStart);
        EventManager.Instance.Register(EventID.CHARGE_SUCCESS, OnChargeSuccess);
        EventManager.Instance.Register(EventID.CHARGE_FAIL, OnChargeFail);
    }


    #region

    public void ReportChargeSuccess(List<ConsumeOrderData> consumeOrderDataList)
    {
        if (consumeOrderDataList == null || consumeOrderDataList.Count <= 0) return;
        List<string> orderNoList = new();
        foreach (ConsumeOrderData consumeOrderData in consumeOrderDataList)
        {
            ReportChargeSuccessToByteDance(consumeOrderData);
            orderNoList.Add(consumeOrderData.OrderNo);
        }
        NetworkManager.Instance.ConsumeOrderNo(orderNoList);
    }
    public void ReportChargeSuccess(int shopItemId, string orderNo)
    {
        ReportChargeSuccessToByteDance(shopItemId, orderNo);
        NetworkManager.Instance.ConsumeOrderNo(new List<string>() { orderNo });
    }
    private void ReportChargeSuccessToByteDance(ConsumeOrderData consumeOrderData)
    {
        int shopItemId = consumeOrderData.ShopItemId;
        string orderNo = consumeOrderData.OrderNo;
        ReportChargeSuccessToByteDance(shopItemId, orderNo);
    }
    private void ReportChargeSuccessToByteDance(int shopItemId, string orderNo)
    {
        int id = shopItemId;
        string name = "Unknow";
        string productId = "Unknow";
        float rmb = 0;
        string type = "Unknow";

        DiamondShopConfig diamondShopConfig = Configs.DiamondShop.GetConfig(shopItemId);
        if (diamondShopConfig != null)
        {
            name = diamondShopConfig.Name;
            productId = diamondShopConfig.ProductId;
            rmb = diamondShopConfig.Rmb;
            type = "diamond";
        }
        MonthCardShopConfig monthCardShopConfig = Configs.MonthCardShop.GetConfig(shopItemId);
        if (monthCardShopConfig != null)
        {
            name = monthCardShopConfig.Name;
            productId = monthCardShopConfig.ProductId;
            rmb = monthCardShopConfig.Rmb;
            type = "card";
        }
        GiftShopConfig giftShopConfig = Configs.GiftShop.GetConfig(shopItemId);
        if (giftShopConfig != null)
        {
            name = giftShopConfig.Name;
            productId = giftShopConfig.ProductId;
            rmb = giftShopConfig.Rmb;
            type = "gift";
        }
        ByteDanceManager.Instance.ReportPay(type, name, shopItemId.ToString(), (int)rmb, true);
    }

    #endregion


    // key=商品ID value=单日购买次数
    public Dictionary<int, int> BuyCount = new Dictionary<int, int>();

    public int GetBuyCount(int key)
    {
        if (BuyCount.ContainsKey(key))
        {
            return BuyCount[key];
        }
        return 0;
    }

    // key=商品ID value=总购买次数
    public Dictionary<int, int> SumCount = new Dictionary<int, int>();

    public int GetSumCount(int key)
    {
        if (SumCount.ContainsKey(key))
        {
            return SumCount[key];
        }
        return 0;
    }

    // key=商品ID value=总购买次数
    public Dictionary<int, int> WeekCount = new Dictionary<int, int>();
    // key=商品ID value=总购买次数
    public Dictionary<int, int> ArenaShopCount = new Dictionary<int, int>();
    //当日购买体力的次数
    public int BuyEnergyCount;
    // 当月购买金额
    public float MonthCost = 0;

    public float SumCost
    {
        get;
        set;
    }

    public bool FirstCharge
    {
        get;
        set;
    }

    public int MonthCard1Days
    {
        get;
        set;
    }

    public int MonthCard2Days
    {
        get;
        set;
    }

    public bool IsGetMonthCard1
    {
        get;
        set;
    }
    public bool IsGetMonthCard2
    {
        get;
        set;
    }

    private ClassicShopItemAdapter adapter;
    public void UnPack(ShopModuleNotify data)
    {
        foreach (var item in data.BuyCount)
        {
            BuyCount[item.ShopItemId] = item.Count;
        }
        foreach (var item in data.SumCount)
        {
            SumCount[item.ShopItemId] = item.Count;
        }
        foreach (var item in data.WeekCount)
        {
            WeekCount[item.ShopItemId] = item.Count;
        }

        MonthCost = data.MonthCost;
        SumCost = data.SumCost;
        FirstCharge = data.FirstCharge;

        MonthCard1Days = data.MonthCard1Days;
        MonthCard2Days = data.MonthCard2Days;

        IsGetMonthCard1 = data.GetMonthCard1;
        IsGetMonthCard2 = data.GetMonthCard2;

        BuyEnergyCount = data.GetEnergyTimes;

        ReportChargeSuccess(data.OrderData.ToList());
        ActivityController.Instance.RefreshClientRedDot(ActivityClientType.MonthCard);
        EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
    }

    public void RecordBuyCount(int shopItemID, float cost)
    {
        // 当日购买次数+1
        BuyCount[shopItemID]++;
        // 总购买次数+1
        SumCount[shopItemID]++;
        // 周购买次数+1
        WeekCount[shopItemID]++;
        MonthCost += cost;
        SumCost += cost;
    }


    public void ExChangeItem(GameItemShopConfig config, int count = 1, Action successCallback = null)
    {
        ExChangeItem(config.Id, count, GameItemUtils.CreateGameItem(config.Cost), GameItemUtils.CreateGameItems(config.Item).ToList()[0].GetName(), successCallback);
    }

    /// <summary>
    /// 是否需要兑换提示，记录在内存中
    /// </summary>
    public bool isNeedAlertExchange = true;
    public void ExChangeItem(int shopItemId, int count, Utils.GameItem.GameItem costItem, string getName, Action successCallback = null)
    {
        string error = Player.PackageManager.IsGameItemEnough(costItem);
        if (error != "")
        {
            //Tips.PopTips(error);
            return;
        }

        if (Player.ShopManager.isNeedAlertExchange)
        {
            string tipStr = "";
            if (count > 1)
            {
                tipStr = "花费{0}{1}购买{2}{3}?".SafeFormat(costItem.Count * count, costItem.GetName(), count, getName);
            }
            else
            {
                tipStr = "花费{0}{1}购买{2}?".SafeFormat(costItem.Count, costItem.GetName(), getName);
            }
            UIController.Instance.OpenWindow<ConfirmBoxCheckUI>(new ConfirmBoxCheckUIProperties(tipStr, () =>
            {
                DoExChangeItem(shopItemId, count, successCallback);
            }, null, !Player.ShopManager.isNeedAlertExchange, "不再提醒", (bool isCheck) => { Player.ShopManager.isNeedAlertExchange = !isCheck; }));
        }
        else
        {
            DoExChangeItem(shopItemId, count, successCallback);
        }
    }
    private void DoExChangeItem(int shopItemId, int count, Action successCallback = null)
    {
        NetworkManager.Instance.ExChangeShopItem(shopItemId, count, response =>
        {
            // 购买成功
            if (response.Succeed)
            {
                RecordBuyCount(shopItemId, 0);
                EventManager.Instance.Dispatch(EventID.ClassicShopUIItemBuy, shopItemId);
                successCallback?.Invoke();
            }
        });
    }

    public void CheckRedDot()
    {
        //限时抽卡活动开启时，会有一个每日可免费领取的礼包
        bool isRed = false;
        ActivityData activityData = ActivityController.Instance.FindTimeRecruitActivity;
        bool isTimeRecruitNeedShow = activityData != null;
        if (isTimeRecruitNeedShow)
        {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + activityData.cfg.Id);
            bool hasGet = ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(activityData.cfg.Id);
            isRed = !hasGet;
            node.AddValue(isRed ? 1 : -1);
        }
    }

    private void OnChargeStart(object[] objects)
    {
        UnityEngine.Debug.Log("PlayerShopManager , OnChargeStart");
        //if (objects != null && objects.Length > 9)
        //{
        //    string cpOrderID = objects[0] as string;
        //    string goodsId = objects[1] as string;
        //    string goodsName = objects[2] as string;
        //    double amount = (double)objects[3];
        //    string serverId = objects[4] as string;
        //    string serverName = objects[5] as string;
        //    string roleId = objects[6] as string;
        //    string roleName = objects[7] as string;
        //    string extrasParams = objects[8] as string;
        //    int shopItemId = (int)objects[9];
        //    ReportChargeStartToByteDance(cpOrderID, goodsId, goodsName, amount, serverId, serverName, roleId, roleName, extrasParams, shopItemId);
        //}

        string clientCreatTimeStr = Babu.Environment.GetValue("client_creat_time", "");
        if (int.TryParse(clientCreatTimeStr, out int clientCreatTime))
        {
            if (clientCreatTime > 9121843)
            {
                UIController.Instance.OpenWindow<PurchaseMaskUI>();
            }
        }
    }
    //private void ReportChargeStartToByteDance(string cpOrderID, string goodsId, string goodsName, double amount, string serverId, string serverName, string roleId, string roleName,
    //        string extrasParams, int shopItemId)
    //{
    //    Dictionary<string, object> reYunChargeStartDict = new Dictionary<string, object>
    //    {
    //        { "param1", cpOrderID },
    //        { "param2", goodsId },
    //        { "param3", goodsName },
    //        { "param4", amount },
    //        { "param5", serverId },
    //        { "param6", serverName },
    //        { "param7", roleId },
    //        { "param8", roleName },
    //        { "param9", extrasParams },
    //        { "param10", shopItemId },
    //    };
    //    ByteDance.Instance.setDD(cpOrderID, "CNY", (float)amount, reYunChargeStartDict);
    //}

    private void OnChargeFail(object[] objects)
    {
        UnityEngine.Debug.Log("PlayerShopManager , OnChargeFail");
        UIController.Instance.CloseWindow<PurchaseMaskUI>(false);
    }
    private void OnChargeSuccess(object[] objects)
    {
        UnityEngine.Debug.Log("PlayerShopManager , OnChargeSuccess");
        UIController.Instance.CloseWindow<PurchaseMaskUI>(false);
    }

}
