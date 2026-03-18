using System;
using System.Collections.Generic;
using System.Linq;

using GameConfig;
using GameConfig.Config;
using Protocol;
using UnityEngine;
using Babu;

namespace BigBang
{

    public static class GameBroadcastList
    {
        // 播报列表(已分组)
        private static List<IGrouping<int, GameBroadcastConfig>> broadcastList;

        public static string GetMessage(int eventID, System.Random random)
        {
            broadcastList ??= Configs.GameBroadcast.GetConfigList().GroupBy(item => item.EventId).ToList();
            // 当前事件的所有播报消息
            var messages = broadcastList.FirstOrDefault(broadcast => broadcast.Key == eventID);
            if (messages == null) return null;
            // 按权重随机选择
            var msg = messages.WeightedRandom(item => item.Weight, random);
            return msg.Content;
        }
    }

    public class FightData
    {
        public string FightId { get; set; }
        public FightType FightType { get; set; }
        public int BeginTime { get; set; }
        public FightTeam HomeTeam { get; set; } = new FightTeam();
        public FightTeam AwayTeam { get; set; } = new FightTeam();

        public FightTeam MyTeam
        {
            get
            {
                return HomeTeam.TeamId == Player.GbId ? HomeTeam : AwayTeam;
            }
        }

        public string Title { get; set; }

        public bool IsReportDataReady { get; set; } = false;
        public FightReportData FightReportData { get; set; }

        private int _curFrame = 1;
        private int _maxFrame = 1;
        // 需要拉取帧数据的帧
        private int _fetchFrame = 99999;

        private Dictionary<int, FightFrameData> _fightFrameDatas = new Dictionary<int, FightFrameData>();

        private FightEventTrigger _eventTrigger = new FightEventTrigger();
        private FightMessageTrigger _messageTrigger = new FightMessageTrigger();
        private int lastRecalculateFrame = -1;

        public FightData(string fightId)
        {
            FightId = fightId;

            HomeTeam.Init(this);
            AwayTeam.Init(this);
            _eventTrigger.Init(this);
            _messageTrigger.Init(this);
        }

        public void Clear()
        {
            _curFrame = 1;
            _maxFrame = 1;
            _fightFrameDatas.Clear();
        }
        public FightTeamPerformanceData GetMineTeamReport()
        {
            if (FightReportData == null) return null;
            if (HomeTeam.TeamId == Player.GbId)
            {
                return FightReportData.HomeTeam;
            }
            else if (AwayTeam.TeamId == Player.GbId)
            {
                return FightReportData.AwayTeam; ;
            }

            return null;
        }
        public void BeginFetch()
        {
            NetworkManager.Instance.FetchFightFrames(FightId, _curFrame);
        }

        public void NotifyRecalculateFrame(int recalculateFrame)
        {
            Debug.Log($"NotifyRecalculateFrame max frame = {_maxFrame}, cur frame = {_curFrame}, recalculate frame = {recalculateFrame}");
            if (recalculateFrame == -1) return;
            lastRecalculateFrame = recalculateFrame;
            if (_maxFrame <= recalculateFrame)
            {
                return;
            }

            if (_curFrame > recalculateFrame)
            {
                // todo 这里需要重新刷新
            }

            SetMaxFrame(recalculateFrame - 1);
        }

        public bool CanCancelSetFormation()
        {
            return lastRecalculateFrame - _curFrame > 10;
        }

        public bool IsFormationSetted()
        {
            return _curFrame > lastRecalculateFrame;
        }
        
        public void UpdateFightFrameData(FightFrameDataNotify fightData)
        {
            var maxFrame = -1;
            foreach (var data in fightData.FightFramesData)
            {
                Debug.Log("add frame " + data.Frame.ToString());
                _fightFrameDatas[data.Frame] = data;
                if (data.Frame > maxFrame)
                {
                    maxFrame = data.Frame;
                }
            }

            SetMaxFrame(maxFrame);
        }

        private void SetMaxFrame(int frame)
        {
            _maxFrame = frame;
            //SetFetchFrame(_maxFrame - FightConst.FETCH_FRAMES_DATA_TICK_COUNT);
        }
        private void SetFetchFrame(int frame)
        {
            _fetchFrame = frame;
        }

        public void UnPackResult(FightReportData fightResult)
        {
            FightReportData = fightResult;
        }

        private void CheckFetchFrame()
        {
            if (_curFrame >= _fetchFrame)
            {
                // 10 帧以后还没回来就再拉一次
                SetFetchFrame(_fetchFrame + 10);
                NetworkManager.Instance.FetchFightFrames(FightId, _maxFrame + 1);
            }
        }

        public FightFrameData WatchCurFrameData()
        {
            CheckFetchFrame();
            var frame = GetFrameData(_curFrame);
            if (frame != null)
            {
                DoFrame(frame);
            }

            return frame;
        }

        public FightFrameData GetFrameData(int index)
        {
            if (_fightFrameDatas.TryGetValue(index, out var frame))
            {
                return frame;
            }

            return null;
        }

        public int GetCurFrameIndex()
        {
            return _curFrame;
        }

        public void WatchFightBegin(Action callback)
        {
            NetworkManager.Instance.GetWatchBeginFrame(FightId, response =>
            {
                _curFrame = response.BeginFrame;
                CalculateBeginStatistics();

                callback?.Invoke();
            });
        }

        public void GoToNextFrame()
        {
            ++_curFrame;
        }

        public void UpdateFightBeginData(FightBeginDataNotify data)
        {
            Title = data.FightTitle;
            FightType = (FightType)data.FightType;
            BeginTime = data.BeginTime;
            HomeTeam.UnPackBeginInfo(data.HomeTeam);
            AwayTeam.UnPackBeginInfo(data.AwayTeam);
            HomeTeam.SetFormationType(FightType);
            AwayTeam.SetFormationType(FightType);
        }

        private void CalculateBeginStatistics()
        {
            for (int i = 0; i < _curFrame; i++)
            {
                Debug.Log("未观看的比赛帧:" + i);
                var frame = GetFrameData(i);
                if (frame == null) continue;
                DoFrame(frame, true);
            }
        }

        private void DoFrame(FightFrameData frame, bool watchBefore = false)
        {
            DoReplacement(frame, watchBefore);
            DoCheckBallController(frame);
            DoFrameEvent(frame);
        }

        private void DoCheckBallController(FightFrameData frame)
        {
            
        }

        public FightTeam GetTeamBySide(int side)
        {
            return null;
        }

        public FightCard GetCardByRoleID(int roleId)
        {
            return null;
        }

        private void DoFrameEvent(FightFrameData frame)
        {
            if (frame == null) return;
            foreach (var eventData in frame.Event)
            {
                _eventTrigger.OnTrigger(frame.Frame, eventData);
                _messageTrigger.OnTrigger(eventData);
            }
        }

        private void DoReplacement(FightFrameData frame, bool watchBefore)
        {
            if (frame == null) return;
            if (frame.LeftTeam.ReplacementData != null)
            {
                HomeTeam.DoReplacement(frame.LeftTeam.ReplacementData, watchBefore);
                if(HomeTeam == MyTeam)
                {
                    EventManager.Instance.Dispatch(EventID.OnFormationSetted);
                }
            }

            if (frame.RightTeam.ReplacementData != null)
            {
                AwayTeam.DoReplacement(frame.RightTeam.ReplacementData, watchBefore);
                if (AwayTeam == MyTeam)
                {
                    EventManager.Instance.Dispatch(EventID.OnFormationSetted);
                }
            }
        }
    }
}