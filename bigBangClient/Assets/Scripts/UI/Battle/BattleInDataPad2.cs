using System;
using System.Collections.Generic;
using System.Linq;
using BigBang.Animation;
using DG.Tweening;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class BattleInDataPad2 : MonoBehaviour
    {
        #region 初始化

        [SerializeField] private Image BgImage1;
        [SerializeField] private List<BattleInDataItem2> BattleInDataItemList = new();
        [SerializeField] private Image EndImage;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TMP_Text BlueTeamNameSwitchLightText;
        [SerializeField] private TMP_Text BlueTeamNameSwitchDarkText;
        [SerializeField] private TMP_Text RedTeamNameSwitchLightText;
        [SerializeField] private TMP_Text RedTeamNameSwitchDarkText;
        [SerializeField] private List<ImageFont> stageScoreTextListBlue = new();
        [SerializeField] private List<ImageFont> stageScoreTextListRed = new();

        FightInfoData fightInfoData;//服务器数据
        List<PlayerStat> awayPlayerStatList = new();
        List<PlayerStat> homePlayerStatList = new();

        private bool isEnd = true;
        public void SetEndInfo(FightInfoData fightInfoData, bool isAutoDoAni = true)
        {
            this.fightInfoData = fightInfoData;
            ClearAni();
            InitUI();
            PrepareAni();
            ProcessEndPlayerData();
            RefreshNameUI();
            RefreshStageUI(fightInfoData.awayTeamStat, fightInfoData.homeTeamStat);
            isBlueLight = true;
            RefreshSwitchUI();
            RefreshPlayerUI();
            if (isAutoDoAni) DoUIAni(null);
        }
        private void ProcessEndPlayerData()
        {
            isEnd = true;
            List<PlayerStat> awayFirstPlayerStatList = new();
            foreach (var item in fightInfoData.fightInfo.Teams.Away.CourtCard)
            {
                awayFirstPlayerStatList.Add(fightInfoData.playerStatDicAll[item.PlayerCardId]);
            }
            awayFirstPlayerStatList = awayFirstPlayerStatList.OrderByDescending(item => item.Point).ThenByDescending(item => item.Assist).ThenByDescending(item => item.Rebound).ToList();

            List<PlayerStat> awayBenchPlayerStatList = new();
            foreach (var item in fightInfoData.fightInfo.Teams.Away.BenchCard)
            {
                awayBenchPlayerStatList.Add(fightInfoData.playerStatDicAll[item.PlayerCardId]);
            }
            awayBenchPlayerStatList = awayBenchPlayerStatList.OrderByDescending(item => item.Point).ThenByDescending(item => item.Assist).ThenByDescending(item => item.Rebound).ToList();

            List<PlayerStat> homeFirstPlayerStatList = new();
            foreach (var item in fightInfoData.fightInfo.Teams.Home.CourtCard)
            {
                homeFirstPlayerStatList.Add(fightInfoData.playerStatDicAll[item.PlayerCardId]);
            }
            homeFirstPlayerStatList = homeFirstPlayerStatList.OrderByDescending(item => item.Point).ThenByDescending(item => item.Assist).ThenByDescending(item => item.Rebound).ToList();

            List<PlayerStat> homeBenchPlayerStatList = new();
            foreach (var item in fightInfoData.fightInfo.Teams.Home.BenchCard)
            {
                homeBenchPlayerStatList.Add(fightInfoData.playerStatDicAll[item.PlayerCardId]);
            }
            homeBenchPlayerStatList = homeBenchPlayerStatList.OrderByDescending(item => item.Point).ThenByDescending(item => item.Assist).ThenByDescending(item => item.Rebound).ToList();

            awayPlayerStatList.Clear();
            awayPlayerStatList.AddRange(awayFirstPlayerStatList);
            awayPlayerStatList.AddRange(awayBenchPlayerStatList);

            homePlayerStatList.Clear();
            homePlayerStatList.AddRange(homeFirstPlayerStatList);
            homePlayerStatList.AddRange(homeBenchPlayerStatList);
        }

        public void OnPlayerDataChange()
        {
            RefreshPlayerUI();
        }
        public void OnTeamDataChange()
        {
            RefreshPlayerUI();
            RefreshStageUI(Player.BattleManager.battleTeamData.teamStatBlue, Player.BattleManager.battleTeamData.teamStatRed);
        }
        public void SetPlayingInfo(FightInfoData fightInfoData)
        {
            this.fightInfoData = fightInfoData;
            ClearAni();
            InitUI();
            ProcessPlayingPlayerData();
            RefreshNameUI();
            RefreshStageUI(Player.BattleManager.battleTeamData.teamStatBlue, Player.BattleManager.battleTeamData.teamStatRed);
            isBlueLight = true;
            RefreshSwitchUI();
            RefreshPlayerUI();
        }
        private void ProcessPlayingPlayerData()
        {
            isEnd = false;
            List<PlayerStat> awayFirstPlayerStatList = new();
            foreach (var item in fightInfoData.fightInfo.Teams.Away.CourtCard)
            {
                awayFirstPlayerStatList.Add(Player.BattleManager.battlePlayerData.GetPlayerStat(item.PlayerCardId));
            }
            //awayFirstPlayerStatList = awayFirstPlayerStatList.OrderByDescending(item => item.Point).ThenByDescending(item => item.Assist).ThenByDescending(item => item.Rebound).ToList();

            List<PlayerStat> awayBenchPlayerStatList = new();
            foreach (var item in fightInfoData.fightInfo.Teams.Away.BenchCard)
            {
                awayBenchPlayerStatList.Add(Player.BattleManager.battlePlayerData.GetPlayerStat(item.PlayerCardId));
            }
            //awayBenchPlayerStatList = awayBenchPlayerStatList.OrderByDescending(item => item.Point).ThenByDescending(item => item.Assist).ThenByDescending(item => item.Rebound).ToList();

            List<PlayerStat> homeFirstPlayerStatList = new();
            foreach (var item in fightInfoData.fightInfo.Teams.Home.CourtCard)
            {
                homeFirstPlayerStatList.Add(Player.BattleManager.battlePlayerData.GetPlayerStat(item.PlayerCardId));
            }
            //homeFirstPlayerStatList = homeFirstPlayerStatList.OrderByDescending(item => item.Point).ThenByDescending(item => item.Assist).ThenByDescending(item => item.Rebound).ToList();

            List<PlayerStat> homeBenchPlayerStatList = new();
            foreach (var item in fightInfoData.fightInfo.Teams.Home.BenchCard)
            {
                homeBenchPlayerStatList.Add(Player.BattleManager.battlePlayerData.GetPlayerStat(item.PlayerCardId));
            }
            //homeBenchPlayerStatList = homeBenchPlayerStatList.OrderByDescending(item => item.Point).ThenByDescending(item => item.Assist).ThenByDescending(item => item.Rebound).ToList();

            awayPlayerStatList.Clear();
            awayPlayerStatList.AddRange(awayFirstPlayerStatList);
            awayPlayerStatList.AddRange(awayBenchPlayerStatList);

            homePlayerStatList.Clear();
            homePlayerStatList.AddRange(homeFirstPlayerStatList);
            homePlayerStatList.AddRange(homeBenchPlayerStatList);
        }

        #endregion

        #region UI显示
        private void InitUI()
        {
            for (int i = 0; i < BattleInDataItemList.Count; i++)
            {
                BattleInDataItem2 battleInDataItem = BattleInDataItemList[i];
                bool isFirst = i < 5;
                bool isDark = i % 2 != 0;

                battleInDataItem.DarkImage.gameObject.SetActive(isDark);
                battleInDataItem.FirstImage.gameObject.SetActive(isFirst);
                for (int j = 0; j < battleInDataItem.LightImageList.Count; j++)
                {
                    bool isLight = j % 2 != 1;
                    battleInDataItem.LightImageList[j].SetActive(isLight);
                }
            }

            scrollRect.horizontalNormalizedPosition = 0;
        }

        public void RefreshNameUI()
        {
            BlueTeamNameSwitchLightText.text = fightInfoData.fightInfo.Teams.Away.TeamName;
            RedTeamNameSwitchLightText.text = fightInfoData.fightInfo.Teams.Home.TeamName;
            BlueTeamNameSwitchDarkText.text = fightInfoData.fightInfo.Teams.Away.TeamName;
            RedTeamNameSwitchDarkText.text = fightInfoData.fightInfo.Teams.Home.TeamName;
        }
        public void RefreshStageUI(TeamStat awayTeamStat, TeamStat homeTeamStat)
        {
            for (int i = 0; i < 5; i++)
            {
                if (i < awayTeamStat.PtsQtrs.Count)
                {
                    stageScoreTextListBlue[i].text = awayTeamStat.PtsQtrs[i].ToString();
                }
                else
                {
                    stageScoreTextListBlue[i].text = "0";
                }
                if (i < homeTeamStat.PtsQtrs.Count)
                {
                    stageScoreTextListRed[i].text = homeTeamStat.PtsQtrs[i].ToString();
                }
                else
                {
                    stageScoreTextListRed[i].text = "0";
                }
            }
        }
        public void RefreshPlayerUI()
        {
            List<PlayerStat> playerStatList = isBlueLight ? awayPlayerStatList : homePlayerStatList;
            for (int i = 0; i < BattleInDataItemList.Count; i++)
            {
                BattleInDataItem2 battleInDataItem = BattleInDataItemList[i];
                if (i < playerStatList.Count)
                {
                    PlayerStat playerStat = playerStatList[i];
                    Protocol.FightCard fightCard = fightInfoData.fightCardDicAll[playerStat.PlayerCardId];
                    battleInDataItem.SetData(playerStat, fightCard, !isEnd);
                }
                else
                {
                    battleInDataItem.SetData(null, null);
                }
            }
        }

        #endregion

        #region 切换球队
        [SerializeField] private Button BlueTeamButton;
        [SerializeField] private Button RedTeamButton;
        private bool isBlueLight = true;
        private void OnEnable()
        {
            BlueTeamButton.onClick.AddListener(OnClickBlueTeam);
            RedTeamButton.onClick.AddListener(OnClickRedTeam);
        }
        private void OnDisable()
        {
            BlueTeamButton.onClick.RemoveListener(OnClickBlueTeam);
            RedTeamButton.onClick.RemoveListener(OnClickRedTeam);
        }
        private void OnClickBlueTeam()
        {
            if (isBlueLight == true) return;
            isBlueLight = true;
            RefreshSwitchUI();
            RefreshPlayerUI();
        }
        private void OnClickRedTeam()
        {
            if (isBlueLight == false) return;
            isBlueLight = false;
            RefreshSwitchUI();
            RefreshPlayerUI();
        }
        [SerializeField] private GameObject BlueLightGameObject;
        [SerializeField] private GameObject BlueDarkGameObject;
        [SerializeField] private GameObject RedLightGameObject;
        [SerializeField] private GameObject RedDarkGameObject;
        private void RefreshSwitchUI()
        {
            BlueLightGameObject.SetActive(isBlueLight);
            BlueDarkGameObject.SetActive(!isBlueLight);
            RedLightGameObject.SetActive(!isBlueLight);
            RedDarkGameObject.SetActive(isBlueLight);
        }

        #endregion

        #region 动画

        public void PrepareAni()
        {
            for (int i = 0; i < BattleInDataItemList.Count; i++)
            {
                BattleInDataItem2 battleInDataItem = BattleInDataItemList[i];
                battleInDataItem.transform.localScale = new Vector3(1, 0, 1);
            }
            this.gameObject.SetAlpha(0);
        }

        Sequence uiSequence = null;
        public void DoUIAni(Action aniEndCallBack = null)
        {
            uiSequence = DOTween.Sequence();
            uiSequence.AppendInterval(0.3f);
            uiSequence.Append(this.gameObject.DOFade(1, 0.5f));
            for (int i = 0; i < BattleInDataItemList.Count; i++)
            {
                BattleInDataItem2 battleInDataItem = BattleInDataItemList[i];
                uiSequence.Append(battleInDataItem.transform.DOScaleY(1f, 0.1f));
                if (i < 8) uiSequence.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP); });
            }
            uiSequence.AppendCallback(() =>
            {
                aniEndCallBack?.Invoke();
            });
        }
        public void ClearAni()
        {
            uiSequence?.Kill();
            uiSequence = null;
            for (int i = 0; i < BattleInDataItemList.Count; i++)
            {
                BattleInDataItem2 battleInDataItem = BattleInDataItemList[i];
                battleInDataItem.ClearStageBigAni();
            }
        }

        #endregion

        #region 让pad适配不同的界面
        public enum BattleInDataPad2BgState
        {
            Normal,
            Dark,
            Light,
        }

        [SerializeField] private GameObject DarkBgImage;
        [SerializeField] private GameObject BgImageLight;
        public void SetBgState(BattleInDataPad2BgState battleInDataPad2BgState)
        {
            DarkBgImage.SetActive(false);
            BgImage1.gameObject.SetActive(false);
            BgImageLight.SetActive(false);
            EndImage.gameObject.SetActive(true);

            switch (battleInDataPad2BgState)
            {
                case BattleInDataPad2BgState.Normal: BgImage1.gameObject.SetActive(true); break;
                case BattleInDataPad2BgState.Dark: DarkBgImage.SetActive(true); break;
                case BattleInDataPad2BgState.Light: BgImageLight.SetActive(true); EndImage.gameObject.SetActive(false); break;
            }
        }
        #endregion

    }
}