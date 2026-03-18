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
    public class HonourLineItemData
    {
        public enum HonourLineType
        {
            Top = 1,
            Mid = 2,
            Bottom = 3,
        }
        public HonourLineType type = HonourLineType.Mid;
        public List<HonourGroupData> honourGroupDataList = new();
    }
    public class HonourAdapter : OSA<HonourParams, BaseHonourViewsHolder>
    {
        public SimpleDataHelper<HonourLineItemData> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<HonourLineItemData>(this);
        }

        protected override void UpdateViewsHolder(BaseHonourViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, newOrRecycled.ItemIndex);
        }
        protected override bool IsRecyclable(BaseHonourViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.CanPresentModelType(Data[indexOfItemThatWillBecomeVisible].type);
        }
        private List<HonourLineItemData> honourLineItemDataList = new();
        public void SetData(List<HonourLineItemData> honourLineItemDataList)
        {
            this.honourLineItemDataList = honourLineItemDataList;
            if (!IsInitialized) Init();
            Data.ResetItems(this.honourLineItemDataList);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void CollectItemsSizes(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, ItemsDescriptor itemsDesc)
        {
            base.CollectItemsSizes(changeMode, count, indexIfInsertingOrRemoving, itemsDesc);
            if (changeMode != ItemCountChangeMode.RESET)
                return;

            if (count == 0)
                return;

            int indexOfFirstItemThatWillChangeSize = 0;
            int end = indexOfFirstItemThatWillChangeSize + count;

            itemsDesc.BeginChangingItemsSizes(indexOfFirstItemThatWillChangeSize);
            for (int i = indexOfFirstItemThatWillChangeSize; i < end; ++i)
            {
                var model = Data[i];
                switch (model.type)
                {
                    case HonourLineItemData.HonourLineType.Top:
                        itemsDesc[i] = 152;
                        break;
                    case HonourLineItemData.HonourLineType.Mid:
                        itemsDesc[i] = 266;
                        break;
                    case HonourLineItemData.HonourLineType.Bottom:
                        itemsDesc[i] = 523;
                        break;
                    default:
                        itemsDesc[i] = 2;
                        break;
                }
            }
            itemsDesc.EndChangingItemsSizes();
        }

        protected override BaseHonourViewsHolder CreateViewsHolder(int itemIndex)
        {
            var model = Data[itemIndex];
            switch (model.type)
            {
                case HonourLineItemData.HonourLineType.Top:
                    {
                        var instance = new HonourHolderTop();
                        instance.Init(_Params.prefabTop, _Params.Content, itemIndex);
                        return instance;
                    }
                case HonourLineItemData.HonourLineType.Mid:
                    {
                        var instance = new HonourHolderMid();
                        instance.Init(_Params.prefabMid, _Params.Content, itemIndex);
                        return instance;
                    }
                case HonourLineItemData.HonourLineType.Bottom:
                    {
                        var instance = new HonourHolderBottom();
                        instance.Init(_Params.prefabBottom, _Params.Content, itemIndex);
                        return instance;
                    }
            }
            return null;
        }
    }

    [Serializable]
    public class HonourParams : BaseParams
    {
        public GameObject prefabTop;
        public GameObject prefabMid;
        public GameObject prefabBottom;
    }

    public abstract class BaseHonourViewsHolder : BaseItemViewsHolder
    {
        public abstract bool CanPresentModelType(HonourLineItemData.HonourLineType lineType);
        public abstract void UpdateViews(HonourLineItemData lineData, int index);
    }

    public class HonourHolderTop : BaseHonourViewsHolder
    {
        private HonourLineItemData lineData;
        // private MyCoursePadItem item;
        private int index = 0;

        public override void CollectViews()
        {
            base.CollectViews();
            // item = root.GetComponent<MyCoursePadItem>();
        }
        public override void UpdateViews(HonourLineItemData lineData, int index)
        {
            this.lineData = lineData;
            this.index = index;
            // item.SetData(model.Data, model.LeagueName, battleEnterType);
        }

        public override bool CanPresentModelType(HonourLineItemData.HonourLineType lineType)
        {
            return lineData.type == lineType;
        }
    }
    public class HonourHolderMid : BaseHonourViewsHolder
    {
        private HonourLineItemData lineData;
        private HonourLineItem item;
        private int index = 0;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<HonourLineItem>();
        }
        public override void UpdateViews(HonourLineItemData lineData, int index)
        {
            this.lineData = lineData;
            this.index = index;
            item.SetData(lineData, index);
        }

        public override bool CanPresentModelType(HonourLineItemData.HonourLineType lineType)
        {
            return lineData.type == lineType;
        }
    }
    public class HonourHolderBottom : BaseHonourViewsHolder
    {
        private HonourLineItemData lineData;
        // private MyCoursePadItem item;
        private int index = 0;

        public override void CollectViews()
        {
            base.CollectViews();
            // item = root.GetComponent<MyCoursePadItem>();
        }
        public override void UpdateViews(HonourLineItemData lineData, int index)
        {
            this.lineData = lineData;
            this.index = index;
            // item.SetData(model.Data, model.LeagueName, battleEnterType);
        }

        public override bool CanPresentModelType(HonourLineItemData.HonourLineType lineType)
        {
            return lineData.type == lineType;
        }
    }

}