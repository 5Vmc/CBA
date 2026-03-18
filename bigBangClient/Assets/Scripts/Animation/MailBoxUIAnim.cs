using BigBang.UI;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.Animation
{
    public class MailBoxUIAnim : AnimBase
    {
        [SerializeField] private Button deleteBtn;
        [SerializeField] private Button receiveBtn;
        [SerializeField] private Button closeBtn;
        [SerializeField] private MailAdapter osa;
        [SerializeField] private RectTransform top;
        [SerializeField] private RectTransform bottom;

        private Vector2 topAnchorPos;
        private Vector2 bottomAnchorPos;

        private void Awake()
        {
            topAnchorPos = top.anchoredPosition;
            bottomAnchorPos = bottom.anchoredPosition;
        }

        public override void Init()
        {
            base.Init();
            top.anchoredPosition = topAnchorPos + new Vector2(0, top.rect.height);
            bottom.anchoredPosition = bottomAnchorPos - new Vector2(0, bottom.rect.height);
            top.gameObject.SetAlpha(0);
            bottom.gameObject.SetAlpha(0);
            deleteBtn.gameObject.SetAlpha(0);
            receiveBtn.gameObject.SetAlpha(0);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            osa.InitAnim();
            top.DOAnchorPosY(topAnchorPos.y, 0.3f);
            top.gameObject.DOFade(1, 0.3f);
            bottom.DOAnchorPosY(bottomAnchorPos.y, 0.3f);
            bottom.gameObject.DOFade(1, 0.3f).OnComplete(() =>
            {
                osa.AnimIn();
            });
            deleteBtn.gameObject.DOFade(1, 0.3f);
            receiveBtn.gameObject.DOFade(1, 0.3f);
        }

        public override void PlayExit(Action callback)
        {
            top.DOAnchorPosY(topAnchorPos.y + top.rect.height, 0.3f);
            top.gameObject.DOFade(0, 0.3f);
            bottom.gameObject.DOFade(0, 0.3f);
            deleteBtn.gameObject.DOFade(0, 0.3f);
            receiveBtn.gameObject.DOFade(0, 0.3f);
            osa.AnimOut();
            Timer.Register(this.gameObject, 0.3f, callback);
        }
    }
}