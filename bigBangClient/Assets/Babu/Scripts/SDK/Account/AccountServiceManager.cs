using System;
using System.Collections.Generic;
using UnityEngine;

namespace Babu.SDK
{
    public class AccountServiceManager : BabuSingleton<AccountServiceManager>
    {
        public enum AccountServiceType
        {
            Local,
            GooglePlay,
            Facebook,
            GooglePlayGame,
            GameCenter
        };

        protected Dictionary<AccountServiceType, AccountService> _accountServiceDict = new Dictionary<AccountServiceType, AccountService>();

        public void AddAccountService(AccountServiceType accountServiceType, AccountService accountService)
        {
            _accountServiceDict.Add(accountServiceType, accountService);
        }

        public bool IsLogined(AccountServiceType accountServiceType)
        {
            AccountService accountService;
            if (_accountServiceDict.TryGetValue(accountServiceType, out accountService))
            {
                return accountService.IsLogined;
            }
            else
            {
                Debug.LogWarning("Invalid Account Service Type: " + accountServiceType);
            }
            return false;
        }

        public string GetAccountId(AccountServiceType accountServiceType)
        {
            AccountService accountService;
            if (_accountServiceDict.TryGetValue(accountServiceType, out accountService))
            {
                return accountService.AccountId;
            }
            else
            {
                Debug.LogWarning("Invalid Account Service Type: " + accountServiceType);
            }
            return "";
        }

        public void SilentLogin(AccountServiceType accountServiceType, Action<bool> callback)
        {
            AccountService accountService;
            if (_accountServiceDict.TryGetValue(accountServiceType, out accountService))
            {
                accountService.SilentLogin(callback);
            }
            else
            {
                Debug.LogWarning("Invalid Account Service Type: " + accountServiceType);
                callback(false);
            }
        }

        public void Login(AccountServiceType accountServiceType, Action<bool> callback)
        {
            AccountService accountService;
            if (_accountServiceDict.TryGetValue(accountServiceType, out accountService))
            {
                accountService.Login(callback);
            }
            else
            {
                Debug.LogWarning("Invalid Account Service Type: " + accountServiceType);
                callback(false);
            }
        }

        public void Login(string account, AccountServiceType accountServiceType, Action<bool> callback)
        {
            AccountService accountService;
            if (_accountServiceDict.TryGetValue(accountServiceType, out accountService))
            {
                accountService.Login(account, callback);
            }
            else
            {
                Debug.LogWarning("Invalid Account Service Type: " + accountServiceType);
                callback(false);
            }
        }

        public void Logout(AccountServiceType accountServiceType)
        {
            AccountService accountService;
            if (_accountServiceDict.TryGetValue(accountServiceType, out accountService))
            {
                accountService.Logout();
            }
            else
            {
                Debug.LogWarning("Invalid Account Service Type: " + accountServiceType);
            }
        }

        public void BindLocalAccount(AccountServiceType accountServiceType)
        {
            // TODO：绑定
        }

        public void RecoverLocalAccount(AccountServiceType accountServiceType)
        {
            // TODO：通过某个账号恢复本地账号
        }
    }
}
