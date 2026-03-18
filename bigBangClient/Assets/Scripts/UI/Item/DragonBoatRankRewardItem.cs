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
    public class DragonBoatRankRewardItem : MonoBehaviour
    {
        [SerializeField] private Image bgImageOther = null;
        [SerializeField] private Image bgImage1 = null;
        [SerializeField] private Image bgImage2 = null;
        [SerializeField] private Image bgImage3 = null;
        [SerializeField] private Image rankBgImage = null;
        [SerializeField] private TMP_Text rankText = null;
        [SerializeField] private Image rankImg1 = null;
        [SerializeField] private Image rankImg2 = null;
        [SerializeField] private Image rankImg3 = null;
        [SerializeField] private List<InventoryItem> inventoryItemList;

        [HideInInspector] public DragonBoatRewardConfig dragonBoatRewardConfig = null;
        [HideInInspector] public int index = 0;

        public void SetData(DragonBoatRewardConfig dragonBoatRewardConfig, int index)
        {
            this.dragonBoatRewardConfig = dragonBoatRewardConfig;
            this.index = index;

            HideAll();
            if (index == 0)
            {
                rankImg1.gameObject.SetActive(true);
                bgImage1.gameObject.SetActive(true);
            }
            else if (index == 1)
            {
                rankImg2.gameObject.SetActive(true);
                bgImage2.gameObject.SetActive(true);
            }
            else if (index == 2)
            {
                rankImg3.gameObject.SetActive(true);
                bgImage3.gameObject.SetActive(true);
            }
            else
            {
                rankBgImage.gameObject.SetActive(true);
                rankText.gameObject.SetActive(true);
                bgImageOther.gameObject.SetActive(true);
                rankText.text = dragonBoatRewardConfig.Name;
            }

            SetReward();
        }
        private void HideAll()
        {
            bgImageOther.gameObject.SetActive(false);
            bgImage1.gameObject.SetActive(false);
            bgImage2.gameObject.SetActive(false);
            bgImage3.gameObject.SetActive(false);
            rankBgImage.gameObject.SetActive(false);
            rankText.gameObject.SetActive(false);
            rankImg1.gameObject.SetActive(false);
            rankImg2.gameObject.SetActive(false);
            rankImg3.gameObject.SetActive(false);
        }
        private void SetReward()
        {
            if (dragonBoatRewardConfig == null) return;
            GameItemUtils.SetRewards(inventoryItemList, dragonBoatRewardConfig.Rewards);
        }
    }
}