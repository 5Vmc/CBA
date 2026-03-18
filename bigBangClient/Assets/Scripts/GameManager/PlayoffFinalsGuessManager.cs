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

namespace BigBang
{
    /// <summary>
    /// 季后赛冠军赛竞猜2024 的管理类
    /// </summary>
    public class PlayoffFinalsGuessManager : BabuSingleton<PlayoffFinalsGuessManager>
    {
        /// <summary> 所选队伍 </summary>
        public enum Team
        {
            /// <summary> 左侧-队伍1 </summary>
            Left = 1,
            /// <summary> 右侧-队伍2 </summary>
            Right = 2,
        }

        public enum Stage
        {
            /// <summary> 活动开始前，或者服务器卡了 </summary>
            NotOpen,
            /// <summary> 可选择队伍 </summary>
            CanSelectTeam,
            /// <summary> 可选择MVP </summary>
            CanSelectMVP,
            /// <summary> 比赛正常进行中 </summary>
            NormalPlaying,
            /// <summary> 结算奖励后 </summary>
            Ending,
            /// <summary> 活动消失后 </summary>
            Closed
        }

        /// <summary>
        /// 奖励类型
        /// </summary>
        public enum RewardType
        {
            /// <summary> 冠军预测 </summary>
            Champion = 1,
            /// <summary> MVP预测 </summary>
            MVP = 2,
            /// <summary> 单场预测 </summary>
            Single = 3,
            /// <summary> 幸运数字 </summary>
            LuckyNumber = 4,
        }
        // /// <summary> 是否已经选择了队伍 </summary>
        // public bool IsTeamSelected
        // {
        //     get
        //     {
        //         return _isTeamSelected;
        //     }
        // }
        // /// <summary> 是否已经选择了MVP </summary>
        // public bool IsMVPSelected
        // {
        //     get
        //     {
        //         return _isMVPSelected;
        //     }
        // }
        /// <summary> 第三场比赛开始后就不能预测最终获胜队伍和MVP了 </summary>
        public readonly int selectStopCourseId = 3;
        /// <summary> 当前时间还可以预测最终获胜队伍和MVP（早于第三场开赛时间） </summary>
        public bool IsTimeCanSelectMvpOrTeam
        {
            get
            {
                int selectStopTime = Configs.FinalsGuessCourse.GetConfig(selectStopCourseId).MatchTime;
                return DataConvUtil.ServerTime < selectStopTime;
            }
        }
        // public bool IsGuessEnd
        // {
        //     get
        //     {

        //         //任何一方胜利4局后比赛结束，没有下一个竞猜
        //         courseData.Courses[0].Teams[0].TeamId
        //     }
        // }

        // private bool isInited = false;
        // /// <summary>
        // /// 配置表加载完成后，对数据进行预处理
        // /// </summary>
        // public void InitOnce(bool forceInit = true)
        // {

        //     if (isInited && !forceInit) return;
        //     isInited = true;

        // }



        public List<MyFinalsGuess> guessDataList = new();
        private SignActivityModuleNotify signActivityModuleNotify = null;
        public void Unpack(SignActivityModuleNotify signActivityModuleNotify)
        {
            this.signActivityModuleNotify = signActivityModuleNotify;
            guessDataList.Clear();
            guessDataList.AddRange(signActivityModuleNotify.FinalsGuessList);
            ProcessGuessData();
        }
        public MyFinalsGuess teamGuessData = null;
        public MyFinalsGuess mvpGuessData = null;
        public bool isTeamSelected = false;
        public bool isMVPSelected = false;
        public bool IsGuessTeamGuessWin
        {
            get
            {
                if (!isTeamSelected) return false;
                if (!isGuessEnd) return false;
                return teamGuessData.Guess == (int)winTeam;
            }
        }
        public bool isGuessMvpGuessWin
        {
            get
            {
                if (!isMVPSelected) return false;
                if (!isGuessEnd) return false;
                return mvpGuessData.Guess == (int)courseData.MvpPlayerId;
            }
        }
        public bool isEndRewardCanGet
        {
            get
            {
                if (!isGuessEnd) return false;
                if (!isTeamSelected && !isMVPSelected) return false;
                if (isTeamSelected && !teamGuessData.IsReceive) return true;
                if (isMVPSelected && !mvpGuessData.IsReceive) return true;
                return false;
            }
        }
        private void ProcessGuessData()
        {
            teamGuessData = guessDataList.Find((x) => x.RewardId == (int)RewardType.Champion);
            isTeamSelected = teamGuessData != null;
            mvpGuessData = guessDataList.Find((x) => x.RewardId == (int)RewardType.MVP);
            isMVPSelected = mvpGuessData != null;
            RefreshRedDot();
        }

        public GetFinalsGuessInfoResponse courseData = null;
        DateTime getCourseDateTime;
        public void GetCourseData(System.Action callback = null)
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.PlayoffFinalsGuessHome);
            if (activityData == null) return;
            if ((DataConvUtil.ServerDateTime - getCourseDateTime).TotalMilliseconds < 500)
            {
                ProcessCourseData();
                callback?.Invoke();
            }
            NetworkManager.Instance.GetFinalsGuessInfo(activityData.cfg.Id, (GetFinalsGuessInfoResponse getFinalsGuessInfoResponse) =>
            {
                getCourseDateTime = DataConvUtil.ServerDateTime;
                courseData = getFinalsGuessInfoResponse;
                if (courseData.Champions != null && courseData.Champions.Count == 2)
                {
                    FinalsGuessCourse finalsGuessCourse = new();
                    finalsGuessCourse.CourseId = 0;
                    finalsGuessCourse.Teams.Add(courseData.Champions[0]);
                    finalsGuessCourse.Teams.Add(courseData.Champions[1]);
                    courseData.Courses.Add(finalsGuessCourse);
                }
                ProcessCourseData();
                callback?.Invoke();
            });
        }
        private Team winTeam = Team.Left;
        private bool isTeam1Win = false;
        private bool isTeam2Win = false;
        public bool isGuessEnd = false;
        private readonly int teamFinalWinCount = 4;
        public int team1Support = 0;
        public int team2Support = 0;
        public int team1WinCount = 0;
        public int team2WinCount = 0;
        private void ProcessCourseData()
        {
            team1WinCount = 0;
            team2WinCount = 0;
            foreach (FinalsGuessCourse finalsGuessCourse in courseData.Courses)
            {
                if (finalsGuessCourse.CourseId == 0)
                {
                    foreach (FinalsGuessTeam finalsGuessTeam in finalsGuessCourse.Teams)
                    {
                        if (finalsGuessTeam.TeamId == (int)Team.Left)
                        {
                            team1Support = finalsGuessTeam.Support;
                        }
                        else if (finalsGuessTeam.TeamId == (int)Team.Right)
                        {
                            team2Support = finalsGuessTeam.Support;
                        }
                    }
                }
                else
                {
                    FinalsGuessTeam leftTeam = null;
                    FinalsGuessTeam rightTeam = null;
                    foreach (FinalsGuessTeam finalsGuessTeam in finalsGuessCourse.Teams)
                    {
                        if (finalsGuessTeam.TeamId == (int)Team.Left)
                        {
                            leftTeam = finalsGuessTeam;
                        }
                        else if (finalsGuessTeam.TeamId == (int)Team.Right)
                        {
                            rightTeam = finalsGuessTeam;
                        }
                    }
                    if (leftTeam != null && rightTeam != null)
                    {
                        if (leftTeam.Point != 0 || rightTeam.Point != 0)
                        {
                            if (leftTeam.Point > rightTeam.Point) team1WinCount++;
                            if (rightTeam.Point > leftTeam.Point) team2WinCount++;
                        }
                    }
                }
            }
            isTeam1Win = team1WinCount >= teamFinalWinCount;
            isTeam2Win = team2WinCount >= teamFinalWinCount;
            winTeam = isTeam1Win ? Team.Left : Team.Right;
            isGuessEnd = isTeam1Win || isTeam2Win;
            RefreshRedDot();
        }
        private void RefreshRedDot()
        {
            ActivityController.Instance.RefreshActivityRedDotByClientType(ActivityClientType.PlayoffFinalsGuessHome);
            ActivityController.Instance.RefreshActivityRedDotByClientType(ActivityClientType.PlayoffFinalsGuessSingle);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }
        public Stage GetStage()
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.PlayoffFinalsGuessHome);
            if (activityData == null) return Stage.NotOpen;
            if (courseData == null) return Stage.NotOpen;
            if (activityData.IsHide) return Stage.Closed;
            if (activityData.IsEnd || isGuessEnd) return Stage.Ending;
            if (IsTimeCanSelectMvpOrTeam)
            {
                if (!isTeamSelected) return Stage.CanSelectTeam;
                if (!isMVPSelected) return Stage.CanSelectMVP;
            }
            return Stage.NormalPlaying;
        }

        public void GuessChampion(int teamId, System.Action callback = null)
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.PlayoffFinalsGuessHome);
            NetworkManager.Instance.FinalsGuess(activityData.cfg.Id, (int)RewardType.Champion, 0, teamId, (FinalsGuessResponse finalsGuessResponse) =>
            {
                if (finalsGuessResponse.Success)
                {
                    MyFinalsGuess myFinalsGuess = new MyFinalsGuess
                    {
                        RewardId = (int)RewardType.Champion,
                        Guess = teamId,
                    };
                    guessDataList.Add(myFinalsGuess);
                    ProcessGuessData();
                    FinalsGuessCourse finalsGuessCourse = GetCourse(0);
                    if (finalsGuessCourse == null)
                    {
                        Debug.LogWarning("PlayoffFinalsGuessManager , GuessChampion , finalsGuessCourse == null");
                    }
                    else
                    {
                        foreach (FinalsGuessTeam finalsGuessTeam in finalsGuessCourse.Teams)
                        {
                            if (finalsGuessTeam.TeamId == teamId) finalsGuessTeam.Support++;
                        }
                    }
                    EventManager.Instance.Dispatch(EventID.RefreshPlayoffFinalsGuessUI);
                    callback?.Invoke();
                    GetNewDataAndRefreshUI();
                }
                else
                {
                    GetNewDataAndRefreshUI();
                }
            });
        }
        public void GuessMVP(int playerId, System.Action callback = null)
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.PlayoffFinalsGuessHome);
            NetworkManager.Instance.FinalsGuess(activityData.cfg.Id, (int)RewardType.MVP, 0, playerId, (FinalsGuessResponse finalsGuessResponse) =>
            {
                if (finalsGuessResponse.Success)
                {
                    MyFinalsGuess myFinalsGuess = new MyFinalsGuess
                    {
                        RewardId = (int)RewardType.MVP,
                        Guess = playerId,
                    };
                    guessDataList.Add(myFinalsGuess);
                    ProcessGuessData();
                    EventManager.Instance.Dispatch(EventID.RefreshPlayoffFinalsGuessUI);
                    callback?.Invoke();
                    GetNewDataAndRefreshUI();
                }
                else
                {
                    GetNewDataAndRefreshUI();
                }
            });
        }
        public void GuessSingle(int courseId, int teamId, System.Action callback = null)
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.PlayoffFinalsGuessHome);
            NetworkManager.Instance.FinalsGuess(activityData.cfg.Id, (int)RewardType.Single, courseId, teamId, (FinalsGuessResponse finalsGuessResponse) =>
            {
                if (finalsGuessResponse.Success)
                {
                    MyFinalsGuess myFinalsGuess = new MyFinalsGuess
                    {
                        RewardId = (int)RewardType.Single,
                        CourseId = courseId,
                        Guess = teamId,
                    };
                    guessDataList.Add(myFinalsGuess);
                    ProcessGuessData();
                    FinalsGuessCourse finalsGuessCourse = GetCourse(courseId);
                    if (finalsGuessCourse == null)
                    {
                        Debug.LogWarning("PlayoffFinalsGuessManager , GuessChampion , finalsGuessCourse == null");
                    }
                    else
                    {
                        foreach (FinalsGuessTeam finalsGuessTeam in finalsGuessCourse.Teams)
                        {
                            if (finalsGuessTeam.TeamId == teamId) finalsGuessTeam.Support++;
                        }
                    }
                    callback?.Invoke();
                    GetNewDataAndRefreshUI();
                }
                else
                {
                    GetNewDataAndRefreshUI();
                }
            });
        }
        public void GuessLuckyNumber(int courseId, int guessNumber, System.Action callback = null)
        {
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.PlayoffFinalsGuessHome);
            NetworkManager.Instance.FinalsGuess(activityData.cfg.Id, (int)RewardType.LuckyNumber, courseId, guessNumber, (FinalsGuessResponse finalsGuessResponse) =>
            {
                if (finalsGuessResponse.Success)
                {
                    MyFinalsGuess myFinalsGuess = new MyFinalsGuess
                    {
                        RewardId = (int)RewardType.LuckyNumber,
                        CourseId = courseId,
                        Guess = guessNumber,
                    };
                    guessDataList.Add(myFinalsGuess);
                    ProcessGuessData();
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
                EventManager.Instance.Dispatch(EventID.RefreshPlayoffFinalsGuessUI);
            });
        }

        public bool IsCourseMatchEnd(int courseId)
        {
            FinalsGuessCourse finalsGuessCourse = GetCourse(courseId);
            if (finalsGuessCourse == null) return false;
            if (finalsGuessCourse.Teams.Count < 2) return false;
            if (finalsGuessCourse.Teams[0].Point == 0 && finalsGuessCourse.Teams[1].Point == 0) return false;
            return true;
        }
        public FinalsGuessCourse GetCourse(int courseId)
        {
            if (courseData == null) return null;
            if (courseData.Courses == null) return null;
            return courseData.Courses.FirstOrDefault((x) => x.CourseId == courseId);
        }
        public MyFinalsGuess GetGuessSingle(int courseId)
        {
            return guessDataList.FirstOrDefault((x) => x.RewardId == (int)RewardType.Single && x.CourseId == courseId);
        }
        public MyFinalsGuess GetGuessLuckyNumber(int courseId)
        {
            return guessDataList.FirstOrDefault((x) => x.RewardId == (int)RewardType.LuckyNumber && x.CourseId == courseId);
        }

        public void ReceiveEndReward(System.Action callback = null)
        {
            List<MyFinalsGuess> myFinalsGuesseList = new();
            foreach (MyFinalsGuess myFinalsGuess in guessDataList)
            {
                if (myFinalsGuess.RewardId == (int)RewardType.Champion && !teamGuessData.IsReceive)
                {
                    myFinalsGuesseList.Add(myFinalsGuess);
                }
                if (myFinalsGuess.RewardId == (int)RewardType.MVP && !mvpGuessData.IsReceive)
                {
                    myFinalsGuesseList.Add(myFinalsGuess);
                }
            }
            NetworkManager.Instance.GetFinalsGuessReward(myFinalsGuesseList, (GetFinalsGuessRewardResponse getFinalsGuessRewardResponse) =>
            {
                if (getFinalsGuessRewardResponse.Success)
                {
                    foreach (MyFinalsGuess myFinalsGuess in myFinalsGuesseList)
                    {
                        myFinalsGuess.IsReceive = true;
                    }
                    callback?.Invoke();
                    GetNewDataAndRefreshUI();
                }
                else
                {
                    GetNewDataAndRefreshUI();
                    Debug.LogWarning("PlayoffFinalsGuessManager , ReceiveEndReward , Success == false");
                }
            });
        }
        public bool HasSingleRewardCanGet(int courseId)
        {
            bool isCourseMatchEnd = PlayoffFinalsGuessManager.Instance.IsCourseMatchEnd(courseId);
            if (!isCourseMatchEnd) return false;
            MyFinalsGuess guessSingle = PlayoffFinalsGuessManager.Instance.GetGuessSingle(courseId);
            bool isGuessSingle = guessSingle != null;
            if (!isGuessSingle) return false;
            return !guessSingle.IsReceive;
        }
        public bool HasLuckyNumberRewardCanGet(int courseId)
        {
            bool isCourseMatchEnd = PlayoffFinalsGuessManager.Instance.IsCourseMatchEnd(courseId);
            if (!isCourseMatchEnd) return false;
            MyFinalsGuess guessLuckyNumber = PlayoffFinalsGuessManager.Instance.GetGuessLuckyNumber(courseId);
            bool isGuessLuckyNumber = guessLuckyNumber != null;
            if (!isGuessLuckyNumber) return false;
            return !guessLuckyNumber.IsReceive;
        }
        public void ReceiveCourseReward(int courseId, System.Action callback = null)
        {
            List<MyFinalsGuess> myFinalsGuesseList = new();
            foreach (MyFinalsGuess myFinalsGuess in guessDataList)
            {
                if (myFinalsGuess.RewardId == (int)RewardType.Single && myFinalsGuess.CourseId == courseId)
                {
                    myFinalsGuesseList.Add(myFinalsGuess);
                }
                if (myFinalsGuess.RewardId == (int)RewardType.LuckyNumber && myFinalsGuess.CourseId == courseId)
                {
                    myFinalsGuesseList.Add(myFinalsGuess);
                }
            }
            NetworkManager.Instance.GetFinalsGuessReward(myFinalsGuesseList, (GetFinalsGuessRewardResponse getFinalsGuessRewardResponse) =>
            {
                if (getFinalsGuessRewardResponse.Success)
                {
                    foreach (MyFinalsGuess myFinalsGuess in myFinalsGuesseList)
                    {
                        myFinalsGuess.IsReceive = true;
                    }
                    callback?.Invoke();
                    GetNewDataAndRefreshUI();
                }
                else
                {
                    GetNewDataAndRefreshUI();
                    Debug.LogWarning("PlayoffFinalsGuessManager , ReceiveCourseReward , Success == false");
                }
            });
        }
        public List<FinalsGuessCourseConfig> GetCanShowCourse()
        {
            List<FinalsGuessCourseConfig> finalsGuessCourseConfigList = new();
            foreach (FinalsGuessCourseConfig finalsGuessCourseConfig in Configs.FinalsGuessCourse.GetConfigList())
            {
                if (finalsGuessCourseConfig.Id == 1)
                {
                    finalsGuessCourseConfigList.Add(finalsGuessCourseConfig);
                }
                else
                {
                    bool isLastCourseMatchEnd = PlayoffFinalsGuessManager.Instance.IsCourseMatchEnd(finalsGuessCourseConfig.Id - 1);
                    if (isLastCourseMatchEnd)
                    {
                        if (PlayoffFinalsGuessManager.Instance.isGuessEnd && !PlayoffFinalsGuessManager.Instance.IsCourseMatchEnd(finalsGuessCourseConfig.Id)) continue;
                        finalsGuessCourseConfigList.Add(finalsGuessCourseConfig);
                    }
                }
            }
            return finalsGuessCourseConfigList;
        }

    }
}