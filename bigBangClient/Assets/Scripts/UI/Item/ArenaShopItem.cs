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
    public class ArenaShopItemData
    {
        public int ShopItemId;
        public GameItem CostItem;

        public ArenaShopConfig cfg;
        public List<GoodsConfig> goodCfgList;
        public List<GameItem> gameItemList;
        public int boughtCount;
        public Protocol.ShopInfo shopInfo;

        public bool Status;
        private const int MoneyId = 400501;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_cfg"></param>
        /// <param name="boughtCount">已经购买的数量，默认为0</param>
        public ArenaShopItemData(ArenaShopConfig _cfg, Protocol.ShopInfo _info)
        {
            shopInfo = _info;
            cfg = _cfg;
            gameItemList = GameItemUtils.CreateGameItems(cfg.Item).ToList();
            goodCfgList = new List<GoodsConfig>();
            foreach (GameItem item in gameItemList)
                goodCfgList.Add(Configs.Goods.GetConfig(item.Id));

            CostItem = GameItemUtils.CreateGameItem(GameItemType.Goods, MoneyId, _cfg.Cost);
            boughtCount = 0;
            if (shopInfo != null)
            {
                boughtCount = shopInfo.Stock;
            }

            if (cfg.Limit > -1)
            {
                Status = boughtCount < cfg.Limit;
            }
            else if (cfg.WeekLimit > -1)
            {
                Status = boughtCount < cfg.WeekLimit;
            }
        }
    }

    public class ArenaShopItem : MonoBehaviour
    {
        [SerializeField] private InventoryItem inv_Item;
        [SerializeField] private TMP_Text txtItemName;
        [SerializeField] private TMP_Text txtItemDesc;
        [SerializeField] private TMP_Text txtItemLimit;
        [SerializeField] private TMP_Text txtItemOption;
        [SerializeField] private Button costBtn;
        [SerializeField] private Image priceIcon;
        [SerializeField] private TMP_Text price;


        public ArenaShopItemData data;

        private void OnEnable()
        {
            costBtn.onClick.AddListener(OnCost);
        }

        private void OnDisable()
        {
            costBtn.onClick.RemoveListener(OnCost);
        }

        public async void SetData(ArenaShopItemData _data)
        {
            this.data = _data;

            inv_Item.SetData(_data.gameItemList[0]);
            txtItemName.text = data.gameItemList[0].GetName();
            txtItemDesc.text = data.gameItemList[0].GetDescription();

            txtItemName.color = CBAColorUtil.Instance.GetColor(_data.gameItemList[0].GetQuality());

            if (_data.CostItem.Type == GameItemType.Resource && _data.CostItem.Id == ResourceId.Diamond)
            {
                priceIcon.sprite = await SpriteProxy.GetPropIcon(_data.CostItem.Id);
            }
            else
            {
                priceIcon.sprite = await _data.CostItem.GetIcon();
            }
            price.text = _data.CostItem.Count.ToString();

            if (_data.cfg.Limit != -1)
            {
                txtItemLimit.text = "限购: " + _data.boughtCount + "/" + _data.cfg.Limit;
            }
            else if (_data.cfg.WeekLimit != -1)
            {
                txtItemLimit.text = "每周限购: " + _data.boughtCount + "/" + _data.cfg.WeekLimit;
            }
            else
            {
                txtItemLimit.text = "";
            }

            if (_data.cfg.Stage > Player.BattleManager.newArenaInfo.ArenaStage)
            {
                txtItemOption.text = "段位达到" + Configs.ArenaStage.GetConfig(_data.cfg.Stage).Name + "可兑换";
                _data.Status = false;
                txtItemOption.gameObject.SetActive(true);
            }
            else
            {
                txtItemOption.gameObject.SetActive(false);
            }


            costBtn.interactable = _data.Status;
            costBtn.image.sprite = _data.Status ? await SpriteProxy.YellowBtnEnable : await SpriteProxy.YellowBtnDisable;

            priceIcon.sprite = await _data.CostItem.GetIcon();
        }

        private void OnCost()
        {

            string error = Player.PackageManager.IsGameItemEnough(data.CostItem);
            if (error != "")
            {
                //Tips.PopTips(error);
                return;
            }

            NetworkManager.Instance.arenaBuy(data.cfg.Id, response =>
            {
                // 购买成功
                if (response.Succeed)
                {
                    if (data.shopInfo == null) data.shopInfo = new Protocol.ShopInfo();
                    data.shopInfo.Stock++;
                    data.shopInfo.Sid = data.cfg.Id;
                    EventManager.Instance.Dispatch(EventID.ClassicShopUIItemBuy, data);
                }
            });
        }
    }
}