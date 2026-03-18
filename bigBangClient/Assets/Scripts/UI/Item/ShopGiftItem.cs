
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

    public class ShopGiftItem : MonoBehaviour
    {
        [SerializeField] private GameObject slotContainer;
        [SerializeField] private TMP_Text txtGiftName;
        [SerializeField] private TMP_Text txtRebate;
        [SerializeField] private Image imgRebate;
        [SerializeField] private TMP_Text txtTime;
        [SerializeField] private TMP_Text txtPrice;
        [SerializeField] private BabuButton btnPay;
        [SerializeField] private InventoryItem prefab;

        public GiftItemData data;
        private int leftTime;
        private void OnEnable()
        {
            btnPay.OnClick += OnBuy;
        }

        private void OnDisable()
        {
            btnPay.OnClick -= OnBuy;
        }

        public void SetData(GiftItemData _data)
        {
            this.data = _data;
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (data.activityData != null)
            {
                txtGiftName.text = data.activityData.cfg.Name;
                txtPrice.text = "0";
                ResetPriceLayout();
                imgRebate.gameObject.SetActive(false);
                bool hasGet = ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(data.activityData.cfg.Id);
                txtTime.text = "今日限购:{0}/1".SafeFormat(hasGet ? "1" : "0");
                SetRewards(data.gameItemList);
                return;
            }

            txtGiftName.text = data.cfg.Name;
            txtPrice.text = data.cfg.Rmb.ToString();
            ResetPriceLayout();

            if (data.cfg.Rebate != 0)
            {
                imgRebate.gameObject.SetActive(true);
                txtRebate.text = data.cfg.Rebate.ToString() + "%";
            }
            else
            {
                imgRebate.gameObject.SetActive(false);
            }

            if (data.cfg.DailyLimit > 0 && data.boughtCountDaily >= data.cfg.DailyLimit)
            {
                txtTime.text = "今日限购:{0}/{1}".SafeFormat(data.boughtCountDaily, data.cfg.DailyLimit);
            }
            else if (data.cfg.Limit > 0 && data.boughtCountTotal >= data.cfg.Limit)
            {
                txtTime.text = "限购:{0}/{1}".SafeFormat(data.boughtCountTotal, data.cfg.Limit);
            }
            else
            {
                if (data.cfg.DailyLimit > 0)
                {
                    txtTime.text = "今日限购:{0}/{1}".SafeFormat(data.boughtCountDaily, data.cfg.DailyLimit);
                }
                else if (data.cfg.Limit > 0)
                {
                    txtTime.text = "限购:{0}/{1}".SafeFormat(data.boughtCountTotal, data.cfg.Limit);
                }
                else
                {
                    txtTime.text = "";
                }
            }
            SetRewards(data.gameItemList);
        }
        private void ResetPriceLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(txtPrice.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(txtPrice.transform.parent as RectTransform);
        }
        private void SetRewards(List<GameItem> gameItemList)
        {
            var children = slotContainer.GetComponentsInChildren<InventoryItem>();
            int slotCount = children.Length;
            int rewardCount = gameItemList.Count;
            int counter = Math.Max(slotCount, rewardCount);

            //处理奖励图标
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
                    item.SetGameItemData(gameItemList[index]);
                }
            }
        }

        private void OnBuy(BabuButton sender)
        {
            if (data.Status)
            {
                if (data.activityData != null)
                {
                    NetworkManager.Instance.ReceiveDailyGift(data.activityData.cfg.Id, (resp) =>
                    {
                        if (resp.ReceiveSucceed == false)
                        {
                            Tips.PopTips("领取失败");
                            Debug.LogWarningFormat("ShopGiftItem , OnBuy , resp.ReceiveSucceed == false , data.activityData.cfg.Id = {0}", data.activityData.cfg.Id);
                            return;
                        }
                        data.Status = false;
                        ActivityController.Instance.dailyGiftReceivedActivityIdSet.Add(data.activityData.cfg.Id);
                        var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(data.activityData.cfg.DailyGift).ToList());
                        UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                        ActivityController.Instance.RefreshRedDot(data.activityData);
                        Player.ShopManager.CheckRedDot();
                        EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
                        //EventManager.Instance.Dispatch(EventID.OnRefreshNavigationUIRedDot);
                        RefreshUI();
                    });
                    return;
                }
                //todo://吊起购买
                PurchaseInfo info = DataConvUtil.NewPurchase(data.cfg.ProductId, data.cfg.Name, data.cfg.Rmb, data.cfg.Id);
#if USER_DEBUG && UNITY_EDITOR
                PurchaseUtil.TestBuyInEditor(info.ShopItemId);
                return;
#endif
                PurchaseServiceManager.Instance.Purchase(info);
            }
            else
            {
                Tips.PopTips("购买次数已达上限");
            }
        }
    }
}