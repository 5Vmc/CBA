using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using System;
using System.Linq;
using BigBang.Animation;
using DG.Tweening;
using Utils;
using UnityTimer;
using GameConfig.Config;
using Babu;
using static BigBang.ClassicManager;
using Protocol;

namespace BigBang.UI
{
    public class HundredGuessHistoryAdapter : OSA<HundredGuessHistoryParams, HundredGuessHistoryHolder>
    {
        public SimpleDataHelper<LeagueCourseItemData> Data { get; private set; }

        private List<LeagueCourseItemData> LeagueCourseItemDataList;
        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<LeagueCourseItemData>(this);
        }

        protected override void UpdateViewsHolder(HundredGuessHistoryHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(HundredGuessHistoryHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif

        protected override void Start()
        {
            base.Start();
            SetData(LeagueCourseItemDataList);
        }
        public void SetData(List<LeagueCourseItemData> LeagueCourseItemDataList)
        {
            this.LeagueCourseItemDataList = LeagueCourseItemDataList;
            if (!IsInitialized) return;
            if (this.LeagueCourseItemDataList == null) return;
            Data.ResetItems(this.LeagueCourseItemDataList);
            if (LeagueCourseItemDataList.Count > 0)
            {
                ScrollTo(0);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override HundredGuessHistoryHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new HundredGuessHistoryHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }

        private UnityTimer.Timer nextFrameTimer = null;

        public void PlayAnim()
        {
            this.gameObject.SetAlpha(0);
            nextFrameTimer?.Cancel();
            nextFrameTimer = UnityTimer.Timer.Register(this.gameObject, 0.05f, () =>
            {
                this.InitAnimAll();
                this.gameObject.SetAlpha(1);
                this.PlayAnimAll();
            }).AddTo(this.gameObject);
        }

        private void PlayAnimAll()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                if (i < 4)
                {
                    Timer.Register(this.gameObject, i * 0.1f, () => AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP));
                }
                GetItemViewsHolder(i).anim.PlayAnim(i * 0.1f);
            }
        }

        private void InitAnimAll()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                GetItemViewsHolder(i).anim.InitAnim();
            }
        }
    }

    [Serializable]
    public class HundredGuessHistoryParams : BaseParams
    {
        public GameObject prefab;
    }

    public class HundredGuessHistoryHolder : BaseItemViewsHolder
    {
        public HundredGuessHistoryItem item;
        public AdapterItemAnim anim;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<HundredGuessHistoryItem>();
            anim = root.GetComponent<AdapterItemAnim>();
        }

        public void UpdateViews(LeagueCourseItemData data)
        {
            item.SetData(data);
            anim.InitAnim();
            anim.PlayAnim(0);
        }
    }
}