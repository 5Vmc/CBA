using System;
using System.Collections.Generic;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GameItem;

namespace BigBang.UI
{
    public class ArenaRewardsItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text descText;
        [SerializeField] private GameObject rank;
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private Image rankIcon;
        [SerializeField] private List<InventoryItem> items;
        [SerializeField] private List<Button> itemsBtn;

        private void OnEnable()
        {
            itemsBtn[0].onClick.AddListener(OnItem1Click);
            itemsBtn[1].onClick.AddListener(OnItem2Click);
            itemsBtn[2].onClick.AddListener(OnItem3Click);
        }

        private void OnDisable()
        {
            itemsBtn[0].onClick.RemoveListener(OnItem1Click);
            itemsBtn[1].onClick.RemoveListener(OnItem2Click);
            itemsBtn[2].onClick.RemoveListener(OnItem3Click);
        }

        public async void SetData(ArenaRewardsItemModel modelData)
        {
            // 显示排名奖励
            switch (modelData.Type)
            {
                case ArenaStageRewardType.Daily:
                case ArenaStageRewardType.Promote:
                case ArenaStageRewardType.ActivityEnd:
                    rankText.gameObject.SetActive(false);
                    rank.SetActive(true);
                    ArenaStageConfigTable conf = Configs.ArenaStage;
                    string tIcon = conf.GetConfig(modelData.Data.Stage).Icon;
                    rankIcon.sprite = await SpriteProxy.GetArenaTierIcon(tIcon);
                    descText.gameObject.SetActive(false);
                    string[] rewards = modelData.Data.Reward.Split("|");
                    for (int i = 0; i < 3; i++)
                    {
                        items[i].gameObject.SetActive(i < rewards.Length);
                        if (i >= rewards.Length) continue;
                        var itemData = rewards[i].Split(':');
                        var gameItem = GameItemUtils.CreateGameItem((GameItemType)int.Parse(itemData[0]), int.Parse(itemData[1]), int.Parse(itemData[2]));
                        items[i].SetGameItemViews(gameItem);
                        items[i].SetGameItemData(gameItem);
                    }
                    break;
            }

            // 显示排名奖励
            switch (modelData.Type)
            {
                case ArenaStageRewardType.Daily:
                case ArenaStageRewardType.Promote:
                    ArenaStageConfigTable conf = Configs.ArenaStage;
                    var stageCfg = conf.GetConfig(modelData.Data.Stage);
                    rankText.text = string.Format("段位达到{0}", stageCfg.Name);
                    rankText.gameObject.SetActive(true);
                    break;
                case ArenaStageRewardType.ActivityEnd:
                    if (modelData.Data.RankMin > 0 && modelData.Data.RankMax > 0)
                    {
                        rankText.gameObject.SetActive(true);
                        if (modelData.Data.RankMin == modelData.Data.RankMax)
                        {
                            rankText.text = string.Format("第{0}名", modelData.Data.RankMin);
                        }
                        else
                        {
                            rankText.text = string.Format("第{0}~{1}名", modelData.Data.RankMin, modelData.Data.RankMax);
                        }
                    }
                    else if (modelData.Data.RankMin > 0)
                    {
                        rankText.gameObject.SetActive(true);
                        rankText.text = string.Format(">{0}名", modelData.Data.RankMin);
                    }
                    break;
            }
        }



        private void OnItem1Click()
        {
            var gameItem = items[0].GetGameItem();
            UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties((int)gameItem.Type, gameItem.Id, gameItem.Count));
        }

        private void OnItem2Click()
        {
            var gameItem = items[1].GetGameItem();
            UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties((int)gameItem.Type, gameItem.Id, gameItem.Count));
        }

        private void OnItem3Click()
        {
            var gameItem = items[2].GetGameItem();
            UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties((int)gameItem.Type, gameItem.Id, gameItem.Count));
        }
    }
}