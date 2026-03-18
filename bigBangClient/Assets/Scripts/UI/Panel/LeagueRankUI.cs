using System;
using System.Collections.Generic;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System.Linq;
using Coffee.UIEffects;
using GameConfig.Config;
using Babu.Config;
using Babu;
using static BigBang.SpriteNames;
using Protocol;
using static BigBang.AllStarManager;

namespace BigBang.UI
{
    public class LeagueRankUI : APanelController
    {
        #region 初始化
        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClickClose;
        }
        protected override void RemoveListeners()
        {
            closeBtn.OnClick -= OnClickClose;
        }

        List<ChampionTeamData> rankInfoList = null;
        protected override void OnPropertiesSet()
        {
            leagueRankUIAnim.Init();
            leagueRankUIAnim.PlayEnter();
            leagueRankUIAnim.PlayEnterTop();
            NetworkManager.Instance.GetLeagueChampionRank((GetLeagueChampionRankResponse getLeagueChampionRankResponse) =>
            {
                rankInfoList = getLeagueChampionRankResponse.RankList.ToList();
                RefreshUI();
                leagueRankUIAnim.PlayEnterAlpha();
            });

        }
        #endregion

        #region 按钮回调
        [SerializeField] private BabuButton closeBtn = null;
        private void OnClickClose(BabuButton _)
        {
            UIController.Instance.HidePanel<LeagueRankUI>();
        }
        #endregion

        #region 刷新内容
        [SerializeField] private List<LeagueTopRankItem> topRankItemList = null;
        [SerializeField] private LeagueRankAdapter leagueRankAdapter = null;
        [SerializeField] private RectTransform emptyPanel = null;
        [SerializeField] private LeagueRankItem myRankItem = null;
        [SerializeField] private LeagueRankUIAnim leagueRankUIAnim = null;

        private void RefreshUI()
        {

            for (int i = 0; i < topRankItemList.Count; i++)
            {
                LeagueTopRankItem leagueTopRankItem = topRankItemList[i];
                ChampionTeamData championTeamData = i >= rankInfoList.Count ? null : rankInfoList[i];
                leagueTopRankItem.SetData(championTeamData);
            }
            bool isEmpty = rankInfoList.Count <= 0;
            leagueRankAdapter.gameObject.SetActive(!isEmpty);
            emptyPanel.gameObject.SetActive(isEmpty);
            bool needShowSelf = !isEmpty;
            myRankItem.gameObject.SetActive(needShowSelf);
            (leagueRankAdapter.transform as RectTransform).SetBottom(needShowSelf ? 246f : 146f);
            if (!isEmpty)
            {
                leagueRankAdapter.SetData(rankInfoList);
            }
            if (needShowSelf)
            {
                ChampionTeamData championTeamData = rankInfoList.FirstOrDefault(info => info.Team.TeamId == Player.GbId);
                myRankItem.SetData(championTeamData, true, -1);
            }
        }


        #endregion
    }
}