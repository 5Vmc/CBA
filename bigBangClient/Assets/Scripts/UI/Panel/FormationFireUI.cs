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
    public class FormationFireUI : AWindowController<WindowProperties>
    {

        [SerializeField] private Button closeBtn;
        [SerializeField] private Button confirmBtn;
        [SerializeField] private ConfirmationBoxUIAnim anim;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            anim.PlayEnter();
        }

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
            confirmBtn.onClick.AddListener(onConfirm);
           
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
            confirmBtn.onClick.RemoveListener(onConfirm);
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            anim.PlayExit(() =>
            {
                UIController.Instance.CloseWindow<FormationFireUI>();
                TouchManager.Instance.EnableTouch();
                
            });
        }

        private void onConfirm()
        {
            this.OnClose();
        }
    }
}