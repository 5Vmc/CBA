using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Utils;
using Coffee.UIEffects;

namespace BigBang.Animation
{
    public class LabourDayHomeButtonAnim : AnimBase
    {
        [SerializeField] private Image dotNodeImg = null;

        public override void Init()
        {
            base.Init();

            transform.SetLocalScale(0);
            dotNodeImg.gameObject.transform.SetLocalScale(0);
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
            seq.Append(transform.DOScale(1f, 0.8f).SetEase(Ease.OutBack));
            seq.AppendInterval(0.2f);
            seq.Join(dotNodeImg.gameObject.transform.DOScale(1f, 0.8f).SetEase(Ease.OutBack));
        }
    }
}