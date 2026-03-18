
using UnityEngine;
using DG.Tweening;
using Utils;
using BigBang.UI;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityTimer;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

namespace BigBang.Animation
{
    public class ArenaResultLosePadAnim : AnimBase
    {
        public Action onAniEnd;

        [SerializeField] private Image LosePeopleImage;
        [SerializeField] private Image FailImage;
        [SerializeField] private RectTransform FailImageTrans;
        [SerializeField] private List<RectTransform> sclaeTransListY;

        public override void Init()
        {
            base.Init();

            LosePeopleImage.SetAlpha(0);
            FailImage.SetAlpha(0);
            FailImageTrans.localScale = Vector3.one * 5.0f;
            foreach (RectTransform sclaeTrans in sclaeTransListY)
            {
                sclaeTrans.localScale = new Vector3(1, 0, 1);
            }
        }

        public override void PlayEnter()
        {
            base.PlayEnter();

            tweens.Add(LosePeopleImage.DOFade(1, 0.5f).SetDelay(0.4f));
            tweens.Add(FailImage.DOFade(1, 0.3f).SetDelay(0.4f + 0.3f));
            tweens.Add(FailImageTrans.DOScale(1, 0.6f).SetDelay(0.4f + 0.3f).SetEase(Ease.OutBack));

            Sequence scaleYSeq = DOTween.Sequence();
            scaleYSeq.AppendInterval(0.4f + 0.3f + 0.6f);
            for (int i = 0; i < sclaeTransListY.Count; i++)
            {
                scaleYSeq.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP); });
                scaleYSeq.Append(sclaeTransListY[i].DOScaleY(1, 0.15f).SetEase(Ease.OutBack));
            }
            tweens.Add(scaleYSeq);

            tweens.Add(DOTween.Sequence().AppendInterval(0.0f).AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON); }));
            tweens.Add(DOTween.Sequence().AppendInterval(0.4f + 0.3f + 0.1f).AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.EVENT_COMMONHIT); }));

            Sequence waitSeq = DOTween.Sequence();
            waitSeq.AppendInterval(0.4f + 0.3f + 0.5f + sclaeTransListY.Count * 0.15f);
            waitSeq.AppendCallback(() => { onAniEnd?.Invoke(); });
            tweens.Add(waitSeq);
        }
    }
}