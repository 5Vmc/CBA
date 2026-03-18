using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using Babu.SDK;
using BigBang.Battle;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using Utils.GameItem;
using static BigBang.ClassicManager;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang.UI
{
    public class HundredDataUI : APanelController
    {

        protected override void AddListeners()
        {
            base.AddListeners();

            closeBtn.OnClick += OnClickCloseBtn;
            foreach (var button in stageButtonList)
            {
                button.OnClick += OnClickStageButton;
            }
            foreach (var button in areaButtonList)
            {
                button.OnClick += OnClickAreaButton;
            }
            EventManager.Instance.Register(EventID.OnClickHundredDataUISeasonItem, OnClickHundredDataUISeasonItem);
            EventManager.Instance.Register(EventID.OnClickHundredDataUIFightItem, OnClickHundredDataUIFightItem);
            playButton.OnClick += OnClickPlayButton;
            homeButton.OnClick += OnClickHomeButton;
            awayButton.OnClick += OnClickAwayButton;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();

            closeBtn.OnClick -= OnClickCloseBtn;
            foreach (var button in stageButtonList)
            {
                button.OnClick -= OnClickStageButton;
            }
            foreach (var button in areaButtonList)
            {
                button.OnClick -= OnClickAreaButton;
            }
            EventManager.Instance.Unregister(EventID.OnClickHundredDataUISeasonItem, OnClickHundredDataUISeasonItem);
            EventManager.Instance.Unregister(EventID.OnClickHundredDataUIFightItem, OnClickHundredDataUIFightItem);
            playButton.OnClick -= OnClickPlayButton;
            homeButton.OnClick -= OnClickHomeButton;
            awayButton.OnClick -= OnClickAwayButton;
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            RefreshInfo();
        }

        [SerializeField] public BabuButton closeBtn = null;

        private void OnClickCloseBtn(BabuButton button)
        {
            UIController.Instance.HidePanel<HundredDataUI>();
        }

        [SerializeField] private HundredDataUISeasonButtonAdapter seasonButtonAdapter = null;
        private void RefreshInfo()
        {
            seasonButtonAdapter.SetData(HundredManager.Instance.nowCourse.SeasonTitles.ToList());
        }

        private int season = 0;//赛季 1-N
        private void OnClickHundredDataUISeasonItem(object[] args)
        {
            HundredDataUISeasonButton hundredDataUISeasonButton = args[0] as HundredDataUISeasonButton;
            season = hundredDataUISeasonButton.index + 1;
            seasonButtonAdapter.SetSelect(hundredDataUISeasonButton);
            CheckGetFightData();
        }

        [SerializeField] private List<BabuButton> stageButtonList = new();
        [SerializeField] private List<BabuButton> areaButtonList = new();

        private int stage = 0;//比赛阶段 1入围赛 2淘汰赛 3冠军赛
        private void OnClickStageButton(BabuButton button)
        {
            int index = stageButtonList.IndexOf(button);
            if (index == -1) return;
            stage = index + 1;
            foreach (var b in stageButtonList)
            {
                if (b == button)
                {
                    b.GetComponent<Image>().color = new Color(0, 1, 0, 1);
                }
                else
                {
                    b.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                }
            }
            CheckGetFightData();
        }

        private int area = 0;//分区 1-8
        private void OnClickAreaButton(BabuButton button)
        {
            int index = areaButtonList.IndexOf(button);
            if (index == -1) return;
            area = index + 1;
            foreach (var b in areaButtonList)
            {
                if (b == button)
                {
                    b.GetComponent<Image>().color = new Color(0, 1, 0, 1);
                }
                else
                {
                    b.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                }
            }
            CheckGetFightData();
        }

        [SerializeField] private HundredDataUIFightItemAdapter fightItemAdapter = null;

        private void CheckGetFightData()
        {
            if (season == 0 || stage == 0 || area == 0) return;
            NetworkManager.Instance.GetHundredHistoryCourse(season, stage, area, (GetHundredHistoryCourseResponse getHundredHistoryCourseResponse) =>
            {
                List<string> fightIdList = new();
                foreach (var item in getHundredHistoryCourseResponse.HistoryCourseList)
                {
                    fightIdList.Add(item.FightId);
                }
                fightItemAdapter.SetData(fightIdList);
            });
        }

        private string fightId = "";
        private void OnClickHundredDataUIFightItem(object[] args)
        {
            HundredDataUIFightItem hundredDataUIFightItem = args[0] as HundredDataUIFightItem;
            fightId = hundredDataUIFightItem.fightId;
            Debug.Log("fightId = " + fightId);
            fightItemAdapter.SetSelect(hundredDataUIFightItem);
        }

        [SerializeField] private BabuButton playButton = null;
        [SerializeField] private BabuButton homeButton = null;
        [SerializeField] private BabuButton awayButton = null;

        private void OnClickPlayButton(BabuButton button)
        {
            if (string.IsNullOrEmpty(fightId))
            {
                Tips.PopTips("请选择比赛");
                return;
            }
            HundredManager.Instance.GetFight(fightId, (FightInfo fightInfo) =>
            {
                UIController.Instance.OpenWindow<HundredTeamDetailUI>(new HundredTeamDetailUIProperties(null, (HundredProgress)(stage + 2), false, fightInfo));
            });
        }
        private void OnClickHomeButton(BabuButton button)
        {
            if (string.IsNullOrEmpty(fightId))
            {
                Tips.PopTips("请选择比赛");
                return;
            }
            UIController.Instance.OpenWindow<HundredSingleDetailUI>(new HundredSingleDetailUIProperties(fightId, false, CompitionID.Hundred, ""));
        }
        private void OnClickAwayButton(BabuButton button)
        {
            if (string.IsNullOrEmpty(fightId))
            {
                Tips.PopTips("请选择比赛");
                return;
            }
            UIController.Instance.OpenWindow<HundredSingleDetailUI>(new HundredSingleDetailUIProperties(fightId, true, CompitionID.Hundred, ""));
        }

    }
}
