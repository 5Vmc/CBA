using GameConfig;
using GameConfig.Config;
using Protocol;

namespace BigBang
{
    public class TaskData
    {
        public int Id { get; set; }
        public int State { get; set; }
        public int Progress { get; set; }
        public int Type { get => GetTaskDataType(Id); }

        public TaskConfig Config { get; set; }

        public TaskData() { }

        public void UnPack(TaskInfoNotify taskInfo)
        {
            Id = taskInfo.Id;
            State = taskInfo.State;
            Progress = taskInfo.Progress;

            Config = Configs.Task.GetConfig(Id);
        }

        // 获得任务类型
        public static int GetTaskDataType(int Id)
        {
            return ((Id % 10000) - (Id % 100)) / 100;
        }
    }
}
