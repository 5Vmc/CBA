using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Babu;
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
    public class RecruitController
    {
        private PlayerCardManager _cardManager;

        private List<RecruitPool> _poolList = new List<RecruitPool>();
        private Dictionary<int, RecruitPool> _poolDic = new Dictionary<int, RecruitPool>();

        public bool allowClick = true;
        // 招募次数
        public int TotalRecruitCount { get; set; }

        private static bool isDiamondEnoughButGoodsNot = false;
        public RecruitController(PlayerCardManager cardManager)
        {
            _cardManager = cardManager;
        }

        public void Init()
        {
            _poolList.Clear();
            _poolDic.Clear();

            ConfirmPool(1);
        }

        public void ConfirmPool(int _poolid)
        {
            if (!_poolDic.ContainsKey(_poolid))
            {
                var initPool = new RecruitPool(_poolid);
                _poolDic.Add(_poolid, initPool);
                _poolList.Add(initPool);
            }
        }

        public void UnPack(RecruitControllerInfo data)
        {
            if (data == null) return;
            foreach (var recruitPoolData in data.RecruitPoolList)
            {
                var pool = GetPool(recruitPoolData.PoolId);
                if (pool == null)
                {
                    pool = new RecruitPool(recruitPoolData.PoolId);
                    _poolDic.Add(recruitPoolData.PoolId, pool);
                    _poolList.Add(pool);
                }
                pool.UnPack(recruitPoolData);
            }
            TotalRecruitCount = data.TotalRecruitCount;
        }

        public RecruitPool GetPool(int poolId)
        {
            if (_poolDic.ContainsKey(poolId)) return _poolDic[poolId];
            return null;
        }

        private ErrorID CheckDoRecruit(int poolId, RecruitCountType recruitCountType, RecruitCostType costType)
        {
            var pool = GetPool(poolId);
            if (pool == null)
            {
                return ErrorID.SystemError;
            }

            var recruitCount = RecruitLogic.GetRecruitCount(recruitCountType);
            if (recruitCount <= 0)
            {
                return ErrorID.SystemError;
            }

            if (pool.TodayTotalCount + recruitCount > GameConst.DayMaxRecruitTimes)
            {
                return ErrorID.RecruitDayMax;
            }

            if (costType == RecruitCostType.Diamond)
            {
                int costDiamond = RecruitLogic.GetCostDiamond(recruitCountType);
                if (Player.PackageManager.Diamond < costDiamond)
                {
                    return ErrorID.DiamondNotEnough;
                }
            }
            else if (costType == RecruitCostType.Goods)
            {
                bool isActivity = pool.HasActivity();
                int goodsId = isActivity ? GoodsId.ActRecruitPoint : GoodsId.RecruitPoint;
                int costCount = recruitCount;
                if (!Player.PackageManager.IsGoodsEnough(goodsId, costCount))
                {
                    if (isActivity)
                    {
                        return ErrorID.RecruitPointNotEnoughActivity;
                    }
                    else
                    {
                        return ErrorID.RecruitPointNotEnoughNormal;
                    }
                }
            }
            else
            {
                return ErrorID.SystemError;
            }

            return ErrorID.None;
        }

        public bool DoRecruit(int poolId, RecruitCountType recruitCountType, RecruitCostType costType, Action<RecruitResponse> onRecruitSuccess)
        {
            var errorId = CheckDoRecruit(poolId, recruitCountType, costType);
            if (errorId == ErrorID.RecruitDayMax)
            {
                Tips.PopError(ErrorID.RecruitDayMax);
                //RecruitBtn.IsClick = false;
                return false;
            }
            if (errorId != ErrorID.None)
            {
                if (errorId == ErrorID.DiamondNotEnough)
                {
                    int costDiamond = RecruitLogic.GetCostDiamond(recruitCountType);
                    UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(GameItemType.Resource, ResourceId.Diamond, costDiamond));
                    //RecruitBtn.IsClick = false;
                    return false;
                }
                else
                {
                    if (errorId == ErrorID.RecruitPointNotEnoughNormal)
                    {
                        GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, GoodsId.RecruitPoint, recruitCountType == RecruitCountType.Once ? 1 : 10);
                        UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(gameItem));
                    }
                    else if (errorId == ErrorID.RecruitPointNotEnoughActivity)
                    {
                        GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, GoodsId.ActRecruitPoint, recruitCountType == RecruitCountType.Once ? 1 : 10);
                        UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(gameItem));
                    }
                    else
                    {
                        Tips.PopError(errorId);
                    }

                    //RecruitBtn.IsClick = false;
                    return false;
                }
            }
            if (isDiamondEnoughButGoodsNot)
            {
                costType = RecruitCostType.Diamond;
                isDiamondEnoughButGoodsNot = false;
            }
            NetworkManager.Instance.Recruit(poolId, recruitCountType, costType, (response) =>
            {
                if (response.Succeed)
                {
                    OnRecruitBack(response);
                    onRecruitSuccess(response);

                    if (CbaLogManager.Instance.isCbaLogEnable)
                    {
                        StringBuilder stringBuilder = new();
                        for (int i = 0; i < response.ResultList.Count; i++)
                        {
                            if (i > 0) stringBuilder.Append(",");
                            stringBuilder.Append(response.ResultList[i].Id);
                        }
                        CbaLogManager.Instance.AddLog(1006, response.PoolInfo.PoolId, response.RecruitCountType == (int)RecruitCountType.Ten ? 1 : 0, stringBuilder.ToString());
                    }
                }
                else
                {
                    //RecruitBtn.IsClick = false;
                }

            });
            return true;
        }

        private void OnRecruitBack(RecruitResponse response)
        {
            Debug.Log("OnRecruitBack");
            var pool = GetPool(response.PoolInfo.PoolId);
            pool?.UnPack(response.PoolInfo);
            EventManager.Instance.Dispatch(EventID.OnRecruitPoolRefresh);
        }

        public void DoAddAppoint(int poolId, int targetCardId, Action successCallBack)
        {
            var pool = GetPool(poolId);
            if (pool == null)
            {
                Tips.PopError(ErrorID.SystemError);
                return;
            }

            var cardConfig = Configs.CardModel.GetConfig(targetCardId);
            if (cardConfig == null)
            {
                Tips.PopError(ErrorID.SystemError);
                return;
            }

            pool.AppointCardDic.TryGetValue(cardConfig.Position, out var appointCard);
            if (appointCard.State == RecruitAppointCardState.Hit)
            {
                Tips.PopError(ErrorID.RecruitAppointAlreadyHit);
                return;
            }
            NetworkManager.Instance.ChangeAppointCard(poolId, cardConfig.Position, targetCardId, response =>
            {
                OnChangeAppointBack(response);
                successCallBack();
            });
        }
        public void DoCancelAppoint(int poolId, int index, Action<int> successCallBack)
        {
            var pool = GetPool(poolId);
            if (pool == null)
            {
                Debug.Log("NULL");
                Tips.PopError(ErrorID.SystemError);
                return;
            }
            var appointCard = pool.AppointCardDic[index];
            if (appointCard == null)
            {
                Debug.Log("NULL2");
                Tips.PopError(ErrorID.SystemError);
                return;
            }

            if (appointCard.CardId == 0)
            {
                successCallBack(index);
                return;
            }

            if (appointCard.State == RecruitAppointCardState.Hit)
            {
                Tips.PopError(ErrorID.RecruitAppointAlreadyHit);
                return;
            }

            NetworkManager.Instance.ChangeAppointCard(poolId, index, 0, response =>
            {
                OnChangeAppointBack(response);
                successCallBack(index);
            });
        }

        private void OnChangeAppointBack(ChangeAppointCardResponse response)
        {
            var pool = GetPool(response.PoolInfo.PoolId);
            if (pool == null) return;
            pool.UnPack(response.PoolInfo);
            EventManager.Instance.Dispatch(EventID.OnRecruitPoolRefresh);
        }

        public bool IsAppointSelect(int poolId, int cardId)
        {
            var pool = GetPool(poolId);
            if (pool == null) return false;
            foreach (var appointCard in pool.AppointCardList)
            {
                if (appointCard.CardId == cardId) return true;
            }

            return false;
        }

        public int GetRecruitTimes()
        {
            return TotalRecruitCount;
        }

        public void GetRecruitRewards(OptionRewardsConfig config)
        {
            NetworkManager.Instance.GetRecruitRewards(config.Id, response =>
            {
                // 购买成功
                if (response.Succeed)
                {
                    _poolDic[config.Pool].GotRewardsCount.Add(config.Id);
                    CheckRedData(config.Pool);
                    var properties = new InventoryObtainedUIProperties(config.Rewards);
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
                    EventManager.Instance.Dispatch(EventID.ClassicShopUIItemBuy, config);
                }
            });
        }

        public List<RecruitRewardItemData> GetRewardsData(int poolid)
        {
            List<RecruitRewardItemData> shopItems = new List<RecruitRewardItemData>();
            var itemConfigs = Configs.OptionRewards.GetConfigList();
            foreach (var cfg in itemConfigs)
            {
                if (cfg.Pool == poolid)
                {
                    RecruitRewardItemData data = new RecruitRewardItemData(cfg);
                    shopItems.Add(new RecruitRewardItemData(cfg));
                }
            }
            return shopItems.OrderByDescending(p => p.Status).ToList<RecruitRewardItemData>();
        }

        /// <summary>
        /// 检查小红点
        /// </summary>
        public bool CheckRedData(int poolid)
        {
            if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Recruit, false)) return false;
            var listData = GetRewardsData(poolid);
            var pool = GetPool(poolid);
            var itemConfigs = Configs.OptionRewards.GetConfigList();
            bool isred = false;
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Recruit, "/" + poolid.ToString() + "/NewRewards");
            foreach (var cfg in itemConfigs)
            {
                if (cfg.Pool == poolid)
                {
                    if (pool != null)
                    {
                        var got = pool.GotRewardsCount.Exists(itemid => itemid == cfg.Id);
                        if (!got && pool.TotalCount >= cfg.Option)
                        {
                            isred = true;
                            break;
                        }
                    }
                    else
                    {
                        //容错
                        isred = false;
                        break;
                    }
                }
            }
            node.AddValue(isred ? 1 : -1);
            return isred;
        }
    }
}