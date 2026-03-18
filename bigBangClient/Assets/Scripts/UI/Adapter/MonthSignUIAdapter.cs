using System.Collections.Generic;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using DG.Tweening;
using GameConfig;
using UnityEngine;
using Utils.GameItem;
using System.Linq;
using GameConfig.Config;
using Babu;
using Utils;
using System;
using Spine;
using Babu.Config;

namespace BigBang.UI
{
    public class MonthSignUIAdapter : GridAdapter<MonthSignUIParams, MonthSignViewHolder>
    {
        public SimpleDataHelper<MonthSignItemData> Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<MonthSignItemData>(this);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(CellGroupViewsHolder<MonthSignViewHolder> potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif
        public async void SetData(List<ActivityManager.RewardState> data)
        {
            if (!IsInitialized) Init();

            var list = new List<MonthSignItemData>();
            foreach (var item in data)
            {
                if (item.GetID() != 0)
                {
                    var model = new MonthSignItemData();
                    model.MonthSiginID = item.GetID();
                    var config = Configs.MoonSiginReward.GetConfig(model.MonthSiginID);
                    model.MonthSiginType = item.GetRewardType();
                    model.State = item.GetState();
                    model.gameItem = GetSignItem(config);
                    model.Icon = await model.gameItem.GetIcon();
                    model.Count = model.gameItem.Count;
                    model.Date = model.MonthSiginID;
                    model.Adapter = this;
                    list.Add(model);
                }
            }
            Data.ResetItems(list);
        }

        public async void RefreshData(int monthID)
        {
            var item = Player.ActivityManager.SignMonth.FirstOrDefault(item => item.GetID() == monthID);
            if (item == null) return;
            var model = Data.FirstOrDefault(item => item.MonthSiginID == monthID);
            if (model == null) return;
            var config = Configs.MoonSiginReward.GetConfig(model.MonthSiginID);
            model.MonthSiginType = item.GetRewardType();
            model.State = item.GetState();
            model.gameItem = GetSignItem(config);
            model.Icon = await model.gameItem.GetIcon();
            model.Count = model.gameItem.Count;
            model.Date = model.MonthSiginID;
            var viewHolder = GetViewHolderByMonthID(monthID);
            if (viewHolder == null) return;
            viewHolder.UpdateViews(model);
        }

        public GameItem GetSignItem(MoonSiginRewardConfig moonSiginRewardConfig)
        {
            long unixTimestampInMilliseconds = DataConvUtil.ServerTimeEx;
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestampInMilliseconds);
            DateTime dateTime = dateTimeOffset.LocalDateTime;
            int daysInMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            Debug.Log("MonthSignUIAdapter , GetSignItem , daysInMonth = " + daysInMonth);
            switch (daysInMonth)
            {
                case 28: return GameItemUtils.CreateGameItem(moonSiginRewardConfig.Content28);
                case 29: return GameItemUtils.CreateGameItem(moonSiginRewardConfig.Content29);
                case 30: return GameItemUtils.CreateGameItem(moonSiginRewardConfig.Content30);
                default: return GameItemUtils.CreateGameItem(moonSiginRewardConfig.Content);
            }
        }

        private MonthSignViewHolder GetViewHolderByMonthID(int monthID)
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                var groupVH = GetItemViewsHolder(i);

                for (int j = 0; j < groupVH.NumActiveCells; j++)
                {
                    var item = groupVH.ContainingCellViewsHolders[j];
                    if (item.MonthID == monthID) return item;
                }
            }
            return null;
        }

        // 播放进入动画
        public void PlayEnter()
        {
            for (int i = 0; i < VisibleItemsCount; i++)
            {
                var groupVH = GetItemViewsHolder(i);

                for (int j = 0; j < groupVH.NumActiveCells; j++)
                {
                    var item = groupVH.ContainingCellViewsHolders[j];
                    item.PlayAnim(i * 0.1f);
                }
            }
        }

        protected override void UpdateCellViewsHolder(MonthSignViewHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
    }

    [System.Serializable]
    public class MonthSignUIParams : GridParams { }

    public class MonthSignViewHolder : CellViewsHolder
    {
        private MonthSignItem item;

        public int MonthID { get => item.MonthID; }

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<MonthSignItem>();
        }

        public void UpdateViews(MonthSignItemData data)
        {
            item.SetData(data);
        }

        public void PlayAnim(float delay)
        {
            root.localScale = Vector3.zero;
            item.Anim.InitMissAnim();
            root.DOScale(1, 0.3f).SetDelay(delay).OnStart(() => item.Anim.PlayMissEnter(0.3f));
        }
    }
}