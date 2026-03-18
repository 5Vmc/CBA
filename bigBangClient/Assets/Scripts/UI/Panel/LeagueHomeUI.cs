using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using Protocol;
using System.Linq;
using TMPro;
using System;
using Utils;
using Babu;
using System.Collections.Generic;
using BigBang.Animation;

namespace BigBang.UI
{

    public class LeagueHomeUI : APanelController
    {
        #region 初始化

        protected override void AddListeners()
        {
            base.AddListeners();

            helpButton.OnClick += OnClickHelpButton;
            historyButton.OnClick += OnClickHistoryButton;
            peakButton.OnClick += OnClickPeakButton;
            rewardButton.OnClick += OnClickRewardButton;
            courseButton.OnClick += OnClickCourseButton;
            playerRankButton.OnClick += OnClickPlayerRankButton;
            signButton.OnClick += OnClickSignButton;
            signedButton.OnClick += OnClickSignedButton;
            recoverButton.OnClick += OnClickRecoverButton;
            formationButton.OnClick += OnClickFormationButton;

            recoverButton2.OnClick += OnClickRecoverButton;
            formationButton2.OnClick += OnClickFormationButton;

            homeTeamPanel.OnClick += OnClickHomeTeamPanel;
            awayTeamPanel.OnClick += OnClickAwayTeamPanel;

            SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();

            helpButton.OnClick -= OnClickHelpButton;
            historyButton.OnClick -= OnClickHistoryButton;
            peakButton.OnClick -= OnClickPeakButton;
            rewardButton.OnClick -= OnClickRewardButton;
            courseButton.OnClick -= OnClickCourseButton;
            playerRankButton.OnClick -= OnClickPlayerRankButton;
            signButton.OnClick -= OnClickSignButton;
            signedButton.OnClick -= OnClickSignedButton;
            recoverButton.OnClick -= OnClickRecoverButton;
            formationButton.OnClick -= OnClickFormationButton;

            recoverButton2.OnClick -= OnClickRecoverButton;
            formationButton2.OnClick -= OnClickFormationButton;

            homeTeamPanel.OnClick -= OnClickHomeTeamPanel;
            awayTeamPanel.OnClick -= OnClickAwayTeamPanel;

            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
        }

        [SerializeField] private RectTransform signPanel = null;
        [SerializeField] private RectTransform playingPanel = null;
        [SerializeField] private LeagueHomeUIAnim leagueHomeUIAnim;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            signPanel.gameObject.SetActive(false);
            playingPanel.gameObject.SetActive(false);
            Player.PVPManager.GetNewLeagueData(() =>
            {
                RefreshUI();
                leagueHomeUIAnim.PlayEnter(isInSignStage);
                CheckPopWindow();
            });
            leagueHomeUIAnim.Init();
        }

        [SerializeField] private TMP_Text leagueLevelText = null;
        private GetLeagueDataResponse serverData = null;
        private bool isInSignStage = false;
        private void RefreshUI()
        {
            serverData = Player.PVPManager.serverLeagueData;
            if (serverData == null)
            {
                signPanel.gameObject.SetActive(false);
                playingPanel.gameObject.SetActive(false);
                Debug.LogWarning("LeagueHomeUI , RefreshUI , serverData == null");
            }
            leagueLevelText.text = "{0}级联赛".SafeFormat(serverData.LeagueInfo.LeagueLevel);
            isInSignStage = (TeamState)serverData.TeamState == TeamState.INIT || (TeamState)serverData.TeamState == TeamState.SIGNUP || (TeamState)serverData.TeamState == TeamState.SETTLE;
            signPanel.gameObject.SetActive(isInSignStage);
            playingPanel.gameObject.SetActive(!isInSignStage);
            if (isInSignStage) RefreshSignUI();
            if (!isInSignStage) RefreshPlayingUI();
        }

        private void RefreshLeftTimeOneSec()
        {
            if (serverData == null) return;
            isInSignStage = (TeamState)serverData.TeamState == TeamState.INIT || (TeamState)serverData.TeamState == TeamState.SIGNUP || (TeamState)serverData.TeamState == TeamState.SETTLE;
            if (isInSignStage) RefreshSignLeftTime();
            if (!isInSignStage) RefreshPlayingLeftTime();
        }
        #endregion

        #region 检查各种弹窗

        private void CheckPopWindow()
        {
            CheckCommonMatchReward();
        }
        private void CheckCommonMatchReward()//领取场次奖励
        {
            if (Player.PVPManager.tmpRewards[CompitionID.League].Count > 0)
            {
                UIController.Instance.OpenWindow<LeagueSessionRewardUI>(new LeagueSessionRewardUIProperties(CheckLeagueEnd));
            }
            else
            {
                CheckLeagueEnd();
            }
        }
        private void CheckLeagueEnd()//领取赛季结算奖励
        {
            if (serverData.LastSeasonSettle != null
                && serverData.LastSeasonSettle.TopCards != null)
            {
                UIController.Instance.OpenWindow<LeagueHistoryDetailUI>(new LeagueHistoryDetailUIProperties(serverData.LastSeasonSettle, true, CheckLeagueStart));
            }
            else
            {
                CheckLeagueStart();
            }
        }

        private void CheckLeagueStart()//赛季开始信件
        {
            if (serverData.TopCards != null
                && serverData.GamePerviewData != null
                && ((serverData.GamePerviewData.HomeTeam.Team.TeamId == Player.GbId && (serverData.GamePerviewData.HomeTeam.Win > 0 || serverData.GamePerviewData.HomeTeam.Failed > 0))
                     || (serverData.GamePerviewData.AwayTeam.Team.TeamId == Player.GbId && (serverData.GamePerviewData.AwayTeam.Win > 0 || serverData.GamePerviewData.AwayTeam.Failed > 0))
                && UnityEngine.PlayerPrefs.GetInt(PlayerPrefsKeys.LeagueFirstEnter + Player.GbId, 0) != Player.PVPManager.serverLeagueData.LeagueInfo.LeagueId))
            {
                UnityEngine.PlayerPrefs.SetInt(PlayerPrefsKeys.LeagueFirstEnter + Player.GbId, Player.PVPManager.serverLeagueData.LeagueInfo.LeagueId);
                UnityEngine.PlayerPrefs.SetInt(PlayerPrefsKeys.LeagueLastMatch + Player.GbId, 0);
                UIController.Instance.OpenWindow<LeagueStartNoticeUI>(new LeagueStartNoticeUIProperties(CheckLastMatch));
            }
            else
            {
                UnityEngine.PlayerPrefs.SetInt(PlayerPrefsKeys.LeagueFirstEnter + Player.GbId, Player.PVPManager.serverLeagueData.LeagueInfo.LeagueId);
                CheckLastMatch();
            }
        }
        private void CheckLastMatch()//上一次比赛战报
        {
            if (serverData.LastCourse != null
                && serverData.LastCourse.HomeTeam != null
                && serverData.LastCourse.AwayTeam != null
                && serverData.LastCourse.HomeGoal > -1
                && serverData.LastCourse.AwayGoal > -1
                && UnityEngine.PlayerPrefs.GetInt(PlayerPrefsKeys.LeagueLastMatch + Player.GbId, 0) != serverData.LastCourse.CourseId)
            {
                UnityEngine.PlayerPrefs.SetInt(PlayerPrefsKeys.LeagueLastMatch + Player.GbId, serverData.LastCourse.CourseId);
                UIController.Instance.OpenWindow<LeagueTeamDetailUI>(new LeagueTeamDetailUIProperties(serverData.LastCourse, true));
            }
        }

        #endregion

        #region 按钮回调

        [SerializeField] private BabuButton helpButton = null;
        [SerializeField] private BabuButton historyButton = null;
        [SerializeField] private BabuButton peakButton = null;
        [SerializeField] private BabuButton rewardButton = null;
        [SerializeField] private BabuButton courseButton = null;
        [SerializeField] private BabuButton playerRankButton = null;
        [SerializeField] private BabuButton signButton = null;
        [SerializeField] private BabuButton signedButton = null;
        [SerializeField] private BabuButton recoverButton = null;
        [SerializeField] private BabuButton formationButton = null;

        [SerializeField] private BabuButton recoverButton2 = null;
        [SerializeField] private BabuButton formationButton2 = null;

        [SerializeField] private BabuButton homeTeamPanel = null;
        [SerializeField] private BabuButton awayTeamPanel = null;

        private void OnClickHelpButton(BabuButton _)//帮助
        {
            UIController.Instance.OpenWindow<LeagueIntroductionUI>(new LeagueIntroductionUIProperties(CompitionID.League));
        }
        private void OnClickHistoryButton(BabuButton _)//历史战绩
        {
            UIController.Instance.ShowPanel<LeagueHistoryUI>();
        }
        private void OnClickPeakButton(BabuButton _)//巅峰榜
        {
            UIController.Instance.ShowPanel<LeagueRankUI>();
        }
        private void OnClickRewardButton(BabuButton _)//奖励预览
        {
            UIController.Instance.OpenWindow<LeagueRewardsUI>(new LeagueRewardsUIProperties(CompitionID.League, serverData.LeagueInfo.LeagueLevel));
        }
        private void OnClickCourseButton(BabuButton _)//赛程
        {
            if ((TeamState)Player.PVPManager.serverLeagueData.TeamState == TeamState.INIT || (TeamState)Player.PVPManager.serverLeagueData.TeamState == TeamState.SIGNUP || (TeamState)Player.PVPManager.serverLeagueData.TeamState == TeamState.SETTLE)
            {
                Tips.PopTips("比赛开始后才可查看赛程信息");
                return;
            }
            UIController.Instance.ShowPanel<LeagueCourseUI>();
        }
        private void OnClickPlayerRankButton(BabuButton _)//球员榜
        {
            if ((TeamState)Player.PVPManager.serverLeagueData.TeamState == TeamState.INIT || (TeamState)Player.PVPManager.serverLeagueData.TeamState == TeamState.SIGNUP || (TeamState)Player.PVPManager.serverLeagueData.TeamState == TeamState.SETTLE)
            {
                Tips.PopTips("比赛开始后才可查看球员榜信息");
                return;
            }
            UIController.Instance.ShowPanel<LeaguePlayerRankUI>();
        }
        private void OnClickSignButton(BabuButton _)//报名联赛
        {
            NetworkManager.Instance.GetLeagueSignUp((GetLeagueSignUpResponse getLeagueSignUpResponse) =>
            {
                if (getLeagueSignUpResponse.Success == true)
                {
                    Player.PVPManager.serverLeagueData.TeamState = (int)TeamState.SIGNUP;
                    Player.PVPManager.updatePVPInfoNotify.LeagueTeamState = (int)TeamState.SIGNUP;
                    Player.PVPManager.RefreshLeagueRedDot();
                    RefreshUI();
                    Tips.PopTips("成功报名此次联赛，请等待比赛开始");
                }
                else
                {
                    Player.PVPManager.GetNewLeagueData(RefreshUI);
                }
            });
        }
        private void OnClickSignedButton(BabuButton _)//已报名
        {
            if (isInGroupState)
            {
                Tips.PopTips("您已报名此次联赛，请等待分组结束");
            }
            else
            {
                Tips.PopTips("您已报名此次联赛，请等待比赛开始");
            }
        }
        private void OnClickRecoverButton(BabuButton _)//球员恢复
        {
            Player.FightManager.FormationController.GetAndCheckDefaultFormation(FormationID.PVP, formation =>
            {
                UIController.Instance.ShowPanel<FormationRecoverUI>(new FormationRecoverUIProperties(formation));
            });
        }
        private void OnClickFormationButton(BabuButton _)//布阵
        {
            Player.FightManager.FormationController.GetAndCheckDefaultFormation(FormationID.PVP, formation =>
            {
                UIController.Instance.ShowPanel<FormationUI>(new FormationProperties(formation, false, FormationUI.FormationShowType.Formation, FormationID.PVP));
            });
        }

        private void OnClickHomeTeamPanel(BabuButton _)//查看主场队伍详情
        {
            //UIController.Instance.OpenWindow<LeagueFirstInfoUI>(new LeagueFirstInfoUIProperties(Player.PVPManager.serverLeagueData?.GamePerviewData?.HomeTeam));
            UIController.Instance.OpenWindow<LeagueTeamPlayerUI>(new LeagueTeamPlayerUIProperties(Player.PVPManager.serverLeagueData?.GamePerviewData?.HomeTeam));
        }
        private void OnClickAwayTeamPanel(BabuButton _)//查看客场队伍详情
        {
            //UIController.Instance.OpenWindow<LeagueFirstInfoUI>(new LeagueFirstInfoUIProperties(Player.PVPManager.serverLeagueData?.GamePerviewData?.AwayTeam));
            UIController.Instance.OpenWindow<LeagueTeamPlayerUI>(new LeagueTeamPlayerUIProperties(Player.PVPManager.serverLeagueData?.GamePerviewData?.AwayTeam));
        }

        #endregion

        #region 报名阶段

        [SerializeField] private TMP_Text startDateText = null;
        [SerializeField] private ImageFont signTimeImageFont = null;
        private DateTime nextStartDateTime;
        private DateTime nextGroupDateTime;
        private bool isInGroupState = false;
        [SerializeField] private TMP_Text timeTitleText = null;
        private void RefreshSignUI()
        {
            bool isCanSign = (TeamState)serverData.TeamState == TeamState.INIT || (TeamState)serverData.TeamState == TeamState.SETTLE;
            signButton.gameObject.SetActive(isCanSign);
            signedButton.gameObject.SetActive(!isCanSign);
            nextStartDateTime = GetNextStartDateTime();
            nextGroupDateTime = GetNextGroupDateTime();
            isInGroupState = nextGroupDateTime < nextStartDateTime && (TeamState)serverData.TeamState == TeamState.SIGNUP;
            timeTitleText.text = isInGroupState ? "分组结束倒计时" : "报名截止倒计时";
            startDateText.text = nextGroupDateTime.ToString("yyyy.MM.dd HH:mm");
            RefreshSignLeftTime();
        }
        private DateTime GetNextStartDateTime()
        {
            DateTime serverTime = Utils.DataConvUtil.ServerDateTime;
            DateTime dayStartTime = serverTime.Date;
            DateTime firstLeagleStartTime = dayStartTime.AddHours(12);
            if (serverTime < firstLeagleStartTime) return firstLeagleStartTime;
            DateTime secondLeagleStartTime = dayStartTime.AddHours(18);
            if (serverTime < secondLeagleStartTime) return secondLeagleStartTime;
            DateTime tomorrowFirstLeagleStartTime = firstLeagleStartTime.AddDays(1);
            return tomorrowFirstLeagleStartTime;
        }
        private DateTime GetNextGroupDateTime()
        {
            DateTime serverTime = Utils.DataConvUtil.ServerDateTime;
            DateTime dayStartTime = serverTime.Date;
            DateTime firstLeagleStartTime = dayStartTime.AddHours(12).AddMinutes(5);
            if (serverTime < firstLeagleStartTime) return firstLeagleStartTime;
            DateTime secondLeagleStartTime = dayStartTime.AddHours(18).AddMinutes(5);
            if (serverTime < secondLeagleStartTime) return secondLeagleStartTime;
            DateTime tomorrowFirstLeagleStartTime = firstLeagleStartTime.AddDays(1);
            return tomorrowFirstLeagleStartTime;
        }

        private void RefreshSignLeftTime()
        {
            long leftTime = -1;
            if (!isInGroupState)
            {
                leftTime = TimeUtils.ToUnixStamp(nextStartDateTime) - Utils.DataConvUtil.ServerTime;
            }
            else
            {
                leftTime = TimeUtils.ToUnixStamp(nextGroupDateTime) - Utils.DataConvUtil.ServerTime;
            }

            if (leftTime < 0)
            {
                Player.PVPManager.GetNewLeagueData(() =>
                {
                    RefreshUI();
                    leagueHomeUIAnim.PlayEnter(isInSignStage, false);
                });
            }
            signTimeImageFont.text = Utility.FormatLeftTimeMustHasHour((int)leftTime).Replace(':', '_');
        }


        #endregion

        #region 开赛阶段

        [SerializeField] private TMP_Text roundText = null;
        [SerializeField] private RectTransform waitNextPanel = null;
        [SerializeField] private TMP_Text nextDateText = null;
        [SerializeField] private RectTransform nearPanel = null;
        [SerializeField] private TMP_Text startMinuteText = null;
        [SerializeField] private TMP_Text startSecondText = null;

        [SerializeField] private ClubIconItem homeClubIcon = null;
        [SerializeField] private ImageFont homeFightPointImageFont = null;
        [SerializeField] private TMP_Text homeTeamNameText = null;
        [SerializeField] private ClubIconItem awayClubIcon = null;
        [SerializeField] private ImageFont awayFightPointImageFont = null;
        [SerializeField] private TMP_Text awayTeamNameText = null;

        [SerializeField] private TMP_Text homeScoreText = null;//篮球没有积分，不显示
        [SerializeField] private TMP_Text awayScoreText = null;//篮球没有积分，不显示
        [SerializeField] private TMP_Text homeRankText = null;
        [SerializeField] private TMP_Text awayRankText = null;
        [SerializeField] private TMP_Text homeWinLoseText = null;
        [SerializeField] private TMP_Text awayWinLoseText = null;
        private readonly string winLoseStr = "<color=#22FF52>{0}</color>/<color=#F13521>{1}</color>";

        [SerializeField] private LeagueHomePlayerStateItem leagueHomePlayerStateItem1 = null;
        [SerializeField] private LeagueHomePlayerStateItem leagueHomePlayerStateItem2 = null;
        [SerializeField] private LeagueHomePlayerStateItem leagueHomePlayerStateItem3 = null;

        [SerializeField] private RectTransform recover2UpPanel = null;

        [SerializeField] private LeagueScoreboardAdapter leagueScoreboardAdapter = null;

        [SerializeField] private RectTransform hasNextPanel = null;
        [SerializeField] private RectTransform noNextPanel = null;

        [SerializeField] private Color MyNameColor = new();
        [SerializeField] private Color EnemyNameColor = new();

        private void RefreshPlayingUI()
        {
            if (serverData.GamePerviewData == null)
            {
                hasNextPanel.gameObject.SetActive(false);
                noNextPanel.gameObject.SetActive(true);
            }
            else
            {
                hasNextPanel.gameObject.SetActive(true);
                noNextPanel.gameObject.SetActive(false);

                roundText.text = "第{0}轮".SafeFormat(serverData.LeagueInfo.LeagueRoundId);
                nextMatchDateTime = TimeUtils.ToDateTime(serverData.GamePerviewData.Time);
                nextMatchDateTime.AddMinutes(1);
                RefreshPlayingLeftTime();

                homeClubIcon.SetIcon(serverData.GamePerviewData.HomeTeam.Team.TeamIcon);
                awayClubIcon.SetIcon(serverData.GamePerviewData.AwayTeam.Team.TeamIcon);
                homeFightPointImageFont.text = serverData.GamePerviewData.HomeTeam.Strength.ToString();
                awayFightPointImageFont.text = serverData.GamePerviewData.AwayTeam.Strength.ToString();
                homeTeamNameText.text = serverData.GamePerviewData.HomeTeam.Team.TeamName;
                homeTeamNameText.color = serverData.GamePerviewData.HomeTeam.Team.TeamId == Player.GbId ? MyNameColor : EnemyNameColor;
                awayTeamNameText.text = serverData.GamePerviewData.AwayTeam.Team.TeamName;
                awayTeamNameText.color = serverData.GamePerviewData.AwayTeam.Team.TeamId == Player.GbId ? MyNameColor : EnemyNameColor;

                homeRankText.text = (serverData.GamePerviewData.HomeTeam.Rank == 0) ? "-" : serverData.GamePerviewData.HomeTeam.Rank.ToString();
                awayRankText.text = (serverData.GamePerviewData.AwayTeam.Rank == 0) ? "-" : serverData.GamePerviewData.AwayTeam.Rank.ToString();
                homeWinLoseText.text = winLoseStr.SafeFormat(serverData.GamePerviewData.HomeTeam.Win, serverData.GamePerviewData.HomeTeam.Failed);
                awayWinLoseText.text = winLoseStr.SafeFormat(serverData.GamePerviewData.AwayTeam.Win, serverData.GamePerviewData.AwayTeam.Failed);

                leagueHomePlayerStateItem1.SetData(GetPlayerCardStatusPercent(serverData.GamePerviewData.HomeTeam), GetPlayerCardStatusPercent(serverData.GamePerviewData.AwayTeam));
                leagueHomePlayerStateItem2.SetData(GetPlayerCardInjuryTypePercent(serverData.GamePerviewData.HomeTeam), GetPlayerCardInjuryTypePercent(serverData.GamePerviewData.AwayTeam));
                leagueHomePlayerStateItem3.SetData(GetPlayerCardEnergyPercent(serverData.GamePerviewData.HomeTeam), GetPlayerCardEnergyPercent(serverData.GamePerviewData.AwayTeam));
            }

            bool isRecoverUp = false;
            recover2UpPanel.gameObject.SetActive(isRecoverUp);

            SetLeagueScoreboardAdapterData(serverData.LeagueScorebarTeamList.ToList());
        }
        private float GetPlayerCardStatusPercent(CourseTeamData courseTeamData)
        {
            List<PlayerCardMiniInfo> playerCardMiniInfoList = new();
            playerCardMiniInfoList.AddRange(courseTeamData.BoardCardMap.Values);
            playerCardMiniInfoList.AddRange(courseTeamData.SubstituteCardMap.Values);
            int totalValue = 0;
            foreach (PlayerCardMiniInfo playerCardMiniInfo in playerCardMiniInfoList)
            {
                switch ((PlayerCardStatus)playerCardMiniInfo.Status)
                {
                    case PlayerCardStatus.VeryDown: totalValue += 0; break;
                    case PlayerCardStatus.Down: totalValue += 25; break;
                    case PlayerCardStatus.Ordinary: totalValue += 50; break;
                    case PlayerCardStatus.Good: totalValue += 75; break;
                    case PlayerCardStatus.VeryGood: totalValue += 100; break;
                    default: break;
                }
            }
            float playerCardStatusPercent = (float)totalValue / playerCardMiniInfoList.Count;
            return playerCardStatusPercent;
        }
        private float GetPlayerCardInjuryTypePercent(CourseTeamData courseTeamData)
        {
            List<PlayerCardMiniInfo> playerCardMiniInfoList = new();
            playerCardMiniInfoList.AddRange(courseTeamData.BoardCardMap.Values);
            playerCardMiniInfoList.AddRange(courseTeamData.SubstituteCardMap.Values);
            int totalValue = 0;
            foreach (PlayerCardMiniInfo playerCardMiniInfo in playerCardMiniInfoList)
            {
                switch ((InjuryType)playerCardMiniInfo.InjuryType)
                {
                    case InjuryType.None: totalValue += 100; break;
                    case InjuryType.Health: totalValue += 100; break;
                    case InjuryType.MinorInjury: totalValue += 50; break;
                    case InjuryType.SeriousInjury: totalValue += 0; break;
                    default: break;
                }
            }
            float playerCardInjuryTypePercent = (float)totalValue / playerCardMiniInfoList.Count;
            return playerCardInjuryTypePercent;
        }
        private float GetPlayerCardEnergyPercent(CourseTeamData courseTeamData)
        {
            List<PlayerCardMiniInfo> playerCardMiniInfoList = new();
            playerCardMiniInfoList.AddRange(courseTeamData.BoardCardMap.Values);
            playerCardMiniInfoList.AddRange(courseTeamData.SubstituteCardMap.Values);
            float totalValue = 0;
            foreach (PlayerCardMiniInfo playerCardMiniInfo in playerCardMiniInfoList)
            {
                totalValue += Utility.KeepInRange(playerCardMiniInfo.Energy / 50 * 100, 0, 100);
            }
            float playerCardEnergyPercent = (float)totalValue / playerCardMiniInfoList.Count;
            return playerCardEnergyPercent;
        }
        private void SetLeagueScoreboardAdapterData(List<LeagueScorebarTeam> data)
        {
            // 积分榜按累计积分从高到低排列：积分相同的，先看场次，场次少在前；场次相同再看净胜球，多的在前；净胜球相同，再看进球数，多的在前；进球数相同，则ID大的在前
            var result = data.OrderByDescending(item => item.Win * 3 + item.Deuce)              // 积分
                             .ThenByDescending(item => item.Win + item.Deuce + item.Failed)     // 场次
                             .ThenByDescending(item => item.Obtain - item.Lost)                 // 净胜球
                             .ThenByDescending(item => item.Obtain)                             // 进球数
                             .ThenByDescending(item => item.BaseData.TeamId);
            leagueScoreboardAdapter.SetData(result.ToList());
        }

        private DateTime nextMatchDateTime;
        private void RefreshPlayingLeftTime()
        {
            if (serverData == null || serverData.GamePerviewData == null) return;
            long leftTime = TimeUtils.ToUnixStamp(nextMatchDateTime) - Utils.DataConvUtil.ServerTime + 60;
            bool isWait = leftTime > 300;
            waitNextPanel.gameObject.SetActive(isWait);
            nearPanel.gameObject.SetActive(!isWait);
            if (!isWait)
            {
                List<string> timeStrList = Utility.FormatLeftTimeWithList((int)leftTime);
                startMinuteText.text = timeStrList[1];
                startSecondText.text = timeStrList[2];
            }
            else
            {
                nextDateText.text = nextMatchDateTime.ToString("yyyy.MM.dd HH:mm");
            }
            if (leftTime < 0)
            {
                if ((DataConvUtil.ServerDateTime - Player.PVPManager.serverLeagueDataDateTime).Seconds > 5)
                {
                    Player.PVPManager.GetNewLeagueData(() =>
                    {
                        RefreshUI();
                        leagueHomeUIAnim.PlayEnter(isInSignStage, false);
                    });
                }
            }

        }

        #endregion
    }
}