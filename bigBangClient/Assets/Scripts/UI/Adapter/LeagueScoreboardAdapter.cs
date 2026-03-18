using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using Protocol;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using System;
using DG.Tweening;

namespace BigBang.UI
{
    public class LeagueScoreboardAdapter : OSA<LeagueScoreboardParams, LeagueScoreboardViewsHolder>
    {
        public SimpleDataHelper<LeagueScoreboardItemModel> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<LeagueScoreboardItemModel>(this);
        }

        protected override void UpdateViewsHolder(LeagueScoreboardViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.root.localScale = Vector3.one;
            newOrRecycled.UpdateViews(model);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(LeagueScoreboardViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        public void SetData(List<LeagueScorebarTeam> data)
        {
            if (!IsInitialized) Init();
            var result = new List<LeagueScoreboardItemModel>();
            int colorIndex = 0;
            for (int i = 0; i < data.Count; i++)
            {
                result.Add(new LeagueScoreboardItemModel() { ColorIndex = colorIndex, Rank = i + 1, Data = data[i] });
                colorIndex++;
            }
            Data.ResetItems(result);
            for (int i = 0; i < Data.Count; ++i)
            {
                if (Data[i].Data.BaseData.TeamId == Player.GbId)
                {
                    ScrollTo(Mathf.Max(0, i - VisibleItemsCount / 2));
                    return;
                }
            }
        }

        protected override LeagueScoreboardViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new LeagueScoreboardViewsHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }
    }

    [Serializable]
    public class LeagueScoreboardParams : BaseParams
    {
        public GameObject prefab;
    }

    public class LeagueScoreboardItemModel
    {
        public int ColorIndex;
        // 排名
        public int Rank;
        public LeagueScorebarTeam Data;
    }

    public class LeagueScoreboardViewsHolder : BaseItemViewsHolder
    {
        private LeagueScoreboardItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<LeagueScoreboardItem>();
        }

        public void UpdateViews(LeagueScoreboardItemModel model)
        {
            item.SetData(model.Rank, model.Data);
            item.SetBackgroundColor(model.ColorIndex % 2 == 0);
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