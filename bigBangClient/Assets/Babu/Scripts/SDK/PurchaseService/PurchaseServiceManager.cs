//using BigBang;
//using BigBang.UI;
//using GameConfig;
using System;
using System.Linq;
using UnityEngine;

namespace Babu.SDK
{
    public class PurchaseServiceManager : Task
    {
        public static PurchaseServiceManager Instance;

        public class Error
        {
            public const int Unknown = -1;
            public const int Succ = 0;
            public const int Canceled = 1;
            public const int PurchaseDuplicated = 2;
            public const int WillTryAgainLater = 3;
        }

        public class Event
        {
            public const string PurchaseResult = "__PurchaseResult";          // 购买结果

            public const string PurchaseTest = "__PurchaseTest";

            public const string PurchaseLocalSuccess = "__PurchaseLocalSuccess";//本地结果,由quick返回的通知
            public const string PurchaseLocalFail = "__PurchaseLocalFail";//本地结果,由quick返回的通知
        }

        protected PurchaseService _purchaseService = new PurchaseServiceMiGu();//PurchaseServiceDefault();

        public event Func<string, bool> CheckPurchase;

        public void SetPurchaseHandler(PurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        void Awake()
        {
            Instance = this;
        }

        public override string GetTaskName()
        {
            return "PurchaseServiceManager";
        }

        public void Purchase(PurchaseInfo info)
        {
            // if (CheckPurchase == null) return;

            // if (!CheckPurchase.Invoke(info.GoodsId)) return;

            //var birthday = DateTime.Parse($"{SDKAntiAddiction.Instance.RealnameInfo.Year}/{SDKAntiAddiction.Instance.RealnameInfo.Month}/{SDKAntiAddiction.Instance.RealnameInfo.Day}");
            //int age = (int)((DateTime.Now - birthday).TotalDays / 365);
            //var cfg = Configs.ProductPrice.GetConfigList().First(item => item.ProductId == productId);
            //// 未满8周岁的用户不能付费
            //if (age < 8)
            //{
            //    UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(Lang.Get(LangID.ChargeError1), Lang.Get(LangID.ConfirmTxt), null, Lang.Get(LangID.ChargeErrorTitle)));
            //    return;
            //}
            //// 8周岁以上未满16周岁的用户，单次充值金额不得超过50元人民币，每月充值金额累计不得超过200元人民币
            //else if (age < 16)
            //{
            //    if (Player.ShopManager.MonthCost > 200 || cfg.Rmb > 50)
            //    {
            //        UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(Lang.Get(LangID.ChargeError2), Lang.Get(LangID.ConfirmTxt), null, Lang.Get(LangID.ChargeErrorTitle)));
            //        return;
            //    }
            //}
            //// 16周岁以上的未成年用户，单次充值金额不得超过100元人民币，每月充值金额累计不得超过400元人民币。
            //else if (age < 18)
            //{
            //    if (Player.ShopManager.MonthCost > 400 || cfg.Rmb > 100)
            //    {
            //        UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(Lang.Get(LangID.ChargeError3), Lang.Get(LangID.ConfirmTxt), null, Lang.Get(LangID.ChargeErrorTitle)));
            //        return;
            //    }
            //}

            _purchaseService.Purchase(info);
        }

        public override void Run(TaskExecutor executor)
        {
            Environment.SetValue("purchase_server_manager", true);

            string productIds = Environment.GetValue("purchase_product_ids", "");
            Debug.Log("Init Purchase With Product Ids: " + productIds);
            _purchaseService.Init(productIds.Split(','));
            executor.OnChildTaskCompleted();
        }
    }
}