using System.Collections.Generic;
using UnityEngine;

namespace Babu.SDK
{
    class SDKAutoLogin : Task
    {
        [SerializeField] List<AccountServiceManager.AccountServiceType> autoLoginAccountServiceList;

        public override string GetTaskName()
        {
            return "SDKAutoLogin";
        }

        public override void Run(TaskExecutor executor)
        {
            foreach (var autoLoginAccountServiceType in autoLoginAccountServiceList)
            {
                Debug.Log("Auto Login: " + autoLoginAccountServiceType);
                AccountServiceManager.Instance.SilentLogin(autoLoginAccountServiceType, (result) =>
                {
                    Debug.Log($"Auto Login {autoLoginAccountServiceType} Result: {result}");
                });
            }

            executor.OnChildTaskCompleted();
        }
    }
}
