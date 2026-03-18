using GameConfig;
using GameConfig.Config;
using Google.Protobuf.Collections;
using Protocol;
using System.Collections.Generic;

namespace BigBang
{
    public class CyclicTasks
    {
        public Dictionary<int, TaskData> Tasks { get; private set; } = new Dictionary<int, TaskData>();
        public int Type { get; private set; }
        public int Point { get; private set; }
        public List<int> CollectedBoxes = new List<int>();

        public void UnPack(CyclicTaskInfoNotify cyclicTaskInfo)
        {
            Tasks.Clear();
            foreach (var iter in cyclicTaskInfo.Tasks)
            {
                TaskData task = new TaskData();
                TaskConfig taskConfig = Configs.Task.GetConfig(iter.Value.Id);
                if (taskConfig == null) continue;
                if (taskConfig.IsHide == 1) continue;
                task.UnPack(iter.Value);
                Tasks.Add(iter.Key, task);
            }

            Type = cyclicTaskInfo.Type;
            Point = cyclicTaskInfo.Point;

            UpdateCollectedBoxes(cyclicTaskInfo.CollectedBoxes);
        }

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
        }

        public void UpdatePoint(int point)
        {
            Point = point;
        }

        public void UpdateCollectedBoxes(RepeatedField<int> collectedBoxes)
        {
            CollectedBoxes.Clear();
            foreach (var id in collectedBoxes)
            {
                CollectedBoxes.Add(id);
            }
        }
    }
}
