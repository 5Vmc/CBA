using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using Protocol;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using System;
using DG.Tweening;
using UnityTimer;
using BigBang.Animation;
using Utils;

namespace BigBang.UI
{
    public class ShootRankAdapter : OSA<ShootRankAdapterParams, ShootRankAdapterViewsHolder>
    {
        public SimpleDataHelper<AllRankInfo> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<AllRankInfo>(this);
        }

        protected override void UpdateViewsHolder(ShootRankAdapterViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.root.localScale = Vector3.one;
            newOrRecycled.UpdateViews(model, newOrRecycled.ItemIndex);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(ShootRankAdapterViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        public void SetData(List<AllRankInfo> data)
        {
            if (!IsInitialized) Init();
            Data.ResetItems(data);
            if (data.Count > 0)
            {
                this.SetNormalizedPosition(1);
            }
        }

        protected override ShootRankAdapterViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new ShootRankAdapterViewsHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }

        // 播放动画
        public void PlayAnim()
        {
            //for (int i = 0; i < VisibleItemsCount; i++)
            //{
            //    if (i < 3)
            //    {
            //        Timer.Register(this.gameObject, i * 0.1f, () => AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP));
            //    }
            //    GetItemViewsHolder(i).InitAnim();
            //    GetItemViewsHolder(i).PlayAnim(i * 0.02f);
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
    public class ShootRankAdapterParams : BaseParams
    {
        public GameObject prefab;
    }

    public class ShootRankAdapterViewsHolder : BaseItemViewsHolder
    {
        private ShootRankItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<ShootRankItem>();
        }

        public void UpdateViews(AllRankInfo model, int index)
        {
            item.SetData(model);
            if (index % 2 == 0)
            {
                item.SetBackgroundColor(new Color(0, 0, 0, 51 / 255f));
            }
            else
            {
                item.SetBackgroundColor(new Color(0, 0, 0, 13 / 255f));
            }
        }

        // 播放动画
        public void PlayAnim(float delay)
        {
            StopAnim();
            root.gameObject.DOFade(1, 0.3f).SetDelay(delay);
            root.DOScale(1, 0.3f).SetDelay(delay);
        }

        public void InitAnim()
        {
            StopAnim();
            root.gameObject.SetAlpha(0);
            root.localScale = Vector3.one * 0.8f;
        }

        public void PlayExit()
        {
            StopAnim();
            root.gameObject.SetAlpha(0);
        }

        public void StopAnim()
        {
            root.gameObject.DOKill();
            root.DOKill();
        }
    }
}