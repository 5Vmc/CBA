using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Utils;
using BigBang.UI;
using System.Collections.Generic;
using UnityTimer;

namespace BigBang.Animation
{
    public class BountyTaskPadAnim : AnimBase
    {

        [SerializeField] private BountyTaskAdapter bountyTaskAdapter = null;
        [SerializeField] private List<GameObject> fadeGroup;

        public override void Init()
        {
            base.Init();
            bountyTaskAdapter.InitAnim();
            fadeGroup.ForEach(item => item.DOFade(0, 0));
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            AudioManager.Instance.PlaySound(AudioNames.ENT_REG);
            fadeGroup.ForEach(item => tweens.Add(item.DOFade(1, 0.3f)));
            bountyTaskAdapter.PlayAnim();
        }

        public override void PlayExit(Action callback)
        {
            base.PlayExit();
            bountyTaskAdapter.PlayExit();
            fadeGroup.ForEach(item => tweens.Add(item.DOFade(0, 0.2f)));
            DOTween.Sequence().AppendInterval(0.2f).AppendCallback(() =>
            {
                callback?.Invoke();
            });
        }
    }
}