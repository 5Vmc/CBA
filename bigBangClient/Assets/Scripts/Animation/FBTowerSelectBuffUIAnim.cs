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
    public class FBTowerSelectBuffUIAnim : AnimBase
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private Image blackImg;
        public override void Init()
        {
            base.Init();
            blackImg.SetAlpha(0);
            panel.localScale = Vector3.zero;
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            // 面板弹出音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_POP);
            Sequence sequence = DOTween.Sequence();
            tweens.Add(sequence);

            sequence.Append(panel.DOScale(1, 0.15f));
            sequence.Join(blackImg.DOFade(0.5f, 0.15f));
        }

        public override void PlayExit(Action callback)
        {
            base.PlayExit();
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            tweens.Add(panel.DOScale(0, 0.15f));
            tweens.Add(blackImg.DOFade(0, 0.15f).OnComplete(() =>
            {
                callback?.Invoke();
            }));
            
        }
    }
}
