using System.Collections;
using System.Collections.Generic;
using BigBang.Animation;
using Com.TheFallenGames.OSA.Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class PlayoffFinalsGuessHomePadPlayerAnim : AnimBase
    {
        public override void Init()
        {
            base.Init();

            rectTransform.SetAnchoredPosition(startPos);
            gameObject.SetAlpha(0f);
        }

        [SerializeField] private Vector2 startPos;
        [SerializeField] private Vector2 endPos;
        private float time = 0.3f;
        private Ease ease = Ease.InQuad;

        private RectTransform _rectTransform = null;
        private RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = this.transform as RectTransform;
                }
                return _rectTransform;
            }
        }

        public void PlayEnter(float delay)
        {
            base.PlayEnter();

            tweens.Add(rectTransform.DOAnchorPos(endPos, time).SetEase(ease).SetDelay(delay));
            tweens.Add(gameObject.DOFade(1f, time).SetDelay(delay));
        }

    }
}