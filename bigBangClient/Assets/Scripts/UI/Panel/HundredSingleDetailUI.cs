using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System.Collections.Generic;
using Protocol;
using BigBang.Animation;
using TMPro;
using Utils;
using GameConfig;

namespace BigBang.UI
{
    public class HundredSingleDetailUIProperties : WindowProperties
    {
        public string fightId = "";
        public bool isAway = false;
        public int compitionId = 0;
        public string teamId = "";
        public CourseTeamData courseTeamData = null;
        public HundredSingleDetailUIProperties(string fightId, bool isAway, int compitionId, string teamId, CourseTeamData courseTeamData = null)
        {
            this.courseTeamData = courseTeamData;
            this.fightId = fightId;
            this.isAway = isAway;
            this.compitionId = compitionId;
            this.teamId = teamId;
        }
    }
    public class HundredSingleDetailUI : AWindowController<HundredSingleDetailUIProperties>
    {
        #region 初始化与监听
        [SerializeField] private ClubIconItem leftClubIconImage = null;
        [SerializeField] private TMP_Text clubNameText = null;
        [SerializeField] private TMP_Text combatNumText = null;
        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private List<HundredTeamDetailCardItem> cardItemList = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClickCloseBtn;
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClickCloseBtn;
        }
        #endregion

        #region 退出与保存
        private void OnClickCloseBtn(BabuButton _)
        {
            UIController.Instance.CloseWindow<HundredSingleDetailUI>();
        }
        #endregion

        #region 数据刷新与显示刷新
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);

            ShowInfo(false);

            Refresh();
        }
        private void RefreshByFight()
        {
            HundredManager.Instance.GetFightNoTip(Properties.fightId, (FightInfo fightInfo) =>
            {
                if (fightInfo == null)
                {
                    RefreshByTeam();
                    return;
                }

                leftClubIconImage.SetIcon(Properties.isAway ? fightInfo.Teams.Away.TeamIcon : fightInfo.Teams.Home.TeamIcon);
                clubNameText.text = Properties.isAway ? fightInfo.Teams.Away.TeamName : fightInfo.Teams.Home.TeamName;
                combatNumText.text = Properties.isAway ? fightInfo.Teams.Away.Strength.ToString("N0") : fightInfo.Teams.Home.Strength.ToString("N0");

                for (int i = 0; i < 5; i++)
                {
                    HundredTeamDetailCardItem cardItem = cardItemList[i];
                    Protocol.FightCard fightCard = (Properties.isAway ? fightInfo.Teams.Away : fightInfo.Teams.Home).CourtCard[i];
                    cardItem.SetData(fightCard);
                }

                ShowInfo(true);
            });
        }
        private void RefreshByTeam()
        {
            NetworkManager.Instance.GetCourseTeamData(Properties.compitionId, Properties.teamId, (GetCourseTeamDataResponse getCourseTeamDataResponse) =>
            {
                if (getCourseTeamDataResponse == null)
                {
                    Debug.LogWarning("HundredSingleDetailUI , RefreshByTeam , getCourseTeamDataResponse == null , Properties.compitionId = " + Properties.compitionId + " , Properties.teamId = " + Properties.teamId);
                    UIController.Instance.CloseWindow<HundredSingleDetailUI>();
                    return;
                }
                RefreshByCourseTeamData(getCourseTeamDataResponse.Team);
            });
        }
        private void RefreshByCourseTeamData(CourseTeamData courseTeamData)
        {
            leftClubIconImage.SetIcon(courseTeamData.Team.TeamIcon);
            clubNameText.text = courseTeamData.Team.TeamName;
            combatNumText.text = courseTeamData.Strength.ToString("N0");

            List<PlayerCardMiniInfo> PlayerCardMiniInfoList = new() { null, null, null, null, null };
            foreach (var item in courseTeamData.BoardCardMap)
            {
                int pos = Configs.FormationBoard.GetDataDictionary()[item.Key].SeparatedPosition;
                PlayerCardMiniInfoList[pos - 1] = item.Value;
            }
            for (int i = 0; i < 5; i++)
            {
                HundredTeamDetailCardItem cardItem = cardItemList[i];
                PlayerCardMiniInfo playerCardMiniInfo = PlayerCardMiniInfoList[i];
                cardItem.SetData(playerCardMiniInfo);
            }
            ShowInfo(true);
        }

        private void ShowInfo(bool isShow)
        {
            leftClubIconImage.gameObject.SetActive(isShow);
            combatNumText.gameObject.SetActive(isShow);
            for (int i = 0; i < 5; i++)
            {
                HundredTeamDetailCardItem cardItem = cardItemList[i];
                cardItem.gameObject.SetActive(isShow);
            }
        }
        private void Refresh()
        {
            if (Properties.courseTeamData != null)
            {
                RefreshByCourseTeamData(Properties.courseTeamData);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Properties.fightId))
                {
                    RefreshByTeam();
                }
                else
                {
                    RefreshByFight();
                }
            }
        }

        #endregion

    }
}