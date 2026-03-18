using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class HonourPadAnim : AnimBase
    {

        [SerializeField] private RectTransform topBar;
        [SerializeField] private RectTransform toggleGroup = null;
        [SerializeField] private RectTransform topRightLightImage = null;
        [SerializeField] private RectTransform topLeftLightImage = null;

        public override void Init()
        {
            base.Init();
            topBar.SetAnchoredPositionY(60);
            toggleGroup.SetAnchoredPositionY(60);
            toggleGroup.gameObject.SetAlpha(0f);
            topRightLightImage.SetLocalRotationZ(-66);
            topLeftLightImage.SetLocalRotationZ(66);
        }

        public void PlayTopBarAnim()
        {
            Init();
            tweens.Add(topBar.DOAnchorPosY(-60, 0.3f));
            tweens.Add(toggleGroup.DOAnchorPosY(195, 0.3f));
            tweens.Add(toggleGroup.gameObject.DOFade(1f, 0.3f));
            tweens.Add(topRightLightImage.DOLocalRotate(new Vector3(0, 0, 23), 1.2f).SetDelay(0.3f));
            tweens.Add(topLeftLightImage.DOLocalRotate(new Vector3(0, 0, -23), 1.2f).SetDelay(0.3f).OnComplete(PlayLightMoveLoop));
        }

        Sequence rightLoopSeq = null;
        Sequence leftLoopSeq = null;
        private readonly float lightMoveTome = 3.5f;
        private void PlayLightMoveLoop()
        {
            rightLoopSeq?.Kill();
            rightLoopSeq = DOTween.Sequence();
            rightLoopSeq.Append(topRightLightImage.DOLocalRotate(new Vector3(0, 0, -10), lightMoveTome));
            rightLoopSeq.Append(topRightLightImage.DOLocalRotate(new Vector3(0, 0, 23), lightMoveTome));
            rightLoopSeq.SetLoops(-1);
            tweens.Add(rightLoopSeq);

            leftLoopSeq?.Kill();
            leftLoopSeq = DOTween.Sequence();
            leftLoopSeq.Append(topLeftLightImage.DOLocalRotate(new Vector3(0, 0, 10), lightMoveTome));
            leftLoopSeq.Append(topLeftLightImage.DOLocalRotate(new Vector3(0, 0, -23), lightMoveTome));
            leftLoopSeq.SetLoops(-1);
            tweens.Add(leftLoopSeq);
        }

    }
}