using UnityEngine;
using DG.Tweening;
using System;
using TMPro;
using Utils;
using UnityEngine.UI;

namespace BigBang.Animation
{
    public class Guide1UIAnim : AnimBase
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private Image blackImg;
        [SerializeField] private TMP_Text content1;
        [SerializeField] private TMP_Text content2;
        [SerializeField] private TMP_Text content3;
        [SerializeField] private TMP_Text content4;
        [SerializeField] private RectTransform btn;

        public override void Init()
        {
            base.Init();
            panel.localScale = new Vector3(1, 0, 1);
            content1.maxVisibleCharacters = 0;
            content2.maxVisibleCharacters = 0;
            content3.maxVisibleCharacters = 0;
            content4.maxVisibleCharacters = 0;
            blackImg.SetAlpha(1);
            content1.SetAlpha(1);
            content2.SetAlpha(1);
            content3.SetAlpha(1);
            content4.SetAlpha(1);
            btn.gameObject.SetAlpha(0);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            TouchManager.Instance.DisableTouch();
            Sequence sequence = DOTween.Sequence();
            sequence.Append(blackImg.DOFade(0, 0.3f));
            sequence.AppendInterval(0.2f);
            // 面板拉伸
            sequence.Append(panel.DOScaleY(1, 0.3f).OnStart(() =>
            {
                AudioManager.Instance.PlaySound(AudioNames.TECHBOARD_POP);
            }));
            // 打字机效果
            sequence.Append(content1.DOText(content1.text.Length * 0.03f));
            sequence.AppendInterval(0.5f);
            sequence.Append(content2.DOText(content2.text.Length * 0.03f));
            sequence.AppendInterval(0.5f);
            sequence.Append(content3.DOText(content3.text.Length * 0.03f));
            sequence.AppendInterval(0.5f);
            sequence.Append(content4.DOText(content4.text.Length * 0.03f));
            // 按钮淡入
            sequence.Append(btn.gameObject.DOFade(1, 0.3f));
            sequence.AppendCallback(TouchManager.Instance.EnableTouch);
        }

        public override void PlayExit(Action callback)
        {
            content1.DOFade(0, 0.3f);
            content2.DOFade(0, 0.3f);
            content3.DOFade(0, 0.3f);
            content4.DOFade(0, 0.3f);
            btn.gameObject.DOFade(0, 0.3f).OnComplete(() =>
            {
                panel.DOScaleY(0, 0.3f).OnComplete(() => callback?.Invoke());
            });
            blackImg.DOFade(1, 0.6f);
        }
    }
}