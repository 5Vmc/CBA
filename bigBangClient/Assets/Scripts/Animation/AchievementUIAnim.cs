using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class AchievementUIAnim : AnimBase
    {
        [SerializeField] private RectTransform bottom;

        public override void Init()
        {
            base.Init();
            bottom.SetAnchoredPositionY(-23);
            bottom.gameObject.DOFade(0, 0.3f);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            bottom.DOAnchorPosY(73, 0.3f);
            bottom.gameObject.DOFade(1, 0.3f);
        }
    }
}