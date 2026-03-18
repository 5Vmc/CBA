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
    public class DragonBoatFestivalHomeTaskAdapter : OSA<DragonBoatFestivalHomeTaskPadParams, DragonBoatFestivalHomeTaskPadHolder>
    {
        public SimpleDataHelper<FestivalTaskInfo> Data { get; private set; }

        private List<FestivalTaskInfo> festivalTaskInfoList;
        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<FestivalTaskInfo>(this);
        }

        protected override void UpdateViewsHolder(DragonBoatFestivalHomeTaskPadHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(DragonBoatFestivalHomeTaskPadHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif

        protected override void Start()
        {
            base.Start();
            SetData(festivalTaskInfoList);
        }
        public void SetData(List<FestivalTaskInfo> festivalTaskInfoList)
        {
            this.festivalTaskInfoList = festivalTaskInfoList;
            if (!IsInitialized) return;
            if (this.festivalTaskInfoList == null) return;
            Data.ResetItems(this.festivalTaskInfoList);
            //if (festivalTaskInfoList.Count > 0)
            //{
            //    this.SetNormalizedPosition(1);
            //}
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override DragonBoatFestivalHomeTaskPadHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new DragonBoatFestivalHomeTaskPadHolder();
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
    public class DragonBoatFestivalHomeTaskPadParams : BaseParams
    {
        public GameObject prefab;
    }

    public class DragonBoatFestivalHomeTaskPadHolder : BaseItemViewsHolder
    {
        public DragonBoatFestivalHomeTaskItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<DragonBoatFestivalHomeTaskItem>();
        }

        public void UpdateViews(FestivalTaskInfo data)
        {
            item.SetData(data);
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