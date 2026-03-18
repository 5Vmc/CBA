using GameConfig;
using GameConfig.Config;
using System.Threading.Tasks;
using UnityEngine;

namespace BigBang
{
    public class Skill
    {
        public int Id { get; set; }

        private int _level;
        public int Level {
            get{ return _level; }
            private set{}
        }

        public void LevelUpgrade()
        {
            this._level += 1;
        }
        //是否解锁，未解锁 false ， 已解锁 true
        public bool Unlock { get; set; }
        public SkillConfig Config { get; set; }

        public int TrainingRoomId { get; set; } = 0;

        public Skill(int id, int level)
        {
            Id = id;
            this._level = level;
            Unlock = false;
            Config = Configs.Skill.GetConfig(id);
        }

        public Task<Sprite> GetIcon()
        {
            if (Config == null) return null;
            return SpriteProxy.GetSkillIcon(Config.Icon);
        }

        public bool IsPlayerCanUnlock()
        {
            if (Config == null) return false;
            foreach (var item in Config.UnlockConditions)
            {
                var train = Player.TrainManager.GetTrainItem(item.Key);
                if (train == null) return false;
                if (train.Level < item.Value) return false;
            }

            return true;
        }

        public bool IsPlayerMoneyEnough()
        {
            if (!Player.PackageManager.IsResourceEnough(ResourceId.Money, Config.UnlockMoney))
            {
                
                return false;
            }

            return true;
        }

        public SkillState GetSkillState()
        {
            if (!Unlock)
            {
                if (IsPlayerCanUnlock() && IsPlayerMoneyEnough())
                {
                    return SkillState.ConditionsMetLock;
                }
                else
                {
                    return SkillState.ConditionsNotMet;
                }
            }
            else
            {
                if (TrainingRoomId != 0)
                {
                    return SkillState.UnlockTraining;
                }
                else
                {
                    return SkillState.UnlockNoTraining;
                }
            }
        }

        public void TrainComplete()
        {
            TrainingRoomId = 0;
        }

        public void TrainBegin(int roomId)
        {
            TrainingRoomId = roomId;
        }

        public PlayerCard GetTrainingCard()
        {
            var room = Player.CardManager.SkillController.GetTrainRoom(TrainingRoomId);

            return room?.GetTrainingCard();
        }

        public int GetEffectaddValue()
        {
            if (Config == null) return 0;
            return Config.EffectaddValue[Level-1];
        }

        public int GetNextEffectaddValue()
        {
            if (Config == null) return 0;
            return Config.EffectaddValue[Level];
        }
    }
}