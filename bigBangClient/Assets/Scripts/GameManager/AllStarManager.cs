using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Google.Protobuf.Collections;
using Protocol;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace BigBang
{
    /// <summary>
    /// 全明星的管理类
    /// </summary>
    public class AllStarManager : BabuSingleton<AllStarManager>
    {
        /// <summary> 阵营 </summary>
        public enum Area
        {
            /// <summary> 北区 </summary>
            North = 1,
            /// <summary> 南区 </summary>
            South = 2,
        }

        /// <summary> 类型 </summary>
        public enum Type
        {
            /// <summary> 票王 </summary>
            Up = 1,
            /// <summary> 首发 </summary>
            First = 2,
            /// <summary> 替补 </summary>
            Substitute = 3,
            /// <summary> 其他 </summary>
            Other = 4,
        }

        public enum Stage
        {
            /// <summary> 活动开始前，或者服务器卡了 </summary>
            NotOpen,
            /// <summary> 可报名 </summary>
            CanSign,
            /// <summary> 游戏进行中 </summary>
            Playing,
            /// <summary> 结算奖励后 </summary>
            Ending,
            /// <summary> 活动消失后 </summary>
            Closed
        }

        public string GetAreaName(Area area)
        {
            switch (area)
            {
                case Area.North: return "北区";
                case Area.South: return "南区";
            }
            return "";
        }

        private bool isInited = false;
        /// <summary>
        /// 配置表加载完成后，对数据进行预处理
        /// </summary>
        public void InitOnce(bool forceInit = true)
        {

            if (isInited && !forceInit) return;
            isInited = true;

        }

        /// <summary> 分组 </summary>
        public int group = 0;
        /// <summary> 在场上的卡牌 </summary>
        public Dictionary<PositionSeparatedType, PlayerCard> usingCardPositionIdDic = new();
        /// <summary> 服务器记录的当前总战力 </summary>
        public int savedTotalNowCombatInServer = 0;
        /// <summary> 服务器记录的最高总战力 </summary>
        public int savedTotalMaxCombatInServer = 0;
        /// <summary> 领取过的战力奖励 ID </summary>
        public HashSet<int> strengthRewardGotOptionSet = new();

        private SignActivityModuleNotify signActivityModuleNotify = null;
        public void Unpack(SignActivityModuleNotify signActivityModuleNotify)
        {
            this.signActivityModuleNotify = signActivityModuleNotify;
        }
        public void ProcessUnPack()
        {
            group = signActivityModuleNotify.AllStarGroup;
            usingCardPositionIdDic.Clear();
            strengthRewardGotOptionSet.Clear();
            bool hasPos = signActivityModuleNotify.AllStarCardsPos.Count > 0;
            for (int i = 0; i < signActivityModuleNotify.AllStarCards.Count; i++)
            {
                int cardId = signActivityModuleNotify.AllStarCards[i];
                PlayerCard playerCard = Player.CardManager.GetCard(cardId);
                if (playerCard == null)
                {
                    // Debug.LogWarning("AllStarManager , Unpack , playerCard == null , cardId = " + cardId);
                    // 全明星活动已结束，卡牌可能已经被删除
                    continue;
                }
                if (hasPos == false)
                {
                    if (usingCardPositionIdDic.ContainsKey(playerCard.GetAdaptPosition()))
                    {
                        Debug.LogWarning("AllStarManager , Unpack , usingCardPositionIdDic.ContainsKey(playerCard.GetAdaptPosition()) , playerCard.GetAdaptPosition() = " + playerCard.GetAdaptPosition());
                        continue;
                    }
                    usingCardPositionIdDic.Add(playerCard.GetAdaptPosition(), playerCard);
                }
                else
                {
                    if (i >= signActivityModuleNotify.AllStarCardsPos.Count)
                    {
                        Debug.LogWarning("AllStarManager , Unpack , i >= signActivityModuleNotify.AllStarCardsPos.Count , i = " + i + " , signActivityModuleNotify.AllStarCardsPos.Count = " + signActivityModuleNotify.AllStarCardsPos.Count);
                        continue;
                    }
                    PositionSeparatedType position = PositionSeparatedType.All;
                    try
                    {
                        position = (PositionSeparatedType)signActivityModuleNotify.AllStarCardsPos[i];
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("AllStarManager , Unpack , prase PositionSeparatedType , signActivityModuleNotify.AllStarCardsPos[i] = " + signActivityModuleNotify.AllStarCardsPos[i]);
                        continue;
                    }
                    if (position == PositionSeparatedType.All)
                    {
                        Debug.LogWarning("AllStarManager , Unpack , position == PositionSeparatedType.All");
                        continue;
                    }
                    if (usingCardPositionIdDic.ContainsKey(position))
                    {
                        Debug.LogWarning("AllStarManager , Unpack , usingCardPositionIdDic.ContainsKey(position) , position = " + position);
                        continue;
                    }
                    usingCardPositionIdDic.Add(position, playerCard);
                }
            }
            savedTotalNowCombatInServer = signActivityModuleNotify.AllStarStrength;
            savedTotalMaxCombatInServer = signActivityModuleNotify.AllStarMaxStrength;
            foreach (var item in signActivityModuleNotify.AllStarStrengthRewards)
            {
                strengthRewardGotOptionSet.Add(item);
            }
            ActivityController.Instance.CheckRedDot_AllStarHome(ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.AllStarHome));
        }

        public GetAllStarInfoResponse serverData = null;
        public void GetServerData(System.Action callback = null)
        {
            NetworkManager.Instance.GetAllStarInfo((GetAllStarInfoResponse getAllStarInfoResponse) =>
            {
                serverData = getAllStarInfoResponse;
                CheckClearMidServerData();
                group = serverData.Group;
                callback?.Invoke();
            });
        }
        private void CheckClearMidServerData()
        {
            if (serverData != null && serverData.Clear == true)
            {
                group = serverData.Group;
                usingCardPositionIdDic.Clear();
                strengthRewardGotOptionSet.Clear();
                savedTotalNowCombatInServer = 0;
                savedTotalMaxCombatInServer = 0;
            }
        }

        public void UseCard(PlayerCard playerCard, PositionSeparatedType selectPosition, Action action)
        {
            PositionSeparatedType usingPosition = PositionSeparatedType.All;
            foreach (var item in usingCardPositionIdDic)
            {
                if (item.Value == playerCard)
                {
                    usingPosition = item.Key;
                    break;
                }
            }
            if (usingPosition != PositionSeparatedType.All)
            {
                usingCardPositionIdDic.Remove(usingPosition);
            }

            if (usingCardPositionIdDic.ContainsKey(selectPosition))
            {
                usingCardPositionIdDic[selectPosition] = playerCard;
            }
            else
            {
                usingCardPositionIdDic.Add(selectPosition, playerCard);
            }

            RefreshSavedTotalCombatInServer();
            action?.Invoke();
        }

        public void SyncAllStarData(Action action = null)
        {
            List<int> cardIdList = new();
            List<int> cardPosList = new();
            foreach (var item in usingCardPositionIdDic)
            {
                if (item.Key == PositionSeparatedType.All) continue;
                if (item.Value == null) continue;
                cardIdList.Add(item.Value.Config.Id);
                cardPosList.Add((int)item.Key);
            }
            if (cardIdList.Count == 0)
            {
                action?.Invoke();
                return;
            }
            NetworkManager.Instance.SyncAllStarData(cardIdList, cardPosList, (SyncAllStarResponse) =>
            {
                if (SyncAllStarResponse.Success)
                {
                    int oldCombat = savedTotalNowCombatInServer;
                    RefreshSavedTotalCombatInServer();
                    int newCombat = savedTotalNowCombatInServer;
                    if (serverData.Area == 1)
                    {
                        serverData.North = serverData.North - oldCombat + newCombat;
                    }
                    if (serverData.Area == 2)
                    {
                        serverData.South = serverData.South - oldCombat + newCombat;
                    }
                    EventManager.Instance.Dispatch(EventID.RefreshAllStarHomePad);
                }
                else
                {
                    AllStarManager.Instance.GetServerData(() =>
                    {
                        EventManager.Instance.Dispatch(EventID.RefreshAllStarHomePad);
                    });
                }
                action?.Invoke();
            });
        }
        private List<int> CardDicToServerList(Dictionary<PositionSeparatedType, PlayerCard> cardDic)
        {
            List<int> usingList = new();
            foreach (var item in cardDic)
            {
                if (item.Key == PositionSeparatedType.All) continue;
                if (item.Value == null) continue;
                usingList.Add(item.Value.Config.Id);
            }
            return usingList;
        }

        private void RefreshSavedTotalCombatInServer()
        {
            savedTotalNowCombatInServer = GetNewTotalCombat();
            savedTotalMaxCombatInServer = Mathf.Max(savedTotalNowCombatInServer, savedTotalMaxCombatInServer);
        }
        public int GetNewTotalCombat()
        {
            int totalCombat = 0;
            foreach (PlayerCard playerCard in usingCardPositionIdDic.Values)
            {
                AllStarAdditionConfig allStarAdditionConfig = Configs.AllStarAddition.GetConfig(playerCard.Config.Id);
                if (allStarAdditionConfig == null)
                {
                    //Debug.LogWarning("AllStarManager , RefreshSavedTotalCombatInServer , allStarAdditionConfig == null , playerCard.Config.Id = " + playerCard.Config.Id);
                    continue;
                }
                int combat = Mathf.RoundToInt(playerCard.FightPoint * allStarAdditionConfig.Addition);
                //Debug.LogWarning($"playerCard.CardId = {playerCard.CardId} , playerCard.Config.Name = {playerCard.Config.Name} , playerCard.FightPoint = {playerCard.FightPoint} , finalCombat = {combat}");
                totalCombat += combat;
            }
            //Debug.LogWarning($"totalCombat = {totalCombat} , serverCombat = {savedTotalNowCombatInServer}");
            return totalCombat;
        }
        private readonly long hideCombatSeconds = 0;//86400 * 2;
        public bool IsCombatNeedHide
        {
            get
            {
                ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.AllStarHome);
                return activityData.StartTime + hideCombatSeconds - Utils.DataConvUtil.ServerTime > 0;
            }
        }
        public Stage GetStage()
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.AllStarHome);
            if (activityData == null) return Stage.Closed;
            if (serverData == null) return Stage.Closed;
            if (serverData.State == 0) return Stage.NotOpen;
            if (serverData.State == 2) return Stage.Closed;
            if (activityData.IsHide) return Stage.Closed;
            if (activityData.IsEnd) return Stage.Ending;
            if (serverData.Area == 0) return Stage.CanSign;
            return Stage.Playing;
        }

        public Area GetBestArea()
        {
            int northAreaBestCombat = GetFirstBestCombat(Area.North);
            int southAreaBestCombat = GetFirstBestCombat(Area.South);
            return northAreaBestCombat > southAreaBestCombat ? Area.North : Area.South;
        }
        private readonly List<PositionSeparatedType> allPos = new List<PositionSeparatedType>() { PositionSeparatedType.DaQianFeng, PositionSeparatedType.DeFenHouWei, PositionSeparatedType.KongQiuHouWei, PositionSeparatedType.XiaoQianFeng, PositionSeparatedType.ZhongFeng };
        private int GetFirstBestCombat(Area area)
        {
            int totalCombat = 0;
            foreach (PositionSeparatedType pos in allPos)
            {
                totalCombat += GetPosBestCombat(area, pos);
            }
            return totalCombat;
        }
        private int GetPosBestCombat(Area area, PositionSeparatedType positionSeparatedType)
        {
            List<AllStarAdditionConfig> allStarAdditionConfigList = Configs.AllStarAddition.GetConfigList()
                .Where((AllStarAdditionConfig allStarAdditionConfig) =>
                {
                    if (allStarAdditionConfig.Area != (int)area) return false;
                    PlayerCard playerCard = Player.CardManager.GetCard(allStarAdditionConfig.Id);
                    bool hasPlayer = playerCard != null;
                    if (hasPlayer == false) return false;
                    bool samePosition = playerCard.GetAdaptPosition() == positionSeparatedType;
                    return samePosition;
                })
                .ToList();
            int bestCombat = 0;
            foreach (AllStarAdditionConfig allStarAdditionConfig in allStarAdditionConfigList)
            {
                PlayerCard playerCard = Player.CardManager.GetCard(allStarAdditionConfig.Id);
                int combat = Mathf.RoundToInt(playerCard.FightPoint * allStarAdditionConfig.Addition);
                if (combat > bestCombat) bestCombat = combat;
            }
            return bestCombat;
        }
        /// <summary> 是否需要展示结尾弹窗 </summary>
        public bool IsNeedShowEnd
        {
            get
            {
                ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.AllStarHome);
                if (activityData == null) return false;
                if (activityData.IsEnd == false) return false;
                if (activityData.IsHide == true) return false;
                if (UnityEngine.PlayerPrefs.GetInt(PlayerPrefsKeys.AllStar2024ShowEnd + Player.GbId, 0) == 1) return false;
                return true;
            }
        }

        private Dictionary<Area, List<AllStarRankInfo>> rankDic = new();
        public List<AllStarRankInfo> GetRankInfoList(Area area)
        {
            if (rankDic.ContainsKey(area))
            {
                return rankDic[area];
            }
            return new();
        }
        public void GetRankData(Area area, System.Action callback = null)
        {
            NetworkManager.Instance.GetAllStarRank((int)area, (GetAllStarRankResponse getAllStarRankResponse) =>
            {
                Area serverArea = (Area)getAllStarRankResponse.Area;
                if (rankDic.ContainsKey(serverArea) == false)
                {
                    rankDic.Add(serverArea, getAllStarRankResponse.Ranks.ToList());
                }
                else
                {
                    rankDic[serverArea] = getAllStarRankResponse.Ranks.ToList();
                }
                callback?.Invoke();
            });
        }
        private List<AllStarRewardConfig> combatRankRewardList = new();
        public List<AllStarRewardConfig> CombatRankRewardList
        {
            get
            {
                if (combatRankRewardList == null) combatRankRewardList = new();
                if (combatRankRewardList.Count <= 0 && group > 0)
                {
                    combatRankRewardList = Configs.AllStarReward.GetConfigList().Where((AllStarRewardConfig allStarRewardConfig) =>
                    {
                        if (allStarRewardConfig.Type != 2) return false;
                        if (allStarRewardConfig.Group != group) return false;
                        return true;
                    }).ToList();
                }
                return combatRankRewardList;
            }
        }
        public AllStarRewardConfig GetAllStarRewardConfigByRank(int rank)
        {
            for (int i = 0; i < CombatRankRewardList.Count; i++)
            {
                int minRank = 0;
                if (i > 0) minRank = CombatRankRewardList[i - 1].Option;
                int maxRank = CombatRankRewardList[i].Option;
                if (minRank < rank && rank <= maxRank) return CombatRankRewardList[i];
            }
            return null;
        }
        public bool IsCombatRewardCanGet()
        {
            bool canGetCombatReward = false;
            bool isSign = AllStarManager.Instance.savedTotalNowCombatInServer > 0;
            if (isSign)
            {
                int group = AllStarManager.Instance.group;
                if (group > 0)
                {
                    int type = 3;
                    AllStarRewardConfig allStarRewardConfig = Configs.AllStarReward.GetConfigList().FirstOrDefault((AllStarRewardConfig allStarRewardConfig) =>
                    {
                        if (allStarRewardConfig.Type != type) return false;
                        if (allStarRewardConfig.Group != group) return false;
                        if (AllStarManager.Instance.savedTotalNowCombatInServer < allStarRewardConfig.Option) return false;
                        if (AllStarManager.Instance.strengthRewardGotOptionSet.Contains(allStarRewardConfig.Option)) return false;
                        return true;
                    });
                    canGetCombatReward = allStarRewardConfig != null;
                }
            }
            return canGetCombatReward;
        }
    }
}