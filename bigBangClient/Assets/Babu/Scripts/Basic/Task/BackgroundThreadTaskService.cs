using System.Collections.Generic;
using System.Threading;

namespace Babu
{
    public class BackgroundThreadTaskService : BabuSingleton<BackgroundThreadTaskService>
    {
        public delegate void TaskCallback();

        private List<TaskCallback> _tasks = new List<TaskCallback>();
        private List<TaskCallback> _executeTasks = new List<TaskCallback>();

        private Thread _workThread;
        private bool _stop = false;

        private EventWaitHandle _signal = new AutoResetEvent(false);

        public override void Awake()
        {
            base.Awake();
            _workThread = new Thread(Work);
            _workThread.Start();
        }

        public void AddTask(TaskCallback callback)
        {
            lock (_tasks)
            {
                _tasks.Add(callback);
            }
            _signal.Set();
        }

        private void Work()
        {
            while (_stop == false)
            {
                if (_tasks.Count == 0)
                {
                    _signal.WaitOne();
                }

                if (_stop)
                {
                    return;
                }

                lock (_tasks)
                {
                    Utils.Swap(ref _executeTasks, ref _tasks);
                }

                foreach (var task in _executeTasks)
                {
                    task();
                }
                _executeTasks.Clear();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _stop = true;
            _signal.Set();
            _workThread.Join();
            _workThread = null;
        }
    }
}
