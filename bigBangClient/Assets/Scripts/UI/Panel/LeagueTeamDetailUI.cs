using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System.Collections.Generic;
using Protocol;
using BigBang.Animation;
using TMPro;
using Babu;
using Utils;

namespace BigBang.UI
{
    public class LeagueTeamDetailUIProperties : WindowProperties
    {
        public LeagueCourseItemData leagueCourseItemData = null;
        public bool isNewMatch = false;
        public LeagueTeamDetailUIProperties(LeagueCourseItemData leagueCourseItemData, bool isNewMatch = false)
        {
            this.leagueCourseItemData = leagueCourseItemData;
            this.isNewMatch = isNewMatch;
        }
    }
    public class LeagueTeamDetailUI : AWindowController<LeagueTeamDetailUIProperties>
    {
        #region 初始化与监听
        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private BabuButton playButton = null;
        [SerializeField] private RectTransform leftWinPanel = null;
        [SerializeField] private RectTransform rightWinPanel = null;
        [SerializeField] private TMP_Text redNumTextLeftWin = null;
        [SerializeField] private TMP_Text blueNumTextLeftWin = null;
        [SerializeField] private TMP_Text redNumTextRightWin = null;
        [SerializeField] private TMP_Text blueNumTextRightWin = null;
        [SerializeField] private ClubIconItem leftClubIconImage = null;
        [SerializeField] private ClubIconItem rightClubIconImage = null;
        [SerializeField] private TMP_Text leftClubNameText = null;
        [SerializeField] private TMP_Text rightClubNameText = null;
        [SerializeField] private TMP_Text leftCombatText = null;
        [SerializeField] private TMP_Text rightCombatText = null;
        [SerializeField] private TMP_Text timeText = null;
        [SerializeField] private BabuButton confirmBtn = null;

        [SerializeField] private Color nameTextColorLightNormal = new();
        [SerializeField] private Color nameTextColorLightSelf = new();

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClickCloseBtn;
            confirmBtn.OnClick += OnClickCloseBtn;
            playButton.OnClick += OnClickPlayButton;
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClickCloseBtn;
            confirmBtn.OnClick -= OnClickCloseBtn;
            playButton.OnClick -= OnClickPlayButton;
        }
        #endregion

        #region 退出与保存
        private void OnClickCloseBtn(BabuButton _)
        {
            UIController.Instance.CloseWindow<LeagueTeamDetailUI>();
        }
        private void OnClickPlayButton(BabuButton button)
        {
            HundredManager.Instance.GetFight(Properties.leagueCourseItemData.FightId, (FightInfo fightInfo) =>
            {
                UIController.Instance.CloseWindow<HundredTeamDetailUI>();
                Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.LeagueUI_LeagueCoursePad;
                Player.BattleManager.SetFightInfo(FightType.League, fightInfo);
                UIController.Instance.CloseWindow<LeagueTeamDetailUI>();
                Player.BattleManager.StartPlayFight();
            });
        }
        #endregion

        #region 数据刷新与显示刷新
        [SerializeField] private TMP_Text titleText = null;
        [SerializeField] private TMP_Text countNumText = null;
        [SerializeField] private Image leftLoseImage = null;
        [SerializeField] private Image leftWinImage = null;
        [SerializeField] private Image rightLoseImage = null;
        [SerializeField] private Image rightWinImage = null;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            titleText.text = Properties.isNewMatch ? "我的上一场比赛" : "比赛战报";
            countNumText.text = "第{0}轮".SafeFormat(Properties.leagueCourseItemData.Round);

            leftClubIconImage.SetIcon(Properties.leagueCourseItemData.AwayTeam.TeamIcon);
            rightClubIconImage.SetIcon(Properties.leagueCourseItemData.HomeTeam.TeamIcon);
            leftClubNameText.text = Properties.leagueCourseItemData.AwayTeam.TeamName;
            bool isAwaySelf = Properties.leagueCourseItemData.AwayTeam.TeamId == Player.GbId;
            leftClubNameText.color = isAwaySelf ? nameTextColorLightSelf : nameTextColorLightNormal;
            rightClubNameText.text = Properties.leagueCourseItemData.HomeTeam.TeamName;
            bool isHomeSelf = Properties.leagueCourseItemData.HomeTeam.TeamId == Player.GbId;
            rightClubNameText.color = isHomeSelf ? nameTextColorLightSelf : nameTextColorLightNormal;
            leftCombatText.text = "";
            rightCombatText.text = "";

            bool isAwayWin = Properties.leagueCourseItemData.AwayGoal > Properties.leagueCourseItemData.HomeGoal;
            leftWinPanel.gameObject.SetActive(isAwayWin);
            rightWinPanel.gameObject.SetActive(!isAwayWin);
            if (isAwayWin)
            {
                redNumTextLeftWin.text = Properties.leagueCourseItemData.AwayGoal.ToString();
                blueNumTextLeftWin.text = Properties.leagueCourseItemData.HomeGoal.ToString();
            }
            else
            {
                redNumTextRightWin.text = Properties.leagueCourseItemData.AwayGoal.ToString();
                blueNumTextRightWin.text = Properties.leagueCourseItemData.HomeGoal.ToString();
            }

            leftWinImage.gameObject.SetActive(Properties.isNewMatch && isAwaySelf && isAwayWin);
            leftLoseImage.gameObject.SetActive(Properties.isNewMatch && isAwaySelf && !isAwayWin);
            rightWinImage.gameObject.SetActive(Properties.isNewMatch && isHomeSelf && !isAwayWin);
            rightLoseImage.gameObject.SetActive(Properties.isNewMatch && isHomeSelf && isAwayWin);

            timeText.text = TimeUtils.GetUnixTimeString(Properties.leagueCourseItemData.Time, "yyyy.MM.dd HH:mm");
        }

        #endregion

    }
}