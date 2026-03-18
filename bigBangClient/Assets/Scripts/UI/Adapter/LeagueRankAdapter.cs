using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using Protocol;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using System;
using DG.Tweening;

namespace BigBang.UI
{
    public class LeagueRankAdapter : OSA<LeagueRankParams, LeagueRankViewsHolder>
    {
        public SimpleDataHelper<ChampionTeamData> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<ChampionTeamData>(this);
        }

        protected override void UpdateViewsHolder(LeagueRankViewsHolder newOrRecycled)
        {
            newOrRecycled.UpdateViews(Data[newOrRecycled.ItemIndex], newOrRecycled.ItemIndex);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(LeagueRankViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        public void SetData(List<ChampionTeamData> data)
        {
            if (!IsInitialized) Init();
            Data.ResetItems(data);
            for (int i = 0; i < Data.Count; ++i)
            {
                if (data[i].Team.TeamId == Player.GbId)
                {
                    ScrollTo(Mathf.Max(0, i - VisibleItemsCount / 2));
                    return;
                }
            }
        }

        protected override LeagueRankViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new LeagueRankViewsHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }
    }

    [Serializable]
    public class LeagueRankParams : BaseParams
    {
        public GameObject prefab;
    }

    public class LeagueRankViewsHolder : BaseItemViewsHolder
    {
        private LeagueRankItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<LeagueRankItem>();
        }

        public void UpdateViews(ChampionTeamData championTeamData, int index)
        {
            item.SetData(championTeamData, false, index);
        }

        public void InitAnim()
        {
            root.transform.localScale = new Vector3(1, 0, 1);
        }

        public Tweener PlayAnim(float delay)
        {
            return root.transform.DOScale(Vector3.one, 0.15f).SetDelay(delay);
        }
    }
}