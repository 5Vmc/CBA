using BigBang.Animation;
using deVoid.UIFramework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    /// <summary>
    /// 带有确认取消的提示窗
    /// 带一个单选按钮
    /// </summary>
    public class ConfirmBoxCheckUIProperties : WindowProperties
    {
        public string Content;
        public Action OnConfirm;
        public Action OnCancel;

        public bool toggleCheckStart;
        public string toggleText;
        public Action<bool> onCheckChanged;

        public ConfirmBoxCheckUIProperties(string content, Action onConfirm, Action onCancel, bool toggleCheckStart, string toggleText, Action<bool> onCheckChanged)
        {
            Content = content;
            OnConfirm = onConfirm;
            OnCancel = onCancel;

            this.toggleCheckStart = toggleCheckStart;
            this.toggleText = toggleText;
            this.onCheckChanged = onCheckChanged;
        }

        public bool IsConfirmRed = false;
        public string ConfirmText = "确认";
        public string CancelText = "取消";
        /// <summary>
        /// 可选的
        /// 正常情况下确认按钮是蓝色，取消按钮是红色，此时的意思是建议玩家点击确认按钮。当此参数是 True 时表示让确认按钮是红色，取消按钮是蓝色，表示更想让玩家点击取消按钮
        /// </summary>
        /// /// <param name="isConfirmRed">正常情况下确认按钮是蓝色，取消按钮是红色，此时的意思是建议玩家点击确认按钮。当此参数是 True 时表示让确认按钮是红色，取消按钮是蓝色，表示更想让玩家点击取消按钮</param>
        public void SetConfirmColor(bool isConfirmRed = false, string confirmText = "确认", string cancelText = "取消")
        {
            IsConfirmRed = isConfirmRed;
            ConfirmText = confirmText;
            CancelText = cancelText;
        }
    }

    public class ConfirmBoxCheckUI : AWindowController<ConfirmBoxCheckUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button cancelBtn;
        [SerializeField] private Button confirmBtn;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private RectTransform cancelRedPanel = null;
        [SerializeField] private RectTransform confirmRedPanel = null;
        [SerializeField] private Button confirmBtn2 = null;
        [SerializeField] private Button cancelBtn2 = null;
        [SerializeField] private TMP_Text confirmBtnText1 = null;
        [SerializeField] private TMP_Text cancelBtnText1 = null;
        [SerializeField] private TMP_Text confirmBtnText2 = null;
        [SerializeField] private TMP_Text cancelBtnText2 = null;
        [SerializeField] private Toggle checkToggle = null;
        [SerializeField] private TMP_Text toggleLabel = null;

        public ConfirmationBoxUIAnim Anim;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            cancelBtn.onClick.AddListener(OnClose);
            confirmBtn.onClick.AddListener(OnConfirm);
            cancelBtn2.onClick.AddListener(OnClose);
            confirmBtn2.onClick.AddListener(OnConfirm);
            checkToggle.onValueChanged.AddListener(OnChangeToggle);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            cancelBtn.onClick.RemoveListener(OnClose);
            confirmBtn.onClick.RemoveListener(OnConfirm);
            cancelBtn2.onClick.RemoveListener(OnClose);
            confirmBtn2.onClick.RemoveListener(OnConfirm);
            checkToggle.onValueChanged.RemoveListener(OnChangeToggle);
        }
        private bool isCheckNow = false;
        private void OnChangeToggle(bool isCheck)
        {
            isCheckNow = isCheck;
            AudioManager.Instance.PlaySound(AudioNames.BTN_SWITCH);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            contentText.text = Properties.Content;
            cancelRedPanel.gameObject.SetActive(!Properties.IsConfirmRed);
            confirmRedPanel.gameObject.SetActive(Properties.IsConfirmRed);
            confirmBtnText1.text = Properties.ConfirmText;
            cancelBtnText1.text = Properties.CancelText;
            confirmBtnText2.text = Properties.ConfirmText;
            cancelBtnText2.text = Properties.CancelText;

            toggleLabel.text = Properties.toggleText;
            checkToggle.SetIsOnWithoutNotify(Properties.toggleCheckStart);
            isCheckNow = Properties.toggleCheckStart;
            LayoutRebuilder.ForceRebuildLayoutImmediate(toggleLabel.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(toggleLabel.transform.parent as RectTransform);

            Anim.PlayEnter();
        }

        private void OnClose()
        {
            // 取消音效
            AudioManager.Instance.PlaySound(AudioNames.BTN_CANCEL);
            Anim.PlayExit(() =>
            {
                Properties.OnCancel?.Invoke();
                UIController.Instance.CloseWindow<ConfirmBoxCheckUI>();
            });
        }

        private void OnConfirm()
        {
            Properties.OnConfirm?.Invoke();
            // 确认音效
            AudioManager.Instance.PlaySound(AudioNames.BTN_CFM);
            // 关闭窗口
            UIController.Instance.CloseWindow<ConfirmBoxCheckUI>();
            Properties.onCheckChanged?.Invoke(isCheckNow);
        }
    }
}
