using System;
using System.Collections.Generic;
using BigBang.Animation;
using DG.Tweening;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Utils;
namespace BigBang.UI
{

    public class HundredTeamDetailLineData
    {
        public HundredTeamDetailCardData redHundredTeamDetailCardData = null;
        public HundredTeamDetailCardData blueHundredTeamDetailCardData = null;
        public int redScore = 0;
        public int blueScore = 0;

        public string fightId = "";
        public int stageIndex = 0;//第几小节
    }

    public class HundredTeamDetailLineItem : MonoBehaviour
    {
        [SerializeField] private BabuButton playButton = null;
        [SerializeField] private TMP_Text countNumText = null;
        [SerializeField] private RectTransform redWinPanel = null;
        [SerializeField] private TMP_Text redWinRedNumText = null;
        [SerializeField] private TMP_Text redWinBlueNumText = null;
        [SerializeField] private RectTransform blueWinPanel = null;
        [SerializeField] private TMP_Text blueWinRedNumText = null;
        [SerializeField] private TMP_Text blueWinBlueNumText = null;
        [SerializeField] private HundredTeamDetailCardItem redHundredTeamDetailCardItem = null;
        [SerializeField] private HundredTeamDetailCardItem blueHundredTeamDetailCardItem = null;

        public HundredTeamDetailLineData hundredTeamDetailLineData = null;
        public int hundredStageIndex = 0;
        public HundredProgress hundredProgress = 0;
        public async void SetData(HundredTeamDetailLineData hundredTeamDetailLineData, int hundredStageIndex, HundredProgress hundredProgress)
        {
            this.hundredStageIndex = hundredStageIndex;
            this.hundredProgress = hundredProgress;
            this.hundredTeamDetailLineData = hundredTeamDetailLineData;
            redHundredTeamDetailCardItem.SetData(hundredTeamDetailLineData.redHundredTeamDetailCardData);
            blueHundredTeamDetailCardItem.SetData(hundredTeamDetailLineData.blueHundredTeamDetailCardData);
            bool isRedWin = hundredTeamDetailLineData.redScore > hundredTeamDetailLineData.blueScore;
            redWinPanel.gameObject.SetActive(isRedWin);
            blueWinPanel.gameObject.SetActive(!isRedWin);
            if (isRedWin)
            {
                redWinRedNumText.text = hundredTeamDetailLineData.redScore.ToString();
                redWinBlueNumText.text = hundredTeamDetailLineData.blueScore.ToString();
            }
            else
            {
                blueWinRedNumText.text = hundredTeamDetailLineData.redScore.ToString();
                blueWinBlueNumText.text = hundredTeamDetailLineData.blueScore.ToString();
            }
        }

        private void OnEnable()
        {
            playButton.OnClick += OnClickPlayButton;
        }
        private void OnDisable()
        {
            playButton.OnClick -= OnClickPlayButton;
        }

        private void OnClickPlayButton(BabuButton button)
        {
            HundredManager.Instance.GetFight(hundredTeamDetailLineData.fightId, (FightInfo fightInfo) =>
            {
                UIController.Instance.CloseWindow<HundredTeamDetailUI>();
                UIController.Instance.CloseWindow<HundredGuessUI>();
                Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.HundredTeamDetailUI;
                Player.BattleManager.SetHundredFightInfo(FightType.Hundred, fightInfo, hundredStageIndex, hundredProgress);
                Player.BattleManager.StartPlayFight();
            });


        }
    }

}
