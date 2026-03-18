using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using System;
using System.Linq;
using BigBang.Animation;
using Utils;
using DG.Tweening;
using UnityTimer;

namespace BigBang.UI
{
    public class AchievementUIAdapter : OSA<AchievementUIAdapterParams, AchievementViewsHolder>
    {
        public SimpleDataHelper<AchievementGroupData> Data { get; private set; }


        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<AchievementGroupData>(this);
        }

        protected override void UpdateViewsHolder(AchievementViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }

#if UNITY_WEBGL 
        protected override bool IsRecyclable(AchievementViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        public void SetData(List<AchievementGroupData> data)
        {
            if (!IsInitialized) Init();
            Data.ResetItems(data);
        }

        protected override AchievementViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new AchievementViewsHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }

        public void PlayAnim()
        {
            this.GetComponent<DestroyTweenTrigger>()?.ClearAllTween();
            ScrollTo(0);
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                if (i < 3)
                {
                    Timer.Register(this.gameObject, i * 0.1f, () => AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP));
                }
                GetItemViewsHolder(i).PlayAnim(i * 0.05f);
            }
        }
    }

    [Serializable]
    public class AchievementUIAdapterParams : BaseParams
    {
        public GameObject prefab;
    }

    public class AchievementViewsHolder : BaseItemViewsHolder
    {
        private AchievementItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<AchievementItem>();
        }

        public void UpdateViews(AchievementGroupData data)
        {
            item.SetData(data);
        }

        public void PlayAnim(float delay)
        {
            item.gameObject.SetAlpha(0);
            item.GetComponent<RectTransform>().DORelativePositionY(-100, 0.1f).SetDelay(delay).From().AddTo(this.item.transform.parent.parent.parent.gameObject);
            item.gameObject.DOFade(1, 0.1f).SetDelay(delay).AddTo(this.item.transform.parent.parent.parent.gameObject);
        }
    }
}