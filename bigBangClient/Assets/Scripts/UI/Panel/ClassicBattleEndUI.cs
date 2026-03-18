using System.Collections.Generic;
using System.Linq;
using Babu;
using Babu.Config;
using BigBang.Animation;
using BigBang.Battle;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using Spine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using static BigBang.BattleManager;
using static BigBang.ClassicManager;
using GameItem = Utils.GameItem.GameItem;
using Spine.Unity;
using Google.Protobuf.Collections;
using Babu.Client.Fsm;
using static BigBang.HeroManager;

namespace BigBang.UI
{

    public class ClassicBattleEndUI : APanelController
    {

        #region 显示

        #region 初始化
        protected override void AddListeners()
        {
            base.AddListeners();
            CloseBtn.OnClick += OnClose;
            DataBtn.OnClick += OnData;
            gotoButton1.onClick.AddListener(OnClickGotoButton1);
            gotoButton2.onClick.AddListener(OnClickGotoButton2);
            gotoButton3.onClick.AddListener(OnClickGotoButton3);
            gotoButton4.onClick.AddListener(OnClickGotoButton4);
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            CloseBtn.OnClick -= OnClose;
            DataBtn.OnClick -= OnData;
            gotoButton1.onClick.RemoveListener(OnClickGotoButton1);
            gotoButton2.onClick.RemoveListener(OnClickGotoButton2);
            gotoButton3.onClick.RemoveListener(OnClickGotoButton3);
            gotoButton4.onClick.RemoveListener(OnClickGotoButton4);
        }

        private RepeatedField<int> starRepeatedField = new() { 0, 0, 0 };
        private int[] star3Array = new int[2] { 1001, 1002 };
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            AudioManager.Instance.PlayMusic(AudioNames.BGM_TRAINING);

            if (Player.BattleManager.battleEnterType == BattleEnterType.ClassicUI)
            {
                starRepeatedField = Player.BattleManager.challengeStartResponse.Stars;
                star3Array = Player.BattleManager.classicTeamData.challengeClubConfig.Star3;
            }
            else if (Player.BattleManager.battleEnterType == BattleEnterType.HeroUI)
            {
                HeroClubData heroClubData = Player.BattleManager.heroClubData;
                starRepeatedField = Player.BattleManager.challengeStartHeroResponse.Stars;
                star3Array = heroClubData.challengeHeroConfig.Star3;
            }
            else if (Player.BattleManager.battleEnterType == BattleEnterType.FBTowerHomeUI)
            {
                starRepeatedField = Player.BattleManager.startTowerChallengeResponse.Stars;
                star3Array = Player.BattleManager.towerLevelData.towerConfig.Star3;
            }
            else
            {
                Debug.LogErrorFormat("ClassicBattleEndUI , OnPropertiesSet , Player.BattleManager.battleEnterType not found , Player.BattleManager.battleEnterType = {0}", Player.BattleManager.battleEnterType);
            }

            SetInfo();
            PrepareAni();
            DoUIAni();
        }

        [SerializeField] private GameObject WinPanel;
        [SerializeField] private GameObject LosePanel;
        [SerializeField] private SkeletonGraphic spineWin;
        [SerializeField] private SkeletonGraphic spineLose;
        private bool isWin = true;
        private bool isAwayWin = false;
        private void SetInfo()
        {
            isAwayWin = !Player.BattleManager.fightInfo.Result.Win;
            isWin = isAwayWin;
            WinPanel.SetActive(isWin);
            LosePanel.SetActive(!isWin);

            if (isWin)
            {
                SpriteManager.GetSprite(AtlasNames.Public, "btn1", (s) => { CloseBtn.GetComponent<Image>().sprite = s; });
                txtClose.color = Color.black;
                SetWin();
            }
            else
            {
                SpriteManager.GetSprite(AtlasNames.Public, "btn4", (s) => { CloseBtn.GetComponent<Image>().sprite = s; });
                txtClose.color = Color.white;
                SetLose();
            }
        }
        #endregion

        #region 公共

        [SerializeField] private TMP_Text AwayTeamNameText;
        [SerializeField] private ImageFont AwayScoreImageLabel;
        [SerializeField] private ClubIconItem AwayClubIconImage;
        [SerializeField] private TMP_Text HomeTeamNameText;
        [SerializeField] private ImageFont HomeScoreImageLabel;
        [SerializeField] private ClubIconItem HomeClubIconImage;
        private void SetScore()
        {
            AwayTeamNameText.text = Player.BattleManager.fightInfo.Teams.Away.TeamName;
            AwayScoreImageLabel.text = Player.BattleManager.fightInfoData.awayTeamStat.Point.ToString();
            AwayClubIconImage.SetIcon(Player.BattleManager.fightInfo.Teams.Away.TeamIcon);
            HomeTeamNameText.text = Player.BattleManager.fightInfo.Teams.Home.TeamName;
            HomeScoreImageLabel.text = Player.BattleManager.fightInfoData.homeTeamStat.Point.ToString();
            HomeClubIconImage.SetIcon(Player.BattleManager.fightInfo.Teams.Home.TeamIcon);
        }

        private void DoClose()
        {
            FsmManager.Instance.ChangeToState<StateHome>(new StateCommonUserData()
            {
                OpenUIAction = async () =>
                {
                    ClearAni();
                    switch (Player.BattleManager.battleEnterType)
                    {
                        case BattleEnterType.ClassicUI:
                            {
                                ClassicManager.Instance.ClearCountryCachedData();
                                ClassicManager.Instance.ClearWorldMapCachedLevelData();

                                ClassicManager.Instance.BackToClassicCountryFormBattle(Player.BattleManager.classicCountryLevelData.challengeCountryConfig.Id, () =>
                                {
                                    ClassicManager.Instance.NeedShowClassicCountryUI = true;
                                    UIController.Instance.HidePanel<ClassicBattleEndUI>();

                                    bool isNeedGuideBox3 = GuideManager.IsFinished(GuideID.guidePass13) && !GuideManager.IsFinished(GuideID.guideGetProgressBox3);
                                    //bool nowBack
                                    if (!isNeedGuideBox3)
                                    {
                                        //是否是当前国家的最后一支队伍
                                        bool isLastInCurrentCountry = Player.BattleManager.classicTeamData.passData.Stars[0] == 0;
                                        isLastInCurrentCountry &= isWin;
                                        isLastInCurrentCountry &= ClassicManager.Instance.teamByCountryDic[Player.BattleManager.classicTeamData.challengeClubConfig.Country][^1].Id == Player.BattleManager.classicTeamData.challengeClubConfig.Id;
                                        //调试飞机
                                        //isLastInCurrentCountry = true;
                                        if (isLastInCurrentCountry)
                                        {
                                            UIController.Instance.OpenWindow<ChallengeAreaCompleteUI>(new ChallengeAreaCompleteUIProperties(Player.BattleManager.classicTeamData, null));
                                        }
                                    }
                                });
                            }
                            break;
                        case BattleEnterType.HeroUI:
                            {
                                UIController.Instance.HidePanel<ClassicBattleEndUI>();
                            }
                            break;
                        case BattleEnterType.FBTowerHomeUI:
                            {
                                UIController.Instance.HidePanel<ClassicBattleEndUI>();
                            }
                            break;
                        default:
                            await UIController.Instance.ShowPanel<HomeUI>();
                            break;
                    }
                }
            });
        }

        [SerializeField] private BabuButton CloseBtn;
        [SerializeField] private TMP_Text txtClose;

        private void OnClose(BabuButton sender)
        {
            DoClose();
        }

        [SerializeField] private BabuButton DataBtn;
        private void OnData(BabuButton sender)
        {
            UIController.Instance.OpenWindow<Battle2EndDataUI>();
        }

        #endregion

        #region 胜利


        private void SetWin()
        {
            SetStar();
            SetScore();
            SetPlayer();
            SetWinMvp();
            SetScrollSize();
            SetTeamPlayer();
            SetReward();
        }
        [SerializeField] private List<GameObject> lightStarDescList = new();
        [SerializeField] private List<GameObject> darkStarDescList = new();
        [SerializeField] private List<TMP_Text> starDescTextList = new();
        private int starCount = 0;
        private void SetStar()
        {
            starCount = 0;
            for (int i = 0; i < 3; i++)
            {
                var isStar = starRepeatedField[i] > 0;

                if (isStar)
                {
                    starCount++;
                    lightStarDescList[i].SetActive(true);
                    darkStarDescList[i].SetActive(false);
                }
                else
                {
                    lightStarDescList[i].SetActive(false);
                    darkStarDescList[i].SetActive(true);
                }
                if (i > 0)
                {
                    starDescTextList[i].text = Configs.ChallengeRule.GetConfig(star3Array[i - 1]).Desc;
                }
            }
        }

        [SerializeField] private TMP_Text playerLevelText;
        [SerializeField] private Image playerProgressBarFgImage;
        private void SetPlayer()
        {
            Debug.Log("Player.BattleManager.classicPlayerLevel = " + Player.BattleManager.classicPlayerLevel.ToString());
            playerLevelText.text = "LV{0}".SafeFormat(Player.BattleManager.classicPlayerLevel);
            playerProgressBarFgImage.fillAmount = Player.GetExpProgress(Player.BattleManager.classicPlayerLevel, Player.BattleManager.classicPlayerExp);
        }
        [SerializeField] private TMP_Text winMvpNameText;
        [SerializeField] private TMP_Text winMvpScoreText;
        [SerializeField] private TMP_Text winMvpReboardText;
        [SerializeField] private TMP_Text winMvpAssitText;
        private void SetWinMvp()
        {
            bool isMvpInAway = Player.BattleManager.fightInfoData.fightCardDicAway.ContainsKey(Player.BattleManager.fightInfoData.mvpFightCard.PlayerCardId);
            if (isMvpInAway == false) return;
            winMvpNameText.text = Player.BattleManager.fightInfoData.mvpFightCard.Name;
            winMvpScoreText.text = "得分：{0}".SafeFormat(Player.BattleManager.fightInfoData.mvpPlayerStat.Point);
            winMvpAssitText.text = "助攻：{0}".SafeFormat(Player.BattleManager.fightInfoData.mvpPlayerStat.Assist);
            winMvpReboardText.text = "篮板：{0}".SafeFormat(Player.BattleManager.fightInfoData.mvpPlayerStat.Rebound);
        }
        [SerializeField] private ScrollRect winPlayerScrollRect;
        [SerializeField] private RectTransform winPlayerContent;
        [SerializeField] private RectTransform mvpPanel;
        [SerializeField] private RectTransform mvpInfoPanel;
        [SerializeField] private RectTransform mvpInfoPanelBgImage;
        [SerializeField] private RectTransform playerLayout;
        private void SetScrollSize()
        {
            int fightCardCount = Player.BattleManager.fightInfoData.fightCardDicAway.Count;
            bool isMvpInAway = Player.BattleManager.fightInfoData.fightCardDicAway.ContainsKey(Player.BattleManager.fightInfoData.mvpFightCard.PlayerCardId);
            mvpPanel.gameObject.SetActive(isMvpInAway);
            mvpInfoPanel.gameObject.SetActive(isMvpInAway);
            if (isMvpInAway)
            {
                playerLayout.SetLocalPositionX(82.19995f);
                winPlayerContent.SetSizeDeltaWidth(69f + 66f + 119f * fightCardCount);
                mvpInfoPanelBgImage.SetSizeDeltaWidth(78f - 19f + 118f * fightCardCount);
            }
            else
            {
                playerLayout.SetLocalPositionX(15f);
                winPlayerContent.SetSizeDeltaWidth(69f + 119f * fightCardCount);
                mvpInfoPanelBgImage.SetSizeDeltaWidth(78f - 19f - 66 + 118f * fightCardCount);
            }
            winPlayerScrollRect.horizontalNormalizedPosition = 0f;
        }

        [SerializeField] private List<ClassicBattleEndPlayerItem> playerItemList = new();
        List<Protocol.FightCard> fightCardList = new();
        private async void SetTeamPlayer()
        {
            #region 排序：mvp最前（如果有），首发替补依次在后（首发和替补内部按照分数助攻篮板排序）
            bool isMvpInAway = Player.BattleManager.fightInfoData.fightCardDicAway.ContainsKey(Player.BattleManager.fightInfoData.mvpFightCard.PlayerCardId);
            List<string> fightCardIdList = new();
            if (isMvpInAway) fightCardIdList.Add(Player.BattleManager.fightInfoData.mvpFightCard.PlayerCardId);

            List<PlayerStat> awayFirstPlayerStatList = new();
            foreach (var item in Player.BattleManager.fightInfoData.fightInfo.Teams.Away.CourtCard)
            {
                if (isMvpInAway && Player.BattleManager.fightInfoData.mvpFightCard.PlayerCardId == item.PlayerCardId) continue;
                awayFirstPlayerStatList.Add(Player.BattleManager.fightInfoData.playerStatDicAll[item.PlayerCardId]);
            }
            awayFirstPlayerStatList.OrderByDescending(item => item.Point).ThenByDescending(item => item.Assist).ThenByDescending(item => item.Rebound);
            foreach (var item in awayFirstPlayerStatList)
            {
                fightCardIdList.Add(item.PlayerCardId);
            }

            List<PlayerStat> awayBenchPlayerStatList = new();
            foreach (var item in Player.BattleManager.fightInfoData.fightInfo.Teams.Away.BenchCard)
            {
                if (isMvpInAway && Player.BattleManager.fightInfoData.mvpFightCard.PlayerCardId == item.PlayerCardId) continue;
                awayBenchPlayerStatList.Add(Player.BattleManager.fightInfoData.playerStatDicAll[item.PlayerCardId]);
            }
            awayBenchPlayerStatList.OrderByDescending(item => item.Point).ThenByDescending(item => item.Assist).ThenByDescending(item => item.Rebound);
            foreach (var item in awayBenchPlayerStatList)
            {
                fightCardIdList.Add(item.PlayerCardId);
            }

            fightCardList.Clear();
            foreach (var item in fightCardIdList)
            {
                fightCardList.Add(Player.BattleManager.fightInfoData.fightCardDicAway[item]);
            }
            #endregion

            for (int i = 0; i < 12; i++)
            {
                ClassicBattleEndPlayerItem playerItem = playerItemList[i];
                playerItem.gameObject.SetActive(i < fightCardList.Count);
                if (i >= fightCardList.Count) continue;
                Protocol.FightCard fightCardInfo = fightCardList[i];
                playerItem.positionText.text = Configs.SeparatedPosition.GetConfig(fightCardInfo.AdaptPosition[0]).Abbreviation;
                playerItem.playerImage.sprite = await SpriteProxy.GetPlayerPortrait(fightCardInfo.Portrait);
                playerItem.progressImage.fillAmount = PlayerCard.GetExpProgress(Player.BattleManager.classicTeamPlayerLevelDic[fightCardInfo.CardId], Player.BattleManager.classicTeamPlayerExpDic[fightCardInfo.CardId]);
            }

        }

        [SerializeField] private Transform content;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private HorizontalAdapter horizontalAdapter;
        private void SetReward()
        {
            //bool isPassedOnce = starRepeatedField[0] > 0;
            bool isPassedOnce = Player.BattleManager.isFirstPass;
            List<GameItem> Rewards = new();
            if (Player.BattleManager.battleEnterType == BattleEnterType.ClassicUI)
            {
                Rewards = GameItemUtils.CreateGameItems(Player.BattleManager.isFirstPass ? Player.BattleManager.classicTeamData.challengeClubConfig.FirstReward : Player.BattleManager.classicTeamData.challengeClubConfig.Reward).ToList();
                //Rewards = GameItemUtils.CreateGameItems(Player.BattleManager.isFirstPass ? Player.BattleManager.classicTeamData.challengeClubConfig.Reward : Player.BattleManager.classicTeamData.challengeClubConfig.FirstReward).ToList();
            }
            else if (Player.BattleManager.battleEnterType == BattleEnterType.HeroUI)
            {
                Rewards = GameItemUtils.CreateGameItems(Player.BattleManager.heroClubData.challengeHeroConfig.Reward).ToList();
            }
            else if (Player.BattleManager.battleEnterType == BattleEnterType.FBTowerHomeUI)
            {
                Rewards = GameItemUtils.CreateGameItems(FBTowerController.Instance.isFirstPass ? Player.BattleManager.towerLevelData.towerConfig.FirstReward : Player.BattleManager.towerLevelData.towerConfig.Reward).ToList();
            }
            else
            {
                Debug.LogErrorFormat("ClassicBattleEndUI , SetReward , Player.BattleManager.battleEnterType not found , Player.BattleManager.battleEnterType = {0}", Player.BattleManager.battleEnterType);
            }
            while (content.childCount < Rewards.Count) Instantiate(itemPrefab, content);
            for (int i = 0; i < content.childCount; i++)
            {
                if (i < Rewards.Count)
                {
                    var reward = Rewards[i];
                    var child = content.GetChild(i);
                    child.gameObject.SetActive(true);
                    child.GetComponent<InventoryItem>().SetData(reward);
                }
                else
                {
                    content.GetChild(i).gameObject.SetActive(false);
                }
            }
            horizontalAdapter.Calculate();
        }


        #endregion

        #region 失败

        [SerializeField] private GameObject AwayHeadBgImage;
        [SerializeField] private GameObject HomeHeadBgImage;
        [SerializeField] private Image HeadImage;

        [SerializeField] private TMP_Text MvpPlayerNameText;
        [SerializeField] private ClubIconItem MvpPlayerClubIconImage;
        [SerializeField] private TMP_Text MvpScoreNumText;
        [SerializeField] private TMP_Text MvpAssistNumText;
        [SerializeField] private TMP_Text MvpReboundNumText;

        [SerializeField] private Button gotoButton1;
        [SerializeField] private Button gotoButton2;
        [SerializeField] private Button gotoButton3;
        [SerializeField] private Button gotoButton4;
        private async void SetLose()
        {
            SetScore();
            AwayHeadBgImage.SetActive(isAwayWin);
            HomeHeadBgImage.SetActive(!isAwayWin);
            //MVP
            if (Player.BattleManager.fightInfoData.mvpFightCard != null)
            {
                bool isMvpHome = Player.BattleManager.fightInfoData.fightCardDicHome.ContainsKey(Player.BattleManager.fightInfoData.mvpFightCard.PlayerCardId);
                MvpPlayerNameText.text = Player.BattleManager.fightInfoData.mvpFightCard.Name;
                MvpPlayerClubIconImage.SetIcon(isMvpHome ? Player.BattleManager.fightInfo.Teams.Home.TeamIcon : Player.BattleManager.fightInfo.Teams.Away.TeamIcon);
                MvpScoreNumText.text = Player.BattleManager.fightInfoData.mvpPlayerStat.Point.ToString();
                MvpAssistNumText.text = Player.BattleManager.fightInfoData.mvpPlayerStat.Assist.ToString();
                MvpReboundNumText.text = Player.BattleManager.fightInfoData.mvpPlayerStat.Rebound.ToString();

                if (Player.BattleManager.fightInfoData.mvpFightCard.Portrait.ToString().Length > 6)
                {
                    HeadImage.sprite = await SpriteProxy.GetNpcPortrait(Player.BattleManager.fightInfoData.mvpFightCard.Portrait);
                }
                else
                {
                    HeadImage.sprite = await SpriteProxy.GetPlayerPortrait(Player.BattleManager.fightInfoData.mvpFightCard.Portrait);
                }
                // if (Player.BattleManager.fightInfoData.mvpFightCard.Portrait != 0)
                // {
                //     HeadImage.sprite = await SpriteProxy.GetNpcPortrait(Player.BattleManager.fightInfoData.mvpFightCard.Portrait);
                // }
                // else
                // {
                //     CardModelConfig cardModelConfig = Configs.CardModel.GetDataDictionary()[Player.BattleManager.fightInfoData.mvpFightCard.CardId];
                //     HeadImage.sprite = await SpriteProxy.GetPlayerPortrait(cardModelConfig.Portrait);
                // }
            }
            else
            {
                Debug.LogError("服务器数据中没有MVP！");
            }
        }

        private void OnClickGotoButton1()
        {
            if (TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Recruit, true))
                TriggerManager.Instance.JumpPanel(TriggerModuleType.Recruit);
        }
        private void OnClickGotoButton2()
        {
            if (TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Card, true))
                TriggerManager.Instance.JumpPanel(TriggerModuleType.Card);
        }
        private void OnClickGotoButton3()
        {
            if (TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.BigBang, true))
                TriggerManager.Instance.JumpPanel(TriggerModuleType.BigBang);
        }
        private void OnClickGotoButton4()
        {
            if (TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.BigBang, true))
                TriggerManager.Instance.JumpPanel(TriggerModuleType.Formation);
        }

        #endregion

        #endregion

        #region 动画

        [UnityEngine.Header("动画")]
        public List<GameObject> showListWin = new();
        public List<GameObject> showListLose = new();
        public RectTransform topPointTrans;
        public RectTransform midPointTrans;
        public RectTransform topPoint219Trans;

        private void PrepareAni()
        {
            List<GameObject> showList = isWin ? showListWin : showListLose;
            foreach (GameObject showGo in showList)
            {
                showGo.SetAlpha(0);
                showGo.SetActive(false);
            }
            this.gameObject.SetAlpha(0);
        }

        private readonly float bigTime = 0.2f;
        private readonly float smallTime = 0.5f;
        private readonly float bigScale = 1.3f;
        private Sequence uiSequence = null;
        private Sequence playerProgressBarSequence = null;
        private Sequence playerProgressTextSequence = null;
        private List<Sequence> teamProgressBarSequenceList = new();
        private void DoUIAni()
        {
            List<GameObject> showList = isWin ? showListWin : showListLose;

            if (isWin)
            {
                spineWin.Initialize(true);
                spineWin.AnimationState.SetAnimation(0, "play" + starCount.ToString(), false);
            }
            else
            {
                spineLose.Initialize(true);
                spineLose.AnimationState.SetAnimation(0, "play", false);
            }

            uiSequence = DOTween.Sequence();
            //uiSequence.AppendInterval(0.3f);

            uiSequence.AppendInterval(0.2f);
            uiSequence.Append(this.gameObject.DOFade(1f, 1f));
            uiSequence.AppendInterval(0.3f);
            foreach (GameObject showGo in showList)
            {
                uiSequence.AppendCallback(() => { showGo.SetActive(true); AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP); });
                uiSequence.Append(showGo.DOFade(1f, 0.16f));
            }

            if (isWin)
            {
                if (Player.BattleManager.classicPlayerLevel < Player.Level)
                {
                    playerProgressBarSequence = DOTween.Sequence();
                    playerProgressBarSequence.Append(playerProgressBarFgImage.DOFillAmount(1f, 1.0f).SetEase(Ease.Linear));
                    playerProgressBarSequence.AppendCallback(() => { playerProgressBarFgImage.fillAmount = 0; });

                    Sequence playerProgressTextSequence = DOTween.Sequence();
                    playerProgressTextSequence.Append(playerLevelText.transform.DOScale(bigScale, bigTime));
                    playerProgressTextSequence.AppendCallback(() => { playerLevelText.text = "LV{0}".SafeFormat(Player.Level); });
                    playerProgressTextSequence.Append(playerLevelText.transform.DOScale(1f, smallTime));
                    playerProgressBarSequence.Insert(1.0f, playerProgressTextSequence);

                    playerProgressBarSequence.Append(playerProgressBarFgImage.DOFillAmount(Player.ExpProgress, 0.5f));

                    uiSequence.Insert(2.0f, playerProgressBarSequence);
                }
                else
                {
                    playerProgressBarSequence = DOTween.Sequence();
                    playerProgressBarSequence.Append(playerProgressBarFgImage.DOFillAmount(Player.ExpProgress, 1.5f));
                    uiSequence.Insert(2.0f, playerProgressBarSequence);
                }

                for (int i = 0; i < fightCardList.Count; i++)
                {
                    ClassicBattleEndPlayerItem playerItem = playerItemList[i];
                    Protocol.FightCard fightCardInfo = fightCardList[i];
                    PlayerCard playerCard = Player.CardManager.GetCard(fightCardInfo.CardId);

                    Sequence teamProgressBarSequence;
                    if (Player.BattleManager.classicTeamPlayerLevelDic[fightCardInfo.CardId] < playerCard.Level)
                    {
                        teamProgressBarSequence = DOTween.Sequence();
                        teamProgressBarSequence.AddTo(this.gameObject);
                        teamProgressBarSequence.Append(playerItem.progressImage.DOFillAmount(1f, 1.0f));
                        teamProgressBarSequence.Append(playerItem.progressImage.DOFillAmount(playerCard.ExpProgress, 0.5f));
                        uiSequence.Insert(2.0f, teamProgressBarSequence);
                        teamProgressBarSequenceList.Add(teamProgressBarSequence);
                    }
                    else
                    {
                        teamProgressBarSequence = DOTween.Sequence();
                        teamProgressBarSequence.AddTo(this.gameObject);
                        teamProgressBarSequence.Append(playerItem.progressImage.DOFillAmount(playerCard.ExpProgress, 1.5f));
                        uiSequence.Insert(2.0f, teamProgressBarSequence);
                        teamProgressBarSequenceList.Add(teamProgressBarSequence);
                    }
                }
            }
        }
        public void ClearAni()
        {
            foreach (var item in teamProgressBarSequenceList)
            {
                item?.Kill();
            }
            teamProgressBarSequenceList.Clear();
            playerProgressTextSequence?.Kill();
            playerProgressTextSequence = null;
            playerProgressBarSequence?.Kill();
            playerProgressBarSequence = null;
            uiSequence?.Kill();
            uiSequence = null;
        }

        #endregion

    }
}