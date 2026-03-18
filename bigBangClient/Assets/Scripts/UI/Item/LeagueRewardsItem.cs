using System;
using System.Collections.Generic;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GameItem;

namespace BigBang.UI
{
    public class LeagueRewardsItem : MonoBehaviour
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

        public async void SetData(bool type, int id, int competitionId)
        {
            // 显示排名奖励
            if (type)
            {
                rank.SetActive(true);
                descText.gameObject.SetActive(false);

                if (competitionId == CompitionID.League)
                {
                    var cfg = Configs.LeagueRewardRank.GetConfig(id);
                    if (cfg == null) return;
                    var rewards = cfg.Reward.Split('|');
                    if (rewards.Length > 3) throw new Exception("错误");
                    int rankNumber = cfg.Rank;
                    if (rankNumber <= 3) rankIcon.sprite = await SpriteProxy.GetRank(rankNumber);
                    rankText.gameObject.SetActive(rankNumber > 3);
                    rankIcon.gameObject.SetActive(rankNumber <= 3);
                    rankText.text = rankNumber.ToString();
                    for (int i = 0; i < 3; i++)
                    {
                        items[i].gameObject.SetActive(i < rewards.Length);
                        if (i >= rewards.Length) continue;
                        var itemData = rewards[i].Split(':');
                        var gameItem = GameItemUtils.CreateGameItem((GameItemType)int.Parse(itemData[0]), int.Parse(itemData[1]), int.Parse(itemData[2]));
                        items[i].SetGameItemViews(gameItem);
                        items[i].SetGameItemData(gameItem);
                    }
                }
                else if(competitionId == CompitionID.Cup)
                {
                    var cfg = Configs.CupRewardRank.GetConfig(id);
                    if (cfg == null) return;
                    var rewards = cfg.Reward.Split('|');
                    if (rewards.Length > 3) throw new Exception("错误");
                    int rankNumber = cfg.Rank;
                    if (rankNumber <= 3) rankIcon.sprite = await SpriteProxy.GetRank(rankNumber);
                    rankText.gameObject.SetActive(rankNumber > 3);
                    rankIcon.gameObject.SetActive(rankNumber <= 3);
                    rankText.text = rankNumber.ToString();
                    for (int i = 0; i < 3; i++)
                    {
                        items[i].gameObject.SetActive(i < rewards.Length);
                        if (i >= rewards.Length) continue;
                        var itemData = rewards[i].Split(':');
                        var gameItem = GameItemUtils.CreateGameItem((GameItemType)int.Parse(itemData[0]), int.Parse(itemData[1]), int.Parse(itemData[2]));
                        items[i].SetGameItemViews(gameItem);
                        items[i].SetGameItemData(gameItem);
                    }
                }
            }
            // 显示其他奖励
            else
            {
                rank.SetActive(false);
                descText.gameObject.SetActive(true);

                if (competitionId == CompitionID.League)
                {
                    var cfg = Configs.LeagueRewardOther.GetConfig(id);
                    if (cfg == null) return;
                    var rewards = cfg.Reward.Split('|');
                    descText.text = cfg.DescText;
                    if (rewards.Length > 3) throw new Exception("错误");
                    for (int i = 0; i < 3; i++)
                    {
                        items[i].gameObject.SetActive(i < rewards.Length);
                        if (i >= rewards.Length) continue;
                        var itemData = rewards[i].Split(':');
                        var gameItem = GameItemUtils.CreateGameItem((GameItemType)int.Parse(itemData[0]), int.Parse(itemData[1]), int.Parse(itemData[2]));
                        items[i].SetGameItemViews(gameItem);
                        items[i].SetGameItemData(gameItem);
                    }
                }
                else if(competitionId == CompitionID.Cup)
                {
                    var cfg = Configs.CupRewardOther.GetConfig(id);
                    if (cfg == null) return;
                    var rewards = cfg.Reward.Split('|');
                    descText.text = cfg.DescText;
                    if (rewards.Length > 3) throw new Exception("错误");
                    for (int i = 0; i < 3; i++)
                    {
                        items[i].gameObject.SetActive(i < rewards.Length);
                        if (i >= rewards.Length) continue;
                        var itemData = rewards[i].Split(':');
                        var gameItem = GameItemUtils.CreateGameItem((GameItemType)int.Parse(itemData[0]), int.Parse(itemData[1]), int.Parse(itemData[2]));
                        items[i].SetGameItemViews(gameItem);
                        items[i].SetGameItemData(gameItem);
                    }
                }
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