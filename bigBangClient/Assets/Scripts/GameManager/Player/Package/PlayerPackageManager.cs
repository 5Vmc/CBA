using System.Collections.Generic;
using Babu;
using GameConfig;
using Protocol;
using UnityEngine;
using Utils;
using System.Linq;
using Utils.GameItem;
using BigBang.UI;
using System;
using GameConfig.Config;

namespace BigBang
{
    public class PlayerPackageManager
    {
        private Resource _diamond;
        public int Diamond => _diamond.Count;

        private Resource _money;

        private Resource _energy;
        public int Money => _money.Count;
        public int Energy => _energy.Count;

        public long EnergyLastUpdateTime;

        private Dictionary<int, GoodsData> _goodsDic = new Dictionary<int, GoodsData>();
        private List<GoodsData> _goodsList = new List<GoodsData>();
        private HashSet<int> oldList = new HashSet<int>();

        // 去除New标签集合
        public HashSet<int> NewToOld = new HashSet<int>();

        public PlayerPackageManager()
        {
            _diamond = 0;
            _money = 0;
            _energy = 0;
        }

        public void Init()
        {
            _goodsDic.Clear();
            _goodsList.Clear();
            NewToOld.Clear();
        }

        public void UnPack(ModulePackageInfo data)
        {
            if (data == null) return;
            _diamond = data.Diamond;
            _money = data.Money;
            _energy = data.Energy;
            EnergyLastUpdateTime = data.EnergyLastUpdateTime;
            foreach (var goods in data.GoodsMap.Values)
            {
                if (_goodsDic.ContainsKey(goods.Id))
                {
                    _goodsDic[goods.Id].UnPack(goods);
                }
                else if (goods.Count > 0)
                {
                    AddNewGoods(goods.Id, goods.Count, goods.IsNew);
                }
                SetGoodsRedDot(goods);
            }
            EventManager.Instance.Dispatch(EventID.OnServerPushPackageChange);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        private void SetGoodsRedDot(Goods goods)
        {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Bag, "/Storage/" + goods.Id);

            var cfg = Configs.Goods.GetConfig(goods.Id);
            if (goods.Count > 0 && cfg != null && cfg.Type == 2 && cfg.Uselv <= Player.Level)
            {
                node.AddValue(1);
            }
            else
            {
                node.AddValue(-1);
            }
        }

        #region Goods

        private void AddNewGoods(int id, int count, bool isNew)
        {
            var goodsConfig = Configs.Goods.GetConfig(id);
            if (goodsConfig == null)
            {
                Debug.Log($"add new goods error goods id = {id} count = {count}");
                return;
            }
            var newGoods = new GoodsData(id, count, isNew);
            _goodsDic.Add(id, newGoods);
            _goodsList.Add(newGoods);
        }

        public IEnumerable<GoodsData> GetGoodsList()
        {
            return _goodsList.Where(item => item.Count > 0);
        }

        public int GetGoodsNumber(int id)
        {
            if (_goodsDic.ContainsKey(id))
            {
                return _goodsDic[id].Count;
            }

            return 0;
        }

        /// <summary>
        /// 判断道具是否充足，第1个不足就会停下来。
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public string IsGameItemsEnough(List<Utils.GameItem.GameItem> items, bool showtips = true, int count = 1)
        {
            string error = "";
            for (var index = 0; index < items.Count; index++)
            {
                error = IsGameItemEnough(items[index], showtips, count);
                if (error != "") return error;
            }
            return error;
        }

        /// <summary>
        /// 检查道具是否充足，会自动判断是个资源还是个道具
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string IsGameItemEnough(Utils.GameItem.GameItem item, bool showDropFrom = true, int count = 1)
        {
            switch (item.Type)
            {
                case GameItemType.Goods:
                    if (IsGoodsEnough(item.Id, item.Count * count))
                    {
                        return "";
                    }
                    else
                    {
                        if (showDropFrom)
                            UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(item.Type, item.Id, item.Count * count));
                        return "您的[" + Configs.Goods.GetConfig(item.Id).Name + "]不足";
                    }
                case GameItemType.Resource:
                    switch (item.Id)
                    {
                        case ResourceId.Money:
                            if (Money < item.Count * count)
                            {
                                if (showDropFrom)
                                    UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(item.Type, item.Id, item.Count * count));
                                return "您的金币不足";
                            }
                            else return "";
                        case ResourceId.Diamond:
                            if (Diamond < item.Count * count)
                            {
                                if (showDropFrom)
                                    UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(item.Type, item.Id, item.Count * count));
                                return "您的钻石不足";
                            }
                            else return "";
                        default: return "未知错误";
                    }
                default: return "未知错误";
            }
        }

        /// <summary>
        /// 判断体力是否足够1场战斗
        /// </summary>
        /// <returns></returns>
        public bool IsEnergyEnough()
        {
            return Energy >= GameConst.BattleEnergy;
        }

        /// <summary>
        /// 是否购买体力
        /// 1、检查当前体力
        /// 2、检查购买次数
        /// 3、检查钻石数量
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool AskBuyEnergy(Action callback = null)
        {
            if (IsEnergyEnough()) return true;

            List<string> priceList = GameConst.EnergyPrice.Split(",").ToList();
            var count = priceList.Count;
            var leftBuyTimes = count - Player.ShopManager.BuyEnergyCount;
            if (leftBuyTimes <= 0)
            {
                Tips.PopTips("体力不足且今日购买次数已达上限");
                return false;
            }

            Utils.GameItem.GameItem priceItem = GameItemUtils.CreateGameItem(GameItemType.Resource, ResourceId.Diamond, int.Parse(priceList[Player.ShopManager.BuyEnergyCount]));
            string error = IsGameItemEnough(priceItem);
            if (error != "")
            {
                //Tips.PopTips(error);
                return false;
            }
            string message = string.Format("是否消耗<color=#2A874B>{0}</color>钻石购买<color=#2A874B>{1}</color>体力？(今日还可购买<color=#2A874B>{2}</color>次)",
                priceItem.Count.ToString(), GameConst.EnergyGoodCount, leftBuyTimes);

            if (callback == null)
            {
                callback = new Action(() =>
                {
                    NetworkManager.Instance.GetEnergyRequest((resp) =>
                    {
                        if (resp.Succeed)
                        {
                            Tips.PopTips("购买成功！");

                            Player.ShopManager.BuyEnergyCount++;
                        }
                    });
                });
            }

            UIController.Instance.OpenWindow<BuySthUI>(new BuySthUIProperty(message, priceItem, callback));
            return false;
        }

        public void OnBuyEnergy()
        {

        }

        public bool IsGoodsEnough(GoodsData goods)
        {
            return IsGoodsEnough(goods.Id, goods.Count);
        }
        public bool IsGoodsEnough(int id, int num)
        {
            if (_goodsDic.ContainsKey(id))
            {
                return _goodsDic[id].IsEnough(num);
            }

            return false;
        }

        public GoodsData GetGoods(int goodsId)
        {
            if (_goodsDic.ContainsKey(goodsId)) return _goodsDic[goodsId];
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获得1个物品，如果没有，返回1个空物品
        /// </summary>
        /// <param name="goodsId"></param>
        /// <returns></returns>
        public GoodsData GetGoodsEx(int goodsId)
        {
            if (_goodsDic.ContainsKey(goodsId)) return _goodsDic[goodsId];
            else
            {
                return new GoodsData(goodsId, 0);
            }
        }

        // 更新道具信息
        public void RefreshGoods(List<Goods> dataGoods)
        {
            CbaLogManager.Instance.LogUpdateGoods(dataGoods);
            foreach (var item in dataGoods)
            {
                var goods = GetGoods(item.Id);
                if (goods == null)
                {
                    if (item.Count > 0)
                    {
                        AddNewGoods(item.Id, item.Count, item.IsNew);
                    }
                }
                else
                {
                    goods.UnPack(item);
                }
                SetGoodsRedDot(item);
            }
            EventManager.Instance.Dispatch(EventID.OnRefreshGoods);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        // 通知服务器
        public void NotifyOld()
        {
            NetworkManager.Instance.SetGoodsAsOldRequest(oldList.ToList(), response => { });
            oldList.Clear();
        }

        /// <summary>
        /// 获取卡牌经验道具的总经验
        /// </summary>
        /// <returns></returns>
        public int GetItemExp()
        {
            var _total = 0;
            GoodsData expItem;
            _goodsDic.TryGetValue(100104, out expItem);
            if (expItem != null) _total += _goodsDic[100104].Count * _goodsDic[100104].Config.Param1;
            _goodsDic.TryGetValue(100105, out expItem);
            if (expItem != null) _total += _goodsDic[100105].Count * _goodsDic[100105].Config.Param1;
            _goodsDic.TryGetValue(100106, out expItem);
            if (expItem != null) _total += _goodsDic[100106].Count * _goodsDic[100106].Config.Param1;
            _goodsDic.TryGetValue(100107, out expItem);
            if (expItem != null) _total += _goodsDic[100107].Count * _goodsDic[100107].Config.Param1;
            return _total;
        }

        #endregion

        #region resource

        public void addEnergy(int value)
        {
            _energy.Count += value;
        }
        public void setEnergy(int value)
        {
            _energy.Count = value;
        }
        public void FixEnergy()
        {
            if (Energy > GameConst.PlayerMaxEnergy) return;
            int fixEnergy = (int)((DataConvUtil.ServerTime - EnergyLastUpdateTime) / GameConst.PlayerEnergyRecoverTime);
            if (fixEnergy > 0)
            {
                int newEnergy = Energy + fixEnergy;
                newEnergy = Utility.KeepInRange(newEnergy, 0, GameConst.PlayerMaxEnergy);
                setEnergy(newEnergy);
                EnergyLastUpdateTime += GameConst.PlayerEnergyRecoverTime * fixEnergy;
                EventManager.Instance.Dispatch(EventID.OnResourceChange);
            }
        }

        public int GetResourceCount(int id)
        {
            switch (id)
            {
                case ResourceId.Diamond:
                    return Diamond;
                case ResourceId.Money:
                    return Money;
                case ResourceId.Energy:
                    return Energy;
                case ResourceId.TrainExpMin:
                    return 0;
                default:
                    return 0;
            }

        }

        public bool IsResourceEnough(int id, int num)
        {
            switch (id)
            {
                case ResourceId.Diamond:
                    return _diamond.IsEnough(num);
                case ResourceId.Money:
                    return _money.IsEnough(num);
                case ResourceId.Energy:
                    return _energy >= 5;    //5点体力打1场
                case ResourceId.TrainExpMin:
                    return Player.TrainManager.Exp >= Player.TrainManager.IncomePerSecond() * TimeUtils.Min;
                default:
                    return false;
            }
        }
        public void RefreshResource(int money, int diamond, int energy, long energyLastUpdateTime)
        {
            _money.Count = money;
            _diamond.Count = diamond;
            _energy.Count = energy;
            EnergyLastUpdateTime = energyLastUpdateTime;
            EventManager.Instance.Dispatch(EventID.OnResourceChange);
        }

        #endregion

        public List<GoodsConfig> GoodsFilter(int quality, int type)
        {
            var cardList = Player.CardManager.GetCardList();
            var result = Configs.Goods.GetConfigList().FindAll(p => p.Type == type &&
                p.Quality == quality &&
                cardList.Exists(card => card.CardId == p.Param2));
            return result;
        }

    }
}