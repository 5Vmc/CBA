using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using UnityTimer;
using BigBang.Animation;
using Protocol;
using GameConfig.Config;
using GameConfig;
using DG.Tweening;
using Utils;

namespace BigBang.UI
{
    public class MatchAgainstUIProperties : WindowProperties
    {
        public System.Action callback;
        public MatchAgainstUIProperties(System.Action _callback = null)
        {
            callback = _callback;
        }

    }

    public class MatchAgainstUI : AWindowController<MatchAgainstUIProperties>
    {
        [SerializeField] private ClubIconItem homeIcon;
        [SerializeField] private TMP_Text homeName;
        [SerializeField] private ClubIconItem awayIcon;
        [SerializeField] private TMP_Text awayName;
        [SerializeField] private Button closeBtn;
        [SerializeField] private TMP_Text awayText = null;
        [SerializeField] private TMP_Text homeText = null;

        [SerializeField] public MatchAgainstUIAnim Anim;
        public GameObject background;

        private Timer timer;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            background.SetAlpha(1f);
            AudioManager.Instance.StopMusic();

            awayText.gameObject.SetActive(Player.BattleManager.fightType != FightType.Hundred);
            homeText.gameObject.SetActive(Player.BattleManager.fightType != FightType.Hundred);

            homeName.text = Player.BattleManager.fightInfo.Teams.Home.TeamName;
            homeIcon.SetIcon(Player.BattleManager.fightInfo.Teams.Home.TeamIcon);

            awayName.text = Player.BattleManager.fightInfo.Teams.Away.TeamName;
            awayIcon.SetIcon(Player.BattleManager.fightInfo.Teams.Away.TeamIcon);

            Anim.Init();
            timer = Timer.Register(this.gameObject, .2f, () =>
            {
                Anim.PlayEnter();
                timer = Timer.Register(this.gameObject, 1, OnClose);
            });
        }

        private void OnClose()
        {
            timer?.Cancel();
            Anim.PlayExit(() =>
            {

                Sequence seq = DOTween.Sequence();
                //UIController.Instance.ShowPanel<MatchStartingUI>(new MatchStartingUIProperties(Properties.PVECurrent));
                seq.AppendCallback(() => { Properties.callback?.Invoke(); });
                seq.Append(background.DOFade(0f, 1f));
                seq.Insert(0f, background.transform.DOScale(1.5f, 1f));
                seq.AppendCallback(() =>
                {
                    background.transform.DOScale(1f, 0f);
                    UIController.Instance.CloseWindow<MatchAgainstUI>();
                });
            });
        }
    }
}