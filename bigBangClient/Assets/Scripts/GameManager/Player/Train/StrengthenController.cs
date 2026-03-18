using System.Collections.Generic;
using Babu;
using Babu.BigNumber;
using GameConfig;
using Protocol;
using UnityEngine;

namespace BigBang
{
    public class StrengthenController
    {
        public StrengthenController()
        {
        }

        public StrengthenController(PlayerTrainManager trainManager)
        {
            _trainManager = trainManager;
        }

        private PlayerTrainManager _trainManager;

        //强化
        private List<PlayerStrengthenItem> _strengthenList = new List<PlayerStrengthenItem>();
        private Dictionary<int, PlayerStrengthenItem> _strengthenDic = new Dictionary<int, PlayerStrengthenItem>();

        public void Init()
        {
            _strengthenDic.Clear();
            _strengthenList.Clear();

            foreach (var config in Configs.Strengthen.GetConfigList())
            {
                var item = new PlayerStrengthenItem(config.Id);
                if (!_strengthenDic.ContainsKey(config.Id))
                {
                    _strengthenList.Add(item);
                    _strengthenDic.Add(config.Id, item);
                }
            }
        }

        public void CheckRedDot() {
            bool isred = false;
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Train, "/Strength");
            List<PlayerStrengthenItem> list = Player.TrainManager.StrengthenController.GetShowList();
            foreach (var item in list) {
                if (Player.TrainManager.Exp >= item.GetCost()) {
                    isred = true;
                    break;
                }
            }
            node.AddValue(isred ? 1 : -1);
        }

        public void UnPack(StrengthenControllerInfo data)
        {
            foreach (var id in data.TrainedList)
            {
                var item = GetStrengthenItem(id);
                if (item == null) continue;
                item.State = StrengthenState.Trained;
            }
        }

        public PlayerStrengthenItem GetStrengthenItem(int id)
        {
            if (_strengthenDic.ContainsKey(id)) return _strengthenDic[id];
            return null;
        }

        public List<PlayerStrengthenItem> GetShowList()
        {
            const int maxCount = 10;
            List<PlayerStrengthenItem> list = new List<PlayerStrengthenItem>();
            foreach (var item in _strengthenList)
            {
                if (item.State == StrengthenState.Trained) continue;
                int effectTrainId = item.Buff.GetEffectTrainId();
                if (effectTrainId != 99)
                {
                    var trainItem = _trainManager.GetTrainItem(effectTrainId);
                    if (trainItem == null || !trainItem.IsUnlock()) continue;
                }
                else
                {
                    if (!_trainManager.IsUnlockAll()) continue;
                }
                list.Add(item);
                if (list.Count > maxCount) break;
            }

            return list;
        }

        const int MaxCount = 100;//最大强化的数量
        public List<PlayerStrengthenItem> GetAllStrengthenList()
        {
            //所有可以强化的表
            //BigNumber expSum = _trainManager.TotalExp.Clone();
            BigNumber expSum = _trainManager.Exp.Clone();
            List<PlayerStrengthenItem> allStrengthenListlist = new List<PlayerStrengthenItem>();
            foreach (var item in _strengthenList)
            {
                if (item.State == StrengthenState.Trained) continue;
                int effectTrainId = item.Buff.GetEffectTrainId();
                if (effectTrainId != 99)
                {
                    var trainItem = _trainManager.GetTrainItem(effectTrainId);
                    if (trainItem == null || !trainItem.IsUnlock()) continue;
                }
                else
                {
                    if (!_trainManager.IsUnlockAll()) continue;
                }
                //if (!((ExpSum = ExpSum - item.GetCost()) >= 0))
                //    continue;
                //AllStrengthenListlist.Add(item);
                if ((expSum = expSum - item.GetCost()) > 0)
                {
                    allStrengthenListlist.Add(item);
                }


                if (allStrengthenListlist.Count >= MaxCount) break;
            }
            return allStrengthenListlist;
        }



        //强化
        public bool DoStrengthen(int id)
        {
            var item = GetStrengthenItem(id);
            if (item?.GetConfig() == null) return false;
            var cost = item.GetCost();
            var success = _trainManager.DelExp(cost);
            if (success)
            {
                _trainManager.CheckAllIncome();
                item.DoTrain();
                _trainManager.AddTrainEvent(TrainEventIds.Strengthen, item.ConfigId, 0);
            }
            return success;
        }

        //是否可以强化
        public bool CanStrengthen(int id)
        {
            var item = GetStrengthenItem(id);
            if (item?.GetConfig() == null) return false;
            var cost = item.GetCost();
            if (cost < 0) return false;
            if (_trainManager.Exp < cost) return false;
            return true;
        }

        //批量强化
        public List<int> DoStrengthenBatch()
        {
            var list = Player.TrainManager.StrengthenController.GetAllStrengthenList();
            _trainManager.CheckAllIncome();
            List<int> strengthenIdList = new List<int>();
            foreach (var item in list)
            {
                var cost = item.GetCost();
                var success = _trainManager.DelExp(cost);
                if (success)
                {
                    item.DoTrain();
                    _trainManager.AddTrainEvent(TrainEventIds.Strengthen, item.ConfigId, 0);
                    strengthenIdList.Add(item.ConfigId);
                }
            }
            return strengthenIdList;
        }

        public bool UnlockStrengthenBatch()//是否解锁一键强化
        {
            if (_trainManager.TotalExp > BigNumberMath.Pow(10, 21)) return true;
            int totalBreak = 0;
            foreach (var trainItem in _trainManager.TrainList())
            {
                totalBreak += trainItem.BreakIndex;
                if (totalBreak >= 3) return true;
            }

            return false;
        }

        //是否可以一键强化
        public bool CanDoStrengthenBatch()
        {
            foreach (var item in _strengthenList)
            {
                if (item.State == StrengthenState.Trained)
                {
                    continue;
                }

                int effectTrainId = item.Buff.GetEffectTrainId();
                if (effectTrainId != 99)
                {
                    var trainItem = _trainManager.GetTrainItem(effectTrainId);
                    if (trainItem == null || !trainItem.IsUnlock()) continue;
                }
                else
                {
                    if (!_trainManager.IsUnlockAll()) continue;
                }

                if (item.GetCost() < _trainManager.Exp)
                {
                    return true;
                }

            }
            return false;
        }
    }
}