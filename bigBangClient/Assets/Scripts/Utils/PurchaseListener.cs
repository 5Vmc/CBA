using Babu.SDK;
using BigBang;
using BigBang.UI;
using GameConfig;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using Utils.GameItem;

namespace Babu
{
    public class PurchaseListener : MonoBehaviour
    {
        private void Start()
        {
            EventManager.Instance.Register(PurchaseServiceManager.Event.PurchaseResult, OnPurchaseResult);
            EventManager.Instance.Register(PurchaseServiceManager.Event.PurchaseTest, OnPurchaseTest);
        }

        private void OnDestroy()
        {
            EventManager.Instance.Unregister(PurchaseServiceManager.Event.PurchaseResult, OnPurchaseResult);
            EventManager.Instance.Unregister(PurchaseServiceManager.Event.PurchaseTest, OnPurchaseTest);
        }

        private void OnPurchaseResult(object[] args)
        {
            int state = (int)args[0];
            int itemID = (int)args[1];
            //string orderId = args[2] as string;
            if (state == PurchaseServiceManager.Error.Succ)
            {
                // 根据productID选择性进入下列函数
                // 礼包商城, 限时礼包也会被这个接口发掉。
                OnGiftShopBuySucceed(itemID);
                // 钻石商城
                OnDiamondShopBuySucceed(itemID);
                //月卡
                OnMonthCardBuySucceed(itemID);
            }
            else
            {
                // 购买失败
                UIController.Instance.OpenWindow<ConfirmationBoxUI>(new ConfirmationBoxUIProperties(Lang.Error(ErrorID.BuyError), null));
            }
        }

        private void OnMonthCardBuySucceed(int itemId)
        {
            var cfg = Configs.MonthCardShop.GetConfigList().FirstOrDefault(item => item.Id == itemId);
            if (cfg == null) return;

            //cfg.

            // 设置奖励
            List<Protocol.GameItem> obtainList = new List<Protocol.GameItem>();
            foreach (var item in GameItemUtils.CreateGameItems(cfg.Obtain))
            {
                obtainList.Add(new Protocol.GameItem() { Type = (int)item.Type, Id = item.Id, Count = item.Count });
            }

            UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(obtainList));
            Player.ShopManager.RecordBuyCount(cfg.Id, cfg.Rmb);


            if (cfg.Id == GameConst.MONTH_CARD_NORMAL)
            {
                Player.ShopManager.MonthCard1Days += 30;
                Player.ShopManager.IsGetMonthCard1 = false;
            }
            else if (cfg.Id == GameConst.MONTH_CARD_SUPER)
            {
                Player.ShopManager.MonthCard2Days += 30;
                Player.ShopManager.IsGetMonthCard2 = false;
            }

            ActivityController.Instance.RefreshClientRedDot(ActivityClientType.MonthCard);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        // 礼包商城购买成功
        private void OnGiftShopBuySucceed(int itemid)
        {
            var cfg = Configs.GiftShop.GetConfigList().FirstOrDefault(item => item.Id == itemid);
            if (cfg == null) return;

            if (!string.IsNullOrWhiteSpace(cfg.Content))
            {
                string[] gifts = cfg.Content.Split('|');
                if (gifts.Length > 0)
                {
                    List<Protocol.GameItem> obtainList = new List<Protocol.GameItem>();
                    foreach (var strContent in gifts)
                    {
                        var gift = GetGift(strContent);
                        if (gift.Item1 != 0)
                        {
                            obtainList.Add(new Protocol.GameItem() { Type = gift.Item1, Id = gift.Item2, Count = gift.Item3 });
                        }
                    }
                    if (obtainList.Count > 0)
                    {
                        // 打开收益界面
                        UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(obtainList));
                    }
                }
            }
            Player.ShopManager.RecordBuyCount(cfg.Id, cfg.Rmb);
            Babu.EventManager.Instance.Dispatch(EventID.OnRefreshGiftShop, itemid);


            // NetworkManager.Instance.GiftShop(cfg.Id, orderId, response =>
            // {
            //     if (response.State == PurchaseSuccessReponseState.SUCC)
            //     {
            //         // 设置获得项目
            //         List<Protocol.GameItem> obtainList = new List<Protocol.GameItem>();
            //         obtainList.Add(new Protocol.GameItem() { Type = gift1.Item1, Id = gift1.Item2, Count = gift1.Item3 });
            //         obtainList.Add(new Protocol.GameItem() { Type = gift2.Item1, Id = gift2.Item2, Count = gift2.Item3 });
            //         // 打开收益界面
            //         UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(obtainList));
            //         Player.ShopManager.Buy(cfg.Id, cfg.Rmb);
            //     }
            //     else{
            //         Debug.Log("xxxxxx 购买失败");
            //     }
            // });
        }

        // 钻石商城购买成功
        private void OnDiamondShopBuySucceed(int itemId)
        {
            var cfg = Configs.DiamondShop.GetConfigList().FirstOrDefault(item => item.Id == itemId);
            if (cfg == null) return;

            // 是否是首充
            bool isFirst = Player.ShopManager.GetSumCount(cfg.Id) == 0;
            Player.ShopManager.RecordBuyCount(cfg.Id, cfg.Rmb);
            // 刷新钻石商城数据
            EventManager.Instance.Dispatch(EventID.OnRefreshDiamondShop);
            // 设置获得项目
            List<Protocol.GameItem> obtainList = new List<Protocol.GameItem>();
            obtainList.Add(new Protocol.GameItem() { Type = (int)GameItemType.Resource, Id = ResourceId.Diamond, Count = cfg.Obtain + (isFirst ? cfg.Give : 0) });
            // 打开收益界面
            UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(obtainList));

            // NetworkManager.Instance.DiamondShop(cfg.Id, response =>
            // {
            //     if (response.State == PurchaseSuccessReponseState.SUCC)
            //     {
            //         // 是否是首充
            //         bool isFirst = Player.ShopManager.SumCount[cfg.Id] == 0;
            //         Player.ShopManager.Buy(cfg.Id, cfg.Rmb);
            //         // 刷新钻石商城数据
            //         EventManager.Instance.Dispatch(EventID.OnRefreshDiamondShop);
            //         // 设置获得项目
            //         List<Protocol.GameItem> obtainList = new List<Protocol.GameItem>();
            //         obtainList.Add(new Protocol.GameItem() { Type = (int)GameItemType.Resource, Id = ResourceId.Diamond, Count = cfg.Obtain + (isFirst ? cfg.Give : 0) });
            //         // 打开收益界面
            //         UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(obtainList));

            //     }
            // });
        }

        private (int, int, int) GetGift(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return (0, 0, 0);
            string[] strArr = str.Split(':');
            if (strArr.Length == 0 || strArr.Length % 3 != 0) return (0, 0, 0);
            try
            {
                int type = int.Parse(str.Split(':')[0]);
                int id = int.Parse(str.Split(':')[1]);
                int count = int.Parse(str.Split(':')[2]);
                return (type, id, count);
            }
            catch (System.Exception)
            {
                return (0, 0, 0);
            }
        }

        private void OnPurchaseTest(object[] args)
        {
            int shopItemId = (int)args[0];
            PurchaseUtil.TestBuyInEditor(shopItemId);
        }
    }
}
