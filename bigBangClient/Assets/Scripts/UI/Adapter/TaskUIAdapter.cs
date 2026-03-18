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
using Babu;

namespace BigBang.UI
{
    public class TaskUIAdapter : OSA<TaskUIParams, TaskViewsHolder>
    {
        public SimpleDataHelper<TaskItemData> Data { get; private set; }

        private List<TaskData> dailyData;
        private List<TaskData> weeklyData;
        private TaskType currentTaskType;
        protected override void Awake()
        {
            base.Awake();
            Data = new SimpleDataHelper<TaskItemData>(this);
        }

        protected override void UpdateViewsHolder(TaskViewsHolder newOrRecycled)
        {
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.UpdateViews(model);
        }
#if UNITY_WEBGL
        protected override bool IsRecyclable(TaskViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            return potentiallyRecyclable.ItemIndex == indexOfItemThatWillBecomeVisible;
        }
#endif 
        public void SetData(IEnumerable<TaskData> dailyData, IEnumerable<TaskData> weeklyData)
        {
            // 优先显示已完成任务，然后是未完成任务，接着是已领取任务。最后显示未解锁任务。
            // 同状态按照配置ID从小到大显示
            this.dailyData = dailyData.OrderByDescending(item => item.State == TaskState.COMPLETE) // 已完成
                .ThenByDescending(item => item.State == TaskState.IN_PROGRESS)      // 未完成
                .ThenByDescending(item => item.State == TaskState.COLLECTED)        // 已领取
                .ThenByDescending(item => item.State == TaskState.LOCK)             // 未解锁
                .ThenBy(item => item.Id).ToList();
            this.weeklyData = weeklyData.OrderByDescending(item => item.State == TaskState.COMPLETE) // 已完成
                .ThenByDescending(item => item.State == TaskState.IN_PROGRESS)      // 未完成
                .ThenByDescending(item => item.State == TaskState.COLLECTED)        // 已领取
                .ThenByDescending(item => item.State == TaskState.LOCK)             // 未解锁
                .ThenBy(item => item.Id).ToList();
        }

        public void ShowDailyData()
        {
            currentTaskType = TaskType.Daily;
            SetData(dailyData);
            //SetRedDotData();
        }

        public void ShowWeeklyData()
        {
            currentTaskType = TaskType.Weekly;
            SetData(weeklyData);
            //SetRedDotData();
        }

        public void SetData(IEnumerable<TaskData> data)
        {
            if (!IsInitialized) Init();
            List<TaskItemData> list = new List<TaskItemData>();

            int index = 0;
            foreach (var item in data)
            {
                var model = new TaskItemData();
                model.IsUnlock = item.State != TaskState.LOCK;
                model.TaskID = item.Id;
                model.Desc = item.Config.Desc;
                model.Sum = item.Config.Condition;
                model.Count = item.Progress;
                model.State = item.State;
                model.Way = item.Config.Way;
                model.Point = item.Config.Point;
                model.ViewHolderIndex = index;
                model.moduleId = item.Config.Moduleid;
                list.Add(model);
                index++;
            }
            Data.ResetItems(list);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Babu.EventManager.Instance.Register(EventID.OnRefreshTaskUI, OnUpdateTaskItemViewHolder);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Babu.EventManager.Instance.Unregister(EventID.OnRefreshTaskUI, OnUpdateTaskItemViewHolder);
        }

        private void OnUpdateTaskItemViewHolder(object[] args)
        {
            int index = (int)args[0];
            var newData = (TaskItemData)args[1];
            Data[index].IsUnlock = newData.IsUnlock;
            Data[index].TaskID = newData.TaskID;
            Data[index].Desc = newData.Desc;
            Data[index].Sum = newData.Sum;
            Data[index].Count = newData.Count;
            Data[index].State = newData.State;

            var data = currentTaskType == TaskType.Daily ? dailyData : weeklyData;
            data.Where(item => item.Config.Id == newData.TaskID).ToList()[0].State = newData.State;

            Player.TaskManager.CheckTaskRedDot(currentTaskType);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
            Refresh();
        }

        protected override TaskViewsHolder CreateViewsHolder(int itemIndex)
        {
            var instance = new TaskViewsHolder();
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
    public class TaskUIParams : BaseParams
    {
        public GameObject prefab;
    }

    public class TaskViewsHolder : BaseItemViewsHolder
    {
        private TaskItem item;

        public override void CollectViews()
        {
            base.CollectViews();
            item = root.GetComponent<TaskItem>();
        }

        public void UpdateViews(TaskItemData data)
        {
            item.Anim.StopPlayObtainAnim();
            item.SetData(data);
        }

        // 播放动画
        public void PlayAnim(float delay)
        {
            clearAnim();
            root.gameObject.DOFade(1, 0.3f).SetDelay(delay);
            root.DOScale(1, 0.3f).SetDelay(delay);
        }

        public void InitAnim()
        {
            clearAnim();
            root.gameObject.SetAlpha(0);
            root.localScale = Vector3.one * 0.8f;
        }

        public void PlayExit()
        {
            clearAnim();
            root.gameObject.DOFade(0, 0.3f);
        }

        private void clearAnim()
        {
            item.Anim.StopPlayObtainAnim();
            root.gameObject.DOKill();
            root.DOKill();
        }
    }
}