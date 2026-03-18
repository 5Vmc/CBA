using UnityEngine;
using DG.Tweening;
using System;
using TMPro;
using Utils;
using UnityEngine.UI;
using BigBang.UI;

namespace BigBang.Animation
{
    public class PlayMovieUIAnim : AnimBase
    {

        [SerializeField] private Image blackImg = null;
        [SerializeField] private RawImage rawImage = null;
        [SerializeField] private BabuButton closeBtn = null;

        public override void Init()
        {
            base.Init();
            blackImg.SetAlpha(0);
            rawImage.SetAlpha(0);
            closeBtn.image.SetAlpha(0);
            closeBtn.transform.localScale = Vector3.zero;
        }

        public void PlayBeforePlayMovie()
        {
            tweens.Add(blackImg.DOFade(1.0f, 0.5f));
        }
        public void PlayAfterPlayMovie()
        {
            tweens.Add(rawImage.DOFade(1.0f, 0.2f));
            tweens.Add(closeBtn.image.DOFade(1.0f, 0.2f).SetEase(Ease.Linear).SetDelay(2.5f));
            tweens.Add(closeBtn.transform.DOScale(1.0f, 0.2f).SetEase(Ease.OutBack).SetDelay(2.5f));
        }
    }
}