using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Globalization;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using Babu;
using GameConfig;
using Utils.GameItem;
using System.Linq;
using GameConfig.Config;

namespace BigBang.UI
{

    public class ArenaExShopUI : APanelController
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private ClassicArenaShopItemAdapter shopItemAdapter;
        [SerializeField] private TMP_Text txtMoney;
        [SerializeField] private BabuToggleGroup bottomToggleGroup;

        //private ArenaExShopUIAnim Anim; 
        public ArenaShopUIAnim Anim;

        protected override void Awake()
        {

        }

        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            SetArenaShop(bottomToggleGroup.EnableIndex);
        }

        public void OnBuyItem(object[] args)
        {
            ArenaShopItemData data = (ArenaShopItemData)args[0];
            //记录购买次数
            ShopInfo info = Player.BattleManager.newArenaInfo.ShopList.FirstOrDefault(item => item.Sid == data.cfg.Id);
            if (info == null)
            {
                Player.BattleManager.newArenaInfo.ShopList.Add(data.shopInfo);
            }
            else
            {
                info.Stock = data.shopInfo.Stock;
            }

            // 刷新商店界面
            //Babu.EventManager.Instance.Dispatch(EventID.OnRefreshItemShop);
            // 设置获得项目

            List<Utils.GameItem.GameItem> obtainList = GameItemUtils.CreateGameItems(data.cfg.Item).ToList<Utils.GameItem.GameItem>();
            // 打开收益界面
            UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(obtainList));

            SetArenaShop(bottomToggleGroup.EnableIndex);
        }

        private void SetArenaShop(int type)
        {
            List<ArenaShopItemData> shopItems = new List<ArenaShopItemData>();
            this.txtMoney.text = Player.PackageManager.GetGoodsNumber(400501).ToString();
            List<ArenaShopConfig> itemConfigs;
            if (type == 1)
            {
                //日常商店
                itemConfigs = Configs.ArenaShop.GetConfigList().FindAll(p => p.Id <= 100);
            }
            else
            {
                //里程商店
                itemConfigs = Configs.ArenaShop.GetConfigList().FindAll(p => p.Id > 100);
            }

            foreach (var cfg in itemConfigs)
            {
                ShopInfo _info = Player.BattleManager.newArenaInfo.ShopList.FirstOrDefault(info => info.Sid == cfg.Id);

                ArenaShopItemData _data = new ArenaShopItemData(cfg, _info);
                shopItems.Add(_data);
            }

            shopItems = shopItems.OrderBy(item => item.cfg.Stage).ThenByDescending(item => item.cfg.Id).ToList();
            shopItemAdapter.SetData(shopItems);
        }
        protected override void AddListeners()
        {
            closeButton.onClick.AddListener(OnClickCloseBtn);
            bottomToggleGroup.OnValueChanged += OnToggleChanged;
            arenaMoneyBtn.OnClick += OnClickArenaMoneyObj;
            EventManager.Instance.Register(EventID.ClassicShopUIItemBuy, OnBuyItem);
            //这里不需要注册小红点事件，可以直接用OnBuyItem来触发
            //EventManager.Instance.Register(EventID.RefreshUIRedDot, refreshRedDot);
        }

        protected override void RemoveListeners()
        {
            closeButton.onClick.RemoveListener(OnClickCloseBtn);
            bottomToggleGroup.OnValueChanged -= OnToggleChanged;
            arenaMoneyBtn.OnClick -= OnClickArenaMoneyObj;
            EventManager.Instance.Unregister(EventID.ClassicShopUIItemBuy, OnBuyItem);
        }

        protected override void OnPropertiesSet()
        {

            SetArenaShop(bottomToggleGroup.EnableIndex);
            shopItemAdapter.PlayAnim();
        }

        private void OnClickCloseBtn()
        {
            shopItemAdapter.PlayExit();
            UIController.Instance.HidePanel<ArenaExShopUI>();
        }

        [SerializeField] private BabuButton arenaMoneyBtn = null;
        private void OnClickArenaMoneyObj(BabuButton button)
        {
            ItemtipsUIProperties itemtipsUIProperties = new ItemtipsUIProperties((int)GameItemType.Goods, GoodsId.ArenaMoneyId, Player.PackageManager.GetGoodsNumber(GoodsId.ArenaMoneyId));
            itemtipsUIProperties.SetPos(arenaMoneyBtn.transform, new Vector3(0, -80f, 0));
            UIController.Instance.OpenWindow<ItemtipsUI>(itemtipsUIProperties);
        }
    }
}