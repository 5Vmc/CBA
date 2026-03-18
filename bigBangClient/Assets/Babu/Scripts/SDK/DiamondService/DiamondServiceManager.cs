using System;

namespace Babu.SDK
{
    class DiamondServiceManager : BabuSingleton<DiamondServiceManager>
    {
        public class Error
        {
            public const int Unknown = -1;
            public const int Succ = 0;
            public const int Timeout = 1;
            public const int NoEnougnDiamond = 103;
            public const int AlreadyPurchased = 105;
        }

        public class Event
        {
            public const string DiamondChanged = "__DiamondChanged";      // 钻石更改事件;
        }

        protected DiamondService _diamondService = new DiamondServiceDefault();

        public void SetDiamondServiceHandler(DiamondService diamondService)
        {
            _diamondService = diamondService;
        }

        public int Diamond => _diamondService.Diamond;

        private void Start()
        {
            _diamondService.Init();
        }

        public bool CanCost(int cost)
        {
            return Diamond >= cost;
        }

        public void Sync(int count, Action<int> callback)
        {
            _diamondService.Sync(count, callback);
        }

        public void Get(Action<int> callback)
        {
            _diamondService.Get(callback);
        }

        public void Cost(string operateId, int cost, int reason, Action<int, string> callback)
        {
            _diamondService.Cost(operateId, cost, reason, callback);
        }

        public void Purchase(string productId, string transactionId, string receipt, Action<int, string> callback)
        {
            _diamondService.Purchase(productId, transactionId, receipt, callback);
        }
    }
}
