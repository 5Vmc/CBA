using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Babu;
using BigBang.Battle;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using Vector2 = UnityEngine.Vector2;
using static BigBang.ClassicManager;
using Newtonsoft.Json;
using static BigBang.HeroManager;
using static BigBang.FightInfoData2.StageBallInfo;
using Babu.Client.Fsm;

namespace BigBang
{

    public class FightInfoData
    {
        public Protocol.FightInfo fightInfo;//服务器发来的战斗信息

        public Dictionary<string, Protocol.FightCard> fightCardDicAway = new();//客场队的所有球员
        public Dictionary<string, Protocol.FightCard> fightCardDicHome = new();//主场队的所有球员
        public Dictionary<string, Protocol.FightCard> fightCardDicAll = new();//所有球员
        public Dictionary<string, Protocol.TeamStat> teamStatDicAll = new();//所有球队的统计信息
        public Dictionary<string, Protocol.PlayerStat> playerStatDicAll = new();//所有球员的统计信息
        public HashSet<string> CourtCardSetAll = new();//所有首发球员的ID
        public Protocol.TeamStat awayTeamStat = null;
        public Protocol.TeamStat homeTeamStat = null;
        public int maxStage = 0;
        public Protocol.PlayerStat mvpPlayerStat = null;
        public Protocol.FightCard mvpFightCard = null;

        public FightInfoData(Protocol.FightInfo fightInfo)
        {
            this.fightInfo = fightInfo;

            ProcessFightInfo();
        }

        private void ProcessFightInfo()
        {
            bool isHundred = Player.BattleManager.fightType == FightType.Hundred;

            CourtCardSetAll.Clear();
            fightCardDicAll.Clear();
            fightCardDicAway.Clear();
            mvpPlayerStat = null;
            mvpFightCard = null;
            foreach (Protocol.FightCard courtCardItem in fightInfo.Teams.Away.CourtCard)
            {
                if (fightCardDicAll.ContainsKey(courtCardItem.PlayerCardId))
                {
                    Debug.LogError("ProcessFightInfo , same PlayerCardId in fightInfo.Teams , PlayerCardId = " + courtCardItem.PlayerCardId);
                    continue;
                }
                fightCardDicAway.Add(courtCardItem.PlayerCardId, courtCardItem);
                fightCardDicAll.Add(courtCardItem.PlayerCardId, courtCardItem);
                CourtCardSetAll.Add(courtCardItem.PlayerCardId);
            }
            foreach (Protocol.FightCard courtCardItem in fightInfo.Teams.Away.BenchCard)
            {
                if (fightCardDicAll.ContainsKey(courtCardItem.PlayerCardId))
                {
                    Debug.LogError("ProcessFightInfo , same PlayerCardId in fightInfo.Teams , PlayerCardId = " + courtCardItem.PlayerCardId);
                    continue;
                }
                fightCardDicAway.Add(courtCardItem.PlayerCardId, courtCardItem);
                fightCardDicAll.Add(courtCardItem.PlayerCardId, courtCardItem);
            }
            fightCardDicHome.Clear();
            foreach (Protocol.FightCard courtCardItem in fightInfo.Teams.Home.CourtCard)
            {
                if (fightCardDicAll.ContainsKey(courtCardItem.PlayerCardId))
                {
                    Debug.LogError("ProcessFightInfo , same PlayerCardId in fightInfo.Teams , PlayerCardId = " + courtCardItem.PlayerCardId);
                    continue;
                }
                fightCardDicHome.Add(courtCardItem.PlayerCardId, courtCardItem);
                fightCardDicAll.Add(courtCardItem.PlayerCardId, courtCardItem);
                CourtCardSetAll.Add(courtCardItem.PlayerCardId);
            }
            foreach (Protocol.FightCard courtCardItem in fightInfo.Teams.Home.BenchCard)
            {
                if (fightCardDicAll.ContainsKey(courtCardItem.PlayerCardId))
                {
                    Debug.LogError("ProcessFightInfo , same PlayerCardId in fightInfo.Teams , PlayerCardId = " + courtCardItem.PlayerCardId);
                    continue;
                }
                fightCardDicHome.Add(courtCardItem.PlayerCardId, courtCardItem);
                fightCardDicAll.Add(courtCardItem.PlayerCardId, courtCardItem);
            }
            teamStatDicAll.Clear();
            foreach (Protocol.TeamStat teamStatItem in fightInfo.Result.TeamStat)
            {
                if (teamStatDicAll.ContainsKey(teamStatItem.TeamId))
                {
                    Debug.LogError("teamStatDicAll , same team id : " + teamStatItem.TeamId);
                }
                else
                {
                    teamStatDicAll.Add(teamStatItem.TeamId, teamStatItem);
                }
            }
            awayTeamStat = teamStatDicAll[fightInfo.Teams.Away.TeamId];
            homeTeamStat = teamStatDicAll[fightInfo.Teams.Home.TeamId];
            playerStatDicAll.Clear();
            foreach (Protocol.PlayerStat playerStatItem in fightInfo.Result.PlayerStat)
            {
                playerStatDicAll.Add(playerStatItem.PlayerCardId, playerStatItem);
                if (playerStatItem.IsMvp == true)
                {
                    mvpPlayerStat = playerStatItem;
                    mvpFightCard = fightCardDicAll[mvpPlayerStat.PlayerCardId];
                }
            }
            if (mvpPlayerStat == null)
            {
                Debug.LogError("服务器数据中没有MVP！");
            }
            if (isHundred)
            {
                maxStage = 1;
            }
            else
            {
                maxStage = fightInfo.Result.Quarters.Count;
            }
        }

        public bool IsPlayerRedByPlayerCardId(string playerCardId)
        {
            return fightCardDicAway.ContainsKey(playerCardId);
        }
    }
    public class FightInfoData2
    {
        public FightInfo fightInfo;//服务器发来的战斗信息

        public class StageBallInfo//一小节内的信息
        {
            public List<int> ballRoundIdList = new();//回合id列表(从1开始
            public Dictionary<int, List<FightPossessionInfo>> ballRoundDic = new();//回合
            public bool hasTeamFire = false;
            public Protocol.FightQuaterInfo fightQuaterInfo = null;
            public StageBallInfo(Protocol.FightQuaterInfo fightQuaterInfo)
            {
                this.fightQuaterInfo = fightQuaterInfo;
                hasTeamFire = fightQuaterInfo.AwayFireBuff > 0 || fightQuaterInfo.HomeFireBuff > 0;
                foreach (FightPossessionInfo fightPossessionInfo in fightQuaterInfo.Possessions)
                {
                    if (ballRoundDic.ContainsKey(fightPossessionInfo.Possession))
                    {
                        ballRoundDic[fightPossessionInfo.Possession].Add(fightPossessionInfo);
                    }
                    else
                    {
                        ballRoundIdList.Add(fightPossessionInfo.Possession);
                        List<FightPossessionInfo> fightPossessionInfoList = new();
                        fightPossessionInfoList.Add(fightPossessionInfo);
                        ballRoundDic.Add(fightPossessionInfo.Possession, fightPossessionInfoList);
                    }
                }
            }

            public class GiftBuffData
            {
                public GiftBuffInfo giftBuffInfo;
                public GiftSkillConfig giftSkillConfig;
            }

        }
        public List<StageBallInfo> stageBallInfoList = new();//每小节的回合进球信息

        public FightInfoData2(FightInfo fightInfo)
        {
            this.fightInfo = fightInfo;

            ProcessFightInfo();
            ProcessGiftSkillInfo();
        }
        private void ProcessFightInfo()
        {
            stageBallInfoList.Clear();
            //if (Player.BattleManager.fightType == FightType.Hundred)
            //{
            //    if (fightInfo.Result.Quarters.Count <= Player.BattleManager.hundredStageIndex)
            //    {
            //        Debug.LogError("ProcessFightInfo , Hundred , Quarters.Count = {0} , hundredStageIndex = {1}".SafeFormat(fightInfo.Result.Quarters.Count, Player.BattleManager.hundredStageIndex));
            //        return;
            //    }
            //    stageBallInfoList.Add(new StageBallInfo(fightInfo.Result.Quarters[Player.BattleManager.hundredStageIndex]));
            //}
            //else
            //{
            foreach (Protocol.FightQuaterInfo fightQuaterInfo in fightInfo.Result.Quarters)
            {
                stageBallInfoList.Add(new StageBallInfo(fightQuaterInfo));
            }
            //}
        }
        private Dictionary<int, List<GiftBuffData>> giftBuffDataDicStart = new();//gift开始回合
        private Dictionary<int, List<GiftBuffData>> giftBuffDataDicEnd = new();//gift结束回合
        private void ProcessGiftSkillInfo()
        {
            giftBuffDataDicStart.Clear();
            giftBuffDataDicEnd.Clear();
            float rountTime = (48f + (fightInfo.Result.Quarters.Count > 4 ? 5f : 0f)) / stageBallInfoList[^1].ballRoundIdList[^1];//每小节时间 = (48分钟+有加时的话5分钟)/所有回合数
            for (int i = 0; i < stageBallInfoList.Count; i++)
            {
                StageBallInfo stageBallInfo = stageBallInfoList[i];
                Protocol.FightQuaterInfo fightQuaterInfo = fightInfo.Result.Quarters[i];
                foreach (GiftBuffInfo giftBuffInfo in fightQuaterInfo.GiftBuffs)
                {
                    GiftBuffData giftBuffData = new();
                    giftBuffData.giftBuffInfo = giftBuffInfo;
                    giftBuffData.giftSkillConfig = Configs.GiftSkill.GetConfig(giftBuffInfo.GiftId);

                    int startKey = giftBuffInfo.Possession;
                    if (giftBuffDataDicStart.ContainsKey(startKey) == false)
                    {
                        giftBuffDataDicStart.Add(startKey, new());
                    }
                    giftBuffDataDicStart[startKey].Add(giftBuffData);

                    int effectRoundCount = Mathf.FloorToInt((float)giftBuffData.giftSkillConfig.Bufftime / rountTime);//持续或回合数 = 向下取整( 天赋时间/每小节时间)
                    int endRoundIndex = giftBuffInfo.Possession + effectRoundCount;
                    int endKey = endRoundIndex;
                    if (giftBuffDataDicEnd.ContainsKey(endKey) == false)
                    {
                        giftBuffDataDicEnd.Add(endKey, new());
                    }
                    giftBuffDataDicEnd[endKey].Add(giftBuffData);
                }
            }
        }
        public List<GiftBuffData> GetGiftBuffDataStartList(int roundIndex)
        {
            if (giftBuffDataDicStart.ContainsKey(roundIndex) == false) return new();
            List<GiftBuffData> giftBuffDataList = giftBuffDataDicStart[roundIndex];
            giftBuffDataDicStart.Remove(roundIndex);
            return giftBuffDataList;
        }
        public List<GiftBuffData> GetGiftBuffDataEndList(int roundIndex)
        {
            List<int> needDelRoundIndexList = new();
            List<GiftBuffData> giftBuffDataList = new();
            foreach (var item in giftBuffDataDicEnd)
            {
                if (item.Key <= roundIndex)
                {
                    needDelRoundIndexList.Add(item.Key);
                    foreach (var giftBuffData in item.Value)
                    {
                        giftBuffDataList.Add(giftBuffData);
                    }
                }
            }
            if (needDelRoundIndexList.Count <= 0) return new();
            foreach (int needDelRoundIndex in needDelRoundIndexList)
            {
                giftBuffDataDicEnd.Remove(needDelRoundIndex);
            }
            return giftBuffDataList;
        }

        public static int GetAddScoreNum(int EventId)
        {
            int addScore = 0;
            switch ((FightEvent)EventId)
            {
                case FightEvent.FT: addScore = 1; break;
                case FightEvent.FG: addScore = 2; break;
                case FightEvent.TP: addScore = 3; break;
                case FightEvent.FG_AST: addScore = 2; break;
                case FightEvent.TP_AST: addScore = 3; break;
                default: addScore = 0; break;
            }
            return addScore;
        }
        public static bool IsAssist(int EventId)
        {
            switch ((FightEvent)EventId)
            {
                case FightEvent.FG_AST: return true;
                case FightEvent.TP_AST: return true;
            }
            return false;
        }
        public static bool IsSub(int EventId)
        {
            switch ((FightEvent)EventId)
            {
                case FightEvent.SUB: return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 战斗的核心逻辑和3D部分在此类中
    /// </summary>
    public class BattleManager : BaseManager
    {
        public int hundredStageIndex = 0;
        public HundredProgress hundredProgress = HundredProgress.Fight1;
        public void SetHundredFightInfo(FightType fightType, FightInfo fightInfo, int hundredStageIndex, HundredProgress hundredProgress)
        {
            this.fightType = fightType;
            this.hundredStageIndex = hundredStageIndex;
            this.hundredProgress = hundredProgress;
            this.fightInfo = fightInfo;
            this.fightInfoData = new(fightInfo);
            this.fightInfoData2 = new(fightInfo);
        }

        public void SetFightInfo(FightType fightType, FightInfo fightInfo)
        {
            this.fightType = fightType;
            this.fightInfo = fightInfo;
            this.fightInfoData = new(fightInfo);
            this.fightInfoData2 = new(fightInfo);
        }
        /// <summary>
        /// 开始播放战斗过程
        /// 根据类型不同，需要提前设置各个配置
        /// Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.Guide;
        /// Player.BattleManager.SetFightInfo(FightType.PVE, response.Fight);
        /// Player.BattleManager.StartPlayFight();
        /// </summary>
        public void StartPlayFight()
        {
            FsmManager.Instance.ChangeToState<StateBattle>(new StateCommonUserData()
            {
                OpenUIAction = async () =>
                {
                    await UIController.Instance.ShowPanel<BattleUI2>();
                }
            });
        }

        public BattleTeamData battleTeamData = null;//队伍数据
        public BattlePlayerData battlePlayerData = null;//球员数据
        public void ClearRunningData()
        {
            battleTeamData = new();
            battlePlayerData = new();
        }


        #region 为了战斗各个流程中的界面缓存数据
        public FightType fightType;//战斗类型
        public FightInfo fightInfo;//战斗服务器信息
        public FightInfoData fightInfoData;
        public FightInfoData2 fightInfoData2;

        //从哪里进的战斗，影响结算面板显示的内容等
        public enum BattleEnterType
        {
            Unknown,
            Debug,
            Guide,
            MyGameUI,
            ChallengeUI,
            LeagueUI,
            ArenaUI,
            CupUI_Course,//赛程
            CupUI_Integral,//积分
            LeagueUI_LeagueCoursePad,
            MyGameUI_MyCoursePad,
            MyGameUI_MyLastGamePad,
            ArenaUI_ArenaPad,
            ChallengeUI_MatchPreviewUI,
            ClassicUI,
            HeroUI,
            FBTowerHomeUI,
            HundredTeamDetailUI,
        }
        public BattleEnterType battleEnterType = BattleEnterType.Unknown;
        /// <summary>
        /// 当前挑战关卡是否首次通关
        /// </summary>
        public bool isFirstPass = false;

        //经典赛配置文件
        public ClassicCountryLevelData classicCountryLevelData;
        public ChallengeStartResponse challengeStartResponse;
        public ClassicTeamData classicTeamData;
        public double classicTeamItemAdapterNormalizedPosition = 0f;
        public int classicPlayerLevel;
        public int classicPlayerExp;
        public Dictionary<int, int> classicTeamPlayerLevelDic = new();
        public Dictionary<int, int> classicTeamPlayerExpDic = new();
        public void SaveLevelAndExp()
        {
            Player.BattleManager.classicPlayerLevel = Player.Level;
            Player.BattleManager.classicPlayerExp = Player.Exp;
            Player.BattleManager.classicTeamPlayerLevelDic.Clear();
            Player.BattleManager.classicTeamPlayerExpDic.Clear();
            foreach (PlayerCard playerCard in Player.CardManager.CardList)
            {
                Player.BattleManager.classicTeamPlayerLevelDic.Add(playerCard.CardId, playerCard.Level);
                Player.BattleManager.classicTeamPlayerExpDic.Add(playerCard.CardId, playerCard.Exp);
            }
        }

        //剧情战斗配置文件
        public HeroClubData heroClubData;
        public ChallengeHeroConfig lastChallengeHeroConfig;
        public ChallengeStartHeroResponse challengeStartHeroResponse;

        //爬塔战斗(篮球殿堂)配置文件
        public TowerLevelData towerLevelData;
        public StartTowerChallengeResponse startTowerChallengeResponse;

        //是否显示国家节点
        public bool showCounties = true;

        //PVE配置文件
        public ChallengeClubConfig challengeClubConfig;

        //为联赛战斗结束界面缓存的数据
        public GetLeagueCourseResponse getLeagueCourseResponse;

        //为竞技场战斗结束界面缓存的数据
        public void AddArenaInfo(ArenaInfo newArenaInfo)
        {
            this.oldArenaInfo = this.newArenaInfo;
            this.newArenaInfo = newArenaInfo;
            EventManager.Instance.Dispatch(EventID.OnArenaGetNewInfo);
        }
        public BattleResponse battleResponse;//竞技场战斗结果(输赢)
        public ArenaInfo oldArenaInfo;//上一次的竞技场信息(旧排名)
        public ArenaInfo newArenaInfo;//当前的竞技场信息(新排名)
        public ArenaTeamData arenaTeamData;//对手信息（对手名字）

        public void CheckArenaRedDot(Action callback)
        {
            if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicArena, false)) return;
            if (newArenaInfo == null)
            {
                NetworkManager.Instance.GetArenaInfo(resp =>
                {
                    if (resp.Succeed)
                    {
                        Player.BattleManager.AddArenaInfo(resp.Info);
                        CheckArenaRedDot(callback);
                    }
                    else
                    {
                        Tips.PopTips("竞技场数据返回错误");
                    }
                });
            }
            else
            {
                //竞技场次数小红点
                RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicArena, "/lefttime");
                node.AddValue(newArenaInfo.BattleTimesLeft > 0 ? 1 : -1);

                //每日奖励小红点
                node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicArena, "/DailyRewards");
                if (newArenaInfo.DailyClaim == true)
                {
                    node.AddValue(-1);
                }
                else
                {
                    node.AddValue(1);
                }

                callback?.Invoke();
                //可购买状态没做小红点，是因为玩家可能攒货币买他想买的东西。
            }
        }

        #endregion


    }
}