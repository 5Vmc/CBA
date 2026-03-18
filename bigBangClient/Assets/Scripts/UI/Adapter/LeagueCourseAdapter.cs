using UnityEngine;
using Com.TheFallenGames.OSA.DataHelpers;
using Protocol;
using System.Collections.Generic;
using System;
using Com.TheFallenGames.OSA.Core;
using TMPro;
using Utils;
using System.Linq;
using DG.Tweening;
using GameConfig;
using static BigBang.BattleManager;
using static BigBang.UI.LeagueCourseUI;
using Babu;

namespace BigBang.UI
{
    public class LeagueCourseAdapter : OSA<LeagueCourseParams, BaseLeagueCourseViewsHolder>
    {
        public SimpleDataHelper<LeagueCourseItemModel> Data { get; private set; }

        private int compitionID;

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<LeagueCourseItemModel>(this);
        }

        protected override void UpdateViewsHolder(BaseLeagueCourseViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model, compitionID, battleEnterType, subUIID);
            newOrRecycled.root.localScale = Vector3.one;
        }
        protected override BaseLeagueCourseViewsHolder CreateViewsHolder(int itemIndex)
        {
            var model = Data[itemIndex];
            if (model.PrefabType == LeagueCourseParams.PrefabType.DateItem)
            {
                var instance = new DateLeagueViewHolder();
                instance.Init(_Params.LeagueCourseDateItem, _Params.Content, itemIndex);
                return instance;
            }
            if (model.PrefabType == LeagueCourseParams.PrefabType.CourseItem)
            {
                var instance = new CourseLeagueViewHolder();
                instance.Init(_Params.LeagueCourseItem, _Params.Content, itemIndex);
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
                if (model.PrefabType == LeagueCourseParams.PrefabType.DateItem)
                {
                    itemsDesc[i] = 46;
                }
                else
                {
                    itemsDesc[i] = 86;
                }
            }
            itemsDesc.EndChangingItemsSizes();
        }

        private BattleEnterType battleEnterType;
        private SubUIID subUIID;
        public void SetData(GetLeagueCourseResponse data, int compitionID, BattleEnterType battleEnterType, SubUIID subUIID = SubUIID.All)
        {
            this.compitionID = compitionID;
            this.battleEnterType = battleEnterType;
            this.subUIID = subUIID;
            if (!IsInitialized) Init();
            var result = new List<LeagueCourseItemModel>();
            int colorIndex = 0;
            // 按轮次分组
            IEnumerable<IGrouping<int, LeagueCourseItemData>> groups = null;
            if (subUIID == SubUIID.All)
            {
                groups = data.LeagueCourseItemList.GroupBy(item => item.Round);
            }
            else
            {
                groups = data.LeagueCourseItemList.GroupBy((item) =>
                {
                    return TimeUtils.ToDateTime(item.Time).DayOfYear;
                });
            }
            // 按时间排序
            foreach (var group in groups)
            {
                // 排除未比赛的轮次
                if (group.AsEnumerable().Count(item => item.HomeTeam != null || item.AwayTeam != null) == 0) continue;
                result.Add(new LeagueCourseItemModel() { PrefabType = LeagueCourseParams.PrefabType.DateItem, Round = group.First().Round, Data = group.First() });
                foreach (var item in group.OrderBy(t => t.Time))
                {
                    result.Add(new LeagueCourseItemModel() { ColorIndex = colorIndex, PrefabType = LeagueCourseParams.PrefabType.CourseItem, Data = item });
                    colorIndex++;
                }
            }
            Data.ResetItems(result);
            // 将我的比赛放在屏幕中央
            for (int i = 0; i < Data.Count; ++i)
            {
                if (Data[i].PrefabType != LeagueCourseParams.PrefabType.CourseItem) continue;
                if (Data[i].Data == null || Data[i].Data.HomeTeam == null || Data[i].Data.AwayTeam == null) continue;
                if (Data[i].Data.AwayGoal != -1 || Data[i].Data.HomeGoal != -1) continue;

                if (Data[i].Data.HomeTeam.TeamId == Player.GbId || Data[i].Data.AwayTeam.TeamId == Player.GbId)
                {
                    // 将视图移动到屏幕中央
                    ScrollTo(Mathf.Max(0, i - VisibleItemsCount / 2));
                    return;
                }
            }
        }

        protected override bool IsRecyclable(BaseLeagueCourseViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.CanPresentModelType(Data[indexOfItemThatWillBecomeVisible].PrefabType);
        }
    }

    public class LeagueCourseItemModel
    {
        public int ColorIndex;
        public LeagueCourseParams.PrefabType PrefabType;
        public LeagueCourseItemData Data;
        public int Round;
    }

    [Serializable]
    public class LeagueCourseParams : BaseParams
    {
        public enum PrefabType
        {
            DateItem,
            CourseItem
        }

        public GameObject LeagueCourseDateItem;
        public GameObject LeagueCourseItem;
    }

    // 比分信息
    public class CourseLeagueViewHolder : BaseLeagueCourseViewsHolder
    {
        private LeagueCourseItemModel model;
        private LeagueCourseItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<LeagueCourseItem>();
        }

        public override void UpdateViews(LeagueCourseItemModel model, int compitionID, BattleEnterType battleEnterType, SubUIID subUIID)
        {
            this.model = model;
            item.SetData(model.Data, battleEnterType, subUIID);
            item.SetBackground(model.ColorIndex % 2 == 0);
        }

        public override bool CanPresentModelType(LeagueCourseParams.PrefabType prefabType)
        {
            return model.PrefabType == prefabType;
        }

        public override void PlayAnim(float delay)
        {
            root.transform.DOScale(Vector3.one, 0.15f).SetDelay(delay);
        }

        public override void InitAnim()
        {
            root.transform.DOKill();
            root.transform.localScale = new Vector3(1, 0, 1);
        }
    }

    // 日期|轮次
    public class DateLeagueViewHolder : BaseLeagueCourseViewsHolder
    {
        private LeagueCourseItemModel model;
        private LeagueCourseDateItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<LeagueCourseDateItem>();
        }

        public override void UpdateViews(LeagueCourseItemModel model, int compitionID, BattleEnterType battleEnterType, SubUIID subUIID)
        {
            this.model = model;
            switch (compitionID)
            {
                case CompitionID.League:
                    if (subUIID == SubUIID.All)
                    {
                        item.normalDateText.gameObject.SetActive(true);
                        item.todayDateText.gameObject.SetActive(false);
                        item.todayTitleText.gameObject.SetActive(false);
                        item.normalDateText.text = Lang.Get(LangID.RoundText).Replace("{value}", model.Round.ToString());
                    }
                    else
                    {
                        if (TimeUtils.ToDateTime(model.Data.Time).DayOfYear == DataConvUtil.ServerDateTime.DayOfYear)
                        {
                            item.normalDateText.gameObject.SetActive(false);
                            item.todayDateText.gameObject.SetActive(true);
                            item.todayTitleText.gameObject.SetActive(true);
                            item.todayDateText.text = TimeUtils.GetUnixTimeString(model.Data.Time, "MM-dd");
                        }
                        else
                        {
                            item.normalDateText.gameObject.SetActive(true);
                            item.todayDateText.gameObject.SetActive(false);
                            item.todayTitleText.gameObject.SetActive(false);
                            item.normalDateText.text = TimeUtils.GetUnixTimeString(model.Data.Time, "MM-dd");
                        }
                    }
                    break;
                case CompitionID.Cup:
                    item.normalDateText.gameObject.SetActive(true);
                    item.todayDateText.gameObject.SetActive(false);
                    item.todayTitleText.gameObject.SetActive(false);
                    item.normalDateText.text = Configs.CupCourse.GetConfig(model.Round).RoundName;
                    break;
            }
        }

        public override bool CanPresentModelType(LeagueCourseParams.PrefabType prefabType)
        {
            return model.PrefabType == prefabType;
        }

        public override void PlayAnim(float delay)
        {
            root.transform.DOScale(Vector3.one, 0.15f).SetDelay(delay);
        }

        public override void InitAnim()
        {
            root.transform.DOKill();
            root.transform.localScale = new Vector3(1, 0, 1);
        }
    }

    public abstract class BaseLeagueCourseViewsHolder : BaseItemViewsHolder
    {
        public abstract bool CanPresentModelType(LeagueCourseParams.PrefabType prefabType);

        public abstract void UpdateViews(LeagueCourseItemModel model, int compitionID, BattleEnterType battleEnterType, SubUIID subUIID);

        public abstract void PlayAnim(float delay);

        public abstract void InitAnim();

    }
}