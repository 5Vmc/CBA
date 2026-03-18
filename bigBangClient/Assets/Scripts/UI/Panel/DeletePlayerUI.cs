using Babu.SDK;
using BigBang.Animation;
using deVoid.UIFramework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{

    public class DeletePlayerUI : AWindowController
    {
        [SerializeField] private Button closeBtn = null;
        [SerializeField] private Button confirmBtn = null;
        [SerializeField] private Button cancelBtn = null;
        [SerializeField] private TMP_InputField inputField = null;

        public ConfirmationBoxUIAnim Anim;

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
            Anim.PlayEnter();
        }

        private void OnClose()
        {
            // 取消音效
            AudioManager.Instance.PlaySound(AudioNames.BTN_CANCEL);
            Anim.PlayExit(() =>
            {
                UIController.Instance.CloseWindow<DeletePlayerUI>();
            });
        }

        private void OnConfirm()
        {
            // 确认音效
            AudioManager.Instance.PlaySound(AudioNames.BTN_CFM);
            
            if(inputField.text == "删除账号")
            {
                DeletePlayer();
            }
            else
            {
                Tips.PopTips("请按提示输入文字");
            }
        }

        private void DeletePlayer()
        {
            // 关闭窗口
            UIController.Instance.CloseWindow<DeletePlayerUI>();
            NetworkManager.Instance.DeletePlayer(() =>
            {
                SDKManager.Instance.CloseGame();
                LoginManager.Instance.BackToLogin();
            });
        }
    }
}
