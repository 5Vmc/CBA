using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

namespace Babu.UI
{
    public class PopWindowNormalAnim : MonoBehaviour
    {
        private Sequence sequence = null;
        private Image blackImage = null;//背景黑色
        private RectTransform rectTransformMove = null;

        public void PlayShowAni(Transform moveTrans, Transform blackTrans, Action animEndCallback, float moveTime = 0.15f, Ease ease = Ease.OutQuad)
        {
            if (blackImage == null)
            {
                blackImage = blackTrans.GetComponent<Image>();
            }
            if (moveTrans != null)
            {
                rectTransformMove = moveTrans.GetComponent<RectTransform>();
            }

            sequence?.Kill();
            sequence = DOTween.Sequence();
            //AudioManager.Instance.PlaySound(AudioNames.BOARD_POP);

            if (blackImage != null) blackImage.color = new Color(0, 0, 0, 0);
            if (rectTransformMove != null) rectTransformMove.localScale = Vector3.zero;

            if (rectTransformMove != null)
            {
                sequence.Append(rectTransformMove.DOScale(1, moveTime).SetEase(ease));
            }
            if (blackImage != null)
            {
                sequence.Join(blackImage.DOFade(0.5f, moveTime).SetEase(ease));
            }
            sequence.AppendCallback(() => { animEndCallback?.Invoke(); });
        }
    }
}