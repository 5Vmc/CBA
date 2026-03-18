using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using Protocol;
using System.Linq;
using TMPro;
using System;
using static BigBang.BattleManager;

namespace BigBang.UI
{
    public class LeagueUIProperties : PanelProperties
    {
        public int LeagueID { get; set; }
        public string LeagueName { get; set; }
        public int LeagueLevel { get; set; }

        public LeagueUIProperties(int leagueID, string leagueName, int leagueLevel)
        {
            this.LeagueID = leagueID;
            LeagueName = leagueName;
            LeagueLevel = leagueLevel;
        }
    }

    public class LeagueUI : APanelController<LeagueUIProperties>
    {
        [SerializeField] private LeagueScoreboardPad leagueScoreboardPad;
        [SerializeField] private LeagueCoursePad leagueCoursePad;
        [SerializeField] private LeaguePlayerIntegralPad leaguePlayerIntegralPad;
        [SerializeField] private Button closeBtn;
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private BabuToggle playerToggle;
        [SerializeField] private BabuToggle scheduleToggle;
        [SerializeField] private BabuToggle integralToggle;
        [SerializeField] private TMP_Text leaugueNameText;

        //private GetLeagueCardRankResponse cardRank;
        //private GetLeagueCourseResponse courseResponse;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            toggleGroup.OnValueChanged += OnToggleChanged;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            toggleGroup.OnValueChanged -= OnToggleChanged;
        }
        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            if (oldToggle == playerToggle) OnPlayerDeselect();
            if (oldToggle == scheduleToggle) OnCourseDeselect();
            if (oldToggle == integralToggle) OnIntegralDeselect();
            if (newToggle == playerToggle) OnPlayerSelect();
            if (newToggle == scheduleToggle) OnCourseSelect();
            if (newToggle == integralToggle) OnIntegralSelect();
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            leaugueNameText.text = Properties.LeagueName;
            if (Player.BattleManager.battleEnterType == BattleManager.BattleEnterType.LeagueUI)
            {
                toggleGroup.Switch(scheduleToggle, true);
                Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.Unknown;
            }
            else
            {
                // 默认打开积分榜
                toggleGroup.Switch(integralToggle, true);
            }
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            UIController.Instance.HidePanel<LeagueUI>();
        }

        // 打开球员榜
        private void OnPlayerSelect()
        {
            leaguePlayerIntegralPad.gameObject.SetActive(true);
            leaguePlayerIntegralPad.InitAnim();
            NetworkManager.Instance.GetLeagueCardRank(CompitionID.League, Properties.LeagueID, response =>
            {
                leaguePlayerIntegralPad.SetData(response, Properties.LeagueName);
            });
        }

        // 打开赛程榜
        private void OnCourseSelect()
        {
            leagueCoursePad.gameObject.SetActive(true);
            leagueCoursePad.Anim.Init();
            NetworkManager.Instance.GetLeagueCourse(CompitionID.League, Properties.LeagueID, GetLeagueCourseType.All, response =>
            {
                Player.BattleManager.getLeagueCourseResponse = response;
                leagueCoursePad.SetData(response, Properties.LeagueName, CompitionID.League, BattleEnterType.LeagueUI);
                Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.LeagueUI_LeagueCoursePad;
                leagueCoursePad.Anim.PlayEnter();
            });
        }

        // 打开积分榜
        private void OnIntegralSelect()
        {
            leagueScoreboardPad.gameObject.SetActive(true);
            leagueScoreboardPad.Anim.Init();
            NetworkManager.Instance.GetLeagueScorebar(CompitionID.League, Properties.LeagueID, response =>
            {
                leagueScoreboardPad.SetData(response.LeagueScorebarTeamList.ToList(), Properties.LeagueName);
                leagueScoreboardPad.Anim.PlayEnter();
            });
        }

        // 关闭球员榜
        private void OnPlayerDeselect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
            leaguePlayerIntegralPad.gameObject.SetActive(false);
        }

        // 关闭赛程榜
        private void OnCourseDeselect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
            leagueCoursePad.gameObject.SetActive(false);
        }

        // 关闭积分榜
        private void OnIntegralDeselect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
            leagueScoreboardPad.gameObject.SetActive(false);
        }
    }
}