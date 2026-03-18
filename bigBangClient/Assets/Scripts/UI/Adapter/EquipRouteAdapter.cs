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
using Protocol;

namespace BigBang.UI
{
    public class EquipRouteAdapter : OSA<EquipRouteItemParams, EquipRouteItemHolder>
    {
        public SimpleDataHelper<PassData> Data { get; private set; }

        private List<PassData> DataSource;
        /// <summary>
        /// 要扫荡的材料id
        /// </summary>
        private int itemid;
        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<PassData>(this);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(EquipRouteItemHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        protected override void UpdateViewsHolder(EquipRouteItemHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, itemid);
        }

        public void SetData(List<PassData> dataList, int _itemid = 0)
        {
            DataSource = dataList;
            itemid = _itemid;
            if (!IsInitialized) Init();
            Data.ResetItems(this.DataSource);
            PlayExit();
            PlayAnim();
        }
        public void UpdateEnergy()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                GetItemViewsHolder(i).RefreshEnergy();
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

        protected override EquipRouteItemHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new EquipRouteItemHolder();
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
    public class EquipRouteItemParams : BaseParams
    {
        public GameObject prefab;
    }

    public class EquipRouteItemHolder : BaseItemViewsHolder
    {
        private EquipRouteItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<EquipRouteItem>();
        }

        public void UpdateViews(PassData data, int _itemid = 0)
        {
            item.SetData(data, _itemid);
        }
        public void RefreshEnergy()
        {
            item.RefreshEnergy();
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