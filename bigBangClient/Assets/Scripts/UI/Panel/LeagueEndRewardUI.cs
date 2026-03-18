using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameConfig.Config;
using GameConfig;
using static BigBang.AllStarManager;
using Utils.GameItem;
using System;
using Protocol;
using UnityEngine.UIElements;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang.UI
{
    public class LeagueEndRewardUIProperties : WindowProperties
    {
        public LeagueHistoryData data;
        public Action Callback = null;
        public LeagueEndRewardUIProperties(LeagueHistoryData leagueHistoryData, Action callback = null)
        {
            data = leagueHistoryData;
            Callback = callback;
        }
    }
    public class LeagueEndRewardUI : AWindowController<LeagueEndRewardUIProperties>
    {
        #region 初始化与监听
        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClickClose;
            getButton.OnClick += OnClickClose;
        }
        protected override void RemoveListeners()
        {
            closeBtn.OnClick -= OnClickClose;
            getButton.OnClick -= OnClickClose;
        }
        [SerializeField] private ScrollRect scrollView = null;
        protected override void OnPropertiesSet()
        {
            SetReward();
            scrollView.enabled = false;
            UnityTimer.Timer.Register(this.gameObject, 0.2f, () =>
            {
                scrollView.enabled = true;
                scrollView.verticalNormalizedPosition = 1f;
            });
        }
        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private BabuButton getButton = null;
        private void OnClickClose(BabuButton _)
        {
            NetworkManager.Instance.ReceiveLeagueSettleReward((ReceiveLeagueSettleRewardResponse receiveLeagueSettleRewardResponse) =>
            {
                try
                {
                    if (receiveLeagueSettleRewardResponse.ReceiveSucceed)
                    {
                        ShowLeagueEndReward();
                    }
                    else
                    {
                        Properties.Callback?.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                finally
                {
                    UIController.Instance.CloseWindow<LeagueEndRewardUI>();
                    Player.PVPManager.updatePVPInfoNotify.LeagueSettle = false;
                    Player.PVPManager.RefreshLeagueRedDot();
                }
            });
        }
        #endregion

        #region 设置奖励

        [SerializeField] private List<LeagueEndRewardItem> leagueEndRewardItemList = new();
        private void SetReward()
        {
            int leagueLevel = Properties.data.LeagueLevel;
            int leagueRank = Properties.data.Rank;
            LeagueRewardRankConfig leagueRewardRankConfig = Configs.LeagueRewardRank.GetConfigList().FirstOrDefault((item) =>
            {
                return item.Level == leagueLevel && item.Rank == leagueRank;
            });
            leagueEndRewardItemList[0].SetData(leagueRewardRankConfig.Reward);
            List<bool> kingList = new()
            {
                Properties.data.TopCards.PointKing != null && Properties.data.TopCards.PointKing.LeagueRank == 1,
                Properties.data.TopCards.AssistKing != null && Properties.data.TopCards.AssistKing.LeagueRank == 1,
                Properties.data.TopCards.ReboundKing != null && Properties.data.TopCards.ReboundKing.LeagueRank == 1,
                Properties.data.TopCards.StealKing != null && Properties.data.TopCards.StealKing.LeagueRank == 1,
                Properties.data.TopCards.BlockKing != null && Properties.data.TopCards.BlockKing.LeagueRank == 1
            };
            int kingCount = 0;
            for (int i = 1; i <= 5; i++)
            {
                bool isKing = kingList[i - 1];
                if (isKing)
                {
                    leagueEndRewardItemList[i].gameObject.SetActive(true);
                    LeagueRewardOtherConfig leagueRewardOtherConfig = Configs.LeagueRewardOther.GetConfig(i);
                    leagueEndRewardItemList[i].SetData(leagueRewardOtherConfig.Reward);
                    kingCount++;
                }
                else
                {
                    leagueEndRewardItemList[i].gameObject.SetActive(false);
                    // leagueEndRewardItemList[i].SetNoData();
                }
            }
            SetWindowSize(kingCount);
        }

        [SerializeField] private float windowMaxHeight = 0;
        [SerializeField] private float kingHeight = 0;
        [SerializeField] private float kingSpace = 0;
        [SerializeField] private float windowOtherHeight = 0;
        [SerializeField] private RectTransform background = null;
        private void SetWindowSize(int kingCount)
        {
            float needHeight = kingHeight + (kingHeight + kingSpace) * kingCount + windowOtherHeight;
            float realHeight = Utility.KeepInRange(needHeight, kingHeight + windowOtherHeight, windowMaxHeight);
            background.SetSizeDeltaHeight(realHeight);
        }

        #endregion

        #region 领取奖励

        private void ShowLeagueEndReward()
        {

            //联赛排名奖励
            LeagueRewardRankConfig leagueRewardRankConfig = Configs.LeagueRewardRank.GetConfigList().FirstOrDefault((item) =>
            {
                return item.Level == Properties.data.LeagueLevel && item.Rank == Properties.data.Rank;
            });
            List<GameItem> leagueEndRewardList = GameItemUtils.CreateGameItems(leagueRewardRankConfig.Reward).ToList();

            //各种王奖励
            List<GameItem> kingRewardList = new();
            List<int> kingRewardIdList = new();
            if (Properties.data.TopCards.PointKing != null && Properties.data.TopCards.PointKing.LeagueRank == 1) kingRewardIdList.Add(1);
            if (Properties.data.TopCards.AssistKing != null && Properties.data.TopCards.AssistKing.LeagueRank == 1) kingRewardIdList.Add(2);
            if (Properties.data.TopCards.ReboundKing != null && Properties.data.TopCards.ReboundKing.LeagueRank == 1) kingRewardIdList.Add(3);
            if (Properties.data.TopCards.StealKing != null && Properties.data.TopCards.StealKing.LeagueRank == 1) kingRewardIdList.Add(4);
            if (Properties.data.TopCards.BlockKing != null && Properties.data.TopCards.BlockKing.LeagueRank == 1) kingRewardIdList.Add(5);
            foreach (int kingRewardId in kingRewardIdList)
            {
                LeagueRewardOtherConfig leagueRewardOtherConfig = Configs.LeagueRewardOther.GetConfig(kingRewardId);
                List<GameItem> oneKingRewardList = GameItemUtils.CreateGameItems(leagueRewardOtherConfig.Reward).ToList();
                kingRewardList.AddRange(oneKingRewardList);
            }

            //奖励汇总
            List<GameItem> totalGameItemList = new();
            totalGameItemList.AddRange(leagueEndRewardList);
            totalGameItemList.AddRange(kingRewardList);
            //totalGameItemList = GameItemUtils.MergeGameItemList(totalGameItemList);

            //展示奖励

            var properties = new InventoryObtainedUIProperties(totalGameItemList, Properties.Callback);
            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);

        }

        #endregion

    }
}