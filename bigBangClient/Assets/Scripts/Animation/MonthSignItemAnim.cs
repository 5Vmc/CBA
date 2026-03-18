using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using System;

namespace BigBang.Animation
{
    public class MonthSignItemAnim : AnimBase
    {
        [SerializeField] private Image signImg;
        [SerializeField] private Image blackImg;
        [SerializeField] private RectTransform missImg;
        [SerializeField] private RectTransform lightRect;

        public void InitMiss()
        {
            missImg.gameObject.SetAlpha(1);
            missImg.rotation = Quaternion.Euler(0, 0, 0);
            missImg.SetAnchoredPositionY(36.5f);
        }

        public void InitMissAnim()
        {
            // 初始化动画
            missImg.gameObject.SetAlpha(0);
            missImg.rotation = Quaternion.Euler(0, 0, 0);
            missImg.SetAnchoredPositionY(45);
        }

        // 播放补签动画
        public void PlayMissEnter(float delay)
        {
            // 播放动画
            missImg.gameObject.DOFade(1, 0.3f).SetDelay(delay);
            missImg.DOAnchorPosY(36.5f, 0.3f).SetDelay(delay);
        }

        // 掉补签
        public void PlayMissExit(Action callback)
        {
            missImg.DOAnchorPosY(0f, 0.3f);
            missImg.gameObject.DOFade(0, 0.3f);
            missImg.DORotate(new Vector3(0, 0, -30), 0.3f).OnComplete(() =>
            {
                callback?.Invoke();
            });
        }

        // 播放打勾动画
        public void PlaySign()
        {
            lightRect.gameObject.SetActive(false);
            missImg.gameObject.SetActive(false);
            blackImg.gameObject.SetActive(true);

            // 初始化动画
            signImg.fillAmount = 0;
            blackImg.SetAlpha(0);
            // 播放动画
            signImg.DOFillAmount(1, 0.3f);
            blackImg.DOFade(153 / 255f, 0.3f);
        }
    }
}