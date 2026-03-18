using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using DG.Tweening;
using System;
using Coffee.UIEffects;
using BigBang.UI;
using UnityTimer;

namespace BigBang.Animation
{
    public class FBTowerRaidResultUIAnim : AnimBase
    {
        [SerializeField] private FBTowerRaidResultItemAdapter osa = null;

        public override void Init()
        {
            base.Init();

            osa.ScrollTo(0);
        }

        public override void PlayEnter()
        {
            Init();

            osa.SmoothScrollTo(osa.Data.Count - 1, 0.5f);
            // osa.SmoothBringToView
        }
        public override void PlayExit(Action callback)
        {
            // topTitle.DORelativePositionY(200, 0.2f);
            // bottomRect.gameObject.DOFade(0, 0.2f).OnComplete(() => callback?.Invoke());
        }
    }
}
