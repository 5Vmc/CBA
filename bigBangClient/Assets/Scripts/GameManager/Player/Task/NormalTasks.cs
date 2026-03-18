using Babu;
using GameConfig;
using GameConfig.Config;
using Google.Protobuf.Collections;
using Protocol;
using System.Collections.Generic;

namespace BigBang
{
    public class NormalTasks
    {
        public Dictionary<int, TaskData> Tasks { get; private set; } = new Dictionary<int, TaskData>(); // 未解锁 已完成组 不出现

        // 完成的任务
        public Dictionary<int, int> CompletedTasks { get; private set; } = new Dictionary<int, int>(); //key:taskID,value:完成时间

        // 该组完成的任务
        public Dictionary<int, List<int>> GroupCompletedTasks = new Dictionary<int, List<int>>();// key:组ID;value:List<完成任务id>

        // 完成的组
        public List<int> CompletedTaskGroups { get; private set; } //已完成组

        public void UnPack(NormalTaskInfo normalTaskInfo)
        {
            Tasks.Clear();
            foreach (var iter in normalTaskInfo.Tasks)
            {
                TaskData task = new TaskData();
                TaskConfig taskConfig = Configs.Task.GetConfig(iter.Value.Id);
                if (taskConfig == null) continue;
                if(taskConfig.IsHide == 1) continue;
                task.UnPack(iter.Value);
                Tasks.Add(iter.Key, task);
            }

            UpdateCompletedTasks(normalTaskInfo.CompletedTasks);
            UpdateCompletedTaskGroups(normalTaskInfo.CompletedTaskGroups);
        }

        /// <summary>
        /// 通知更新任务
        /// </summary>
        /// <param name="taskInfo"></param>
        public void UpdateSingleTask(TaskInfoNotify taskInfo)
        {
            TaskConfig taskConfig = Configs.Task.GetConfig(taskInfo.Id);
            if (taskConfig == null) return;
            if (taskConfig.IsHide == 1) return;
            if (Tasks.ContainsKey(taskInfo.Id))
            {
                Tasks[taskInfo.Id].UnPack(taskInfo);
            }
            else
            {
                TaskData task = new TaskData();
                task.UnPack(taskInfo);
                Tasks[taskInfo.Id] = task;
            }
            EventManager.Instance.Dispatch(EventID.Refresh_Normal_Task, taskInfo.Id);
        }


        /// <summary>
        /// 删除的时候同时会把下一个任务装入
        /// </summary>
        /// <param name="id"></param>
        public void RemoveTask(int id)
        {
            var _taskConfig = Configs.Task.GetConfig(id);
            if (_taskConfig == null) return;
            if (_taskConfig.NextTask != 0) {
                TaskData task = new TaskData();
                task.Id = _taskConfig.NextTask;
                task.State = TaskState.IN_PROGRESS;
                task.Progress = 0;

                task.Config = Configs.Task.GetConfig(_taskConfig.NextTask);
                if (!Tasks.ContainsKey(task.Id))
                    Tasks[task.Id] = task;
            }

            Tasks.Remove(id);
        }

        public void UpdateCompletedTasks(NormalCompletedTaskInfoNotify normalCompletedTaskInfo)
        {
            UpdateCompletedTasks(normalCompletedTaskInfo.CompletedTasks);
        }

        public void UpdateCompletedTaskGroups(NormalCompletedTaskGroupInfoNotify normalCompletedTaskGroupInfo)
        {
            UpdateCompletedTaskGroups(normalCompletedTaskGroupInfo.CompletedTaskGroups);
        }

        private void UpdateCompletedTasks(MapField<int, int> completedTasks)
        {
            CompletedTasks.Clear();
            GroupCompletedTasks.Clear();
            foreach (var iter in completedTasks)
            {
                CompletedTasks.Add(iter.Key, iter.Value);
                int type = TaskData.GetTaskDataType(iter.Key);
                if (!GroupCompletedTasks.ContainsKey(type))
                {
                    GroupCompletedTasks[type] = new List<int>();
                }
                GroupCompletedTasks[type].Add(iter.Key);
            }

        }

        private void UpdateCompletedTaskGroups(RepeatedField<int> completedTaskGroups)
        {
            CompletedTaskGroups = new List<int>();
            foreach (var id in completedTaskGroups)
            {
                CompletedTaskGroups.Add(id);
            }
        }
    }
}
