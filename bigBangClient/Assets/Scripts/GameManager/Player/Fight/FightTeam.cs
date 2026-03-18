using System.Collections.Generic;
using System.Linq;
using Protocol;

namespace BigBang
{
    public class FightTeam
    {
        public FightData Fight { get; set; }
        public string TeamId { get; set; }
        public string TeamName { get; set; }
        public int TeamIcon { get; set; }
        public int TeamType { get; set; }

        // 首发阵容，只做显示用
        public FightFormation StartFormation { get; set; }
        // 战斗中的阵容，实时更新的阵容
        public FightFormation FightFormation { get; set; }
        //战斗中调整阵型，保存修改之前的阵型数据
        public FormationInfo PreFormationInfo { get; set; }

        // 所有的球员，包括替补。key : cardid
        public Dictionary<int, FightCard> AllFightCards = new Dictionary<int, FightCard>();

        // 上场 fight roles key: roleid
        public Dictionary<int, FightCard> FightRoles = new Dictionary<int, FightCard>();

        // 进球数
        public int Goal { get => AllFightCards.Sum(item => item.Value.Goal); }
        // 射门数
        public int ShootCount { get; set; }
        // ⚠射正
        public int HitCount { get; set; }
        // ⚠任意球
        public int FreeKick { get; set; }
        // ⚠角球
        public int CornerKick { get; set; }
        // 犯规
        public int FoulCount { get; set; }
        // 红牌
        public int RedCardCount { get => AllFightCards.Sum(item => item.Value.RedCard); }
        // 黄牌
        public int YellowCardCount { get => AllFightCards.Sum(item => item.Value.YellowCard); }
        // 控球时间
        public int BallControlTime { get; set; }
        public int PassCount { get; set; }
        public int PassSuccessCount { get; set; }
        // 传球成功率
        public int PassSuccessRatio
        {
            get
            {
                if (PassCount <= 0)
                {
                    return 0;
                }
                return (int)(PassSuccessCount * 1.0f / PassCount  * 100f) ;
            }
        }
        public void Init(FightData fight)
        {
            Fight = fight;
            StartFormation = new FightFormation(this);
            FightFormation = new FightFormation(this);
        }
        
        public void UnPackBeginInfo(FightTeamBeginData data)
        {

            AllFightCards.Clear();
            FightRoles.Clear();
            
            TeamId = data.BaseData.TeamId;
            TeamName = data.BaseData.TeamName;
            TeamIcon = data.BaseData.TeamIcon;
            TeamType = data.BaseData.TeamType;

            foreach (var cardData in data.AllCardList)
            {
                FightCard card = new FightCard(this);
                card.UnpackFightTeamCardData(cardData);

                AllFightCards[card.CardId] = card;
            }

            foreach (var pair in data.FightRolesCardMapping)
            {
                var card = GetCardByCardId(pair.Value);
                if (card == null) continue;
                card.RoleId = pair.Key;
                FightRoles[card.RoleId] = card;
            }

            StartFormation.UnPack(data.StartFormation);
            FightFormation.UnPack(data.StartFormation);
        }

        public FightCard GetCardByRoleID(int roleId)
        {
            if (FightRoles.TryGetValue(roleId, out var card))
            {
                return card;
            }
            return null;
        }

        public FightCard GetCardByCardId(int cardId)
        {
            if (AllFightCards.TryGetValue(cardId, out var card))
            {
                return card;
            }
            return null;
        }

        
        // 更换fightrole 上的card
        public void ReplaceFightRoleCard(int roleId, int cardId)
        {
            var card = GetCardByCardId(cardId);
            if (card == null) return;
            FightRoles[roleId] = card;
        }
        
        // 更换card的位置
        public void ReplaceCardBoard(int cardId, int boardId)
        {
            var card = GetCardByCardId(cardId);
            card?.ExchangeBoardId(boardId);
        }

        public FightCard GetPlayingGoalKeeper()
        {
            foreach (var role in FightRoles.Values)
            {
                if (role.BoardId == FormationBoardId.GKId)
                {
                    return role;
                }
            }

            return null;
        }
        
        public FightTeam GetEnemyTeam()
        {
            if (Fight.AwayTeam.TeamId == TeamId)
            {
                return Fight.HomeTeam;
            }
            else return Fight.AwayTeam;
        }

        public void DoReplacement(FightReplacementData replacementData, bool watchBefore)
        {
            // 更新他的fight role mapping
            foreach (var item in replacementData.FightRolesCardMapping)
            {
                ReplaceFightRoleCard(item.Key, item.Value);
            }
            
            if (!watchBefore)
            {
                AfterReplacementLoadFormation();
            }
        }

        //战斗中调整阵型，保存修改之前的阵型信息
        public void PreFormationInfoBackUp()
        {
            PreFormationInfo = FightFormation.Pack();
            FightFormation.ExchangeTimesBackup();
        }

        //仅客户端逻辑，重置修改前的战斗阵型
        public void ResetFormation()
        {
            FightFormation.FormationId = PreFormationInfo.FormationId;
            FightFormation.FormationName = PreFormationInfo.FormationName;
            FightFormation.BaseFormationName = PreFormationInfo.BaseFormationName;
            FightFormation.StarterBoardCardDic = PreFormationInfo.StarterBoardCardMap.ToDictionary(x => x.Key, y => y.Value);
            FightFormation.SubstituteBoardCardDic = PreFormationInfo.SubstituteBoardCardMap.ToDictionary(x => x.Key, y => y.Value);
            //FightFormation.TacticsIdList = PreFormationInfo.TacticsIdList.ToList();
            FightFormation.ResetExchangeTimesOnCancelAdjust();
        }

        //设置一下Formation的FormationId，即PVP还是PVE
        public void SetFormationType(FightType fightType)
        {
            if(fightType == FightType.PVE)
            {
                StartFormation.FormationId = FormationID.PVE;
                FightFormation.FormationId = FormationID.PVE;
            }
            else
            {
                StartFormation.FormationId = FormationID.PVP;
                FightFormation.FormationId = FormationID.PVP;
            }
        }
        
        // 换人后重载阵容 board id
        private void AfterReplacementLoadFormation()
        {
            if (FightFormation.State == FightFormation.FormationState.Normal)
            {
                return;
            }

            foreach (var item in FightFormation.StarterBoardCardDic)
            {
                var boardId = item.Key;
                var cardId = item.Value;
                ReplaceCardBoard(cardId, boardId);
            }

            foreach (var item in FightFormation.SubstituteBoardCardDic)
            {
                var boardId = item.Key;
                var cardId = item.Value;
                ReplaceCardBoard(cardId, boardId);
            }
        }
    }
}