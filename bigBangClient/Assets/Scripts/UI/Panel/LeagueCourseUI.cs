using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using Protocol;
using System.Linq;
using TMPro;
using System;
using static BigBang.BattleManager;
using Utils;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using BigBang.Animation;

namespace BigBang.UI
{
    public class LeagueCourseUI : APanelController
    {
        #region 初始化

        [SerializeField] private TMP_Text leagueRankNumText = null;
        [SerializeField] private HorizontalLayoutGroup winLoseLayout = null;
        [SerializeField] private TMP_Text leagueWinNumText = null;
        [SerializeField] private TMP_Text leagueLoseNumText = null;
        [SerializeField] private List<HundredFight1WinLoseItem> hundredFight1WinLoseItemList = new();
        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private BabuToggleGroup bottomToggleGroup = null;
        [SerializeField] private LeagueCourseUIAnim leagueCourseUIAnim = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
            bottomToggleGroup.OnValueChanged += OnToggleChanged;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
            bottomToggleGroup.OnValueChanged -= OnToggleChanged;
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            leagueCourseUIAnim.Init();
            leagueCourseUIAnim.InitTopBar();
            leagueCourseUIAnim.InitAdapter();
            RefreshWinLose(Player.BattleManager.getLeagueCourseResponse);
            bottomToggleGroup.Switch((int)SubUIID.All);
            RefreshRank();
            leagueCourseUIAnim.PlayEnterTopBar();
        }

        private void OnClose(BabuButton _)
        {
            UIController.Instance.HidePanel<LeagueCourseUI>();
        }

        #endregion

        #region 顶部信息

        private void RefreshRank()
        {
            int rank = 0;
            if (Player.PVPManager.serverLeagueData?.GamePerviewData?.HomeTeam?.Team.TeamId == Player.GbId) rank = Player.PVPManager.serverLeagueData.GamePerviewData.HomeTeam.Rank;
            if (Player.PVPManager.serverLeagueData?.GamePerviewData?.AwayTeam?.Team.TeamId == Player.GbId) rank = Player.PVPManager.serverLeagueData.GamePerviewData.AwayTeam.Rank;
            bool hasRank = rank > 0;
            leagueRankNumText.gameObject.SetActive(hasRank);
            noInfoText1.gameObject.SetActive(!hasRank);
            leagueRankNumText.text = !hasRank ? "-" : "第{0}名".SafeFormat(rank);
        }

        [SerializeField] private HorizontalLayoutGroup recentWinLoseLayout = null;
        [SerializeField] private TMP_Text noInfoText1 = null;
        [SerializeField] private TMP_Text noInfoText2 = null;
        [SerializeField] private TMP_Text noInfoText3 = null;
        private void RefreshWinLose(GetLeagueCourseResponse getLeagueCourseResponse)
        {
            List<LeagueCourseItemData> fightDataList = new();
            if (getLeagueCourseResponse != null)
            {
                foreach (LeagueCourseItemData leagueCourseItemData in getLeagueCourseResponse.LeagueCourseItemList)
                {
                    if (leagueCourseItemData.HomeTeam == null || leagueCourseItemData.AwayTeam == null) continue;
                    if (leagueCourseItemData.HomeTeam.TeamId == Player.GbId || leagueCourseItemData.AwayTeam.TeamId == Player.GbId)
                    {
                        if (leagueCourseItemData.HomeGoal > -1 && leagueCourseItemData.AwayGoal > -1)
                            fightDataList.Add(leagueCourseItemData);
                    }
                }
            }
            fightDataList.Reverse();
            int win = fightDataList.Count((data) =>
            {
                if (data.HomeTeam.TeamId == Player.GbId && data.HomeGoal > data.AwayGoal) return true;
                if (data.AwayTeam.TeamId == Player.GbId && data.HomeGoal < data.AwayGoal) return true;
                return false;
            });
            int lose = fightDataList.Count((data) =>
            {
                if (data.HomeTeam.TeamId == Player.GbId && data.HomeGoal < data.AwayGoal) return true;
                if (data.AwayTeam.TeamId == Player.GbId && data.HomeGoal > data.AwayGoal) return true;
                return false;
            });

            bool isMeNotFight = win == 0 && lose == 0;
            winLoseLayout.gameObject.SetActive(!isMeNotFight);
            recentWinLoseLayout.gameObject.SetActive(!isMeNotFight);
            noInfoText2.gameObject.SetActive(isMeNotFight);
            noInfoText3.gameObject.SetActive(isMeNotFight);
            if (isMeNotFight) return;

            for (int i = 0; i < 5; i++)
            {
                HundredFight1WinLoseItem hundredFight1WinLoseItem = hundredFight1WinLoseItemList[i];
                bool hasData = i < fightDataList.Count;
                if (hasData)
                {
                    LeagueCourseItemData leagueCourseItemData = fightDataList[i];
                    bool isWin = HundredManager.Instance.IsFightWin(leagueCourseItemData);
                    hundredFight1WinLoseItem.winImage.gameObject.SetActive(isWin);
                    hundredFight1WinLoseItem.loseImage.gameObject.SetActive(!isWin);
                    hundredFight1WinLoseItem.emptyImage.gameObject.SetActive(false);
                }
                else
                {
                    hundredFight1WinLoseItem.winImage.gameObject.SetActive(false);
                    hundredFight1WinLoseItem.loseImage.gameObject.SetActive(false);
                    hundredFight1WinLoseItem.emptyImage.gameObject.SetActive(false);
                }
            }

            leagueWinNumText.text = "{0}胜".SafeFormat(win);
            leagueLoseNumText.text = "{0}负".SafeFormat(lose);
            LayoutRebuilder.ForceRebuildLayoutImmediate(leagueWinNumText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(leagueLoseNumText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(winLoseLayout.transform as RectTransform);
        }

        #endregion

        #region 切换页签

        public enum SubUIID
        {
            Mine = 0,
            All = 1,
        }

        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            int selectedIndex = bottomToggleGroup.EnableIndex;
            ShowPad((SubUIID)selectedIndex);
        }
        private SubUIID subUIID = SubUIID.All;
        private void ShowPad(SubUIID padIndex)
        {
            subUIID = padIndex;
            switch (padIndex)
            {
                case SubUIID.Mine: OnShowMine(); break;
                case SubUIID.All: OnShowAll(); break;
            }
        }

        private void OnShowMine()
        {
            OnCourseSelect(GetLeagueCourseType.Mine);
        }
        private void OnShowAll()
        {
            OnCourseSelect(GetLeagueCourseType.All);
        }

        #endregion

        #region 赛程列表

        [SerializeField] private LeagueCourseAdapter adapter;
        // 打开赛程榜
        private void OnCourseSelect(int getLeagueCourseType)
        {
            if ((TeamState)Player.PVPManager.serverLeagueData.TeamState != TeamState.MATCHING) return;
            NetworkManager.Instance.GetLeagueCourse(CompitionID.League, Player.PVPManager.serverLeagueData.LeagueInfo.LeagueId, getLeagueCourseType, response =>
            {
                Player.BattleManager.getLeagueCourseResponse = response;
                adapter.SetData(response, CompitionID.League, BattleEnterType.LeagueUI_LeagueCoursePad, subUIID);
                Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.LeagueUI_LeagueCoursePad;
                RefreshWinLose(response);
                leagueCourseUIAnim.InitAdapter();
                leagueCourseUIAnim.PlayEnterAdapter(false);
            });
        }

        #endregion

    }
}