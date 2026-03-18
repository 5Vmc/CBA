using Babu;
using BigBang;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.GameItem;

public class RecruitShopPad : MonoBehaviour, IDataPad
{

    private void OnEnable()
    {
        EventManager.Instance.Register(EventID.ClassicShopUIItemBuy, OnBuyItem);
    }

    private void OnDisable()
    {
        EventManager.Instance.Unregister(EventID.ClassicShopUIItemBuy, OnBuyItem);
    }

    [SerializeField] private ClassicShopItemAdapter shopItemAdapter;
    public void SetData(object[] args = null)
    {
        List<ShopItemData> shopItems = new List<ShopItemData>();
        var itemConfigs = Configs.GameItemShop.GetConfigList();
        foreach (var cfg in itemConfigs)
        {
            shopItems.Add(new ShopItemData(cfg));
        }
        //球探商城，type = 4
        shopItemAdapter.SetData(shopItems, 6);
    }

    public void OnBuyItem(object[] args)
    {
        int shopItemId = (int)args[0];
        GameItemShopConfig item = Configs.GameItemShop.GetConfigList().Find(p => p.Id == shopItemId);
        // 刷新商店界面
        //Babu.EventManager.Instance.Dispatch(EventID.OnRefreshItemShop);
        // 设置获得项目

        List<Utils.GameItem.GameItem> obtainList = GameItemUtils.CreateGameItems(item.Item).ToList<Utils.GameItem.GameItem>();
        // 打开收益界面
        UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(obtainList));

        EventManager.Instance.Dispatch(EventID.OnResourceChange);
        SetData();
    }
}
