
using System.Linq;
using Babu;
using Protocol;
using UnityEngine;

namespace BigBang
{
    public class Formation : FormationBase
    {
        public Formation(int formationId) : base()
        {
            FormationId = formationId;
        }

        public void UnPack(FormationInfo data)
        {
            if (data == null)
            {
                return;
            }
            ClearFormation();
            SetChangeFlag(false);

            FormationId = data.FormationId;
            BaseFormationName = data.BaseFormationName;
            FormationName = data.FormationName;
            foreach (var item in data.StarterBoardCardMap)
            {
                StarterBoardCardDic.Add(item.Key, item.Value);
                BoardIdList.Add(item.Key);
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
            if (FormationId != FormationID.Hundred && FormationId != FormationID.Bounty)
            {
                if (TacticsIdList.Count != 2 && FormationId != FormationID.Hundred)
                {
                    //Debug.LogWarningFormat("Formation UnPack TacticsIdList.Count != 2 , idList.Count = {0}", TacticsIdList.Count);
                    TacticsIdList = new()
                    {
                        101,
                        201
                    };
                }
            }

            LineupShowTime = data.LineupShowTime;
        }
        public override bool IsFightFormation()
        {
            return false;
        }

        public override void UseFormationTemp(FormationTemp temp)
        {
            SetChangeFlag(true);
            SetBaseFormationName(temp.Name);

            AutoMakeFormation_FromPool(temp.BoardIdList, Player.CardManager.CardList);

            UpdateCardFormationInfo();

            EventManager.Instance.Dispatch(EventID.OnUseFormationTemplate);
        }

        public override void SaveToServer()
        {
            UpdateCardFormationInfo();
            if (ChangeFlag == false) return;
            NetworkManager.Instance.SaveFormation(FormationId, this, response =>
            {
                if (response.Success)
                {
                    if (FormationId == FormationID.PVE) Player.CardManager.CheckRedDot(0, true);
                    SetChangeFlag(false);
                }
            });
        }

        // 更新卡牌信息
        public override void UpdateCardFormationInfo()
        {
            foreach (var card in Player.CardManager.CardList)
            {
                card.FormationDataDic[FormationId].Clear();
            }

            foreach (var pair in StarterBoardCardDic)
            {
                int boardId = pair.Key;
                int cardId = pair.Value;
                var card = Player.CardManager.GetCard(cardId);
                if (card == null) continue;
                card.FormationDataDic[FormationId].SetData(FormationCardState.Starter, boardId, 0);
            }

            for (int i = 1; i <= FormationConst.SubstituteCount; i++)
            {
                if (SubstituteBoardCardDic.ContainsKey(i) == false) return;
                var card = Player.CardManager.GetCard(SubstituteBoardCardDic[i]);
                if (card == null) continue;
                card.FormationDataDic[FormationId].SetData(FormationCardState.Substitute, 0, i);
            }
        }
    }
}
