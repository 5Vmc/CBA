using Babu;
using Coffee.UIEffects;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class HundredGuessExchangeItem : MonoBehaviour
    {
        [SerializeField] private InventoryItem inv_Item;
        [SerializeField] private TMP_Text txtItemName;
        [SerializeField] private TMP_Text descTextLabel = null;
        [SerializeField] private TMP_Text txtItemLimit;
        [SerializeField] private Button costBtn;
        [SerializeField] private Image priceIconCost;
        [SerializeField] private TMP_Text priceCost;
        [SerializeField] private Button waitBtn;
        [SerializeField] private Image priceIconWait;
        [SerializeField] private TMP_Text priceWait;


        public ShopItemData data;

        private void OnEnable()
        {
            costBtn.onClick.AddListener(OnCost);
            waitBtn.onClick.AddListener(OnCost);
        }

        private void OnDisable()
        {
            costBtn.onClick.RemoveListener(OnCost);
            waitBtn.onClick.RemoveListener(OnCost);
        }

        private readonly string redColorStr = "<color=#CF0B0B>";
        private readonly string whiteColorStr = "<color=#FFFFFF>";
        private readonly string colorEnd = "</color>";

        public async void SetData(ShopItemData _data)
        {
            //这里只有1个道具
            this.data = _data;

            inv_Item.SetData(_data.gameItemList[0]);
            txtItemName.text = _data.gameItemList[0].GetName();

            txtItemName.color = CBAColorUtil.Instance.GetColor(_data.gameItemList[0].GetQuality());
            descTextLabel.text = _data.gameItemList[0].GetDescription();

            bool isCanBuy = _data.Status && _data.CostItem.Count <= _data.CostItem.GetPlayerCount();
            costBtn.gameObject.SetActive(isCanBuy);
            waitBtn.gameObject.SetActive(!isCanBuy);
            if (isCanBuy)
            {
                priceIconCost.sprite = await _data.CostItem.GetIcon();
                priceCost.text = _data.CostItem.Count.ToString();
            }
            else
            {
                priceIconWait.sprite = await _data.CostItem.GetIcon();
                priceWait.text = _data.CostItem.Count.ToString();
            }

            if (_data.cfg.Limit != -1)
            {
                txtItemLimit.text = "限购: " + ((_data.boughtCountTotal >= _data.cfg.Limit) ? redColorStr : whiteColorStr) + _data.boughtCountTotal + colorEnd + "/" + _data.cfg.Limit;
            }
            else if (_data.cfg.SeasonLimit != -1)
            {
                txtItemLimit.text = "赛季限购: " + _data.cfg.SeasonLimit;//UNDO
            }
            else if (_data.cfg.WeekLimit != -1)
            {
                txtItemLimit.text = "每周限购: " + ((_data.boughtCountWeek >= _data.cfg.WeekLimit) ? redColorStr : whiteColorStr) + _data.boughtCountWeek + colorEnd + "/" + _data.cfg.WeekLimit;
            }
            else if (_data.cfg.DayLimit != -1)
            {
                txtItemLimit.text = "每日限购: " + ((_data.boughtCountDaily >= _data.cfg.DayLimit) ? redColorStr : whiteColorStr) + _data.boughtCountDaily + colorEnd + "/" + _data.cfg.DayLimit;
            }
            else
            {
                txtItemLimit.text = "";
            }
        }

        private void OnCost()
        {
            if (!data.Status)
            {
                Tips.PopError("已达最大限购次数");
                return;
            }
            if (data.CostItem.GetPlayerCount() < data.CostItem.Count)
            {
                Tips.PopError("{0}不足".SafeFormat(data.CostItem.GetName()));
                return;
            }
            Player.ShopManager.ExChangeItem(data.cfg);
        }
    }
}