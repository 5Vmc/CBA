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

    public class FBTowerRaidResultItem : MonoBehaviour
    {
        [SerializeField] private RectTransform fBTowerRaidResultItem = null;
        [SerializeField] private Image bgImage = null;
        [SerializeField] private TMP_Text chapterText = null;
        [SerializeField] private TMP_Text levelText = null;
        [SerializeField] private HorizontalLayoutGroup inventoryLayout = null;
        [SerializeField] private List<InventoryItem> inventoryList = new();


        public TowerLevelData towerLevelData;

        public void SetData(TowerLevelData towerLevelData)
        {
            this.towerLevelData = towerLevelData;
            chapterText.text = towerLevelData.towerChapterConfig.Name;
            int level = towerLevelData.towerConfig.Id % 100;
            levelText.text = "第<color=#fed701>{0}</color>关".SafeFormat(level);
            List<GameItem> rewardGameItemList = towerLevelData.rewardGameItemList;
            for (int i = 0; i < inventoryList.Count; i++)
            {
                if(rewardGameItemList.Count > i)
                {
                    inventoryList[i].gameObject.SetActive(true);
                    inventoryList[i].SetData(rewardGameItemList[i]);
                }
                else
                {
                    inventoryList[i].gameObject.SetActive(false);
                }
            }
        }
    }
}