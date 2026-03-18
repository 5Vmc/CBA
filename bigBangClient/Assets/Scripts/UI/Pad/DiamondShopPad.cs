using BigBang;
using BigBang.UI;
using GameConfig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondShopPad : MonoBehaviour, IDataPad
{
    [SerializeField] private List<ShopDiamondItem> diamondItems;

    private void OnEnable()
    {
        Babu.EventManager.Instance.Register(EventID.OnRefreshDiamondShop, SetData);
    }

    private void OnDisable()
    {
        Babu.EventManager.Instance.Unregister(EventID.OnRefreshDiamondShop, SetData);
    }


    public void SetData(object[] args = null)
    {
        var diamondConfigs = Configs.DiamondShop.GetConfigList();
        int index = 0;
        foreach (var cfg in diamondConfigs)
        {
            diamondItems[index++].SetData(new ShopDiamondItemData(cfg));
        }
    }
}
