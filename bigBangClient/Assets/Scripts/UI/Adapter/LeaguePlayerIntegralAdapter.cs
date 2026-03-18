using System.Collections.Generic;
using BigBang.Animation;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.DataHelpers;
using DG.Tweening;
using Protocol;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class LeaguePlayerIntegralAdapter : OSA<PlayerIntegralParams, LeaguePlayerIntegralViewsHolder>
    {
        public SimpleDataHelper<LeaguePlayerIntegralModel> Data { get; private set; }

        public enum ValueMode
        {
            Score,
            Assists,
            Steal,
            Rebound,

            Block,
        }

        public ValueMode mode = ValueMode.Score;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<LeaguePlayerIntegralModel>(this);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(LeaguePlayerIntegralViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        public void SetData(List<LeagueCardRankData> data)
        {
            if (!IsInitialized) Init();
            var result = new List<LeaguePlayerIntegralModel>();
            int colorIndex = 0;
            for (int i = 0; i < data.Count; i++)
            {
                result.Add(new LeaguePlayerIntegralModel() { ColorIndex = colorIndex, Rank = i + 1, Data = data[i] });
                colorIndex++;
            }
            Data.ResetItems(result);
        }

        public void InitAnim()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                GetItemViewsHolder(i).InitAnim();
            }
        }

        public void PlayAnim()
        {
            AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP);
            // 播放动画
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                GetItemViewsHolder(i).PlayAnim(i * 0.03f);
            }
        }

        //public void PlayFlash()
        //{
        //    for (int i = 0; i < VisibleItemsCount; i++)
        //    {
        //        GetItemViewsHolder(i).PlayFlash(i * 0.03f);
        //    }
        //}

        protected override LeaguePlayerIntegralViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new LeaguePlayerIntegralViewsHolder();
            instance.Init(_Params.Prefab, _Params.Content, itemIndex);
            return instance;
        }

        protected override void UpdateViewsHolder(LeaguePlayerIntegralViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.itemIndexInView];
            newOrRecycled.UpdateViews(model, mode);
        }
    }

    [System.Serializable]
    public class PlayerIntegralParams : BaseParams
    {
        public GameObject Prefab;
    }

    public class LeaguePlayerIntegralModel
    {
        public int ColorIndex;
        public int Rank = 0;
        public LeagueCardRankData Data;
    }

    public class LeaguePlayerIntegralViewsHolder : BaseItemViewsHolder
    {
        private PlayerIntegralItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<PlayerIntegralItem>();
        }

        public void UpdateViews(LeaguePlayerIntegralModel model, LeaguePlayerIntegralAdapter.ValueMode valueModel)
        {
            switch (valueModel)
            {
                case LeaguePlayerIntegralAdapter.ValueMode.Score:
                    item.SetData(model.Rank, model.Data, model.Data.Point);
                    break;
                case LeaguePlayerIntegralAdapter.ValueMode.Assists:
                    item.SetData(model.Rank, model.Data, model.Data.Assist);
                    break;
                case LeaguePlayerIntegralAdapter.ValueMode.Steal:
                    item.SetData(model.Rank, model.Data, model.Data.Steal);
                    break;
                case LeaguePlayerIntegralAdapter.ValueMode.Rebound:
                    item.SetData(model.Rank, model.Data, model.Data.Rebound);
                    break;
                case LeaguePlayerIntegralAdapter.ValueMode.Block:
                    item.SetData(model.Rank, model.Data, model.Data.Block);
                    break;
            }
            item.SetBackground(model.ColorIndex % 2 == 0);
        }

        //public void PlayFlash(float delay)
        //{
        //    item.PlayFlash(delay);
        //}

        public void PlayAnim(float delay)
        {
            root.transform.DOScale(Vector3.one, 0.15f).SetDelay(delay);
        }

        public void InitAnim()
        {
            root.transform.DOKill();
            root.transform.localScale = new Vector3(1, 0, 1);
        }
    }
}
