using System;

namespace Babu.SDK
{
    class DiamondServiceDefault : DiamondService
    {
        public override void Sync(int count, Action<int> callback)
        {
            SetDiamond(count);
            callback(DiamondServiceManager.Error.Succ);
        }

        public override void Get(Action<int> callback)
        {
            callback(DiamondServiceManager.Error.Succ);
        }

        public override void Cost(string operateId, int cost, int reason, Action<int, string> callback)
        {
            if (_diamond < cost)
            {
                callback(DiamondServiceManager.Error.NoEnougnDiamond, operateId);
            }
            else
            {
                SetDiamond(_diamond - cost);
                callback(DiamondServiceManager.Error.Succ, operateId);
            }
        }

        public override void Purchase(string productId, string transactionId, string receipt, Action<int, string> callback)
        {
            SetDiamond(_diamond + 100);
            callback(DiamondServiceManager.Error.Succ, transactionId);
        }
    }
}
