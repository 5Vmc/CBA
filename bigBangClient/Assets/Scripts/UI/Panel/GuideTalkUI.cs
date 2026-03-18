using System;
using BigBang.Animation;
using deVoid.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class GuideTalkUIProperties : WindowProperties
    {
        public string content = "";
        public Action closeCallBack = null;
        public GuideTalkUIProperties(string content, Action closeCallBack = null)
        {
            this.content = content;
            this.closeCallBack = closeCallBack;
        }
    }

    public class GuideTalkUI : AWindowController<GuideTalkUIProperties>
    {
        [SerializeField] private BabuButton closeButton = null;
        [SerializeField] private Image blackImage = null;
        [SerializeField] private TMP_Text contentText = null;
        [SerializeField] private TMP_Text clickTipText = null;

        [SerializeField] private GuideTalkUIAnim anim;

        protected override void OnPropertiesSet()
        {
            AudioManager.Instance.PlaySound(AudioNames.ANI_TIPS);
            contentText.text = Properties.content;
            anim.PlayEnter();
        }

        protected override void AddListeners()
        {
            closeButton.OnClick += OnClickCloseButton;
        }
        protected override void RemoveListeners()
        {
            closeButton.OnClick -= OnClickCloseButton;
        }
        private void OnClickCloseButton(BabuButton sender)
        {
            UIController.Instance.CloseWindow<GuideTalkUI>();
            Properties.closeCallBack?.Invoke();
        }
    }
}
