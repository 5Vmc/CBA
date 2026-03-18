using BigBang;
using System.Collections.Generic;

namespace Utils
{
    public struct RewardItemData
    {
        public GameItemType type;
        public int id;
        public int count;
    }
    public class StringUtil
    {
        public static List<RewardItemData> GetRewardByCfg(string rewardStr)
        {
            List<RewardItemData> list = new List<RewardItemData>();
            string[] arr = rewardStr.Split('|');
            for (int i = 0; i < arr.Length; i++)
            {
                string[] rewardArr = arr[i].Split(':');
                GameItemType itemType = (GameItemType)int.Parse(rewardArr[0]);
                int itemId = int.Parse(rewardArr[1]);
                int itemCount = int.Parse(rewardArr[2]);
                list.Add(new RewardItemData()
                {
                    type = itemType,
                    id = itemId,
                    count = itemCount
                });
            }
            return list;
        }
    }
}
