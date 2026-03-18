using System.Collections.Generic;
using UnityEngine;
using Protocol;
using GameConfig;
using System.Linq;
using System;
using Babu;
using Utils;

namespace BigBang
{
    public class ActivityManager
    {
        public class RewardState
        {
            private int rewardID;
            private int rewardState;
            private int rewardType;

            public void SetValue(int id, int state, int type = (int)RewardType.AddedSigin)
            {
                rewardID = id;
                rewardState = state;
                rewardType = type;
            }

            public int GetID()
            {
                return rewardID;
            }

            public int GetState()
            {
                return rewardState;
            }

            public void SetState(int state)
            {
                rewardState = state;
            }

            public int GetRewardType()
            {
                return rewardType;
            }
        }

        public int LoginDay = 0;
        public int FestivalLoginDay = 0;
        public int SignDay = 0;
        public List<RewardState> SevenRewardList = new List<RewardState>();
        public List<RewardState> SignMonth = new List<RewardState>();
        public List<RewardState> AddedSignMonth = new List<RewardState>();
        private bool IsSevenSignCollect = false;
        private bool IsFestivalSevenSignCollect = false;
        private bool IsMonthSignCollect = false;
        private bool IsMonthSignCollectAdd = false;
        public int ShootGamePoint = 0;
        public int ShootGameTimesLeft = 0;
        public int ShootGameTimes = 0;
        public int ShootGameTodayPoint = 0;
        /// <summary>
        /// 活动充值信息
        /// </summary>
        public Dictionary<int, ActivityPayInfoData> activityPayInfoDict = new();

        /// <summary>
        /// 七日签到是否完成
        /// </summary>
        /// <param name="type">type = 1;普通七日签到；2，节假日签到</param>
        /// <returns></returns>
        public bool GetIsSevenSignFinish(int type = 1)
        {
            foreach (var item in SevenRewardList)
            {
                if (item.GetState() != (int)RewardStates.RECEIVED)
                {
                    return false;
                }
            }
            return true;
        }

        public bool GetIsSevenSignRedDot(bool isFestival = false)
        {
            return isFestival ? IsFestivalSevenSignCollect : IsSevenSignCollect;
        }

        public void SetIsSevenSignRedDot(bool isFestival = false)
        {
            if (isFestival)
            {
                Player.ActivityManager.IsFestivalSevenSignCollect = false;
                ActivityController.Instance.RefreshClientRedDot(ActivityClientType.NationalDayLogin);
            }
            else
            {
                Player.ActivityManager.IsSevenSignCollect = false;
                ActivityController.Instance.RefreshClientRedDot(ActivityClientType.Sign7Day);
            }
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        public bool GetIsMonthSignRedDot()
        {
            return (Player.ActivityManager.IsMonthSignCollect || Player.ActivityManager.IsMonthSignCollectAdd);
        }

        public bool GetIsMonthCardRedDot()
        {
            return (Player.ShopManager.MonthCard1Days > 0 && Player.ShopManager.IsGetMonthCard1 == false) ||
                (Player.ShopManager.MonthCard2Days > 0 && Player.ShopManager.IsGetMonthCard2 == false);
        }

        public bool GetIsNoviceRedDot()
        {
            return Player.NoviceTaskManager.HasRedDot;
        }

        public void Init()
        {
            SignMonth.Clear();
            AddedSignMonth.Clear();
            SevenRewardList.Clear();
            if (SevenRewardList == null || SevenRewardList.Count == 0)
            {
                foreach (var item in Configs.SevenDayLoginReward.GetConfigList())
                {
                    RewardState reward = new RewardState();
                    reward.SetValue(item.Id, (int)RewardStates.UNLOGIN);
                    SevenRewardList.Add(reward);
                }
            }

            foreach (var item in Configs.MoonSiginAddedReward.GetConfigList())
            {
                RewardState reward = new RewardState();
                reward.SetValue(item.Id, (int)RewardStates.UNLOGIN, (int)RewardType.AddedSigin);
                AddedSignMonth.Add(reward);
            }

            DateTime dtNow = DateTime.Now; //用客户端的月份
            int days = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);

            int day = 1;
            foreach (var item in Configs.MoonSiginReward.GetConfigList())
            {
                if (day > days) break;
                day++;
                RewardState reward = new RewardState();
                reward.SetValue(item.Id, (int)RewardStates.UNLOGIN, (int)RewardType.DaySigin);
                SignMonth.Add(reward);
            }
            AddListenerOnce();
        }
        private bool isAddListener = false;
        private void AddListenerOnce()
        {
            if (isAddListener) return;
            isAddListener = true;
            EventManager.Instance.Register(EventID.OnRefreshGoods, ActivityController.Instance.RefreshChristmasBoxRedDot);
            EventManager.Instance.Register(EventID.OnServerPushPackageChange, ActivityController.Instance.RefreshChristmasBoxRedDot);
        }

        /// <summary>
        /// 登录的时候全量更新给客户端，单个更新不通过这个接口。
        /// </summary>
        /// <param name="data"></param>
        public void UnPack(SignActivityModuleNotify data)
        {

            if (data == null)
            {
                return;
            }
            LoginDay = data.LoginDay;
            SignDay = data.SignDay;
            UpdateSevenSignInServer(data.SevenLoginList.ToList());
            RefreshMonthSignAddedInServer(data.MonthSignAdded.ToList());
            RefreshMonthSignInServer(data.MonthSign.ToList());
            UpdateMonthSign();
            UpdateMonthSignAdded();

            ShootGamePoint = data.ShootGamePoint;
            ShootGameTimesLeft = data.ShootGameTimesLeft;
            ShootGameTimes = data.ShootGameTimes;
            ShootGameTodayPoint = data.ShootGameTodayPoint;
            ActivityController.Instance.ServerOpenTime = data.OpenServerTime;
            ActivityController.Instance.InitOnlineActivityList();

            //通知条件礼包
            TimeGiftController.Instance.Update(data.PayTriggerList, 0, true);

            ActivityController.Instance.UpdatePointList(data.PointList);
            ActivityController.Instance.UpdatePayMicroList(data.PayMicroList);
            ActivityController.Instance.UpdateDailyGiftActivities(data.DailyGiftActivities);
            ActivityController.Instance.UpdateEnergy(data.DailyEnergyRewards);
            if (!LoginManager.Instance.isBeforeLoadingEnd)
                EventManager.Instance.Dispatch(EventID.OnRefreshActivityTab);
            ActivityController.Instance.RebuildFeativalTaskData(data.FestivalTasks);
            ActivityController.Instance.todayWishTimes = data.TodayWishTimes;
            ActivityController.Instance.wishSigns = data.WishSigns.ToList();
            ActivityController.Instance.wishSignRewards = data.WishSignRewards.ToList();
            AllStarManager.Instance.Unpack(data);
            LabourDayManager.Instance.Unpack(data);
            PlayoffFinalsGuessManager.Instance.Unpack(data);
            DragonBoatFestivalManager.Instance.Unpack(data);

            ////准备启动就要弹的各种礼包窗口，不能挪到其他地方，例如unpack，在服务器启动的时候有可能被推送多次。
            //ActivityController.Instance.PrepareStartWindow();
            //if (UIController.Instance.PopwindowFlag) UIController.Instance.OpenAllHideScreens();

            ActivityController.Instance.RefreshAllRedDot();
            ActivityController.Instance.CheckGetMoreInformation();
        }
        public void RefreshOnlineActivity()
        {
            ActivityController.Instance.InitOnlineActivityList(false);
            ActivityController.Instance.RefreshAllRedDot();
        }

        private void UpdateSevenSignInServer(List<KeyValuePair<int, int>> list)
        {
            int index = 0;
            foreach (var item in list)
            {
                if (item.Value == 3)
                    SevenRewardList[index].SetState((int)RewardStates.RECEIVED);
                ++index;
            }
            foreach (var item in SevenRewardList)
            {
                if (item.GetID() <= LoginDay && item.GetState() != (int)RewardStates.RECEIVED)
                {
                    item.SetState((int)RewardStates.COLLECT);
                    IsSevenSignCollect = true;              //显示红点
                }
            }
            ActivityController.Instance.RefreshClientRedDot(ActivityClientType.Sign7Day);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        /// <summary>
        /// 检查是否有小红点
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public bool CheckFunRed(int moduleId)
        {
            if (moduleId == 1801)
            {
                //小游戏
                return Player.ActivityManager.ShootGameTimesLeft > 0;
            }
            else if (moduleId == 1802)
            {
                //领体力
                var currentIndex = ActivityController.Instance.GetCurrentEnergyStatus();
                if (currentIndex != -1 && ActivityController.Instance.EnergyRecord[currentIndex + 1] == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public void RefreshChallengeRedDot()
        {
            ActivityController.Instance.RefreshNewYearChallengeRedDot();
            if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Games, false)) return;
            var moduleId = 1801;
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Games, "/" + moduleId.ToString());
            node.AddValue(CheckFunRed(moduleId) ? 1 : -1);
            moduleId = 1802;
            node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Games, "/" + moduleId.ToString());
            node.AddValue(CheckFunRed(moduleId) ? 1 : -1);
        }

        private void RefreshMonthSignAddedInServer(List<int> list)
        {
            // Debug.Log("RefreshMonthSignAddedInServer listCount=" + list.Count);
            if (list.Count == 0)
            {
                return;
            }

            AddedSignMonth.Clear();
            foreach (var item in Configs.MoonSiginAddedReward.GetConfigList())
            {
                RewardState reward = new RewardState();
                reward.SetValue(item.Id, (int)RewardStates.UNLOGIN, (int)RewardType.AddedSigin);
                AddedSignMonth.Add(reward);
            }


            foreach (var itemID in list)
            {
                // if (item != 0)
                // {
                //     Debug.Log("RefreshMonthSignAddedInServer state=" + item);
                //     AddedSignMonth[index].SetState((int)RewardStates.RECEIVED);
                //     ++index;
                // }
                ChangeAddedSignMonthState2Received(itemID);
            }
        }

        private void ChangeAddedSignMonthState2Received(int itemID)
        {
            foreach (RewardState state in AddedSignMonth)
            {
                if (state.GetID() == itemID)
                {
                    state.SetState((int)RewardStates.RECEIVED);
                    break;
                }
            }
        }

        private void RefreshMonthSignInServer(List<KeyValuePair<int, int>> list)
        {
            if (list.Count == 0)
            {
                return;
            }

            SignMonth.Clear();
            //DateTime dtNow = Convert.ToDateTime(DateTime.Parse(DateTime.Now.ToString("1970-01-01 08:00:00")).AddMilliseconds(DataConvUtil.ServerTimeEx).ToString());
            DateTime dtNow = DataConvUtil.ServerDateTime;
            //Debug.Log("ServerDateTime = " + dtNow);
            int days = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);

            int day = 1;
            foreach (var item in Configs.MoonSiginReward.GetConfigList())
            {
                if (day > days) break;
                day++;
                RewardState reward = new RewardState();
                reward.SetValue(item.Id, (int)RewardStates.UNLOGIN, (int)RewardType.DaySigin);
                SignMonth.Add(reward);
            }

            foreach (var item in list)
            {
                if (item.Value != 0)
                {
                    int index = item.Key - 1;
                    if (index >= SignMonth.Count || index < 0)
                    {
                        Debug.LogWarning("ActivityManager , RefreshMonthSignInServer , SignMonth.Count == " + SignMonth.Count + " , index = " + index);
                        continue;
                    }
                    SignMonth[index].SetState(item.Value);
                }
            }
        }

        public void UpdateMonthSign()
        {
            IsMonthSignCollect = false;
            foreach (var item in SignMonth)
            {
                if (item.GetState() == (int)RewardStates.COLLECT)
                {
                    IsMonthSignCollect = true;
                }
            }
            ActivityController.Instance.RefreshClientRedDot(ActivityClientType.Sign30Day);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }

        public void UpdateMonthSignAdded()
        {
            IsMonthSignCollectAdd = false;
            foreach (var item in AddedSignMonth)
            {
                if (SignDay >= item.GetID() && item.GetState() != (int)RewardStates.RECEIVED)
                {
                    item.SetState((int)RewardStates.COLLECT);
                    IsMonthSignCollectAdd = true;
                }
            }
            ActivityController.Instance.RefreshClientRedDot(ActivityClientType.Sign30Day);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }
    }
}

