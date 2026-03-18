using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using System;

namespace BigBang.Animation
{
    public class LeagueRewardsUIAnim : AnimBase
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
            panel.DOScale(1, 0.15f);
            blackImg.DOFade(0.5f, 0.15f);

        }

        public override void PlayExit(Action callback)
        {
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            panel.DOScale(0, 0.15f);
            blackImg.DOFade(0, 0.15f).OnComplete(() =>
            {
                callback?.Invoke();
            });
            base.PlayExit();
        }
    }
}