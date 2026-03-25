using Babu;
using Babu.SDK;
using Coffee.UIEffects;
using DG.Tweening;
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
    public class GiftItemData
    {
        public ActivityData activityData = null;

        public int boughtCountDaily;
        public int boughtCountTotal;

        public ETimeGiftType type;
        public GiftShopConfig cfg;
        public int EndTime;
        /// <summary>
        /// 礼包内容
        /// </summary>
        public List<GameItem> gameItemList;

        /// <summary>
        /// 是否可以购买
        /// </summary>
        public bool Status;

        /// <summary>
        /// 礼包商城的物品
        /// </summary>
        /// <param name="id"></param>
        public GiftItemData(int id, ETimeGiftType _type, int _endtime, ActivityData activityData = null)
        {
            this.activityData = activityData;
            type = _type;
            EndTime = _endtime;
            if (activityData != null)
            {
                gameItemList = GameItemUtils.CreateGameItems(activityData.cfg.DailyGift).ToList();
                bool hasGet = ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(activityData.cfg.Id);
                Status = !hasGet;
            }
            else
            {
                cfg = Configs.GiftShop.GetConfig(id);
                if (cfg == null)
                {
                    Debug.LogWarning("GiftShopConfig is null, id = " + id);
                    return;
                }
                gameItemList = GameItemUtils.CreateGameItems(cfg.Content).ToList();
                this.boughtCountDaily = Player.ShopManager.BuyCount.FirstOrDefault(item => item.Key == cfg.Id).Value;
                this.boughtCountTotal = Player.ShopManager.SumCount.FirstOrDefault(item => item.Key == cfg.Id).Value;

                Status = true;
                if (cfg.DailyLimit > 0)
                {
                    Status = this.boughtCountDaily < cfg.DailyLimit;
                }
                if (Status == true && cfg.Limit > 0)
                {
                    Status = this.boughtCountTotal < cfg.Limit;
                }
            }
        }
    }

    public class TimeGiftItem : PageViewVirtualItemBase
    {
        [SerializeField] private GameObject slotContainer;
        [SerializeField] private TMP_Text txtGiftName;
        [SerializeField] private TMP_Text txtTime;
        [SerializeField] private TMP_Text txtPrice;
        [SerializeField] private Image bgImg;
        [SerializeField] private BabuButton btnPay;
        [SerializeField] private InventoryItem prefab;
        [SerializeField] private Image img_card;
        [SerializeField] private Image imgRebate;
        [SerializeField] private TMP_Text txtRebate;

        public GiftItemData data;
        private int leftTime;
        private void OnEnable()
        {
            btnPay.OnClick += OnBuy;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
        }

        private void OnDisable()
        {
            btnPay.OnClick -= OnBuy;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
        }

        public override void SetData(object obj)
        {
            SetData(obj as GiftItemData);
        }
        public void SetData(GiftItemData _data)
        {
            this.data = _data;
            RefreshUI();
        }

        public async void RefreshUI()
        {
            if (data.cfg.Rebate != 0)
            {
                imgRebate.gameObject.SetActive(true);

                Color newColor;
                if (data.cfg.Rebate > 800)
                {
                    imgRebate.sprite = await SpriteProxy.GetActivityImage("红");
                    ColorUtility.TryParseHtmlString("#fbed3b", out newColor);
                }
                else
                {
                    imgRebate.sprite = await SpriteProxy.GetActivityImage("黄");
                    ColorUtility.TryParseHtmlString("#ff2224", out newColor);
                }

                txtRebate.color = newColor;
                txtRebate.text = data.cfg.Rebate.ToString() + "%";
            }
            else
            {
                imgRebate.gameObject.SetActive(false);
            }


            txtGiftName.text = data.cfg.Name;
            txtPrice.text = data.cfg.Rmb + " 元";
            bgImg.sprite = await SpriteProxy.GetActivityImage(data.cfg.Pic);
            var children = slotContainer.GetComponentsInChildren<InventoryItem>();
            int slotCount = children.Length;
            int rewardCount = data.gameItemList.Count;
            int counter = Math.Max(slotCount, rewardCount);

            for (int index = 0; index < counter; index++)
            {
                InventoryItem item;
                if (index > slotCount - 1)
                {
                    item = Instantiate<InventoryItem>(prefab, slotContainer.transform);
                    item.transform.localScale = new Vector3(0.8f, 0.8f);
                }
                else
                {
                    item = children[index];
                }


                if (index > rewardCount - 1)
                {
                    item.gameObject.SetActive(false);
                }
                else
                {
                    item.gameObject.SetActive(true);
                    item.SetGameItemData(data.gameItemList[index]);
                }
            }
            if (data.EndTime >= Utils.DataConvUtil.ServerTime)
            {
                RefreshLeftTime();
            }
            else
            {
                EventManager.Instance.Dispatch(EventID.OnTimeGiftTimeEnd, this);
            }

            btnPay.transform.localPosition = new Vector3(data.cfg.Pos, btnPay.transform.localPosition.y);
            slotContainer.transform.localPosition = new Vector3(data.cfg.Pos, slotContainer.transform.localPosition.y);

            //设置头像
            if (data.type == ETimeGiftType.GiftCard)
            {


                img_card.sprite = await SpriteProxy.GetPlayerPortraitYellow(int.Parse(data.cfg.Param));
                img_card.SetNativeSize();
                var cardCfg = Configs.CardModel.GetConfig(int.Parse(data.cfg.Param));
                string[] args = cardCfg.Param.Split("|");
                img_card.rectTransform.anchoredPosition = new Vector2(float.Parse(args[0]), float.Parse(args[1]));
                //img_card.rectTransform.anchoredPosition = new Vector3(43f, -118f, 0f);
                //img_card.rectTransform.localScale = new Vector3(float.Parse(args[2])/100, float.Parse(args[2]) / 100, float.Parse(args[2]) / 100);
                img_card.gameObject.SetActive(true);
            }
            else
            {
                img_card.gameObject.SetActive(false);
            }
        }

        private void RefreshLeftTime()
        {
            long leftTime = data.EndTime - Utils.DataConvUtil.ServerTime;
            txtTime.text = TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime);
            if (leftTime < 0)
            {
                EventManager.Instance.Dispatch(EventID.OnTimeGiftTimeEnd, this);
            }
        }

        private void OnBuy(BabuButton sender)
        {
            // Tips.PopTips("购买限时礼包");
            PurchaseInfo info = DataConvUtil.NewPurchase(data.cfg.ProductId, data.cfg.Name, data.cfg.Rmb, data.cfg.Id);
#if USER_DEBUG && UNITY_EDITOR
            PurchaseUtil.TestBuyInEditor(info.ShopItemId);
#else
            PurchaseServiceManager.Instance.Purchase(info);
#endif
        }


    }
}
