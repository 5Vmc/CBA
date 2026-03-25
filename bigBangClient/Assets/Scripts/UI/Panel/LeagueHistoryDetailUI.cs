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

namespace BigBang.UI
{

    [Serializable]
    public class LeagueHistoryDetailUIProperties : WindowProperties
    {
        public LeagueHistoryData data;
        public bool isLeagueEnd = false;
        public Action Callback = null;
        public LeagueHistoryDetailUIProperties(LeagueHistoryData leagueHistoryData, bool isLeagueEnd = false, Action callback = null)
        {
            data = leagueHistoryData;
            this.isLeagueEnd = isLeagueEnd;
            Callback = callback;
        }
    }

    public class LeagueHistoryDetailUI : AWindowController<LeagueHistoryDetailUIProperties>
    {
        #region 初始化
        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            getButton.OnClick += OnClose;
            confirmBtn.OnClick += OnClose;
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            getButton.OnClick -= OnClose;
            confirmBtn.OnClick -= OnClose;
        }
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            SetRankInfoPanel();
            SetAveragePanel();
            SetKingPanel();
            SetButtonPanel();
        }
        #endregion

        #region 排名信息

        [SerializeField] private TMP_Text leagueLevelText = null;
        [SerializeField] private TMP_Text dateText = null;
        [SerializeField] private TMP_Text teamNameText = null;
        [SerializeField] private ClubIconItem teamIcon = null;
        [SerializeField] private Image cupImage1 = null;
        [SerializeField] private Image cupImage2 = null;
        [SerializeField] private Image cupImage3 = null;
        [SerializeField] private TMP_Text rankNumText = null;
        [SerializeField] private TMP_Text rankPrefixText = null;
        [SerializeField] private Image rank1Image = null;
        [SerializeField] private Image rank2Image = null;
        [SerializeField] private Image rank3Image = null;
        [SerializeField] private TMP_Text winLoseText = null;
        [SerializeField] private TMP_Text downText = null;
        [SerializeField] private ImageFont upImageFont = null;

        public void SetRankInfoPanel()
        {
            leagueLevelText.text = "我的联赛报告({0}级)".SafeFormat(Properties.data.LeagueLevel);
            dateText.text = "{0}-{1}".SafeFormat(TimeUtils.GetUnixTimeString(Properties.data.StartTime, "yyyy.MM.dd"), TimeUtils.GetUnixTimeString(Properties.data.EndTime, "yyyy.MM.dd"));
            teamNameText.text = Player.Name;
            teamIcon.SetIcon(Player.Icon);
            cupImage1.gameObject.SetActive(Properties.data.Rank == 1);
            cupImage2.gameObject.SetActive(Properties.data.Rank == 2);
            cupImage3.gameObject.SetActive(Properties.data.Rank == 3);
            bool isFirst3 = Properties.data.Rank <= 3;
            rankNumText.gameObject.SetActive(!isFirst3);
            rankPrefixText.gameObject.SetActive(isFirst3);
            rank1Image.gameObject.SetActive(Properties.data.Rank == 1);
            rank2Image.gameObject.SetActive(Properties.data.Rank == 2);
            rank3Image.gameObject.SetActive(Properties.data.Rank == 3);
            if (!isFirst3) rankNumText.text = "第<size=40>{0}</size>名".SafeFormat(Properties.data.Rank);
            winLoseText.text = "<size=40>{0}</size>胜 <size=40>{1}</size>负".SafeFormat(Properties.data.Win, Properties.data.Failed);
            bool isUp = Properties.data.Rank <= 3 && Properties.data.LeagueLevel < Configs.LeagueRewardRank.GetConfigList()[^1].Level;
            bool isDown = Properties.data.Rank >= 18 && Properties.data.LeagueLevel > Configs.LeagueRewardRank.GetConfigList()[0].Level;
            upImageFont.gameObject.SetActive(isUp);
            downText.gameObject.SetActive(isDown);
            if (isUp) upImageFont.text = "晋级至{0}级联赛".SafeFormat(Properties.data.LeagueLevel + 1);
            if (isDown) downText.text = "降级至{0}级联赛".SafeFormat(Properties.data.LeagueLevel - 1);
        }

        #endregion

        #region 平均成绩

        [SerializeField] private List<LeagueHistoryDetailAverageItem> leagueHistoryDetailAverageItemList = new();

        private void SetAveragePanel()
        {
            leagueHistoryDetailAverageItemList[0].numText.text = Properties.data.Point.ToString();
            leagueHistoryDetailAverageItemList[1].numText.text = Properties.data.Assist.ToString();
            leagueHistoryDetailAverageItemList[2].numText.text = Properties.data.Rebound.ToString();
            leagueHistoryDetailAverageItemList[3].numText.text = Properties.data.Steal.ToString();
            leagueHistoryDetailAverageItemList[4].numText.text = Properties.data.Block.ToString();
        }

        #endregion

        #region 各种王

        [SerializeField] private List<LeagueHistoryDetailKingItem> leagueHistoryDetailKingItemList = new();

        private void SetKingPanel()
        {
            List<TeamTopCardData> teamTopCardDataList = new()
            {
                Properties.data.TopCards.PointKing,
                Properties.data.TopCards.AssistKing,
                Properties.data.TopCards.ReboundKing,
                Properties.data.TopCards.StealKing,
                Properties.data.TopCards.BlockKing,
            };
            for (int i = 0; i < 5; i++)
            {
                LeagueHistoryDetailKingItem leagueHistoryDetailKingItem = leagueHistoryDetailKingItemList[i];
                TeamTopCardData teamTopCardData = teamTopCardDataList[i];
                _ = leagueHistoryDetailKingItem.SetDataAsync(teamTopCardData, i);
            }
        }

        #endregion

        #region 关闭按钮

        [SerializeField] private Button closeBtn = null;
        [SerializeField] private BabuButton getButton = null;
        [SerializeField] private BabuButton confirmBtn = null;

        private void SetButtonPanel()
        {
            getButton.gameObject.SetActive(Properties.isLeagueEnd);
            confirmBtn.gameObject.SetActive(!Properties.isLeagueEnd);
        }

        private void OnClose()
        {
            if (Properties.isLeagueEnd)
            {
                UIController.Instance.OpenWindow<LeagueEndRewardUI>(new LeagueEndRewardUIProperties(Properties.data, Properties.Callback));
            }
            else
            {
                Properties.Callback?.Invoke();
            }
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<LeagueHistoryDetailUI>();
        }
        private void OnClose(BabuButton _)
        {
            OnClose();
        }

        #endregion

    }
}
