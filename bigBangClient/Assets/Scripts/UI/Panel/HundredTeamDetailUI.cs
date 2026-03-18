using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System.Collections.Generic;
using Protocol;
using BigBang.Animation;

namespace BigBang.UI
{
    public class HundredTeamDetailUIProperties : WindowProperties
    {
        public LeagueCourseItemData leagueCourseItemData = null;
        public HundredProgress hundredProgress = HundredProgress.Fight1;
        public bool forceMineLeft = false;
        public FightInfo fightInfo = null;
        public HundredTeamDetailUIProperties(LeagueCourseItemData leagueCourseItemData, HundredProgress hundredProgress, bool forceMineLeft = false, FightInfo fightInfo = null)
        {
            this.fightInfo = fightInfo;
            this.leagueCourseItemData = leagueCourseItemData;
            this.hundredProgress = hundredProgress;
            this.forceMineLeft = forceMineLeft;
        }
    }
    public class HundredTeamDetailUI : AWindowController<HundredTeamDetailUIProperties>
    {
        #region 初始化与监听
        [SerializeField] private HundredHomeUIFight1Item hundredHomeUIFight1Item = null;
        [SerializeField] private ScrollRect scrollView = null;
        [SerializeField] private List<HundredTeamDetailLineItem> hundredTeamDetailLineItemList = new();
        [SerializeField] private BabuButton confirmBtn = null;
        [SerializeField] private BabuButton closeBtn = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClickCloseBtn;
            confirmBtn.OnClick += OnClickCloseBtn;
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClickCloseBtn;
            confirmBtn.OnClick -= OnClickCloseBtn;
        }
        #endregion

        #region 退出与保存
        private void OnClickCloseBtn(BabuButton _)
        {
            UIController.Instance.CloseWindow<HundredTeamDetailUI>();
        }
        #endregion

        #region 数据刷新与显示刷新
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            hundredHomeUIFight1Item.detailButton.gameObject.SetActive(false);
            if (Properties.fightInfo != null)
            {
                hundredHomeUIFight1Item.gameObject.SetActive(false);
                SetData(Properties.fightInfo);
                RefreshHundredTeamDetailLineItemList();
                return;
            }
            else
            {
                scrollView.gameObject.SetActive(false);
                hundredHomeUIFight1Item.gameObject.SetActive(true);
                hundredHomeUIFight1Item.SetData(Properties.leagueCourseItemData, Properties.forceMineLeft);
                //获取战斗数据
                HundredManager.Instance.GetFight(Properties.leagueCourseItemData.FightId, (FightInfo fightInfo) =>
                {
                    scrollView.gameObject.SetActive(true);
                    SetData(fightInfo);
                    RefreshHundredTeamDetailLineItemList();
                });
            }

            scrollView.enabled = false;
            UnityTimer.Timer.Register(this.gameObject, 0.2f, () =>
            {
                scrollView.enabled = true;
            });
        }

        List<HundredTeamDetailLineData> hundredTeamDetailLineDataList = new();
        private void SetData(Protocol.FightInfo fightInfo)//处理战斗数据
        {
            Protocol.FightTeamInfo fightTeamInfoLeft = fightInfo.Teams.Away.TeamId == Player.GbId ? fightInfo.Teams.Away : fightInfo.Teams.Home;
            Protocol.FightTeamInfo fightTeamInfoRight = fightInfo.Teams.Away.TeamId == Player.GbId ? fightInfo.Teams.Home : fightInfo.Teams.Away;
            if (!Properties.forceMineLeft)
            {
                fightTeamInfoLeft = fightInfo.Teams.Away;
                fightTeamInfoRight = fightInfo.Teams.Home;
            }

            hundredTeamDetailLineDataList = new();
            Dictionary<string, Protocol.PlayerStat> serverPlayerIdDic = new();
            foreach (Protocol.PlayerStat playerStat in fightInfo.Result.PlayerStat)
            {
                serverPlayerIdDic.Add(playerStat.PlayerCardId, playerStat);
            }

            for (int i = 0; i < 5; i++)
            {
                Protocol.FightCard fightCardLeft = fightTeamInfoLeft.CourtCard[i];
                Protocol.FightCard fightCardRight = fightTeamInfoRight.CourtCard[i];
                int scoreLeft = serverPlayerIdDic[fightCardLeft.PlayerCardId].Point;
                int scoreRight = serverPlayerIdDic[fightCardRight.PlayerCardId].Point;
                int stageIndex = i;
                bool isLeftWin = scoreLeft > scoreRight;
                HundredTeamDetailCardData redHundredTeamDetailCardData = new()
                {
                    fightCard = fightCardLeft,
                    isWin = isLeftWin,
                };
                HundredTeamDetailCardData blueHundredTeamDetailCardData = new()
                {
                    fightCard = fightCardRight,
                    isWin = !isLeftWin,
                };
                HundredTeamDetailLineData hundredTeamDetailLineData = new HundredTeamDetailLineData()
                {
                    redHundredTeamDetailCardData = redHundredTeamDetailCardData,
                    blueHundredTeamDetailCardData = blueHundredTeamDetailCardData,
                    redScore = scoreLeft,
                    blueScore = scoreRight,
                    fightId = fightInfo.FightId,
                    stageIndex = stageIndex,
                };
                hundredTeamDetailLineDataList.Add(hundredTeamDetailLineData);
            }
        }

        private void RefreshHundredTeamDetailLineItemList()//设置列表信息
        {
            for (int i = 0; i < 5; i++)
            {
                HundredTeamDetailLineItem hundredTeamDetailLineItem = hundredTeamDetailLineItemList[i];
                HundredTeamDetailLineData hundredTeamDetailLineData = hundredTeamDetailLineDataList[i];
                hundredTeamDetailLineItem.SetData(hundredTeamDetailLineData, i, Properties.hundredProgress);
            }
            scrollView.ScroolToTop(0);
        }

        #endregion

    }
}