using System;
using GameConfig;
using GameConfig.Config;
using Protocol;

namespace BigBang
{


    //球队数据计算
    public class BattleTeamData
    {
        #region 外部使用接口
        public Action onDataChange = null;
        #endregion

        #region 初始化

        public TeamStat teamStatBlue = new();
        public TeamStat teamStatRed = new();

        public BattleTeamData()
        {
            teamStatBlue = new();
            teamStatRed = new();
            for (int i = 0; i < 5; i++)
            {
                teamStatBlue.PtsQtrs.Add(0);
                teamStatRed.PtsQtrs.Add(0);
            }
            onDataChange?.Invoke();
        }

        #endregion

        #region 数据累加

        public void AddDrop(int stage, float aniTime)
        {
            FightQuaterInfo fightQuaterInfo = Player.BattleManager.fightInfoData.fightInfo.Result.Quarters[stage];
            var teamAllStat = GetInfoDataStage(fightQuaterInfo, stage);
            AddTeamStatToTarget(teamStatBlue, teamAllStat.teamStatAddBlue);
            AddTeamStatToTarget(teamStatRed, teamAllStat.teamStatAddRed);
            onDataChange?.Invoke();
        }
        public void AddRound(FightPossessionInfo fightPossessionInfo, int stage)
        {
            AddInfoToTeamStat(fightPossessionInfo, teamStatBlue, teamStatRed, stage);
            onDataChange?.Invoke();
        }

        #endregion

        #region 数据计算
        public (TeamStat teamStatAddBlue, TeamStat teamStatAddRed) GetInfoDataStage(FightQuaterInfo fightQuaterInfo, int stage)//获取某一节的服务器回合统计
        {
            TeamStat teamStatAddBlue = new TeamStat();
            TeamStat teamStatAddRed = new TeamStat();
            for (int i = 0; i < 5; i++)
            {
                teamStatAddBlue.PtsQtrs.Add(0);
                teamStatAddRed.PtsQtrs.Add(0);
            }
            foreach (var possession in fightQuaterInfo.Possessions)
            {
                AddInfoToTeamStat(possession, teamStatAddBlue, teamStatAddRed, stage);
            }
            return (teamStatAddBlue, teamStatAddRed);
        }
        public void AddInfoToTeamStat(FightPossessionInfo fightPossessionInfo, TeamStat teamStatAddBlue, TeamStat teamStatAddRed, int stage)
        {
            AddActionToTeamStat(fightPossessionInfo.EventId, fightPossessionInfo.PlayerCardId, fightPossessionInfo.Player2CardId, teamStatAddBlue, teamStatAddRed, stage);
        }
        public void AddActionToTeamStat(int actionId, string PlayerCardId, string Player2CardId, TeamStat teamStatAddBlue, TeamStat teamStatAddRed, int stage)//将Action添加到统计里
        {
            TeamStat teamStatPlayer1 = null;//玩家1所在的阵营
            TeamStat teamStatPlayer2 = null;//玩家2所在的阵营
            if (string.IsNullOrWhiteSpace(PlayerCardId) == false)
            {
                if (Player.BattleManager.fightInfoData.fightCardDicAway.ContainsKey(PlayerCardId))
                {
                    teamStatPlayer1 = teamStatAddBlue;
                }
                else
                {
                    teamStatPlayer1 = teamStatAddRed;
                }
            }
            if (string.IsNullOrWhiteSpace(Player2CardId) == false)
            {
                if (Player.BattleManager.fightInfoData.fightCardDicAway.ContainsKey(Player2CardId))
                {
                    teamStatPlayer2 = teamStatAddBlue;
                }
                else
                {
                    teamStatPlayer2 = teamStatAddRed;
                }
            }
            switch ((FightEvent)actionId)
            {
                case FightEvent.FT:
                    teamStatPlayer1.PtsQtrs[stage] += 1;
                    teamStatPlayer1.Point += 1;
                    teamStatPlayer1.FtTotal += 1;
                    teamStatPlayer1.FtCount += 1;
                    break;
                case FightEvent.FG:
                    teamStatPlayer1.PtsQtrs[stage] += 2;
                    teamStatPlayer1.Point += 2;
                    teamStatPlayer1.FgTotal += 1;
                    teamStatPlayer1.FgCount += 1;
                    break;
                case FightEvent.TP:
                    teamStatPlayer1.PtsQtrs[stage] += 3;
                    teamStatPlayer1.Point += 3;
                    teamStatPlayer1.TpTotal += 1;
                    teamStatPlayer1.TpCount += 1;
                    break;
                case FightEvent.FG_AST:
                    teamStatPlayer1.PtsQtrs[stage] += 2;
                    teamStatPlayer2.Assist += 1;
                    teamStatPlayer1.Point += 2;
                    teamStatPlayer2.FgTotal += 1;
                    teamStatPlayer2.FgCount += 1;
                    break;
                case FightEvent.TP_AST:
                    teamStatPlayer1.PtsQtrs[stage] += 3;
                    teamStatPlayer2.Assist += 1;
                    teamStatPlayer1.Point += 3;
                    teamStatPlayer2.TpTotal += 1;
                    teamStatPlayer2.TpCount += 1;
                    break;
                case FightEvent.FT_MISS:
                    teamStatPlayer1.FtTotal += 1;
                    break;
                case FightEvent.FG_MISS:
                    teamStatPlayer1.FgTotal += 1;
                    break;
                case FightEvent.TP_MISS:
                    teamStatPlayer1.TpTotal += 1;
                    break;
                case FightEvent.REB:
                    teamStatPlayer1.Rebound += 1;
                    break;
                case FightEvent.STL:
                    teamStatPlayer1.Steal += 1;
                    teamStatPlayer2.Turnover += 1;
                    break;
                case FightEvent.BLK:
                    teamStatPlayer1.Block += 1;
                    break;
                case FightEvent.TOV:
                    teamStatPlayer1.Turnover += 1;
                    break;
                case FightEvent.FOUL:
                    teamStatPlayer1.Foul += 1;
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
        public TeamStat AddTeamStatToNew(TeamStat a, TeamStat b)//a+b，返回新TeamStat
        {
            TeamStat result = new TeamStat();

            result.FtCount = a.FtCount + b.FtCount;
            result.FtTotal = a.FtTotal + b.FtTotal;
            result.FgCount = a.FgCount + b.FgCount;
            result.FgTotal = a.FgTotal + b.FgTotal;
            result.TpCount = a.TpCount + b.TpCount;
            result.TpTotal = a.TpTotal + b.TpTotal;
            result.Point = a.Point + b.Point;
            result.TpPoint = a.TpPoint + b.TpPoint;
            result.Rebound = a.Rebound + b.Rebound;
            result.Assist = a.Assist + b.Assist;
            result.Steal = a.Steal + b.Steal;
            result.Block = a.Block + b.Block;
            result.Turnover = a.Turnover + b.Turnover;
            result.Foul = a.Foul + b.Foul;

            for (int i = 0; i < 5; i++)
            {
                result.PtsQtrs.Add(0);
                result.PtsQtrs[i] = a.PtsQtrs[i] + b.PtsQtrs[i];
            }

            return result;
        }
        public void AddTeamStatToTarget(TeamStat target, TeamStat add)//把add添加到target中
        {
            target.FtCount = target.FtCount + add.FtCount;
            target.FtTotal = target.FtTotal + add.FtTotal;
            target.FgCount = target.FgCount + add.FgCount;
            target.FgTotal = target.FgTotal + add.FgTotal;
            target.TpCount = target.TpCount + add.TpCount;
            target.TpTotal = target.TpTotal + add.TpTotal;
            target.Point = target.Point + add.Point;
            target.TpPoint = target.TpPoint + add.TpPoint;
            target.Rebound = target.Rebound + add.Rebound;
            target.Assist = target.Assist + add.Assist;
            target.Steal = target.Steal + add.Steal;
            target.Block = target.Block + add.Block;
            target.Turnover = target.Turnover + add.Turnover;
            target.Foul = target.Foul + add.Foul;

            for (int i = 0; i < 5; i++)
            {
                target.PtsQtrs[i] = target.PtsQtrs[i] + add.PtsQtrs[i];
            }
        }

        #endregion


    }
}