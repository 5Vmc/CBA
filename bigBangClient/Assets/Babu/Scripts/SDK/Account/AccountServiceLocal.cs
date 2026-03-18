using System;
using UnityEngine;

namespace Babu.SDK
{
    public class AccountServiceLocal : AccountService
    {
        private string _account;

        public override string AccountId => _account;

        public override string Token => "";

        public override bool IsLogined => _account != null && _account != "";

        public override void SilentLogin(Action<bool> callback)
        {
            _account = StorageManager.Instance.Load("account.txt");
            if (_account == null)
            {
                _account = Guid.NewGuid().ToString();
                StorageManager.Instance.Store("account.txt", _account);
            }

            callback(true);
        }

        public override void Login(Action<bool> callback)
        {
            SilentLogin(callback);
        }

        public override void Login(string account, Action<bool> callback)
        {
            _account = account;
            callback(true);
        }

        public override void Logout()
        {
            Debug.Log("Logout");
        }
    }
}
