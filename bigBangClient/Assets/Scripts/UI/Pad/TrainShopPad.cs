using BigBang;
using BigBang.UI;
using GameConfig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainShopPad : MonoBehaviour, IDataPad
{
    private void OnEnable()
    {
        Babu.EventManager.Instance.Register(EventID.OnRefreshTrainShop, SetData);
    }

    private void OnDisable()
    {
        Babu.EventManager.Instance.Unregister(EventID.OnRefreshTrainShop, SetData);
    }

    [SerializeField] private List<ShopTrainItem> trainItems;
    // 刷新训练商店数据
    public void SetData(object[] args = null)
    {
        var trainConfigs = Configs.TrainShop.GetConfigList();
        int index = 0;
        foreach (var cfg in trainConfigs)
        {
            trainItems[index++].SetData(new ShopTrainItemData(cfg));
        }
    }
}
