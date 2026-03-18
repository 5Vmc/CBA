using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class EmailItem : MonoBehaviour
{
    [SerializeField] private Image whiteImg;

    Sequence s;
    public void PlayLightAnim(Action callback)
    {
        s = DOTween.Sequence();
        s.Append(whiteImg.DOFade(0.2f, 0.1f));
        s.Append(whiteImg.DOFade(0, 0.1f));
        s.AppendCallback(() => callback?.Invoke());
    }

    private void OnDisable()
    {
        s?.Kill();
        s = null;
    }
}
