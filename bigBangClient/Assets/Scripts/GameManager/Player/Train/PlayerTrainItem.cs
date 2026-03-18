using System;
using System.Collections.Generic;
using Babu;
using Babu.BigNumber;
using GameConfig;
using GameConfig.Config;
using Protocol;
using UnityEngine;
using Utils;

namespace BigBang
{
    public class PlayerTrainItem
    {
        public int ConfigId { get; set; }

        public TrainConfig _config;

        private int _level = 0;
        private int _rewardLevel = 0;

        private BigNumber _incomeAdd = 1;
        private BigNumber _timeReduce = 1;
        private BigNumber _consumeReduce = 1;

        //上一次收益时间毫秒级
        public long LastIncomeTimeStamp { get; set; }

        private BigNumber _incomePerSecond = new BigNumber();

        public int BreakIndex { get; set; } = 0;
        private List<BreakConfig> _breakConfigs = new List<BreakConfig>();
        private Dictionary<int, BreakConfig> _breakConfigMap = new Dictionary<int, BreakConfig>();
        private Dictionary<int, BreakConfig> _breakTargetConfigMap = new Dictionary<int, BreakConfig>();
        public int Level
        {
            get { return _level + _rewardLevel; }
        }

        public PlayerTrainItem(int configId)
        {
            ConfigId = configId;
            _config = Configs.Train.GetConfig(ConfigId);
        }

        public void UnPack(TrainElementInfo trainItemData)
        {
            _level = trainItemData.Level;
            _rewardLevel = trainItemData.RewardLevel;
            _incomeAdd = trainItemData.IncomeAdd.ToBigNumber();
            _timeReduce = trainItemData.TimeReduce.ToBigNumber();
            _consumeReduce = trainItemData.ConsumeReduce.ToBigNumber();
            LastIncomeTimeStamp = trainItemData.LastIncomeTime;
            BreakIndex = trainItemData.BreakIndex;
        }

        public TrainConfig GetConfig()
        {
            return _config;
            // return Configs.Train.GetConfig(ConfigId);
        }

        public BigNumber GetUpLevelCost(int n)
        {
            var cfg = GetConfig();
            BigNumber cost;

            if (cfg.CostAdd.CompareTo(1) == 0)
            {
                cost = cfg.BaseCost * BigNumberMath.Pow(Math.Round(cfg.CostAdd, 2), Level) * n;
            }
            else
            {
                cost = cfg.BaseCost * BigNumberMath.Pow(Math.Round(cfg.CostAdd, 2), Level) * (1 - BigNumberMath.Pow(Math.Round(cfg.CostAdd, 2), n)) /
                       (1 - cfg.CostAdd);
            }

            // var ret = cost * _consumeReduce;
            // Debug.Log($"item id = {ConfigId},{cfg.BaseCost.ToString()}, {BigNumberMath.Pow(Math.Round(cfg.CostAdd,2), Level).ToString()}, {(1 - BigNumberMath.Pow(Math.Round(cfg.CostAdd,2), n)).ToString()}, {(1 - Math.Round(cfg.CostAdd,2))}, {_consumeReduce}, {ret}");
            return cost * _consumeReduce;
        }


        public int GetMaxLevel(BigNumber total)
        {
            int left = 1, right = 5000;

            if (total >= GetUpLevelCost(right)) return right;
            if (total < GetUpLevelCost(left)) return 0;

            while (left <= right)
            {
                int mid = (right + left) / 2;
                BigNumber cost = GetUpLevelCost(mid);
                if (total == cost)
                {
                    return mid;
                }
                else if (cost < total)
                {
                    left = mid + 1;
                }
                else if (cost > total)
                {
                    right = mid - 1;
                }
            }

            return left - 1;
        }

        public void UpLevel(int upLevel)
        {

            _upLevel(upLevel, 0);
            UpdateIncomePerSecond();
        }

        private void _upLevel(int upLevel, int upRewardLevel)
        {
            _level += upLevel;
            _rewardLevel += upRewardLevel;

            CheckBreak();
        }

        public void UpdateIncomePerSecond(int level = 0)
        {
            if (level == 0)
            {
                level = Level;
            }
            var cfg = GetConfig();
            _incomePerSecond = cfg.BaseAdd * level * _incomeAdd * Player.TrainManager.GetIncomeForceAdd() /
                               GetInComeTimeUnit();
            // Debug.Log("update income per second = " + _incomePerSecond.ToString());
        }

        //换算出的每秒产出
        public BigNumber GetInComePerSecond(int level = 0)
        {
            if (level == 0)
            {
                level = Level;
            }
            if (Level == 0) return 0;
            if (_incomePerSecond == 0)
            {
                UpdateIncomePerSecond(level);
            }

            return _incomePerSecond;
        }

        //产出的单位时间 单位s
        public BigNumber GetInComeTimeUnit()
        {
            var cfg = GetConfig();
            if (cfg == null) return 1;
            return cfg.BaseTime / _timeReduce;
        }

        public double GetAbility(int level = 0)
        {
            if (level == 0)
            {
                level = Level;
            }
            var inComePreSecond = GetInComePerSecond(level);
            return 0.75 * BigNumberMath.Log10(inComePreSecond);
        }

        //突破后队员能力提升
        public double TeamGetAbility()
        {
            //改为 当前突破加成-上次突破加成
            //突破节点 breakConfig当前节点 
            if (BreakIndex != 0)
            {
                var teamPreSecondCurrent = 0;
                for (int i = 1; i <= BreakIndex; i++)
                {
                    var breakConfig = _breakConfigMap.ContainsKey(i) ? _breakConfigMap[i] : null;
                    teamPreSecondCurrent += (int)breakConfig.CardBuffValue;
                }
                return Math.Floor((double)(teamPreSecondCurrent));
            }
            else
            {
                return 0;
            }

        }

        //项目属性加成
        public double ItemGetAbility()
        {
            var currentTotalAddAbility = 0;
            if (BreakIndex == 0)
            {
                currentTotalAddAbility = 0;
            }
            else
            {
                var breakConfig = _breakConfigMap.ContainsKey(BreakIndex) ? _breakConfigMap[BreakIndex] : null;
                currentTotalAddAbility = (int)breakConfig.CardTotalValue;
            }
            return currentTotalAddAbility;
        }

        //是否解锁项目，解锁true 
        public bool IsUnlock()
        {
            return Level > 0;
        }
        //是否进行第一次突破
        public bool IsFirstBreak()
        {
            return BreakIndex >= 1;
        }

        public void RewardInCome(double value)
        {
            if (value <= 0) return;
            _incomeAdd *= value;

            UpdateIncomePerSecond();
        }

        public void RewardLevel(int addLevel)
        {

            if (addLevel < 0) return;
            _upLevel(0, addLevel);

            UpdateIncomePerSecond();
        }

        public void RewardTimeReduce(double value)
        {
            if (value <= 0) return;
            _timeReduce *= value;

            UpdateIncomePerSecond();
        }

        public void RewardConsumeReduce(double value)
        {
            if (value <= 0) return;
            _consumeReduce *= (1 - value);
        }

        public void CheckInCome()
        {
            if (!IsUnlock()) return;
            long now = Utils.DataConvUtil.ServerTimeEx;
            double timeDiff = (now - LastIncomeTimeStamp) / 1000f;
            if (timeDiff >= GameConst.AddExpMinUnitSecond && timeDiff >= GetInComeTimeUnit())
            {
                AddExpInUnit(timeDiff);
            }
        }

        /**
         * timeDiff 单位s
         */
        private void AddExpInUnit(double timeDiff)
        {
            double unitSecond = GetInComeTimeUnit() > GameConst.AddExpMinUnitSecond
                ? GetInComeTimeUnit().ToDouble()
                : GameConst.AddExpMinUnitSecond;
            int addTimes = (int)(timeDiff / unitSecond);
            if (addTimes <= 0) return;
            var addExp = addTimes * GetInComePerSecond() * unitSecond;
            Player.TrainManager.AddExp(addExp);
            LastIncomeTimeStamp += (long)(addTimes * unitSecond * 1000f);

            // if (ConfigId == 1)
            // {
            //     var pastTime = (Utils.DataConvUtil.ServerTimeEx - LastIncomeTimeStamp) / 1000.0f;
            //     Debug.LogWarningFormat("trainItem.LastIncomeTimeStamp = {0}", LastIncomeTimeStamp);
            //     Debug.LogWarningFormat("------- timeDiff = {0} , unitSecond = {1} , addTimes = {2} , pastTime = {3} , time = {4} ", timeDiff, unitSecond, addTimes, pastTime, Time.time);
            // }
        }

        //获得突破进度[0-1]
        public float GetBreakThroughProgress()
        {
            var breakConfig = _breakConfigMap.ContainsKey(BreakIndex) ? _breakConfigMap[BreakIndex] : null;
            var nextBreakConfig = _breakConfigMap.ContainsKey(BreakIndex + 1) ? _breakConfigMap[BreakIndex + 1] : null;
            var targetLevel = nextBreakConfig?.TargetLevel ?? Level;
            var beginLevel = breakConfig?.TargetLevel ?? 0;

            return (float)(Level - beginLevel) / (float)(targetLevel - beginLevel);
        }

        // public void CheckInCome()
        // {
        //     if (!IsUnlock()) return;
        //     long now = TimeUtils.NowEx();
        //     long timeDiff = now - _lastIncomeTimeStamp;
        //     var incomeTimeUnit = GetInComeTimeUnit();
        //     if (incomeTimeUnit < GameConst.AddExpMinUnitSecond)
        //     {
        //         AddExpInMinUnit(timeDiff);
        //     }
        //     else
        //     {
        //         if (timeDiff >= incomeTimeUnit)
        //         {
        //             AddExpOneUnit(timeDiff);
        //         }
        //     }
        // }
        //
        // private void AddExpOneUnit(long timeDiff)
        // {
        //     var timeUnit = (long) (GetInComeTimeUnit().ToDouble() * 1000);
        //     int addTimes = (int)(timeDiff / timeUnit);
        //     if (addTimes <= 0) return;
        //     Player.TrainManager.AddExp(addTimes * GetInComeTimeUnit());
        //     _lastIncomeTimeStamp += addTimes * timeUnit;
        // }
        //
        // private void AddExpInMinUnit(long timeDiff)
        // {
        //     int addTimes = (int)(timeDiff / GameConst.AddExpMinUnitMircoSecond);
        //     if (addTimes <= 0) return;
        //     Player.TrainManager.AddExp(addTimes * GetInComePerSecond() * GameConst.AddExpMinUnitSecond); ;
        //     _lastIncomeTimeStamp += addTimes * GameConst.AddExpMinUnitMircoSecond;
        //
        //     Debug.Log($"income item id = {ConfigId} , timediff = {timeDiff} add times = {addTimes} , Exp = {Player.TrainManager.Exp.ToFormatString()}");
        // }

        public void BeginIncome()
        {
            LastIncomeTimeStamp = Utils.DataConvUtil.ServerTimeEx;
        }

        public void AddBreakItem(BreakConfig config)
        {
            _breakConfigs.Add(config);
            _breakConfigMap.Add(config.BreakIndex, config);
            _breakTargetConfigMap.Add(config.TargetLevel, config);
        }

        private void CheckBreak()
        {
            int targetIndex = BreakIndex;
            BuffTypeConfig cfg = null;
            CardBuffTypeConfig cardcfg = null;
            float buffValue = 0;
            float cardBuffValue = 0;
            int targetLevel = 0;
            foreach (var breakConfig in _breakConfigs)
            {
                if (breakConfig.BreakIndex <= BreakIndex) continue;
                if (breakConfig.TargetLevel > Level) break;
                targetIndex = breakConfig.BreakIndex;
                var buff = TrainBuffFactory.Create(breakConfig.BuffType, breakConfig.BuffValue);
                buff?.Reward();
                //zmh
                Player.TrainManager.AddTrainEvent(TrainEventIds.Break, ConfigId, breakConfig.Id, 0);

                cfg = Configs.BuffType.GetConfig(breakConfig.BuffType);
                cardcfg = Configs.CardBuffType.GetConfig(breakConfig.CardBuffType);

                buffValue += breakConfig.BuffValue;
                targetLevel = breakConfig.TargetLevel;

                cardBuffValue += breakConfig.CardBuffValue;
                //Player.CalFightPoint(true);
                //Player.TrainManager.AddMessage(MessageType.BigBreakThrough, cfg.Desc, cfg.DescOperator + breakConfig.BuffValue.ToString(), breakConfig.TargetLevel.ToString(), cardcfg.DescOperator + breakConfig.CardBuffValue.ToString(), cardcfg.Desc);
            }
            BreakIndex = targetIndex;
            if (cfg != null)
            {
                Player.TrainManager.AddMessage(MessageType.BigBreakThrough, cfg.Desc, cfg.DescOperator + buffValue.ToString(), targetLevel.ToString(), cardcfg.DescOperator + cardBuffValue.ToString(), cardcfg.Desc);

            }
        }

        public void DevelopSetLevel(int level)
        {
            if (Level == level) return;
            _level = level;
            _rewardLevel = 0;
            UpdateIncomePerSecond();
        }

        //四个按钮的突破等级
        public List<int> SetBreakLevelData()
        {
            List<int> intsList = new List<int>();
            intsList.Clear();
            var n = -1;
            for (int i = 0; i < 4; i++)
            {
                var breakConfig = _breakConfigMap.ContainsKey(BreakIndex + 1 + i) ? _breakConfigMap[BreakIndex + 1 + i] : null;
                var targetLevel = breakConfig?.TargetLevel ?? Level;
                if (targetLevel == Level)
                {
                    n = i;
                    break;
                }
            }
            if (n != -1)
            {
                for (int i = (3 - n); i >= 0; i--)
                {
                    var breakConfig = _breakConfigMap.ContainsKey(BreakIndex - 1 - i) ? _breakConfigMap[BreakIndex - 1 - i] : null;
                    var targetLevel = breakConfig?.TargetLevel ?? Level;
                    intsList.Add(targetLevel);
                }
            }
            else
            {
                for (var i = 0; i < 4; i++)
                {
                    var breakConfig = _breakConfigMap.ContainsKey(BreakIndex + 1 + i) ? _breakConfigMap[BreakIndex + 1 + i] : null;
                    var targetLevel = breakConfig?.TargetLevel ?? Level;
                    intsList.Add(targetLevel);
                }
            }
            return intsList;
        }

        public BuffTypeConfig SetBuffTypeData(int listElement)
        {
            var breakConfig = _breakTargetConfigMap.ContainsKey(listElement) ? _breakTargetConfigMap[listElement] : null;
            var bufftype = breakConfig.BuffType;
            var cfg = Configs.BuffType.GetConfig(bufftype);
            return cfg;
        }
        public BreakConfig SetBuffData(int listElement)
        {
            var breakConfig = _breakTargetConfigMap.ContainsKey(listElement) ? _breakTargetConfigMap[listElement] : null;
            return breakConfig;
        }

        public CardBuffTypeConfig SetCardBuffData(int listElement)
        {
            var breakConfig = _breakTargetConfigMap.ContainsKey(listElement) ? _breakTargetConfigMap[listElement] : null;
            var cardbufftype = breakConfig.CardBuffType;
            var cardcfg = Configs.CardBuffType.GetConfig(cardbufftype);
            return cardcfg;
        }
    }
}