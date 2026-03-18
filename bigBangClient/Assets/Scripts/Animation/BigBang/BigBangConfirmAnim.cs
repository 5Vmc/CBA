using System;
using System.Collections.Generic;
using BigBang.Animation;
using BigBang.UI;
using DG.Tweening;
using UnityEngine;
using Utils;

public class BigBangConfirmAnim : MonoBehaviour
{
    [SerializeField] private BigBangConfirmUIComponent com;

    private List<Tween> tweens = new List<Tween>();

    private void Init()
    {
        com.Pad.localScale = new Vector3(1, 0, 1);
        com.BigBangBtn.image.rectTransform.localScale = Vector3.one;
        com.SuperBigBangBtn.image.rectTransform.localScale = Vector3.one;
        com.BigBangBtn.image.rectTransform.localScale = Vector3.zero;
        com.SuperBigBangBtn.image.rectTransform.localScale = Vector3.zero;
        com.CloseBtn.image.SetAlpha(0);
        com.TitleText.SetAlpha(0);
        com.SuperImg.SetAlpha(0);
    }

    public void Play()
    {
        Kill();
        Init();
        //上下展开
        tweens.Add(com.Pad.DOScale(1, 0.3f).SetEase(Ease.InQuart).OnComplete(() =>
        {
            com.TitleText.SetAlpha(1);
            tweens.Add(com.CloseBtn.image.DOFade(1, 0.2f));
            tweens.Add(com.BigBangBtn.image.rectTransform.DOScale(1, 0.2f));
            //按钮缩放
            tweens.Add(com.SuperBigBangBtn.image.rectTransform.DOScale(1, 0.2f));
            tweens.Add(com.SuperBigBangBtn.image.rectTransform.DOScale(1, 0.2f).OnComplete(() =>
            {
                //Super标签闪烁3下
                tweens.Add(com.SuperImg.DOFlash(3, 0.05f, 0.05f, 0.1f));
            }));
        }));
    }

    public void PlayNext(Action callback)
    {
        tweens.Add(com.CloseBtn.image.DOFade(0, 0.2f));
        tweens.Add(com.TitleText.DOFade(0, 0.2f));
        //按钮缩小
        tweens.Add(com.BigBangBtn.image.rectTransform.DOScale(0, 0.2f));
        tweens.Add(com.SuperBigBangBtn.image.rectTransform.DOScale(0, 0.2f).OnComplete(() =>
        {
            //上下缩小
            tweens.Add(com.Pad.DOScaleY(0, 0.13f).SetEase(Ease.Linear).OnComplete(() =>
            {
                callback?.Invoke();
            }));
        }));
    }

    private void Kill()
    {
        tweens.ForEach(item => item.Kill());
        tweens.Clear();
    }
}
