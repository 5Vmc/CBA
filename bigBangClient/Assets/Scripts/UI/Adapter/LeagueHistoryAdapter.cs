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
    public class LeagueHistoryAdapter : OSA<HistoryParams, LeagueHistoryViewsHolder>
    {
        public SimpleDataHelper<LeagueHistoryData> Data { get; private set; }

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
            Data = new SimpleDataHelper<LeagueHistoryData>(this);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(LeagueHistoryViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        public void SetData(List<LeagueHistoryData> dataList)
        {
            if (!IsInitialized) Init();
            Data.ResetItems(dataList);
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

        protected override LeagueHistoryViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new LeagueHistoryViewsHolder();
            instance.Init(_Params.Prefab, _Params.Content, itemIndex);
            return instance;
        }

        protected override void UpdateViewsHolder(LeagueHistoryViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.itemIndexInView];
            newOrRecycled.UpdateViews(model);
        }
    }

    [System.Serializable]
    public class HistoryParams : BaseParams
    {
        public GameObject Prefab;
    }

    public class LeagueHistoryViewsHolder : BaseItemViewsHolder
    {
        private LeagueHistoryItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<LeagueHistoryItem>();
        }

        public void UpdateViews(LeagueHistoryData data)
        {
            item.SetData(data);
        }

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
