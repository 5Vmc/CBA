using System;

namespace Babu.SDK
{
    public abstract class AccountService
    {
        public abstract bool IsLogined { get; }
        public abstract string AccountId { get; }
        public abstract string Token { get; }

        public abstract void SilentLogin(Action<bool> callback);
        public abstract void Login(Action<bool> callback);
        public abstract void Login(string account, Action<bool> callback);
        public abstract void Logout();
    }
}
