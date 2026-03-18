using System;

namespace Babu.SDK
{
    public class PurchaseServiceDefault : PurchaseService
    {
        public override void Init(string[] productIdList)
        {
        }

        public override void Purchase(PurchaseInfo info)
        {
            string orderId = Guid.NewGuid().ToString();
            string productId = info.GoodsId;
            DiamondServiceManager.Instance.Purchase(productId, orderId, Guid.NewGuid().ToString(), (error, id) =>
            {
                if (error == DiamondServiceManager.Error.Succ)
                {
                    EventManager.Instance.Dispatch(PurchaseServiceManager.Event.PurchaseResult, PurchaseServiceManager.Error.Succ, productId, orderId);
                }
                else
                {
                    EventManager.Instance.Dispatch(PurchaseServiceManager.Event.PurchaseResult, PurchaseServiceManager.Error.Unknown, productId);
                }
            });
        }
    }
}
