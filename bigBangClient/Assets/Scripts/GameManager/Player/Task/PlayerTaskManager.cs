using Babu;
using Babu.Config;
using GameConfig;
using GameConfig.Config;
using Google.Protobuf.Collections;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BigBang
{
    public class PlayerTaskManager
    {
        public NormalTasks NormalTasks = new NormalTasks();
        public CyclicTasks DailyTasks = new CyclicTasks();
        public CyclicTasks WeeklyTasks = new CyclicTasks();

        public void UnPack(ModuleTaskInfoNotify moduleTaskInfo)
        {
            NormalTasks.UnPack(moduleTaskInfo.NormalTaskInfo);
            DailyTasks.UnPack(moduleTaskInfo.DailyTaskInfo);
            WeeklyTasks.UnPack(moduleTaskInfo.WeeklyTaskInfo);
            ActivityController.Instance.RefreshTaskAboutActivityRedDot();
        }

        public void UpdateSingleTask(TaskInfoNotify task, bool refreshRedDot = false)
        {
            int id = task.Id;
            TaskConfig config = Configs.Task.GetConfig(id);
            if (config == null) return;

            switch (config.Type)
            {
                case (int)TaskType.Normal:
                    if (NormalTasks.CompletedTasks.ContainsKey(task.Id)) return;
                    NormalTasks.UpdateSingleTask(task);
                    if (refreshRedDot) CheckTaskRedDot_Normal(TaskType.Normal);
                    break;
                case (int)TaskType.Daily:
                    DailyTasks.UpdateSingleTask(task);
                    if (refreshRedDot) CheckTaskRedDot(TaskType.Daily);
                    break;
                case (int)TaskType.Weekly:
                    WeeklyTasks.UpdateSingleTask(task);
                    if (refreshRedDot) CheckTaskRedDot(TaskType.Weekly);
                    break;
                default: Debug.LogError("Invalid Task Type: " + config.Type); break;
            }
        }

        public void BatchUpdateTask(TaskInfoBatchNotify task)
        {
            foreach (var item in task.Tasks)
            {
                UpdateSingleTask(item, false);
            }

            CheckTaskRedDot_Normal(TaskType.Normal);
            CheckTaskRedDot(TaskType.Daily);
            CheckTaskRedDot(TaskType.Weekly);
        }

        /// <summary>
        /// 生涯任务小红点检查
        /// </summary>
        public void CheckTaskRedDot_Normal(TaskType type, Action callback = null)
        {
            if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Career, false)) return;
            Dictionary<int, TaskData> dict = NormalTasks.Tasks;

            var task = dict.Where(task => task.Value.State == TaskState.COMPLETE).FirstOrDefault().Value;
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Career, "");
            if (task != null)
            {
                node.AddValue(1);
            }
            else
            {
                node.AddValue(-1);
            }
            callback?.Invoke();
        }

        /// <summary>
        /// 任务小红点检查
        /// </summary>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        public void CheckTaskRedDot(TaskType type)
        {
            if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Task, false)) return;
            var _task = type == TaskType.Daily ? DailyTasks : WeeklyTasks;
            var _point = _task.Point;
            var isred = false;
            //任意任务处于完成状态
            var task = _task.Tasks.Where(task => task.Value.State == TaskState.COMPLETE).FirstOrDefault().Value;
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Task, "/" + ((int)type).ToString() + "/task");
            node.AddValue(task == null ? -1 : 1);

            //任意宝箱没有领
            node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Task, "/" + ((int)type).ToString() + "/box");
            List<TaskRewardBoxConfig> _configs = Configs.TaskRewardBox.GetConfigList();
            for (var index = 0; index < _configs.Count; index++)
            {
                if (_configs[index].Type == (int)type && _point >= _configs[index].NeedPoint)
                {
                    if (!_task.CollectedBoxes.Exists(p => p == _configs[index].Id))
                    {
                        isred = true;
                    }
                }
            }
            node.AddValue(isred ? 1 : -1);
        }

        public void RemoveTask(int taskId)
        {
            NormalTasks.RemoveTask(taskId);
            CheckTaskRedDot_Normal(TaskType.Normal);
        }

        public void UpdateCyclicTaskPoint(int type, int point)
        {
            if (type == (int)TaskType.Daily)
            {
                DailyTasks.UpdatePoint(point);
                ActivityController.Instance.RefreshTaskAboutActivityRedDot();
            }
            else if (type == (int)TaskType.Weekly)
            {
                WeeklyTasks.UpdatePoint(point);
            }
            EventManager.Instance.Dispatch(EventID.OnRefreshTaskProgressItem, null);
        }

        public void UpdateCyclicTask(CyclicTaskInfoNotify cyclicTaskInfo)
        {
            if (cyclicTaskInfo.Type == (int)TaskType.Daily)
            {
                DailyTasks.UnPack(cyclicTaskInfo);
            }
            else if (cyclicTaskInfo.Type == (int)TaskType.Weekly)
            {
                WeeklyTasks.UnPack(cyclicTaskInfo);
            }
            EventManager.Instance.Dispatch(EventID.OnRefreshTaskProgressItem, null);
        }

        public void UpdateCyclicTaskCollectedBoxes(int type, RepeatedField<int> collectedBoxes)
        {
            if (type == (int)TaskType.Daily)
            {
                DailyTasks.UpdateCollectedBoxes(collectedBoxes);
            }
            else if (type == (int)TaskType.Weekly)
            {
                WeeklyTasks.UpdateCollectedBoxes(collectedBoxes);
            }
            //EventManager.Instance.Dispatch(EventID, null);
        }


    }
}
