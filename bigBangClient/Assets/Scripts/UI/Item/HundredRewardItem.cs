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
    public class HundredRewardItem : MonoBehaviour
    {
        [SerializeField] private Image bgImageOther = null;
        [SerializeField] private Image bgImage1 = null;
        [SerializeField] private Image bgImage2 = null;
        [SerializeField] private Image bgImageType1 = null;
        [SerializeField] private Image bgTitleImageType2 = null;
        [SerializeField] private TMP_Text bgTitleTextType2 = null;
        [SerializeField] private Image rankBgImageType2 = null;
        [SerializeField] private Image rankBgImageType3 = null;
        [SerializeField] private TMP_Text rankTextType3 = null;
        [SerializeField] private TMP_Text rankTextType2 = null;
        [SerializeField] private TMP_Text rankTextType1 = null;
        [SerializeField] private Image rankImgType21 = null;
        [SerializeField] private Image rankImgType22 = null;
        [SerializeField] private Image rankImgType31 = null;
        [SerializeField] private Image rankImgType32 = null;
        [SerializeField] private List<InventoryItem> inventoryItemList;

        [HideInInspector] public HundredRewardConfig hundredRewardConfig = null;
        [HideInInspector] public int index = 0;

        public void SetData(HundredRewardConfig hundredRewardConfig, int index)
        {
            this.hundredRewardConfig = hundredRewardConfig;
            this.index = index;

            HideAll();
            switch ((HundredRewardType)hundredRewardConfig.Type)
            {
                case HundredRewardType.Fight1:
                    {
                        bgImageType1.gameObject.SetActive(true);
                        rankTextType1.gameObject.SetActive(true);
                        rankTextType1.text = hundredRewardConfig.Name;
                    }
                    break;
                case HundredRewardType.Fight2:
                    {
                        bgTitleImageType2.gameObject.SetActive(true);
                        bgTitleTextType2.gameObject.SetActive(true);
                        if (index == 0)
                        {
                            rankImgType21.gameObject.SetActive(true);
                            bgImage1.gameObject.SetActive(true);
                        }
                        else if (index == 1)
                        {
                            rankImgType22.gameObject.SetActive(true);
                            bgImage2.gameObject.SetActive(true);
                        }
                        else
                        {
                            rankBgImageType2.gameObject.SetActive(true);
                            rankTextType2.gameObject.SetActive(true);
                            bgImageOther.gameObject.SetActive(true);
                            rankTextType2.text = hundredRewardConfig.Name;
                        }
                    }
                    break;
                case HundredRewardType.Fight3:
                    {
                        if (index == 0)
                        {
                            rankImgType31.gameObject.SetActive(true);
                            bgImage1.gameObject.SetActive(true);
                        }
                        else if (index == 1)
                        {
                            rankImgType32.gameObject.SetActive(true);
                            bgImage2.gameObject.SetActive(true);
                        }
                        else
                        {
                            rankBgImageType3.gameObject.SetActive(true);
                            rankTextType3.gameObject.SetActive(true);
                            bgImageOther.gameObject.SetActive(true);
                            rankTextType3.text = hundredRewardConfig.Name;
                        }
                    }
                    break;
            }

            SetReward();
        }
        private void HideAll()
        {
            bgImageOther.gameObject.SetActive(false);
            bgImage1.gameObject.SetActive(false);
            bgImage2.gameObject.SetActive(false);
            bgImageType1.gameObject.SetActive(false);
            bgTitleImageType2.gameObject.SetActive(false);
            bgTitleTextType2.gameObject.SetActive(false);
            rankBgImageType2.gameObject.SetActive(false);
            rankBgImageType3.gameObject.SetActive(false);
            rankTextType3.gameObject.SetActive(false);
            rankTextType2.gameObject.SetActive(false);
            rankTextType1.gameObject.SetActive(false);
            rankImgType21.gameObject.SetActive(false);
            rankImgType22.gameObject.SetActive(false);
            rankImgType31.gameObject.SetActive(false);
            rankImgType32.gameObject.SetActive(false);
        }
        private void SetReward()
        {
            if (hundredRewardConfig == null) return;
            var rewards = hundredRewardConfig.ItemContent.Split('|');
            if (rewards.Length > 3) Debug.LogWarning("HundredRewardItem , SetReward , only can show 3 item , but {0} in config , hundredRewardConfig.Id = {1}".SafeFormat(rewards.Length, hundredRewardConfig.Id));
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