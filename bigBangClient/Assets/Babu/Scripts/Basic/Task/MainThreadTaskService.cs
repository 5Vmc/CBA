using System;
using System.Collections.Generic;
using UnityEngine;

namespace Babu
{
    // 主线程任务服务
    // 1. 用于把任务从其他线程放到主线程执行
    // 2. 用于把任务放到下一帧执行
    public class MainThreadTaskService : BabuSingleton<MainThreadTaskService>
    {
        public delegate void TaskCallback();

        private List<TaskCallback> _tasks = new List<TaskCallback>();
        private List<TaskCallback> _executeTasks = new List<TaskCallback>();
        private int _mainThreadId;

        public override void Awake()
        {
            base.Awake();
            _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public bool IsMainThread()
        {
            return _mainThreadId == System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        [Obsolete("Use Run instead.", false)]
        public void AddTask(TaskCallback callback)
        {
            lock (_tasks)
            {
                _tasks.Add(callback);
            }
        }

        public void Run(TaskCallback callback)
        {
            lock (_tasks)
            {
                _tasks.Add(callback);
            }
        }

        void Update()
        {
            lock (_tasks)
            {
                Utils.Swap(ref _executeTasks, ref _tasks);
            }

            foreach (var task in _executeTasks)
            {
                try
                {
                    task();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            _executeTasks.Clear();
        }
    }
}