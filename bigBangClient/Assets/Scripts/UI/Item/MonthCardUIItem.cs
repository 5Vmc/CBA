using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Utils.GameItem;
using System.Linq;
using Utils;
using deVoid.UIFramework;
using GameConfig.Config;
using Babu.SDK;
using Coffee.UIEffects;
using Babu;
using Protocol;

namespace BigBang.UI
{

    public enum MonthCardType
    {
        Noraml,
        Super,
    }
    public class MonthCardUIItem : APanelController
    {
        [System.Serializable]
        public class OffInfo
        {
            public Image num1;
            public Image num2;
            public Image percentSign;
            public Image bg;
            public Image name;

        }
        [SerializeField] private List<InventoryItem> obtain;
        [SerializeField] private List<InventoryItem> everyDay;
        [SerializeField] private TMP_Text leftDaysText;
        [SerializeField] private BabuButton claimBtn;
        [SerializeField] private Image claimImage;
        [SerializeField] private TMP_Text claimText;
        [SerializeField] private OffInfo offInfo;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text renewText;
        [SerializeField] private BabuButton buyBtn;
        [SerializeField] private UIShiny shiny;
        [SerializeField] private MonthCardType cardType = MonthCardType.Noraml;

        //样式
        [SerializeField] private Image cardBg;
        [SerializeField] private Image cardImage;
        [SerializeField] private Image cardName;
        [SerializeField] private Image offBg;
        [SerializeField] private Image percentSign;
        [SerializeField] private Image offName;
        [SerializeField] private Image obtainRewardBg;
        [SerializeField] private Image everyDayewardBg;


        private MonthCardShopConfig confData;

        protected override async void Awake()
        {
            base.Awake();
            buyBtn.Anim = null;
            buyBtn.Sound = null;
            claimBtn.Anim = null;
            claimBtn.Sound = null;

            int postfix = 1;
            switch (cardType)
            {
                case MonthCardType.Noraml:
                    postfix = 1;
                    break;
                case MonthCardType.Super:

                    postfix = 2;
                    break;
            }
            cardBg.sprite = await SpriteProxy.GetMonthCardImageStyle($"card_bg_{postfix}");
            cardImage.sprite = await SpriteProxy.GetMonthCardImageStyle($"month_card_{postfix}");
            cardName.sprite = await SpriteProxy.GetMonthCardImageStyle($"card_name_{postfix}");
            offBg.sprite = await SpriteProxy.GetMonthCardImageStyle($"off_bg_{postfix}");
            percentSign.sprite = await SpriteProxy.GetMonthCardImageStyle($"%_{postfix}");
            offName.sprite = await SpriteProxy.GetMonthCardImageStyle($"off_{postfix}");
            obtainRewardBg.sprite = await SpriteProxy.GetMonthCardImageStyle($"reward_bg_{postfix}");
            everyDayewardBg.sprite = await SpriteProxy.GetMonthCardImageStyle($"reward_bg_{postfix}");

            if (shiny && cardType == MonthCardType.Super)
            {
                shiny.enabled = true;
                shiny.Play();
            }
            else
            {
                shiny.enabled = false;
            }


        }
        private void OnEnable()
        {
            buyBtn.OnClick += OnBuy;
            claimBtn.OnClick += OnClaim;
        }
        private void OnDisable()
        {
            buyBtn.OnClick -= OnBuy;
            claimBtn.OnClick -= OnClaim;
        }

        public void SetData(MonthCardShopConfig cfg)
        {
            this.confData = cfg;
            this.RefreshUI();
        }

        [SerializeField] private Color textDarkColor = new();
        [SerializeField] private Color textLightColor = new();
        public async void RefreshUI()
        {
            // 设置价格
            costText.text = confData.Rmb.ToString();

            // 续费文本可见性
            Player.ShopManager.SumCount.TryGetValue(confData.Id, out var count);
            renewText.gameObject.SetActive(count > 0);
            if (cardType == MonthCardType.Noraml)
            {

                if (Player.ShopManager.MonthCard1Days <= 0)
                {
                    leftDaysText.text = $"（30）天";
                    buyBtn.gameObject.SetActive(true);
                    claimBtn.gameObject.SetActive(false);
                }
                else
                {
                    leftDaysText.text = $"（{Player.ShopManager.MonthCard1Days}）天";
                    buyBtn.gameObject.SetActive(false);
                    claimBtn.gameObject.SetActive(true);
                    if (Player.ShopManager.IsGetMonthCard1)
                    {
                        if (Player.ShopManager.MonthCard1Days == 1)
                        {
                            buyBtn.gameObject.SetActive(true);
                            claimBtn.gameObject.SetActive(false);
                        }
                        else
                        {
                            claimImage.sprite = await SpriteManager.GetSprite(AtlasNames.Public, "btn2");
                            claimText.text = "已领取";
                            claimText.color = textDarkColor;
                        }
                    }
                    else
                    {
                        claimImage.sprite = await SpriteManager.GetSprite(AtlasNames.Public, "btn1");
                        claimText.text = "领取";
                        claimText.color = textLightColor;
                    }
                }
            }
            else if (cardType == MonthCardType.Super)
            {
                if (Player.ShopManager.MonthCard2Days <= 0)
                {
                    leftDaysText.text = $"（30）天";
                    buyBtn.gameObject.SetActive(true);
                    claimBtn.gameObject.SetActive(false);
                }
                else
                {
                    leftDaysText.text = $"（{Player.ShopManager.MonthCard2Days}）天";
                    buyBtn.gameObject.SetActive(false);
                    claimBtn.gameObject.SetActive(true);
                    if (Player.ShopManager.IsGetMonthCard2)
                    {
                        if (Player.ShopManager.MonthCard2Days == 1)
                        {
                            buyBtn.gameObject.SetActive(true);
                            claimBtn.gameObject.SetActive(false);
                        }
                        else
                        {
                            claimImage.sprite = await SpriteManager.GetSprite(AtlasNames.Public, "btn2");
                            claimText.text = "已领取";
                            claimText.color = textDarkColor;
                        }
                    }
                    else
                    {
                        claimImage.sprite = await SpriteManager.GetSprite(AtlasNames.Public, "btn1");
                        claimText.text = "领取";
                        claimText.color = textLightColor;
                    }
                }
            }


            obtain.ForEach(item => item.gameObject.SetActive(false));
            everyDay.ForEach(item => item.gameObject.SetActive(false));

            // 设置奖励
            foreach (var item in obtain.Zip(GameItemUtils.CreateGameItems(confData.Obtain), (inventoryItem, gameItem) => (inventoryItem, gameItem)))
            {
                item.inventoryItem.gameObject.SetActive(true);
                item.inventoryItem.SetGameItemData(item.gameItem);
            }

            foreach (var item in everyDay.Zip(GameItemUtils.CreateGameItems(confData.EveryDay), (inventoryItem, gameItem) => (inventoryItem, gameItem)))
            {
                item.inventoryItem.gameObject.SetActive(true);
                item.inventoryItem.SetGameItemData(item.gameItem);
            }

        }

        private void OnBuy(BabuButton sender)
        {
            if (ServerConst.OPEN_BUY == false)
            {
                Tips.PopTips("测试期间不开放充值");
                return;
            }

            PurchaseInfo info = DataConvUtil.NewPurchase(confData.ProductId, confData.Name, confData.Rmb, confData.Id);
#if USER_DEBUG && UNITY_EDITOR
            PurchaseUtil.TestBuyInEditor(info.ShopItemId);
#else
            PurchaseServiceManager.Instance.Purchase(info);
#endif
        }

        private void OnClaim(BabuButton sender)
        {
            if (cardType == MonthCardType.Noraml && Player.ShopManager.IsGetMonthCard1)
            {
                Tips.PopError("今日奖励已领取，请明日再来");
                return;
            }
            if (cardType == MonthCardType.Super && Player.ShopManager.IsGetMonthCard2)
            {
                Tips.PopError("今日奖励已领取，请明日再来");
                return;
            }
            NetworkManager.Instance.GetMonthCardReward(confData.Id, response =>
            {
                if (response.Succeed)
                {
                    if (cardType == MonthCardType.Noraml)
                    {
                        Player.ShopManager.IsGetMonthCard1 = true;
                    }
                    else if (cardType == MonthCardType.Super)
                    {
                        Player.ShopManager.IsGetMonthCard2 = true;
                    }
                    var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(confData.EveryDay).ToList());
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                    RefreshUI();
                    ActivityController.Instance.RefreshClientRedDot(ActivityClientType.MonthCard);
                    EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
                }
            });
        }
    }
}
