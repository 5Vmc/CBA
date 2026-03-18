using System;
using Babu;
using BigBang.UI;
using Protocol;
using UnityEngine;

namespace BigBang
{
    public class SkillTrainRoom
    {
        public int RoomId { get; set; }

        public Skill Skill { get; set; }
        public PlayerCard Card { get; set; }
        public SkillTrainRoomState State { get; set; }
        public long BeginTime { get; set; }
        public long EndTime { get; set; }

        public SkillTrainRoom(int id)
        {
            RoomId = id;
            State = SkillTrainRoomState.Lock;
            Skill = null;
            Card = null;
        }

        public void UnPack(SkillTrainroomInfo data)
        {
            Card = Player.CardManager.GetCard(data.CardId);
            if (data.CardId != 0 && Card == null)
            {
                Debug.LogWarning("SkillTrainRoom , UnPack , Card == null , data.CardId = " + data.CardId);
            }
            if (Card != null)
            {
                Card.SkillTrainBeing(RoomId);
            }
            Skill = Player.CardManager.SkillController.GetSkill(data.SkillId);
            State = (SkillTrainRoomState)data.State;
            BeginTime = data.BeginTime;
            EndTime = data.EndTime;
        }

        public int GetUnlockCostGoodsCount()
        {
            switch (RoomId)
            {
                case 1: return 0;
                case 2: return 1;
                case 3: return 3;
            }

            return 0;
        }

        public void TrainBegin()
        {
            if (Skill != null)
            {
                State = SkillTrainRoomState.Training;
                Skill.TrainBegin(RoomId);
            }
        }

        public void TrainComplete()
        {
            SkillUI.roomInfo.Add(new SkillUI.RoomInfo(Card.CardId, Skill.Id));
            State = SkillTrainRoomState.Idle;
            Skill = null;
            Card = null;
            BeginTime = 0;
            EndTime = 0;
        }

        public PlayerCard GetTrainingCard()
        {
            return Card;
        }

        public long GetTotalSecond()
        {
            return (EndTime - BeginTime) / 1000;
        }

        public long GetCdSecond()
        {
            var cd = (EndTime - Utils.DataConvUtil.ServerTimeEx) / 1000;
            if (cd < 0) cd = 0;
            return cd;
        }

        public int GetClearCdDiamond()
        {
            //10*（剩余CD时间/6） * （0.5*N）^2
            var N = Player.CardManager.SkillController.TodayClearTrainRoomCount + 1;
            var cdMin = Math.Ceiling(GetCdSecond() * 1.0f / 60.0f);
            return (int)Math.Ceiling(10.0f * cdMin / 6.0f * Math.Pow(0.5f * N, 2));
        }
    }
}