using Babu.SDK;
using BigBang.Animation;
using deVoid.UIFramework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Babu
{
    public class ConfirmationPolicyPanel : MonoBehaviour
    {
        public Action beforeConfirmAnimCallBack;
        public Action afterConfirmAnimCallBack;
        [SerializeField] private Button cancelBtn;
        [SerializeField] private Button confirmBtn;
        [SerializeField] private Button policyButton = null;
        [SerializeField] private Button privacyButton = null;

        public ConfirmationPolicyPanelAnim Anim;

        void OnEnable()
        {
            cancelBtn.onClick.AddListener(OnClose);
            confirmBtn.onClick.AddListener(OnConfirm);
            policyButton.onClick.AddListener(OnPolicy);
            privacyButton.onClick.AddListener(OnPrivacy);

            Anim.ClearAnim();
            Anim.Init();
            Anim.PlayEnter();
        }
        void OnDisable()
        {
            cancelBtn.onClick.RemoveListener(OnClose);
            confirmBtn.onClick.RemoveListener(OnConfirm);
            policyButton.onClick.RemoveListener(OnPolicy);
            privacyButton.onClick.RemoveListener(OnPrivacy);
        }

        private void OnPolicy()
        {
            var url = PolicyConst.GetPolicyUrl();
            Application.OpenURL(url);
        }

        private void OnPrivacy()
        {
            var url = PolicyConst.GetPrivacyUrl();
            Application.OpenURL(url);
        }

        private void OnClose()
        {
            Debug.LogWarning("不同意隐私协议退出游戏");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void OnConfirm()
        {
            beforeConfirmAnimCallBack?.Invoke();
            Anim.ClearAnim();
            Anim.PlayExit(() =>
            {
                afterConfirmAnimCallBack?.Invoke();
            });
        }
    }
}
