using System;

namespace Babu.SDK
{
    abstract class DiamondService
    {
        protected int _diamond;

        public int Diamond => _diamond;

        protected void SetDiamond(int diamond)
        {
            int old = _diamond;
            _diamond = diamond;
            if (old != _diamond)
            {
                EventManager.Instance.Dispatch(DiamondServiceManager.Event.DiamondChanged, _diamond);
            }
        }

        public virtual void Init() { }
        public abstract void Sync(int count, Action<int> callback);
        public abstract void Get(Action<int> callback);
        public abstract void Cost(string operateId, int cost, int reason, Action<int, string> callback);
        public abstract void Purchase(string productId, string transactionId, string receipt, Action<int, string> callback);
    }
}
