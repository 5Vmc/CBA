using BigBang.Animation;
using deVoid.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class HeroChapterUI : APanelController
    {

        #region 初始化
        protected override void Awake()
        {
            base.Awake();
        }
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

        [SerializeField] private Button closeBtn;
        private void OnClose()
        {
            anim.PlayExit(() => UIController.Instance.HidePanel<HeroChapterUI>());
        }

        [SerializeField] private HeroChapterItemAdapter heroChapterItemAdapter;
        [SerializeField] private HeroChapterUIAnim anim;
        protected override void OnPropertiesSet()
        {

            base.OnPropertiesSet();

            UpdatePlayerInfo();
            SetStrength();
            anim.PlayEnter();

            heroChapterItemAdapter.SetData(new());

            HeroManager.Instance.GetHeroChapterData(() =>
            {
                heroChapterItemAdapter.SetData(HeroManager.Instance.heroChapterDataNeedShowList);
                heroChapterItemAdapter.InitAnim();
                heroChapterItemAdapter.PlayAnim();
            });
        }
        #endregion

        #region 个人信息

        [SerializeField] private TMP_Text clubNameText;//玩家俱乐部名
        [SerializeField] private ClubIconItem clubIcon;//玩家俱乐部图标
        public void UpdatePlayerInfo()
        {
            clubNameText.text = Player.Name;
            clubIcon.SetIcon(Player.Icon);
        }

        [SerializeField] private TMP_Text clubScoreText;//当前玩家战力
        private void SetStrength()
        {
            clubScoreText.text = Player.Strength.ToString();
        }

        #endregion



    }
}