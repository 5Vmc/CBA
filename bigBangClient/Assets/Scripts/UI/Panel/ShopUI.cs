using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using UnityEngine.UI;
using GameConfig;
using BigBang.Animation;
using Babu.SDK;
using Utils;
using System;
using System.Linq;
using Utils.GameItem;
using GameConfig.Config;
using Babu;

namespace BigBang.UI
{
    public class ShopUIProperties : PanelProperties
    {
        public ShopUI.SubUIID SubUI = ShopUI.SubUIID.Diamond;

        public ShopUIProperties(ShopUI.SubUIID ui)
        {
            SubUI = ui;
        }
    }

    public class ShopUI : APanelController<ShopUIProperties>
    {
        public enum SubUIID
        {
            Diamond = 4,
            MonthCard = 3,
            Train = 2,
            Gift = 1,
            Recruit = 0
        }

        [SerializeField] private BabuButton closeBtn;
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private ResourceTitle resTitle;
        [SerializeField] private GameObject padContainer;
        [NonSerialized]
        private List<string> padPathList = new List<string>(){
            "Prefabs/Pad/AsyncPad/RecruitShopPad.prefab",
            "Prefabs/Pad/AsyncPad/ShopGiftPad.prefab",
            "Prefabs/Pad/AsyncPad/TrainShopPad.prefab",
            "Prefabs/Pad/AsyncPad/MonthCardPad.prefab",
            "Prefabs/Pad/AsyncPad/DiamondShopPad.prefab"
        };
        public ShopUIAnim Anim;
        private Dictionary<int, GameObject> padState = new();

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
            toggleGroup.OnValueChanged += OnToggleChanged;

            PurchaseServiceManager.Instance.CheckPurchase += PurchaseUtil.OnCheckPurchase;
            Babu.EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshAllRedDot);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
            toggleGroup.OnValueChanged -= OnToggleChanged;

            PurchaseServiceManager.Instance.CheckPurchase -= PurchaseUtil.OnCheckPurchase;
            Babu.EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshAllRedDot);
        }

        [SerializeField] private Image giftDotNodeImg = null;
        [SerializeField] private Image monthCardDotNodeImg = null;
        private void RefreshAllRedDot(object[] args = null)
        {
            //RefreshGiftShopRedDot();
            RefreshMonthCardShopRedDot();
        }
        private void RefreshMonthCardShopRedDot()
        {
            bool isRed = Player.ActivityManager.GetIsMonthCardRedDot();
            monthCardDotNodeImg.gameObject.SetActive(isRed);
        }
        //private void RefreshGiftShopRedDot()
        //{
        //    RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.GiftShop, "");
        //    bool isRed = false;
        //    ActivityData activityData = ActivityController.Instance.FindTimeRecruitActivity;
        //    bool isTimeRecruitNeedShow = activityData != null;
        //    if (isTimeRecruitNeedShow)
        //    {
        //        bool hasGet = ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(activityData.cfg.Id);
        //        isRed = !hasGet;
        //    }
        //    node.AddValue(isRed ? 1 : -1);
        //    node.IsRed(giftDotNodeImg.transform);
        //}

        // 检测购买是否通过
        // private bool OnCheckPurchase(string productId)
        // {
        //     // var birthday = DateTime.Parse($"{SDKAntiAddiction.Instance.RealnameInfo.Year}/{SDKAntiAddiction.Instance.RealnameInfo.Month}/{SDKAntiAddiction.Instance.RealnameInfo.Day}");
        //     // int age = (int)((DateTime.Now - birthday).TotalDays / 365);
        //     int age = 18;
        //     var cfg = Configs.ProductPrice.GetConfigList().First(item => item.ProductId == productId);
        //     // 未满8周岁的用户不能付费
        //     if (age < 8)
        //     {
        //         UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(Lang.Get(LangID.ChargeError1), Lang.Get(LangID.ConfirmTxt), null, Lang.Get(LangID.ChargeErrorTitle)));
        //         return false;
        //     }
        //     // 8周岁以上未满16周岁的用户，单次充值金额不得超过50元人民币，每月充值金额累计不得超过200元人民币
        //     else if (age < 16)
        //     {
        //         if (Player.ShopManager.MonthCost > 200 || cfg.Rmb > 50)
        //         {
        //             UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(Lang.Get(LangID.ChargeError2), Lang.Get(LangID.ConfirmTxt), null, Lang.Get(LangID.ChargeErrorTitle)));
        //             return false;
        //         }
        //     }
        //     // 16周岁以上的未成年用户，单次充值金额不得超过100元人民币，每月充值金额累计不得超过400元人民币。
        //     else if (age < 18)
        //     {
        //         if (Player.ShopManager.MonthCost > 400 || cfg.Rmb > 100)
        //         {
        //             UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(Lang.Get(LangID.ChargeError3), Lang.Get(LangID.ConfirmTxt), null, Lang.Get(LangID.ChargeErrorTitle)));
        //             return false;
        //         }
        //     }
        //     return true;
        // }

        [SerializeField] private ClassicShopItemAdapter shopItemAdapter;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            if (ServerConst.OPEN_BUY == false)
            {
                //关闭充值
                if (Properties.SubUI == SubUIID.Gift || Properties.SubUI == SubUIID.Diamond) Properties.SubUI = SubUIID.Train;
            }

            RefreshAllRedDot(null);

            // 播放动画
            Anim.PlayEnter();
            toggleGroup.Switch((int)Properties.SubUI);
        }

        private void OnClose(BabuButton _)
        {
            UIController.Instance.HidePanel<ShopUI>();
        }

        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            oldToggle?.GetComponent<StatusControl>().SetStatus(false);
            newToggle?.GetComponent<StatusControl>().SetStatus(true);
            int EnabledIndex = toggleGroup.EnableIndex;
            foreach (var p in padState.Values)
            {
                if (p != null) p.SetActive(false);
            }
            initPad(EnabledIndex);
            switch (EnabledIndex)
            {
                case 0:
                    resTitle.gameObject.SetActive(true);
                    //Anim.PlayArenaPadAnim();
                    break;
                case 1:
                    resTitle.gameObject.SetActive(true);
                    //Babu.DelayTaskService.Instance.Run(this.gameObject, Anim.PlayDiamondPadAnim);
                    break;
                case 2:
                    resTitle.gameObject.SetActive(true);
                    //Anim.PlayTrainPadAnim();
                    break;
                case 3:
                    resTitle.gameObject.SetActive(false);
                    break;
                case 4:
                    resTitle.gameObject.SetActive(true);
                    //Anim.PlayDiamondPadAnim();
                    break;
            }
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
        }

        private async void initPad(int padIndex)
        {
            if (!padState.ContainsKey(padIndex))
            {
                var padtask = await CBAUtils.GetPrefab(padPathList[padIndex], padContainer.transform);
                padState.Add(padIndex, padtask);
            }

            padState[padIndex].SetActive(true);
            if (padIndex == 3) return;

            if (padIndex == 1)
            {
                var _pad = padState[padIndex].GetComponent<ShopGiftPad>();
                _pad.giftType = 1;
                _pad.SetData();
            }
            else
            {
                padState[padIndex].GetComponent<IDataPad>().SetData();
            }

        }

    }
}