using DG.Tweening;
using System;
using UnityEngine;
using Utils;

namespace BigBang.Animation
{
    public class AchievementTipsUIAnim : AnimBase
    {
        [SerializeField] private RectTransform panel;

        public override void Init()
        {
            base.Init();
            gameObject.SetAlpha(0);
            panel.gameObject.SetAlpha(0);
            panel.SetAnchoredPositionY(0);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            gameObject.DOFade(1, 0.3f).OnComplete(() =>
            {
                panel.gameObject.DOFade(1, 0.3f);
                panel.DOAnchorPosY(-157, 0.3f);
            });
        }

        public override void PlayExit(Action callback)
        {
            gameObject.DOFade(0, 0.3f).OnComplete(() =>
            {
                callback?.Invoke();
            });
        }
    }
}
