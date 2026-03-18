using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Utils;

namespace BigBang.Animation
{
    public class EmailDetailWindowAnim : AnimBase
    {
        [SerializeField] private Image blackImg;
        [SerializeField] private RectTransform paper;
        [SerializeField] private Image envelope;
        [SerializeField] private Button confirmBtn;
        [SerializeField] private RectTransform overdue;

        public override void Init()
        {
            base.Init();
            overdue.gameObject.SetAlpha(0);

            envelope.SetAlpha(0);
            envelope.rectTransform.localScale = Vector3.one;
            envelope.rectTransform.anchoredPosition = Vector3.zero;
            envelope.rectTransform.eulerAngles = new Vector3(0, 0, 15);

            paper.gameObject.SetAlpha(0);
            paper.localScale = Vector3.one;
            paper.anchoredPosition = Vector3.zero;
            paper.eulerAngles = new Vector3(0, 0, 10);
            confirmBtn.gameObject.SetAlpha(0);
            blackImg.SetAlpha(0);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            blackImg.DOFade(200 / 255f, 0.3f);
            envelope.rectTransform.DORotate(Vector3.zero, 0.2f).SetDelay(0.1f);
            envelope.DOFade(1, 0.2f).SetDelay(0.1f);

            paper.DORotate(Vector3.zero, 0.3f);
            paper.DOScale(1, 0.3f);
            paper.gameObject.DOFade(1, 0.3f).OnComplete(() =>
            {
                overdue.gameObject.DOFade(1, 0.3f);
                confirmBtn.gameObject.DOFade(1, 0.3f);
            });
        }

        public override void PlayExit(Action callback)
        {
            envelope.DOFade(0, 0.3f);
            envelope.rectTransform.DOScale(0, 0.3f);
            paper.DOScale(0, 0.3f);
            paper.gameObject.DOFade(0, 0.3f).OnComplete(() => callback?.Invoke());
        }
    }
}