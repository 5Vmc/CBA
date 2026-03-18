using System.Collections.Generic;
using System.Linq;
using BigBang.Animation;
using BigBang.Battle;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Spine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using static BigBang.BattleManager;

namespace BigBang.UI
{

    public class Battle2EndUI : APanelController
    {

        #region 显示

        [SerializeField] private Transform content;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private HorizontalAdapter horizontalAdapter;
        [SerializeField] private BabuButton CloseBtn;
        [SerializeField] private BabuButton DataBtn;

        [SerializeField] private GameObject WinImage;
        [SerializeField] private GameObject LoseImage;
        [SerializeField] private GameObject WinPanel;
        [SerializeField] private GameObject LosePanel;

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

        [SerializeField] private TMP_Text AwayTeamNameText;
        [SerializeField] private ImageFont AwayScoreImageLabel;
        [SerializeField] private ClubIconItem AwayClubIconImage;

        [SerializeField] private TMP_Text HomeTeamNameText;
        [SerializeField] private ImageFont HomeScoreImageLabel;
        [SerializeField] private ClubIconItem HomeClubIconImage;


        protected override void AddListeners()
        {
            base.AddListeners();
            CloseBtn.OnClick += OnClose;
            DataBtn.OnClick += OnData;
            gotoButton1.onClick.AddListener(OnClickGotoButton1);
            gotoButton2.onClick.AddListener(OnClickGotoButton2);
            gotoButton3.onClick.AddListener(OnClickGotoButton3);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            CloseBtn.OnClick -= OnClose;
            DataBtn.OnClick -= OnData;
            gotoButton1.onClick.RemoveListener(OnClickGotoButton1);
            gotoButton2.onClick.RemoveListener(OnClickGotoButton2);
            gotoButton3.onClick.RemoveListener(OnClickGotoButton3);
        }

        private bool isWin = true;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            AudioManager.Instance.PlayMusic(AudioNames.BGM_TRAINING);
            SetInfo();
            PrepareAni();
            DoUIAni();
        }

        private async void SetInfo()
        {
            bool isAwayWin = !Player.BattleManager.fightInfo.Result.Win;
            isWin = true;
            switch (Player.BattleManager.fightType)
            {
                case FightType.PVE:
                    isWin = isAwayWin;
                    break;
                case FightType.ARENA:
                    isWin = Player.BattleManager.battleResponse.BattleWin;
                    break;
                default:
                    break;
            }


            WinImage.SetActive(isWin);
            LoseImage.SetActive(!isWin);
            WinPanel.SetActive(isWin);
            LosePanel.SetActive(!isWin);

            AwayHeadBgImage.SetActive(isAwayWin);
            HomeHeadBgImage.SetActive(!isAwayWin);

            if (isWin)
            {
                List<GameItem> Rewards = GameItemUtils.CreateGameItems(Player.BattleManager.challengeClubConfig.Reward).ToList();
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

            if (Player.BattleManager.fightInfoData.mvpFightCard != null)
            {
                bool isMvpHome = Player.BattleManager.fightInfoData.fightCardDicHome.ContainsKey(Player.BattleManager.fightInfoData.mvpFightCard.PlayerCardId);
                bool isMvpNpc = Player.BattleManager.fightType == FightType.PVE && isMvpHome;
                MvpPlayerNameText.text = Player.BattleManager.fightInfoData.mvpFightCard.Name;
                if (isMvpNpc)
                {
                    MvpPlayerClubIconImage.SetIcon(Player.BattleManager.fightInfo.Teams.Home.TeamIcon);
                }
                else
                {
                    MvpPlayerClubIconImage.SetIcon(isMvpHome ? Player.BattleManager.fightInfo.Teams.Home.TeamIcon : Player.BattleManager.fightInfo.Teams.Away.TeamIcon);
                }
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

            AwayTeamNameText.text = Player.BattleManager.fightInfo.Teams.Away.TeamName;
            AwayScoreImageLabel.text = Player.BattleManager.fightInfoData.awayTeamStat.Point.ToString();
            AwayClubIconImage.SetIcon(Player.BattleManager.fightInfo.Teams.Away.TeamIcon);

            HomeTeamNameText.text = Player.BattleManager.fightInfo.Teams.Home.TeamName;
            HomeScoreImageLabel.text = Player.BattleManager.fightInfoData.homeTeamStat.Point.ToString();
            HomeClubIconImage.SetIcon(Player.BattleManager.fightInfo.Teams.Home.TeamIcon);
        }

        private void OnClickGotoButton1()
        {
            UIController.Instance.HidePanel<Battle2EndUI>();
            if (Player.ChallengeManager.ChallengeId <= 103)
            {
                Tips.PopTips(LangID.NeedToUnlockRecruit);
                return;
            }
            UIController.Instance.ShowPanel<RecruitUI>(new RecruitUIProperties(RecruitUI.SubUIID.Auto));
        }
        private void OnClickGotoButton2()
        {
            UIController.Instance.HidePanel<Battle2EndUI>();
            UIController.Instance.ShowPanel<CardUI>(new CardUIProperties(CardUI.SubUIID.Card));
        }
        private void OnClickGotoButton3()
        {
            Player.FightManager.FormationController.GetAndCheckDefaultFormation(FormationID.ARENA, formation =>
            {
                UIController.Instance.HidePanel<Battle2EndUI>();
                UIController.Instance.ShowPanel<ArenaUI>(new ArenaUIProperties(ArenaUI.SubUIID.Tactic));
            });
        }
        private void OnClose(BabuButton sender)
        {
            DoClose();
        }

        private void DoClose()
        {
            ClearAni();
            //UIController.Instance.HidePanel<Battle2EndUI>();

            switch (Player.BattleManager.battleEnterType)
            {
                case BattleEnterType.ChallengeUI:
                    Player.ChallengeManager.OpenChallengeUI();
                    break;
                default:
                    UIController.Instance.ShowPanel<HomeUI>();
                    break;
            }
        }

        private void OnData(BabuButton sender)
        {
            UIController.Instance.OpenWindow<Battle2EndDataUI>();
        }

        #endregion

        #region 动画

        [UnityEngine.Header("动画")]
        public List<GameObject> showList = new();
        public Image winImageImage;
        public Image loseImageImage;
        public RectTransform winImageTrans;
        public RectTransform loseImageTrans;
        public RectTransform topPointTrans;
        public RectTransform midPointTrans;
        public RectTransform topPoint219Trans;

        private void PrepareAni()
        {
            foreach (GameObject showGo in showList)
            {
                showGo.SetAlpha(0);
                showGo.SetActive(false);
            }
            this.gameObject.SetAlpha(0);
            winImageImage.SetAlpha(0);
            loseImageImage.SetAlpha(0);
            winImageTrans.localScale = Vector3.one * 20f;
            loseImageTrans.localScale = Vector3.one * 20f;
            winImageTrans.localPosition = midPointTrans.localPosition;
            loseImageTrans.localPosition = midPointTrans.localPosition;
        }

        Sequence uiSequence = null;
        private void DoUIAni()
        {
            Image moveImageImage = isWin ? winImageImage : loseImageImage;
            RectTransform moveImageTrans = isWin ? winImageTrans : loseImageTrans;

            uiSequence = DOTween.Sequence();
            //uiSequence.AppendInterval(0.3f);

            uiSequence.Append(this.gameObject.DOFade(1f, 0.5f));
            uiSequence.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.BATTLE_HIT_BOARD); });
            uiSequence.Append(moveImageTrans.DOScale(1f, 0.3f).SetEase(Ease.InCubic));
            uiSequence.Join(moveImageImage.DOFade(1f, 0.3f).SetEase(Ease.Linear));
            uiSequence.AppendInterval(0.3f);
            foreach (GameObject showGo in showList)
            {
                uiSequence.AppendCallback(() => { showGo.SetActive(true); AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP); });
                uiSequence.Append(showGo.DOFade(1f, 0.16f));
            }
            float endY = Utility.Lerp(topPointTrans.localPosition.y, topPoint219Trans.localPosition.y, UIFrame.GetFixScreenLerpT());
            uiSequence.Insert(1.1f, moveImageTrans.DOLocalMoveY(endY, 1.2f).SetEase(Ease.OutBack));
        }
        public void ClearAni()
        {
            uiSequence?.Kill();
            uiSequence = null;
        }

        #endregion

    }
}