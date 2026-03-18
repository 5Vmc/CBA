namespace Babu
{
    public abstract class TaskExecutor : Task
    {
        public delegate void TaskExecuteCallback(bool result);

        public override string GetTaskName()
        {
            return "Unknow Task Executor";
        }

        public abstract void Execute(TaskExecuteCallback callback);

        public abstract void OnChildTaskPaused();
        public abstract void OnChildTaskResumed();
        public abstract void OnChildTaskCompleted();
    }
}
