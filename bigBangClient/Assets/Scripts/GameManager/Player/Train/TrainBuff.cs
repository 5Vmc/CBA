using GameConfig;
using GameConfig.Config;

namespace BigBang
{
    public abstract class TrainBuffBase
    {
        private int _configId;

        public int ConfigId => _configId;

        public int BuffType => _configId;

        protected double _value;
        
        public TrainBuffBase(int configId, double value)
        {
            _configId = configId;
            _value = value;
        }

        public BuffTypeConfig GetConfig()
        {
            return Configs.BuffType.GetConfig(_configId);
        }

        public abstract void Reward();

        //获取作用到的trainid
        public int GetEffectTrainId()
        {
            return GetConfig().EffectTrainId;
        }
    }

    //收益增加
    public class TrainBuffInCome : TrainBuffBase
    {
        public TrainBuffInCome(int configId, double value) : base(configId, value)
        {
        }

        public override void Reward()
        {
            var effectId = GetEffectTrainId();
            if (effectId == TrainId.All) RewardAll();
            else RewardOne(effectId);
        }

        private void RewardOne(int effectId)
        {
            var item = Player.TrainManager.GetTrainItem(effectId);
            item?.RewardInCome(_value);
        }

        private void RewardAll()
        {
            foreach (var item in Player.TrainManager.TrainList())
            {
                item?.RewardInCome(_value);
            }
        }
    }

    //经验增加
    public class TrainBuffExp : TrainBuffBase
    {
        public TrainBuffExp(int configId, double value) : base(configId, value)
        {
        }

        public override void Reward()
        {
            Player.TrainManager.AddExp(_value);
        }
    }

    //May the force be with you
    //源力
    public class TrainBuffForce : TrainBuffBase
    {
        public TrainBuffForce(int configId, double value) : base(configId, value)
        {
        }

        public override void Reward()
        {
            Player.TrainManager.AddForceBuffAdd(_value);
        }
    }


    //训练等级
    public class TrainBuffLevel : TrainBuffBase
    {
        public TrainBuffLevel(int configId, double value) : base(configId, value)
        {
        }

        public override void Reward()
        {
            var effectId = GetEffectTrainId();
            if (effectId == TrainId.All) RewardAll();
            else RewardOne(effectId);
        }

        private void RewardOne(int effectId)
        {
            var item = Player.TrainManager.GetTrainItem(effectId);
            item?.RewardLevel((int)_value);
        }

        private void RewardAll()
        {
            foreach (var item in Player.TrainManager.TrainList())
            {
                item?.RewardLevel((int)_value);
            }
        }
    }

    //消耗降低
    public class TrainBuffConsumeReduce : TrainBuffBase
    {
        public TrainBuffConsumeReduce(int configId, double value) : base(configId, value)
        {
        }

        public override void Reward()
        {
            var effectId = GetEffectTrainId();
            if (effectId == TrainId.All) RewardAll();
            else RewardOne(effectId);
        }

        private void RewardOne(int effectId)
        {
            var item = Player.TrainManager.GetTrainItem(effectId);
            item?.RewardConsumeReduce(_value);
        }

        private void RewardAll()
        {
            foreach (var item in Player.TrainManager.TrainList())
            {
                item?.RewardConsumeReduce(_value);
            }
        }
    }

    //速度
    public class TrainBuffTimeReduce : TrainBuffBase
    {
        public TrainBuffTimeReduce(int configId, double value) : base(configId, value)
        {
        }

        public override void Reward()
        {
            var effectId = GetEffectTrainId();
            if (effectId == TrainId.All) RewardAll();
            else RewardOne(effectId);
        }

        private void RewardOne(int effectId)
        {
            var item = Player.TrainManager.GetTrainItem(effectId);
            item?.RewardTimeReduce(_value);
        }

        private void RewardAll()
        {
            foreach (var item in Player.TrainManager.TrainList())
            {
                item?.RewardTimeReduce(_value);
            }
        }
    }


    public static class TrainBuffFactory
    {
        private static class BuffType
        {
            public const int InCome = 0;
            public const int Other = 1;
            public const int Level = 2;
            public const int ConsumeReduce = 3;
            public const int TimeReduce = 4;
        }
        
        public static TrainBuffBase Create(int id, double value)
        {
            switch (id / 100)
            {
                case BuffType.InCome:
                    return new TrainBuffInCome(id, value);
                case BuffType.Other when id == 100:
                    return new TrainBuffForce(id, value);
                case BuffType.Other when id == 101:
                    return new TrainBuffExp(id, value);
                case BuffType.Level:
                    return new TrainBuffLevel(id, value);
                case BuffType.ConsumeReduce:
                    return new TrainBuffConsumeReduce(id, value);
                case BuffType.TimeReduce:
                    return new TrainBuffTimeReduce(id, value);
                default:
                    return null;
            }
        }
    }
}