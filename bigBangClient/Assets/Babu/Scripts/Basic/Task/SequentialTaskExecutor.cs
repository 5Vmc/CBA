using System.Collections.Generic;
using UnityEngine;

namespace Babu
{
    // 序列任务执行器，按顺序把绑定的任务一一执行
    public class SequentialTaskExecutor : TaskExecutor
    {
        [SerializeField] protected List<Task> tasks = new List<Task>();
        [SerializeField] private float timeout = -1;

        protected TaskExecutor _executor;

        protected TaskExecuteCallback _callback;

        protected bool _executing = false;
        protected bool _childTaskPaused = false;
        protected float _executeTime = 0;
        protected int _taskId = 0;
        protected int _targetTaskId = 0;

        void Update()
        {
            if (!_executing)
            {
                return;
            }

            if (timeout > 0 && _childTaskPaused == false)
            {
                _executeTime += Time.deltaTime;
                if (_executeTime > timeout)
                {
                    OnAllTaskComplete(false);
                    return;
                }
            }

            if (_taskId < _targetTaskId)
            {
                // 下一个任务;
                ++_taskId;
                ExecutOneTask();
            }
        }

        public override void Execute(TaskExecuteCallback callback)
        {
            _callback = callback;
            BeginExecute();
        }

        public override void Run(TaskExecutor executor)
        {
            _executor = executor;
            BeginExecute();
        }

        void ExecutOneTask()
        {
            if (_taskId >= tasks.Count)
            {
                OnAllTaskComplete(true);
            }
            else
            {
                Task task = tasks[_taskId];
                Debug.LogFormat("Task: {0} Begin ========", task.GetTaskName());
                task.Run(this);
            }
        }

        public override void OnChildTaskCompleted()
        {
            Debug.LogFormat("Task: {0} End ========", tasks[_taskId].GetTaskName());
            ++_targetTaskId;
        }

        void BeginExecute()
        {
            _taskId = -1;
            _targetTaskId = 0;
            _executing = true;
        }

        void OnAllTaskComplete(bool result)
        {
            _callback?.Invoke(result);
            _executor?.OnChildTaskCompleted();

            _executing = false;
            _executeTime = 0;
        }

        public override void OnChildTaskPaused()
        {
            if (_childTaskPaused == false)
            {
                Debug.LogFormat("Task: {0} Paused ========", tasks[_taskId].GetTaskName());
                _childTaskPaused = true;
                _executor?.OnChildTaskPaused();
            }
        }

        public override void OnChildTaskResumed()
        {
            if (_childTaskPaused == true)
            {
                Debug.LogFormat("Task: {0} Resumed ========", tasks[_taskId].GetTaskName());
                _childTaskPaused = true;
                _executor?.OnChildTaskResumed();
            }
        }
    }
}
