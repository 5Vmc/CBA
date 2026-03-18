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
    public class ClassicShopItemAdapter : OSA<ClassicShopItemParams, ClassicShopItemHolder>
    {
        public SimpleDataHelper<ShopItemData> Data { get; private set; }

        private List<ShopItemData> DataSource;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<ShopItemData>(this);
        }

        protected override void UpdateViewsHolder(ClassicShopItemHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(ClassicShopItemHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        public void SetData(List<ShopItemData> ClassicShopDataList, int type)
        {
            DataSource = new List<ShopItemData>();
            foreach (ShopItemData item in ClassicShopDataList)
            {
                if (item.cfg.Type == type)
                {
                    DataSource.Add(item);
                }
            }
            //DataSource = (List<ShopItemData>)ClassicShopDataList.Where(p => p.cfg.Type == type);
            if (!IsInitialized) Init();
            Data.ResetItems(this.DataSource);
            PlayExit();
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

        protected override ClassicShopItemHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new ClassicShopItemHolder();
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
    public class ClassicShopItemParams : BaseParams
    {
        public GameObject prefab;
    }

    public class ClassicShopItemHolder : BaseItemViewsHolder
    {
        private ShopItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<ShopItem>();
        }

        public void UpdateViews(ShopItemData data)
        {
            item.SetData(data);
        }

        // 播放动画
        public void PlayAnim(float delay)
        {
            root.gameObject.DOKill();
            root.gameObject.DOFade(1, 0.3f).SetDelay(delay);
            root.DOKill();
            root.DOScale(1, 0.3f).SetDelay(delay);
        }

        public void InitAnim()
        {
            root.gameObject.DOKill();
            root.gameObject.SetAlpha(0);
            root.DOKill();
            root.localScale = Vector3.one * 0.8f;
        }

        public void PlayExit()
        {
            root.DOKill();
            root.gameObject.SetAlpha(0);
        }
    }
}