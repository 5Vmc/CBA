using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameConfig;
using GameConfig.Config;
using Protocol;

namespace BigBang
{
    public class RecruitPool
    {
        public int PoolId { get; set; }
        public int ContinueCount { get; set; }
        public int TodayTotalCount { get; set; }

        public List<int> GotRewardsCount { get; set; }
        public int TotalCount { get; set; }

        public List<AppointCard> AppointCardList { get; set; } = new List<AppointCard>();
        public Dictionary<int, AppointCard> AppointCardDic { get; set; } = new Dictionary<int, AppointCard>();

        public CardPoolInfoConfig Config { get; set; }

        private Dictionary<int, List<CardModelConfig>> _poolQualityDic = new Dictionary<int, List<CardModelConfig>>();
        private List<CardModelConfig> _poolList = new List<CardModelConfig>();
        private Dictionary<int, int> _poolQualityWeightDic = new Dictionary<int, int>();
        private int _poolTotalWeight = 0;
        public RecruitPool(int poolId)
        {
            PoolId = poolId;
            ContinueCount = 0;
            TodayTotalCount = 0;
            TotalCount = 0;
            GotRewardsCount = new List<int>();
            for (int i = 1; i <= 3; i++)
            {
                var ken = new AppointCard(i);
                AppointCardList.Add(ken);
                AppointCardDic.Add(i, ken);
            }
            Config = Configs.CardPoolInfo.GetConfig(PoolId);

            InitConfig();
        }

        public void UnPack(RecruitPoolInfo data)
        {
            ContinueCount = data.ContinueCount;
            TodayTotalCount = data.TodayCount;
            TotalCount = data.TotalRecruitCount;
            GotRewardsCount = data.Rewards.ToList();
            foreach (var appointCardData in data.AppointCardList)
            {
                var ken = AppointCardDic[appointCardData.Index];
                ken?.UnPack(appointCardData);
            }
        }
        private void InitConfig()
        {
            _poolQualityDic.Clear();
            //_poolQualityWeightDic.Clear();
            _poolList.Clear();

            for (int i = QualityType.Green; i <= QualityType.Red; i++)
            {
                _poolQualityDic.Add(i, new List<CardModelConfig>());
                //_poolQualityWeightDic.Add(i, 0);
            }

            var _poolgroup = Configs.CardPoolGroup.GetConfigList().FindAll(P => P.PoolInfo == PoolId);

            var poolConfigList = Configs.CardPool.GetConfigList().FindAll(P => _poolgroup.Exists(P1 => P1.PoolId == P.PoolId));

            _poolTotalWeight = 0;
            foreach (var config in poolConfigList)
            {
                //if (config.PoolId == PoolId)
                //{
                var cardConfig = Configs.CardModel.GetConfig(config.CardId);
                if (cardConfig == null) continue;
                var quality = cardConfig.Quality;
                _poolTotalWeight += config.Weight;
                //if (_poolQualityWeightDic.ContainsKey(quality)) _poolQualityWeightDic[quality] += config.Weight;

                if (_poolQualityDic.ContainsKey(quality))
                {
                    _poolQualityDic[quality].Add(cardConfig);
                    _poolList.Add(cardConfig);
                }

                //}
            }
            _poolList.Sort((a, b) => { return -a.Quality.CompareTo(b.Quality); });
        }

        public List<int> GetQualityCardIdList(int quality)
        {
            List<int> list = new List<int>();
            foreach (var config in _poolQualityDic[quality])
            {
                if (config.Quality == quality)
                {
                    list.Add(config.Id);
                }
            }
            list = list.Distinct().ToList();
            return list;
        }

        /// <summary>
        /// 是否有活动
        /// 抽卡按钮样式根据是否有活动来设置
        /// </summary>
        /// <returns>有活动返回true，无活动返回false</returns>
        public bool HasActivity()
        {
            return PoolId != 1;
        }

        public double GetQualityRatio(int quality)
        {
            //return _poolQualityWeightDic[quality] / (_poolTotalWeight * 1.0);
            double rate;
            switch (quality)
            {
                case 1: rate = 0.7; break;
                case 2: rate = 0.25; break;
                case 3: rate = 0.04; break;
                case 4: rate = 0.01; break;
                case 5: rate = 0; break;
                default: rate = 0.10; break;
            }
            return rate;
        }

        public List<CardModelConfig> GetPoolCardList(int quality = QualityType.All, int position = (int)PositionType.All)
        {
            List<CardModelConfig> selectQualityList;
            List<CardModelConfig> retList = new List<CardModelConfig>();
            if (quality != -1)
            {
                if (quality == QualityType.All) selectQualityList = _poolList;
                else selectQualityList = _poolQualityDic[quality];


                foreach (var config in selectQualityList)
                {
                    if (position == (int)PositionType.All || config.Position == position)
                    {
                        if (!retList.Exists(p => p.Id == config.Id))
                            retList.Add(config);
                    }
                }

            }
            else
            {
                //未开放的球员列表
                var exceptList = Configs.CardModel.GetConfigList().Except(_poolList).ToList();
                foreach (var config in exceptList)
                {
                    if (position == (int)PositionType.All || config.Position == position)
                    {
                        if (!retList.Exists(p => p.Id == config.Id))
                            retList.Add(config);
                    }
                }
            }
            return retList;




        }
    }
}