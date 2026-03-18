using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Utils;

namespace BigBang.Animation
{
    public class QuickExchangeUIAnim : AnimBase
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private Image blackImg;

        public override void Init()
        {
            base.Init();
            blackImg.SetAlpha(0);
            panel.localScale = Vector3.zero;
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            // 黑色背景淡入
            blackImg.DOFade(0.5f, 0.15f);
            // 面板缩放
            panel.DOScale(1, 0.15f);
            AudioManager.Instance.PlaySound(AudioNames.BOARD_POP);
        }

        public override void PlayExit(Action callback)
        {
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            // 面板缩放
            panel.DOScale(0, 0.15f);
            // 背景淡出
            blackImg.DOFade(0, 0.15f).OnComplete(() =>
            {
                callback?.Invoke();
            });
        }
    }
}