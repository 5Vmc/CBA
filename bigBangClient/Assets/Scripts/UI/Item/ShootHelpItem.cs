using System;
using System.Collections.Generic;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class ShootHelpItem : MonoBehaviour
    {
        [SerializeField] private Image bgImageType1 = null;
        [SerializeField] private TMP_Text rankTextType1 = null;
        [SerializeField] private List<InventoryItem> inventoryItemList;

        [HideInInspector] public ShootGameStageConfig shootGameStageConfig = null;
        [HideInInspector] public int index = 0;

        public void SetData(ShootGameStageConfig shootGameStageConfig, int index)
        {
            this.shootGameStageConfig = shootGameStageConfig;
            this.index = index;

            rankTextType1.text = "难度等级{0}".SafeFormat(shootGameStageConfig.Id);

            SetReward();
        }
        private void SetReward()
        {
            if (shootGameStageConfig == null) return;
            var rewards = shootGameStageConfig.Reward.Split('|');
            if (rewards.Length > 3) Debug.LogWarning("ShootHelpItem , SetReward , only can show 3 item , but {0} in config , shootHelpConfig.Id = {1}".SafeFormat(rewards.Length, shootGameStageConfig.Id));
            for (int i = 0; i < 3; i++)
            {
                inventoryItemList[i].gameObject.SetActive(i < rewards.Length);
                if (i >= rewards.Length) continue;
                var itemData = rewards[i].Split(':');
                var gameItem = GameItemUtils.CreateGameItem((GameItemType)int.Parse(itemData[0]), int.Parse(itemData[1]), int.Parse(itemData[2]));
                inventoryItemList[i].SetGameItemViews(gameItem);
                inventoryItemList[i].SetGameItemData(gameItem);
            }
        }
    }
}