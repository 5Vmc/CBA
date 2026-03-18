
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using TMPro;

using UnityEngine;

using UnityEngine.UI;
using Utils;


namespace BigBang.UI
{

    //球员数据
    public class Battle2EndDataUI : AWindowController
    {
        #region 初始化
        [SerializeField] private BattleInDataPad2 battleInDataPad2;
        protected override void Awake()
        {
            base.Awake();
        }
        [SerializeField] private BabuButton jumpBtn;
        protected override void AddListeners()
        {
            jumpBtn.OnClick += OnClose;
        }
        protected override void RemoveListeners()
        {
            jumpBtn.OnClick -= OnClose;
        }
        private void OnClose(BabuButton sender)
        {
            UIController.Instance.CloseWindow<Battle2EndDataUI>();
        }

        protected override void OnPropertiesSet()
        {
            SetUIInfo();
            OnData();
            PrepareAni();
            DoUIAni();
        }
        protected override void WhileHiding()
        {
            base.WhileHiding();
            ClearAni();
        }
        #endregion

        #region UI

        [SerializeField] private ClubIconItem blueClubIconImage;
        [SerializeField] private TMP_Text blueClubNameText;
        [SerializeField] private ClubIconItem redClubIconImage;
        [SerializeField] private TMP_Text redClubNameText;
        [SerializeField] private TMP_Text blueScoreText;
        [SerializeField] private TMP_Text redScoreText;
        private void SetUIInfo()
        {
            blueClubIconImage.SetIcon(Player.BattleManager.fightInfo.Teams.Away.TeamIcon);
            blueClubNameText.text = Player.BattleManager.fightInfo.Teams.Away.TeamName;
            redClubIconImage.SetIcon(Player.BattleManager.fightInfo.Teams.Home.TeamIcon);
            redClubNameText.text = Player.BattleManager.fightInfo.Teams.Home.TeamName;
            blueScoreText.text = Player.BattleManager.fightInfoData.awayTeamStat.Point.ToString();
            redScoreText.text = Player.BattleManager.fightInfoData.homeTeamStat.Point.ToString();
        }
        private void OnData()
        {
            battleInDataPad2.gameObject.SetActive(true);
            battleInDataPad2.SetEndInfo(Player.BattleManager.fightInfoData, false);
            battleInDataPad2.SetBgState(BattleInDataPad2.BattleInDataPad2BgState.Dark);
        }

        #endregion

        #region 动画

        private void PrepareAni()
        {
            this.gameObject.SetAlpha(0);
            jumpBtn.gameObject.SetActive(false);
            jumpBtn.gameObject.SetAlpha(0);
        }

        Sequence uiSequence1 = null;
        Sequence uiSequence2 = null;
        private void DoUIAni()
        {
            uiSequence1 = DOTween.Sequence();
            //uiSequence.AppendInterval(0.3f);
            uiSequence1.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP); });
            uiSequence1.Append(this.gameObject.DOFade(1f, 0.5f));
            battleInDataPad2.DoUIAni(() =>
            {
                uiSequence2 = DOTween.Sequence();
                uiSequence2.AppendCallback(() =>
                {
                    jumpBtn.gameObject.SetActive(true);
                });
                uiSequence2.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP); });
                uiSequence2.Append(jumpBtn.gameObject.DOFade(1f, 0.5f));
            });
        }
        public void ClearAni()
        {
            battleInDataPad2.ClearAni();
            uiSequence1?.Kill();
            uiSequence1 = null;
            uiSequence2?.Kill();
            uiSequence2 = null;
        }

        #endregion


    }
}
