using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using BigBang.UI;
using TMPro;
using Utils;

namespace BigBang.Animation
{
    public class DialogueBoxUIAnim : AnimBase
    {
        [SerializeField] private Image blackImg;
        [SerializeField] private Image panel;
        [SerializeField] private TMP_Text content;
        [SerializeField] BabuButton btn;

        public override void Init()
        {
            base.Init();
            // 初始化缩放
            panel.rectTransform.localScale = Vector3.zero;
            // 初始化透明度
            blackImg.SetAlpha(0);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            // 面板弹出音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_POP);
            // 黑色背景淡入
            blackImg.DOFade(0.5f, 0.15f);
            // 面板缩放
            panel.rectTransform.DOScale(1, 0.15f);
            AudioManager.Instance.PlaySound(AudioNames.BOARD_POP);
        }

        public override void PlayExit(Action callback)
        {
            ClearAnim();
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            // 面板缩放
            panel.rectTransform.DOScale(0, 0.15f);
            // 背景淡出
            blackImg.DOFade(0, 0.15f).OnComplete(() =>
            {
                callback?.Invoke();
            });
        }
    }
}