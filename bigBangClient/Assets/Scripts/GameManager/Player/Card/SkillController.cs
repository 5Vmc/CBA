using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Protocol;
using UnityEngine;
using Utils;

namespace BigBang
{
    public class SkillController
    {
        private PlayerCardManager _cardManager;

        private List<Skill> _skillList = new List<Skill>();
        private Dictionary<int, Skill> _skillDic = new Dictionary<int, Skill>();

        private Dictionary<int, SkillTrainRoom> _trainRoomDic = new Dictionary<int, SkillTrainRoom>();

        public int TodayClearTrainRoomCount { get; set; }

        // 解锁成功回调
        private Action DoUnlockSkillSucceed;

        public SkillController(PlayerCardManager cardManager)
        {
            _cardManager = cardManager;
        }

        public void Init()
        {
            _skillList.Clear();
            _skillDic.Clear();
            var configList = Configs.Skill.GetConfigList();
            foreach (var skillConfig in configList)
            {
                var skill = new Skill(skillConfig.Id, 0);
                _skillList.Add(skill);
                _skillDic.Add(skill.Id, skill);
            }

            _trainRoomDic.Clear();
            for (int i = 1; i <= 3; i++)
            {
                _trainRoomDic.Add(i, new SkillTrainRoom(i));
            }
        }

        public void UnPack(SkillControllerInfo data)
        {
            if (data == null) return;
            foreach (var item in data.UnlockSkillIds)
            {
                var skill = GetSkill(item.Key);
                if (skill != null && item.Value == 1)
                {
                    skill.Unlock = true;
                }
            }

            foreach (var trainRoomData in data.TrainRooms.Values)
            {
                var room = GetTrainRoom(trainRoomData.RoomId);
                room?.UnPack(trainRoomData);
                room?.TrainBegin();
            }

            TodayClearTrainRoomCount = data.TodayClearTrainRoomCount;
        }

        public void CheckRedDot()
        {
            if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.CardSkill, false)) return;

            foreach (var item in _skillDic.Values)
            {
                RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_SkillTrain, "/UnlockSkill/" + item.Config.Id);
                if (item.Unlock == false)
                {
                    SkillState state = item.GetSkillState();
                    if(state == SkillState.ConditionsMetLock)
                    {
                        node.AddValue(1);
                        continue;
                    }
                }
                node.AddValue(-1);
            }

            
            foreach (var item in _trainRoomDic.Values)
            {
                RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_SkillTrain, "/TrainRoom/" + item.RoomId);
                if (item.State == SkillTrainRoomState.Lock)
                {
                    var costCount = item.GetUnlockCostGoodsCount();
                    if (Player.PackageManager.IsGoodsEnough(GoodsId.TrainRoomUnlockGoods, costCount))
                    {
                        node.AddValue(1);
                        continue;
                    }
                }
                if(item.State == SkillTrainRoomState.Training)
                {
                    bool isEnd = (item.EndTime <= Utils.DataConvUtil.ServerTimeEx);
                    if(isEnd)
                    {
                        node.AddValue(1);
                        continue;
                    }
                }
                node.AddValue(-1);
            }
        }

        public SkillTrainRoom GetTrainRoom(int roomId)
        {
            if (_trainRoomDic.ContainsKey(roomId)) return _trainRoomDic[roomId];
            return null;
        }

        public Skill GetSkill(int id)
        {
            if (_skillDic.ContainsKey(id)) return _skillDic[id];
            return null;
        }

        public void DoUnlockSkill(int skillId, Action succeed, Action faild)
        {
            var skill = GetSkill(skillId);
            if (skill == null)
            {
                Tips.PopError(ErrorID.SystemError);
                faild?.Invoke();
                return;
            }

            if (skill.Unlock)
            {
                Tips.PopError(ErrorID.UnLockSkillRepeat);
                faild?.Invoke();
                return;
            }

            if (!skill.IsPlayerCanUnlock())
            {
                Tips.PopError(ErrorID.UnLockSkillConditionsNotMet);
                faild?.Invoke();
                return;
            }

            if (!skill.IsPlayerMoneyEnough())
            {
                Tips.PopError(ErrorID.MoneyNotEnough);
                faild?.Invoke();
                return;
            }
            // 设置解锁成功回调
            DoUnlockSkillSucceed = succeed;
            NetworkManager.Instance.UnlockSkill(skillId, response =>
            {
                OnUnlockSkillBack(response);
            });
        }

        private void OnUnlockSkillBack(UnlockSkillResponse response)
        {
            var skill = GetSkill(response.SkillId);
            if (skill == null) return;

            skill.Unlock = true;

            CheckRedDot();
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);

            // 执行解锁成功回调
            DoUnlockSkillSucceed?.Invoke();
            DoUnlockSkillSucceed = null;
        }

        public List<Skill> GetSkillList()
        {
            return _skillList;
        }

        public List<Skill> GetCanUnlockSkillList()
        {
            List<Skill> list = new List<Skill>();
            foreach (var skill in _skillList)
            {
                if (skill.Unlock == true) continue;
                if (!skill.IsPlayerCanUnlock()) continue;
                if (!skill.IsPlayerMoneyEnough()) continue;
                list.Add(skill);
            }

            return list;
        }

        public bool IsPlayerCardTraining(int cardId)
        {
            for (int i = 1; i <= 3; i++)
            {
                var room = GetTrainRoom(i);
                if (room.Card != null && room.Card.CardId == cardId) return true;
            }
            return false;
        }

        public bool IsSkillTraining(int skillId)
        {
            var skill = GetSkill(skillId);
            if (skill == null) return false;
            return skill.TrainingRoomId != 0;
        }

        //获取可以训练的技能
        //如果不传入cardid，就是所有的卡
        public List<SkillConfig> GetCanTrainSkillList(int cardId = 0)
        {
            var card = Player.CardManager.GetCard(cardId);

            List<SkillConfig> list = new List<SkillConfig>();
            foreach (var skill in _skillList)
            {
                if (skill.Unlock == false) continue;
                if (skill.GetSkillState() != SkillState.UnlockNoTraining) continue;
                if (card != null && !card.CanTrainSkill(skill.Id)) continue;
                list.Add(skill.Config);
            }

            return list;
        }

        public List<Skill> GetUnlockSkillList()
        {
            List<Skill> list = new List<Skill>();
            foreach (var skill in _skillList)
            {
                if (skill.Unlock == false) continue;
                list.Add(skill);
            }

            return list;
        }

        public List<PlayerCard> GetCanTrainPlayerCardList(int skillId = 0)
        {
            List<PlayerCard> list = new List<PlayerCard>();
            foreach (var card in Player.CardManager.CardList)
            {
                if (IsPlayerCardTraining(card.CardId)) continue;
                if (skillId != 0 && !card.CanTrainSkill(skillId)) continue;
                list.Add(card);
            }
            return list;
        }

        public void UnlockSkillTrainRoom(int roomId)
        {
            var room = GetTrainRoom(roomId);
            Debug.Log("11111  " + room);
            if (room == null)
            {
                Tips.PopError(ErrorID.SystemError);
                return;
            }

            if (room.State != SkillTrainRoomState.Lock)
            {
                Tips.PopError(ErrorID.SystemError);
                return;
            }

            var costCount = room.GetUnlockCostGoodsCount();
            if (!Player.PackageManager.IsGoodsEnough(GoodsId.TrainRoomUnlockGoods, costCount))
            {
                UIController.Instance.OpenWindow<SupplementUI>(new SupplementUIProperties(GameItemType.Goods, GoodsId.TrainRoomUnlockGoods, costCount));
                return;
            }

            NetworkManager.Instance.UnlockSkillTrainRoom(roomId, OnUnlockSkillTrainRoom);
        }

        private void OnUnlockSkillTrainRoom(UnlockSkillTrainRoomResponse response)
        {
            var room = GetTrainRoom(response.TrainRoom.RoomId);
            room.UnPack(response.TrainRoom);

            EventManager.Instance.Dispatch(EventID.OnUnlockSkillTrainRoom, room);
            CheckRedDot();
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        public void BeginTrainSkill(int roomId, int cardId, int skillId, Action onBeginTrainSkillSuccess)
        {
            var room = GetTrainRoom(roomId);
            if (room.State != SkillTrainRoomState.Idle)
            {
                Tips.PopError(ErrorID.SystemError);
                return;
            }
            NetworkManager.Instance.BeginTrainSkill(roomId, cardId, skillId, response =>
            {
                // var room = GetTrainRoom(response.TrainRoom.RoomId);
                room.UnPack(response.TrainRoom);

                room.TrainBegin();
                onBeginTrainSkillSuccess();
            });
        }

        public void OnTrainRoomComplete(int roomId, int skillId, int cardId)
        {
            var room = GetTrainRoom(roomId);
            room.TrainComplete();

            var skill = GetSkill(skillId);
            skill.TrainComplete();

            var card = Player.CardManager.GetCard(cardId);
            card.SkillTrainComplete(skill.Id);
        }

        public void ClearTrainRoomCD(int roomId)
        {
            NetworkManager.Instance.ClearTrainRoomCD(roomId, OnClearTrainRoomCD);
        }

        public bool CanClearTrainRoomCD(int roomID)
        {
            var room = GetTrainRoom(roomID);
            if (room == null) return false;
            int costDiamond = room.GetClearCdDiamond();
            return Player.PackageManager.IsResourceEnough(ResourceId.Diamond, costDiamond);
        }

        private void OnClearTrainRoomCD(ClearTrainRoomCDResponse response)
        {
            TodayClearTrainRoomCount = response.TodayClearTrainRoomCount;
            CheckRedDot();
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        public SkillTrainRoom GetIdleRoom()
        {
            foreach (var room in _trainRoomDic.Values)
            {
                if (room.State == SkillTrainRoomState.Idle)
                {
                    return room;
                }
            }

            return null;
        }

        //获取球员的特技学习情况
        public SkillTrainSelectCardState GetSelectCardShowState(int cardId, int skillId)
        {
            var card = Player.CardManager.GetCard(cardId);
            if (card == null) return SkillTrainSelectCardState.Normal;

            var isTraining = IsPlayerCardTraining(cardId);
            if (isTraining)
            {
                return SkillTrainSelectCardState.DoTraining;
            }

            var skill = GetSkill(skillId);
            if (skill == null) return SkillTrainSelectCardState.Normal;

            if (card.HaveSkill(skillId))
            {
                return SkillTrainSelectCardState.HaveBeenTrain;
            }

            if (!card.CanTrainSkill(skillId))
            {
                return SkillTrainSelectCardState.CanNotTrain;
            }

            return SkillTrainSelectCardState.Normal;
        }

        public SkillTrainSelectSkillState GetSelectSkillShowState(int skillId, int cardId)
        {
            var skill = GetSkill(skillId);
            if (skill == null) return SkillTrainSelectSkillState.Normal;
            var isTraining = IsSkillTraining(skillId);
            if (isTraining)
            {
                return SkillTrainSelectSkillState.DoTraining;
            }

            var card = Player.CardManager.GetCard(cardId);
            if (card == null) return SkillTrainSelectSkillState.Normal;
            if (card.HaveSkill(skillId))
            {
                return SkillTrainSelectSkillState.HaveBeenTrain;
            }

            if (!card.CanTrainSkill(skillId))
            {
                return SkillTrainSelectSkillState.CanNotTrain;
            }

            return SkillTrainSelectSkillState.Normal;
        }
    }
}