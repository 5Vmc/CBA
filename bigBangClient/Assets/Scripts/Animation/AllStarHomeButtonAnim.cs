using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Utils;
using Coffee.UIEffects;

namespace BigBang.Animation
{
    public class AllStarHomeButtonAnim : AnimBase
    {

        [SerializeField] private UIShiny iconImage = null;
        [SerializeField] private ParticleSystem starParticle = null;
        [SerializeField] private Image dotNodeImg = null;

        public override void Init()
        {
            base.Init();

            iconImage.Stop(true);
            iconImage.gameObject.transform.SetLocalScale(0);
            dotNodeImg.gameObject.transform.SetLocalScale(0);
            iconImage.effectFactor = 0f;
            starParticle.Stop(true);
            starParticle.Clear();
        }

        private void OnEnable()
        {
            PlayEnter();
        }


        public override void PlayEnter()
        {
            base.PlayEnter();

            Sequence seq = DOTween.Sequence();
            tweens.Add(seq);
            seq.AppendInterval(0.5f);
            seq.Append(iconImage.gameObject.transform.DOScale(1f, 0.8f).SetEase(Ease.OutBack));
            seq.AppendCallback(() => { iconImage.Play(); });
            seq.AppendInterval(1.5f);
            seq.Join(dotNodeImg.gameObject.transform.DOScale(1f, 0.8f).SetEase(Ease.OutBack));
            seq.AppendCallback(() => { starParticle.Play(); });
        }
    }
}