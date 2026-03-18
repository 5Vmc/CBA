using System;
using System.Collections;
using System.Collections.Generic;
using BigBang.Animation;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class AdapterItemAnim : MonoBehaviour
{
    private Tween fadeTween = null;
    private Tween scaleTween = null;
    // 播放动画
    public void PlayAnim(float delay)
    {
        fadeTween?.Kill();
        scaleTween?.Kill();
        fadeTween = gameObject.DOFade(1, 0.3f).SetDelay(delay);
        scaleTween = transform.DOScale(1, 0.3f).SetDelay(delay);
    }

    public void InitAnim()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();
        gameObject.SetAlpha(0);
        transform.SetLocalScale(0.8f);
    }

    private void OnDisable()
    {
        fadeTween?.Kill();
        scaleTween?.Kill();
        gameObject.SetAlpha(1);
        transform.SetLocalScale(1);
    }
}
