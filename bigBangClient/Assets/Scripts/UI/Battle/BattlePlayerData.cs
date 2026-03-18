using System;
using System.Collections.Generic;
using Protocol;
using UnityEngine;

namespace BigBang
{


    //球队数据计算
    public class BattlePlayerData
    {
        #region 外部使用接口
        public Action onDataChange = null;
        #endregion

        #region 初始化

        public Dictionary<string, PlayerStat> playerStatDic = new();
        //public void Clear()
        //{
        //    playerStatDic.Clear();
        //    onDataChange?.Invoke();
        //}
        public PlayerStat GetPlayerStat(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                Debug.LogError("GetPlayerStat , string.IsNullOrWhiteSpace(playerId)");
                return new();
            }
            if (playerStatDic.ContainsKey(playerId))
            {
                return playerStatDic[playerId];
            }
            else
            {
                PlayerStat playerStat = new();
                playerStat.PlayerCardId = playerId;
                playerStatDic.Add(playerId, playerStat);
                return playerStat;
            }
        }


        #endregion

        #region 数据累加

        public void AddDrop(int stage, float aniTime)
        {
            FightQuaterInfo fightQuaterInfo = Player.BattleManager.fightInfoData.fightInfo.Result.Quarters[stage];
            GetInfoDataStage(fightQuaterInfo);
            onDataChange?.Invoke();
        }
        public void AddRound(FightPossessionInfo fightPossessionInfo)
        {
            AddInfoToPlayerStat(fightPossessionInfo);
            onDataChange?.Invoke();
        }

        #endregion

        #region 数据计算
        public void GetInfoDataStage(FightQuaterInfo fightQuaterInfo)//获取某一节的服务器回合统计
        {
            foreach (var possession in fightQuaterInfo.Possessions)
            {
                AddInfoToPlayerStat(possession);
            }
        }
        public void AddInfoToPlayerStat(FightPossessionInfo fightPossessionInfo)
        {
            AddActionToPlayerStat(fightPossessionInfo.EventId, fightPossessionInfo.PlayerCardId, fightPossessionInfo.Player2CardId);
        }
        public void AddActionToPlayerStat(int actionId, string PlayerCardId, string Player2CardId)//将Action添加到统计里
        {
            PlayerStat playerStatPlayer1 = null;
            PlayerStat playerStatPlayer2 = null;
            if (string.IsNullOrWhiteSpace(PlayerCardId) == false)
            {
                playerStatPlayer1 = GetPlayerStat(PlayerCardId);
            }
            if (string.IsNullOrWhiteSpace(Player2CardId) == false)
            {
                playerStatPlayer2 = GetPlayerStat(Player2CardId);
            }
            switch ((FightEvent)actionId)
            {
                case FightEvent.FT:
                    playerStatPlayer1.Point += 1;
                    playerStatPlayer1.FtTotal += 1;
                    playerStatPlayer1.FtCount += 1;
                    break;
                case FightEvent.FG:
                    playerStatPlayer1.Point += 2;
                    playerStatPlayer1.FgTotal += 1;
                    playerStatPlayer1.FgCount += 1;
                    break;
                case FightEvent.TP:
                    playerStatPlayer1.Point += 3;
                    playerStatPlayer1.TpTotal += 1;
                    playerStatPlayer1.TpCount += 1;
                    break;
                case FightEvent.FG_AST:
                    playerStatPlayer2.Assist += 1;
                    playerStatPlayer1.Point += 2;
                    playerStatPlayer2.FgTotal += 1;
                    playerStatPlayer2.FgCount += 1;
                    break;
                case FightEvent.TP_AST:
                    playerStatPlayer2.Assist += 1;
                    playerStatPlayer1.Point += 3;
                    playerStatPlayer2.TpTotal += 1;
                    playerStatPlayer2.TpCount += 1;
                    break;
                case FightEvent.FT_MISS:
                    playerStatPlayer1.FtTotal += 1;
                    break;
                case FightEvent.FG_MISS:
                    playerStatPlayer1.FgTotal += 1;
                    break;
                case FightEvent.TP_MISS:
                    playerStatPlayer1.TpTotal += 1;
                    break;
                case FightEvent.REB:
                    playerStatPlayer1.Rebound += 1;
                    break;
                case FightEvent.STL:
                    playerStatPlayer1.Steal += 1;
                    playerStatPlayer2.Turnover += 1;
                    break;
                case FightEvent.BLK:
                    playerStatPlayer1.Block += 1;
                    break;
                case FightEvent.TOV:
                    playerStatPlayer1.Turnover += 1;
                    break;
                case FightEvent.FOUL:
                    playerStatPlayer1.Foul += 1;
                    break;
                case FightEvent.T_FOUL:

                    break;
                case FightEvent.HURT:

                    break;
                case FightEvent.SUB:

                    break;
                default:
                    break;
            }
        }
        //public PlayerStat AddPlayerStatToNew(PlayerStat a, PlayerStat b)//a+b，返回新PlayerStat
        //{
        //    PlayerStat result = new PlayerStat();

        //    result.FtCount = a.FtCount + b.FtCount;
        //    result.FtTotal = a.FtTotal + b.FtTotal;
        //    result.FgCount = a.FgCount + b.FgCount;
        //    result.FgTotal = a.FgTotal + b.FgTotal;
        //    result.TpCount = a.TpCount + b.TpCount;
        //    result.TpTotal = a.TpTotal + b.TpTotal;
        //    result.Point = a.Point + b.Point;
        //    result.TpPoint = a.TpPoint + b.TpPoint;
        //    result.Rebound = a.Rebound + b.Rebound;
        //    result.Assist = a.Assist + b.Assist;
        //    result.Steal = a.Steal + b.Steal;
        //    result.Block = a.Block + b.Block;
        //    result.Turnover = a.Turnover + b.Turnover;
        //    result.Foul = a.Foul + b.Foul;

        //    return result;
        //}
        //public void AddPlayerStatToTarget(PlayerStat target, PlayerStat add)//把add添加到target中
        //{
        //    target.FtCount = target.FtCount + add.FtCount;
        //    target.FtTotal = target.FtTotal + add.FtTotal;
        //    target.FgCount = target.FgCount + add.FgCount;
        //    target.FgTotal = target.FgTotal + add.FgTotal;
        //    target.TpCount = target.TpCount + add.TpCount;
        //    target.TpTotal = target.TpTotal + add.TpTotal;
        //    target.Point = target.Point + add.Point;
        //    target.TpPoint = target.TpPoint + add.TpPoint;
        //    target.Rebound = target.Rebound + add.Rebound;
        //    target.Assist = target.Assist + add.Assist;
        //    target.Steal = target.Steal + add.Steal;
        //    target.Block = target.Block + add.Block;
        //    target.Turnover = target.Turnover + add.Turnover;
        //    target.Foul = target.Foul + add.Foul;
        //}

        #endregion


    }
}