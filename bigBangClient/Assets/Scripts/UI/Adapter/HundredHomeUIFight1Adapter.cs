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
    public class HundredHomeUIFight1Adapter : OSA<HundredHomeUIFight1Params, HundredHomeUIFight1Holder>
    {
        public SimpleDataHelper<LeagueCourseItemData> Data { get; private set; }

        private List<LeagueCourseItemData> LeagueCourseItemDataList;
        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<LeagueCourseItemData>(this);
        }

        protected override void UpdateViewsHolder(HundredHomeUIFight1Holder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(HundredHomeUIFight1Holder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
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

        protected override HundredHomeUIFight1Holder CreateViewsHolder(int itemIndex)
        {
            var instance = new HundredHomeUIFight1Holder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }

        // 播放动画
        public void PlayAnim()
        {
            //for (int i = 0; i < VisibleItemsCount; i++)
            //{
            //    if (i < 4)
            //    {
            //        Timer.Register(this.gameObject, i * 0.1f, () => AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP));
            //    }
            //    GetItemViewsHolder(i).PlayAnim(i * 0.1f);
            //}
        }

        public void InitAnim()
        {
            //for (int i = 0; i < VisibleItemsCount; i++)
            //{
            //    GetItemViewsHolder(i).InitAnim();
            //}
        }

        public void PlayExit()
        {
            //for (int i = 0; i < VisibleItemsCount; i++)
            //{
            //    GetItemViewsHolder(i).PlayExit();
            //}
        }
    }

    [Serializable]
    public class HundredHomeUIFight1Params : BaseParams
    {
        public GameObject prefab;
    }

    public class HundredHomeUIFight1Holder : BaseItemViewsHolder
    {
        public HundredHomeUIFight1Item item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<HundredHomeUIFight1Item>();
        }

        public void UpdateViews(LeagueCourseItemData data)
        {
            item.SetData(data, true);
        }

        Tween fadeTween;
        Tween scaleTween;
        // 播放动画
        public void PlayAnim(float delay)
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
            fadeTween = root.gameObject.DOFade(1, 0.3f).SetDelay(delay);
            scaleTween = root.DOScale(1, 0.3f).SetDelay(delay);
        }

        public void InitAnim()
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
            root.gameObject.SetAlpha(0);
            root.localScale = Vector3.one * 0.8f;
        }

        public void PlayExit()
        {
            fadeTween?.Kill();
            scaleTween?.Kill();
            fadeTween = root.gameObject.DOFade(0, 0.3f);
        }
    }
}