using System.Runtime.InteropServices.ComTypes;
using GameConfig;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using GameConfig.Config;
using UnityEngine;
using Babu;
namespace BigBang
{


    public class NoviceTaskManager
    {
        public Dictionary<int, NoviceTaskInfo> NoviceTasks { get; private set; }

        private Dictionary<int, bool> dayReddotDict = new Dictionary<int, bool>(); //每日红点

        public event Action OnUpdateData;

        public NoviceTaskManager()
        {
            NoviceTasks = new Dictionary<int, NoviceTaskInfo>();
            foreach (var cfg in Configs.NoviceTargetTask.GetConfigList())
            {
                var taskData = new NoviceTaskInfo()
                {
                    Id = cfg.Id,
                    Current = 0,
                    Obtain = false
                };
                NoviceTasks[cfg.Id] = taskData;
            }
        }

        // 更新进度
        public void UpdateData(List<NoviceTaskInfo> data)
        {
            foreach (var item in data)
            {
                var newTaskData = new NoviceTaskInfo()
                {
                    Id = item.Id,
                    Current = item.Current,
                    Obtain = item.Obtain
                };
                NoviceTasks[item.Id] = newTaskData;
            }


            //重新算红点
            this.dayReddotDict.Clear();
            HasRedDot = false;
            foreach (int id in NoviceTasks.Keys)
            {
                var cfg = Configs.NoviceTargetTask.GetConfig(id);
                if (this.IsFinished(id) && this.IsObtain(id) == false)
                {
                    this.dayReddotDict[cfg.Day] = true;

                    if (cfg.Day <= this.Days) HasRedDot = true;
                }

            }

            OnUpdateData?.Invoke();
            ActivityController.Instance.RefreshClientRedDot(ActivityClientType.NoviceTarget);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        /// <summary>
        /// 是否个人目标全部完成
        /// </summary>
        /// <returns></returns>
        public bool IsNoviceTaskFinish()
        {
            foreach (int id in NoviceTasks.Keys)
            {
                var cfg = Configs.NoviceTargetTask.GetConfig(id);
                if (!this.IsObtain(id))
                {
                    return false;
                }

            }
            return true;
        }

        public int Days
        {
            get;
            set;
        }
        public int GetFinishedCount()
        {
            return NoviceTasks.Where(item => IsFinished(item.Value.Id)).Count();
        }

        public bool IsFinished(int id)
        {
            var cfg = Configs.NoviceTargetTask.GetConfig(id);
            return NoviceTasks[id].Current >= cfg.Target;
        }

        public bool IsObtain(int id)
        {
            return NoviceTasks[id].Obtain;
        }

        public int GetCurrnetCount(int id)
        {
            if (NoviceTasks.ContainsKey(id))
                return NoviceTasks[id].Current;
            return 0;
        }
        public int GetTargetCount(int id)
        {
            var cfg = Configs.NoviceTargetTask.GetConfig(id);
            return cfg.Target;
        }

        public bool HasRedDot
        {
            get;
            set;
        }

        public bool HasDayRedDot(int day)
        {
            bool ret = dayReddotDict.ContainsKey(day);
            return ret;
        }

        public bool IsOpen
        {
            get
            {
                bool isModuleOpen = TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Activity_NewPlay7Day, false);
                bool isBeforeEndtime = Player.NoviceTaskManager.Days <= GameConst.NOVICE_TASK_END_DADYS;
                bool isAllFinish = Player.NoviceTaskManager.IsNoviceTaskFinish();
                return isModuleOpen && isBeforeEndtime && !isAllFinish;
            }
        }

    }
}