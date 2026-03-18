using System.Collections;
using System.Collections.Generic;
using BigBang.UI;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using Utils;
using Utils.GameItem;

public class PlayoffFinalsGuessHelpRewardItem : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText = null;
    [SerializeField] private List<InventoryItem> inventoryItemListWin = null;
    [SerializeField] private List<InventoryItem> inventoryItemListLose = null;

    public void SetData(FinalsGuessRewardConfig finalsGuessRewardConfig)
    {
        titleText.text = finalsGuessRewardConfig.Name;
        GameItemUtils.SetRewards(inventoryItemListWin, finalsGuessRewardConfig.SuccessReward);
        GameItemUtils.SetRewards(inventoryItemListLose, finalsGuessRewardConfig.FailReward);
    }
}
