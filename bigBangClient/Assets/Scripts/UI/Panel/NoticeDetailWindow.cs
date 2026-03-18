using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using Utils.GameItem;
using static BigBang.ServerNoticeData;

namespace BigBang.UI
{



    [Serializable]
    public class NoticeDetailWindowProperties : WindowProperties
    {
        public NoticeOneData noticeOneData;
        public NoticeDetailWindowProperties(NoticeOneData noticeOneData)
        {
            this.noticeOneData = noticeOneData;
        }
    }

    public class NoticeDetailWindow : AWindowController<NoticeDetailWindowProperties>
    {
        [SerializeField] private Button confirmBtn;
        [SerializeField] private Button closeBtn;
        [SerializeField] private NoticeDetailWindowAnim Anim;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text topText;
        [SerializeField] private TMP_Text midText;
        [SerializeField] private TMP_Text bottomText;

        protected override void AddListeners()
        {
            base.AddListeners();
            confirmBtn.onClick.AddListener(OnClose);
            closeBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();

            confirmBtn.onClick.RemoveListener(OnClose);
            closeBtn.onClick.RemoveListener(OnClose);
        }
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            titleText.text = Properties.noticeOneData.titleText;
            topText.text = Properties.noticeOneData.topText;
            midText.text = Properties.noticeOneData.midText;
            bottomText.text = Properties.noticeOneData.bottomText;
            ForceRebuildLayout();

            Anim.PlayEnter();
        }

        [SerializeField] private VerticalLayoutGroup content = null;
        [SerializeField] private ScrollRect scrollView = null;
        private void ForceRebuildLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(titleText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(topText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(midText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(bottomText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.transform as RectTransform);
            scrollView.verticalNormalizedPosition = 1f;
        }

        private void OnClose()
        {
            Anim.PlayExit(() => UIController.Instance.CloseWindow<NoticeDetailWindow>());
        }

    }
}
