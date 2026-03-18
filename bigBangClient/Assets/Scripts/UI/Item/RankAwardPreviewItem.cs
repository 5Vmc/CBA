using System;
using System.Collections.Generic;
using System.Linq;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GameItem;

namespace BigBang.UI
{
    public class RankAwardPreviewItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text descText;
        [SerializeField] private GameObject rank;
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private Image rankIcon;
        [SerializeField] private List<InventoryItem> items;

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        public async void SetData(ActivityTopRewardConfig cfg)
        {
            if (cfg == null) return;
            if (cfg.Min == cfg.Max && cfg.Min <= 3) {
                int rankNumber = cfg.Min;
                rankIcon.sprite = await SpriteProxy.GetRank(rankNumber);
                rankText.gameObject.SetActive(false);
                rankIcon.gameObject.SetActive(true);
            }
            else {
                rankText.gameObject.SetActive(true);
                rankIcon.gameObject.SetActive(false);
                if (cfg.Min == cfg.Max)
                {
                    rankText.text = cfg.Min.ToString();
                }
                else {
                    rankText.text = cfg.Min.ToString() + "-" + cfg.Max.ToString();
                }
                
            }

            var rewards = GameItemUtils.CreateGameItems(cfg.Rewards).ToList();
            for (int i = 0; i < 4; i++)
            {
                items[i].gameObject.SetActive(i < rewards.Count);
                if (i < rewards.Count) {
                    items[i].SetGameItemData(rewards[i]);
                }
                
            }
        }
    }
}