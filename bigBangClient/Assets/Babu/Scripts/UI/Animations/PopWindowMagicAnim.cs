using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace Babu.UI
{
    public class PopWindowMagicAnim : MonoBehaviour
    {
        Sequence sequence = null;
        private float blackBackgroundAlpha = 0.75f;//背景黑色透明度记录
        private Image blackImage = null;//背景黑色

        public void PlayShowAni(Transform targetTransform, Transform moveTrans, Transform blackTrans, Action animEndCallback, float moveTime = 1.0f, Ease ease = Ease.OutQuad)
        {
            if (blackImage == null)
            {
                blackImage = blackTrans.GetComponent<Image>();
                if (blackImage != null)
                    blackBackgroundAlpha = blackImage.color.a;
            }
            sequence?.Kill();
            sequence = DOTween.Sequence();
            if (blackImage != null) blackImage.color = new Color(0, 0, 0, 0);
            if (targetTransform != null && moveTrans != null)
            {
                RectTransform rectTransformMove = moveTrans.GetComponent<RectTransform>();
                RectTransform rectRoot = rectTransformMove.parent.GetComponent<RectTransform>();
                RectTransform rectTargetTransform = targetTransform.GetComponent<RectTransform>();
                Vector3 startPosition = ConvertLocalPosition(rectTargetTransform.parent, rectTargetTransform.localPosition, rectRoot);
                Vector3 endPosition = Vector3.zero;
                rectTransformMove.localScale = Vector3.zero;
                rectTransformMove.localPosition = startPosition;
                sequence.Append(rectTransformMove.DOLocalMove(endPosition, moveTime));
                sequence.Join(rectTransformMove.DOScale(Vector3.one, moveTime).SetEase(ease));
            }
            if (blackImage != null) sequence.Join(blackImage.DOFade(blackBackgroundAlpha, moveTime / 2));
            sequence.AppendCallback(() => { animEndCallback?.Invoke(); });
        }

        public void PlayHideAni(Transform targetTransform, Transform moveTrans, Transform blackTrans, Action animEndCallback, float moveTime = 1.0f, Ease ease = Ease.OutQuad)
        {
            if (blackImage == null)
            {
                blackImage = blackTrans.GetComponent<Image>();
                if (blackImage != null)
                    blackBackgroundAlpha = blackImage.color.a;
            }
            sequence?.Kill();
            sequence = DOTween.Sequence();
            if (targetTransform != null && moveTrans != null)
            {
                RectTransform rectTransformMove = moveTrans.GetComponent<RectTransform>();
                RectTransform rectRoot = rectTransformMove.parent.GetComponent<RectTransform>();
                RectTransform rectTargetTransform = targetTransform.GetComponent<RectTransform>();
                Vector3 startPosition = Vector3.zero;
                Vector3 endPosition = ConvertLocalPosition(rectTargetTransform.parent, rectTargetTransform.localPosition, rectRoot);
                rectTransformMove.localPosition = startPosition;
                sequence.Append(rectTransformMove.DOLocalMove(endPosition, moveTime));
                sequence.Join(rectTransformMove.DOScale(Vector3.zero, moveTime).SetEase(ease));
            }
            if (blackImage != null) sequence.Join(blackImage.DOFade(0, moveTime / 2));
            sequence.AppendCallback(() => { animEndCallback?.Invoke(); });
        }

        /// <summary>
        /// 将transform下的本地坐标转换成targetTransform下的本地坐标
        /// </summary>
        public static Vector3 ConvertLocalPosition(Transform transform, Vector3 localPosition, Transform targetTransform)
        {
            return targetTransform.InverseTransformPoint(transform.TransformPoint(localPosition));
        }
    }
}