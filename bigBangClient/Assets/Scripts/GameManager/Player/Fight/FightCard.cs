using GameConfig;
using GameConfig.Config;
using Protocol;
using UnityEngine;

namespace BigBang
{
    public class FightCard
    {
        public FightCard(FightTeam team)
        {
            Team = team;
        }
        
        public int RoleId { get; set; }
        public int CardId { get; set; }
        public int BoardId { get; set; }
        public int Number { get; set; }
        public FightTeam Team { get; set; }

        public bool IsCaptain {
            get
            {
                return Team.StartFormation.CaptainCardId == CardId;
            }
        }
        // 战斗力
        public int CombatEffectiveness { get; set; }
        // 射门能力
        public int ShootAbility { get; set; }
        // 意志力能力
        public int WillAbility { get; set; }
        public int ShootCount { get; set; }
        // 进球数
        public int Goal { get; set; }
        // 助攻数
        public int Assist { get; set; }
        // 过人数
        public int BreachSuccess { get; set; }
        // 抢断数
        public int StealStls { get; set; }
        // 拦截
        public int Intercept { get; set; }
        // 封堵
        public int Plugging { get; set; }
        // 扑救
        public int ShootSave { get; set; }
        // ⚠传球偏离数
        public int PassCount { get; set; }
        public int PassSuccess { get; set; }
        // 被过人次数
        public int BeBreach { get; set; }
        // 被踢进球次数(仅门将,非门将该值为0)
        public int BeShootIn { get; set; }
        // 黄牌
        public int YellowCard { get; set; }
        // 红牌
        public int RedCard { get; set; }
        // MVP
        public bool IsMvp { get; set; }
        // ⚠是否是首发，true 首发，false 替补
        public bool IsStarter { get; set; }
        // ⚠换人次数
        public int ReplacementCount { get; set; }
        // ⚠比赛结束后场上的状态， 1 场上， 2 替补席
        public int EndFightState { get; set; }
        // 比赛时的场上位置 -1 表示替补
        public int FightSeparatedPosition { get; set; }

        public int GetFightSeparatedPosition()
        {
            var config = Configs.FormationBoard.GetConfig(BoardId);
            if (config == null)
            {
                return -1;
            }
            return config.SeparatedPosition;
        }

        // 获取比赛中的得分
        public double GetPlayingScore()
        {
            float addition = 0;
            // 进球加成
            addition += 0.5f * Mathf.Pow(Goal, 2);
            // 助攻加成
            addition += 0.5f * Mathf.Pow(Assist, 2);
            // 过人加成
            addition += 0.5f * Mathf.Pow(BreachSuccess / 3f, 2);
            // 抢断加成
            addition += 0.5f * Mathf.Pow(StealStls / 5f, 2);
            // 拦截加成
            addition += 0.5f * Mathf.Pow(Intercept / 5f, 2);
            // 封堵加成
            addition += 0.5f * Mathf.Pow(Plugging / 5f, 2);
            // 扑救加成
            addition += 0.5f * Mathf.Pow(ShootSave / 4f, 2);
            // 红黄牌加成
            addition -= 0.5f * Mathf.Pow(YellowCard / 2f + RedCard, 2);
            // 传球偏离加成
            addition -= (PassCount - PassSuccess) / 5f;
            // 射门踢飞加成
            addition -= (ShootCount - Goal) / 3f;
            // 被过人加成
            addition -= BeBreach / 5f;
            // 被踢进球加成
            addition -= BeShootIn / 3f;
            var score = 6 + addition;
            if (score < 0) score = 0;
            if (score > 10) score = 10;
            return score;
        }
        // 得分
        public double Score { get; set; }

        public CardModelConfig GetConfig()
        {
            return Configs.CardModel.GetConfig(CardId);
        }

        public void AddYellowCard()
        {
            YellowCard++;
        }

        public void AddRedCard()
        {
            RedCard++;
            YellowCard = 0;
        }
        public void UnpackFightTeamCardData(FightTeamCardData data)
        {
            CardId = data.CardId;
            BoardId = data.BoardId;
            Number = data.Number;
            CombatEffectiveness = data.CombatEffectiveness;
            ShootAbility = data.ShootAbility;
            WillAbility = data.WillAbility;
        }

        public void UnpackFightPerformance(FightCardPerformanceData data)
        {
            CardId = data.CardId;
            // BoardId = data.BoardId;
            Goal = data.Goal;
            Score = data.Score;
            // Assist = data.
            // StealStls = data.se
            YellowCard = data.YellowCard;
            RedCard = data.RedCard;
            IsMvp = data.IsMvp;
            IsStarter = data.IsStarter;
            ReplacementCount = data.ReplacementCount;
            EndFightState = data.EndFightState;
            FightSeparatedPosition = data.FightSeparatedPosition;
        }
        
        public void ExchangeBoardId(int boardId)
        {
            BoardId = boardId;
        }
    }
}