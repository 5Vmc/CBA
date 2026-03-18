using System;
using UnityEngine;

namespace Babu.SDK
{
    class SDKOnlineTick : Task
    {
        [SerializeField] float tickTime = 300;

        public override string GetTaskName()
        {
            return "SDKOnlineTick";
        }

        const string TICK_URL = "https://cdn.japi.babuyo.com/account/tick";

        private float _onlineTime = 0;
        private bool _onlineValid = false;

        public override void Run(TaskExecutor executor)
        {
            Environment.SetValue("sdk_online_tick", true);

            Tick();

            _onlineValid = true;
            executor.OnChildTaskCompleted();
        }

        void Tick()
        {
            Debug.Log("Online Tick");
            string accountId = AccountServiceManager.Instance.GetAccountId(AccountServiceManager.AccountServiceType.Local);
            if (accountId != null && accountId != "")
            {
                string url = TICK_URL + "?accountId=" + accountId + "&appVersion=" + Application.version + "&packageName=" + Application.identifier + "&channel="
                    + Environment.GetValue("channel_id", "unknown");
                HttpService.Instance.AsyncGet(url, delegate(bool result, string response){ }, 5);
            }

            if (_onlineTime > 0)
            {
                AppEventManager.Instance.UpdateOnlineTime((int)_onlineTime);
                _onlineTime = 0;
            }

            DelayTaskService.Instance.Run(this.gameObject, tickTime, Tick);
        }

        void Update()
        {
            if (_onlineValid && Time.deltaTime < 10)
            {
                _onlineTime += Time.deltaTime;
            }
        }
    }
}
