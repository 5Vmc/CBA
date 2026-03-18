using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using Babu.Config;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Protocol;
using UnityEngine;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang
{
    public class PlayerCardManager
    {
        private Dictionary<int, PlayerCard> _dic = new Dictionary<int, PlayerCard>();

        private List<PlayerCard> _list = new List<PlayerCard>();

        public List<PlayerCard> CardList
        {
            get => _list;
        }

        public RecruitController RecruitController { get; set; }
        public SkillController SkillController { get; set; }

        public PlayerCardManager()
        {
            RecruitController = new RecruitController(this);
            SkillController = new SkillController(this);
        }

        public void Init()
        {
            _dic.Clear();
            _list.Clear();

            RecruitController.Init();
            SkillController.Init();
        }

        public void UnPack(ModuleCardInfoNotify data)
        {
            if (data == null) return;
            foreach (var cardInfo in data.PlayerCardMap.Values)
            {
                var card = GetCard(cardInfo.CardId) ?? new PlayerCard(cardInfo.CardId);
                card.UnPack(cardInfo);
                AddCard(card);
            }

            RecruitController.UnPack(data.RecruitController);
            SkillController.UnPack(data.SkillController);

            EventManager.Instance.Dispatch(EventID.OnCardRefreshData);
        }

        public PlayerCard GetCard(int id)
        {
            if (_dic.ContainsKey(id)) return _dic[id];
            return null;
        }

        public void AddNewCard(int modelId, PlayerCardInfo data)
        {
            if (GetCard(modelId) != null) return;
            PlayerCard card = new PlayerCard(modelId);
            card.UnPack(data);
            AddCard(card);
            Player.CalFightPoint_Single(card.CardId, false, 0);
        }

        public void ChangeCardNumber(int cardId, int number)
        {
            GetCard(cardId).PlayerCardNumber = number;
        }

        private void AddCard(PlayerCard card)
        {
            if (!_dic.ContainsKey(card.CardId))
            {
                _dic.Add(card.CardId, card);
                _list.Add(card);
            }
        }

        public PlayerCard RemoveCard(int cardId)
        {
            if (_dic.ContainsKey(cardId))
            {
                PlayerCard card = _dic[cardId];
                _dic.Remove(cardId);
                _list.Remove(card);
                return card;
            }
            return null;
        }

        // public bool CanUpgradeStar(int cardId)
        // {
        //     var card = GetCard(cardId);
        //     if (card == null)
        //     {
        //         return false;
        //     }

        //     if (card.Star >= GameConst.MaxCardStar)
        //     {
        //         return false;
        //     }
        //     var upgradeConfig = card.GetUpgradeStarConfig(card.Star + 1);
        //     if (upgradeConfig == null)
        //     {
        //         return false;
        //     }
        //     return true;
        // }

        /**
        *upgradeType 0 star 1 quality
        **/
        private bool CheckUpgrade(int cardId, int upgradeType)
        {
            var card = GetCard(cardId);
            if (card == null)
            {
                Tips.PopError(ErrorID.SystemError);
                return false;
            }
            if (upgradeType == 0)
            {
                if (card.Star >= GameConst.MaxCardStar)
                {
                    // 该球员已满星，无法升星
                    Tips.PopError(ErrorID.ErrorAlreadyFull);
                    return false;
                }
            }


            int costPieces = 0;
            Dictionary<int, int> costGoodsDict = null;
            int costMoney = 0;
            if (upgradeType == 0)
            {
                QualityStarConfig upgradeConfig = card.GetUpgradeStarConfig(card.Star + 1);
                if (upgradeConfig == null)
                {
                    Tips.PopError(ErrorID.CanNotUpgradeStar);
                    return false;
                }
                costPieces = upgradeConfig.CostSelf;
                costGoodsDict = upgradeConfig.CostGoods;
                costMoney = upgradeConfig.CostMoney;
            }
            else if (upgradeType == 1)
            {
                QualityUpgradeConfig upgradeConfig = card.GetUpgradeQualityConfig(card.Quality + 1);
                if (upgradeConfig == null)
                {
                    Tips.PopError(ErrorID.CanNotUpgradeQuality);
                    return false;
                }
                costPieces = upgradeConfig.CostSelf;
                costGoodsDict = upgradeConfig.CostGoods;
                costMoney = upgradeConfig.CostMoney;
            }

            if (costPieces > 0)
            {
                //碎片不足
                int piecesId = card.Config.PiecesId;
                var pieces = new GoodsData(piecesId, costPieces);
                if (!Player.PackageManager.IsGoodsEnough(pieces))
                {
                    UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(pieces.ToGameItem()));
                    return false;
                }

            }
            foreach (var goodsItem in costGoodsDict)
            {
                var goods = new GoodsData(goodsItem.Key, goodsItem.Value);
                if (!Player.PackageManager.IsGoodsEnough(goods))
                {
                    // 升星道具不足
                    UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(goods.ToGameItem()));
                    return false;
                }
            }

            if (!Player.PackageManager.IsResourceEnough(ResourceId.Money, costMoney))
            {
                // 欧元不足
                Tips.PopError(ErrorID.MoneyNotEnough);
                return false;
            }

            return true;

        }

        public void CardUpgradeQuality(int cardId, Action upgradeSuccess)
        {
            //if(false == this.CheckUpgrade(cardId, 1)) return;
            NetworkManager.Instance.CardUpgradeQualityRequest(cardId, (resp) =>
            {
                var card = GetCard(resp.CardId);
                if (card == null) return;
                card.UpgradeQuality();

                upgradeSuccess();
            });
        }
        public void CardUpgradeStar(int cardId, Action upgradeSuccess)
        {
            //if(false == this.CheckUpgrade(cardId, 0))
            //    return;
            NetworkManager.Instance.CardUpgradeStar(cardId, (response) =>
            {
                OnCardUpgradeStar(response, upgradeSuccess);
            });
        }

        private void OnCardUpgradeStar(CardUpgradeStarResponse response, Action upgradeSuccess)
        {
            var card = GetCard(response.CardId);
            if (card == null) return;

            card.UpgradeStar();

            upgradeSuccess();
        }

        //获得升星需要的道具
        public List<GameItem> GetUpgradeStarItems(int cardId)
        {
            var card = GetCard(cardId);
            if (card is null) return null;
            if (card.Star >= GameConst.MaxCardStar) return null;
            var upgradeConfig = card.GetUpgradeStarConfig(card.Star + 1);
            if (upgradeConfig is null) return null;
            List<GameItem> data = new List<GameItem>();

            int costPieces = upgradeConfig.CostSelf;
            if (costPieces > 0)
            {
                int piecesId = card.Config.PiecesId;
                data.Add(GameItemUtils.CreateGameItem(GameItemType.Goods, piecesId, costPieces));
            }

            foreach (var goodsItem in upgradeConfig.CostGoods)
            {
                data.Add(GameItemUtils.CreateGameItem(GameItemType.Goods, goodsItem.Key, goodsItem.Value));
            }

            if (upgradeConfig.CostMoney > 0)
            {
                data.Add(GameItemUtils.CreateGameItem(GameItemType.Resource, ResourceId.Money,
                    upgradeConfig.CostMoney));
            }

            return data;
        }

        //获得进阶需要的
        public List<GameItem> GetUpgradeQualityItems(int cardId)
        {
            var card = GetCard(cardId);
            if (card is null) return null;
            if (card.Quality >= QualityType.Red) return null;

            var upgradeConfig = card.GetUpgradeQualityConfig(card.Quality + 1);
            if (upgradeConfig is null) return null;
            List<GameItem> data = new List<GameItem>();

            int costPieces = upgradeConfig.CostSelf;
            if (costPieces > 0)
            {
                int piecesId = card.Config.PiecesId;
                data.Add(GameItemUtils.CreateGameItem(GameItemType.Goods, piecesId, costPieces));
            }

            foreach (var goodsItem in upgradeConfig.CostGoods)
            {
                data.Add(GameItemUtils.CreateGameItem(GameItemType.Goods, goodsItem.Key, goodsItem.Value));
            }

            if (upgradeConfig.CostMoney > 0)
            {
                data.Add(GameItemUtils.CreateGameItem(GameItemType.Resource, ResourceId.Money,
                    upgradeConfig.CostMoney));
            }

            return data;
        }


        /* public void MergeCard(int id)
         {
             var card = GetCard(id);
             if (card != null) return;

             var cardConfig = Configs.CardModel.GetConfig(id);
             if (cardConfig == null) return;

             var piecesId = cardConfig.PiecesId;
             if (Player.PackageManager.IsGoodsEnough(piecesId, 60)) ;

             NetworkManager.Instance.MergeCard(piecesId, OnMergeCard);
         } */

        public void MergeCard(int piecesId, Action<MergeCardResponse> succeed)
        {
            //if (!Player.PackageManager.IsGoodsEnough(piecesId, 60))
            //{
            //    failed?.Invoke();
            //    return;
            //}
            NetworkManager.Instance.MergeCard(piecesId, succeed);
        }

        private void OnMergeCard(MergeCardResponse response)
        {
        }

        public bool IsAchieve(int configId)
        {
            var card = GetCard(configId);
            return card != null;
        }

        public List<PlayerCard> GetCardList(PositionType type = PositionType.All, int quality = -1)
        {
            var list = _list;
            if (type != PositionType.All)
            {
                list = CardList.Where(item => item.Config.Position == (int)type).ToList();
            }

            if (quality != -1)
            {
                list = list.Where(item => item.Config.Quality == quality).ToList();
            }

            var result = list.OrderByDescending(item => item.IsStarter()) //首发
                .ThenByDescending(item => item.FightPoint) // 战力
                .ThenByDescending(item => item.Quality) //品质
                .ThenBy(item => item.CardId);


            return result.ToList();
        }

        public List<PlayerCard> GetCollectionCard()
        {
            return _list.Where(item => item.Config.Sale == 1).ToList();
        }

        /// <summary>
        /// 球员装备升级
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="partIndex"></param>
        /// <param name="callback"></param>
        public void CardEquipLevelUp(PlayerCard card, int partIndex, Action callback)
        {
            NetworkManager.Instance.EquipPartLevelUp(partIndex + 1, card.CardId, (resp) =>
            {
                if (resp.Succeed)
                {
                    card.EquipLevels[partIndex]++;
                    Player.CalFightPoint_Single(card.CardId, true);
                    EventManager.Instance.Dispatch(EventID.RefreshWindow, 1);
                    callback?.Invoke();
                }
            });
        }

        public void CardEquipGradeUp(int cardId, Action callback)
        {
            NetworkManager.Instance.EquipPartUpGrade(cardId, (resp) =>
            {
                if (resp.Succeed)
                {
                    var card = Player.CardManager.GetCard(cardId);
                    card.EquipGrade++;
                    Player.CalFightPoint_Single(cardId, true);
                    //todo:   注意，玩家进阶是有选择性的，所以只对战力前5的提示小红点就可以了。
                    callback?.Invoke();
                }
            });
        }

        /// <summary>
        /// 获取天赋技能，包含了天赋技能的配置和是否解锁等信息
        /// </summary>
        /// <param name="cardid"></param>
        /// <returns></returns>
        public List<SkillGiftItemData> GetGiftSkill(int cardid)
        {
            var _card = GetCard(cardid);
            //查出解锁的几个进阶等级
            Dictionary<int, int> unlockGradeDict = new Dictionary<int, int>();
            var breakConfigs = Configs.JerseyBreak.GetConfigList().FindAll(p => p.Position == _card.DefaultPosition);
            int skillIndex = 0;
            foreach (var _cfg in breakConfigs)
            {
                if (_cfg.Talent > skillIndex)
                {
                    skillIndex = _cfg.Talent;
                    unlockGradeDict[skillIndex] = _cfg.Id % 1000;
                }
            }  //结果是第1个技能，第N级解锁


            //取技能等级
            var skillLvs = new Dictionary<int, int> { { 1, 1 }, { 2, 1 }, { 3, 1 }, { 4, 1 } };
            var skillLevelMap = new Dictionary<int, HashSet<int>>();
            var skillStarMap = new Dictionary<int, List<CardUpgradeConfig>>();
            var starConfigLst = Configs.CardUpgrade.GetConfigList().FindAll(P => P.CardId == cardid);
            foreach (var _cfg in starConfigLst)
            {
                foreach (var _key in _cfg.Sklv.Keys)
                {
                    if (!skillLevelMap.ContainsKey(_key))
                    {
                        skillLevelMap.Add(_key, new HashSet<int>() { 1 });
                    }
                    if (!skillStarMap.ContainsKey(_key))
                    {
                        skillStarMap.Add(_key, new List<CardUpgradeConfig>());
                    }
                    if (!skillLevelMap[_key].Contains(_cfg.Sklv[_key]))
                    {
                        skillLevelMap[_key].Add(_cfg.Sklv[_key]);
                        skillStarMap[_key].Add(_cfg);
                    }
                }
                //当前星级，把对应技能等级拿出来
                if (_cfg.Star == _card.Star && _cfg.Quality == _card.Quality)
                {
                    skillLvs = _cfg.Sklv;
                }
            }

            //取技能模板id，根据等级拼接技能ID
            var giftList = Configs.CardModel.GetConfig(cardid).GiftIds;
            var giftSkillCfgList = new List<GiftSkillConfig>();
            List<SkillGiftItemData> result = new List<SkillGiftItemData>();
            for (var index = 1; index <= giftList.Count(); index++)
            {
                var _cfg = Configs.GiftSkill.GetConfig(giftList[index - 1] + (skillLvs[index] - 1) * 10);
                giftSkillCfgList.Add(_cfg);

                var _data = new SkillGiftItemData(cardid, unlockGradeDict[index], _cfg);
                _data.SkillStarMap = skillStarMap[index];
                result.Add(_data);
            }

            return result;
        }


        public PlayerCard GetBestCard(PositionSeparatedType positionSeparatedType)
        {
            PlayerCard playerCardTarget = null;
            int maxCombat = -1;
            foreach (PlayerCard playerCard in CardList)
            {
                if (playerCard.GetAdaptPosition() != positionSeparatedType) continue;
                int cardCombat = playerCard.FightPoint;
                if (cardCombat > maxCombat)
                {
                    maxCombat = cardCombat;
                    playerCardTarget = playerCard;
                }
            }
            return playerCardTarget;
        }

        /// <summary>
        /// 检查球员的装备状态
        /// </summary>
        /// <param name="card"></param>
        public void CheckEquipStatusAll(PlayerCard card)
        {
            if (card.EquipStatus == null) card.EquipStatus = new CardEquipStatus();
            card.EquipStatus.PartStatus.Clear();
            List<JerseyUpgradeConfig> list = card.GetEquipLevelsConfig(card.EquipLevels);
            bool canTupo = true;
            for (int index = 0; index < 4; index++)
            {
                JerseyUpgradeConfig equipConfig = list[index];
                card.EquipStatus.PartStatus.Add(CheckEquipStatus(card, list[index]));
                if (equipConfig == null)
                {
                    card.EquipStatus.IsMaxLevel = true;
                }
                canTupo &= card.EquipStatus.PartStatus[index] == EquipStatus.LackOfUpgrade;
            }

            if (!canTupo)
            {
                card.EquipStatus.CanTuPo = EquipStatus.LackOfUpgrade;
            }
            else
            {
                var tupoConfig = Configs.JerseyBreak.GetConfig(card.DefaultPosition * 1000 + card.EquipGrade + 1);
                if (tupoConfig == null)
                {
                    //等级上限
                    card.EquipStatus.CanTuPo = EquipStatus.MaxLevel;
                }
                else if (tupoConfig.CardLevel > card.Level)
                {
                    card.EquipStatus.CanTuPo = EquipStatus.LackOfLevel;
                }
                else
                {
                    var costItems = GameItemUtils.CreateGameItems(tupoConfig.Cost).ToList();
                    string error = Player.PackageManager.IsGameItemsEnough(costItems, false);
                    card.EquipStatus.CanTuPo = error == "" ? EquipStatus.Ready : EquipStatus.LackOfMaterial;
                }
            }
        }

        /// <summary>
        /// 返回某个卡牌的某个部位状态
        /// </summary>
        /// <param name="card"></param>
        /// <param name="cfg"></param>
        public EquipStatus CheckEquipStatus(PlayerCard card, JerseyUpgradeConfig cfg)
        {
            EquipStatus Status;
            if (cfg == null)
            {
                Status = EquipStatus.MaxLevel;
            }
            else if (card.EquipGrade + 1 < cfg.Level)
            {
                Status = EquipStatus.LackOfUpgrade;
            }
            else if (card.Level < cfg.CardLevel)
            {
                Status = EquipStatus.LackOfLevel;
            }
            else
            {
                //必定是单一消耗材料
                var costItem = GameItemUtils.CreateGameItem(cfg.Cost);
                string error = Player.PackageManager.IsGameItemEnough(costItem, false);
                Status = error != "" ? EquipStatus.LackOfMaterial : EquipStatus.Ready;
            }
            return Status;
        }

        /// <summary>
        /// 检查小红点
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="clearAll"></param>
        public void CheckRedDot(int cardId = 0, bool clearAll = false)
        {

            List<PlayerCard> formationCardList = new();

            if (cardId == 0)
            {
                formationCardList = GetCardList().FindAll(item => item.IsStarter());
            }
            else
            {
                var _card = GetCard(cardId);
                if (_card.IsStarter())
                {
                    formationCardList.Add(_card);
                }
            }

            int totalExp = Player.PackageManager.GetItemExp();

            //todo
            //1为了性能，在返回首页面才检查红点;
            //2红点结构应该以5个位置12345为索引;
            //3换人的时候要重新检查红点;
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Card, "");
            if (clearAll) node.ClearChilds();

            for (var index = 0; index < formationCardList.Count; index++)
            {
                var _card = formationCardList[index];
                CheckRedDotPlayerLevel(_card, totalExp);
                CheckRedDotEquip(_card);
                CheckRedDotStar(_card);
            }
        }

        //升级小红点
        private void CheckRedDotPlayerLevel(PlayerCard _card, int totalExp)
        {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Card, "/" + _card.CardId.ToString() + "/lv");
            if (Player.Level <= _card.Level)
            {
                node.AddValue(-1);
            }
            else
            {
                var lvcfg = Configs.CardLevel.GetConfig(_card.Level);
                var exp = _card.Exp - lvcfg.ExpTotal;
                node.AddValue(lvcfg.Exp <= (exp + totalExp) ? 1 : -1);
            }
        }

        //突破和装备升级小红点
        private void CheckRedDotEquip(PlayerCard _card)
        {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Card, "/" + _card.CardId.ToString() + "/equip");
            CheckEquipStatusAll(_card);
            var isRed = _card.EquipStatus.CanTuPo == EquipStatus.Ready || _card.EquipStatus.PartStatus.Any(p => p == EquipStatus.Ready);
            node.AddValue(isRed ? 1 : -1);
        }

        //升星小红点
        private void CheckRedDotStar(PlayerCard _card)
        {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Card, "/" + _card.CardId.ToString() + "/star");
            if (_card.IsStarAndQualityMax() == false)
            {
                List<GameItem> costItems = new();
                if (_card.CouldUpgradeStarInThisQuality())
                {
                    costItems = Player.CardManager.GetUpgradeStarItems(_card.CardId);
                }
                else
                {
                    costItems = Player.CardManager.GetUpgradeQualityItems(_card.CardId);
                }
                string error = Player.PackageManager.IsGameItemsEnough(costItems, false);
                node.AddValue(error == "" ? 1 : -1);
            }
            else
            {
                node.AddValue(-1);
            }
        }


    }
}