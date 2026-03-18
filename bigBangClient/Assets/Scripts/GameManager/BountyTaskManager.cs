using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Protocol;
using UnityEngine;
using Utils;

namespace BigBang
{
    /// <summary>
    /// 悬赏任务的管理类
    /// </summary>
    public class BountyTaskManager : BabuSingleton<BountyTaskManager>
    {

        #region 对配置数据在loading时处理

        private bool isInited = false;
        /// <summary>
        /// 配置表加载完成后，对数据进行预处理
        /// </summary>
        public void InitOnce(bool forceInit = true)
        {
            if (isInited && !forceInit) return;
            isInited = true;
            InitPlayerLevelDataDic();
        }

        List<UserLevelConfig> bountyTaskCountChangeUserLevelConfigList = new();
        private void InitPlayerLevelDataDic()
        {
            bountyTaskCountChangeUserLevelConfigList.Clear();

            List<UserLevelConfig> userLevelConfigList = Configs.UserLevel.GetConfigList();
            bountyTaskCountChangeUserLevelConfigList.Add(userLevelConfigList[0]);
            for (int i = 0; i < userLevelConfigList.Count; i++)
            {
                UserLevelConfig userLevelConfig = userLevelConfigList[i];
                UserLevelConfig LastChangeUserLevelConfig = bountyTaskCountChangeUserLevelConfigList[^1];
                if (userLevelConfig.BountyTaskCount > LastChangeUserLevelConfig.BountyTaskCount)
                {
                    bountyTaskCountChangeUserLevelConfigList.Add(userLevelConfig);
                }
            }
        }

        #endregion

        #region 悬赏任务列表使用的数据

        public class BountyTaskData
        {
            public bool isLock = false;
            public UserLevelConfig userLevelConfig;

            public BountyTaskInfo bountyTaskInfo;
            public BountyTaskConfig bountyTaskConfig;

            public bool IsStart
            {
                get
                {
                    return bountyTaskInfo.StartTime > 0;
                }
            }

            public bool IsFinish
            {
                get
                {
                    int needTime = bountyTaskConfig.Time;
                    int startTime = bountyTaskInfo.StartTime;
                    int nowTime = (int)DataConvUtil.ServerTime;
                    return startTime + needTime <= nowTime;
                }
            }

            public int IsNotStartInt
            {
                get
                {
                    return IsStart ? 0 : 1;
                }
            }
            public int IsCanGetInt
            {
                get
                {
                    if (IsStart == false) return 0;
                    return IsFinish ? 1 : 0;
                }
            }

        }

        public List<BountyTaskData> GetBountyTaskDataList()
        {
            List<BountyTaskData> bountyTaskDataList = new();
            foreach (BountyTaskInfo bountyTaskInfo in bountyTaskInfoDic.Values)
            {
                BountyTaskData bountyTaskData = new();
                bountyTaskData.bountyTaskInfo = bountyTaskInfo;
                bountyTaskData.bountyTaskConfig = Configs.BountyTask.GetConfig(bountyTaskInfo.Id);
                if (bountyTaskData.bountyTaskConfig == null)
                {
                    Debug.LogWarningFormat("BountyTaskManager , GetBountyTaskDataList , bountyTaskData.bountyTaskConfig == null , bountyTaskInfo.Id = {0}", bountyTaskInfo.Id);
                    continue;
                }
                bountyTaskDataList.Add(bountyTaskData);
            }
            bountyTaskDataList = bountyTaskDataList
                .OrderByDescending(data => data.IsCanGetInt)
                .ThenByDescending(data => data.IsNotStartInt)
                .ThenBy(data => data.bountyTaskConfig.Id)
                .ToList();

            UserLevelConfig nextUserLevelConfig = null;
            foreach (UserLevelConfig userLevelConfig in bountyTaskCountChangeUserLevelConfigList)
            {
                if (userLevelConfig.Id > Player.Level)
                {
                    nextUserLevelConfig = userLevelConfig;
                    break;
                }
            }
            if (nextUserLevelConfig != null)
            {
                BountyTaskData bountyTaskData = new();
                bountyTaskData.isLock = true;
                bountyTaskData.userLevelConfig = nextUserLevelConfig;
                bountyTaskDataList.Add(bountyTaskData);
            }

            return bountyTaskDataList;
        }

        #endregion

        #region 从服务器更新数据

        [HideInInspector] public Dictionary<int, BountyTaskInfo> bountyTaskInfoDic = new();
        [HideInInspector] public int completedCount = 0;
        [HideInInspector] public int boxId = 0;

        public void UpdateBountyTaskInfo(BountyTaskInfoNotify bountyTaskInfoNotify)
        {
            bountyTaskInfoDic.Clear();
            foreach (BountyTaskInfo bountyTaskInfo in bountyTaskInfoNotify.Tasks)
            {
                if (bountyTaskInfoDic.ContainsKey(bountyTaskInfo.Id))
                {
                    Debug.LogWarningFormat("BountyTaskManager , UpdateBountyTaskInfo , bountyTaskInfoDic.ContainsKey(bountyTaskInfo.Id) , bountyTaskInfo.Id = {0}", bountyTaskInfo.Id);
                    continue;
                }
                bountyTaskInfoDic.Add(bountyTaskInfo.Id, bountyTaskInfo);
            }
            boxId = 0;
            foreach (int isGetInt in bountyTaskInfoNotify.CollectedBoxes)
            {
                if (isGetInt > boxId) boxId = isGetInt;
            }
            boxId++;
            completedCount = bountyTaskInfoNotify.CompletedCount;
            EventManager.Instance.Dispatch(EventID.OnBountyTaskDataChange);
        }
        public void AddBountyTaskInfo(BountyTaskInfo bountyTaskInfo)
        {
            if (bountyTaskInfoDic.ContainsKey(bountyTaskInfo.Id) == true)
            {
                Debug.LogWarningFormat("BountyTaskManager , AddBountyTaskInfo , bountyTaskInfoDic.ContainsKey(bountyTaskInfo.Id) == true , bountyTaskInfo.Id = {0}", bountyTaskInfo.Id);
                bountyTaskInfoDic.Remove(bountyTaskInfo.Id);
            }
            bountyTaskInfoDic.Add(bountyTaskInfo.Id, bountyTaskInfo);
            EventManager.Instance.Dispatch(EventID.OnBountyTaskDataChange);
        }
        public void RemoveBountyTaskInfo(int bountyTaskId)
        {
            if (bountyTaskInfoDic.ContainsKey(bountyTaskId) == false)
            {
                Debug.LogWarningFormat("BountyTaskManager , RemoveBountyTaskInfo , bountyTaskInfoDic.ContainsKey(bountyTaskId) == false , bountyTaskId = {0}", bountyTaskId);
                return;
            }
            bountyTaskInfoDic.Remove(bountyTaskId);
            EventManager.Instance.Dispatch(EventID.OnBountyTaskDataChange);
        }

        #endregion

        #region 判断卡片使用状态

        [HideInInspector] public HashSet<int> cardIdUsingSet = new();
        public void RefreshCardUseSet()
        {
            cardIdUsingSet.Clear();
            foreach (BountyTaskInfo bountyTaskInfo in bountyTaskInfoDic.Values)
            {
                if (bountyTaskInfo.StartTime <= 0) continue;
                if (bountyTaskInfo.CardIds.Count <= 0) continue;
                foreach (int cardId in bountyTaskInfo.CardIds)
                {
                    if (cardIdUsingSet.Contains(cardId) == true)
                    {
                        Debug.LogWarningFormat("BountyTaskManager , RefreshCardUseSet , cardIdBountyTaskDic.ContainsKey(cardId) == true , cardId = {0}", cardId);
                        continue;
                    }
                    cardIdUsingSet.Add(cardId);
                }
            }
        }
        /// <summary>
        /// 使用前先手动调用RefreshCardUseSet
        /// </summary>
        public bool IsPlayerCardUsing(int cardId)
        {
            return cardIdUsingSet.Contains(cardId);
        }

        #endregion

        #region 开始任务

        public void StartBountyTask(int taskId, FormationInfo formation, bool isAllLimitPass, Action<StartBountyTaskResponse> callback)
        {
            NetworkManager.Instance.StartBountyTask(taskId, formation, (resp) =>
            {
                if (bountyTaskInfoDic.ContainsKey(taskId) == false)
                {
                    Debug.LogErrorFormat("BountyTaskManager , StartBountyTask , bountyTaskInfoDic.ContainsKey(taskId) == false , taskId = {0}", taskId);
                }
                BountyTaskInfo bountyTaskInfo = bountyTaskInfoDic[taskId];
                bountyTaskInfo.StartTime = (int)DataConvUtil.ServerTime;
                bountyTaskInfo.CardIds.Clear();
                foreach (var item in formation.StarterBoardCardMap.Values)
                {
                    bountyTaskInfo.CardIds.Add(item);
                }
                bountyTaskInfo.Twice = isAllLimitPass;
                EventManager.Instance.Dispatch(EventID.OnBountyTaskDataRefreshList);
                callback?.Invoke(resp);
            });
        }

        #endregion

        public void CheckRedDot()
        {
            if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Task_Bounty, false)) return;
            //任意任务处于未开始和待领奖状态
            var isred = false;
            List<BountyTaskData> tasks = GetBountyTaskDataList();
            foreach (BountyTaskData _task in tasks)
            {
                if (!_task.isLock && (_task.IsStart == false || _task.IsCanGetInt == 1))
                {
                    isred = true;
                    break;
                }
            }

            BountyTaskBoxConfig bountyTaskBoxConfig = Configs.BountyTaskBox.GetConfig(BountyTaskManager.Instance.boxId);
            if (bountyTaskBoxConfig != null)
            {
                int taskCountNow = BountyTaskManager.Instance.completedCount;
                int taskCountNeed = bountyTaskBoxConfig.Count;
                isred |= taskCountNow >= taskCountNeed;
            }


            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Task, "/bounty");
            node.AddValue(isred ? 1 : -1);
        }
    }
}