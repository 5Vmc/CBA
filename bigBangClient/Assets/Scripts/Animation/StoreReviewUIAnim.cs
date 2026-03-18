using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using System;
using TMPro;

namespace BigBang.Animation
{
    public class StoreReviewUIAnim : AnimBase
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private Image blackImg;

        public override void Init()
        {
            base.Init();
            // 初始化缩放
            panel.localScale = Vector3.zero;
            // 初始化透明度
            blackImg.SetAlpha(0);
            contentText.text = "";
        }

        [SerializeField] private TMP_Text contentText = null;
        public void PlayEnter(string content)
        {
            base.PlayEnter();
            // 面板弹出音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_POP);
            // 黑色背景淡入
            tweens.Add(blackImg.DOFade(0.5f, 0.15f));
            // 面板缩放
            tweens.Add(panel.DOScale(1, 0.15f));

            tweens.Add(contentText.DOText(content, content.Length * 0.02f).SetDelay(0.15f));
        }

        public override void PlayExit(Action callback)
        {
            ClearAnim();
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            // 面板缩放
            tweens.Add(panel.DOScale(0, 0.15f));
            // 背景淡出
            tweens.Add(blackImg.DOFade(0, 0.15f).OnComplete(() =>
            {
                callback?.Invoke();
            }));
        }
    }
}