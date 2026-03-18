using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Google.Protobuf.Collections;
using Protocol;
using UnityEngine;
using Utils;

namespace BigBang
{
    /// <summary>
    /// 百分大战的阶段
    /// 与服务器消息“message GetHundredCourseResponse”中的“int
    /// </summary>
    public enum HundredProgress
    {
        /// <summary> 活动未开启（服务器在跑定时任务，或服务器设置了不开这个活动） </summary>
        NotOpen = 0,
        /// <summary>等待报名（结算期）</summary>
        Wait = 1,
        /// <summary>报名期</summary>
        Sign = 2,
        /// <summary>入围赛</summary>
        Fight1 = 3,
        /// <summary>淘汰赛</summary>
        Fight2 = 4,
        /// <summary>冠军赛</summary>
        Fight3 = 5,
    }

    /// <summary>
    /// 百分大战奖励类型
    /// 与配置表“百分大战/百分奖励.xlsx”中的“type”字段对应
    /// </summary>
    public enum HundredRewardType
    {
        /// <summary>入围赛</summary>
        Fight1 = 1,
        /// <summary>淘汰赛</summary>
        Fight2 = 2,
        /// <summary>冠军赛</summary>
        Fight3 = 3,
    }

    /// <summary>
    /// 百分大战的管理类
    /// </summary>
    public class HundredManager : BabuSingleton<HundredManager>
    {
        private readonly string NotOpenStr = "百分大战";
        private readonly string WaitStr = "休赛期";
        private readonly string SignStr = "报名期";
        private readonly string Fight1Str = "入围赛";
        private readonly string Fight2Str = "淘汰赛";
        private readonly string Fight3Str = "冠军赛";
        public string GetStageName(HundredProgress hundredProgress)
        {
            switch (hundredProgress)
            {
                case HundredProgress.NotOpen: return NotOpenStr;
                case HundredProgress.Wait: return WaitStr;
                case HundredProgress.Sign: return SignStr;
                case HundredProgress.Fight1: return Fight1Str;
                case HundredProgress.Fight2: return Fight2Str;
                case HundredProgress.Fight3: return Fight3Str;
            }
            return NotOpenStr;
        }
        public bool isMeInNowCourse()
        {
            HundredProgress hundredProgress = (HundredProgress)HundredManager.Instance.nowCourse.Stage;
            switch (hundredProgress)
            {
                case HundredProgress.Wait: return true;
                case HundredProgress.Sign: return true;
                case HundredProgress.Fight1: return HundredManager.Instance.nowCourse.MyZoneId > 0;
                case HundredProgress.Fight2:
                case HundredProgress.Fight3:
                    {
                        if (HundredManager.Instance.nowCourse.MyZoneId <= 0) return false;
                        return nowCourse.LeagueCourseItemList.FirstOrDefault((item) =>
                        {
                            if (item.AwayTeam != null && item.AwayTeam.TeamId == Player.GbId) return true;
                            if (item.HomeTeam != null && item.HomeTeam.TeamId == Player.GbId) return true;
                            return false;
                        }) != null;
                    }
            }
            return false;
        }
        public int GetMyMaxRound(bool isFihgt2)
        {
            int maxRound = isFihgt2 ? 6 : 3;
            int targetRound = 0;
            for (int i = 1; i <= maxRound; i++)
            {
                bool isFind = nowCourse.LeagueCourseItemList.FirstOrDefault((item) =>
                {
                    if (item.Round != i) return false;
                    if (item.AwayTeam != null && item.AwayTeam.TeamId == Player.GbId) return true;
                    if (item.HomeTeam != null && item.HomeTeam.TeamId == Player.GbId) return true;
                    return false;
                }) != null;
                if (isFind)
                {
                    targetRound = i;
                }
                else
                {
                    break;
                }
            }
            if (targetRound == maxRound)
            {
                bool isFind = nowCourse.LeagueCourseItemList.FirstOrDefault((item) =>
                {
                    if (item.Round != maxRound) return false;
                    if (item.AwayTeam != null && item.AwayTeam.TeamId == Player.GbId && item.AwayGoal > item.HomeGoal) return true;
                    if (item.HomeTeam != null && item.HomeTeam.TeamId == Player.GbId && item.AwayGoal < item.HomeGoal) return true;
                    return false;
                }) != null;
                if (isFind) targetRound = maxRound + 1;
            }
            return targetRound;
        }
        public int GetNowMaxRound(GetHundredCourseResponse course)
        {
            if (course == null || course.LeagueCourseItemList.Count <= 0) return 0;
            LeagueCourseItemData leagueCourseItemData = course.LeagueCourseItemList.FirstOrDefault((item) =>
            {
                if (item.AwayTeam != null && item.HomeTeam != null && item.AwayGoal <= 0 && item.HomeGoal <= 0) return true;
                return false;
            });
            if (leagueCourseItemData == null) return 0;
            return leagueCourseItemData.Round;
        }

        private readonly List<string> Fight2RoundPlayerTitleStrList = new()
        {
            "64强",
            "32强",
            "16强",
            "8强",
            "4强",
            "双强",
            "冠军",
        };
        private readonly List<string> Fight3RoundPlayerTitleStrList = new()
        {
            "8强",
            "4强",
            "双强",
            "冠军",
        };
        public string GetRoundPlayerTitle(bool isFihgt2, int round)
        {
            if (isFihgt2)
            {
                if (round > 0 && round < Fight2RoundPlayerTitleStrList.Count)
                {
                    return Fight2RoundPlayerTitleStrList[round - 1];
                }
            }
            else
            {
                if (round > 0 && round < Fight3RoundPlayerTitleStrList.Count)
                {
                    return Fight3RoundPlayerTitleStrList[round - 1];
                }
            }
            return "";
        }
        private readonly List<string> Fight2RoundMatchTitleStrList = new()
        {
            "",
            "32强",
            "16强",
            "8强",
            "4强",
            "半决赛",
            "决赛",
        };
        private readonly List<string> Fight3RoundMatchTitleStrList = new()
        {
            "",
            "4强",
            "半决赛",
            "决赛",
        };
        public string GetRoundMatchTitle(bool isFihgt2, int round)
        {
            if (isFihgt2)
            {
                if (round > 0 && round < Fight2RoundMatchTitleStrList.Count)
                {
                    return Fight2RoundMatchTitleStrList[round];
                }
            }
            else
            {
                if (round > 0 && round < Fight3RoundMatchTitleStrList.Count)
                {
                    return Fight3RoundMatchTitleStrList[round];
                }
            }
            return "";
        }

        public void GetFight1EndAndWin(GetHundredCourseResponse serverData, out bool isEnd, out bool isDown)
        {
            int totalNum = serverData.ZoneSignTeamCount[serverData.MyZoneId - 1];
            int outNum = serverData.ZoneOutTeamCount[serverData.MyZoneId - 1];
            int leftNum = totalNum - outNum;
            isEnd = leftNum <= 64;

            isDown = false;
            int loseCount = 0;
            for (int i = 0; i < serverData.LeagueCourseItemList.Count; i++)
            {
                bool isLose = IsFightLose(serverData.LeagueCourseItemList[i]);
                if (isLose) loseCount++;
            }
            if (loseCount >= 3) isDown = true;
            isEnd = isDown || isEnd;
        }


        //未知下一场战斗时刷新的时间
        public const float refreshNextBattleTime = 3 * 60f;

        //淘汰赛看那个赛区
        [HideInInspector] private int _dropdownValue = -1;
        public int dropdownValue
        {
            get
            {
                //Debug.Log("dropdownValue , get value = " + _dropdownValue);
                return _dropdownValue;
            }
            set
            {
                Debug.Log("dropdownValue , set value = " + value);
                _dropdownValue = value;
            }
        }

        private bool isInited = false;
        /// <summary>
        /// 配置表加载完成后，对数据进行预处理
        /// </summary>
        public void InitOnce(bool forceInit = true)
        {
            Clear();
            if (isInited && !forceInit) return;
            isInited = true;
        }

        public void Clear()
        {
            MyZoneId = 0;
            nowCourse = null;
            courseDic.Clear();
            nowCourseDateTime = DateTime.MinValue;
            HundredFightDic.Clear();
            hundredRedDotTimer?.Cancel();
            hundredRedDotTimer = null;
            historyDic.Clear();
            guessCourseInfo = null;
            guessSupportInfo = null;
        }

        public void OpenHundredHome()
        {
            HundredManager.Instance.GetCourse(0, true, (GetHundredCourseResponse getHundredCourseResponse) =>
            {
                if ((HundredProgress)getHundredCourseResponse.Stage == HundredProgress.NotOpen)
                {
                    Tips.PopTips("活动未开启");
                    return;
                }
                UIController.Instance.ShowPanel<HundredHomeUI>(new HundredHomeUIProperties(true));
            });
        }

        public int MyZoneId = 0;
        public GetHundredCourseResponse nowCourse = null;
        public Dictionary<int, GetHundredCourseResponse> courseDic = new();
        public DateTime nowCourseDateTime;
        public void GetCourse(int ZoneId, bool needRefresh, System.Action<GetHundredCourseResponse> callback = null)
        {
            Debug.Log("HundredManager ,GetCourse , ZoneId = " + ZoneId + " , needRefresh = " + needRefresh);
            if (!needRefresh && courseDic.ContainsKey(ZoneId))
            {
                callback?.Invoke(courseDic[ZoneId]);
                return;
            }
            NetworkManager.Instance.GetHundredCourse(ZoneId, (GetHundredCourseResponse getHundredCourseResponse) =>
            {
                Debug.Log("HundredManager , GetCourse , Stage = " + getHundredCourseResponse.Stage);
                Debug.Log("HundredManager , GetCourse , Count = " + getHundredCourseResponse.LeagueCourseItemList.Count);
                Debug.Log("HundredManager , GetCourse , LeftTime = " + TimeUtils.FormatLeftTime((int)(getHundredCourseResponse.StageEndTime - Utils.DataConvUtil.ServerTime)));

                if (ZoneId == 0)
                {
                    nowCourseDateTime = DataConvUtil.ServerDateTime;
                    MyZoneId = getHundredCourseResponse.MyZoneId;
                    nowCourse = getHundredCourseResponse;
                    EventManager.Instance.Dispatch(EventID.OnHundredGetMineInfo);
                }
                else
                {
                    if (nowCourse != null) nowCourse.HasSupportReward = getHundredCourseResponse.HasSupportReward;
                }
                CheckHundredRedDot();
                EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
                courseDic[getHundredCourseResponse.PlayoffZoneId] = getHundredCourseResponse;
                if (ZoneId == 0) courseDic[0] = getHundredCourseResponse;
                callback?.Invoke(getHundredCourseResponse);
                return;
            });
        }

        public Dictionary<string, FightInfo> HundredFightDic = new();
        public void GetFight(string fightId, System.Action<FightInfo> callBack)
        {
            if (HundredFightDic.ContainsKey(fightId))
            {
                callBack?.Invoke(HundredFightDic[fightId]);
                return;
            }
            NetworkManager.Instance.GetFightReport(fightId, response =>
            {
                HundredFightDic[response.FightId] = response;
                callBack?.Invoke(response);
            });
        }
        public void GetFightNoTip(string fightId, System.Action<FightInfo> callBack)
        {
            if (HundredFightDic.ContainsKey(fightId))
            {
                callBack?.Invoke(HundredFightDic[fightId]);
                return;
            }
            NetworkManager.Instance.GetFightReport(fightId, null, true, response =>
            {
                HundredFightDic[response.FightId] = response;
                callBack?.Invoke(response);
            });
        }

        public void GetFormationLeftTime(GetHundredCourseResponse serverData, int endRound, out HundredFormationUI.HFType hfType, out int leftTime)
        {
            if (serverData == null || serverData.LeagueCourseItemList == null || serverData.LeagueCourseItemList.Count <= 0)
            {
                hfType = HundredFormationUI.HFType.Lock;
                leftTime = -1;
                return;
            }

            //自己输了不能布阵
            bool isOut = serverData.IsOut;
            if (isOut)
            {
                hfType = HundredFormationUI.HFType.Lock;
                leftTime = -1;
                return;
            }


            bool isWinner = false;
            var data1 = serverData.LeagueCourseItemList.Where(item => item.Round == endRound).ToList();
            if (data1 != null && data1.Count > 0)
            {
                LeagueCourseItemData leagueCourseItemData = data1[0];
                if (leagueCourseItemData.AwayTeam != null && leagueCourseItemData.HomeTeam != null && leagueCourseItemData.AwayGoal > -1 && leagueCourseItemData.HomeGoal > -1)
                {
                    if (leagueCourseItemData.AwayTeam.TeamId == Player.GbId && leagueCourseItemData.AwayGoal > leagueCourseItemData.HomeGoal) isWinner = true;
                    if (leagueCourseItemData.HomeTeam.TeamId == Player.GbId && leagueCourseItemData.AwayGoal < leagueCourseItemData.HomeGoal) isWinner = true;
                }
            }
            if (isWinner)
            {
                switch ((HundredProgress)serverData.Stage)
                {
                    case HundredProgress.Fight2:
                        {
                            hfType = HundredFormationUI.HFType.Open;//淘汰赛拿到冠军之后可以布阵
                            leftTime = -1;
                        }
                        break;
                    default:
                        {
                            hfType = HundredFormationUI.HFType.Lock;//冠军赛拿到冠军之后不能布阵
                            leftTime = -1;
                        }
                        break;
                }
                return;
            }

            //找自己的最近的下一场战斗
            long minTime = long.MaxValue;
            bool isFind = false;
            foreach (LeagueCourseItemData leagueCourseItemData in serverData.LeagueCourseItemList)
            {
                bool isFindAway = leagueCourseItemData.AwayTeam != null && leagueCourseItemData.AwayTeam.TeamId == Player.GbId && leagueCourseItemData.Time > 0;
                bool isFindHome = leagueCourseItemData.HomeTeam != null && leagueCourseItemData.HomeTeam.TeamId == Player.GbId && leagueCourseItemData.Time > 0;
                if (isFindAway || isFindHome)
                {
                    if (leagueCourseItemData.Time > Utils.DataConvUtil.ServerTime && leagueCourseItemData.Time < minTime)
                    {
                        minTime = leagueCourseItemData.Time;
                        isFind = true;
                    }
                }
            }
            if (isFind)
            {
                leftTime = (int)(minTime - Utils.DataConvUtil.ServerTime) - 30 * 60;
                if (leftTime > 0)//最近战斗时间大于半小时，剩余时间布阵
                {
                    hfType = HundredFormationUI.HFType.Limit;
                }
                else//最近战斗时间小于半小时，不布阵
                {
                    hfType = HundredFormationUI.HFType.Lock;
                }
                return;
            }

            //没有最近的战斗，服务器还没生成下一场战斗时间，可以布阵
            hfType = HundredFormationUI.HFType.Open;
            leftTime = -1;
        }

        public void GetFormationLeftTimeFight1(GetHundredCourseResponse serverData, out HundredFormationUI.HFType hfType, out int leftTime)
        {
            if (serverData == null)
            {
                hfType = HundredFormationUI.HFType.Lock;
                leftTime = -1;
                return;
            }

            //自己输了不能布阵
            bool isOut = serverData.IsOut;
            if (isOut)
            {
                hfType = HundredFormationUI.HFType.Lock;
                leftTime = -1;
                return;
            }

            HundredManager.Instance.GetFight1EndAndWin(HundredManager.Instance.nowCourse, out bool isEnd, out bool isDown);

            bool isWinner = false;
            if (isEnd && !isDown)
            {
                isWinner = true;
            }
            if (isWinner)
            {
                hfType = HundredFormationUI.HFType.Open;//入围赛拿到冠军之后可以布阵
                leftTime = -1;
                return;
            }

            //找自己的最近的下一场战斗
            long minTime = long.MaxValue;
            bool isFind = false;
            foreach (LeagueCourseItemData leagueCourseItemData in serverData.LeagueCourseItemList)
            {
                bool isFindAway = leagueCourseItemData.AwayTeam != null && leagueCourseItemData.AwayTeam.TeamId == Player.GbId && leagueCourseItemData.Time > 0;
                bool isFindHome = leagueCourseItemData.HomeTeam != null && leagueCourseItemData.HomeTeam.TeamId == Player.GbId && leagueCourseItemData.Time > 0;
                if (isFindAway || isFindHome)
                {
                    if (leagueCourseItemData.Time > Utils.DataConvUtil.ServerTime && leagueCourseItemData.Time < minTime)
                    {
                        minTime = leagueCourseItemData.Time;
                        isFind = true;
                    }
                }
            }
            if (isFind)
            {
                leftTime = (int)(minTime - Utils.DataConvUtil.ServerTime) - 30 * 60;
                if (leftTime > 0)//最近战斗时间大于半小时，剩余时间布阵
                {
                    hfType = HundredFormationUI.HFType.Limit;
                }
                else//最近战斗时间小于半小时，不布阵
                {
                    hfType = HundredFormationUI.HFType.Lock;
                }
                return;
            }

            //没有最近的战斗，服务器还没生成下一场战斗时间，可以布阵
            hfType = HundredFormationUI.HFType.Open;
            leftTime = -1;
        }

        public bool IsFightLose(LeagueCourseItemData leagueCourseItemData)
        {
            bool isLose = false;
            if (leagueCourseItemData.HomeTeam == null || leagueCourseItemData.AwayTeam == null) return false;
            if (leagueCourseItemData.HomeGoal < 0 || leagueCourseItemData.AwayGoal < 0) return false;
            if (leagueCourseItemData.HomeTeam.TeamId == Player.GbId && leagueCourseItemData.HomeGoal < leagueCourseItemData.AwayGoal) isLose = true;
            if (leagueCourseItemData.AwayTeam.TeamId == Player.GbId && leagueCourseItemData.HomeGoal > leagueCourseItemData.AwayGoal) isLose = true;
            return isLose;
        }
        public bool IsFightWin(LeagueCourseItemData leagueCourseItemData)
        {
            bool isWin = false;
            if (leagueCourseItemData.HomeTeam == null || leagueCourseItemData.AwayTeam == null) return false;
            if (leagueCourseItemData.HomeGoal < 0 || leagueCourseItemData.AwayGoal < 0) return false;
            if (leagueCourseItemData.HomeTeam.TeamId == Player.GbId && leagueCourseItemData.HomeGoal > leagueCourseItemData.AwayGoal) isWin = true;
            if (leagueCourseItemData.AwayTeam.TeamId == Player.GbId && leagueCourseItemData.HomeGoal < leagueCourseItemData.AwayGoal) isWin = true;
            return isWin;
        }


        public bool IsMeInServerData(GetHundredCourseResponse serverData)
        {
            bool isFindSelf = false;
            foreach (LeagueCourseItemData leagueCourseItemData in serverData.LeagueCourseItemList)
            {
                if (leagueCourseItemData.AwayTeam != null && leagueCourseItemData.AwayTeam.TeamId == Player.GbId) isFindSelf = true;
                if (leagueCourseItemData.HomeTeam != null && leagueCourseItemData.HomeTeam.TeamId == Player.GbId) isFindSelf = true;
                if (isFindSelf) break;
            }
            return isFindSelf;
        }
        public bool IsMeFail(GetHundredCourseResponse serverData)
        {
            bool isMeFail = false;
            foreach (LeagueCourseItemData leagueCourseItemData in serverData.LeagueCourseItemList)
            {
                if (leagueCourseItemData == null || leagueCourseItemData.AwayTeam == null || leagueCourseItemData.HomeTeam == null || leagueCourseItemData.AwayGoal < 0 || leagueCourseItemData.HomeGoal < 0) continue;
                if (leagueCourseItemData.AwayTeam.TeamId == Player.GbId && leagueCourseItemData.AwayGoal < leagueCourseItemData.HomeGoal) isMeFail = true;
                if (leagueCourseItemData.HomeTeam.TeamId == Player.GbId && leagueCourseItemData.AwayGoal > leagueCourseItemData.HomeGoal) isMeFail = true;
                if (isMeFail) break;
            }
            return isMeFail;
        }
        public bool IsWinnerCome(GetHundredCourseResponse serverData, int endRound)
        {
            bool isWinnerCome = false;
            var data1 = serverData.LeagueCourseItemList.Where(item => item.Round == endRound).ToList();
            if (data1 != null && data1.Count > 0)
            {
                LeagueCourseItemData leagueCourseItemData = data1[0];
                if (leagueCourseItemData.AwayTeam != null && leagueCourseItemData.HomeTeam != null && leagueCourseItemData.AwayGoal > -1 && leagueCourseItemData.HomeGoal > -1)
                {
                    isWinnerCome = true;
                }
            }
            return isWinnerCome;
        }

        public void SetTitle(ImageFont titleImageFont, GetHundredCourseResponse serverData)
        {
            if (serverData == null || serverData.SeasonTitles.Count <= 0 || serverData.SeasonId - 1 < 0 || serverData.SeasonId - 1 >= serverData.SeasonTitles.Count)
            {
                titleImageFont.text = "";
                Debug.LogWarning("HundredHomeUISignPad , SetTitle , serverData == null");
                return;
            }
            GetYearAndSession(serverData.SeasonTitles[serverData.SeasonId - 1], out int year, out int session);
            if (year == 0 || session == 0)
            {
                titleImageFont.text = "";
                return;
            }
            titleImageFont.text = "{0}第{1}届".SafeFormat(year, session.ToChinese());
        }
        public bool GetYearAndSession(string allStr, out int year, out int session)
        {
            if (string.IsNullOrWhiteSpace(allStr))
            {
                Debug.LogWarning("HundredHomeUISignPad , GetYearAndSession , allStr == null");
                year = 0;
                session = 0;
                return false;
            }
            string[] strArr = allStr.Split(',');
            if (strArr.Length != 2)
            {
                Debug.LogWarning("HundredHomeUISignPad , GetYearAndSession , strArr.Length != 2 , allStr = " + allStr);
                year = 0;
                session = 0;
                return false;
            }
            int.TryParse(strArr[0], out year);
            int.TryParse(strArr[1], out session);
            if (year == 0 || session == 0)
            {
                Debug.LogWarning("HundredHomeUISignPad , GetYearAndSession , year == 0 || session == 0 , allStr = " + allStr);
                return false;
            }
            return true;
        }

        public void CheckHundredRedDot()
        {
            {
                hundredRedDotTimer?.Cancel();
                hundredRedDotTimer = null;

                bool isRed = false;
                RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Hundred, "/Sign");

                System.DateTime serverDT = Utils.DataConvUtil.ServerDateTime;
                int dayOfWeek = (int)serverDT.DayOfWeek;
                bool isMonday = dayOfWeek == 1;
                bool isSign = MyZoneId != 0;
                bool isSignStage = nowCourse != null && (HundredProgress)nowCourse.Stage == HundredProgress.Sign;
                bool isOpen = TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Hundred, false);
                isRed |= (isMonday && !isSign && isOpen && isSignStage);

                redDotNode.AddValue(isRed ? 1 : -1);
                Debug.Log("CheckHundredRedDot , isRed = " + isRed);

                if (isRed)
                {
                    System.DateTime dayEndTime = serverDT.Date.AddDays(1);
                    int leftTime = nowCourse.StageEndTime - (int)Utils.DataConvUtil.ServerTime;
                    hundredRedDotTimer = UnityTimer.Timer.Register(this.gameObject, leftTime, () =>
                    {
                        redDotNode.AddValue(-1);
                        hundredRedDotTimer?.Cancel();
                        hundredRedDotTimer = null;
                        EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
                    });
                }
                if (!isRed && nowCourse != null && (HundredProgress)nowCourse.Stage == HundredProgress.Wait)
                {
                    int leftTime = nowCourse.StageEndTime - (int)Utils.DataConvUtil.ServerTime;
                    hundredRedDotTimer = UnityTimer.Timer.Register(this.gameObject, Mathf.Min(60 * 10, leftTime + 60 * 10), () =>
                    {
                        CheckHundredRedDotWhenEnterGame();
                    });
                }
            }
            {
                bool isRed = false;
                RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Hundred, "/Guess/Reward");
                isRed = nowCourse != null && nowCourse.HasSupportReward && ((HundredProgress)nowCourse.Stage == HundredProgress.Fight2 || (HundredProgress)nowCourse.Stage == HundredProgress.Fight3);
                redDotNode.AddValue(isRed ? 1 : -1);
            }
        }
        private UnityTimer.Timer hundredRedDotTimer = null;

        public void CheckHundredRedDotWhenEnterGame()
        {
            hundredRedDotTimer?.Cancel();
            hundredRedDotTimer = null;

            System.DateTime serverDT = Utils.DataConvUtil.ServerDateTime;
            int dayOfWeek = (int)serverDT.DayOfWeek;
            bool isMonday = dayOfWeek == 1;

            if (isMonday == false)
            {
                RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Hundred, "/Sign");
                redDotNode.AddValue(-1);
                //return;
            }
            if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Hundred, false))
            {
                RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Hundred, "/Sign");
                redDotNode.AddValue(-1);
                //return;
            }
            {
                RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Hundred, "/Guess/Reward");
                redDotNode.AddValue(-1);
            }

            GetCourse(0, true, (GetHundredCourseResponse getHundredCourseResponse) =>
            {
                EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
            });
        }

        private Dictionary<string, List<CourseTeamData>> historyDic = new();
        public void GetHistory(string seasonTitle, Action<List<CourseTeamData>> callback = null)
        {
            if (historyDic.ContainsKey(seasonTitle))
            {
                callback.Invoke(historyDic[seasonTitle]);
                return;
            }
            NetworkManager.Instance.GetHundredHof(seasonTitle, (GetHundredHofResponse getHundredHofResponse) =>
            {
                List<CourseTeamData> dataList = getHundredHofResponse.Ranks.ToList();
                if (historyDic.ContainsKey(getHundredHofResponse.SeasonTitle) == false)
                {
                    historyDic.Add(getHundredHofResponse.SeasonTitle, dataList);
                }
                callback.Invoke(dataList);
            });
        }

        public bool hasSupportReward
        {
            get
            {
                if (nowCourse == null) return false;
                return nowCourse.HasSupportReward;
            }
        }

        /// <summary>
        /// 是否需要应援消耗应援物的提示，记录在内存中
        /// </summary>
        public bool isNeedAlertSupport = true;

        public GetHundredCourseResponse guessCourseInfo = null;
        public GetHundredSupportResponse guessSupportInfo = null;
        public void GetSupportServerInfo(Action successCallback = null)
        {
            switch ((HundredProgress)nowCourse.Stage)
            {
                case HundredProgress.Fight2:
                    {
                        NetworkManager.Instance.GetHundredSupport((GetHundredSupportResponse getHundredSupportResponse) =>
                        {
                            guessSupportInfo = getHundredSupportResponse;
                            bool isSupported = nowCourse.SupportZone >= 1 && nowCourse.SupportZone <= 8;
                            int zoneId = isSupported ? nowCourse.SupportZone : dropdownValue + 1;
                            HundredManager.Instance.GetCourse(zoneId, true, (GetHundredCourseResponse getHundredCourseResponse) =>
                            {
                                guessCourseInfo = getHundredCourseResponse;
                                successCallback?.Invoke();
                            });
                        });
                    }
                    break;
                default:
                    {
                        NetworkManager.Instance.GetHundredSupport((GetHundredSupportResponse getHundredSupportResponse) =>
                        {
                            guessSupportInfo = getHundredSupportResponse;
                            HundredManager.Instance.GetCourse(0, true, (GetHundredCourseResponse getHundredCourseResponse) =>
                            {
                                guessCourseInfo = getHundredCourseResponse;
                                successCallback?.Invoke();
                            });
                        });
                    }
                    break;
            }
        }


        public bool IsSupported(LeagueCourseItemData leagueCourseItemData, bool isAway)
        {
            int zoneId = 0;
            if ((HundredProgress)guessCourseInfo.Stage == HundredProgress.Fight2)
            {
                zoneId = guessCourseInfo.SupportZone;
            }

            bool isSupported = guessCourseInfo.HundredSupportCourses.FirstOrDefault((SupportCourseData supportCourseData) =>
            {
                if (supportCourseData.ZoneId != zoneId) return false;
                if (supportCourseData.CourseId != leagueCourseItemData.CourseId) return false;
                if (supportCourseData.TeamId != (isAway ? leagueCourseItemData.AwayTeam.TeamId : leagueCourseItemData.HomeTeam.TeamId)) return false;
                return true;
            }) != null;

            return isSupported;
        }
        public bool IsSupported(CupScoreboardPadItemData cupScoreboardPadItemData)
        {
            if (cupScoreboardPadItemData == null || cupScoreboardPadItemData.getHundredCourseResponse == null) return false;
            if (cupScoreboardPadItemData.getHundredCourseResponse.Stage != (int)HundredProgress.Fight2 && cupScoreboardPadItemData.getHundredCourseResponse.Stage != (int)HundredProgress.Fight3) return false;
            int zoneId = 0;
            if ((HundredProgress)cupScoreboardPadItemData.getHundredCourseResponse.Stage == HundredProgress.Fight2)
            {
                zoneId = cupScoreboardPadItemData.getHundredCourseResponse.SupportZone;
            }

            bool isSupported = cupScoreboardPadItemData.getHundredCourseResponse.HundredSupportCourses.FirstOrDefault((SupportCourseData supportCourseData) =>
            {
                if (supportCourseData.ZoneId != zoneId) return false;
                if (supportCourseData.CourseId != cupScoreboardPadItemData.CourseId) return false;
                if (supportCourseData.TeamId != cupScoreboardPadItemData.TeamId) return false;
                return true;
            }) != null;

            return isSupported;
        }
        public bool IsSupported(GetHundredCourseResponse getHundredCourseResponse, SupportCourseData supportCourseData1)
        {
            bool isSupported = getHundredCourseResponse.HundredSupportCourses.FirstOrDefault((SupportCourseData supportCourseData) =>
            {
                if (supportCourseData.ZoneId != supportCourseData1.ZoneId) return false;
                if (supportCourseData.CourseId != supportCourseData1.CourseId) return false;
                if (supportCourseData.TeamId != supportCourseData1.TeamId) return false;
                return true;
            }) != null;
            return isSupported;
        }

        public SupportCourseData AddSupportLocal(LeagueCourseItemData leagueCourseItemData, bool isAway)
        {
            int zoneId = 0;
            if ((HundredProgress)guessCourseInfo.Stage == HundredProgress.Fight2)
            {
                zoneId = guessCourseInfo.SupportZone;
            }

            SupportCourseData supportCourseData = new SupportCourseData();
            supportCourseData.ZoneId = zoneId;
            supportCourseData.CourseId = leagueCourseItemData.CourseId;
            supportCourseData.TeamId = isAway ? leagueCourseItemData.AwayTeam.TeamId : leagueCourseItemData.HomeTeam.TeamId;
            guessCourseInfo.HundredSupportCourses.Add(supportCourseData);
            return supportCourseData;
        }

        private readonly int newStarCourseCount = 4;
        public bool IsNewStar
        {
            get
            {
                if (nowCourse == null) return false;
                // 服务器开启时间
                DateTime serverOpenDT = TimeUtils.ToDateTime(ActivityController.Instance.ServerOpenTime);
                // serverOpenDT = DataConvUtil.ServerDateTime.AddDays(0);//用于测试，强制显示新星赛区
                // 服务器开启时间是星期几
                System.DayOfWeek serverOpenDayOfWeek = serverOpenDT.DayOfWeek;
                // 计算本周一的日期  
                DateTime thisMonday = serverOpenDT.Date.AddDays(-(int)serverOpenDayOfWeek + (int)DayOfWeek.Monday);
                // 设置时间为本周一凌晨零点  
                DateTime thisMondayMidnight = new DateTime(thisMonday.Year, thisMonday.Month, thisMonday.Day, 0, 0, 0);
                //新手保护结束时间（减掉一天防止误差）
                DateTime closeDate;
                if (serverOpenDayOfWeek == DayOfWeek.Monday)
                {
                    closeDate = thisMondayMidnight.AddDays(7 * newStarCourseCount - 1);
                }
                else
                {
                    closeDate = thisMondayMidnight.AddDays(7 * (newStarCourseCount + 1) - 1);
                }
                return DataConvUtil.ServerDateTime < closeDate;
            }
        }

    }





}