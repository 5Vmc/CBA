using System;
using BigBang.Animation;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class LeagueStartNoticeUIAnim : MonoBehaviour
{
    [SerializeField] private Sequence sequence = null;
    [SerializeField] private Image blackImage = null;
    [SerializeField] private RectTransform moveTrans = null;
    [SerializeField] public Action animEndCallback = null;
    [SerializeField] private float moveTime = 0.15f;
    [SerializeField] private Ease ease = Ease.OutQuad;

    [SerializeField] private GameObject closeTipPanel = null;

    public void OnEnable()
    {
        sequence?.Kill();
        if (blackImage != null) blackImage.color = new Color(0, 0, 0, 0);
        if (moveTrans != null) moveTrans.localScale = Vector3.zero;
        closeTipPanel.gameObject.SetAlpha(0f);
        closeTipPanel.gameObject.SetActive(false);

        PlayShowAni();
    }

    public void PlayShowAni()
    {
        sequence = DOTween.Sequence();
        sequence.AddTo(this.gameObject);
        if (blackImage != null) sequence.Join(blackImage.DOFade(0.5f, moveTime).SetEase(ease));
        if (moveTrans != null) sequence.Append(moveTrans.DOScale(1, moveTime).SetEase(ease));
        sequence.AppendCallback(() => { animEndCallback?.Invoke(); });
        sequence.AppendInterval(0.2f);
        sequence.AppendCallback(() => { closeTipPanel.gameObject.SetActive(true); });
        sequence.Append(closeTipPanel.gameObject.DOFade(1.0f, 0.6f));
    }

}
