using System.Collections.Generic;
using System.Linq;
using Babu;
using CBA;
using GameConfig;
using Protocol;
using UnityEngine;
using Utils;
using Debug = UnityEngine.Debug;

namespace BigBang
{
    public abstract class FormationBase
    {
        public int FormationId { get; set; } = FormationID.PVP;

        //主力阵容
        public Dictionary<int, int> StarterBoardCardDic { get; set; } = new Dictionary<int, int>();

        // 替补球员
        public Dictionary<int, int> SubstituteBoardCardDic { get; set; } = new Dictionary<int, int>();

        //
        public string BaseFormationName { get; set; }

        public string FormationName { get; set; }

        //使用的阵容棋盘id
        public List<int> BoardIdList { get; set; } = new List<int>();

        //使用的战术
        public List<int> TacticsIdList { get; set; } = new List<int>();

        private bool _changeFlag = false;
        public bool ChangeFlag => _changeFlag;

        // private bool _isInitialized = false;

        //阵容的展示时间(todo 做到客户端存储)
        public long LineupShowTime { get; set; } = 0;

        public FormationBase()
        {
            // _isInitialized = false;
            for (int i = 1; i <= FormationConst.SubstituteCount; i++)
            {
                SubstituteBoardCardDic.Add(i, 0);
            }
        }

        public FormationInfo Pack()
        {
            FormationInfo data = new FormationInfo();
            data.FormationName = FormationName;
            data.BaseFormationName = BaseFormationName;

            Debug.Log("FormationInfo Pack:" + FormationName);

            foreach (var id in TacticsIdList)
            {
                data.TacticsIdList.Add(id);
            }

            foreach (var pair in StarterBoardCardDic)
            {
                data.StarterBoardCardMap.Add(pair.Key, pair.Value);
            }

            foreach (var pair in SubstituteBoardCardDic)
            {
                data.SubstituteBoardCardMap.Add(pair.Key, pair.Value);
            }

            data.LineupShowTime = LineupShowTime;
            return data;
        }

        #region formation

        public abstract bool IsFightFormation();
        // 使用模板更新
        public abstract void UseFormationTemp(FormationTemp temp);

        // 保存到服务器
        public abstract void SaveToServer();

        public void MoveStarterBoard(int fromBoardId, int toBoardId)
        {
            SetChangeFlag(true);
            if (StarterBoardCardDic.ContainsKey(toBoardId))
            {
                Tips.PopError(ErrorID.SystemError);
                return;
            }
            StarterBoardCardDic.Add(toBoardId, StarterBoardCardDic[fromBoardId]);
            StarterBoardCardDic.Remove(fromBoardId);
            BoardIdList.Remove(fromBoardId);
            BoardIdList.Add(toBoardId);
            CheckFormationRepeat();
        }

        public void MoveSubstituteBoard(int fromBoardId, int toBoardId)
        {
            SetChangeFlag(true);
            if (SubstituteBoardCardDic.ContainsKey(toBoardId))
            {
                SubstituteBoardCardDic[toBoardId] = SubstituteBoardCardDic[fromBoardId];
                SubstituteBoardCardDic[fromBoardId] = 0;
            }
        }

        protected virtual void AddExchangeTimes()
        {

        }
        public virtual bool CheckCanExchangeCard()
        {
            return true;
        }

        public void SwapBoard(int swapType, int fromBoardId, int toBoardId)
        {
            SetChangeFlag(true);
            int tmpCardId = 0;
            switch (swapType)
            {
                case FormationSwapType.MainToMain:
                    // 主力位置互换
                    tmpCardId = StarterBoardCardDic[fromBoardId];
                    StarterBoardCardDic[fromBoardId] = StarterBoardCardDic[toBoardId];
                    StarterBoardCardDic[toBoardId] = tmpCardId;
                    return;
                case FormationSwapType.MainToBench:
                    // 主力换替补
                    tmpCardId = StarterBoardCardDic[fromBoardId];
                    StarterBoardCardDic[fromBoardId] = SubstituteBoardCardDic[toBoardId];
                    SubstituteBoardCardDic[toBoardId] = tmpCardId;
                    AddExchangeTimes();
                    return;
                case FormationSwapType.BenchToBench:
                    // 替补位置互换
                    tmpCardId = SubstituteBoardCardDic[fromBoardId];
                    SubstituteBoardCardDic[fromBoardId] = SubstituteBoardCardDic[toBoardId];
                    SubstituteBoardCardDic[toBoardId] = tmpCardId;
                    return;
                case FormationSwapType.BenchToMan:
                    // 替补换主力
                    tmpCardId = SubstituteBoardCardDic[fromBoardId];
                    SubstituteBoardCardDic[fromBoardId] = StarterBoardCardDic[toBoardId];
                    StarterBoardCardDic[toBoardId] = tmpCardId;
                    AddExchangeTimes();
                    return;
            }
        }

        public void SwapBenchWithBackup(int backupCardId, int benchBoardId)
        {
            SetChangeFlag(true);
            SubstituteBoardCardDic[benchBoardId] = backupCardId;
        }
        public void SwapMainWithBackup(int backupCardId, int mainBoardId)
        {
            SetChangeFlag(true);
            StarterBoardCardDic[mainBoardId] = backupCardId;
        }

        public void SetBaseFormationName(string name)
        {
            BaseFormationName = name;
            FormationName = name;
        }

        public void CheckFormationRepeat()
        {
            var temp = Player.FightManager.FormationController.CheckFormationBoardIdRepeat(BoardIdList);
            if (temp == null)
            {
                FormationName = GetNewFormationName();
            }
            else
            {
                FormationName = temp.Name;
            }
            EventManager.Instance.Dispatch(EventID.OnChangeFormation, FormationName);
        }

        public string GetNewFormationName()
        {
            return $"{BaseFormationName}^*";
        }

        public void ExchangeCardBoard(PlayerCard a_card, PlayerCard b_card)
        {
            if (a_card == null || b_card == null)
            {
                Debug.Log("error ");
                return;
            }

            SetChangeFlag(true);

            // todo bug fix
            // 这里不应该同步到布阵模块，
            // 第一在战斗中调整，不需要同步，
            // 第二，有可能这里同步了，但是并不能保证一定能保存到服务端
            var a_data = a_card.FormationDataDic[FormationId];
            var b_data = b_card.FormationDataDic[FormationId];
            var tempBoardId = b_data.BoardId;
            var tempState = b_data.State;
            var tempSubstituteIndex = b_data.SubstituteIndex;
            b_data.SetData(a_data.State, a_data.BoardId, a_data.SubstituteIndex);
            a_data.SetData(tempState, tempBoardId, tempSubstituteIndex);

            UpdateFormationByCardData(a_card.CardId, a_data);
            UpdateFormationByCardData(b_card.CardId, b_data);
        }

        private void UpdateFormationByCardData(int cardId, FormationData data)
        {
            if (data.State == FormationCardState.Starter)
            {
                SetStarterBoardIdCard(data.BoardId, cardId);
            }
            else if (data.State == FormationCardState.Substitute)
            {
                SetSubstituteBoardIdCard(data.SubstituteIndex, cardId);
            }
        }

        private void SetStarterBoardIdCard(int boardId, int cardId)
        {
            if (StarterBoardCardDic.ContainsKey(boardId))
            {
                StarterBoardCardDic[boardId] = cardId;
            }
        }

        private void SetSubstituteBoardIdCard(int index, int cardId)
        {
            if (index < 1 || index > FormationConst.SubstituteCount) return;
            if (SubstituteBoardCardDic.ContainsKey(index) == false) return;
            SubstituteBoardCardDic[index] = cardId;
        }

        #endregion


        #region tactics

        //public void UseTacticsTemp(List<int> tempTacticsIdList)
        //{
        //    TacticsIdList = CopyUtils.DeepCopy<int>(tempTacticsIdList);

        //    SetChangeFlag(true);
        //}

        //public void SetTacticsId(int tacticsType, int level)
        //{
        //    int tacticsId = tacticsType * 100 + level;
        //    var config = Configs.Tactics.GetConfig(tacticsId);
        //    if (config == null) return;

        //    SetChangeFlag(true);

        //    for (int i = 0; i < TacticsIdList.Count; i++)
        //    {
        //        var id = TacticsIdList[i];
        //        if (id / 100 == tacticsType)
        //        {
        //            TacticsIdList[i] = tacticsId;
        //            return;
        //        }
        //    }
        //}


        #endregion

        public void SetChangeFlag(bool b)
        {
            _changeFlag = b;
        }

        #region auto make

        private bool CheckStartCard()
        {
            if (StarterBoardCardDic.Count < FormationConst.StarterCount) return false;
            foreach (var pair in StarterBoardCardDic)
            {
                var card = Player.CardManager.GetCard(pair.Value);
                if (card == null) return false;
                if (!card.CanFight()) return false;
            }

            return true;
        }

        private bool CheckTactics()
        {
            return true;
        }

        private bool CheckSubstitute()
        {
            bool flag = true;
            for (int i = 1; i <= FormationConst.SubstituteCount; i++)
            {
                if (SubstituteBoardCardDic.ContainsKey(i) == false) continue;
                var cardId = SubstituteBoardCardDic[i];
                var card = Player.CardManager.GetCard(cardId);
                if (card == null)
                {
                    SubstituteBoardCardDic[i] = 0;
                    flag = false;
                }
            }

            return flag;
        }

        // public bool CheckNeedMake()
        // {
        //     if (!_isInitialized) return true;
        //     var startCardFlag = CheckStartCard();
        //     if (!startCardFlag) return true;
        //     var substituteFlag = CheckSubstitute();
        //     if (!substituteFlag) return true;
        //     var tacticsFlag = CheckTactics();
        //     if (!tacticsFlag) return true;
        //
        //     return false;
        // }

        protected void ClearFormation()
        {
            TacticsIdList.Clear();
            BoardIdList.Clear();
            StarterBoardCardDic.Clear();
            for (int i = 1; i <= FormationConst.SubstituteCount; i++)
            {
                if (SubstituteBoardCardDic.ContainsKey(i) == false) continue;
                SubstituteBoardCardDic[i] = 0;
            }
        }
        protected bool AutoMakeFormation_FromPool(List<int> boardIdList, List<PlayerCard> cardPool)
        {
            ClearFormation();
            for (int i = boardIdList.Count - 1; i >= 0; i--)
            {
                var boardId = boardIdList[i];
                BoardIdList.Add(boardId);
                var separatedPosition = Configs.FormationBoard.GetConfig(boardId).SeparatedPosition;
                var card = AutoSelectCard(separatedPosition, cardPool);
                if (card == null) return false;
                StarterBoardCardDic[boardId] = card.CardId;
            }

            // select substitute
            List<PlayerCard> substitutePool = new List<PlayerCard>();
            foreach (var card in cardPool)
            {
                if (!InStarter(card.CardId))
                {
                    substitutePool.Add(card);
                }
            }

            var result = substitutePool.OrderBy(item => item.FightPoint)
                .ThenBy(item => item.CardId).ToList();
            int index = 1;
            foreach (var playerCard in result)
            {
                SubstituteBoardCardDic[index++] = playerCard.CardId;
                if (index > FormationConst.SubstituteCount) break;
            }
            return true;
        }

        //根据阵型，仅仅重新对首发进行重新放置
        protected bool AutoRemakeStarter(List<int> boardIdList)
        {
            //存放目前首发的cardId
            var tmpList = new List<PlayerCard>();
            foreach (int boardId in StarterBoardCardDic.Keys)
            {
                var card = Player.CardManager.GetCard(StarterBoardCardDic[boardId]);
                tmpList.Add(card);
            }

            BoardIdList.Clear();
            StarterBoardCardDic.Clear();

            for (int i = boardIdList.Count - 1; i >= 0; i--)
            {
                var boardId = boardIdList[i];
                BoardIdList.Add(boardId);
                var separatedPosition = Configs.FormationBoard.GetConfig(boardId).SeparatedPosition;
                var card = AutoSelectCard(separatedPosition, tmpList);
                if (card == null) return false;
                StarterBoardCardDic[boardId] = card.CardId;
            }

            return true;
        }

        private bool InStarter(int targetCardId)
        {
            foreach (var cardId in StarterBoardCardDic.Values)
            {
                if (targetCardId == cardId) return true;
            }

            return false;
        }

        private bool InSubstitute(int targetCardId)
        {
            for (int i = 1; i <= FormationConst.SubstituteCount; i++)
            {
                if (SubstituteBoardCardDic.ContainsKey(i) == false) continue;
                if (targetCardId == SubstituteBoardCardDic[i])
                    return true;
            }

            return false;
        }

        private PlayerCard AutoSelectCard(int separatedPosition, List<PlayerCard> cardPool)
        {
            int max = -1;
            PlayerCard maxCard = null;

            foreach (var card in cardPool)
            {
                if (!card.CanFight()) continue;
                if (InStarter(card.CardId)) continue;
                var value = Player.CalFightPoint_Single(card.CardId, false, separatedPosition);
                if (value > max)
                {
                    max = value;
                    maxCard = card;
                }
            }

            return maxCard;
        }

        private List<PlayerCard> GetSubstituteCardList()
        {
            List<PlayerCard> list = new List<PlayerCard>();
            foreach (var pair in SubstituteBoardCardDic)
            {
                var card = Player.CardManager.GetCard(pair.Value);
                if (card != null)
                    list.Add(card);
            }

            return list;
        }

        private bool AutoMake_InSubstitute()
        {
            // 2.补全上场首发
            // 3.补全替补席
            foreach (var pair in StarterBoardCardDic)
            {
                var downCardId = -1;
                var boardId = pair.Key;
                var card = Player.CardManager.GetCard(pair.Value);
                if (card == null) downCardId = 0;
                else
                {
                    if (!card.CanFight()) downCardId = card.CardId;
                }

                if (downCardId == -1) continue;

                var separatedPosition = Configs.FormationBoard.GetConfig(pair.Key).SeparatedPosition;
                var upCard = AutoSelectCard(separatedPosition, GetSubstituteCardList());
                if (upCard == null)
                {
                    //去卡池里找
                    upCard = AutoSelectCard(separatedPosition, Player.CardManager.CardList);
                    if (upCard == null) return false;
                    else
                    {
                        StarterBoardCardDic[boardId] = upCard.CardId;
                    }
                }
                else
                {
                    StarterBoardCardDic[boardId] = upCard.CardId;
                    StarterToSubstitute(downCardId, upCard.CardId);
                }
            }

            List<PlayerCard> substitutePool = new List<PlayerCard>();
            foreach (var card in Player.CardManager.CardList)
            {
                if (!InStarter(card.CardId) && !InSubstitute(card.CardId))
                {
                    substitutePool.Add(card);
                }
            }

            var result = substitutePool.OrderBy(item => item.FightPoint)
                .ThenBy(item => item.CardId).ToList();
            int index = 0;


            for (int i = 1; i <= FormationConst.SubstituteCount; i++)
            {
                if (SubstituteBoardCardDic.ContainsKey(i) == false) continue;
                var card = Player.CardManager.GetCard(SubstituteBoardCardDic[i]);
                if (card != null) continue;
                if (index >= result.Count)
                {
                    break;
                }

                SubstituteBoardCardDic[i] = result[index++].CardId;
            }
            return true;
        }

        public void StarterToSubstitute(int downCardId, int upCardId)
        {
            for (int i = 1; i <= FormationConst.SubstituteCount; i++)
            {
                if (SubstituteBoardCardDic.ContainsKey(i) == false) continue;
                if (SubstituteBoardCardDic[i] == upCardId)
                {
                    SubstituteBoardCardDic[i] = downCardId;
                    return;
                }
            }
        }

        #endregion

        public void UpdateLineupShowTime()
        {
            LineupShowTime = Utils.DataConvUtil.ServerTime;
            SetChangeFlag(true);
        }

        public void ChangeName(string name)
        {
            FormationName = name;
            SetChangeFlag(true);
            EventManager.Instance.Dispatch(EventID.OnRefreshFormationName, FormationId, name);
        }

        public abstract void UpdateCardFormationInfo();

        public List<PlayerCard> GetStarterCards()
        {
            List<PlayerCard> list = new List<PlayerCard>();
            foreach (var item in StarterBoardCardDic)
            {
                list.Add(Player.CardManager.GetCard(item.Value));
            }
            return list;
        }
        public List<PlayerCard> GetSubstituteCards()
        {
            List<PlayerCard> list = new List<PlayerCard>();
            foreach (var item in SubstituteBoardCardDic)
            {
                list.Add(Player.CardManager.GetCard(item.Value));
            }
            return list;
        }

        public int GetMainTotalCombat()
        {
            int totalCombat = 0;
            foreach (var item in StarterBoardCardDic)
            {
                int separatedPosition = Configs.FormationBoard.GetDataDictionary()[item.Key].SeparatedPosition;
                PlayerCard playerCard = Player.CardManager.GetCard(item.Value);
                int CombatEffectiveness = playerCard.FightPoint;
                CombatEffectiveness = Mathf.RoundToInt(CombatEffectiveness * (((float)playerCard.Config.PositionRatio[separatedPosition]) / 100));
                totalCombat += CombatEffectiveness;
            }
            foreach (var item in SubstituteBoardCardDic.Values)
            {
                if (item == 0) continue;
                PlayerCard playerCard = Player.CardManager.GetCard(item);
                if (playerCard == null) continue;
                int CombatEffectiveness = playerCard.FightPoint;
                CombatEffectiveness = Mathf.RoundToInt(CombatEffectiveness * 0.6f);
                totalCombat += CombatEffectiveness;
            }
            return totalCombat;
        }

        public void RemoveCard(int cardId)
        {
            int deleteKey = -1;
            foreach (var item in SubstituteBoardCardDic)
            {
                if (item.Value == cardId)
                {
                    deleteKey = item.Key;
                    break;
                }
            }
            if (deleteKey != -1)
            {
                SubstituteBoardCardDic[deleteKey] = 0;
            }
        }

        /// <summary>
        /// boardid, list 每个位置的爆发技能
        /// </summary>
        public Dictionary<int, List<SkillGiftItemData>> boardGiftSkillList;
        /// <summary>
        /// sectionid(1-4)， list，每节的爆发技能
        /// </summary>
        public Dictionary<int, List<SkillGiftItemData>> sectionGiftSkillList;
        /// <summary>
        /// sectionid(1-4), cardidList  每节的爆发球员
        /// </summary>
        public Dictionary<int, HashSet<int>> sectionFireCard;
        /// <summary>
        /// 第几节爆发，爆发几个星
        /// </summary>
        public KeyValuePair<int, int> fireSection = new KeyValuePair<int, int>();
        /// <summary>
        /// 第几节爆发，爆发几个星，这个用来飘字的时候对比
        /// </summary>
        public KeyValuePair<int, int> oldfireSection = new KeyValuePair<int, int>();
        /// <summary>
        /// 球队爆发加成参数
        /// </summary>
        public static readonly List<int> fireAddList = new List<int> { 0, 5, 8, 12, 18, 25 };
        /// <summary>
        /// 球队爆发加成参数
        /// </summary>
        public List<int> FireAddList
        {
            get
            {
                return fireAddList;
            }
        }
        /// <summary>
        /// 从球队爆发加成，反推出有几人爆发
        /// </summary>
        /// <param name="percent">球队爆发加成</param>
        /// <returns>有几人爆发</returns>
        public static int GetFireCount(int percent)
        {
            for (int i = 0; i < fireAddList.Count; i++)
            {
                if (percent == fireAddList[i]) return i;
            }
            return 0;
        }
        /// <summary>
        /// 分析阵容的爆发特性，产出爆发参数，具体跟踪进来看。
        /// </summary>
        public void Analysis()
        {
            //boardid, list
            boardGiftSkillList = new();
            //sectionid(1-4)， list
            sectionGiftSkillList = new() { { 1, new List<SkillGiftItemData>() }, { 2, new List<SkillGiftItemData>() }, { 3, new List<SkillGiftItemData>() }, { 4, new List<SkillGiftItemData>() } };
            //sectionid(1-4), 爆发的球员个数
            sectionFireCard = new() { { 1, new HashSet<int>() }, { 2, new HashSet<int>() }, { 3, new HashSet<int>() }, { 4, new HashSet<int>() } };
            foreach (var _boardId in StarterBoardCardDic.Keys)
            {
                var _skillList = Player.CardManager.GetGiftSkill(StarterBoardCardDic[_boardId]);
                _skillList.ForEach((_skill) =>
                {

                    if (_skill.cfg.Fire > 0)
                    {
                        if (_skill.cfg.When == FActionTimeType.OnBattle)
                        {
                            //战斗开始触发的为第1节爆发
                            sectionGiftSkillList[1].Add(_skill);
                            if (_skill.isUnLock) sectionFireCard[1].Add(_skill.cardId);
                        }
                        else if (_skill.cfg.When == FActionTimeType.OnSection)
                        {
                            //单节爆发的读取具体节数
                            int section = _skill.cfg.Wparam2;
                            sectionGiftSkillList[section].Add(_skill);
                            if (_skill.isUnLock) sectionFireCard[section].Add(_skill.cardId);
                        }
                    }
                });
                boardGiftSkillList.Add(_boardId, _skillList);
            }
            fireSection = getMaxKey(sectionFireCard);
        }

        private KeyValuePair<int, int> getMaxKey(Dictionary<int, HashSet<int>> list)
        {
            int result = 0;
            int count = 0;
            foreach (var key in list.Keys)
            {
                int newcount = list[key].Count;
                if (newcount > count)
                {
                    result = key;
                    count = newcount;
                }
            }
            return new KeyValuePair<int, int>(result, count);
        }
    }
}