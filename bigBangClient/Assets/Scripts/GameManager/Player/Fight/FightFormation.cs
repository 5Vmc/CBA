using System.Collections.Generic;
using System.Linq;
using Babu;
using Protocol;
using UnityEngine;

namespace BigBang
{
    // 战斗内阵容， 
    public class FightFormation : FormationBase
    {
        public class FormationState
        {
            public const int Normal = 1;
            public const int WaitExchange = 2;
        }
        private FightTeam _team;
        public int CaptainCardId { get; set; }

        public Dictionary<int, FightCard> BoardCardDic { get; set; } = new Dictionary<int, FightCard>();

        private int _exchangeTimes = 0;
        private int _preExchangeTimes = 0;
        public int State { get; set; } = FormationState.Normal;

        public FightFormation(FightTeam team) : base()
        {
            _team = team;
        }

        public void UnPack(FightFormationData data)
        {
            if (data == null)
            {
                return;
            }

            ClearFormation();
            SetChangeFlag(false);

            State = data.State;
            BaseFormationName = data.BaseFormationName;
            FormationName = data.FormationName;

            foreach (var item in data.StarterBoardCardMap)
            {
                var boardId = item.Key;
                var cardId = item.Value;
                var card = _team.GetCardByCardId(cardId);
                if (card == null) continue;
                StarterBoardCardDic.Add(boardId, item.Value);
                BoardIdList.Add(boardId);
                BoardCardDic.Add(boardId, card);
            }

            for (int i = 1; i <= FormationConst.SubstituteCount; i++)
            {
                if (data.SubstituteBoardCardMap.ContainsKey(i))
                {
                    if (SubstituteBoardCardDic.ContainsKey(i) == false) continue;
                    SubstituteBoardCardDic[i] = data.SubstituteBoardCardMap[i];
                }
            }

            TacticsIdList = data.TacticsIdList.ToList();
            CaptainCardId = data.CaptainCardId;
        }

        public override bool IsFightFormation()
        {
            return true;
        }

        public override void SaveToServer()
        {
            if (ChangeFlag == false) return;
            NetworkManager.Instance.FightExchangeFormation(_team.Fight.FightId, this, response =>
            {
                SetChangeFlag(false);
                State = FormationState.WaitExchange;
            });
        }

        //若在战中更换阵型，系统依然会被动使用一次自动上阵，但范围是场上的球员。
        private List<PlayerCard> GetFightStarterCards()
        {
            List<PlayerCard> list = new List<PlayerCard>();
            foreach (var item in StarterBoardCardDic)
            {
                list.Add(Player.CardManager.GetCard(item.Value));
            }

            return list;
        }
        // 使用模板自动构建阵容
        public override void UseFormationTemp(FormationTemp temp)
        {
            SetChangeFlag(true);
            SetBaseFormationName(temp.Name);

            //List<PlayerCard> poolList = GetFightStarterCards();
            //AutoMakeFormation_FromPool(temp.BoardIdList, poolList);
            AutoRemakeStarter(temp.BoardIdList);

            EventManager.Instance.Dispatch(EventID.OnUseFormationTemplate);
        }

        protected override void AddExchangeTimes()
        {
            _exchangeTimes++;
        }

        // 是否可以换人
        public override bool CheckCanExchangeCard()
        {
            return _exchangeTimes < 5;
        }

        public void ExchangeTimesBackup()
        {
            _preExchangeTimes = _exchangeTimes;
        }

        public void ResetExchangeTimesOnCancelAdjust()
        {
            _exchangeTimes = _preExchangeTimes;
        }

        //设置PlayerCard的FightFormationData，区别于Formation.cs中的版本
        public override void UpdateCardFormationInfo()
        {
            foreach (var card in Player.CardManager.CardList)
            {
                card.FightFormationData.Clear();
            }

            foreach (var pair in StarterBoardCardDic)
            {
                int boardId = pair.Key;
                int cardId = pair.Value;
                var card = Player.CardManager.GetCard(cardId);
                if (card == null) continue;
                card.FightFormationData.SetData(FormationCardState.Starter, boardId, 0);
            }

            for (int i = 1; i <= FormationConst.SubstituteCount; i++)
            {
                if (SubstituteBoardCardDic.ContainsKey(i) == false) continue;
                var card = Player.CardManager.GetCard(SubstituteBoardCardDic[i]);
                if (card == null) continue;
                card.FightFormationData.SetData(FormationCardState.Substitute, 0, i);
            }
        }
    }
}
