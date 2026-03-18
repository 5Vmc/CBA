using UnityEngine;

namespace Babu
{
    public abstract class Task : MonoBehaviour
    {
        public virtual string GetTaskName()
        {
            return "Unknow Task";
        }

        public abstract void Run(TaskExecutor executor);
    }
}
