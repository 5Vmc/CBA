using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System;
using System.Collections.Generic;
using Babu;
using Protocol;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;
using GameConfig.Config;
using GameConfig;
using System.Linq;
using System.Text;

namespace BigBang.UI
{

    [Serializable]
    public class LeagueStartNoticeUIProperties : WindowProperties
    {
        public Action Callback = null;
        public LeagueStartNoticeUIProperties(Action callback = null)
        {
            Callback = callback;
        }
    }

    public class LeagueStartNoticeUI : AWindowController<LeagueStartNoticeUIProperties>
    {
        #region 初始化
        protected override void AddListeners()
        {
            base.AddListeners();
            fullScreenButton.onClick.AddListener(OnClose);
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            fullScreenButton.onClick.RemoveListener(OnClose);
        }

        [SerializeField] private ScrollRect tipScrollView = null;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            SetInfo();

            tipScrollView.ScroolToTop(0);
            tipScrollView.enabled = false;
            UnityTimer.Timer.Register(this.gameObject, 0.2f, () =>
            {
                tipScrollView.enabled = true;
            });
        }
        #endregion

        #region 拼接信息

        [SerializeField] private ScrollRect scrollView = null;
        [SerializeField] private VerticalLayoutGroup content = null;
        [SerializeField] private TMP_Text firstText = null;
        [SerializeField] private TMP_Text secondText = null;

        private readonly string strStartPrefab = "    {0}级别联赛已于<color=#F0610F>{1}</color>正式开启。目前进行到<color=#F0610F>第{2}轮</color>，您已取得<color=#F0610F>{3}胜{4}负</color>的成绩，暂时<color=#F0610F>排名第{5}</color>。";
        private readonly string strMidPrefab1 = "\n    您的球员发挥出色";
        private readonly string strMidPrefab2 = "，球员<color=#F0610F>{0}</color>在联赛{1}榜中排名第<color=#F0610F>{2}</color>";
        private readonly string strMidPrefab3 = "。";
        private readonly string strEndPrefab = "\n    请您及时布置阵容与战术，调整好球员的身体和状态，准备下一轮比赛吧！";
        public void SetInfo()
        {
            firstText.text = "尊敬的<color=#31B70D><size=40>{0}</size></color>俱乐部:".SafeFormat(Player.Name);

            StringBuilder sb = new();

            int rank = 0;
            int win = 0;
            int lose = 0;
            if (Player.PVPManager.serverLeagueData.GamePerviewData.AwayTeam.Team.TeamId == Player.GbId)
            {
                win = Player.PVPManager.serverLeagueData.GamePerviewData.AwayTeam.Win;
                lose = Player.PVPManager.serverLeagueData.GamePerviewData.AwayTeam.Failed;
                rank = Player.PVPManager.serverLeagueData.GamePerviewData.AwayTeam.Rank;
            }
            if (Player.PVPManager.serverLeagueData.GamePerviewData.HomeTeam.Team.TeamId == Player.GbId)
            {
                win = Player.PVPManager.serverLeagueData.GamePerviewData.HomeTeam.Win;
                lose = Player.PVPManager.serverLeagueData.GamePerviewData.HomeTeam.Failed;
                rank = Player.PVPManager.serverLeagueData.GamePerviewData.HomeTeam.Rank;
            }
            sb.Append(strStartPrefab.SafeFormat(Player.PVPManager.serverLeagueData.LeagueInfo.LeagueLevel, TimeUtils.GetUnixTimeString(Player.PVPManager.serverLeagueData.LeagueInfo.StartTime, "yyyy年MM月dd日HH时mm分"), Player.PVPManager.serverLeagueData.LeagueInfo.LeagueRoundId, win, lose, rank));

            bool isFirstKing = false;
            List<TeamTopCardData> teamTopCardDataList = new()
            {
                Player.PVPManager.serverLeagueData.TopCards.PointKing,
                Player.PVPManager.serverLeagueData.TopCards.AssistKing,
                Player.PVPManager.serverLeagueData.TopCards.ReboundKing,
                Player.PVPManager.serverLeagueData.TopCards.StealKing,
                Player.PVPManager.serverLeagueData.TopCards.BlockKing
            };
            for (int i = 0; i < 5; i++)
            {
                TeamTopCardData teamTopCardData = teamTopCardDataList[i];
                if (teamTopCardData == null) continue;
                if (teamTopCardData.LeagueRank <= 20)
                {
                    if (isFirstKing == false)
                    {
                        isFirstKing = true;
                        sb.Append(strMidPrefab1);
                    }
                    CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(teamTopCardData.CardId);
                    string rankName = "";
                    switch (i)
                    {
                        case 0: rankName = "得分"; break;
                        case 1: rankName = "助攻"; break;
                        case 2: rankName = "篮板"; break;
                        case 3: rankName = "抢断"; break;
                        case 4: rankName = "盖帽"; break;
                    }
                    sb.Append(strMidPrefab2.SafeFormat(PlayerCard.GetFullName(cardModelConfig), rankName, teamTopCardData.LeagueRank));
                }
            }
            if (isFirstKing == true) sb.Append(strMidPrefab3);

            sb.Append(strEndPrefab);
            secondText.text = sb.ToString();

            LayoutRebuilder.ForceRebuildLayoutImmediate(firstText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(secondText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.transform as RectTransform);
        }

        #endregion

        #region 关闭按钮

        [SerializeField] private Button fullScreenButton = null;

        private void OnClose()
        {
            Properties.Callback?.Invoke();
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<LeagueStartNoticeUI>();
        }

        #endregion

    }
}