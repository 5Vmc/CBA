using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.Animation
{
    public class ButtonAnim : MonoBehaviour
    {
        private bool isPlaying = false;

        private Color redColor = new Color(1, 211 / 255f, 215 / 255f, 1);

        //缩小后放大
        public void Play(Action callback = null, float delay = 0, bool playAudio = true, Action audioCallback = null)
        {
            if (!isPlaying)
            {
                if (playAudio)
                {
                    AudioManager.Instance.PlaySound(AudioNames.BTN_1);
                }
                audioCallback?.Invoke();
                isPlaying = true;
                transform.DOScale(0.95f, 0.05f).SetDelay(delay).OnComplete(() =>
                {
                    transform.DOScale(1, 0.05f).OnComplete(() =>
                    {
                        callback?.Invoke();
                        isPlaying = false;
                    });
                });
            }
        }

        public void PlayNull(Action callback = null)
        {
            if (!isPlaying)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_NULL);
                isPlaying = true;
                transform.DOShakePosition(0.5f, new Vector3(10, 0, 0), 50).OnComplete(() =>
                {
                    callback?.Invoke();
                    isPlaying = false;
                });
                var img = GetComponent<Image>();
                img.color = redColor;
                //变色
                img.DOColor(Color.white, 0.3f);
            }
        }

        public void PlayBack(Action callback = null, Action playAudio = null)
        {
            if (!isPlaying)
            {
                if (playAudio == null)
                {
                    AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
                    AudioManager.Instance.PlaySound(AudioNames.BTN_BACKBG);
                }
                else
                {
                    playAudio.Invoke();
                }
                isPlaying = true;
                transform.DOScale(0.95f, 0.05f).OnComplete(() =>
                {
                    transform.DOScale(1, 0.05f).OnComplete(() =>
                    {
                        callback?.Invoke();
                        isPlaying = false;
                    });
                });
            }
        }
    }
}