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

namespace BigBang.UI
{
    public class ClassicArenaShopItemAdapter : OSA<ClassicArenaShopItemParams, ClassicArenaShopItemHolder>
    {
        public SimpleDataHelper<ArenaShopItemData> Data { get; private set; }

        private List<ArenaShopItemData> DataSource;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<ArenaShopItemData>(this);
        }
#if UNITY_WEBGL 
        protected override bool IsRecyclable(ClassicArenaShopItemHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        protected override void UpdateViewsHolder(ClassicArenaShopItemHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }

        public void SetData(List<ArenaShopItemData> ClassicShopDataList)
        {
            DataSource = ClassicShopDataList;
            if (!IsInitialized) Init();
            Data.ResetItems(this.DataSource);
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

        protected override ClassicArenaShopItemHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new ClassicArenaShopItemHolder();
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
                GetItemViewsHolder(i).InitAnim();
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

    [Serializable]
    public class ClassicArenaShopItemParams : BaseParams
    {
        public GameObject prefab;
    }

    public class ClassicArenaShopItemHolder : BaseItemViewsHolder
    {
        private ArenaShopItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<ArenaShopItem>();
        }

        public void UpdateViews(ArenaShopItemData data)
        {
            item.SetData(data);
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
            root.gameObject.SetAlpha(0);
        }
    }
}