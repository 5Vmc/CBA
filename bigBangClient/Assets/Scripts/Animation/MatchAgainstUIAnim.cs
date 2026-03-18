using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Utils;
using Coffee.UIEffects;
using System.Collections.Generic;

namespace BigBang.Animation
{
    public class MatchAgainstUIAnim : AnimBase
    {
        [SerializeField] private Image blackImg;
        [SerializeField] private Image redLine;
        [SerializeField] private Image blueLine;
        [SerializeField] private Image vImg;
        [SerializeField] private Image sImg;
        [SerializeField] private RectTransform homeRect;
        [SerializeField] private RectTransform awayRect;
        [SerializeField] private List<Image> fadeGroup;

        public override void Init()
        {
            base.Init();
            // 初始化位置
            homeRect.SetAnchoredPositionX(-900f);
            awayRect.SetAnchoredPositionX(900f);
            vImg.rectTransform.anchoredPosition = new Vector2(-500, -9);
            sImg.rectTransform.anchoredPosition = new Vector2(500, 41);
            redLine.rectTransform.anchoredPosition = new Vector2(-624f, -500f);
            blueLine.rectTransform.anchoredPosition = new Vector2(624f, 500f);
            // 初始化透明度
            blackImg.SetAlpha(0);
            fadeGroup.ForEach(item => item.SetAlpha(0));
            vImg.SetAlpha(1);
            sImg.SetAlpha(1);
            redLine.SetAlpha(1);
            blueLine.SetAlpha(1);
        }

        public override void PlayEnter()
        {
            TouchManager.Instance.DisableTouch();
            ClearAnim();
            var vIllusion = vImg.GetComponent<IllusionAnim>();
            var sIllusion = sImg.GetComponent<IllusionAnim>();
            var vShiny = vImg.GetComponent<UIShiny>();
            var sShiny = sImg.GetComponent<UIShiny>();
            AudioManager.Instance.PlaySound(AudioNames.EVENT_MATCHVS);

            fadeGroup.ForEach(item => tweens.Add(item.DOFade(1, 0.3f)));
            tweens.Add(blueLine.DOFade(1, 0.3f));
            tweens.Add(blackImg.DOFade(155 / 255f, 0.3f));

            // 斜纹对角飞入
            tweens.Add(redLine.rectTransform.DOAnchorPos(new Vector2(-106f, -163f), 0.3f));
            tweens.Add(blueLine.rectTransform.DOAnchorPos(new Vector2(110f, 150f), 0.3f));
            tweens.Add(homeRect.DOAnchorPosX(-5.5f, 0.3f));
            tweens.Add(awayRect.DOAnchorPosX(5.5f, 0.3f));
            tweens.Add(vImg.rectTransform.DOAnchorPosX(-48f, 0.3f).SetEase(Ease.InQuart));
            tweens.Add(sImg.rectTransform.DOAnchorPosX(-20f, 0.3f).SetEase(Ease.InQuart).OnComplete(() =>
            {
                // 播放虚影
                tweens.Add(vIllusion.Play(1.5f, 0, 0.3f));
                tweens.Add(sIllusion.Play(1.5f, 0, 0.3f));
                // 播放流光
                vShiny.Play();
                sShiny.Play();
                // VS拉开
                tweens.Add(vImg.rectTransform.DOAnchorPos(new Vector2(-33f, 18f), 0.3f).SetDelay(0.3f));
                tweens.Add(sImg.rectTransform.DOAnchorPos(new Vector2(-33f, 18f), 0.3f).SetDelay(0.3f));
            }));
        }

        public override void PlayExit(Action callback)
        {
            ClearAnim();
            tweens.Add(homeRect.DOAnchorPosX(1200f, 0.3f));
            tweens.Add(awayRect.DOAnchorPosX(-1200f, 0.3f));
            fadeGroup.ForEach(item => tweens.Add(item.DOFade(0, 0.3f)));
            tweens.Add(vImg.DOFade(0, 0.3f));
            tweens.Add(sImg.DOFade(0, 0.3f));
            tweens.Add(redLine.DOFade(0, 0.3f));
            tweens.Add(blueLine.DOFade(0, 0.3f).OnComplete(() =>
            {
                TouchManager.Instance.EnableTouch();
                callback?.Invoke();
            }));
        }
    }
}