using Coffee.UIEffects;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class HonourDetailUIAnim : AnimBase
    {

        [SerializeField] private RectTransform cupImageRoot = null;
        [SerializeField] private float blackBackgroundAlpha = 0.75f;//背景黑色透明度记录
        [SerializeField] private Image blackImage = null;//背景黑色
        [SerializeField] private RectTransform moveWindowTrans = null;
        [SerializeField] private RectMask2D cupMask = null;

        // 奖杯跳跃进入动画
        // public void PlayShowAni(Transform cupImageTrans, Transform targetTransform, Action animEndCallback = null)
        // {
        //     cupMask.enabled = false;
        //     float moveTime = 0.5f;
        //     if (moveWindowTrans != null) moveWindowTrans.localScale = Vector3.zero;
        //     if (moveWindowTrans != null) tweens.Add(moveWindowTrans.DOScale(1, 0.3f).SetEase(Ease.OutQuad));

        //     Transform moveTrans = cupImageTrans;
        //     Sequence sequence = DOTween.Sequence();
        //     tweens.Add(sequence);
        //     if (blackImage != null) blackImage.color = new Color(0, 0, 0, 0);
        //     if (targetTransform != null && moveTrans != null)
        //     {
        //         RectTransform rectTransformMove = moveTrans.GetComponent<RectTransform>();
        //         RectTransform rectRoot = rectTransformMove.parent.GetComponent<RectTransform>();
        //         RectTransform rectTargetTransform = targetTransform.GetComponent<RectTransform>();
        //         Vector3 startPosition = Utility.ConvertLocalPosition(rectTargetTransform.parent, rectTargetTransform.localPosition, rectRoot);
        //         Vector3 endPosition = Vector3.zero;
        //         rectTransformMove.localScale = targetTransform.localScale;
        //         rectTransformMove.localPosition = startPosition;
        //         sequence.Append(DOBezier2LocalMove(rectTransformMove, endPosition, moveTime, 100));
        //         sequence.Join(rectTransformMove.DOScale(Vector3.one, moveTime).SetEase(Ease.OutQuad));
        //     }
        //     if (blackImage != null) sequence.Join(blackImage.DOFade(0.75f, 0.3f));
        //     sequence.AppendCallback(() => { cupMask.enabled = true; animEndCallback?.Invoke(); });
        // }

        // 缩放动画
        public void PlayShowAni(Transform cupImageTrans, Transform targetTransform, Action animEndCallback = null)
        {
            cupMask.enabled = false;
            float moveTime = 0.5f;
            if (moveWindowTrans != null) moveWindowTrans.localScale = Vector3.zero;
            if (moveWindowTrans != null) tweens.Add(moveWindowTrans.DOScale(1, 0.3f).SetEase(Ease.OutQuad));

            Transform moveTrans = cupImageTrans;
            Sequence sequence = DOTween.Sequence();
            tweens.Add(sequence);
            if (blackImage != null) blackImage.color = new Color(0, 0, 0, 0);
            if (targetTransform != null && moveTrans != null)
            {
                RectTransform rectTransformMove = moveTrans.GetComponent<RectTransform>();
                rectTransformMove.localScale = targetTransform.localScale;
                rectTransformMove.localPosition = Vector3.one * 0.8f;
                sequence.Join(rectTransformMove.DOScale(Vector3.one, moveTime).SetEase(Ease.OutBack));
            }
            if (blackImage != null) sequence.Join(blackImage.DOFade(0.75f, 0.3f));
            sequence.AppendCallback(() => { cupMask.enabled = true; animEndCallback?.Invoke(); });
        }

        public void PlayMoveAni(Transform moveTransform, Transform targetTransform, float endScale, Action animEndCallback = null)
        {
            cupMask.enabled = true;
            float moveTime = 0.5f;
            Sequence sequence = DOTween.Sequence();
            tweens.Add(sequence);
            sequence.Append(DOBezier2LocalMove(moveTransform, targetTransform.localPosition, moveTime, 100));
            sequence.Join(moveTransform.DOScale(Vector3.one * endScale, moveTime).SetEase(Ease.OutQuad));
            sequence.AppendCallback(() => { animEndCallback?.Invoke(); });
        }

        public TweenerCore<float, float, FloatOptions> DOBezier2LocalMove(Transform transform, Vector3 endPos, float duration, float jumpHeight = 0)
        {
            Vector3 startPos = transform.localPosition;
            Vector3 controlPos = new Vector3();
            controlPos.x = (startPos.x + endPos.x) / 2;
            controlPos.y = endPos.y + jumpHeight;
            return transform.DOBezier2LocalMove(startPos, controlPos, endPos, duration);
        }

        public override void Init()
        {
            base.Init();

        }

        public override void PlayEnter()
        {
            base.PlayEnter();

        }

    }
}