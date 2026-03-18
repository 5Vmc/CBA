using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using System;
using GameConfig;

namespace BigBang.UI
{
    public class LeagueRewardsUIAdapter : OSA<LeagueRewardsParams, LeagueRewardsViewsHolder>
    {
        public SimpleDataHelper<LeagueRewardsItemModel> Data { get; private set; }

        private const int BASE_REWRD_INDEX = 5; //前5条不是等级奖励

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<LeagueRewardsItemModel>(this);
        }

        protected override void UpdateViewsHolder(LeagueRewardsViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(LeagueRewardsViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        // flag为true显示球队排名奖励；为false显示其他奖励
        public void SetData(bool flag, int compitionID, int level)
        {
            if (!IsInitialized) Init();
            switch (compitionID)
            {
                case CompitionID.League:
                    SetLeageReward(flag, level);
                    break;
                case CompitionID.Cup:
                    SetCupReward(flag, level);
                    break;
            }
        }

        private void SetLeageReward(bool isRankReward, int level)
        {
            List<LeagueRewardsItemModel> result = new List<LeagueRewardsItemModel>();
            if (isRankReward)
            {
                Configs.LeagueRewardRank.GetConfigList().ForEach(item =>
                {
                    if (item.Level == level)
                        result.Add(new LeagueRewardsItemModel(true, item.Id, CompitionID.League));
                });
            }
            else
            {
                Configs.LeagueRewardOther.GetConfigList().ForEach(item =>
                {
                    if(item.Id <= BASE_REWRD_INDEX)
                    {
                        result.Add(new LeagueRewardsItemModel(false, item.Id, CompitionID.League));
                    }
                    else
                    {
                        if(item.Id - BASE_REWRD_INDEX == level)
                        {
                            result.Add(new LeagueRewardsItemModel(false, item.Id, CompitionID.League));
                        }
                    }
                });
            }
            Data.ResetItems(result);
        }

        private void SetCupReward(bool flag, int level)
        {
            List<LeagueRewardsItemModel> result = new List<LeagueRewardsItemModel>();
            if (flag)
            {
                Configs.CupRewardRank.GetConfigList().ForEach(item =>
                {
                    if (item.Level == level)
                    {
                        result.Add(new LeagueRewardsItemModel(true, item.Id, CompitionID.Cup));
                    }
                });
            }
            else
            {
                Configs.CupRewardOther.GetConfigList().ForEach(item =>
                {
                    if(item.Id <= BASE_REWRD_INDEX)
                    {
                        result.Add(new LeagueRewardsItemModel(false, item.Id, CompitionID.Cup));
                    }
                    else
                    {
                        if(item.Id - BASE_REWRD_INDEX == level)
                        {
                            result.Add(new LeagueRewardsItemModel(false, item.Id, CompitionID.Cup));
                        }
                    }
                });
            }
            Data.ResetItems(result);
        }

        protected override LeagueRewardsViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new LeagueRewardsViewsHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }
    }

    [Serializable]
    public class LeagueRewardsParams : BaseParams
    {
        public GameObject prefab;
    }

    public class LeagueRewardsItemModel
    {
        public bool RankReward;     // true显示排名奖，false显示其他奖
        public int RewardID;
        public int CompetitionID;

        public LeagueRewardsItemModel(bool rankReward, int rewardID, int compitionID)
        {
            RankReward = rankReward;
            RewardID = rewardID;
            CompetitionID = compitionID;
        }
    }

    public class LeagueRewardsViewsHolder : BaseItemViewsHolder
    {
        private LeagueRewardsItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<LeagueRewardsItem>();
        }

        public void UpdateViews(LeagueRewardsItemModel model)
        {
            item.SetData(model.RankReward, model.RewardID, model.CompetitionID);
        }
    }
}