using Protocol;
using System.Collections.Generic;
using System.Linq;

namespace BigBang
{
    public class PlayerOnoffManager
    {
        private Dictionary<int, bool> onoffMap = new Dictionary<int, bool>();

        public void UnPack(OnoffInfoNotify data)
        {
            onoffMap = data.OnoffMap.ToDictionary(iter => iter.Key, iter => iter.Value);
        }

        public void OnOnoffChanged(OnoffChangedNotify data)
        {
            return;
            //onoffMap[data.Key] = data.Value;
            //// 挑战解锁
            //if (data.Key == (int)OnoffId.CHALLENGE)
            //{
            //    Player.TrainManager.AddMessage(MessageType.UnlockChallenge);
            //}
            //// 招募解锁
            //if (data.Key == (int)OnoffId.RECRUIT)
            //{
            //    //GuideManager.Finish(GuideID.Trigger_Recruit_1);
            //}
            //// 联赛解锁
            //if (data.Key == (int)OnoffId.LEAGUE)
            //{
            //    Player.TrainManager.AddMessage(MessageType.UnlockLeague);
            //}
        }

        public bool IsOn(OnoffId id)
        {
            if (onoffMap.TryGetValue((int)id, out var onoff))
            {
                return onoff;
            }
            return false;
        }
    }
}
