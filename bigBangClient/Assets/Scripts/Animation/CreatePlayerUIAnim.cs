using UnityEngine;
using DG.Tweening;
using TMPro;
using Utils;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityTimer;
using Coffee.UIEffects;

namespace BigBang.Animation
{
    public class CreatePlayerUIAnim : AnimBase
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private List<RectTransform> results;
        [SerializeField] private List<TMP_Text> texts;
        [SerializeField] private GameObject confirmBtn;
        [SerializeField] private Image backBtn;
        [SerializeField] private TMP_Text confirmTxt;
        [SerializeField] private GameObject panel;

        [SerializeField] private Sequence confirmSequence;

        private List<Vector3> sourceScale = new List<Vector3>();

        private void Awake()
        {
            results.ForEach(item => sourceScale.Add(item.localScale));
        }

        private void InitResult()
        {
            // 初始化位置
            title.rectTransform.SetAnchoredPositionY(526);
            // 初始化透明度
            panel.SetAlpha(1);
            title.SetAlpha(0);
            for (int i = 0; i < results.Count; i++)
            {
                // 砸入
                results[i].localScale = sourceScale[i];
                // 淡入
                results[i].gameObject.SetAlpha(0);
                texts[i].SetAlpha(0);
            }
            backBtn.SetAlpha(0);
            confirmBtn.gameObject.SetAlpha(0);
            confirmTxt.SetAlpha(0);
        }

        public void PlayResult()
        {
            TouchManager.Instance.DisableTouch();
            InitResult();
            AudioManager.Instance.PlaySound(AudioNames.ENT_REG);
            // 标题淡入
            title.DOFade(1, 0.15f);
            title.rectTransform.DORelativePositionY(10, 0.15f).From().OnComplete(() =>
            {
                for (int i = 0; i < results.Count; i++)
                {
                    // 砸入
                    results[i].DOScale(1.2f, 0.15f).SetDelay(i * 0.1f).From();
                    // 淡入
                    results[i].gameObject.DOFade(1, 0.15f).SetDelay(i * 0.1f);
                    texts[i].DOFade(1, 0.15f).SetDelay(i * 0.15f);
                }
                Timer.Register(this.gameObject, 0.3f, () =>
                {
                    confirmBtn.DOFade(0.2f, 0.15f).OnComplete(() =>
                    {
                        backBtn.DOFade(0.6f, 0.15f);
                        StartConfirmBtnAnim();
                    });
                    confirmTxt.DOFade(1, 0.15f).OnComplete(() =>
                    {
                        TouchManager.Instance.EnableTouch();
                    });
                });
            });

        }

        public void PlayResultBack(Action callback)
        {
            var effect = backBtn.GetComponent<UIEffect>();
            confirmSequence?.Kill();
            DOTween.To(value => effect.colorFactor = value, 0, 1, 0.15f).OnComplete(() =>
            {
                DOTween.To(value => effect.colorFactor = value, 1, 0, 0.15f).OnComplete(() =>
                {
                    panel.DOFade(0, 0.3f).OnComplete(() => callback?.Invoke());
                });
            });
        }

        // 呼吸动画
        public void StartConfirmBtnAnim()
        {
            confirmSequence?.Kill();
            confirmSequence = DOTween.Sequence();
            confirmBtn.DOFade(0.2f, 0.1f);
            confirmSequence.Append(DOTween.To(value => confirmBtn.SetAlpha(value), 0.2f, 0.5f, 1f));
            confirmSequence.Append(DOTween.To(value => confirmBtn.SetAlpha(value), 0.5f, 0.2f, 1f));
            confirmSequence.AppendInterval(1);
            confirmSequence.SetLoops(-1);
        }

        public void StopConfirmBtnAinm()
        {
            confirmSequence?.Kill();
            confirmBtn.DOFade(0.2f, 0.1f);
        }
    }
}