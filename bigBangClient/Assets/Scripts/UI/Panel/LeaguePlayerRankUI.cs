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
    public class LeaguePlayerRankUI : APanelController
    {
        #region 初始化
        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private LeaguePlayerIntegralAdapter adapter;
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private BabuToggle scoreToggle;
        [SerializeField] private BabuToggle assistToggle;
        [SerializeField] private BabuToggle reboundToggle; //篮板
        [SerializeField] private BabuToggle stealToggle; // 抢断
        [SerializeField] private BabuToggle blockToggle; // 盖帽
        [SerializeField] private TMP_Text valueTitle;
        [SerializeField] private LeaguePlayerRankUIAnim leaguePlayerRankUIAnim;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
            scoreToggle.OnSelect += OnScoreSelect;
            assistToggle.OnSelect += OnAssistSelect;
            stealToggle.OnSelect += OnStealSelect;
            reboundToggle.OnSelect += OnReboundSelect;
            blockToggle.OnSelect += OnBlockSelect;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
            scoreToggle.OnSelect -= OnScoreSelect;
            assistToggle.OnSelect -= OnAssistSelect;
            stealToggle.OnSelect -= OnStealSelect;
            reboundToggle.OnSelect -= OnReboundSelect;
            blockToggle.OnSelect -= OnBlockSelect;
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            leaguePlayerRankUIAnim.InitTopBottomBar();
            leaguePlayerRankUIAnim.PlayEnterTopBottomBar();
            NetworkManager.Instance.GetLeagueCardRank(CompitionID.League, Player.PVPManager.serverLeagueData.LeagueInfo.LeagueId, response =>
            {
                SetData(response);
            });
        }

        private void OnClose(BabuButton _)
        {
            UIController.Instance.HidePanel<LeaguePlayerRankUI>();
        }

        #endregion



        private GetLeagueCardRankResponse data;

        public void InitAnim()
        {
            adapter.InitAnim();
        }

        [SerializeField] private RectTransform emptyPanel = null;
        // 设置联赛数据
        public void SetData(GetLeagueCardRankResponse data)
        {
            this.data = data;
            // 默认显示得分榜
            toggleGroup.Switch(scoreToggle);
            var list = data.GoalsScoredRank.Where(item => item.Point > 0).OrderByDescending(item => item.Point).ToList();
            emptyPanel.gameObject.SetActive(list.Count <= 0);
            adapter.SetData(list);
            adapter.InitAnim();
            adapter.PlayAnim();
        }

        // 显示得分榜
        private void OnScoreSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            adapter.mode = LeaguePlayerIntegralAdapter.ValueMode.Score;
            var list = data.GoalsScoredRank.Where(item => item.Point > 0).OrderByDescending(item => item.Point).ToList();
            emptyPanel.gameObject.SetActive(list.Count <= 0);
            adapter.SetData(list);
            valueTitle.text = "得分";//Lang.Get(LangID.GoalsScoredText);
                                   //adapter.PlayFlash();
            adapter.InitAnim();
            adapter.PlayAnim();
        }

        // 显示助攻榜
        private void OnAssistSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            adapter.mode = LeaguePlayerIntegralAdapter.ValueMode.Assists;
            var list = data.AssistsRank.Where(item => item.Assist > 0).OrderByDescending(item => item.Assist).ToList();
            emptyPanel.gameObject.SetActive(list.Count <= 0);
            adapter.SetData(list);
            valueTitle.text = Lang.Get(LangID.AssistsNumberText);
            //adapter.PlayFlash();
            adapter.InitAnim();
            adapter.PlayAnim();
        }

        // 显示抢断榜
        private void OnStealSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            adapter.mode = LeaguePlayerIntegralAdapter.ValueMode.Steal;
            var list = data.StealRank.Where(item => item.Steal > 0).OrderByDescending(item => item.Steal).ToList();
            emptyPanel.gameObject.SetActive(list.Count <= 0);
            adapter.SetData(list);
            valueTitle.text = Lang.Get(LangID.StealCountText);
            //adapter.PlayFlash();
            adapter.InitAnim();
            adapter.PlayAnim();
        }

        // 显示篮板榜
        private void OnReboundSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            adapter.mode = LeaguePlayerIntegralAdapter.ValueMode.Rebound;
            var list = data.ReboundRank.Where(item => item.Rebound > 0).OrderByDescending(item => item.Rebound).ToList();
            emptyPanel.gameObject.SetActive(list.Count <= 0);

            adapter.SetData(list);
            valueTitle.text = "篮板数";//Lang.Get(LangID.ZeroKeeperText);
                                    //adapter.PlayFlash();
            adapter.InitAnim();
            adapter.PlayAnim();
        }

        //显示盖帽榜
        private void OnBlockSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            adapter.mode = LeaguePlayerIntegralAdapter.ValueMode.Block;
            var list = data.BlockRank.Where(item => item.Block > 0).OrderByDescending(item => item.Block).ToList();
            emptyPanel.gameObject.SetActive(list.Count <= 0);
            adapter.SetData(list);
            valueTitle.text = "盖帽数";
            //adapter.PlayFlash();
            adapter.InitAnim();
            adapter.PlayAnim();
        }

    }
}