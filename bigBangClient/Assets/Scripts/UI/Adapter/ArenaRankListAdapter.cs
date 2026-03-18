using Babu;
using BigBang.UI;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using DG.Tweening;
using Protocol;
using System.Collections.Generic;
using UnityTimer;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using BigBang.Animation;
namespace BigBang.UI
{
    public class ArenaRankListAdapter :
        OSA<BaseParamsWithPrefab, ArenaRankItemViewsHolder>,
        ArenaRankItem.ISelectListener
    {

        public SimpleDataHelper<Protocol.ArenaRankInfo> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<ArenaRankInfo>(this);
        }

        public void OnSelect(ArenaRankItem item)
        {
            throw new System.NotImplementedException();
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(ArenaRankItemViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        public void SetItems(IList<ArenaRankInfo> items)
        {
            if (!IsInitialized) Init();

            Data.ResetItems(items);
            //AnimIn();
        }
        protected override ArenaRankItemViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new ArenaRankItemViewsHolder();

            instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

            //instance.ItemComponent.SelectListener = this;
            return instance;
        }

        protected override void UpdateViewsHolder(ArenaRankItemViewsHolder newOrRecycled)
        {
            var modelData = Data[newOrRecycled.ItemIndex];

            newOrRecycled.UpdateViews(modelData);

        }


        // 播放动画
        public void PlayAnim()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                if (i < 3)
                {
                    //Timer.Register(this.gameObject, i * 0.1f, () => AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP));
                }
                GetItemViewsHolder(i).PlayAnim(i * 0.1f);
            }
        }

        public void InitAnim()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                GetItemViewsHolder(i).InitAnim();
            }
        }

        public void PlayExit()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                GetItemViewsHolder(i).PlayExit();
            }
        }

    }


    public class ArenaRankItemViewsHolder : BaseItemViewsHolder
    {

        public ArenaRankItem ItemComponent;
        public override void CollectViews()
        {
            base.CollectViews();
            ItemComponent = root.GetComponent<ArenaRankItem>();
            //ItemComponent.Click += OnClick;
            //root.GetComponentAtPath("BackgroundImage", out backgroundImage);
        }

        public void UpdateViews(ArenaRankInfo data)
        {
            ItemComponent.SetData(data);
        }


        // 播放动画
        public void PlayAnim(float delay)
        {
            root.gameObject.DOFade(1, 0.3f).SetDelay(delay);
            root.DOScale(1, 0.3f).SetDelay(delay);
        }

        public void InitAnim()
        {
            root.gameObject.SetAlpha(0);
            root.localScale = Vector3.one * 0.8f;
        }

        public void PlayExit()
        {
            root.gameObject.DOFade(0, 0.3f);
        }
    }
}
