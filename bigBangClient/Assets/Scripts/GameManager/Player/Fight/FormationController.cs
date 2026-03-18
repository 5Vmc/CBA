using CBA;
using GameConfig;
using GameConfig.Config;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utils;

namespace BigBang
{
    public class FormationController
    {
        private PlayerFightManager _manager;
        //public List<FormationTemp> FormationTempList { get; private set; }
        public Dictionary<string, FormationTemp> FormationTempDic { get; private set; }
        public List<TacticsTemp> TacticsTempList { get; set; }
        // public Formation DefaultPveFormation { get; set; }
        // public Formation DefaultPvpFormation { get; set; }

        private Dictionary<int, Formation> _defaultFormationDic = new();
        private Dictionary<int, int> _tacticsLevelDic = new();

        public Dictionary<int, int> TacticsLevelDic
        {
            get
            {
                return _tacticsLevelDic;
            }
        }

        /// <summary>
        /// 是否需要阵容推荐，记录在内存中
        /// </summary>
        public bool isNeedRecommendedFormation = true;

        public const int TempCharMaxCount = 20;
        public const int MaxFormationTempNumber = 10; //最大阵行模板个数

        public FormationController(PlayerFightManager manager)
        {
            _manager = manager;

            //FormationTempList = new List<FormationTemp>();
            FormationTempDic = new Dictionary<string, FormationTemp>();
            TacticsTempList = new List<TacticsTemp>();
        }

        public void Init()
        {
            //FormationTempList.Clear();
            _defaultFormationDic.Clear();
            _tacticsLevelDic.Clear();
            FormationTempDic.Clear();
            TacticsTempList.Clear();
            foreach (var sysFormationConfig in Configs.SysFormation.GetConfigList())
            {
                var temp = new FormationTemp();
                temp.InitFromConfig(sysFormationConfig);
                AddFormationTemp(temp);
            }

            foreach (var sysTacticsConfig in Configs.SysTactics.GetConfigList())
            {
                var temp = new TacticsTemp();
                temp.InitFromConfig(sysTacticsConfig);
                AddTacticsTemp(temp);
            }

            _defaultFormationDic.Add(FormationID.PVP, new Formation(FormationID.PVP));
            _defaultFormationDic.Add(FormationID.PVE, new Formation(FormationID.PVE));
            _defaultFormationDic.Add(FormationID.ARENA, new Formation(FormationID.ARENA));
            _defaultFormationDic.Add(FormationID.HERO, new Formation(FormationID.HERO));
            _defaultFormationDic.Add(FormationID.TOWER, new Formation(FormationID.TOWER));
            _defaultFormationDic.Add(FormationID.Hundred, new Formation(FormationID.Hundred));
        }

        public void UnPack(FormationControllerInfo data)
        {
            foreach (var formationTempInfo in data.FormationTempList)
            {
                var temp = new FormationTemp(formationTempInfo);
                AddFormationTemp(temp);
            }

            // foreach (var tacticsTempInfo in data.TacticsTempList)
            // {
            //     var temp = new TacticsTemp(tacticsTempInfo);
            //     AddTacticsTemp(temp);
            // }
            foreach (var tactics in data.TacticsLevelsList)
            {
                if (_tacticsLevelDic.ContainsKey(tactics.Id))
                {
                    _tacticsLevelDic[tactics.Id] = tactics.Level;
                }
                else
                {
                    _tacticsLevelDic.Add(tactics.Id, tactics.Level);
                }
            }
            foreach (var pairItem in data.DefaultFormationList)
            {
                GetDefaultFormation(pairItem.FormationId)?.UnPack(pairItem);
            }
        }

        public void LoginSuccess()
        {
            foreach (var formation in _defaultFormationDic.Values)
            {
                formation.UpdateCardFormationInfo();
            }
        }

        //检查并且获取到默认阵容
        public void GetAndCheckDefaultFormation(int formationId, Action<Formation> callBack)
        {
            var formation = GetDefaultFormation(formationId);
            callBack.Invoke(formation);

            //NetworkManager.Instance.GetDefaultFormation(formationId, response =>
            //{
            //    var formation = GetDefaultFormation(formationId);
            //    formation.UnPack(response.Formation);
            //    formation.UpdateCardFormationInfo();

            //    Debug.Log("SaveFormation callback :" + response.ToString());

            //    callBack.Invoke(formation);
            //});
        }

        /// <summary>
        /// 当前经典赛阵容是否为推荐阵容
        /// </summary>
        public bool IsBestClassicFormation(FightType fightType, bool isNeedInfo, out string info)
        {
            info = "";
            Formation formation = GetFormation(fightType);
            Tuple<Dictionary<int, int>, Dictionary<int, int>> StartAndSubstituteDic = Player.FightManager.FormationController.GetStartAndSubstituteDic(formation);
            if (Player.FightManager.FormationController.IsFormationSame(formation, StartAndSubstituteDic))
            {
                return true;
            }

            if (!isNeedInfo) return false;

            StringBuilder startInfoSB = new StringBuilder();
            foreach (var key in formation.StarterBoardCardDic.Keys)
            {
                if (formation.StarterBoardCardDic[key] != StartAndSubstituteDic.Item1[key])
                {
                    var card1 = Player.CardManager.GetCard(formation.StarterBoardCardDic[key]);
                    var card2 = Player.CardManager.GetCard(StartAndSubstituteDic.Item1[key]);
                    if (startInfoSB.Length == 0) startInfoSB.Append("首发阵容:\n");
                    startInfoSB.AppendFormat("[{0}]{1}-{2}号↓   [{3}]{4}-{5}号↑\n", card1.GetPositionName(), PlayerCard.GetFullName(card1.Config), card1.PlayerCardNumber, card2.GetPositionName(), PlayerCard.GetFullName(card2.Config), card2.PlayerCardNumber);
                }
            }

            List<int> upCardId = new();
            HashSet<int> oldValue = new();
            foreach (var item in formation.SubstituteBoardCardDic.Values)
            {
                oldValue.Add(item);
            }
            foreach (var item in StartAndSubstituteDic.Item2.Values)
            {
                if (oldValue.Contains(item) == false)
                {
                    upCardId.Add(item);
                }
            }

            List<int> downCardId = new();
            HashSet<int> newValue = new();
            foreach (var item in StartAndSubstituteDic.Item2.Values)
            {
                newValue.Add(item);
            }
            foreach (var item in formation.SubstituteBoardCardDic.Values)
            {
                if (item != 0 && newValue.Contains(item) == false)
                {
                    downCardId.Add(item);
                }
            }

            StringBuilder substituteInfoSB = new StringBuilder();
            for (int i = 0; i < upCardId.Count + downCardId.Count; i++)
            {
                bool isFirst = i % 2 == 0;
                bool isUp = i < upCardId.Count;
                int cardId = isUp ? upCardId[i] : downCardId[i - upCardId.Count];
                var card = Player.CardManager.GetCard(cardId);
                if (card == null) continue;
                if (substituteInfoSB.Length == 0) substituteInfoSB.Append("替补阵容:\n");
                string format = "";
                if (isFirst)
                {
                    format = "[{0}]{1}-{2}号" + (isUp ? "↑" : "↓");
                }
                else
                {
                    format = "   [{0}]{1}-{2}号  " + (isUp ? "↑" : "↓") + "\n";
                }
                substituteInfoSB.AppendFormat(format, card.GetPositionName(), PlayerCard.GetFullName(card.Config), card.PlayerCardNumber);
            }

            info = startInfoSB.ToString() + "\n" + substituteInfoSB.ToString();
            if ((upCardId.Count + downCardId.Count) % 2 != 0) info += "\n";

            return false;
        }

        /// <summary>
        /// 当前经典赛阵容更改为推荐阵容
        /// </summary>
        public void ChangeClassicFormationToBest()
        {
            Formation formation = GetFormation(FightType.PVE);
            Tuple<Dictionary<int, int>, Dictionary<int, int>> StartAndSubstituteDic = Player.FightManager.FormationController.GetStartAndSubstituteDic(formation);
            formation.StarterBoardCardDic = StartAndSubstituteDic.Item1;
            formation.SubstituteBoardCardDic = StartAndSubstituteDic.Item2;
            formation.SetChangeFlag(true);
            formation.SaveToServer();
        }


        #region 经典赛自动换人
        /*
            一键换人规则：优先顺序
            1.  位置对应优先，完全一致优先，没有位置完全一致按大类一致优先（使用配置表里的擅长位置）
            [大前锋，小前锋] 
            [控球后卫， 得分后卫]
            [中锋]
            //2. 颜色品质高优先
            //3. 星级优先
            4. 能力优先
            5. 上述都没有，按能力排序选一个
            6. 选7人进入大名单：先选5个按照 [ 1 - 5 ]步骤，最后2个按照能力排序选2个
         */

        public bool IsFormationSame(FormationBase formationBase, Tuple<Dictionary<int, int>, Dictionary<int, int>> StartAndSubstituteDic)
        {
            if (IsDicSame(StartAndSubstituteDic.Item1, formationBase.StarterBoardCardDic) == false) return false;
            if (IsDicValueSame(StartAndSubstituteDic.Item2, formationBase.SubstituteBoardCardDic) == false) return false;
            return true;
        }

        public bool IsDicSame(Dictionary<int, int> dic1, Dictionary<int, int> dic2)
        {
            if (dic1.Count != dic2.Count) return false;
            foreach (var item in dic1)
            {
                if (dic2.ContainsKey(item.Key) == false) return false;
                if (dic2[item.Key] != item.Value) return false;
            }
            return true;
        }
        public bool IsDicValueSame(Dictionary<int, int> dic1, Dictionary<int, int> dic2)
        {
            if (dic1.Count != dic2.Count) return false;
            HashSet<int> dic2Value = new();
            foreach (var item in dic2.Values)
            {
                dic2Value.Add(item);
            }
            foreach (var item in dic1.Values)
            {
                if (dic2Value.Contains(item) == false) return false;
            }
            return true;
        }

        public Tuple<Dictionary<int, int>, Dictionary<int, int>> GetStartAndSubstituteDic(FormationBase formationBase)//获取首发与替补阵容
        {
            HashSet<PlayerCard> usedPlayerSet = new();

            List<PlayerCard> playerCardList = new();
            playerCardList.AddRange(Player.CardManager.CardList);
            playerCardList = playerCardList
                .OrderByDescending(card => card.FightPoint)
                .ThenBy(card => card.CardId).ToList();

            Dictionary<int, int> starterBoardCardDic = Get5PosPlayerCardDic(formationBase, playerCardList, usedPlayerSet);

            Dictionary<int, int> substituteBoardCardDic = new();

            Dictionary<int, int> substituteBoardCardDicTemp = Get5PosPlayerCardDic(formationBase, playerCardList, usedPlayerSet);

            int addCount = -2;
            foreach (PlayerCard playerCard in playerCardList)
            {
                if (usedPlayerSet.Contains(playerCard) == true) continue;
                substituteBoardCardDicTemp.Add(addCount, playerCard.Config.Id);
                usedPlayerSet.Add(playerCard);
                addCount++;
                if (addCount == 0) break;
            }

            int substituteBoardCardPosIndex = 1;
            foreach (var substituteBoardCardPair in substituteBoardCardDicTemp)
            {
                if (substituteBoardCardPair.Value == 0) continue;
                substituteBoardCardDic.Add(substituteBoardCardPosIndex, substituteBoardCardPair.Value);
                substituteBoardCardPosIndex++;
            }

            for (int i = 1; i <= FormationConst.SubstituteCount; i++)
            {
                if (substituteBoardCardDic.ContainsKey(i) == false)
                {
                    substituteBoardCardDic.Add(i, 0);
                }
            }

            Tuple<Dictionary<int, int>, Dictionary<int, int>> tuple = new(starterBoardCardDic, substituteBoardCardDic);
            return tuple;
        }

        public Dictionary<int, int> Get5PosPlayerCardDic(FormationBase formationBase, List<PlayerCard> playerCardList, HashSet<PlayerCard> usedPlayerSet, HashSet<PlayerCard> mustUseSet = null, HashSet<PlayerCard> mustFirstUsePlayerSet = null)//按照 [ 1 - 5 ]步骤获取5个位置的最佳
        {
            Dictionary<int, int> boardCardDicTemp = new();
            foreach (FormationBoardConfig formationBoard in Configs.FormationBoard.GetConfigList())
            {
                boardCardDicTemp.Add(formationBoard.Id, 0);
            }

            if (mustFirstUsePlayerSet != null)
            {
                if (formationBase.FormationId != FormationID.Bounty)
                {
                    if (mustFirstUsePlayerSet.Count > 0) ProcessLimitGet5PosPlayer(formationBase, usedPlayerSet, mustFirstUsePlayerSet, boardCardDicTemp, 1);
                    if (mustFirstUsePlayerSet.Count > 0) ProcessLimitGet5PosPlayer(formationBase, usedPlayerSet, mustFirstUsePlayerSet, boardCardDicTemp, 2);
                }
                if (mustFirstUsePlayerSet.Count > 0) ProcessLimitGet5PosPlayer(formationBase, usedPlayerSet, mustFirstUsePlayerSet, boardCardDicTemp, 3);
            }

            if (mustUseSet != null)
            {
                if (formationBase.FormationId != FormationID.Bounty)
                {
                    if (mustUseSet.Count > 0) ProcessLimitGet5PosPlayer(formationBase, usedPlayerSet, mustUseSet, boardCardDicTemp, 1);
                    if (mustUseSet.Count > 0) ProcessLimitGet5PosPlayer(formationBase, usedPlayerSet, mustUseSet, boardCardDicTemp, 2);
                }
                if (mustUseSet.Count > 0) ProcessLimitGet5PosPlayer(formationBase, usedPlayerSet, mustUseSet, boardCardDicTemp, 3);
            }

            HashSet<PlayerCard> allPlayerSet = playerCardList.ToHashSet();
            if (formationBase.FormationId != FormationID.Bounty)
            {
                ProcessLimitGet5PosPlayer(formationBase, usedPlayerSet, allPlayerSet, boardCardDicTemp, 1);
                ProcessLimitGet5PosPlayer(formationBase, usedPlayerSet, allPlayerSet, boardCardDicTemp, 2);
            }
            ProcessLimitGet5PosPlayer(formationBase, usedPlayerSet, allPlayerSet, boardCardDicTemp, 3);

            return boardCardDicTemp;

        }

        private void ProcessLimitGet5PosPlayer(FormationBase formationBase, HashSet<PlayerCard> usedPlayerSet, HashSet<PlayerCard> needUseSet, Dictionary<int, int> boardCardDicTemp, int condition)
        {
            List<PlayerCard> mustUseList = null;
            if (formationBase.FormationId == FormationID.Bounty)
            {
                mustUseList = needUseSet
                .OrderBy(card => card.IsUsingInBounty)
                .ThenBy(card => card.FightPoint)
                .ThenBy(card => card.CardId)
                .ToList();
            }
            else
            {
                mustUseList = needUseSet
                .OrderByDescending(card => card.FightPoint)
                .ThenBy(card => card.CardId)
                .ToList();
            }

            foreach (FormationBoardConfig formationBoard in Configs.FormationBoard.GetConfigList())
            {
                if (boardCardDicTemp[formationBoard.Id] != 0) continue;
                foreach (PlayerCard playerCard in mustUseList)
                {
                    bool isFind = false;
                    switch (condition)
                    {
                        case 1:
                            {
                                if (usedPlayerSet.Contains(playerCard) == false && playerCard.Config.AdaptPosition[0] == formationBoard.SeparatedPosition)
                                {
                                    boardCardDicTemp[formationBoard.Id] = playerCard.Config.Id;
                                    usedPlayerSet.Add(playerCard);
                                    needUseSet.Remove(playerCard);
                                    isFind = true;
                                    break;
                                }
                            }
                            break;
                        case 2:
                            {
                                if (usedPlayerSet.Contains(playerCard) == false && playerCard.Config.AdaptPosition.Length >= 2 && playerCard.Config.AdaptPosition[1] == formationBoard.SeparatedPosition)
                                {
                                    boardCardDicTemp[formationBoard.Id] = playerCard.Config.Id;
                                    usedPlayerSet.Add(playerCard);
                                    needUseSet.Remove(playerCard);
                                    isFind = true;
                                    break;
                                }
                            }
                            break;
                        case 3:
                            {
                                if (usedPlayerSet.Contains(playerCard) == false)
                                {
                                    boardCardDicTemp[formationBoard.Id] = playerCard.Config.Id;
                                    usedPlayerSet.Add(playerCard);
                                    needUseSet.Remove(playerCard);
                                    isFind = true;
                                    break;
                                }
                            }
                            break;
                    }
                    if (isFind) break;
                }
            }
        }

        #endregion

        //现在都是放在客户端了
        public Formation GetFormation(FightType fightType)
        {
            int formationId = -1;
            switch (fightType)
            {
                case FightType.PVE: formationId = FormationID.PVE; break;
                case FightType.League: case FightType.Cup: formationId = FormationID.PVP; break;
                case FightType.ARENA: formationId = FormationID.ARENA; break;
                case FightType.Hero: formationId = FormationID.HERO; break;
                case FightType.Tower: formationId = FormationID.TOWER; break;
                case FightType.Hundred: formationId = FormationID.Hundred; break;
                default:
                    {
                        Debug.LogWarningFormat("FormationController , GetFormation , FightType not match to FormationID , FightType = {0}", fightType.ToString());
                        formationId = FormationID.PVE;
                    }
                    break;
            }
            return _defaultFormationDic[(int)formationId];
        }
        public Formation GetFormation(int formationId)
        {
            return _defaultFormationDic[formationId];
        }

        public List<Formation> GetAllFormationList()
        {
            List<Formation> formationList = new();
            foreach (Formation formation in _defaultFormationDic.Values)
            {
                formationList.Add(formation);
            }
            return formationList;
        }

        // 获取默认阵容，私有函数，对外建议调用GetAndCheckDefaultFormation
        private Formation GetDefaultFormation(int formationId)
        {
            if (_defaultFormationDic.ContainsKey(formationId))
            {
                return _defaultFormationDic[formationId];
            }

            return null;
        }

        private void AddFormationTemp(FormationTemp temp)
        {
            if (FormationTempDic.ContainsKey(temp.Name))
            {
                Debug.LogError("Add Formation Temp With Same Name: " + temp.Name);
                return;
            }
            FormationTempDic.Add(temp.Name, temp);
            //FormationTempList.Add(temp);
        }

        private void AddTacticsTemp(TacticsTemp temp)
        {
            TacticsTempList.Add(temp);
        }

        // public FormationTemp GetFormationTemp(int tempId)
        // {
        //     return null;
        // }

        public FormationTemp CheckFormationBoardIdRepeat(List<int> boardIdList)
        {
            boardIdList.Sort();
            foreach (var temp in FormationTempDic.Values)
            {
                if (temp.CheckSame(boardIdList))
                {
                    return temp;
                }
            }
            //foreach (var temp in FormationTempList)
            //{
            //    if (temp.CheckSame(boardIdList))
            //    {
            //        return temp;
            //    }
            //}

            return null;
        }

        public TacticsTemp CheckTacticsIdRepeat(List<int> tacticsIdList)
        {
            tacticsIdList.Sort();
            foreach (var tacticsTemp in TacticsTempList)
            {
                if (tacticsTemp.CheckSame(tacticsIdList))
                {
                    return tacticsTemp;
                }
            }

            return null;
        }

        private ErrorID CheckFormationTempNameLegal(string name)
        {
            var length = System.Text.Encoding.Default.GetBytes(name).Length;
            if (length > TempCharMaxCount)
                return ErrorID.FormationTemplateNameTooLong;
            if (FormationTempDic.ContainsKey(name))
                return ErrorID.FormationTemplateRepeatName;

            return ErrorID.None;
        }

        private bool CheckBoardIdListLegal(List<int> boardIdList)
        {
            if (boardIdList.Count != 11) return false;
            bool gkFlag = false;
            foreach (var boardId in boardIdList)
            {
                if (boardId == FormationBoardId.GKId)
                {
                    gkFlag = true;
                }

                var config = Configs.FormationBoard.GetConfig(boardId);
                if (Configs.FormationBoard.GetConfig(boardId) == null)
                {
                    return false;
                }

                if (config.SeparatedPosition == 0)
                {
                    return false;
                }
            }

            return gkFlag;
        }


        private int GetFormationTempNum()
        {
            int count = FormationTempDic.Count - Configs.SysFormation.GetConfigList().Count;
            //int count = FormationTempList.Count - Configs.SysFormation.GetConfigList().Count;
            if (count < 0) count = 0;
            return count;
        }

        public void SaveFormationTemp(string name, FormationBase formation, Action successCallBack)
        {
            if (GetFormationTempNum() >= MaxFormationTempNumber)
            {
                Tips.PopError(ErrorID.FormationTemplateMax);
                return;
            }

            var errorId = CheckFormationTempNameLegal(name);
            if (errorId != ErrorID.None)
            {
                Tips.PopError(errorId);
                return;
            }

            if (!CheckBoardIdListLegal(formation.BoardIdList))
            {
                Tips.PopError(ErrorID.SystemError);
                return;
            }

            NetworkManager.Instance.SaveFormationTemp(name, formation.BoardIdList, resp =>
            {
                var temp = new FormationTemp(resp.FormationTemp);
                AddFormationTemp(temp);
                formation.SetBaseFormationName(name);
                successCallBack?.Invoke();
            });
        }

        public void DeleteFormationTemp(FormationTemp fTemp, Action successCallback)
        {
            NetworkManager.Instance.DelFormationTemp(fTemp.TempId, response =>
            {
                if (response.Success)
                {
                    FormationTempDic.Remove(fTemp.Name);
                    fTemp = null;
                    //FormationTempList.Remove(fTemp);
                    successCallback?.Invoke();
                }
            });
        }

        public void RenameFormationTemp(string name, FormationTemp fTemp, Action successCallback)
        {
            var errorId = CheckFormationTempNameLegal(name);
            if (errorId != ErrorID.None)
            {
                Tips.PopError(errorId);
                return;
            }

            NetworkManager.Instance.ChangeFormationTempName(fTemp.TempId, name, response =>
            {
                var oldName = fTemp.Name;
                FormationTempDic.Remove(oldName);
                fTemp.Name = name;
                FormationTempDic.Add(fTemp.Name, fTemp);
                foreach (var formation in _defaultFormationDic.Values)
                {
                    if (formation.FormationName == oldName)
                    {
                        formation.ChangeName(name);
                    }
                }

                successCallback?.Invoke();
            });
        }

        private int GetTacticsTempNum()
        {
            int count = TacticsTempList.Count - Configs.SysTactics.GetConfigList().Count;
            if (count < 0) count = 0;
            return count;
        }

        public bool CheckTacticsTempNameLegal(string name)
        {
            name = name.Substring(0, TempCharMaxCount);
            foreach (var temp in TacticsTempList)
            {
                if (temp.Name == name)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckTacticsIdListLegal(List<int> idList)
        {
            return true;
        }

        public void SaveFormationToServer(FormationBase formation)
        {
            if (formation == null) return;

            // var formation = GetDefaultFormation(formationId);
            formation.SaveToServer();
        }

        /// <summary>
        /// 从球员位置ID，获取boardId，在天赋技能中会用到，技能作用目标填的是1,2,3,4,5
        /// </summary>
        /// <param name="positionId"></param>
        /// <returns></returns>
        public int GetBoardIdFromPositionId(int positionId)
        {
            return Configs.FormationBoard.GetConfigList().Find(p => p.SeparatedPosition == positionId).Id;
        }

        /// <summary>
        /// 天赋技能处理成战斗和布阵能用的信息, 没写完，暂时没空写了
        /// </summary>
        /// <param name="formationId"></param>
        public void AnalysisGiftSkill(FightType formationId)
        {
            Formation formation = Player.FightManager.FormationController.GetFormation(formationId);
            List<PlayerCard> list = new();

            //准备球员
            foreach (var _cardId in formation.StarterBoardCardDic.Values)
            {
                var card = Player.CardManager.GetCard(_cardId);
                list.Add(card);
            }


            foreach (var card in list)
            {
                var skillList = Player.CardManager.GetGiftSkill(card.CardId);
                foreach (var skill in skillList)
                {
                    FGiftSkill Fskill = new FGiftSkill(skill.cfg);
                    FActionPlayer.PlayAction(card, Fskill, list, formation.StarterBoardCardDic);
                }
            }
        }

        /// <summary>
        /// 百分大战中是否需要提示换了状态不佳的球员，记录在内存中
        /// </summary>
        public bool isNeedAlertHundredWrongChange = true;

    }
}
