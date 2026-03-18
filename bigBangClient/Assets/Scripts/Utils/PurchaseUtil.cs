using System.Diagnostics;
using BigBang;
using BigBang.UI;
using GameConfig;
using System.Linq;
using Babu;
using Babu.SDK;

namespace Utils
{
    public static class PurchaseUtil
    {
        public static bool OnCheckPurchase(string productId)
        {
            //账号实名信息和充值限制方面应由QuickSDK处理
            // // var birthday = DateTime.Parse($"{SDKAntiAddiction.Instance.RealnameInfo.Year}/{SDKAntiAddiction.Instance.RealnameInfo.Month}/{SDKAntiAddiction.Instance.RealnameInfo.Day}");
            // // int age = (int)((DateTime.Now - birthday).TotalDays / 365);
            // int age = 18;
            // var cfg = Configs.ProductPrice.GetConfigList().First(item => item.ProductId == productId);
            // // 未满8周岁的用户不能付费
            // if (age < 8)
            // {
            //     UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(Lang.Get(LangID.ChargeError1), Lang.Get(LangID.ConfirmTxt), null, Lang.Get(LangID.ChargeErrorTitle)));
            //     return false;
            // }
            // // 8周岁以上未满16周岁的用户，单次充值金额不得超过50元人民币，每月充值金额累计不得超过200元人民币
            // else if (age < 16)
            // {
            //     if (Player.ShopManager.MonthCost > 200 || cfg.Rmb > 50)
            //     {
            //         UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(Lang.Get(LangID.ChargeError2), Lang.Get(LangID.ConfirmTxt), null, Lang.Get(LangID.ChargeErrorTitle)));
            //         return false;
            //     }
            // }
            // // 16周岁以上的未成年用户，单次充值金额不得超过100元人民币，每月充值金额累计不得超过400元人民币。
            // else if (age < 18)
            // {
            //     if (Player.ShopManager.MonthCost > 400 || cfg.Rmb > 100)
            //     {
            //         UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(Lang.Get(LangID.ChargeError3), Lang.Get(LangID.ConfirmTxt), null, Lang.Get(LangID.ChargeErrorTitle)));
            //         return false;
            //     }
            // }
            return true;
        }

        public static void TestBuyInEditor(int shopItemId)
        {
            EventManager.Instance.Dispatch(EventID.CHARGE_START);
            var cfg = Configs.MonthCardShop.GetConfig(shopItemId);
            if (cfg != null)
            {
                NetworkManager.Instance.MonthCardBuy(cfg.Id, (resp) =>
                {
                    Tips.PopTips("购买月卡商城成功，cfg.Name={0}".SafeFormat(cfg.Name));
                });
                return;
            }

            var cfg2 = Configs.GiftShop.GetConfig(shopItemId);
            if (cfg2 != null)
            {
                NetworkManager.Instance.GiftShop(cfg2.Id, (resp) =>
                {
                    Tips.PopTips("购买礼包商城成功，cfg2.Name={0}".SafeFormat(cfg2.Name));
                });
                return;
            }

            var cfg3 = Configs.DiamondShop.GetConfig(shopItemId);
            if (cfg3 != null)
            {
                NetworkManager.Instance.DiamondShop(cfg3.Id, (resp) =>
                {
                    Tips.PopTips("购买钻石成功，cfg3.Name={0}".SafeFormat(cfg3.Name));
                });
                return;
            }

            Tips.PopTips("未知购买类型");
        }
    }
}