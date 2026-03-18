using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.Animation;
using BigBang.UI;
using deVoid.UIFramework;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static BigBang.BattleManager;
using static BigBang.FightInfoData2.StageBallInfo;
using Vector2 = UnityEngine.Vector2;

namespace BigBang.Battle
{

    public class BattleUI2 : APanelController
    {
        #region 初始化

        private float _playTimeScale = 1.0f;
        private float playTimeScale
        {
            get { return _playTimeScale; }
            set
            {
                _playTimeScale = value;

                UseNewtimeScale();
            }
        }
        private float overtimeTimeScale
        {
            get { return 2.0f; }
        }
        private float teamFireTimeScale
        {
            get { return 2.0f; }
        }
        private void UseNewtimeScale()
        {
            if (stageSeq != null) stageSeq.timeScale = playTimeScale;
            foreach (Sequence countOneSeq in countOneSeqList)
            {
                if (countOneSeq != null) countOneSeq.timeScale = playTimeScale;
            }
            if (count321Seq != null) count321Seq.timeScale = playTimeScale;
            foreach (Sequence ballSeq in ballSeqList)
            {
                if (ballSeq != null) ballSeq.timeScale = playTimeScale;
            }
        }

        private Protocol.FightInfo fightRound;//服务器发来的战斗信息
                                              //fightRound.Result.Quarters.Count
                                              //fightRound.Result.Quarters[0].Possessions.ToList();



        protected override void AddListeners()
        {
            base.AddListeners();
            skipBtn.OnClick += OnSkip;
            teamDataBtn.OnClick += OnClickTeamDataBtn;
            playerDataBtn.OnClick += OnClickPlayerDataBtn;
            dataDarkBgButton.onClick.AddListener(OnClickDataDarkBgButton);
            RegDebugEvents();
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            skipBtn.OnClick -= OnSkip;
            teamDataBtn.OnClick -= OnClickTeamDataBtn;
            playerDataBtn.OnClick -= OnClickPlayerDataBtn;
            dataDarkBgButton.onClick.RemoveListener(OnClickDataDarkBgButton);

            UnRegDebugEvents();
        }

        private bool isHundred = false;//百分大战，要求 1V1
        [SerializeField] private GameObject QuickDebugPanel;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            //测试数据
            isHundred = Player.BattleManager.fightType == FightType.Hundred;

            //QuickDebugPanel.SetActive(Player.BattleManager.battleEnterType == BattleEnterType.Debug);
            AudioManager.Instance.PlayMusic(AudioNames.BATTLE_BG);

            switch (Player.BattleManager.battleEnterType)
            {
                case BattleEnterType.Guide:
                    skipBtn.gameObject.SetActive(false);
                    stageNumText.gameObject.SetActive(false);
                    break;
                default:
                    skipBtn.gameObject.SetActive(true);
                    stageNumText.gameObject.SetActive(true);
                    break;
            }

            Process3DRes();

            SetHundredUI();

            //初始化完成后隐藏球员
            InitOnce();
            Clear();
            //插入战斗前动画，MatchAgainstUI播放
            UIController.Instance.OpenWindow<MatchAgainstUI>(new MatchAgainstUIProperties(ResumeBattle));
        }

        private void SetHundredUI()
        {
            teamDataBtn.gameObject.SetActive(!isHundred);
            playerDataBtn.gameObject.SetActive(!isHundred);
            timeNumText.gameObject.SetActive(!isHundred);
            timeBgImage.gameObject.SetActive(!isHundred);
            stageNumText.gameObject.SetActive(!isHundred && Player.BattleManager.battleEnterType != BattleEnterType.Guide);

            leftBgImage.gameObject.SetActive(!isHundred);
            rightBgImage.gameObject.SetActive(!isHundred);
            leftHundredBgImage.gameObject.SetActive(isHundred);
            rightHundredBgImage.gameObject.SetActive(isHundred);

            BlueDataHidePanel.gameObject.SetActive(!isHundred);
            RedDataHidePanel.gameObject.SetActive(!isHundred);

            hundredProgressFight1Image.gameObject.SetActive(isHundred && Player.BattleManager.hundredProgress == HundredProgress.Fight1);
            hundredProgressFight2Image.gameObject.SetActive(isHundred && Player.BattleManager.hundredProgress == HundredProgress.Fight2);
            hundredProgressFight3Image.gameObject.SetActive(isHundred && Player.BattleManager.hundredProgress == HundredProgress.Fight3);

            battleCardItemListHome[2].PositionText.gameObject.SetActive(!isHundred);
            battleCardItemListAway[2].PositionText.gameObject.SetActive(!isHundred);

            stage = Player.BattleManager.hundredStageIndex;
        }

        public GameObject background;
        //战斗前动画回调
        public void ResumeBattle()
        {
            battleUI2Anim.SetUIActive(false);
            background.gameObject.SetAlpha(0f);
            Sequence seq = DOTween.Sequence();
            seq.Append(background.DOFade(1f, 1f));
            //seq.Insert(0f, lanqiujiaTrans.DOScale(1.5f, 0.5f).From());
            //seq.Insert(0f, lanqiujiaTrans.DOMove(new Vector3(0f, -4.4f, -2.24f), 0.5f).From());
            //seq.Insert(0f, background.transform.DOScale(15f, 0.5f).From());
            seq.AppendCallback(() =>
            {
                sceneTrans.gameObject.SetActive(true);
                RestartBattle();
            });
        }

        private void RestartBattle()
        {
            Clear();

            SetHundredUI();

            if (Player.BattleManager.fightInfo == null)
            {
                Debug.LogError("fightInfo is null");
                return;
            }

            SetTopUIInfo();
            SetInfo();
            SetBattleInData();
            PlayShowCardAni(() =>
            {
                PlayCount321Ani(StartFight);
            });
            battleUI2Anim.PlayEnter();
            PlaySkipShowAni();
        }
        private bool isClose = false;
        private void CheckShowDataAndClose()
        {
            if (isClose) return;
            isClose = true;
            switch (Player.BattleManager.battleEnterType)
            {
                case BattleEnterType.ClassicUI:
                case BattleEnterType.HeroUI:
                case BattleEnterType.FBTowerHomeUI:
                case BattleEnterType.ChallengeUI:
                case BattleEnterType.ArenaUI:
                    OnClose();
                    break;
                case BattleEnterType.HundredTeamDetailUI:
                    ClearShowCardAni();
                    ClearPlayAni();
                    ClearCount321Ani();
                    Protocol.FightCard leftFightCard = Player.BattleManager.fightInfo.Teams.Away.CourtCard[Player.BattleManager.hundredStageIndex];
                    Protocol.FightCard rightFightCard = Player.BattleManager.fightInfo.Teams.Home.CourtCard[Player.BattleManager.hundredStageIndex];
                    int leftScore = Player.BattleManager.fightInfoData.playerStatDicAll[leftFightCard.PlayerCardId].Point;
                    int rightScore = Player.BattleManager.fightInfoData.playerStatDicAll[rightFightCard.PlayerCardId].Point;
                    BlueScoreWheel.SetScore(leftScore, false);
                    RedScoreWheel.SetScore(rightScore, false);
                    UIController.Instance.OpenWindow<HundredBattleEndUI>(new HundredBattleEndUIProperties(leftFightCard, rightFightCard, leftScore, rightScore, OnClose));
                    break;
                default:
                    ShowDataAndClose();
                    break;
            }
        }
        [SerializeField] private Image timeBgImage = null;
        private void ShowDataAndClose()
        {
            isClose = true;
            battleInDataPad2.gameObject.SetActive(false);
            battleInDataPad1.gameObject.SetActive(false);
            OnClickPlayerDataBtn(null);
            teamDataBtn.interactable = false;
            playerDataBtn.interactable = false;
            BlueScoreWheel.SetScore(Player.BattleManager.fightInfoData.awayTeamStat.Point, false);
            RedScoreWheel.SetScore(Player.BattleManager.fightInfoData.homeTeamStat.Point, false);
            timeBgImage.gameObject.SetActive(false);
            timeNumText.gameObject.SetActive(false);
            stageNumText.gameObject.SetActive(false);
        }
        private void OnClose()
        {
            isClose = true;
            //UIController.Instance.HidePanel<BattleUI2>();
            switch (Player.BattleManager.battleEnterType)
            {
                case BattleEnterType.ClassicUI:
                case BattleEnterType.HeroUI:
                case BattleEnterType.FBTowerHomeUI:
                    UIController.Instance.HidePanel<BattleUI2>(true);
                    UIController.Instance.ShowPanel<ClassicBattleEndUI>();
                    break;
                case BattleEnterType.ChallengeUI:
                    UIUtils.CloseAllPanels();
                    UIController.Instance.ShowPanel<Battle2EndUI>();
                    break;
                case BattleEnterType.CupUI_Course:
                case BattleEnterType.CupUI_Integral:
                case BattleEnterType.LeagueUI:
                case BattleEnterType.MyGameUI:
                case BattleEnterType.MyGameUI_MyCoursePad:
                case BattleEnterType.MyGameUI_MyLastGamePad:
                case BattleEnterType.HundredTeamDetailUI:
                case BattleEnterType.LeagueUI_LeagueCoursePad:
                    UIController.Instance.HidePanel<BattleUI2>();
                    AudioManager.Instance.PlayMusic(AudioNames.BGM_HOME);
                    break;
                case BattleEnterType.ArenaUI:
                    UIUtils.CloseAllPanels();
                    UIController.Instance.ShowPanel<ArenaEndRewardUI>();
                    break;
                default:
                    UIController.Instance.ShowPanel<HomeUI>();
                    break;
            }
            Player.DispatchLevelUp();
            PlayerAchievementManager.CheckSaveId();
            Player.InBattleAni = false;
            GuideManager.UpdatePopwindowFlag();
            if (UIController.Instance.PopwindowFlag == true) UIController.Instance.OpenAllHideScreens();
        }

        private void Clear()
        {
            timeBgImage.gameObject.SetActive(true);
            timeNumText.gameObject.SetActive(true);
            stageNumText.gameObject.SetActive(true);
            isClose = false;
            stage = 0;
            ClearShowCardAni();
            ClearPlayAni();
            ClearCount321Ani();
            ClearBallAni();
            ClearShakeHoopAni();
            ClearHighlightCardAni();
            ClearFlipCardAni();
            ClearScoreAni();
            ClearChangePlayerAni();
            ClearOverTimePopAni();
            ClearPlayCheerAni();
            ClearGetScoreEffect();
            ClearChangePlayerSmallCard();
            ResetAllCardPosition();
            ClearStageBigAni();
            ClearAllDataPad();
            ClearFireOnCardAni();
            ClearSkipShowAni();
            BlueScoreWheel.SetScore(0, false);
            RedScoreWheel.SetScore(0, false);
        }
        private bool isInitOnce = false;
        private void InitOnce()
        {
            if (isInitOnce == true) return;
            isInitOnce = true;

            InitOnceBattleInDataPad();
        }
        private void SetInfo()
        {
            SetStageInfo(stage);
            SetPlayStartInfo();
        }

        #endregion

        #region 顶部信息
        [SerializeField] private ClubIconItem blueClubIconImage;
        [SerializeField] private TMP_Text blueClubNameText;
        [SerializeField] private TMP_Text blueDefNameText;
        [SerializeField] private TMP_Text blueAtkNameText;

        [SerializeField] private ClubIconItem redClubIconImage;
        [SerializeField] private TMP_Text redClubNameText;
        [SerializeField] private TMP_Text redDefNameText;
        [SerializeField] private TMP_Text redAtkNameText;

        [SerializeField] private TMP_Text stageNumText;//第几小节文本
        [SerializeField] private TMP_Text timeNumText;//小节时间文本

        [SerializeField] private BattleScoreWheel BlueScoreWheel;//分数
        [SerializeField] private BattleScoreWheel RedScoreWheel;//分数

        [SerializeField] private Image hundredProgressFight1Image = null;
        [SerializeField] private Image hundredProgressFight2Image = null;
        [SerializeField] private Image hundredProgressFight3Image = null;

        private void SetTopUIInfo()
        {
            blueClubIconImage.SetIcon(Player.BattleManager.fightInfoData.fightInfo.Teams.Away.TeamIcon);
            blueClubNameText.text = Player.BattleManager.fightInfoData.fightInfo.Teams.Away.TeamName;
            if (!isHundred)
            {
                string textATKBlue = "";
                string textDEFBlue = "";
                DataConvUtil.TacticsIdList2AtkDef(Player.BattleManager.fightInfoData.fightInfo.Teams.Away.TacticsIdList.ToList(), ref textATKBlue, ref textDEFBlue);
                blueAtkNameText.text = textATKBlue;
                blueDefNameText.text = textDEFBlue;
            }

            redClubIconImage.SetIcon(Player.BattleManager.fightInfo.Teams.Home.TeamIcon);
            redClubNameText.text = Player.BattleManager.fightInfoData.fightInfo.Teams.Home.TeamName;
            if (!isHundred)
            {
                string textATKRed = "";
                string textDEFRed = "";
                DataConvUtil.TacticsIdList2AtkDef(Player.BattleManager.fightInfoData.fightInfo.Teams.Home.TacticsIdList.ToList(), ref textATKRed, ref textDEFRed);
                redAtkNameText.text = textATKRed;
                redDefNameText.text = textDEFRed;
            }

            BlueScoreWheel.SetScore(0, false);
            RedScoreWheel.SetScore(0, false);

            timeNumText.text = "12:00";

            BlueScoreWheel.ChangeSpeedToVeryFast();
            RedScoreWheel.ChangeSpeedToVeryFast();
        }
        private void SetStageInfo(int stageNum)
        {
            if (stageNum < normalStageMax)
            {
                stageNumText.text = "第{0}节".SafeFormat(stageNum + 1);
            }
            else
            {
                stageNumText.text = "加时赛";
            }
        }
        #endregion

        #region 战斗逻辑

        //private void Update()
        //{

        //}

        private int stage = 0;
        private readonly int normalStageMax = 4;//有几个正常回合（之后就是加时赛）
        //开始战斗逻辑
        private void StartFight()
        {
            PlayStage(stage, OnOneStagePlayEnd);
        }
        private void OnOneStagePlayEnd(int stageNum)
        {
            if (Player.BattleManager.battleEnterType == BattleEnterType.Guide)
            {
                UnityTimer.Timer.Register(this.gameObject, 2.0f, () =>
                {
                    UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties("基础不错，经验不足", "回到俱乐部", OnClose));
                });
                return;
            }

            if (stageNum < Player.BattleManager.fightInfoData.maxStage - 1)
            {
                stage++;
                SetStageInfo(stage);
                PlayStage(stage, OnOneStagePlayEnd);
            }
            else
            {
                OnAllStagePlayEnd();
            }
        }
        private void OnAllStagePlayEnd()
        {
            CheckShowDataAndClose();
        }

        Sequence stageSeq = null;
        private void PlayStage(int stageNum, Action<int> stageEndCallBack)
        {
            timeNumText.text = GetFrameLeftTimeStr(0);
            FightInfoData2.StageBallInfo stageBallInfo = Player.BattleManager.fightInfoData2.stageBallInfoList[stageNum];
            stageSeq = DOTween.Sequence();
            stageSeq.timeScale = playTimeScale;
            if (stageBallInfo.hasTeamFire)
            {
                if (stageNum > 0)
                {
                    stageSeq.AppendInterval(1f);
                }
                stageSeq.AppendCallback(() =>
                {
                    stageFireItem.PlayTeamFireAni(stage + 1, stageBallInfo.fightQuaterInfo.HomeFireBuff, stageBallInfo.fightQuaterInfo.AwayFireBuff, Player.BattleManager.fightInfoData.fightInfo.Teams.Home.TeamIcon, Player.BattleManager.fightInfoData.fightInfo.Teams.Away.TeamIcon, null);
                });
                stageSeq.AppendInterval(3f);
                stageSeq.AppendCallback(() =>
                {
                    stageSeq.timeScale = teamFireTimeScale;
                });
            }
            else
            {
                if (stageNum > 0 && stageNum < normalStageMax)
                {
                    PlayStageBigAni();
                }
            }
            if (stageNum >= normalStageMax && !isHundred)//加时赛
            {
                stageSeq.AppendInterval(1f);
                if (stage >= normalStageMax) PlayOvertimePopAni();
                stageSeq.AppendInterval(1f);
                stageSeq.AppendCallback(() =>
                {
                    AudioManager.Instance.PlaySound(AudioNames.BATTLE_START_WHISTLE);
                });
                stageSeq.AppendInterval(1f);
                stageSeq.AppendCallback(() =>
                {
                    stageSeq.timeScale = overtimeTimeScale;
                });
            }

            int roundIndex = 0;
            foreach (int ballRoundId in stageBallInfo.ballRoundIdList)
            {
                bool isRoundPass = true;
                List<FightPossessionInfo> fightPossessionInfoList = stageBallInfo.ballRoundDic[ballRoundId];
                bool hasScore = false;
                for (int i = 0; i < fightPossessionInfoList.Count; i++)
                {
                    FightPossessionInfo fightPossessionInfo = fightPossessionInfoList[i];
                    int score = FightInfoData2.GetAddScoreNum(fightPossessionInfo.EventId);
                    if (FightInfoData2.IsSub(fightPossessionInfo.EventId) == true)//换人
                    {
                        isRoundPass = false;
                        PlayChangePlayerSmallCardAni(stageSeq, fightPossessionInfo);
                        if (i != fightPossessionInfoList.Count - 1)
                        {
                            stageSeq.AppendInterval(0.1f);
                        }
                    }

                    if (score > 0)//有进球
                    {
                        isRoundPass = false;
                        hasScore = true;
                        if (FightInfoData2.IsAssist(fightPossessionInfo.EventId))
                        {
                            stageSeq.AppendCallback(() =>
                            {
                                BattleCardItem battleCardItem1 = battleCardItemDic[fightPossessionInfo.PlayerCardId];
                                BattleCardItem battleCardItem2 = battleCardItemDic[fightPossessionInfo.Player2CardId];
                                PlayBallAssistAni(battleCardItem1.isRed, battleCardItem2.index, battleCardItem1.index, score, fightPossessionInfo.ShotType, fightPossessionInfo, stageNum);
                            });
                        }
                        else
                        {
                            stageSeq.AppendCallback(() =>
                            {
                                BattleCardItem battleCardItem1 = battleCardItemDic[fightPossessionInfo.PlayerCardId];
                                PlayBallEnterAni(battleCardItem1.isRed, battleCardItem1.index, score, fightPossessionInfo.ShotType, fightPossessionInfo, stageNum);
                            });
                        }
                        if (i != fightPossessionInfoList.Count - 1)
                        {
                            stageSeq.AppendInterval(0.1f);
                        }
                    }
                    else
                    {
                        stageSeq.AppendCallback(() =>
                        {
                            AddRoundData(fightPossessionInfo, stageNum);
                        });
                    }
                }

                stageSeq.AppendCallback(() =>
                {
                    //开始冒火
                    List<GiftBuffData> GiftBuffDataListStart = Player.BattleManager.fightInfoData2.GetGiftBuffDataStartList(ballRoundId);
                    if (GiftBuffDataListStart.Count > 0)
                    {
                        foreach (GiftBuffData giftBuffData in GiftBuffDataListStart)
                        {
                            if (battleCardItemDic.ContainsKey(giftBuffData.giftBuffInfo.FromPlayerCardId) == false)
                            {
                                // Debug.LogFormat("BattleUI2 , PlayStage , GiftBuffDataListStart , battleCardItemDic.ContainsKey(giftBuffData.giftBuffInfo.FromPlayerCardId) == false , giftBuffData.giftBuffInfo.FromPlayerCardId = {0}", giftBuffData.giftBuffInfo.FromPlayerCardId);
                                //当前设计为使用技能的人身上冒火，而不是被使用技能的人身上冒火，所以此log出现是正常的，因为技能生效时，使用技能的人可能已经不在场上了
                                continue;
                            }
                            BattleCardItem battleCardItem1 = battleCardItemDic[giftBuffData.giftBuffInfo.FromPlayerCardId];
                            battleCardItem1.PlayFireOnCardAni();
                        }
                    }
                    //结束冒火
                    List<GiftBuffData> GiftBuffDataListEnd = Player.BattleManager.fightInfoData2.GetGiftBuffDataEndList(ballRoundId);
                    if (GiftBuffDataListEnd.Count > 0)
                    {
                        foreach (GiftBuffData giftBuffData in GiftBuffDataListEnd)
                        {
                            if (battleCardItemDic.ContainsKey(giftBuffData.giftBuffInfo.FromPlayerCardId) == false)
                            {
                                //Debug.LogWarningFormat("BattleUI2 , PlayStage , GiftBuffDataListEnd , battleCardItemDic.ContainsKey(giftBuffData.giftBuffInfo.FromPlayerCardId) == false , giftBuffData.giftBuffInfo.FromPlayerCardId = {0}", giftBuffData.giftBuffInfo.FromPlayerCardId);
                                continue;
                            }
                            BattleCardItem battleCardItem1 = battleCardItemDic[giftBuffData.giftBuffInfo.FromPlayerCardId];
                            battleCardItem1.ClearFireOnCardAni();
                        }
                    }
                });

                if (hasScore == true)
                {
                    stageSeq.AppendInterval(0.333f);
                }
                if (isRoundPass == true)
                {
                    stageSeq.AppendInterval(0.1f);
                }
                roundIndex++;
            }
            if (stageNum == Player.BattleManager.fightInfo.Result.Quarters.Count - 1)
            {
                stageSeq.AppendInterval(0.6f);//传球时间
                stageSeq.AppendInterval(0.6f);//进球时间
            }
            float stopTimeMoveTime = 0;
            if (stageBallInfo.hasTeamFire)
            {
                if (stageNum > 0)
                {
                    stopTimeMoveTime = 4;
                }
                else
                {
                    stopTimeMoveTime = 3;
                }
            }
            else if (stageNum >= normalStageMax)
            {
                stopTimeMoveTime = 3;
            }
            else
            {
                stopTimeMoveTime = 0;
            }
            stageSeq.Insert(stopTimeMoveTime, DOTimeMove(timeNumText, stageSeq.Duration() - stopTimeMoveTime));
            if (stageNum == Player.BattleManager.fightInfo.Result.Quarters.Count - 1)
            {
                stageSeq.AppendInterval(1.0f);//等待结束时间
            }
            if (isHundred) stageSeq.AppendInterval(2.0f);//等待结束时间
            stageSeq.AppendCallback(() =>
            {
                stageEndCallBack.Invoke(stageNum);
            });
        }
        public static TweenerCore<float, float, FloatOptions> DOTimeMove(TMP_Text text, float duration)
        {
            TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(
                () => 0f,
                (progress) =>
                {
                    text.text = GetFrameLeftTimeStr(progress);
                }
                , 1f, duration).SetEase(Ease.Linear);
            tweenerCore.SetTarget(text.transform);
            return tweenerCore;
        }

        public static string GetFrameLeftTimeStr(float t)//获取某一帧还有多久结束
        {
            int nowSec = Mathf.FloorToInt(Mathf.Lerp(12 * 60, 0, t));
            return TimeUtils.FormatLeftTime(nowSec);
        }


        private void ClearPlayAni()
        {
            stageSeq?.Kill();
        }

        #endregion

        #region 设置卡片信息

        [SerializeField] public Sprite normalStar;
        [SerializeField] public Sprite colorfulStar;
        [SerializeField] public List<Sprite> bgImageList = new();
        [SerializeField] public List<Sprite> bgNoStarImageList = new();
        [SerializeField] public List<Sprite> ballImageList = new();
        [SerializeField] public List<Color> textColorList = new();

        private List<BattleCardItem> battleCardItemListAway = new();
        private List<BattleCardItem> battleCardItemListHome = new();
        private Dictionary<string, BattleCardItem> battleCardItemDic = new();
        private void SetPlayStartInfo()
        {
            battleCardItemDic.Clear();
            if (isHundred)
            {
                for (int i = 0; i < 5; i++)
                {
                    battleCardItemListAway[i].gameObject.SetActive(i == 2);
                    if (i == 2)
                    {
                        Protocol.FightCard fightCard = Player.BattleManager.fightInfo.Teams.Away.CourtCard[Player.BattleManager.hundredStageIndex];
                        battleCardItemListAway[i].SetData(fightCard, i, Player.BattleManager.fightInfoData.fightCardDicHome.ContainsKey(fightCard.PlayerCardId), Player.BattleManager.fightInfoData.CourtCardSetAll.Contains(fightCard.PlayerCardId), true);
                        battleCardItemDic.Add(fightCard.PlayerCardId, battleCardItemListAway[i]);
                        continue;
                    }
                }
                for (int i = 0; i < 5; i++)
                {
                    battleCardItemListHome[i].gameObject.SetActive(i == 2);
                    if (i == 2)
                    {
                        Protocol.FightCard fightCard = Player.BattleManager.fightInfo.Teams.Home.CourtCard[Player.BattleManager.hundredStageIndex];
                        battleCardItemListHome[i].SetData(fightCard, i, Player.BattleManager.fightInfoData.fightCardDicHome.ContainsKey(fightCard.PlayerCardId), Player.BattleManager.fightInfoData.CourtCardSetAll.Contains(fightCard.PlayerCardId), true);
                        battleCardItemDic.Add(fightCard.PlayerCardId, battleCardItemListHome[i]);
                        continue;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    battleCardItemListAway[i].gameObject.SetActive(true);
                    Protocol.FightCard fightCard = Player.BattleManager.fightInfo.Teams.Away.CourtCard[i];
                    battleCardItemListAway[i].SetData(fightCard, i, Player.BattleManager.fightInfoData.fightCardDicHome.ContainsKey(fightCard.PlayerCardId), Player.BattleManager.fightInfoData.CourtCardSetAll.Contains(fightCard.PlayerCardId), true);
                    battleCardItemDic.Add(fightCard.PlayerCardId, battleCardItemListAway[i]);
                }
                for (int i = 0; i < 5; i++)
                {
                    battleCardItemListHome[i].gameObject.SetActive(true);
                    Protocol.FightCard fightCard = Player.BattleManager.fightInfo.Teams.Home.CourtCard[i];
                    battleCardItemListHome[i].SetData(fightCard, i, Player.BattleManager.fightInfoData.fightCardDicHome.ContainsKey(fightCard.PlayerCardId), Player.BattleManager.fightInfoData.CourtCardSetAll.Contains(fightCard.PlayerCardId), true);
                    battleCardItemDic.Add(fightCard.PlayerCardId, battleCardItemListHome[i]);
                }
            }
        }

        #endregion

        #region 3D

        [SerializeField] private GameObject battle2Asset;
        private GameObject battle2GameObject;
        private Transform sceneTrans;
        private Transform battle2Trans;
        private Transform lanqiujiaTrans;
        [SerializeField] private RawImage battle2Img;
        private Camera battle2Camera;

        private Transform circleMidPointTrans;
        private List<Transform> leftCardPointTransList = new();
        private List<Transform> rightCardPointTransList = new();
        private Animator basketAnimator;
        private Transform basketFlatTrans;
        //private Transform basketMoveTrans;
        private SpriteRenderer bgSpriteRenderer;

        private Transform leftBgImage = null;
        private Transform rightBgImage = null;

        private Transform leftHundredBgImage = null;
        private Transform rightHundredBgImage = null;

        private void Process3DRes()
        {
            battle2GameObject = GameObject.Instantiate(battle2Asset);
            battle2Trans = battle2GameObject.transform;
            sceneTrans = battle2Trans.Find("scene").transform;
            leftBgImage = sceneTrans.Find("LeftBgImage").transform;
            rightBgImage = sceneTrans.Find("RightBgImage").transform;
            leftHundredBgImage = sceneTrans.Find("LeftHundredBgImage").transform;
            rightHundredBgImage = sceneTrans.Find("RightHundredBgImage").transform;
            battle2Camera = battle2Trans.Find("Main Camera").GetComponent<Camera>();
            CameraManager.Instance.SetTexture(CameraID.Battle2, battle2Img);
            circleMidPointTrans = sceneTrans.Find("CircleMidPoint");
            lanqiujiaTrans = battle2Trans.Find("zhandou_lanqiujia");
            basketAnimator = lanqiujiaTrans.GetComponent<Animator>();
            basketFlatTrans = lanqiujiaTrans.Find("Dummy009");
            //basketMoveTrans = battle2Trans.Find("qiukuangce").Find("qiukuangMove");
            bgSpriteRenderer = battle2Trans.Find("Battle2Bg").GetComponent<SpriteRenderer>();
            leftCardPointTransList.Clear();
            rightCardPointTransList.Clear();
            battleCardItemListAway.Clear();
            battleCardItemListHome.Clear();
            //battle2GameObject.SetActive(false);
            for (int i = 1; i <= 5; i++)
            {
                leftCardPointTransList.Add(sceneTrans.Find("LeftCard" + i));
                rightCardPointTransList.Add(sceneTrans.Find("RightCard" + i));
                BattleCardItem leftCard = sceneTrans.Find("Zhandou_ka_A" + i).GetComponent<BattleCardItem>();
                leftCard.SetBattleUI2(this);
                battleCardItemListAway.Add(leftCard);
                BattleCardItem rightCard = sceneTrans.Find("Zhandou_ka_B" + i).GetComponent<BattleCardItem>();
                rightCard.SetBattleUI2(this);
                battleCardItemListHome.Add(rightCard);
            }
            InitBallPoolOnce();
            float cameraOrthographicSize = Utility.Lerp(5.8f, 7.5f, UIFrame.GetFixScreenLerpT());
            battle2Camera.orthographicSize = cameraOrthographicSize;
            isCanHide = true;
            sceneTrans.gameObject.SetActive(false);
        }

        private bool isCanHide = false;
        protected override void WhileHiding()
        {
            if (isCanHide == false) return;
            Clear();
            DeatoeyBallPoolOnce();
            CameraManager.Instance.ReleaseTexture(CameraID.Battle2, battle2Img);
            GameObject.Destroy(battle2GameObject);
            battleCardItemDic.Clear();
            isCanHide = false;
        }

        #endregion

        #region 动画

        #region 卡牌出现动画

        private List<Sequence> showCardSeqList = new();
        private void ClearShowCardAni()
        {
            foreach (Sequence showCardSeq in showCardSeqList)
            {
                showCardSeq?.Kill();
            }
            showCardSeqList.Clear();
        }
        private void PlayShowCardAni(Action playEndCallBack = null)
        {
            ClearShowCardAni();

            for (int i = 0; i < 5; i++)
            {
                showCardSeqList.Add(PlayShowCardOneAni(false, i));
                showCardSeqList.Add(PlayShowCardOneAni(true, i));
            }

            float showCardTime = showCardSeqList[^1].Duration();
            Sequence showCardwaitSeq = DOTween.Sequence();
            changePlayerSeqList.Add(showCardwaitSeq);
            showCardwaitSeq.target = this.transform;
            showCardwaitSeq.timeScale = playTimeScale;
            showCardwaitSeq.AppendInterval(showCardTime);
            showCardwaitSeq.AppendCallback(() =>
            {
                playEndCallBack?.Invoke();
            });

        }
        private Sequence PlayShowCardOneAni(bool isRed, int index)
        {
            BattleCardItem battleCardItem = (isRed ? battleCardItemListHome : battleCardItemListAway)[index];
            Vector3 normalPosition = (isRed ? rightCardPointTransList : leftCardPointTransList)[index].transform.position;
            float normalX = normalPosition.x;
            float farX = isRed ? normalX + 2.5f : normalX - 2.5f;
            Vector3 farPosition = normalPosition;
            farPosition.x = farX;

            battleCardItem.transform.localScale = Vector3.one * 1.2f;
            battleCardItem.transform.localRotation = Quaternion.identity;
            battleCardItem.transform.position = farPosition;

            Sequence showCardOneSeq = DOTween.Sequence();
            changePlayerSeqList.Add(showCardOneSeq);
            showCardOneSeq.target = battleCardItem.transform;
            showCardOneSeq.timeScale = playTimeScale;
            showCardOneSeq.AppendInterval(0.1f * index);
            if (isHundred)
            {
                if (index == 2) showCardOneSeq.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP); });
            }
            else
            {
                showCardOneSeq.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP); });
            }
            showCardOneSeq.Append(battleCardItem.transform.DOMoveX(normalX, 0.7f).SetEase(Ease.OutBack));
            showCardOneSeq.Join(battleCardItem.transform.DOScale(1, 0.7f).SetEase(Ease.OutSine));

            return showCardOneSeq;
        }

        #endregion

        #region 球框晃动

        private void ClearShakeHoopAni()
        {
            shakeHoopSeq?.Kill();
            shakeHoopSeq = null;
            //basketMoveTrans.localPosition = Vector3.zero;
        }

        private Sequence shakeHoopSeq;
        private float shakeHoopDistanceY = -0.03f;
        private float shakeHoopDownTime = 0.05f;
        private float shakeHoopUpTime = 0.1f;
        private void PlayShakeHoopAni()
        {
            if (shakeHoopSeq != null) return;

            shakeHoopSeq = DOTween.Sequence();
            shakeHoopSeq.timeScale = playTimeScale;

            //shakeHoopSeq.AppendInterval(0.05f);

            //球网晃动
            shakeHoopSeq.AppendCallback(() =>
            {
                basketAnimator.SetTrigger("GoalTrigger");
            });

            ////球篮下沉
            //shakeHoopSeq.Append(basketMoveTrans.DOLocalMoveY(shakeHoopDistanceY, shakeHoopDownTime));

            ////球篮上浮
            //shakeHoopSeq.Append(basketMoveTrans.DOLocalMoveY(0, shakeHoopUpTime));

            //清除动画进行标记
            shakeHoopSeq.AppendCallback(() =>
            {
                shakeHoopSeq = null;
            });
        }

        #endregion

        #region 球框破碎

        [SerializeField] private GameObject basketBreakPrefab;
        private void PlayBasketBreakAni()
        {
            GameObject basketBreakGo = GameObject.Instantiate(basketBreakPrefab, basketFlatTrans);
            //basketBreakGo.transform.SetParent(basketFlatTrans);
            basketBreakGo.SetLayerInThisAndAllChild(Layers.Battle2);
            GameObject.Destroy(basketBreakGo, 3f);
        }

        #endregion

        #region 卡牌变亮

        private List<Sequence> highlightCardSeqList = new();
        private float highlightCardLightTime = 0.3f;
        private float highlightCardDarkTime = 0.2f;
        private void ClearHighlightCardAni()
        {
            foreach (Sequence highlightCardSeq in highlightCardSeqList)
            {
                highlightCardSeq?.Kill();
            }
            highlightCardSeqList.Clear();

            foreach (BattleCardItem battleCardItem in battleCardItemListAway)
            {
                battleCardItem.CardHighLightImage.SetHighlight(0f);
            }
            foreach (BattleCardItem battleCardItem in battleCardItemListHome)
            {
                battleCardItem.CardHighLightImage.SetHighlight(0f);
            }
        }
        private void PlayHighlightCardAni(bool isRed, int index)
        {
            List<BattleCardItem> battleCardItemList = isRed ? battleCardItemListHome : battleCardItemListAway;
            BattleCardItem battleCardItem = battleCardItemList[index];

            battleCardItem.CardHighLightImage.DOKill();

            Sequence highlightCardSeq = DOTween.Sequence();
            highlightCardSeq.target = battleCardItem.CardHighLightImage;
            highlightCardSeq.timeScale = playTimeScale;
            highlightCardSeqList.Add(highlightCardSeq);

            highlightCardSeq.Append(battleCardItem.CardHighLightImage.DOHighlight(1, highlightCardLightTime).SetEase(Ease.Linear));
            highlightCardSeq.Append(battleCardItem.CardHighLightImage.DOHighlight(0, highlightCardDarkTime).SetEase(Ease.Linear));
        }

        #endregion

        #region 卡牌翻转

        private List<Sequence> flipCardSeqList = new();
        private float flipCardHideTime = 0.3f;
        private float flipCardShowTime = 0.2f;
        private void ClearFlipCardAni()
        {
            foreach (Sequence flipCardSeq in flipCardSeqList)
            {
                flipCardSeq?.Kill();
            }
            flipCardSeqList.Clear();

            foreach (BattleCardItem battleCardItem in battleCardItemListAway)
            {
                battleCardItem.transform.localScale = Vector3.one;
            }
            foreach (BattleCardItem battleCardItem in battleCardItemListHome)
            {
                battleCardItem.transform.localScale = Vector3.one;
            }
        }
        private void PlayFlipCardAni(bool isRed, int index, Action changeCallBack)
        {
            List<BattleCardItem> battleCardItemList = isRed ? battleCardItemListHome : battleCardItemListAway;
            BattleCardItem battleCardItem = battleCardItemList[index];

            Sequence flipCardSeq = DOTween.Sequence();
            flipCardSeq.target = battleCardItem.transform;
            flipCardSeq.timeScale = playTimeScale;
            flipCardSeqList.Add(flipCardSeq);

            flipCardSeq.Append(battleCardItem.transform.DOScaleX(0, flipCardHideTime));
            flipCardSeq.AppendCallback(() => { changeCallBack?.Invoke(); });
            flipCardSeq.Append(battleCardItem.transform.DOScaleX(1, flipCardShowTime));
        }

        #endregion

        #region 换人动画

        private List<Sequence> changePlayerSeqList = new();
        private void ClearChangePlayerAni()
        {
            foreach (Sequence changePlayerSeq in changePlayerSeqList)
            {
                changePlayerSeq?.Kill();
            }
            changePlayerSeqList.Clear();

            foreach (BattleCardItem changePlayerCardDel in changePlayerCardDelSet)
            {
                changePlayerCardDel.transform?.DOKill();
                GameObject.Destroy(changePlayerCardDel.gameObject);
            }
            changePlayerCardDelSet.Clear();
        }
        private void ResetAllCardPosition()
        {
            for (int i = 0; i < 5; i++)
            {
                battleCardItemListHome[i].transform.position = rightCardPointTransList[i].position;
                battleCardItemListAway[i].transform.position = leftCardPointTransList[i].position;
                battleCardItemListHome[i].MoveParent.localPosition = Vector3.zero;
                battleCardItemListAway[i].MoveParent.localPosition = Vector3.zero;
                battleCardItemListHome[i].transform.localScale = Vector3.one;
                battleCardItemListAway[i].transform.localScale = Vector3.one;
            }
        }

        private HashSet<BattleCardItem> changePlayerCardDelSet = new();
        private void PlayChangePlayerAni(bool isRed, int index)
        {
            BattleCardItem battleCardItemOld = null;
            BattleCardItem battleCardItemNew = null;

            Vector3 normalPosition = (isRed ? rightCardPointTransList : leftCardPointTransList)[index].transform.position;
            float normalX = normalPosition.x;
            float farX = isRed ? normalX + 2.5f : normalX - 2.5f;
            Vector3 farPosition = normalPosition;
            farPosition.x = farX;

            //旧卡牌放到后面
            List<BattleCardItem> battleCardItemList = isRed ? battleCardItemListHome : battleCardItemListAway;
            battleCardItemOld = battleCardItemList[index];
            Vector3 battleCardItemOldPos = battleCardItemOld.transform.position;
            battleCardItemOldPos.z += 0.2f;
            battleCardItemOld.transform.position = battleCardItemOldPos;

            //创建新卡牌
            battleCardItemNew = GameObject.Instantiate(battleCardItemOld.gameObject).GetComponent<BattleCardItem>();
            battleCardItemNew.MoveParent.localPosition = Vector3.zero;
            battleCardItemNew.gameObject.name = isRed ? "LeftCard" : "RightCard" + (index + 1);
            battleCardItemNew.transform.SetParent(battleCardItemOld.transform.parent);
            battleCardItemNew.transform.localScale = Vector3.one * 1.2f;
            battleCardItemNew.transform.localRotation = Quaternion.identity;
            battleCardItemNew.transform.position = farPosition;
            battleCardItemNew.SetData(battleCardItemOld.fightCard, battleCardItemOld.index, battleCardItemOld.isRed, Player.BattleManager.fightInfoData.CourtCardSetAll.Contains(battleCardItemOld.fightCard.PlayerCardId));
            battleCardItemList[index] = battleCardItemNew;
            battleCardItemDic[battleCardItemNew.fightCard.PlayerCardId] = battleCardItemNew;
            changePlayerCardDelSet.Add(battleCardItemOld);

            //新卡牌出现
            Sequence changePlayerNewSeq = DOTween.Sequence();
            changePlayerSeqList.Add(changePlayerNewSeq);
            changePlayerNewSeq.target = battleCardItemNew.transform;
            changePlayerNewSeq.timeScale = playTimeScale;
            changePlayerNewSeq.AppendInterval(0.1f);
            changePlayerNewSeq.Append(battleCardItemNew.transform.DOMoveX(normalX, 0.7f).SetEase(Ease.OutBack));
            changePlayerNewSeq.Join(battleCardItemNew.transform.DOScale(1, 0.7f).SetEase(Ease.OutSine));

            //旧卡牌移出
            Sequence changePlayerOldSeq = DOTween.Sequence();
            changePlayerSeqList.Add(changePlayerOldSeq);
            changePlayerOldSeq.target = battleCardItemOld.transform;
            changePlayerOldSeq.timeScale = playTimeScale;
            changePlayerOldSeq.AppendInterval(0.0f);
            changePlayerOldSeq.Append(battleCardItemOld.transform.DOMoveX(farX, 0.3f).SetEase(Ease.InSine));
            changePlayerOldSeq.Join(battleCardItemOld.transform.DOScale(0.7f, 0.3f).SetEase(Ease.OutQuad));
            changePlayerOldSeq.AppendCallback(() =>
            {
                battleCardItemOld.transform.DOKill();
                GameObject.Destroy(battleCardItemOld.gameObject, 3f);
                changePlayerCardDelSet.Remove(battleCardItemOld);
            });
        }

        #endregion

        #region 换人动画(小卡形式)(嵌入主循环)

        private List<string> battleCardIdListAway = new();
        private List<string> battleCardIdListHome = new();
        private HashSet<BattleCardItem> changePlayerCardSmallCardDelSet = new();
        private void ClearChangePlayerSmallCard()
        {
            battleCardIdListAway.Clear();
            battleCardIdListHome.Clear();
            if (Player.BattleManager.fightInfo != null && Player.BattleManager.fightInfo.Teams != null && Player.BattleManager.fightInfo.Teams.Away != null && Player.BattleManager.fightInfo.Teams.Away.CourtCard != null)
            {
                foreach (var item in Player.BattleManager.fightInfo.Teams.Away.CourtCard)
                {
                    if (item == null) continue;
                    battleCardIdListAway.Add(item.PlayerCardId);
                }
            }
            if (Player.BattleManager.fightInfo != null && Player.BattleManager.fightInfo.Teams != null && Player.BattleManager.fightInfo.Teams.Home != null && Player.BattleManager.fightInfo.Teams.Home.CourtCard != null)
            {
                foreach (var item in Player.BattleManager.fightInfo.Teams.Home.CourtCard)
                {
                    if (item == null) continue;
                    battleCardIdListHome.Add(item.PlayerCardId);
                }
            }
            foreach (BattleCardItem changePlayerCardSmallCardDel in changePlayerCardSmallCardDelSet)
            {
                if (changePlayerCardSmallCardDel == null) continue;
                changePlayerCardSmallCardDel?.transform?.DOKill();
                GameObject.Destroy(changePlayerCardSmallCardDel?.gameObject);
            }
            changePlayerCardSmallCardDelSet.Clear();
        }
        private float smallCardOffsetX = 0.612f;
        private float smallCardOffsetY = 0.428f;
        private float smallCardScale = 0.3f;
        private void PlayChangePlayerSmallCardAni(Sequence mainSeq, FightPossessionInfo fightPossessionInfo)
        {
            string newPlayerId = fightPossessionInfo.PlayerCardId;
            string oldPlayerId = fightPossessionInfo.Player2CardId;

            bool isRed = false;
            int index = 0;
            for (int i = 0; i < battleCardIdListHome.Count; i++)
            {
                string battleCardId = battleCardIdListHome[i];
                if (oldPlayerId == battleCardId)
                {
                    isRed = true;
                    index = i;
                    break;
                }
            }
            if (isRed == false)
            {
                for (int i = 0; i < battleCardIdListAway.Count; i++)
                {
                    string battleCardId = battleCardIdListAway[i];
                    if (oldPlayerId == battleCardId)
                    {
                        index = i;
                        break;
                    }
                }
            }

            BattleCardItem battleCardItemNew = null;
            float seqTime = mainSeq.Duration();
            float seqTimeBefore = seqTime - 1f;
            if (seqTimeBefore < 0) seqTimeBefore = 0;
            Sequence creatCardSeq = DOTween.Sequence();
            changePlayerSeqList.Add(creatCardSeq);
            creatCardSeq.timeScale = playTimeScale;
            creatCardSeq.AppendCallback(() =>
            {
                battleCardItemNew = GameObject.Instantiate((isRed ? battleCardItemListHome : battleCardItemListAway)[index].gameObject).GetComponent<BattleCardItem>();
                battleCardItemNew.gameObject.name = (isRed ? "LeftCard" : "RightCard") + (index + 1);
                battleCardItemNew.transform.SetParent((isRed ? battleCardItemListHome : battleCardItemListAway)[index].transform.parent);
                battleCardItemNew.transform.localScale = Vector3.one * smallCardScale;
                battleCardItemNew.transform.localRotation = Quaternion.identity;
                battleCardItemNew.MoveParent.localPosition = Vector3.zero;
                Vector3 normalPosition = (isRed ? rightCardPointTransList : leftCardPointTransList)[index].transform.position;
                normalPosition.x += isRed ? -smallCardOffsetX : smallCardOffsetX;
                normalPosition.y += -smallCardOffsetY;
                battleCardItemNew.transform.position = normalPosition;
                Protocol.FightCard fightCard = Player.BattleManager.fightInfoData.fightCardDicAll[newPlayerId];
                battleCardItemNew.SetData(fightCard, index, isRed, Player.BattleManager.fightInfoData.CourtCardSetAll.Contains(fightCard.PlayerCardId));
                changePlayerCardSmallCardDelSet.Add(battleCardItemNew);
            });
            mainSeq.Insert(seqTimeBefore, creatCardSeq);

            mainSeq.AppendCallback(() =>
            {
                BattleCardItem battleCardItemOld = battleCardItemDic[oldPlayerId];
                battleCardItemDic.Remove(oldPlayerId);
                battleCardItemDic.Add(newPlayerId, battleCardItemNew);
                (isRed ? battleCardItemListHome : battleCardItemListAway)[index] = battleCardItemNew;
                changePlayerCardSmallCardDelSet.Remove(battleCardItemNew);
                changePlayerCardSmallCardDelSet.Add(battleCardItemOld);

                Vector3 normalPosition = (isRed ? rightCardPointTransList : leftCardPointTransList)[index].transform.position;
                float normalX = normalPosition.x;
                float farX = isRed ? normalX + 2.5f : normalX - 2.5f;
                Vector3 farPosition = normalPosition;
                farPosition.x = farX;

                Sequence changeCardOutSeq = DOTween.Sequence();
                changePlayerSeqList.Add(changeCardOutSeq);
                changeCardOutSeq.timeScale = playTimeScale;
                changeCardOutSeq.target = battleCardItemOld.transform;
                changeCardOutSeq.AppendInterval(0.0f);
                changeCardOutSeq.Append(battleCardItemOld.transform.DOMoveX(farX, 0.3f).SetEase(Ease.InSine));
                changeCardOutSeq.Join(battleCardItemOld.transform.DOScale(0.7f, 0.3f).SetEase(Ease.OutQuad));
                changeCardOutSeq.AppendCallback(() =>
                {
                    battleCardItemOld.transform.DOKill();
                    battleCardItemOld.ClearFireOnCardAni();
                    GameObject.Destroy(battleCardItemOld.gameObject, 3f);
                    changePlayerCardSmallCardDelSet.Remove(battleCardItemOld);
                });

                Sequence changeCardInSeq = DOTween.Sequence();
                changePlayerSeqList.Add(changeCardInSeq);
                changeCardInSeq.timeScale = playTimeScale;
                changeCardInSeq.target = battleCardItemNew.transform;
                changeCardInSeq.AppendInterval(0.1f);
                changeCardInSeq.Append(battleCardItemNew.transform.DOMove(normalPosition, 0.3f).SetEase(Ease.OutBack));
                changeCardInSeq.Join(battleCardItemNew.transform.DOScale(1, 0.3f).SetEase(Ease.OutSine));

            });

            (isRed ? battleCardIdListHome : battleCardIdListAway)[index] = newPlayerId;
        }

        #endregion

        #region 投球

        [SerializeField] private GameObject ballPrefab;
        private ComponentPool<Battle2Ball> BattleBallPool = new();
        private List<Sequence> ballSeqList = new();
        private void InitBallPoolOnce()
        {
            BattleBallPool.InitComponentPool(ballPrefab, 6, sceneTrans, InitBall);
        }
        private void DeatoeyBallPoolOnce()
        {
            BattleBallPool.DestoryAll();
        }
        private void InitBall(Battle2Ball battleBall)
        {
            //battleBall.transform.localScale = Vector3.one * 0.6f;
        }
        private void PlayBallEnterAni(bool isRed, int index, int score, int shotType, FightPossessionInfo fightPossessionInfo = null, int stage = 0)//直接投篮
        {
            //List<BattleCardItem> battleCardItemList = isRed ? battleCardItemListHome : battleCardItemListAway;
            //BattleCardItem battleCardItem = battleCardItemList[index];
            Battle2Ball battleBall = BattleBallPool.GetComponentFormPool();
            battleBall.SetTrail(isRed);

            List<Transform> cardPointTransList = isRed ? rightCardPointTransList : leftCardPointTransList;
            Vector3 startPos = cardPointTransList[index].position;
            startPos.z = circleMidPointTrans.position.z;
            battleBall.transform.position = startPos;
            battleBall.transform.localScale = Vector3.one;
            battleBall.ballTrans.localRotation = Quaternion.Euler(Utility.GetRandomFloat(0, 360), Utility.GetRandomFloat(0, 360), Utility.GetRandomFloat(0, 360));
            battleBall.ballRotTrans.localRotation = Quaternion.Euler(0, isRed ? 30 : -30, 0);
            battleBall.ClearTrail();

            Sequence seq = DOTween.Sequence();
            seq.timeScale = playTimeScale;

            //球员卡片高亮，球渐隐出现
            PlayHighlightCardAni(isRed, index);
            seq.AppendInterval(highlightCardLightTime);

            //投篮动画
            BallGotoBasket(seq, battleBall.transform.position, battleBall, isRed, score, index, shotType, fightPossessionInfo, stage);

            ballSeqList.Add(seq);
        }
        private void PlayBallAssistAni(bool isRed, int indexForm, int indexTo, int score, int shotType, FightPossessionInfo fightPossessionInfo = null, int stage = 0)//助攻投篮
        {
            //List<BattleCardItem> battleCardItemList = isRed ? battleCardItemListHome : battleCardItemListAway;
            //BattleCardItem battleCardItemForm = battleCardItemList[indexForm];
            //BattleCardItem battleCardItemTo = battleCardItemList[indexTo];
            Battle2Ball battleBall = BattleBallPool.GetComponentFormPool();
            battleBall.SetTrail(isRed);
            List<Transform> cardPointTransList = isRed ? rightCardPointTransList : leftCardPointTransList;
            Vector3 startPos = cardPointTransList[indexForm].position;
            startPos.z = circleMidPointTrans.position.z;
            battleBall.transform.position = startPos;
            battleBall.transform.localScale = Vector3.one;
            battleBall.ballTrans.localRotation = Quaternion.Euler(Utility.GetRandomFloat(0, 360), Utility.GetRandomFloat(0, 360), Utility.GetRandomFloat(0, 360));
            battleBall.ballRotTrans.localRotation = Quaternion.Euler(0, isRed ? 30 : -30, 0);
            battleBall.ClearTrail();

            Sequence seq = DOTween.Sequence();
            seq.timeScale = playTimeScale;

            //球员卡片高亮，球渐隐出现
            PlayHighlightCardAni(isRed, indexForm);
            seq.AppendInterval(highlightCardLightTime);

            //A传球给B
            Vector3 passStartPos = battleBall.transform.position;
            Vector3 passEndPos = cardPointTransList[indexTo].position;
            if (Mathf.Abs(indexForm - indexTo) == 1)
            {
                float passDuration = 0.3f;
                seq.Append(battleBall.transform.DOMove(passEndPos, passDuration).SetEase(Ease.Linear));
            }
            else
            {
                Vector3 passControlPos = (passStartPos + passEndPos) / 2;
                if (passControlPos.x < 0) passControlPos.x -= 0.7f; else passControlPos.x += 0.7f;
                float passDuration = Mathf.Lerp(0.3f, 0.6f, Mathf.Abs(indexForm - indexTo) / (float)4);
                seq.Append(battleBall.transform.DOBezier2Move(passStartPos, passControlPos, passEndPos, passDuration).SetEase(Ease.Linear));
            }

            ////得到球后停顿
            //seq.AppendInterval(0.12f);

            //投篮动画
            BallGotoBasket(seq, passEndPos, battleBall, isRed, score, indexTo, shotType, fightPossessionInfo, stage);

            ballSeqList.Add(seq);
        }
        private void BallGotoBasket(Sequence seq, Vector3 bezierStartPos, Battle2Ball battleBall, bool isRed, int score, int index, int shotType, FightPossessionInfo fightPossessionInfo = null, int stage = 0)//投篮动画
        {
            //卡牌上下移动模拟投篮
            float moveDownDuration = 0.1f;
            float moveUpDuration = 0.05f;
            seq.Append((isRed ? battleCardItemListHome : battleCardItemListAway)[index].MoveParent.DOLocalMoveY(-0.2f, moveDownDuration));

            //B把球扔到球篮上方
            Vector3 bezierEndPos = circleMidPointTrans.transform.position + new Vector3(0, 0.7f, 0);
            Vector3 bezierControlPos = (bezierStartPos + bezierEndPos) / 2 + new Vector3(0, 2.0f, 0);
            float bezierDuration = 0.6f;
            seq.Append(battleBall.transform.DOBezier2Move(bezierStartPos, bezierControlPos, bezierEndPos, bezierDuration).SetEase(Ease.Linear));
            seq.Join(battleBall.transform.DOScale(0.9f, bezierDuration).SetEase(Ease.Linear));
            seq.Join(battleBall.ballRotTrans.DORotate(new Vector3(0, isRed ? 30 : -30, isRed ? -270 : 270), bezierDuration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
            seq.Join((isRed ? battleCardItemListHome : battleCardItemListAway)[index].MoveParent.DOLocalMoveY(0, moveUpDuration));

            //加分
            seq.AppendCallback(() =>
            {
                if (shotType == (int)ShotType.SHOT_DUNK)
                {
                    PlayBasketBreakAni();
                    PlayCheerAni();
                }
                BattleScoreWheel battleScoreWheel = isRed ? RedScoreWheel : BlueScoreWheel;
                if (isClose == false)
                {
                    battleScoreWheel.AddScore(score, true, 0.05f);
                }
                PlayShakeHoopAni();
                OnShowDropScore(score, isRed);
                if (score >= 3) PlayGetScoreEffect();
                if (shotType == (int)ShotType.SHOT_DUNK)
                {
                    AudioManager.Instance.PlaySound(AudioNames.BATTLE_SLAM);
                }
                else
                {
                    AudioManager.Instance.PlaySound(score >= 3 ? AudioNames.BATTLE_GOAL2 : AudioNames.BATTLE_GOAL);
                }
                //UnityTimer.Timer.Register(this.gameObject, 0.1f, () =>
                //{
                //    AudioManager.Instance.PlaySound(AudioNames.BATTLE_CHEERING);//欢呼声音过密
                //});
                AddRoundData(fightPossessionInfo, stage);
            });

            //球进篮筐
            float fadeOutDuration = 0.1f;
            float moveOutEndPosY = circleMidPointTrans.transform.position.y - 0.3f;
            seq.Append(battleBall.transform.DOMoveY(moveOutEndPosY, fadeOutDuration));
            seq.AppendCallback(() =>
            {
                BattleBallPool.ReturnComponentToPool(battleBall);
            });
        }
        private void ClearBallAni()
        {
            foreach (Sequence ballSeq in ballSeqList)
            {
                ballSeq?.Kill();
            }
            ballSeqList.Clear();
            BattleBallPool.ClearOutComponent();
        }

        #endregion

        #region 倒数321

        private Sequence count321Seq;
        private List<Sequence> countOneSeqList = new();
        [SerializeField] private GameObject count321Panel;
        [SerializeField] private List<Image> countImageList = new();

        private void ClearCount321Ani()
        {
            countIndex = 0;
            foreach (Sequence countOneSeq in countOneSeqList)
            {
                countOneSeq?.Kill();
            }
            countOneSeqList.Clear();
            count321Seq?.Kill();
            count321Panel.SetActive(false);
        }
        private float timeOffsetOneSecond = 1.0f;//1秒
        private float timeOffsetCount321 = 0.8f;//321的间隔
        private void PlayCount321Ani(Action playEndCallBack)
        {
            ClearCount321Ani();
            count321Panel.SetActive(true);
            foreach (Image countImage in countImageList)
            {
                countImage.gameObject.SetActive(false);
            }

            count321Seq = DOTween.Sequence();
            count321Seq.timeScale = playTimeScale;

            for (int i = 0; i < 3; i++)
            {
                count321Seq.AppendCallback(SetOneCountAni);
                count321Seq.AppendInterval(timeOffsetCount321);
            }
            if (timeOffsetOneSecond > timeOffsetCount321)
            {
                count321Seq.AppendInterval(timeOffsetOneSecond - timeOffsetCount321);
            }
            count321Seq.AppendCallback(() =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BATTLE_START_WHISTLE);
                playEndCallBack?.Invoke();
            });
        }

        private int countIndex = 0;
        private void SetOneCountAni()
        {
            Image countImage = countImageList[countIndex];
            countIndex++;

            Sequence countOneSeq = DOTween.Sequence();
            countOneSeq.timeScale = playTimeScale;
            countOneSeqList.Add(countOneSeq);
            countOneSeq.AppendCallback(() =>
            {
                countImage.gameObject.SetActive(true);
                countImage.SetAlpha(0);
                countImage.transform.localScale = Vector3.one * 15;
                AudioManager.Instance.PlaySound(AudioNames.BIBI);
            });
            countOneSeq.Append(countImage.DOFade(1, 0.3f));
            countOneSeq.Join(countImage.transform.DOScale(1.9f, 0.3f));
            countOneSeq.Append(countImage.transform.DOScale(1.5f, 0.3f));
            countOneSeq.AppendCallback(() =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_SWITCH);
            });
            countOneSeq.Append(countImage.transform.DOScale(0.4f, 0.4f).SetEase(Ease.InBack));
            countOneSeq.Join(countImage.DOFade(0, 0.3f));
            countOneSeq.AppendCallback(() =>
            {
                countImage.gameObject.SetActive(false);
            });
        }
        #endregion

        #region 分数飘字

        [SerializeField] private List<GameObject> aniScoreTextList = new();
        List<Tween> aniScoreTextTweenList = new();
        HashSet<RectTransform> aniScoreTextTransSet = new();

        private void OnShowDropScore(int score, bool isRed)//篮筐上的+2飘字
        {
            RectTransform aniScoreTextPrefabTrans = null;
            RectTransform scoreTextTrans = null;
            aniScoreTextPrefabTrans = aniScoreTextList[score - 1].GetComponent<RectTransform>();
            scoreTextTrans = GameObject.Instantiate(aniScoreTextList[score - 1].gameObject).GetComponent<RectTransform>();//篮筐上的+2飘字
            aniScoreTextTransSet.Add(scoreTextTrans);
            scoreTextTrans.SetParent(aniScoreTextPrefabTrans.parent);
            scoreTextTrans.position = aniScoreTextPrefabTrans.position;
            scoreTextTrans.localScale = aniScoreTextPrefabTrans.localScale;
            scoreTextTrans.rotation = aniScoreTextPrefabTrans.rotation;
            scoreTextTrans.gameObject.SetActive(true);
            Sequence scoreTween = GetAniScoreTextTween(scoreTextTrans, score, isRed);
            aniScoreTextTweenList.Add(scoreTween);
        }
        float[,] scoreAniTimeList = { { 0.0f, 0.2f, 0.4f, 0.2f, 1.0f }, { 0.0f, 0.25f, 0.4f, 0.25f, 1.25f }, { 0.0f, 0.3f, 0.6f, 0.3f, 1.5f } };
        private Sequence GetAniScoreTextTween(RectTransform scoreTextTrans, int score, bool isRed)
        {
            float randomT = Utility.GetRandomFloat(0f, 1f);
            scoreTextTrans.rotation = isRed ? Quaternion.Euler(0, 0, Mathf.Lerp(-20, -35, randomT)) : Quaternion.Euler(0, 0, Mathf.Lerp(20, 35, randomT));
            Vector2 targetPos = new Vector2(isRed ? Mathf.Lerp(30, 80, randomT) : Mathf.Lerp(-30, -80, randomT), 150);

            scoreTextTrans.localScale = Vector3.zero;
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(scoreAniTimeList[score - 1, 0]);
            sequence.AppendCallback(() =>
            {

            });
            sequence.Append(scoreTextTrans.DOScale(scoreAniTimeList[score - 1, 4], scoreAniTimeList[score - 1, 1]).SetEase(Ease.OutQuad));
            sequence.Join(scoreTextTrans.DOBezier2LocalMove(targetPos, scoreAniTimeList[score - 1, 2]).SetEase(Ease.InOutCubic));
            sequence.Append(scoreTextTrans.DOScale(0, scoreAniTimeList[score - 1, 3]).SetEase(Ease.InQuad));
            sequence.Join(scoreTextTrans.gameObject.DOFade(0, scoreAniTimeList[score - 1, 3]));
            sequence.AppendCallback(() =>
            {
                scoreTextTrans.gameObject.SetActive(false);
                GameObject.Destroy(scoreTextTrans.gameObject);
                aniScoreTextTransSet.Remove(scoreTextTrans);
            });
            return sequence;
        }
        private void ClearScoreAni()
        {
            foreach (var item in aniScoreTextTweenList)
            {
                item?.Kill();
            }
            aniScoreTextTweenList.Clear();
            foreach (var item in aniScoreTextTransSet)
            {
                Destroy(item?.gameObject);
            }
            aniScoreTextTransSet.Clear();
        }

        #endregion

        #region 加时赛飘字

        [SerializeField] private RectTransform overtimePopTrans;
        private Sequence overtimePopSeq = null;
        private void PlayOvertimePopAni()
        {
            ClearOverTimePopAni();
            overtimePopTrans.gameObject.SetActive(true);
            overtimePopSeq = DOTween.Sequence();
            overtimePopSeq.AppendInterval(0.5f);
            overtimePopSeq.Append(overtimePopTrans.DOScale(1, 0.5f).SetEase(Ease.OutBack));
            overtimePopSeq.AppendInterval(1.0f);
            overtimePopSeq.Append(overtimePopTrans.DOScale(0, 0.8f).SetEase(Ease.InBack));
            overtimePopSeq.AppendCallback(() =>
            {
                overtimePopTrans.gameObject.SetActive(false);
            });
        }
        private void ClearOverTimePopAni()
        {
            overtimePopSeq?.Kill();
            overtimePopTrans.gameObject.SetActive(false);
            overtimePopTrans.localScale = Vector3.zero;
        }

        #endregion

        #region 欢呼背景图更换
        [SerializeField] private List<Sprite> cheerSpriteList = new();
        private Sequence cheerSeq;
        private void ClearPlayCheerAni()//停止播放欢呼背景图更换效果,并设置为默认背景
        {
            cheerSeq?.Kill();
            cheerSeq = null;
            if (bgSpriteRenderer != null) bgSpriteRenderer.sprite = cheerSpriteList[0];
        }
        private int cheerPlayTimes = 4;//播放几个循环
        private float cheerPlayOffsetTime = 0.2f;//帧与帧的时间间隔
        private void PlayCheerAni()//播放欢呼背景图更换效果
        {
            if (cheerSeq != null) return;
            UnityTimer.Timer.Register(this.gameObject, 0.1f, () =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BATTLE_CHEERING);
            });
            cheerSeq?.Kill();
            cheerSeq = DOTween.Sequence();
            for (int i = 0; i < cheerPlayTimes; i++)
            {
                foreach (Sprite cheerSprite in cheerSpriteList)
                {
                    cheerSeq.AppendCallback(() =>
                    {
                        bgSpriteRenderer.sprite = cheerSprite;
                    });
                    cheerSeq.AppendInterval(cheerPlayOffsetTime);
                }
            }
            cheerSeq.AppendCallback(() =>
            {
                ClearPlayCheerAni();
            });
        }

        #endregion

        #region UI出现动画

        [SerializeField] private BattleUI2Anim battleUI2Anim;

        #endregion

        #region 进球粒子效果

        [SerializeField] private GameObject getScoreEffectPrefab;
        private HashSet<GameObject> getScoreEffectSet = new();
        private void PlayGetScoreEffect()
        {
            GameObject effectGo = GameObject.Instantiate(getScoreEffectPrefab, circleMidPointTrans);
            effectGo.SetLayerInThisAndAllChild(Layers.Battle2);
            getScoreEffectSet.Add(effectGo);
            Transform effectTrans = effectGo.transform;
            //effectTrans.SetParent(circleMidPointTrans);
            //effectTrans.localPosition = Vector3.zero;
            //effectTrans.localScale = Vector3.one;
            //effectTrans.rotation = Quaternion.identity;
            UnityTimer.Timer.Register(this.gameObject, 3f, () =>
            {
                GameObject.Destroy(effectGo);
                getScoreEffectSet.Remove(effectGo);
            });

        }

        private void ClearGetScoreEffect()
        {
            foreach (var item in getScoreEffectSet)
            {
                GameObject.Destroy(item);
            }
            getScoreEffectSet.Clear();
        }

        #endregion

        #region x秒后可以跳过

#if !UNITY_EDITOR
        private readonly int canSkipAfterTimeMax = 5 + 3;//321动画耗时4.5秒后可以跳过,再延后3秒可以跳过
#else
        private readonly int canSkipAfterTimeMax = 3;//编辑器内减少等待时间方便测试
#endif
        private int canSkipAfterTimeNow = 2;
        [SerializeField] private BabuButton skipBtn;
        [SerializeField] private TMP_Text skipTimeText;
        private Sequence skipShowSeq;
        private void OnSkip(BabuButton sender)
        {
            if (canSkipAfterTimeNow > 0) return;
            CheckShowDataAndClose();
        }
        private void ClearSkipShowAni()
        {
            skipShowSeq?.Kill();
            canSkipAfterTimeNow = canSkipAfterTimeMax;
            skipTimeText.gameObject.SetActive(true);
            skipTimeText.text = "({0})".SafeFormat(canSkipAfterTimeNow);
        }
        private void PlaySkipShowAni()
        {
            ClearSkipShowAni();
            skipShowSeq = DOTween.Sequence();
            for (int i = 0; i < canSkipAfterTimeMax; i++)
            {
                skipShowSeq.AppendCallback(() =>
                {
                    canSkipAfterTimeNow--;
                    skipTimeText.text = "({0})".SafeFormat(canSkipAfterTimeNow);
                });
                skipShowSeq.AppendInterval(1.0f);
            }
            skipShowSeq.AppendCallback(() =>
            {
                skipTimeText.gameObject.SetActive(false);
            });
        }

        #endregion

        #region 小节文字变大

        private readonly float stageBigTime = 0.1f;
        private readonly float stageSmallTime = 0.3f;
        private readonly float stageBigScale = 1.3f;
        private Sequence stageBigSeq;
        private void ClearStageBigAni()
        {
            stageBigSeq?.Kill();
            stageNumText.transform.localScale = Vector3.one;
        }
        private void PlayStageBigAni()
        {
            ClearStageBigAni();
            stageBigSeq = DOTween.Sequence();
            stageBigSeq.Append(stageNumText.transform.DOScale(stageBigScale, stageBigTime));
            stageBigSeq.Append(stageNumText.transform.DOScale(1f, stageSmallTime));
        }

        #endregion

        #region 爆发球员着火

        private void PlayFireOnCardAni(bool isRed, int index)
        {
            BattleCardItem battleCardItem = (isRed ? battleCardItemListHome : battleCardItemListAway)[index];
            battleCardItem.PlayFireOnCardAni();
        }
        private void ClearFireOnCardAni(bool isRed, int index)
        {
            BattleCardItem battleCardItem = (isRed ? battleCardItemListHome : battleCardItemListAway)[index];
            battleCardItem.ClearFireOnCardAni();
        }
        private void ClearFireOnCardAni()
        {
            foreach (BattleCardItem battleCardItem in battleCardItemDic.Values)
            {
                battleCardItem.ClearFireOnCardAni();
            }
        }

        #endregion

        #region 球队爆发

        [SerializeField] private StageFireItem stageFireItem = null;

        #endregion

        #endregion

        #region 游戏内数据
        [SerializeField] private GameObject dataDarkBgGo;
        [SerializeField] private GameObject BlueDataHidePanel;
        [SerializeField] private GameObject RedDataHidePanel;
        [SerializeField] private Button dataDarkBgButton;
        [SerializeField] private BabuButton teamDataBtn;
        [SerializeField] private BabuButton playerDataBtn;
        [SerializeField] private BattleInDataPad1 battleInDataPad1;
        [SerializeField] private BattleInDataPad2 battleInDataPad2;
        private void InitOnceBattleInDataPad()
        {
            battleInDataPad1.SetBgState(BattleInDataPad1.BattleInDataPad1BgState.Light);
            battleInDataPad2.SetBgState(BattleInDataPad2.BattleInDataPad2BgState.Light);
            dataDarkBgGo.SetAlpha(0);
            BlueDataHidePanel.SetAlpha(1);
            RedDataHidePanel.SetAlpha(1);
        }
        private void SetBattleInData()
        {
            Player.BattleManager.ClearRunningData();
            battleInDataPad1.InitUI();
            battleInDataPad2.SetPlayingInfo(Player.BattleManager.fightInfoData);
            Player.BattleManager.battleTeamData.onDataChange += () => { if (isClose == false) battleInDataPad2.OnTeamDataChange(); };
            Player.BattleManager.battlePlayerData.onDataChange += () => { if (isClose == false) battleInDataPad2.OnPlayerDataChange(); };
            Player.BattleManager.battleTeamData.onDataChange += battleInDataPad1.RefreshUI;
        }
        private void OnClickTeamDataBtn(BabuButton sender)
        {
            battleInDataPad2.gameObject.SetActive(false);
            battleInDataPad1.gameObject.SetActive(true);
            teamDataBtn.interactable = false;
            playerDataBtn.interactable = true;

            battleInDataPad1.ClearAni();
            battleInDataPad1.RefreshUIFast();
            battleInDataPad1.PrepareAni();
            battleInDataPad1.DoUIAni();

            ShowDataDarkBg();
        }
        private void OnClickPlayerDataBtn(BabuButton sender)
        {
            battleInDataPad2.gameObject.SetActive(true);
            battleInDataPad1.gameObject.SetActive(false);
            teamDataBtn.interactable = true;
            playerDataBtn.interactable = false;

            battleInDataPad2.ClearAni();
            if (isClose == false)
            {
                battleInDataPad2.OnPlayerDataChange();
                battleInDataPad2.OnTeamDataChange();
            }
            else
            {
                battleInDataPad2.SetEndInfo(Player.BattleManager.fightInfoData, false);
            }
            battleInDataPad2.PrepareAni();
            battleInDataPad2.DoUIAni();

            ShowDataDarkBg();
        }
        private void OnClickDataDarkBgButton()
        {
            if (isClose)
            {
                OnClose();
            }
            else
            {
                ClearAllDataPad();
            }
        }
        private HashSet<Tween> dataDarkBgTweenSet = new();
        private void ShowDataDarkBg()
        {
            dataDarkBgGo.SetActive(true);
            ClearDataDarkBgTween();
            dataDarkBgTweenSet.Add(dataDarkBgGo.DOFade(1, 1.5f));
            dataDarkBgTweenSet.Add(BlueDataHidePanel.DOFade(0, 0.5f));
            dataDarkBgTweenSet.Add(RedDataHidePanel.DOFade(0, 0.5f));
        }
        private void HideDataDarkBg()
        {
            ClearDataDarkBgTween();
            dataDarkBgGo.SetActive(false);
            dataDarkBgGo.SetAlpha(0);
            BlueDataHidePanel.SetAlpha(1);
            RedDataHidePanel.SetAlpha(1);
        }
        private void ClearDataDarkBgTween()
        {
            foreach (var dataDarkBgTween in dataDarkBgTweenSet)
            {
                dataDarkBgTween?.Kill();
            }
            dataDarkBgTweenSet.Clear();
        }
        private void ClearAllDataPad()
        {
            battleInDataPad1.ClearAni();
            battleInDataPad2.ClearAni();
            battleInDataPad2.gameObject.SetActive(false);
            battleInDataPad1.gameObject.SetActive(false);
            teamDataBtn.interactable = true;
            playerDataBtn.interactable = true;
            HideDataDarkBg();
            ClearDataDarkBgTween();
        }
        public void AddRoundData(FightPossessionInfo fightPossessionInfo, int stage)
        {
            Player.BattleManager.battleTeamData.AddRound(fightPossessionInfo, stage);
            Player.BattleManager.battlePlayerData.AddRound(fightPossessionInfo);
        }


        #endregion

        #region Debug面板

        #region 注册事件

        private void RegDebugEvents()
        {
            stopPlayBtn.OnClick += OnClickStopPlayBtn;
            replayBtn.OnClick += OnClickReplayBtn;
            countBtn.OnClick += OnClickCountBtn;
            debugBtn.OnClick += OnClickDebugBtn;
            sendBallBtn.OnClick += OnClickSendBallBtn;
            assistBallBtn.OnClick += OnClickAssistBallBtn;
            shakeHoopBtn.OnClick += OnClickShakeHoopBtn;
            highlightCardBtn.OnClick += OnClickHighlightCardBtn;
            flipCardBtn.OnClick += OnClickFlipCardBtn;
            ScoreFlyBtn.OnClick += OnClickScoreFlyBtn;
            ChangePlayerBtn.OnClick += OnClickChangePlayerBtn;
            BasketBreakBtn.OnClick += OnClickBasketBreakBtn;
            OvertimePopBtn.OnClick += OnClickOvertimePopBtn;
            ShowCardBtn.OnClick += OnClickShowCardBtn;
            CheerBtn.OnClick += OnClickCheerBtn;
            GetScoreEffectBtn.OnClick += OnClickGoalEffectBtn;
            ShowUIBtn.OnClick += OnClickShowUIBtn;
            SkipShowBtn.OnClick += OnSkipShowUIBtn;
            StageBigBtn.OnClick += OnStageBigBtn;
            cardFireBtn.OnClick += OnCardFireBtn;
            stopCardFireBtn.OnClick += OnStopCardFireBtn;
            teamFireBtn.OnClick += OnTeamFireBtn;

            frameSpeedSlider.onValueChanged.AddListener(OnChangeSpeedSlider);
            frameSpeedInputField.onEndEdit.AddListener(OnChangeInputField);
        }
        private void UnRegDebugEvents()
        {
            stopPlayBtn.OnClick -= OnClickStopPlayBtn;
            replayBtn.OnClick -= OnClickReplayBtn;
            countBtn.OnClick -= OnClickCountBtn;
            debugBtn.OnClick -= OnClickDebugBtn;
            sendBallBtn.OnClick -= OnClickSendBallBtn;
            assistBallBtn.OnClick -= OnClickAssistBallBtn;
            shakeHoopBtn.OnClick -= OnClickShakeHoopBtn;
            highlightCardBtn.OnClick -= OnClickHighlightCardBtn;
            flipCardBtn.OnClick -= OnClickFlipCardBtn;
            ScoreFlyBtn.OnClick -= OnClickScoreFlyBtn;
            ChangePlayerBtn.OnClick -= OnClickChangePlayerBtn;
            BasketBreakBtn.OnClick -= OnClickBasketBreakBtn;
            OvertimePopBtn.OnClick -= OnClickOvertimePopBtn;
            ShowCardBtn.OnClick -= OnClickShowCardBtn;
            CheerBtn.OnClick -= OnClickCheerBtn;
            GetScoreEffectBtn.OnClick -= OnClickGoalEffectBtn;
            ShowUIBtn.OnClick -= OnClickShowUIBtn;
            SkipShowBtn.OnClick -= OnSkipShowUIBtn;
            StageBigBtn.OnClick -= OnStageBigBtn;
            cardFireBtn.OnClick -= OnCardFireBtn;
            stopCardFireBtn.OnClick -= OnStopCardFireBtn;
            teamFireBtn.OnClick -= OnTeamFireBtn;

            frameSpeedSlider.onValueChanged.RemoveListener(OnChangeSpeedSlider);
            frameSpeedInputField.onEndEdit.RemoveListener(OnChangeInputField);
        }


        #endregion

        #region 播放控制
        [SerializeField] private BabuButton stopPlayBtn;
        [SerializeField] private BabuButton replayBtn;
        private void OnClickStopPlayBtn(BabuButton sender)
        {
            Clear();
        }
        private void OnClickReplayBtn(BabuButton sender)
        {
            if (Player.BattleManager.fightInfo == null)
            {
                Debug.LogError("fightInfo is null");
                return;
            }

            //do fight
            RestartBattle();
        }
        #endregion

        #region 面板开关
        [SerializeField] private GameObject debugPad;
        [SerializeField] private BabuButton debugBtn;
        private void OnClickDebugBtn(BabuButton sender)
        {
            debugPad.SetActive(!debugPad.activeSelf);
        }

        #endregion

        #region 动画调试

        #region 倒数321
        [SerializeField] private BabuButton countBtn;
        private void OnClickCountBtn(BabuButton sender)
        {
            PlayCount321Ani(null);
        }
        #endregion

        #region 直接进球
        [SerializeField] private BabuButton sendBallBtn;
        private void OnClickSendBallBtn(BabuButton sender)
        {
            PlayBallEnterAni(Utility.GetRandomBool(), Utility.GetRandomInt(0, 4), Utility.GetRandomInt(1, 3), Utility.GetRandomInt(9, 12));
        }
        #endregion

        #region 助攻进球
        [SerializeField] private BabuButton assistBallBtn;
        private void OnClickAssistBallBtn(BabuButton sender)
        {
            PlayBallAssistAni(Utility.GetRandomBool(), Utility.GetRandomInt(0, 4), Utility.GetRandomInt(0, 4), Utility.GetRandomInt(1, 3), Utility.GetRandomInt(9, 12));
        }
        #endregion

        #region 球框晃动
        [SerializeField] private BabuButton shakeHoopBtn;
        private void OnClickShakeHoopBtn(BabuButton sender)
        {
            PlayShakeHoopAni();
        }
        #endregion

        #region 卡牌变亮
        [SerializeField] private BabuButton highlightCardBtn;
        private void OnClickHighlightCardBtn(BabuButton sender)
        {
            PlayHighlightCardAni(Utility.GetRandomBool(), Utility.GetRandomInt(0, 4));
        }
        #endregion

        #region 卡牌翻转
        [SerializeField] private BabuButton flipCardBtn;
        private void OnClickFlipCardBtn(BabuButton sender)
        {
            PlayFlipCardAni(Utility.GetRandomBool(), Utility.GetRandomInt(0, 4), null);
        }
        #endregion

        #region 分数飘字
        [SerializeField] private BabuButton ScoreFlyBtn;
        private void OnClickScoreFlyBtn(BabuButton sender)
        {
            OnShowDropScore(Utility.GetRandomInt(1, 3), Utility.GetRandomBool());
        }
        #endregion

        #region 换人
        [SerializeField] private BabuButton ChangePlayerBtn;
        private void OnClickChangePlayerBtn(BabuButton sender)
        {
            PlayChangePlayerAni(Utility.GetRandomBool(), Utility.GetRandomInt(0, 4));
        }
        #endregion

        #region 球框破碎
        [SerializeField] private BabuButton BasketBreakBtn;
        private void OnClickBasketBreakBtn(BabuButton sender)
        {
            PlayBasketBreakAni();
        }
        #endregion

        #region 加时赛飘字
        [SerializeField] private BabuButton OvertimePopBtn;
        private void OnClickOvertimePopBtn(BabuButton sender)
        {
            PlayOvertimePopAni();
        }
        #endregion

        #region 卡牌出现
        [SerializeField] private BabuButton ShowCardBtn;
        private void OnClickShowCardBtn(BabuButton sender)
        {
            PlayShowCardAni();
        }
        #endregion

        #region 欢呼背景图更换

        [SerializeField] private BabuButton CheerBtn;
        private void OnClickCheerBtn(BabuButton sender)
        {
            PlayCheerAni();
        }

        #endregion

        #region 进球粒子效果

        [SerializeField] private BabuButton GetScoreEffectBtn;
        private void OnClickGoalEffectBtn(BabuButton sender)
        {
            PlayGetScoreEffect();
        }

        #endregion

        #region UI出现动画

        [SerializeField] private BabuButton ShowUIBtn;
        private void OnClickShowUIBtn(BabuButton sender)
        {
            //battleUI2Anim.PlayEnter();
        }

        #endregion

        #region x秒后可以跳过

        [SerializeField] private BabuButton SkipShowBtn;
        private void OnSkipShowUIBtn(BabuButton sender)
        {
            PlaySkipShowAni();
        }

        #endregion

        #region 小节文字变大

        [SerializeField] private BabuButton StageBigBtn;
        private void OnStageBigBtn(BabuButton sender)
        {
            PlayStageBigAni();
        }

        #endregion

        #endregion

        #region 播放速度控制
        public Slider frameSpeedSlider = null;
        public TMP_InputField frameSpeedInputField = null;
        private void RefreshFrameSpeedSlider()
        {
            frameSpeedSlider.SetValueWithoutNotify(playTimeScale);

        }
        private void RefreshFrameSpeedInputField()
        {
            frameSpeedInputField.SetTextWithoutNotify(playTimeScale.ToString());
        }
        public void OnChangeSpeedSlider(float newValue)
        {
            playTimeScale = newValue;
            RefreshFrameSpeedInputField();
        }
        public void OnChangeInputField(string newValueStr)
        {
            try
            {
                float newValue = float.Parse(newValueStr);
                playTimeScale = newValue;
                RefreshFrameSpeedSlider();
            }
            catch (Exception ex)
            {
                RefreshFrameSpeedInputField();
            }
        }
        #endregion

        #region 测试拖尾

        //private void Test()
        //{
        //    RectTransform rectTf = playBall.GetComponent<RectTransform>();
        //    Sequence seq = DOTween.Sequence();
        //    seq.Append(rectTf.DoRelativeAnchorPosX(100, 0.1f));
        //    seq.AppendInterval(0.2f);
        //    seq.Append(rectTf.DoRelativeAnchorPosX(-100, 0.1f));
        //    seq.SetLoops(-1);
        //}

        #endregion

        #region 爆发球员着火

        [SerializeField] private BabuButton cardFireBtn = null;
        [SerializeField] private BabuButton stopCardFireBtn = null;
        private void OnCardFireBtn(BabuButton sender)
        {
            PlayFireOnCardAni(Utility.GetRandomBool(), Utility.GetRandomInt(0, 4));
        }
        private void OnStopCardFireBtn(BabuButton sender)
        {
            ClearFireOnCardAni();
        }

        #endregion

        #region 球队爆发

        [SerializeField] private BabuButton teamFireBtn = null;
        private void OnTeamFireBtn(BabuButton sender)
        {
            debugPad.SetActive(false);
            int random = Utility.GetRandomInt(0, 2);
            switch (random)
            {
                case 0: stageFireItem.PlayTeamFireAni(Utility.GetRandomInt(1, 4), FormationBase.fireAddList[Utility.GetRandomInt(1, 5)], 0, Player.BattleManager.fightInfoData.fightInfo.Teams.Home.TeamIcon, Player.BattleManager.fightInfoData.fightInfo.Teams.Away.TeamIcon, null); break;
                case 1: stageFireItem.PlayTeamFireAni(Utility.GetRandomInt(1, 4), 0, FormationBase.fireAddList[Utility.GetRandomInt(1, 5)], Player.BattleManager.fightInfoData.fightInfo.Teams.Home.TeamIcon, Player.BattleManager.fightInfoData.fightInfo.Teams.Away.TeamIcon, null); break;
                case 2: stageFireItem.PlayTeamFireAni(Utility.GetRandomInt(1, 4), FormationBase.fireAddList[Utility.GetRandomInt(1, 5)], FormationBase.fireAddList[Utility.GetRandomInt(1, 5)], Player.BattleManager.fightInfoData.fightInfo.Teams.Home.TeamIcon, Player.BattleManager.fightInfoData.fightInfo.Teams.Away.TeamIcon, null); break;
            }
        }

        #endregion

        #endregion
    }
}