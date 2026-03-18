using System.Data.SqlTypes;
using System;
using System.Collections.Generic;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System.Linq;
using Utils.GameItem;

namespace BigBang.UI
{
    public class PlayerFireUIProperties : WindowProperties
    {
        public PlayerCard Card { get; set; }

        public PlayerFireUIProperties(PlayerCard card)
        {
            Card = card;

        }
    }
    public class PlayerFireUI : AWindowController<PlayerFireUIProperties>
    {
        [SerializeField] private InventoryItem returnMoneyItem;
        [SerializeField] private InventoryItem[] returnGoodsItems;

        [SerializeField] private CardItem cardItem;
        [SerializeField] private Button fireBtn;

        [SerializeField] private Button closeBtn;

        protected override void OnPropertiesSet()
        {
            this.UpdateUI();
        }

        protected override void AddListeners()
        {
            fireBtn.onClick.AddListener(OnClickFireBtn);
            closeBtn.onClick.AddListener(OnClickCloseBtn);
        }

        protected override void RemoveListeners()
        {
            fireBtn.onClick.RemoveListener(OnClickFireBtn);
            closeBtn.onClick.RemoveListener(OnClickCloseBtn);
        }


        private void ClearUI(IList<PlayerCard> delCardList = null)
        {
            returnMoneyItem.gameObject.SetActive(false);
            for (int j = 0; j < returnGoodsItems.Length; j++)
            {
                returnGoodsItems[j].gameObject.SetActive(false);
            }
        }

        private void UpdateUI()
        {
            PlayerCard card = Properties.Card;
            cardItem.SetData(card);
            GameItem moneyItem = CardFirePad.ReturnMoney(card.Config.Quality, card.Quality, card.Star, card.EquipGrade, card.DefaultPosition);

            List<GameItem> goodsList = CardFirePad.ReturnGoods(card.Config.Quality, card.Quality, card.Star, card.EquipGrade, card.EquipLevels, card.DefaultPosition, card.Exp);

            if (moneyItem != null && moneyItem.Count > 0)
            {
                returnMoneyItem.SetData(moneyItem);
                returnMoneyItem.gameObject.SetActive(true);
            }
            else
            {
                returnMoneyItem.gameObject.SetActive(false);
            }


            goodsList.Sort(delegate (GameItem s1, GameItem s2)
            {
                return s2.GetQuality() - s1.GetQuality();
            });

            int i = 0;
            foreach (GameItem gi in goodsList)
            {
                if (i >= returnGoodsItems.Length) break;
                returnGoodsItems[i].gameObject.SetActive(true);
                returnGoodsItems[i].SetData(new GoodsData(gi.Id, gi.Count));
                i++;
            }
            for (int j = i; j < returnGoodsItems.Length; j++)
            {
                returnGoodsItems[j].gameObject.SetActive(false);
            }
        }

        private void OnClickFireBtn()
        {
            var owerList = Player.CardManager.GetCardList();

            if (owerList.Count - 1 <= GameConst.FIRE_MIN_LEFT)
            {
                Tips.PopTips($"俱乐部球员少于{GameConst.FIRE_MIN_LEFT}个。");
                return;
            }
            PlayerCard card = Properties.Card;
            if (card.IsStarter())
            {
                Tips.PopTips($"经典赛首发球员不能解雇。");
                return;
            }
            if (card.IsStarter1())
            {
                Tips.PopTips($"赛事首发球员不能解雇。");
                return;
            }
            if (card.IsStarter2())
            {
                Tips.PopTips($"排位赛首发球员不能解雇。");
                return;
            }
            if (card.IsStarter3())
            {
                Tips.PopTips($"篮球殿堂首发球员不能解雇。");
                return;
            }
            if (card.IsStarter4())
            {
                Tips.PopTips($"百分大战上场球员不能解雇。");
                return;
            }
            if (card.IsUsingInBounty)
            {
                Tips.PopTips($"悬赏任务已派遣球员不能解雇。");
                return;
            }
            if (card.SkillTrainRoomId != 0)
            {
                Tips.PopTips($"特级训练中的球员不能解雇。");
                return;
            }

            List<int> delList = new List<int>();
            delList.Add(card.CardId);
            NetworkManager.Instance.CardFire(delList, (resp) =>
            {
                Tips.PopTips("解雇成功");
                UIController.Instance.CloseWindow<PlayerFireUI>();
                OnFireResp(resp.CardIdList);
            });
        }

        private void OnClickCloseBtn()
        {
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<PlayerFireUI>();
        }


        private void OnFireResp(IList<int> fireList)
        {
            List<PlayerCard> delList = new List<PlayerCard>();
            foreach (int cardId in fireList)
            {
                PlayerCard card = Player.CardManager.RemoveCard(cardId);
                if (card != null)
                {
                    delList.Add(card);
                }
            }
            this.ClearUI(delList);
        }


    }
}