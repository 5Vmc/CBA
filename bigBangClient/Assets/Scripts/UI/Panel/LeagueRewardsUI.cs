using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;

namespace BigBang.UI
{
    public class LeagueRewardsUIProperties : WindowProperties
    {
        public int CompitionID;
        public int leagueLevel;

        public LeagueRewardsUIProperties(int compitionID, int leagueLevel)
        {
            CompitionID = compitionID;
            this.leagueLevel = leagueLevel;
        }
    }

    public class LeagueRewardsUI : AWindowController<LeagueRewardsUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private BabuToggle rankToggle;
        [SerializeField] private BabuToggle otherToggle;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private LeagueRewardsUIAdapter adapter;

        public LeagueRewardsUIAnim Anim;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            rankToggle.OnSelect += OnRankToggleSelect;
            otherToggle.OnSelect += OnOtherToggleSelect;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            rankToggle.OnSelect -= OnRankToggleSelect;
            otherToggle.OnSelect -= OnOtherToggleSelect;
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            switch (Properties.CompitionID)
            {
                case CompitionID.League:
                    // 联赛奖励
                    titleText.text = Lang.Get(LangID.LeagueNameText) + Lang.Get(LangID.RewardText);
                    break;
                case CompitionID.Cup:
                    // 杯赛奖励
                    titleText.text = Lang.Get(LangID.CupRewardText);
                    break;
            }
            toggleGroup.Switch(rankToggle);
            adapter.SetData(true, Properties.CompitionID, Properties.leagueLevel);
            Anim.PlayEnter();

        }

        private void OnRankToggleSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            adapter.SetData(true, Properties.CompitionID, Properties.leagueLevel);
        }

        private void OnOtherToggleSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            adapter.SetData(false, Properties.CompitionID, Properties.leagueLevel);
        }

        private void OnClose()
        {
            Anim.PlayExit(() => UIController.Instance.CloseWindow<LeagueRewardsUI>());
        }
    }
}