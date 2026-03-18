using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class ClassicFightEnterPlayerItemAnim : AnimBase
    {
        [SerializeField] private RectTransform views = null;
        [SerializeField] private Image upImage = null;
        [SerializeField] private Image downImage = null;
        [SerializeField] private TMP_Text fightPointText = null;

        [SerializeField] private bool isBlue = false;

        public override void Init()
        {
            base.Init();

            views.localPosition = Vector3.zero;
            views.SetLocalPositionX(isBlue ? -150 : 150);
            views.gameObject.SetAlpha(0);
            // upImage.SetAlpha(0);
            // downImage.SetAlpha(0);
            upImage.transform.SetLocalScaleX(0);
            downImage.transform.SetLocalScaleX(0);
            fightPointText.text = "";
        }

        public void PlayEnter(int fightPoint)
        {
            base.PlayEnter();

            Sequence sequence = DOTween.Sequence();
            sequence.Append(views.gameObject.DOFade(1, 0.3f));
            sequence.Join(views.DOLocalMoveX(0, 0.3f).SetEase(Ease.OutBack));
            sequence.Append(fightPointText.DOChangeNumber(fightPoint, 0.3f).SetEase(Ease.Linear));
            // sequence.Append(upImage.DOFade(1, 0.3f));
            // sequence.Join(downImage.DOFade(1, 0.3f));
            sequence.Join(upImage.transform.DOScaleX(1, 0.3f));
            sequence.Join(downImage.transform.DOScaleX(1, 0.3f));
            tweens.Add(sequence);
        }
    }
}
