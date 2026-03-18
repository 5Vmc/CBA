using BigBang.Animation;
using deVoid.UIFramework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class ConfirmationBoxUIProperties : WindowProperties
    {
        public string Content;
        public Action OnConfirm;
        public Action OnCancel;
        public bool IsConfirmRed;
        public string ConfirmText;
        public string CancelText;

        /// <param name="isConfirmRed">正常情况下确认按钮是蓝色，取消按钮是红色，此时的意思是建议玩家点击确认按钮。当此参数是 True 时表示让确认按钮是红色，取消按钮是蓝色，表示更想让玩家点击取消按钮</param>
        public ConfirmationBoxUIProperties(string content, Action onConfirm = null, Action onCancel = null, bool isConfirmRed = false, string confirmText = "确认", string cancelText = "取消")
        {
            Content = content;
            OnConfirm = onConfirm;
            OnCancel = onCancel;
            IsConfirmRed = isConfirmRed;
            ConfirmText = confirmText;
            CancelText = cancelText;
        }
    }

    public class ConfirmationBoxUI : AWindowController<ConfirmationBoxUIProperties>
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

        public ConfirmationBoxUIAnim Anim;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            cancelBtn.onClick.AddListener(OnClose);
            confirmBtn.onClick.AddListener(OnConfirm);
            cancelBtn2.onClick.AddListener(OnClose);
            confirmBtn2.onClick.AddListener(OnConfirm);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            cancelBtn.onClick.RemoveListener(OnClose);
            confirmBtn.onClick.RemoveListener(OnConfirm);
            cancelBtn2.onClick.RemoveListener(OnClose);
            confirmBtn2.onClick.RemoveListener(OnConfirm);
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
            Anim.PlayEnter();
        }

        private void OnClose()
        {
            // 取消音效
            AudioManager.Instance.PlaySound(AudioNames.BTN_CANCEL);
            Anim.PlayExit(() =>
            {
                Properties.OnCancel?.Invoke();
                UIController.Instance.CloseWindow<ConfirmationBoxUI>();
            });
        }

        private void OnConfirm()
        {
            Properties.OnConfirm?.Invoke();
            // 确认音效
            AudioManager.Instance.PlaySound(AudioNames.BTN_CFM);
            // 关闭窗口
            UIController.Instance.CloseWindow<ConfirmationBoxUI>();
        }
    }
}
