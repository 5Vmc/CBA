using UnityEngine;
using deVoid.UIFramework;
using TMPro;
using System;
using BigBang.Animation;

namespace BigBang.UI
{
    public class DialogueBoxUIProperties : WindowProperties
    {
        public string Title;
        public string Content;
        public string BtnTxt;
        public Action Callback;

        public DialogueBoxUIProperties(string content, string btnTxt, Action callback, string title = "")
        {
            Content = content;
            Callback = callback;
            BtnTxt = btnTxt;
            Title = title;
        }
    }

    public class DialogueBoxUI : AWindowController<DialogueBoxUIProperties>
    {
        [SerializeField] private BabuButton closeBtn;
        [SerializeField] private TMP_Text content;
        [SerializeField] private TMP_Text btnTxt;
        [SerializeField] private TMP_Text titleTxt;

        [SerializeField] public DialogueBoxUIAnim Anim;

        protected override void Awake()
        {
            base.Awake();
            closeBtn.Sound = AudioNames.BTN_CFM;
        }

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            content.text = Properties.Content;
            btnTxt.text = Properties.BtnTxt;
            titleTxt.text = Properties.Title;
            Anim.PlayEnter();
        }

        private void OnClose(BabuButton sender)
        {
            Anim.PlayExit(() =>
            {
                UIController.Instance.CloseWindow<DialogueBoxUI>();
                Properties.Callback?.Invoke();
            });
        }
    }
}