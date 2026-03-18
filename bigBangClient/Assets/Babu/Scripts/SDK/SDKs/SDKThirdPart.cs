using UnityEngine;

namespace Babu.SDK
{
    class SDKThirdPart : SequentialTaskExecutor
    {
        public static SDKThirdPart Instance;

        public override string GetTaskName()
        {
            return "SDKThirdPart";
        }

        void Awake()
        {
            Instance = this;
        }

        public void AddThirdPartSDK(Task task)
        {
            Debug.Log("Add Third Part SDK: " + task.GetTaskName());
            tasks.Add(task);
        }
    }
}
