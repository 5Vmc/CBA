using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameConfig.Config;
using GameConfig;
using static BigBang.AllStarManager;
using Utils.GameItem;

namespace BigBang.UI
{
    public class PlayoffFinalsGuessHelpUI : AWindowController
    {
        #region 初始化与监听
        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClickClose;
            toggleGroup.OnValueChanged += OnToggleChanged;
        }
        protected override void RemoveListeners()
        {
            closeBtn.OnClick -= OnClickClose;
            toggleGroup.OnValueChanged -= OnToggleChanged;
        }
        [SerializeField] private ScrollRect rewardScrollView = null;
        [SerializeField] private RectTransform rulePanel = null;
        protected override void OnPropertiesSet()
        {
            rewardScrollView.enabled = false;
            SetReward();
            toggleGroup.Switch(1);
            UnityTimer.Timer.Register(this.gameObject, 0.2f, () =>
            {
                rewardScrollView.verticalNormalizedPosition = 1f;
                rewardScrollView.enabled = true;
            });
        }
        [SerializeField] private BabuButton closeBtn = null;
        private void OnClickClose(BabuButton _)
        {
            UIController.Instance.CloseWindow<PlayoffFinalsGuessHelpUI>();
        }
        #endregion

        #region 设置奖励

        [SerializeField] private GameObject rewardItemPrefab = null;
        [SerializeField] private VerticalLayoutGroup contentPanel = null;
        private bool isSetReward = false;
        private void SetReward()
        {
            if (isSetReward)
            {
                return;
            }
            isSetReward = true;
            foreach (FinalsGuessRewardConfig finalsGuessRewardConfig in Configs.FinalsGuessReward.GetConfigList())
            {
                GameObject rewardItemGo = Instantiate(rewardItemPrefab, contentPanel.transform);
                rewardItemGo.gameObject.SetActive(true);
                PlayoffFinalsGuessHelpRewardItem playoffFinalsGuessHelpRewardItem = rewardItemGo.GetComponent<PlayoffFinalsGuessHelpRewardItem>();
                playoffFinalsGuessHelpRewardItem.SetData(finalsGuessRewardConfig);
            }
        }

        #endregion

        #region 底部页签

        [SerializeField] private BabuToggleGroup toggleGroup;
        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            int selectIndex = toggleGroup.EnableIndex;
            rewardScrollView.verticalNormalizedPosition = 1f;
            rulePanel.gameObject.SetActive(selectIndex == 1);
            rewardScrollView.gameObject.SetActive(selectIndex == 0);
        }

        #endregion

    }
}