using BigBang.Animation;
using deVoid.UIFramework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    [Serializable]
    public class StoreReviewUIProperties : WindowProperties
    {
        [HideInInspector] public string Content;
        [HideInInspector] public Action OnConfirm;
        [HideInInspector] public Action OnCancel;
        [HideInInspector] public string ConfirmText;
        [HideInInspector] public string CancelText;

        public StoreReviewUIProperties(string content = null, Action onConfirm = null, Action onCancel = null, string confirmText = "五星好评", string cancelText = "残忍拒绝")
        {
            Content = content;
            OnConfirm = onConfirm;
            OnCancel = onCancel;
            ConfirmText = confirmText;
            CancelText = cancelText;
        }
    }

    public class StoreReviewUI : AWindowController<StoreReviewUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button cancelBtn;
        [SerializeField] private Button confirmBtn;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private TMP_Text confirmBtnText = null;
        [SerializeField] private TMP_Text cancelBtnText = null;

        public StoreReviewUIAnim Anim;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            cancelBtn.onClick.AddListener(OnClose);
            confirmBtn.onClick.AddListener(OnConfirm);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            cancelBtn.onClick.RemoveListener(OnClose);
            confirmBtn.onClick.RemoveListener(OnConfirm);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            if (string.IsNullOrEmpty(Properties.Content) == false) contentText.text = Properties.Content;
            if (string.IsNullOrEmpty(Properties.ConfirmText) == false) confirmBtnText.text = Properties.ConfirmText;
            if (string.IsNullOrEmpty(Properties.CancelText) == false) cancelBtnText.text = Properties.CancelText;
            Anim.PlayEnter(contentText.text);
        }

        private void OnClose()
        {
            // 取消音效
            AudioManager.Instance.PlaySound(AudioNames.BTN_CANCEL);
            Anim.PlayExit(() =>
            {
                Properties.OnCancel?.Invoke();
                UIController.Instance.CloseWindow<StoreReviewUI>();
            });
        }

        private void OnConfirm()
        {
            Properties.OnConfirm?.Invoke();
            // 确认音效
            AudioManager.Instance.PlaySound(AudioNames.BTN_CFM);
            // 关闭窗口
            UIController.Instance.CloseWindow<StoreReviewUI>();

            PlayerPrefs.SetInt(PlayerPrefsKeys.NeedShowStoreReview, 0);
            //打开iOS系统的 AppStore 评价弹窗
            Debug.Log("拉起 iOS 系统的 AppStore 评价弹窗");
#if UNITY_IOS
            bool isSuccess = UnityEngine.iOS.Device.RequestStoreReview();
            Debug.Log("Device.RequestStoreReview siccess = " + isSuccess);
#endif

        }
    }
}
