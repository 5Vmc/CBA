using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using BigBang.Animation;
using Utils;
using DG.Tweening;
using System.Collections.Generic;
using Babu;
using System.Linq;

namespace BigBang.UI
{
    public class AchievementUIProperties : PanelProperties
    {
        public AchievementUI.SubUIID SubUI = AchievementUI.SubUIID.Achievement;

        public AchievementUIProperties(AchievementUI.SubUIID ui)
        {
            SubUI = ui;
        }
    }
    public class AchievementUI : APanelController<AchievementUIProperties>
    {
        public enum SubUIID
        {
            Honour = 0,
            Achievement = 1,
        }

        [SerializeField] private Button closeBtn;
        [SerializeField] private BabuToggleGroup bottomToggleGroup;
        [SerializeField] public AchievementUIAnim Anim;
        [SerializeField] public HonourPad honourPad;
        [SerializeField] public AchievementPad achievementPad;
        [SerializeField] public Image honourRedDot;
        [SerializeField] public Image achievementRedDot;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
            bottomToggleGroup.OnValueChanged += OnToggleChanged;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
            bottomToggleGroup.OnValueChanged -= OnToggleChanged;
        }


        public void RefreshRedDot(object[] args = null)
        {
            RedDotNode honourNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Honour, "");
            honourNode.IsRed(honourRedDot.transform);
            RedDotNode activityNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Achieve, "");
            activityNode.IsRed(achievementRedDot.transform);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            bottomToggleGroup.Switch((int)Properties.SubUI);
            RefreshRedDot();
            Anim.PlayEnter();
        }

        private void OnClose()
        {
            UIController.Instance.HidePanel<AchievementUI>();
        }

        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            int selectedIndex = bottomToggleGroup.EnableIndex;
            ShowPad((SubUIID)selectedIndex);
        }
        private void ShowPad(SubUIID padIndex)
        {
            HideAllPad();
            switch (padIndex)
            {
                case SubUIID.Honour: OnShowTotal(); break;
                case SubUIID.Achievement: OnShowAchievement(); break;
            }
        }
        private void HideAllPad()
        {
            honourPad.gameObject.SetActive(false);
            achievementPad.gameObject.SetActive(false);
        }
        private void OnShowTotal()
        {
            honourPad.gameObject.SetActive(true);
            honourPad.OnShow();
        }
        private void OnShowAchievement()
        {
            achievementPad.gameObject.SetActive(true);
            achievementPad.OnShow();
        }
    }
}