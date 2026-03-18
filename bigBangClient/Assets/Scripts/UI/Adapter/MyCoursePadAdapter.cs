using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using System;
using Com.TheFallenGames.OSA.Core;
using System.Collections.Generic;
using Protocol;
using System.Linq;
using Babu;
using Utils;
using DG.Tweening;
using static BigBang.BattleManager;

namespace BigBang.UI
{
    public class MyCoursePadAdapter : OSA<MyCourseParams, BaseMyCourseViewsHolder>
    {
        public SimpleDataHelper<MyCourseItemModel> Data { get; private set; }

        public event Action<LeagueCourseItemData, BattleEnterType> OnItemClick;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<MyCourseItemModel>(this);
        }

        protected override void UpdateViewsHolder(BaseMyCourseViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, battleEnterType);
        }

        protected override BaseMyCourseViewsHolder CreateViewsHolder(int itemIndex)
        {
            var model = Data[itemIndex];
            if (model.PrefabType == MyCourseParams.PrefabType.DateItem)
            {
                var instance = new DateMyCourseViewHolder();
                instance.Init(_Params.MyCourseDateItem, _Params.Content, itemIndex);
                return instance;
            }
            if (model.PrefabType == MyCourseParams.PrefabType.CourseItem)
            {
                var instance = new MyCourseViewHolder();
                instance.Init(_Params.MyCourseItem, _Params.Content, itemIndex);
                return instance;
            }
            return null;
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
                if (model.PrefabType == MyCourseParams.PrefabType.DateItem)
                {
                    itemsDesc[i] = 58;
                }
                else
                {
                    itemsDesc[i] = 100;
                }
            }
            itemsDesc.EndChangingItemsSizes();
        }

        private BattleEnterType battleEnterType;
        public void SetData(GetLeagueCourseResponse data, string leagueName, BattleEnterType battleEnterType)
        {
            this.battleEnterType = battleEnterType;
            if (!IsInitialized) Init();
            var result = new List<MyCourseItemModel>();
            int colorIndex = 0;
            // 按日期分组
            var groups = data.LeagueCourseItemList.GroupBy(item => TimeUtils.GetUnixTimeString(item.Time, Lang.Get(LangID.DateString2)));
            // 按时间排序
            foreach (var group in groups)
            {
                result.Add(new MyCourseItemModel() { PrefabType = MyCourseParams.PrefabType.DateItem, Date = group.First().Time });
                foreach (var item in group.OrderBy(t => t.Time))
                {
                    result.Add(new MyCourseItemModel()
                    {
                        ColorIndex = colorIndex,
                        PrefabType = MyCourseParams.PrefabType.CourseItem,
                        Data = item,
                        ItemClick = OnItemClick,
                        LeagueName = leagueName
                    });
                    colorIndex++;
                }
            }
            Data.ResetItems(result);
        }

        protected override bool IsRecyclable(BaseMyCourseViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.CanPresentModelType(Data[indexOfItemThatWillBecomeVisible].PrefabType);
        }
    }

    public class MyCourseItemModel
    {
        public int ColorIndex;
        public MyCourseParams.PrefabType PrefabType;
        public LeagueCourseItemData Data;
        public long Date;
        public Action<LeagueCourseItemData, BattleEnterType> ItemClick;
        public string LeagueName;
    }

    [Serializable]
    public class MyCourseParams : BaseParams
    {
        public enum PrefabType
        {
            DateItem,
            CourseItem
        }

        public GameObject MyCourseDateItem;
        public GameObject MyCourseItem;
    }

    public class MyCourseViewHolder : BaseMyCourseViewsHolder
    {
        private MyCourseItemModel model;
        private MyCoursePadItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<MyCoursePadItem>();
            item.Click += OnClick;
        }

        private void OnClick()
        {
            // 跳转到比赛回顾界面
            model.ItemClick.Invoke(model.Data, battleEnterType);
        }

        private BattleEnterType battleEnterType;
        public override void UpdateViews(MyCourseItemModel model, BattleEnterType battleEnterType)
        {
            this.battleEnterType = battleEnterType;
            this.model = model;
            item.SetData(model.Data, model.LeagueName, battleEnterType);
            if (model.ColorIndex % 2 == 0)
            {
                item.SetBackgroundColor(new Color(0, 0, 0, 51 / 255f));
            }
            else
            {
                item.SetBackgroundColor(new Color(0, 0, 0, 13 / 255f));
            }
        }

        public override bool CanPresentModelType(MyCourseParams.PrefabType prefabType)
        {
            return model.PrefabType == prefabType;
        }

        public override void PlayAnim(float delay)
        {
            root.transform.localScale = new Vector3(1, 0, 1);
            root.transform.DOScale(Vector3.one, 0.15f).SetDelay(delay);
        }
    }

    public class DateMyCourseViewHolder : BaseMyCourseViewsHolder
    {
        private MyCourseItemModel model;
        private MyCoursePadDateItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<MyCoursePadDateItem>();
        }

        public override void UpdateViews(MyCourseItemModel model, BattleEnterType battleEnterType)
        {
            this.model = model;
            item.SetData(model.Date);
        }

        public override bool CanPresentModelType(MyCourseParams.PrefabType prefabType)
        {
            return model.PrefabType == prefabType;
        }

        public override void PlayAnim(float delay) { }
    }

    public abstract class BaseMyCourseViewsHolder : BaseItemViewsHolder
    {
        public abstract bool CanPresentModelType(MyCourseParams.PrefabType prefabType);

        public abstract void UpdateViews(MyCourseItemModel model, BattleEnterType battleEnterType);


        public abstract void PlayAnim(float delay);
    }
}