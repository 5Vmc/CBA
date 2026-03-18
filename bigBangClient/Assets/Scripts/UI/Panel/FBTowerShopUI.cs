using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Globalization;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
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

    public class FBTowerShopUI : APanelController
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private ClassicShopItemAdapter shopItemAdapter;
        [SerializeField] private FBTowerShopUIAnim anim;

        protected override void Awake()
        {

        }

        public void OnBuyItem(object[] args)
        {
            int shopItemId = (int)args[0];
            GameItemShopConfig item = Configs.GameItemShop.GetConfigList().Find(p => p.Id == shopItemId);
            // 刷新商店界面
            //Babu.EventManager.Instance.Dispatch(EventID.OnRefreshItemShop);
            // 设置获得项目

            List<GameItem> obtainList = GameItemUtils.CreateGameItems(item.Item).ToList<GameItem>();
            // 打开收益界面
            UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(obtainList));

            refreshData();
            EventManager.Instance.Dispatch(EventID.OnCostTowerHoner);
            EventManager.Instance.Dispatch(EventID.OnResourceChange);
        }

        private void refreshData()
        {
            List<ShopItemData> shopItems = new List<ShopItemData>();
            var itemConfigs = Configs.GameItemShop.GetConfigList();
            foreach (var cfg in itemConfigs)
            {
                shopItems.Add(new ShopItemData(cfg));
            }
            //篮球殿堂，type = 7
            shopItemAdapter.SetData(shopItems, 7);
        }
        protected override void AddListeners()
        {
            closeButton.onClick.AddListener(OnClickCloseBtn);
            EventManager.Instance.Register(EventID.ClassicShopUIItemBuy, OnBuyItem);
            //这里不需要注册小红点事件，可以直接用OnBuyItem来触发
            //EventManager.Instance.Register(EventID.RefreshUIRedDot, refreshRedDot);
        }

        protected override void RemoveListeners()
        {
            closeButton.onClick.RemoveListener(OnClickCloseBtn);
            EventManager.Instance.Unregister(EventID.ClassicShopUIItemBuy, OnBuyItem);
        }

        protected override void OnPropertiesSet()
        {
            refreshData();
            shopItemAdapter.InitAnim();
            shopItemAdapter.PlayAnim();
            anim.PlayEnter();
        }

        private void OnClickCloseBtn()
        {
            shopItemAdapter.PlayExit();
            anim.PlayExit(() =>
            {
                UIController.Instance.HidePanel<FBTowerShopUI>();
            });
        }
    }
}