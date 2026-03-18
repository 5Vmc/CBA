using System;
using System.Collections.Generic;
using BigBang.UI;
using UnityEngine;
using DG.Tweening;
using Utils;

namespace BigBang.Animation
{
    public class BigBangCDAnim : MonoBehaviour
    {
        [SerializeField] private BigBangPadComponent com;

        private List<Tween> tweens = new List<Tween>();

        private void Init()
        {
            com.CdPad.localScale = new Vector3(1, 0, 1);
            com.CDTitle.SetAlpha(0);
            com.CDText.SetAlpha(0);
            com.ClockImg.SetAlpha(0);
            com.ClearCDBtn.gameObject.SetAlpha(0);
        }

        public void Play(Action callback = null)
        {
            Kill();
            Init();
            //上下展开
            tweens.Add(com.CdPad.DOScale(1, 0.3f).SetEase(Ease.InQuart).OnComplete(() =>
            {
                tweens.Add(com.CDTitle.DOFade(1, 0.2f));
                tweens.Add(com.CDText.DOFade(1, 0.2f));
                tweens.Add(DOTween.To(value => com.ClearCDBtn.gameObject.SetAlpha(value), 0, 1, 0.2f));
                tweens.Add(com.ClockImg.DOFade(1, 0.2f).OnComplete(() =>
                 {
                     callback?.Invoke();
                 }));
            }));
        }

        [EditorButton("播放倒计时动画")]
        private void PlayOverAnim()
        {
            var cdTime = Player.TrainManager.BigBangController.BigBangCDSecond();
            com.CDText.SetAlpha(0);
            //倒计时闪烁
            tweens.Add(com.CDText.DOFlash2(4, 0.05f, 0.05f, 0.15f).SetDelay(0.03f).OnComplete(() =>
            {
                com.CDText.SetAlpha(0);
                tweens.Add(com.CDTitle.DOFade(0, 0.3f));
                //加速按钮淡出
                tweens.Add(DOTween.To(value => com.ClearCDBtn.gameObject.SetAlpha(value), 1, 0, 0.3f).OnComplete(() =>
                {
                    //上下缩小
                    tweens.Add(com.CdPad.DOScaleY(0, 0.3f).SetEase(Ease.InQuart));
                }));
            }));
            //时钟闪烁
            tweens.Add(com.ClockImg.DOFlash2(4, 0.05f, 0.05f, 0.15f).SetDelay(0.03f).OnComplete(() =>
            {
                com.ClockImg.SetAlpha(0);
            }));
        }

        //倒计时结束动画
        public void PlayOver(Action callback)
        {
            var cdTime = Player.TrainManager.BigBangController.BigBangCDSecond();
            Action action = () =>
            {
                TouchManager.Instance.DisableTouch();
                com.CDText.SetAlpha(0);
                //倒计时闪烁
                tweens.Add(com.CDText.DOFlash2(4, 0.05f, 0.05f, 0.15f).SetDelay(0.03f).OnComplete(() =>
                {
                    com.CDText.SetAlpha(0);
                    tweens.Add(com.CDTitle.DOFade(0, 0.3f));
                    //加速按钮淡出
                    tweens.Add(DOTween.To(value => com.ClearCDBtn.gameObject.SetAlpha(value), 1, 0, 0.3f).OnComplete(() =>
                    {
                        //上下缩小
                        tweens.Add(com.CdPad.DOScaleY(0, 0.3f).SetEase(Ease.InQuart).OnComplete(() =>
                        {
                            TouchManager.Instance.EnableTouch();
                            callback?.Invoke();
                        }));
                    }));
                }));
                //时钟闪烁
                tweens.Add(com.ClockImg.DOFlash2(4, 0.05f, 0.05f, 0.15f).SetDelay(0.03f).OnComplete(() =>
                {
                    com.ClockImg.SetAlpha(0);
                }));
            };
            if (cdTime > 0)
            {
                TouchManager.Instance.DisableTouch();
                DOTween.To(value => com.CDText.text = TimeSpan.FromSeconds((int)(value)).ToString(), cdTime, 0, 1.5f).OnComplete(() =>
                {
                    action?.Invoke();
                });
            }
            else
            {
                action?.Invoke();
            }

        }

        private void Kill()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }

        private void OnDisable()
        {
            Kill();
        }
    }
}
