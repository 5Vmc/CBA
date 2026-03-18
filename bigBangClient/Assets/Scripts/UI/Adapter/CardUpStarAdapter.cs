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
using Utils.GameItem;
using GameConfig;

namespace BigBang.UI
{

    public class CardUpStarAdapter : OSA<CardUpStarItemParams, CardUpStarItemHolder>
    {
        public SimpleDataHelper<CardUpStarItemData1> Data { get; private set; }

        private List<CardUpStarItemData1> DataSource;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<CardUpStarItemData1>(this);
        }
#if UNITY_WEBGL 
        protected override bool IsRecyclable(CardUpStarItemHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        protected override void UpdateViewsHolder(CardUpStarItemHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, newOrRecycled.ItemIndex);
        }

        public void SetData(List<CardUpStarItemData1> dataList)
        {
            DataSource = dataList;
            if (!IsInitialized) Init();
            Data.ResetItems(this.DataSource);
            //InitAnim();
            PlayAnim();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override CardUpStarItemHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new CardUpStarItemHolder();
            instance.Init(_Params.prefab, _Params.Content, itemIndex);
            return instance;
        }

        // 播放动画
        public void PlayAnim()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                if (i < 3)
                {
                    Timer.Register(this.gameObject, i * 0.1f, () => AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP));
                }
                GetItemViewsHolder(i).PlayAnim(i * 0.1f);
            }
        }

        //public void InitAnim()
        //{
        //    for (int i = 0; i < VisibleItemsCount; i++)
        //    {
        //        GetItemViewsHolder(i).InitAnim();
        //    }
        //}

        //public void PlayExit()
        //{
        //    for (int i = 0; i < VisibleItemsCount; i++)
        //    {
        //        GetItemViewsHolder(i).PlayExit();
        //    }
        //}
    }

    [Serializable]
    public class CardUpStarItemParams : BaseParams
    {
        public GameObject prefab;
    }

    public class CardUpStarItemHolder : BaseItemViewsHolder
    {
        private CardUpStarItem1 item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<CardUpStarItem1>();
        }

        public void UpdateViews(CardUpStarItemData1 data, int index)
        {
            item.SetData(data, index);
        }

        // 播放动画
        public void PlayAnim(float delay)
        {
            root.DOKill();
            root.gameObject.DOKill();

            root.gameObject.SetAlpha(0);
            root.localScale = Vector3.one * 0.8f;

            root.gameObject.DOFade(1, 0.3f).SetDelay(delay);
            root.DOScale(1, 0.3f).SetDelay(delay);
        }

        //public void InitAnim()
        //{
        //    root.DOKill();
        //    root.gameObject.DOKill();
        //    root.gameObject.SetAlpha(0);
        //    root.localScale = Vector3.one * 0.8f;
        //}

        //public void PlayExit()
        //{
        //    root.gameObject.DOKill();
        //    root.gameObject.SetAlpha(0);
        //}
    }
}