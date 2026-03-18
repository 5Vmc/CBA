using System;
using Google.Protobuf.WellKnownTypes;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

namespace Babu.SDK
{
    public class PurchaseServiceMiGu : PurchaseService
    {
        public override void Init(string[] productIdList)
        {

        }

        public override void Purchase(PurchaseInfo info)
        {
            EventManager.Instance.Dispatch(EventManager.CanNotHotFixId.CHARGE_START);
            UnityEngine.Debug.Log("start pay ... " + info.GoodsName);

            // 获取当前UTC时间  
            DateTime now = DateTime.UtcNow;
            // 定义Unix纪元的开始时间（1970年1月1日）  
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            // 计算自Unix纪元以来的时间间隔，并转换为秒  
            long secondsSinceUnixEpoch = (long)((now - unixEpoch).TotalSeconds);

            //string cpOrderID = info.ServerId + "$$" + info.GoodsId + "$$" + info.ShopItemId + "$$" + info.Amount + "$$" + info.RoleId + "$$" + DateTime.Now.Millisecond.ToString();

            //例如“17_10001005_648_0171726739596571_1726802606”，（限制32个）
            //string cpOrderID = info.ServerId + "_" + info.ShopItemId + "_" + info.Amount + "_" + info.RoleId + "_" + secondsSinceUnixEpoch.ToString();

            string cpOrderID = ToBase62(int.Parse(info.ServerId)) + "_" + ToBase62(info.ShopItemId) + "_" + ToBase62((long)info.Amount) + "_" + ToBase62(long.Parse("1" + info.RoleId)) + "_" + ToBase62(secondsSinceUnixEpoch);


            MiGuPlayManager.Instance.Pay(info.RoleId, cpOrderID, (int)(info.Amount * 100), info.GoodsName, "中职篮:全力以赴");
        }

        private readonly string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string ToBase62(long num)
        {
            StringBuilder result = new();
            long baseLength = 62;
            while (num > 0)
            {
                result.Append(chars[(int)(num % baseLength)]);
                num /= baseLength;
            }
            return string.Concat(result.ToString().Reverse());
        }

    }
}