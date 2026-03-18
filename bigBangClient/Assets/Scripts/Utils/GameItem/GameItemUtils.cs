using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Babu.Config;
using BigBang;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Google.Protobuf;
using Google.Protobuf.Collections;
using UnityEngine;

namespace Utils.GameItem
{
    public class GameItemUtils
    {
        public static GameItem CreateGameItem(GameItemType type, int id, int count)
        {
            switch (type)
            {
                case GameItemType.None:
                    return null;
                case GameItemType.Goods:
                    return new GoodsGameItem(id, count);
                case GameItemType.Card:
                    return new CardGameItem(id, count);
                case GameItemType.Resource:
                    return new ResourceGameItem(id, count);
                case GameItemType.Honour:
                    return new HonourGameItem(id, count);
                case GameItemType.NFT:
                    return null;
            }
            return null;
        }

        public static GameItem CreateGameItem(string content)
        {
            var strs = content.Split(':');
            if (strs.Length != 3)
            {
                Debug.LogWarning("GameItemUtils , CreateGameItem , strs.Length != 3 , content = " + content);
                return null;
            }
            try
            {
                var type = int.Parse(strs[0]);
                var id = int.Parse(strs[1]);
                var count = int.Parse(strs[2]);
                return CreateGameItem((GameItemType)type, id, count);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("GameItemUtils , CreateGameItem , int.Parse Error , content = " + content);
                return null;
            }
        }

        public static IEnumerable<GameItem> CreateGameItems(string contents)
        {
            foreach (var item in contents.Split('|'))
            {
                GameItem gameItem = CreateGameItem(item);
                if (gameItem == null) continue;
                yield return gameItem;
            }
        }
        /// <summary>
        /// 设置道具图标
        /// </summary>
        public static void SetRewards(List<InventoryItem> boxInventoryList, string rewardStr)
        {
            List<GameItem> gameItemList = GameItemUtils.CreateGameItems(rewardStr).ToList();
            SetRewards(boxInventoryList, gameItemList);
        }
        /// <summary>
        /// 设置道具图标
        /// </summary>
        public static void SetRewards(List<InventoryItem> boxInventoryList, List<GameItem> gameItemList)
        {
            for (int i = 0; i < boxInventoryList.Count; i++)
            {
                InventoryItem inventoryItem = boxInventoryList[i];
                if (i < gameItemList.Count)
                {
                    GameItem gameItem = gameItemList[i];
                    inventoryItem.SetData(gameItemList[i]);
                    inventoryItem.gameObject.SetActive(true);
                }
                else
                {
                    inventoryItem.gameObject.SetActive(false);
                }
            }
        }
        /// <summary>
        /// 返回道具的名字和数量
        /// 钻石x200、体力x100、高级经验书x10
        /// </summary>
        public static string GetNameCountStr(string rewardStr)
        {
            List<GameItem> gameItemList = GameItemUtils.CreateGameItems(rewardStr).ToList();
            return GetNameCountStr(gameItemList);
        }
        /// <summary>
        /// 返回道具的名字和数量
        /// 钻石x200、体力x100、高级经验书x10
        /// </summary>
        public static string GetNameCountStr(List<GameItem> gameItemList)
        {
            string str = "";
            for (int i = 0; i < gameItemList.Count; i++)
            {
                GameItem gameItem = gameItemList[i];
                if (gameItem == null) continue;
                if (string.IsNullOrWhiteSpace(str) == false)
                {
                    str += "、";
                }
                str += gameItem.GetName();
                str += "x";
                str += gameItem.CountString();
            }
            return str;
        }

        public static IEnumerable<GameItem> UnPackList(RepeatedField<Protocol.GameItem> protoList)
        {
            for (int i = 0; i < protoList.Count; i++)
            {
                yield return CreateGameItem((GameItemType)protoList[i].Type, protoList[i].Id, protoList[i].Count);
            }
        }
        public static GameItem UnPack(Protocol.GameItem protoGameItem)
        {
            return CreateGameItem((GameItemType)protoGameItem.Type, protoGameItem.Id, protoGameItem.Count);
        }

        public static List<GameItem> MergeGameItemList(List<GameItem> gameItemList)
        {
            if (gameItemList == null || gameItemList.Count <= 0) return new();
            Dictionary<int, GameItem> gameItemDic = new();
            List<int> gameItemHashList = new();
            foreach (GameItem gameItem in gameItemList)
            {
                if (gameItem == null) continue;
                int gameItemHash = gameItem.Id * 100 + (int)gameItem.Type;
                if (gameItemDic.ContainsKey(gameItemHash))
                {
                    GameItem savedGameItem = gameItemDic[gameItemHash];
                    savedGameItem.Count += gameItem.Count;
                }
                else
                {
                    gameItemDic.Add(gameItemHash, gameItem);
                    gameItemHashList.Add(gameItemHash);
                }
            }
            List<GameItem> mergedGameItemList = new();
            foreach (int gameItemHash in gameItemHashList)
            {
                mergedGameItemList.Add(gameItemDic[gameItemHash]);
            }
            return mergedGameItemList;
        }

        public static GameItem ChangeCardToPiece(GameItem gameItem, bool forceChange = false)
        {
            if (gameItem == null) return null;
            if (gameItem.Type != GameItemType.Card) return gameItem;
            CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(gameItem.Id);
            if (cardModelConfig == null) return gameItem;
            if (cardModelConfig.PiecesCount <= 0) return gameItem;
            bool hasPlayerCard = Player.CardManager.GetCard(gameItem.Id) != null;
            if (forceChange || hasPlayerCard)
            {
                GameItem pieceGameItem = CreateGameItem(GameItemType.Goods, cardModelConfig.PiecesId, cardModelConfig.PiecesCount * gameItem.Count);
                return pieceGameItem;
            }
            return gameItem;
        }

        public static List<GameItem> ChangeCardToPiece(List<GameItem> gameItemList, bool forceChange = false)
        {
            if (gameItemList == null || gameItemList.Count <= 0) return new();
            List<GameItem> changedGameItemList = new();
            foreach (GameItem gameItem in gameItemList)
            {
                changedGameItemList.Add(ChangeCardToPiece(gameItem, forceChange));
            }
            changedGameItemList = MergeGameItemList(changedGameItemList);
            return changedGameItemList;
        }

        public static bool Equals(GameItem gameItem, GameItem gameItemOther, bool needCountSame = true)
        {
            if (gameItem.Type != gameItemOther.Type) return false;
            if (gameItem.Id != gameItemOther.Id) return false;
            if (needCountSame && gameItem.Count != gameItemOther.Count) return false;
            return true;
        }


    }
}