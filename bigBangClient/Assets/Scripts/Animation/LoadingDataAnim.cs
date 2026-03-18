using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class LoadingDataAnim : AnimBase
    {
        [SerializeField] private Transform circleImageTransform = null;
        public override void Init()
        {
            base.Init();
            this.gameObject.SetAlpha(0);
            circleImageTransform.SetLocalScale(1.0f);
        }

        [SerializeField] private float delay = 0.0f;
        public override void PlayEnter()
        {
            base.PlayEnter();
            this.gameObject.SetActive(true);
            tweens.Add(this.gameObject.DOFade(1f, 0.3f).SetDelay(delay));
            tweens.Add(circleImageTransform.DOScale(1.0f, 0.3f).SetDelay(delay));
        }

        public override void PlayExit()
        {
            base.PlayExit();
            tweens.Add(this.gameObject.DOFade(0f, 0.2f));
            tweens.Add(circleImageTransform.DOScale(1.5f, 0.2f).OnComplete(() => this.gameObject.SetActive(false)));
        }
    }
}
