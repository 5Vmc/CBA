using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using Babu.BigNumber;
using BigBang.UI;
using GameConfig;
using Protocol;
using UnityEngine;
using Utils;

namespace BigBang
{
    public class PlayerTrainManager : BaseManager
    {
        //经验
        public BigNumber Exp { get; set; } = 0;

        //总共获得的经验
        public BigNumber TotalExp { get; set; } = 0;

        //源力
        public BigNumber Force { get; set; } = 0;

        public double ForceAdd { get; set; } = 0;

        public long OfflineExpBeginTime { get; set; } = 0;

        public BigNumber OfflineExp { get; set; } = 0;

        /// <summary>
        /// 资源自动生产，多久才更新1次.
        /// 目前资源是每0.2秒刷新1次，这里设置75，即每15秒更新1次
        /// </summary>
        private const int refreshRedDotFrequence = 30;

        private int refreshRedDotCounter = 0;

        public LinkedList<PlayerMessage> messages = new LinkedList<PlayerMessage>();

        //训练
        private List<PlayerTrainItem> _trainList = new List<PlayerTrainItem>();

        private Dictionary<int, PlayerTrainItem> _trainDic = new Dictionary<int, PlayerTrainItem>();
        public Dictionary<int, PlayerTrainItem> TrainDic
        {
            get
            {
                return _trainDic;
            }
        }

        public TrainUpLevelType UpLevelType { get; set; } = TrainUpLevelType.UpgradeOne;

        private List<TrainEvent> _trainEvents = new List<TrainEvent>();

        public InviteMatchController InviteMatchController { get; set; }
        public BigBangController BigBangController { get; set; }

        public StrengthenController StrengthenController { get; set; }

        public PlayerTrainManager()
        {
            InviteMatchController = new InviteMatchController(this);
            BigBangController = new BigBangController(this);
            StrengthenController = new StrengthenController(this);
        }

        public void CheckRedDot()
        {
            if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.BigBang, false)) return;
            bool isred = false;
            //可解锁的小红点。
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Train, "/Regular");
            foreach (var trainItem in Player.TrainManager.TrainList())
            {
                if (trainItem.Level == 0 && Player.TrainManager.Exp > trainItem.GetUpLevelCost(1))
                {
                    isred = true;
                    break;
                }
            }
            node.AddValue(isred ? 1 : -1);

            //可升级的小红点
            StrengthenController.CheckRedDot();
            //是否能大数值升级
            BigBangController.CheckRedDot();
            //是否有邀请任务
            InviteMatchController.CheckRedDot();
        }

        public PlayerTrainItem GetTrainItem(int id)
        {
            if (_trainDic.ContainsKey(id)) return _trainDic[id];
            return null;
        }

        public int GetTrainItemLevel(int id)
        {
            var train = GetTrainItem(id);
            if (train == null) return 0;
            return train.Level;
        }

        public List<PlayerTrainItem> TrainList()
        {
            return _trainList;
        }

        public int GetUnlockCount()
        {
            return _trainList.Count(item => item.IsUnlock());
        }

        public void Init()
        {
            Exp = new BigNumber(1000);
            TotalExp = new BigNumber(1000);

            _trainDic.Clear();
            _trainList.Clear();

            foreach (var config in Configs.Train.GetConfigList())
            {
                var item = new PlayerTrainItem(config.Id);
                if (!_trainDic.ContainsKey(config.Id))
                {
                    _trainList.Add(item);
                    _trainDic.Add(config.Id, item);
                }
            }

            foreach (var breakItem in Configs.Break.GetConfigList())
            {
                var trainItem = GetTrainItem(breakItem.TrainId);
                trainItem?.AddBreakItem(breakItem);
            }

            StrengthenController.Init();
        }

        public void UnPack(TrainInfoNotify data)
        {
            Exp = data.Exp.ToBigNumber();
            TotalExp = data.TotalExp.ToBigNumber();
            Force = data.Force.ToBigNumber();
            ForceAdd = data.ForceAdd;
            UpLevelType = (TrainUpLevelType)data.UpLevelType;
            OfflineExp = data.OfflineExp.ToBigNumber();
            OfflineExpBeginTime = data.OfflineExpBeginTime;

            foreach (var trainItemData in data.TrainElements)
            {
                var item = GetTrainItem(trainItemData.Id);
                item?.UnPack(trainItemData);
            }

            BigBangController.UnPack(data.BigBang);
            InviteMatchController.UnPack(data.InviteMatch);
            StrengthenController.UnPack(data.Strengthen);
        }

        public void CheckAllIncome()
        {
            foreach (var trainItem in Player.TrainManager.TrainList())
            {
                trainItem.CheckInCome();
            }

            //大数据收入是0.2秒检测一次，这里做个计数， 每15秒更新1次小红点就可以了。
            refreshRedDotCounter++;
            if (refreshRedDotCounter >= refreshRedDotFrequence)
            {
                CheckRedDot();
                refreshRedDotCounter = 0;
                EventManager.Instance.Dispatch(EventID.RefreshBigBangUIRedDot);
            }
        }

        public void DoUpgrade(int itemId)
        {
            var item = GetTrainItem(itemId);
            if (item == null) return;
            bool isUnlockUpgrade = item.Level == 0;

            int upLevel = isUnlockUpgrade ? 1 : GetUpgradeLevel(item);
            if (upLevel == 0) return;

            CheckAllIncome();

            BigNumber cost = item.GetUpLevelCost(upLevel);
            var success = DelExp(cost);
            if (!success) return;

            AddTrainUpgradeEvent(itemId, upLevel, cost);

            item.UpLevel(upLevel);

            if (isUnlockUpgrade)
            {
                item.BeginIncome();
                if (item.ConfigId == 5) InviteMatchController.Unlock();
                //打开解锁弹窗
                // UIController.Instance.OpenWindow<UnlockTrainItemUI>(new UnlockTrainItemUIProperties(itemId));
                AddMessage(MessageType.Unlock, itemId);
            }
        }

        //判断能否升级或解锁
        public bool CanUpgrade(int itemId)
        {
            var item = GetTrainItem(itemId);
            if (item == null) return false;
            bool isUnlockUpgrade = item.Level == 0;
            int upLevel = GetUpgradeLevel(item);
            if (upLevel == 0) return false;
            BigNumber cost = item.GetUpLevelCost(upLevel);
            if (cost < 0) return false;
            if (Exp < cost) return false;
            return true;
        }

        public BigNumber IncomePerSecond()
        {
            BigNumber inCome = 0;
            foreach (var playerTrainItem in _trainList)
            {
                inCome += playerTrainItem.GetInComePerSecond();
            }

            return inCome;
        }

        public String GetInComeShowString()
        {
            BigNumber inCome = 0;
            foreach (var playerTrainItem in _trainList)
            {
                inCome += playerTrainItem.GetInComePerSecond();
            }

            return (inCome * 3600).ToFormatString() + "/" + Lang.Get(LangID.HourTxt);
        }

        //设置倍率
        public void ChangeUpLevelType()
        {
            int current = (int)UpLevelType;
            int next = (current + 1) % 4;
            UpLevelType = (TrainUpLevelType)next;
            if (UpLevelType == TrainUpLevelType.UpgradeMAX) return;
            if (UpLevelType == TrainUpLevelType.UpgradeTen)
            {
                if (!UnlockTrainUpLevelTypeTen()) ChangeUpLevelType();
            }

            if (UpLevelType == TrainUpLevelType.UpgradeHundred)
            {
                if (!UnlockTrainUpLevelTypeHundred()) ChangeUpLevelType();
            }
        }

        //是否解锁10档
        private bool _unlockTrainUpLevelTypeTen = false;

        public bool UnlockTrainUpLevelTypeTen()
        {
            if (_unlockTrainUpLevelTypeTen == false)
            {
                var item = GetTrainItem(TrainId.ManToMan);
                _unlockTrainUpLevelTypeTen = item != null && item.IsUnlock();
            }

            return _unlockTrainUpLevelTypeTen;
        }

        public bool IsUnlockAll()
        {
            foreach (var item in _trainList)
            {
                if (!item.IsUnlock()) return false;
            }
            return true;
        }

        public bool UnlockTrainUpLevelTypeHundred()
        {
            return BigBangController.BigBangTimes >= 2;
        }

        //根据倍率判断升级多少次
        public int GetUpgradeLevel(PlayerTrainItem item)
        {
            switch (UpLevelType)
            {
                case TrainUpLevelType.UpgradeOne: return 1;
                case TrainUpLevelType.UpgradeTen: return 10;
                case TrainUpLevelType.UpgradeHundred: return 100;
                case TrainUpLevelType.UpgradeMAX: return item.GetMaxLevel(Exp);
            }

            return 0;
        }

        public bool DelExp(BigNumber num)
        {
            if (num < 0) return false;
            if (Exp < num) return false;
            Exp -= num;
            EventManager.Instance.Dispatch(EventID.OnExpChanged);
            EventManager.Instance.Dispatch(EventID.RefreshBigBangUIRedDot);
            return true;
        }

        public void AddExp(double value)
        {
            AddExp(new BigNumber(value));
        }

        public void AddExp(BigNumber value)
        {
            if (value <= 0) return;
            Exp += value;
            TotalExp += value;
            EventManager.Instance.Dispatch(EventID.OnExpChanged);
        }

        public BigNumber GetMinExpReward(int min)
        {
            if (min < 0) return 0;
            var reward = min * TimeUtils.Min * IncomePerSecond();
            return reward;
        }
        public void DelMinExp(int min)
        {
            if (min < 0) return;
            var delon = min * TimeUtils.Min * IncomePerSecond();
            DelExp(delon);
        }
        public void UpdateAllTrainIncome()
        {
            foreach (var item in _trainList)
            {
                item?.UpdateIncomePerSecond();
            }
        }

        public BigNumber GetIncomeForceAdd()
        {
            if (Force == 0) return 1;
            return 1 + Force * (0.02 + ForceAdd);
        }

        public void AddForceBuffAdd(double value)
        {
            if (value <= 0) return;
            ForceAdd += value;
            UpdateAllTrainIncome();
        }

        public void AddForce(BigNumber value)
        {
            if (value <= 0) return;
            Force += value;
            UpdateAllTrainIncome();
        }

        #region TrainEvent

        public void AddTrainUpgradeEvent(int itemId, int upLevel, BigNumber cost)
        {
            var eventData = new TrainEvent();
            eventData.Event = TrainEventIds.Upgrade;
            eventData.Time = Utils.DataConvUtil.ServerTimeEx;
            eventData.Arg1 = itemId;
            eventData.Arg2 = upLevel;
            eventData.Cost = cost.ToProto();
            eventData.Exp = Player.TrainManager.Exp.ToProto();
            _trainEvents.Add(eventData);
            Debug.Log($"add upgarde event, exp = {Player.TrainManager.Exp.ToString()}");
        }

        public void AddTrainEvent(int eventId, int arg1, int arg2, long time = 0)
        {
            var eventData = new TrainEvent();
            eventData.Event = eventId;
            eventData.Time = time == 0 ? Utils.DataConvUtil.ServerTimeEx : time;
            eventData.Arg1 = arg1;
            eventData.Arg2 = arg2;
            eventData.Exp = Player.TrainManager.Exp.ToProto();
            _trainEvents.Add(eventData);
            Debug.Log($"add event = {eventId}, exp = {Player.TrainManager.Exp.ToString()}");
        }

        public void SyncTrainEvents()
        {
            if (_trainEvents.Count <= 0) return;

            Debug.Log($"SyncTrainEvents event list count = {_trainEvents.Count} ");
            int count = _trainEvents.Count;

            TrainEvent[] syncArray = new TrainEvent[count];
            lock (_trainEvents)
            {
                _trainEvents.CopyTo(0, syncArray, 0, count);
                _trainEvents.Clear();
            }

            NetworkManager.Instance.SyncTrainEvents(syncArray);
        }

        public void ShowOfflineExp()
        {
            if (OfflineExp > 0)
            {
                UIController.Instance.OpenWindow<OfflineUI>();
            }
        }

        public void DoOfflineReward(OfflineExpConfirmType type)
        {
            if (type == OfflineExpConfirmType.Video)
            {
                NetworkManager.Instance.DoOfflineReward(type, OnDoOfflineReward);
            }
            else
            {
                NetworkManager.Instance.DoOfflineReward(type, resopnse =>
                {

                    var exp = resopnse.RewardExp.ToBigNumber();
                    if (exp <= 0) return;
                    OfflineExp = 0;
                    OfflineExpBeginTime = 0;
                    AddExp(exp);
                    //UIController.Instance.OpenWindow<ExpRewardUI>(new ExpRewardProperties(exp, string.Empty));
                });
            }
        }

        private void OnDoOfflineReward(DoOfflineRewardResponse resopnse)
        {
            var exp = resopnse.RewardExp.ToBigNumber();
            if (exp <= 0) return;
            OfflineExp = 0;
            OfflineExpBeginTime = 0;
            AddExp(exp);

            UIController.Instance.OpenWindow<ExpRewardUI>(new ExpRewardProperties(exp, Lang.Get(LangID.OfflineVideoReward)));
        }

        #endregion

        #region server_notification

        #endregion


        public int GetAbilityAdd(int abilityId)
        {
            var effectItemConfig = Configs.CardBuffType.GetConfig(abilityId);
            var effectItemId = effectItemConfig.EffectTrainId;
            //队员能力提升 zmh
            var trainItem = GetTrainItem(effectItemId);
            if (trainItem == null) return 0;
            //return (int)trainItem.GetAbility();
            //突破后才能加属性           
            return (int)trainItem.TeamGetAbility();
        }

        # region 消息弹窗

        public void AddMessage(MessageType type, params object[] args)
        {
            switch (type)
            {
                case MessageType.Unlock:
                    messages.AddFirst(new PlayerMessage() { MsgType = type, Args = args });
                    break;
                case MessageType.BigBreakThrough:
                case MessageType.BreakThrough:
                case MessageType.UnlockChallenge:
                case MessageType.UnlockLeague:
                    messages.AddLast(new PlayerMessage() { MsgType = type, Args = args });
                    break;
            }
        }

        public void ShowMessage()
        {
            if (messages.Count <= 0) return;

            var msg = messages.First.Value;
            messages.RemoveFirst();
            switch (msg.MsgType)
            {
                case MessageType.Unlock:
                    UIController.Instance.OpenWindow<UnlockTrainItemUI>(new UnlockTrainItemUIProperties((int)msg.Args[0]));
                    EventManager.Instance.Dispatch(EventID.CheckGuide);
                    break;
                case MessageType.BigBreakThrough:
                    UIController.Instance.OpenWindow<BigBreakthroughUI>(new BigBreakthroughProperties((string)(msg.Args[0]), (string)(msg.Args[1]), (string)(msg.Args[2]), (string)(msg.Args[3]), (string)(msg.Args[4])));
                    EventManager.Instance.Dispatch(EventID.CheckGuide);
                    break;
                case MessageType.BreakThrough:
                    UIController.Instance.OpenWindow<BreakthroughUI>(new BreakthroughProperties((string)(msg.Args[0]), (string)(msg.Args[1]), (string)(msg.Args[2]), (string)(msg.Args[3]), (string)(msg.Args[4])));
                    EventManager.Instance.Dispatch(EventID.CheckGuide);
                    break;
                case MessageType.UnlockChallenge:
                    UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(Lang.Get(LangID.Guide7Txt), Lang.Get(LangID.ConfirmTxt), () =>
                    {
                        EventManager.Instance.Dispatch(EventID.CheckGuide);
                    }));
                    break;
                case MessageType.UnlockLeague:
                    UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(Lang.Get(LangID.Guide8Txt), Lang.Get(LangID.ConfirmTxt), () =>
                    {
                        EventManager.Instance.Dispatch(EventID.CheckGuide);
                    }));
                    break;
            }
        }

        #endregion
    }

    public class PlayerMessage
    {
        public MessageType MsgType;
        public object[] Args;
    }
}

