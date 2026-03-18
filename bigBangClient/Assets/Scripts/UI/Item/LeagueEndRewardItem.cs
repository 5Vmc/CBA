using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

public class LeagueEndRewardItem : MonoBehaviour
{
    [SerializeField] private List<InventoryItem> inventoryItemList = null;
    [SerializeField] private HorizontalLayoutGroup goodsPanel = null;
    [SerializeField] private TMP_Text emptyText = null;

    public void SetData(string content)
    {
        SetData(GameItemUtils.CreateGameItems(content).ToList());
    }
    public void SetNoData()
    {
        goodsPanel.gameObject.SetActive(false);
        emptyText.gameObject.SetActive(true);
    }
    public void SetData(List<GameItem> gameItemList)
    {
        bool hasData = gameItemList.Count > 0;
        goodsPanel.gameObject.SetActive(hasData);
        if (hasData)
        {
            GameItemUtils.SetRewards(inventoryItemList, gameItemList);
        }
        emptyText.gameObject.SetActive(!hasData);
    }
}
