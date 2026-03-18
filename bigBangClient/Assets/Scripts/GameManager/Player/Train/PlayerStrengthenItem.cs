using Babu.BigNumber;
using GameConfig;
using GameConfig.Config;

namespace BigBang
{
    public class PlayerStrengthenItem
    {
        public int ConfigId { get; set; }
        public StrengthenState State { get; set; } = StrengthenState.Untrained;
        public TrainBuffBase Buff { get; set; }

        private StrengthenConfig _config;

        public PlayerStrengthenItem(int configId)
        {
            ConfigId = configId;
            _config = Configs.Strengthen.GetConfig(configId);
            Buff = TrainBuffFactory.Create(_config.BuffType, _config.BuffValue);
        }

        public StrengthenConfig GetConfig()
        {
            return _config;
        }

        public BigNumber GetCost()
        {
            if (_config == null) return 0;
            return new BigNumber(_config.CostNum, _config.CostUnit);
        }

        public void DoTrain()
        {
            Buff.Reward();
            State = StrengthenState.Trained;
        }
        
    }
}