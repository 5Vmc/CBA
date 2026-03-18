using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WindowAnim : MonoBehaviour
{
    [SerializeField] private Sequence sequence = null;
    [SerializeField] private Image blackImage = null;
    [SerializeField] private RectTransform moveTrans = null;
    [SerializeField] public Action animEndCallback = null;
    [SerializeField] private float moveTime = 0.15f;
    [SerializeField] private Ease ease = Ease.OutQuad;
    [SerializeField] private float darkAlpha = 0.5f;

    public void OnEnable()
    {
        PlayShowAni();
    }

    public void PlayShowAni()
    {
        sequence?.Kill();
        sequence = DOTween.Sequence();
        sequence.AddTo(this.gameObject);
        if (blackImage != null) blackImage.color = new Color(0, 0, 0, 0);
        if (moveTrans != null) moveTrans.localScale = Vector3.zero;
        if (blackImage != null) sequence.Join(blackImage.DOFade(darkAlpha, moveTime).SetEase(ease));
        if (moveTrans != null) sequence.Append(moveTrans.DOScale(1, moveTime).SetEase(ease));
        sequence.AppendCallback(() => { animEndCallback?.Invoke(); });
    }

}
