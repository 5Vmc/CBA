using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Protocol;
using UnityEngine;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang
{
    /// <summary>
    /// 端午节赛龙舟活动2024 的管理类
    /// </summary>
    public class DragonBoatFestivalManager : BabuSingleton<DragonBoatFestivalManager>
    {
        /// <summary> 所选队伍 </summary>
        public enum Team
        {
            /// <summary> 未选择 </summary>
            Unknown = 0,
            /// <summary> 左侧-甜 </summary>
            Left = 1,
            /// <summary> 右侧-咸 </summary>
            Right = 2,
        }

        /// <summary> 活动阶段 </summary>
        public enum Stage
        {
            /// <summary> 活动开始前，或者服务器卡了 </summary>
            NotOpen,
            /// <summary> 可选择队伍 </summary>
            CanSelectTeam,
            /// <summary> 比赛正常进行中 </summary>
            NormalPlaying,
            /// <summary> 结算奖励后 </summary>
            Ending,
            /// <summary> 活动消失后 </summary>
            Closed
        }

        /// <summary> 奖励状态 </summary>
        public enum RewardState
        {
            /// <summary> 不可领取 </summary>
            CanNotGet,
            /// <summary> 可领取 </summary>
            CanGet,
            /// <summary> 已领取 </summary>
            HasGot,
        }

        /// <summary>
        /// 领过的里程碑奖励id
        /// </summary>
        public List<int> progressRewardGetList = new();
        public Team myTeam = Team.Unknown;
        private SignActivityModuleNotify signActivityModuleNotify = null;
        public void Unpack(SignActivityModuleNotify signActivityModuleNotify)
        {
            this.signActivityModuleNotify = signActivityModuleNotify;
            progressRewardGetList.Clear();
            progressRewardGetList.AddRange(signActivityModuleNotify.DragonBoatMeterRewards);
            myTeam = (Team)signActivityModuleNotify.DragonBoatSide;
            ProcessPushData();
        }
        private void ProcessPushData()
        {
            RefreshRedDot();
        }

        public GetDragonBoatInfoResponse courseData = null;
        DateTime getCourseDateTime;
        public void GetCourseData(System.Action callback = null)
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalHome);
            if (activityData == null) return;
            if ((DataConvUtil.ServerDateTime - getCourseDateTime).TotalMilliseconds < 500)
            {
                ProcessCourseData();
                callback?.Invoke();
            }
            NetworkManager.Instance.GetDragonBoatInfo(activityData.cfg.Id, (GetDragonBoatInfoResponse getDragonBoatInfoResponse) =>
            {
                getCourseDateTime = DataConvUtil.ServerDateTime;
                courseData = getDragonBoatInfoResponse;
                List<AllStarRankInfo> allStarRankInfoList = courseData.Ranks.Where(r => r.Rank > 0).ToList();
                courseData.Ranks.Clear();
                courseData.Ranks.AddRange(allStarRankInfoList);
                ProcessCourseData();
                callback?.Invoke();
            });
        }
        private void ProcessCourseData()
        {
            RefreshRedDot();
        }
        private void RefreshRedDot()
        {
            ActivityController.Instance.RefreshActivityRedDotByClientType(ActivityClientType.DragonBoatFestivalHome);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }
        public Stage GetStage()
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalHome);
            if (activityData == null) return Stage.NotOpen;
            if (courseData == null) return Stage.NotOpen;
            if (activityData.IsHide) return Stage.Closed;
            if (activityData.IsEnd) return Stage.Ending;
            if (myTeam == Team.Unknown) return Stage.CanSelectTeam;
            return Stage.NormalPlaying;
        }

        public void PickDragonBoat(Team team, System.Action callback = null)
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalHome);
            NetworkManager.Instance.PickDragonBoat(activityData.cfg.Id, (int)team, (PickDragonBoatResponse pickDragonBoatResponse) =>
            {
                if (pickDragonBoatResponse.Success)
                {
                    myTeam = team;
                    ProcessPushData();
                    callback?.Invoke();
                    GetNewDataAndRefreshUI();
                }
                else
                {
                    GetNewDataAndRefreshUI();
                }
            });
        }
        private void GetNewDataAndRefreshUI()
        {
            GetCourseData(() =>
            {
                EventManager.Instance.Dispatch(EventID.RefreshDragonBoatFestivalUI);
            });
        }

        public RewardState GetRewardState(DragonBoatRewardConfig dragonBoatRewardConfig)
        {
            if (myTeam == Team.Unknown) return RewardState.CanNotGet;
            if (dragonBoatRewardConfig.Type != 2) return RewardState.CanNotGet;
            if (progressRewardGetList.Contains(dragonBoatRewardConfig.Option)) return RewardState.HasGot;
            if (myTeam == Team.Left && courseData != null && courseData.Meters.Count == 2 && courseData.Meters[0] >= dragonBoatRewardConfig.Option) return RewardState.CanGet;
            if (myTeam == Team.Right && courseData != null && courseData.Meters.Count == 2 && courseData.Meters[1] >= dragonBoatRewardConfig.Option) return RewardState.CanGet;
            return RewardState.CanNotGet;
        }
        public void GetDragonBoatMetersReward(DragonBoatRewardConfig dragonBoatRewardConfig, System.Action callback = null)
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalHome);
            NetworkManager.Instance.GetDragonBoatMetersReward(activityData.cfg.Id, dragonBoatRewardConfig.Option, (GetDragonBoatMetersRewardResponse getDragonBoatMetersRewardResponse) =>
            {
                if (getDragonBoatMetersRewardResponse.Success)
                {
                    if (progressRewardGetList.Contains(dragonBoatRewardConfig.Option) == false)
                    {
                        progressRewardGetList.Add(dragonBoatRewardConfig.Option);
                    }
                    ProcessPushData();
                    callback?.Invoke();
                    GetNewDataAndRefreshUI();
                }
                else
                {
                    GetNewDataAndRefreshUI();
                }
            });
        }
        public void AddDragonBoatMeters(int costCount, System.Action callback = null)
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalHome);
            NetworkManager.Instance.AddDragonBoatMeters(activityData.cfg.Id, costCount, (AddDragonBoatMetersResponse addDragonBoatMetersResponse) =>
            {
                if (addDragonBoatMetersResponse.Success)
                {
                    if (courseData.Meters.Count == 2)
                    {
                        courseData.Meters[(int)myTeam - 1] += addDragonBoatMetersResponse.Meter;
                    }
                    else
                    {
                        Debug.LogWarning("DragonBoatFestivalManager , AddDragonBoatMeters , courseData.Meters.Count != 2");
                    }
                    Tips.PopTips("助力成功，前进了{0}米".SafeFormat(addDragonBoatMetersResponse.Meter));
                    ProcessCourseData();
                    callback?.Invoke();
                    EventManager.Instance.Dispatch(EventID.OnUpDragonBoatFestivalTeam);
                    GetNewDataAndRefreshUI();
                }
                else
                {
                    GetNewDataAndRefreshUI();
                }
            });
        }

        /// <summary> 是否需要展示结尾弹窗 </summary>
        public bool IsNeedShowEnd
        {
            get
            {
                Stage stage = GetStage();
                if (stage != Stage.Ending) return false;
                if (UnityEngine.PlayerPrefs.GetInt(PlayerPrefsKeys.DragonBoatFestival2024ShowEnd + Player.GbId, 0) == 1) return false;
                return true;
            }
        }

        public bool CanUseDrum
        {
            get
            {
                Stage stage = GetStage();
                if (stage != Stage.NormalPlaying) return false;
                ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalHome);
                GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, activityData.cfg.Param1, 0);
                int drumCount = gameItem.GetPlayerCount();
                return drumCount > 0;
            }
        }
        public bool CanSelectTeam
        {
            get
            {
                Stage stage = GetStage();
                if (stage != Stage.CanSelectTeam) return false;
                return myTeam == Team.Unknown;
            }
        }
        public bool CanCollectTask
        {
            get
            {
                Stage stage = GetStage();
                if (stage != Stage.NormalPlaying) return false;
                ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalTask);
                return ActivityController.Instance.CheckRedDot_DragonBoatFestivalTask(activityData);
            }
        }
        public bool CanCollectProgress
        {
            get
            {
                Stage stage = GetStage();
                if (stage != Stage.NormalPlaying && stage != Stage.Ending) return false;
                foreach (DragonBoatRewardConfig dragonBoatRewardConfig in Configs.DragonBoatReward.GetConfigList())
                {
                    if (dragonBoatRewardConfig.Type != 2) continue;
                    RewardState rewardState = GetRewardState(dragonBoatRewardConfig);
                    if (rewardState == RewardState.CanGet)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}