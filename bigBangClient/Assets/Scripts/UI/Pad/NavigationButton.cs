using System;
using Babu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{

    public class NavigationButton : MonoBehaviour
    {
        public enum NavigationButtonType
        {
            PVP,
            Classic,
            Home,
            Train,
            Player,
        }

        [SerializeField] private NavigationPad navigationPad = null;

        [SerializeField] private Button button = null;
        [SerializeField] private Image lightImage = null;
        [SerializeField] private Image iconImage = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private Image redDotImage = null;
        [SerializeField] private Image lockImage = null;
        [SerializeField] public NavigationButtonType navigationButtonType = NavigationButtonType.Home;

        [SerializeField] private Color colorLight = new();
        [SerializeField] private Color colorNormal = new();
        [SerializeField] private Color colorLock = new();

        [SerializeField] private TriggerModuleType triggerModuleType = TriggerModuleType.Unknow;
        [SerializeField] private string redDotString = "";

        private void SetColor(Color color)
        {
            iconImage.color = color;
            nameText.color = color;
            lockImage.color = color;
        }

        public void RefreshState(object[] _)
        {
            RefreshLight();

            bool isModuleOpen = TriggerManager.Instance.CheckModuleOpen(triggerModuleType, false);

            if (isModuleOpen == false)
            {
                SetColor(colorLock);
                redDotImage.gameObject.SetActive(false);
                lockImage.gameObject.SetActive(true);
                return;
            }
            bool isLight = navigationPad.showPanel == this.navigationButtonType;
            SetColor(isLight ? colorLight : colorNormal);
            lockImage.gameObject.SetActive(false);

            RefreshRedDot(null);
        }
        public void RefreshLight()
        {
            bool isLight = navigationPad.showPanel == this.navigationButtonType;
            lightImage.gameObject.SetActive(isLight);
        }
        private void OnEnable()
        {
            RefreshState(null);
            button.onClick.AddListener(OnClickButton);
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
            EventManager.Instance.Register(EventID.OnTeamlevelUp, RefreshState);
            //EventManager.Instance.Register(EventID.OnRefreshNavigationUIRedDot, RefreshRedDot);
            EventManager.Instance.Register(EventID.RefreshBigBangUIRedDot, RefreshRedDot);
        }
        private void OnDisable()
        {
            if (button != null) button.onClick.RemoveListener(OnClickButton);
            EventManager.Instance?.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
            EventManager.Instance?.Unregister(EventID.OnTeamlevelUp, RefreshState);
            //EventManager.Instance.Unregister(EventID.OnRefreshNavigationUIRedDot, RefreshRedDot);
            EventManager.Instance?.Unregister(EventID.RefreshBigBangUIRedDot, RefreshRedDot);
        }
        public Action OnClick = null;
        private void OnClickButton()
        {
            bool isLight = navigationPad.showPanel == this.navigationButtonType;
            if (isLight) return;
            bool isModuleOpen = TriggerManager.Instance.CheckModuleOpen(triggerModuleType, true);
            if (!isModuleOpen) return;
            TriggerManager.Instance.JumpPanel((int)triggerModuleType, false, 0, 1);
            OnClick?.Invoke();
        }

        public void RefreshRedDot(object[] _)
        {
            bool isModuleOpen = TriggerManager.Instance.CheckModuleOpen(triggerModuleType, false);
            if (!isModuleOpen || string.IsNullOrWhiteSpace(redDotString))
            {
                redDotImage.gameObject.SetActive(false);
                return;
            }
            RedDotNode node = RedDotManager.Instance.ConfirmNode(redDotString, "");
            node.IsRed(redDotImage.transform);
        }
    }
}
