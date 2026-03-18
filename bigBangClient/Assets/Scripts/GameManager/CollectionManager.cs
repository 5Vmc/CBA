using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Protocol;
using UnityEngine;
using Utils;

namespace BigBang
{
    /// <summary>
    /// 数字藏品的管理类
    /// </summary>
    public class CollectionManager : BabuSingleton<CollectionManager>
    {
        public void Clear()
        {
            selectedPlayerCard = null;
            selectedPlayerCardIndex = -1;
            collectionCardInfoList.Clear();
            noServerData = true;
        }

        public PlayerCard selectedPlayerCard = null;
        public int selectedPlayerCardIndex = -1;

        List<PlayerCardInfo> collectionCardInfoList = new();

        public List<PlayerCard> GetCollectionUIData()
        {
            List<PlayerCard> playerCardList = new();
            List<PlayerCard> playerCardListInBag = Player.CardManager.GetCollectionCard();
            List<PlayerCard> playerCardListInCollection = CollectionManager.Instance.GetCollectionPlayerCardList();
            playerCardList.AddRange(playerCardListInBag);
            playerCardList.AddRange(playerCardListInCollection);
            playerCardList = playerCardList.OrderByDescending(card => card.Config.Quality)
                .ThenByDescending(card => card.FightPoint)
                .ThenBy(card => card.CardId).ToList();
            return playerCardList;
        }

        public List<PlayerCard> GetCollectionPlayerCardList()
        {
            List<PlayerCard> playerCardList = new();
            foreach (PlayerCardInfo playerCardInfo in collectionCardInfoList)
            {
                PlayerCard playerCard = PlayerCard.GetCollectionCard(playerCardInfo);
                playerCardList.Add(playerCard);
            }
            return playerCardList;
        }

        public bool noServerData = true;
        public void GetCollectionList(Action callback = null)
        {
            NetworkManager.Instance.GetPropCards((GetPropCardsResponse getPropCardsResponse) =>
            {
                noServerData = false;
                collectionCardInfoList.Clear();
                collectionCardInfoList.AddRange(getPropCardsResponse.CardList);
                EventManager.Instance.Dispatch(EventID.RefreshCollectionUI);
                callback?.Invoke();
            });
        }

        public void UpCard(int cardId, Action callback = null)
        {
            NetworkManager.Instance.SaleCard(cardId, (SaleCardResponse saleCardResponse) =>
            {
                if (saleCardResponse.Succeed)
                {
                    collectionCardInfoList.Add(saleCardResponse.Card);
                    PlayerCard card = Player.CardManager.RemoveCard(saleCardResponse.Card.CardId);
                    List<Formation> formationList = Player.FightManager.FormationController.GetAllFormationList();
                    foreach (Formation formation in formationList)
                    {
                        formation.RemoveCard(cardId);
                    }
                    EventManager.Instance.Dispatch(EventID.RefreshCollectionUI);
                    EventManager.Instance.Dispatch(EventID.RefreshWindow, 0);
                }
                else
                {
                    Debug.LogWarning("CollectionManager , DownCard , usePropCardResponse.Succeed == false");
                    CollectionManager.Instance.noServerData = true;
                    EventManager.Instance.Dispatch(EventID.RefreshCollectionUI);
                    CollectionManager.Instance.GetCollectionList();
                    Tips.PopTips("数字藏品信息已更改");
                }
                callback?.Invoke();
            });
        }

        public void DownCard(string propId, Action callback = null)
        {
            NetworkManager.Instance.UsePropCard(propId, (UsePropCardResponse usePropCardResponse) =>
            {
                if (usePropCardResponse.Succeed)
                {
                    Player.CardManager.AddNewCard(usePropCardResponse.Card.CardId, usePropCardResponse.Card);
                    int index = collectionCardInfoList.FindIndex(info => info.PropId == propId);
                    if (index >= 0)
                    {
                        collectionCardInfoList.RemoveAt(index);
                        EventManager.Instance.Dispatch(EventID.RefreshCollectionUI);
                        EventManager.Instance.Dispatch(EventID.RefreshWindow, 0);
                    }
                    else
                    {
                        Debug.LogWarning("CollectionManager , DownCard , index < 0");
                        CollectionManager.Instance.noServerData = true;
                        EventManager.Instance.Dispatch(EventID.RefreshCollectionUI);
                        CollectionManager.Instance.GetCollectionList();
                        Tips.PopTips("数字藏品信息已更改");
                    }
                }
                else
                {
                    Debug.LogWarning("CollectionManager , DownCard , usePropCardResponse.Succeed == false");
                    CollectionManager.Instance.noServerData = true;
                    EventManager.Instance.Dispatch(EventID.RefreshCollectionUI);
                    CollectionManager.Instance.GetCollectionList();
                    Tips.PopTips("数字藏品信息已更改");
                }
                callback?.Invoke();
            });
        }

    }
}