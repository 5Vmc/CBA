using Babu;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Google.Protobuf.Collections;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.GameItem;

namespace BigBang
{
    public class PlayerAchievementManager
    {
        // 成就排名(暂无)
        public int Rank { get => 0; }
        /// <summary>
        /// Type(客户端分页), (fungroup, data)
        /// </summary>
        public Dictionary<int, List<AchievementGroupData>> AchGroupData;

        // 成就完成事件(参数:成就ID)
        public event Action<int> OnAchievementCompleted;
        // 是否已经初始化
        private bool isInit = false;

        private int pointChangeValue = 0;



        public PlayerAchievementManager()
        {
            AchGroupData = new();
            foreach (var cfg in Configs.Achievement.GetConfigList())
            {
                var achievement = new AchievementData(cfg.Id, 0, 0, 0);
                //类别分组
                if (!AchGroupData.ContainsKey(achievement.Config.Type)) AchGroupData.Add(achievement.Config.Type, new());
                //fungroup分组
                var list = AchGroupData[achievement.Config.Type];
                if (list.Count == 0)
                {
                    var newGroup = new AchievementGroupData();
                    newGroup.funid = cfg.Fungroup;
                    newGroup.list.Add(achievement);
                    list.Add(newGroup);
                }
                else
                {
                    var existGroup = list.FirstOrDefault(p => p.funid == cfg.Fungroup);
                    if (existGroup == null)
                    {
                        existGroup = new AchievementGroupData();
                        existGroup.funid = cfg.Fungroup;
                        existGroup.list.Add(achievement);
                        list.Add(existGroup);
                    }
                    else
                    {
                        existGroup.list.Add(achievement);
                    }

                }
            }
            OnAchievementCompleted += OnCompleted;
        }

        public static int savedId = -1;
        private void OnCompleted(int id)
        {
            if (Player.InBattleAni)
            {
                savedId = id;
            }
            else
            {
                savedId = -1;
                UIController.Instance.OpenWindow<AchievementTipsUI>(new AchievementTipsUIProperties(id));
            }
        }
        public static void CheckSaveId()
        {
            if (savedId == -1) return;
            UIController.Instance.OpenWindow<AchievementTipsUI>(new AchievementTipsUIProperties(savedId));
            savedId = -1;
        }

        public int GetPointChangeValue()
        {
            return pointChangeValue;
        }

        public void ClearPointChangeValue()
        {
            pointChangeValue = 0;
        }

        public AchievementData GetAchievementData(int achievementId)
        {
            AchievementData result = null;
            foreach (var _list in AchGroupData.Values)
            {
                foreach (var gpData in _list)
                {
                    var data = gpData.GetAchievementData(achievementId);
                    if (data.Item1 != -1)
                    {
                        result = data.Item2;
                        break;
                    }
                }

                if (result != null) break;
            }
            return result;
        }

        // 更新成就
        public void UnPack(RepeatedField<Protocol.AchievemtData> datas)
        {
            foreach (var item in datas)
            {
                var config = Configs.Achievement.GetConfig(item.Id);

                AchievementData achData = null;
                foreach (var _list in AchGroupData.Values)
                {
                    var findData = false;
                    foreach (var gpData in _list)
                    {
                        var _data = gpData.GetAchievementData(item.Id);
                        if (_data.Item1 != -1)
                        {
                            gpData.Index = _data.Item1;
                            achData = _data.Item2;
                            findData = true;
                            break;
                        }
                    }
                    if (findData) break;
                }

                //兼容
                if (achData == null) continue;
                var newAchData = new AchievementData(item.Id, item.Current, item.Received, item.Time);
                achData.Received = newAchData.Received;
                if (newAchData.IsComplete)
                {
                    if (isInit && !achData.IsComplete)
                    {
                        pointChangeValue += newAchData.Config.Point;
                        achData.Current = newAchData.Current;
                        //新完成
                        OnAchievementCompleted?.Invoke(item.Id);
                    }
                }
                pointChangeValue += newAchData.Config.Point;
                achData.Current = newAchData.Current;
                achData.time = newAchData.time;
            }
            isInit = true;
        }

        /// <summary>
        /// 当前分数
        /// </summary>
        private Dictionary<int, int> OwnPoint;
        private Dictionary<int, int> TotalPoint;
        /// <summary>
        /// 当前数量
        /// </summary>
        private Dictionary<int, int> CompletedCount;
        /// <summary>
        /// 当前完成数量
        /// </summary>
        private Dictionary<int, int> TotalCount;

        public void CountAllData()
        {
            OwnPoint = new() { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 } };
            TotalPoint = new() { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 } };
            CompletedCount = new() { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 } };
            TotalCount = new() { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 } };
            foreach (var tabDataList in AchGroupData.Values)
            {
                foreach (var dataList in tabDataList)
                {
                    foreach (var data in dataList.list)
                    {
                        if (data.Config.IsHide == 1) continue;
                        if (data.Config.Type > 3) continue;
                        TotalCount[data.Config.Type]++;
                        TotalPoint[data.Config.Type] += data.Config.Point;
                        if (data.Status != 1)
                        {
                            OwnPoint[data.Config.Type] += data.Config.Point;
                            CompletedCount[data.Config.Type]++;
                        }
                    }
                }
            }

            TotalCount[0] = TotalCount[1] + TotalCount[2] + TotalCount[3];
            TotalPoint[0] = TotalPoint[1] + TotalPoint[2] + TotalPoint[3];
            OwnPoint[0] = OwnPoint[1] + OwnPoint[2] + OwnPoint[3];
            CompletedCount[0] = CompletedCount[1] + CompletedCount[2] + CompletedCount[3];
        }

        // 已获得的成就点
        public int GetOwnPoint(AchievementType type)
        {
            return OwnPoint[(int)type];
        }

        // 总成就点
        public int GetTotalPoint(AchievementType type)
        {
            return TotalPoint[(int)type];
        }

        // 获得指定类别的完成个数
        public int GetCompletedCount(AchievementType type)
        {
            return CompletedCount[(int)type];
        }

        // 获得指定类别的成就个数
        public int GetCount(AchievementType type)
        {
            return TotalCount[(int)type];
        }

        /// <summary>
        /// 领取成就奖励
        /// </summary>
        /// <param name="achievementId"></param>
        public void GetAchievementRewards(int achievementId, Action<ReceiveAchievementResponse> callback)
        {
            NetworkManager.Instance.GetAchievementRewards(achievementId, callback);
        }


        private HashSet<int> _hideHashSet = null;
        private HashSet<int> HideHashSet
        {
            get
            {
                if (_hideHashSet == null)
                {
                    _hideHashSet = new();
                    foreach (var item in Configs.Achievement.GetConfigList())
                    {
                        if (item.IsHide == 1 && _hideHashSet.Contains(item.Id) == false) _hideHashSet.Add(item.Id);
                    }
                }
                return _hideHashSet;
            }
        }

        private HashSet<int> _hideGroupHashSet = null;
        private HashSet<int> HideGroupHashSet
        {
            get
            {
                if (_hideGroupHashSet == null)
                {
                    _hideGroupHashSet = new();
                    foreach (var item in Configs.Achievement.GetConfigList())
                    {
                        if (item.IsHide == 1 && _hideGroupHashSet.Contains(item.Fungroup) == false) _hideGroupHashSet.Add(item.Fungroup);
                    }
                }
                return _hideGroupHashSet;
            }
        }

        /// <summary>
        /// 成就小红点检查，为3个页签分别设置小红点。
        /// </summary>
        public void CheckAchievementRedDot()
        {

            for (var index = 1; index <= 3; index++)
            {
                var tmpList = AchGroupData[index];

                var redNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Achieve, "/" + index.ToString());
                //Status 会检查 Current>target，Current会根据ClientCheck来决定用服务端数据还是用客户端的实时数据来判断。
                //这个算法其实还可以再优化，检查每组任务当前的那个就可以了，有空再搞。==> 每组当前的那个，如果是服务端的，那就是
                var isRed = tmpList.Any<AchievementGroupData>(p => p.CurrentData.Status == 2 && HideGroupHashSet.Contains(p.funid) == false);
                //var isRed = tmpList.Any<AchievementGroupData>(p => p.CurrentData.Status == 2);
                redNode.AddValue(isRed ? 1 : -1);
            }
        }

        /// <summary>
        /// 荣誉小红点检查，为2个页签分别设置小红点。
        /// </summary>
        public void CheckHonourRedDot()
        {

            for (var index = 11; index <= 12; index++)
            {
                var redNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Honour, "/" + index.ToString());
                bool isFind = false;
                List<AchievementGroupData> achievementGroupDataList = AchGroupData[index];
                foreach (AchievementGroupData achievementGroupData in achievementGroupDataList)
                {
                    foreach (var item in achievementGroupData.list)
                    {
                        if (item.Status == 2 && item.Config.IsHide == 0)
                        {
                            isFind = true;
                            break;
                        }
                    }
                    if (isFind) break;
                }
                redNode.AddValue(isFind ? 1 : -1);
            }
        }
    }
}