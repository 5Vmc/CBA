using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using GameConfig;
using System.Collections.Generic;
using GameConfig.Config;

namespace BigBang.UI
{
    public class ArenaRewardsUI : AWindowController<WindowProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private BabuToggle promoteToggle;
        [SerializeField] private BabuToggle dailyToggle;
        [SerializeField] private BabuToggle seasonToggle;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private ArenaRewardsUIAdapter adapter;

        public LeagueRewardsUIAnim Anim;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            promoteToggle.OnSelect += OnPromoteToggleSelect;
            dailyToggle.OnSelect += OnDailyToggleSelect;
            seasonToggle.OnSelect += OnSeasonToggleSelect;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            promoteToggle.OnSelect -= OnPromoteToggleSelect;
            dailyToggle.OnSelect -= OnDailyToggleSelect;
            seasonToggle.OnSelect -= OnSeasonToggleSelect;
        }


        private void SetRewardsList(ArenaStageRewardType type)
        {
            List<ArenaRewardsItemModel> iList = new List<ArenaRewardsItemModel>();
            Configs.ArenaReward.GetConfigList().ForEach(item =>
            {
                if (item.Type == (int)type)
                {
                    ArenaRewardsItemModel model = new ArenaRewardsItemModel(type, item);
                    iList.Add(model);
                }
            });

            adapter.SetData(iList);
        }
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            toggleGroup.Switch(promoteToggle);
            SetRewardsList(ArenaStageRewardType.Promote);
            Anim.PlayEnter();

        }

        private void OnPromoteToggleSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            SetRewardsList(ArenaStageRewardType.Promote);
        }

        private void OnDailyToggleSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            SetRewardsList(ArenaStageRewardType.Daily);
        }

        private void OnSeasonToggleSelect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            SetRewardsList(ArenaStageRewardType.ActivityEnd);
        }

        private void OnClose()
        {
            Anim.PlayExit(() => UIController.Instance.CloseWindow<ArenaRewardsUI>());


        }
    }
}