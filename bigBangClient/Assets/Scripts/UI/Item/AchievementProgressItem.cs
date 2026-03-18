using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using System;
using BigBang.Animation;
using UnityTimer;

namespace BigBang.UI
{
    public class AchievementProgressItem : MonoBehaviour
    {
        [SerializeField] private Sprite iconSprite1;
        [SerializeField] private Sprite iconSprite2;
        [SerializeField] private Sprite iconSprite3;
        [SerializeField] private Image icon;
        [SerializeField] private Image progressValue1;
        [SerializeField] private Image progressValue2;
        [SerializeField] private RectTransform lightPoint;
        [SerializeField] private new Image light;

        public event Action<int> OnDelChangePoint;
        public event Action OnProgressAnimCompleted;

        // 进度条动画时长
        private float animTime = 1;

        // 铜牌(0-100)
        int level1 = 100;
        // 银牌(100-300)
        int level2 = 300;

        public void InitAnim()
        {
            icon.rectTransform.localScale = Vector3.one;
            icon.SetAlpha(1);
            light.gameObject.SetActive(false);
            int ownPoint = Player.AchievementManager.GetOwnPoint(AchievementType.All);
            progressValue1.SetAlpha(0);
            int changePoint = Player.AchievementManager.GetPointChangeValue();
            if (changePoint <= 0)
            {
                SetIcon();
                progressValue1.fillAmount = progressValue2.fillAmount = GetProgress(ownPoint);
                return;
            }

            // 铜牌变银牌
            if (ownPoint - changePoint < level1 && ownPoint >= level1)
            {
                icon.sprite = iconSprite1;
                progressValue1.fillAmount = 1;
                progressValue2.fillAmount = GetProgress(ownPoint - changePoint);
            }
            // 银牌变金牌
            else if (ownPoint - changePoint < level2 && ownPoint >= level2)
            {
                icon.sprite = iconSprite2;
                progressValue1.fillAmount = 1;
                progressValue2.fillAmount = GetProgress(ownPoint - changePoint);
            }
            // 徽章不变
            else
            {
                SetIcon();
                progressValue1.fillAmount = GetProgress(ownPoint);
                progressValue2.fillAmount = GetProgress(ownPoint - changePoint);
            }
        }

        private void SetIcon()
        {
            int ownPoint = Player.AchievementManager.GetOwnPoint(AchievementType.All);
            int changePoint = Player.AchievementManager.GetPointChangeValue();
            if (ownPoint < level1)
            {
                icon.sprite = iconSprite1;
            }
            else if (ownPoint < level2)
            {
                icon.sprite = iconSprite2;
            }
            else
            {
                icon.sprite = iconSprite3;
            }
        }

        public void PlayAnim()
        {
            int changePoint = Player.AchievementManager.GetPointChangeValue();
            if (changePoint <= 0) return;
            int ownPoint = Player.AchievementManager.GetOwnPoint(AchievementType.All);
            progressValue1.DOFade(1, 0.3f);
            Timer.Register(this.gameObject, 0.3f + 0.5f, () =>
            {
                if (ownPoint - changePoint < level1 && ownPoint >= level1)
                {
                    PlayIconAnim(level1, () => icon.sprite = iconSprite2, () =>
                    {
                        Player.AchievementManager.ClearPointChangeValue();
                        light.gameObject.SetActive(false);
                        OnProgressAnimCompleted?.Invoke();
                    });
                }
                else if (ownPoint - changePoint < level2 && ownPoint >= level2)
                {
                    PlayIconAnim(level2, () => icon.sprite = iconSprite3, () =>
                    {
                        Player.AchievementManager.ClearPointChangeValue();
                        light.gameObject.SetActive(false);
                        OnProgressAnimCompleted?.Invoke();
                    });
                }
                else
                {
                    light.gameObject.SetActive(true);
                    // 徽章不变
                    progressValue2.DOFillAmount(progressValue1.fillAmount, animTime).OnComplete(() =>
                    {
                        Player.AchievementManager.ClearPointChangeValue();
                        OnProgressAnimCompleted?.Invoke();
                        light.gameObject.SetActive(false);
                    });
                    light.gameObject.SetActive(true);
                    DOTween.To(value => lightPoint.localEulerAngles = new Vector3(0, -180, value), GetProgress(ownPoint - changePoint) * 360, GetProgress(ownPoint) * 360, animTime);
                    light.DOFlash(3, animTime / 3f / 2f, animTime / 3f / 2f);
                    DOTween.To(value => OnDelChangePoint?.Invoke((int)value), changePoint, 0, animTime);
                }
            });
        }

        private void PlayIconAnim(int level, Action mid, Action end)
        {
            int ownPoint = Player.AchievementManager.GetOwnPoint(AchievementType.All);
            int changePoint = Player.AchievementManager.GetPointChangeValue();
            light.gameObject.SetActive(true);
            progressValue2.DOFillAmount(1, animTime).OnComplete(() =>
            {
                // 徽章缩放
                icon.rectTransform.DOScale(0.5f, 0.3f);
                // 徽章淡出
                icon.DOFade(0, 0.3f).OnComplete(() =>
                {
                    // 徽章替换
                    mid?.Invoke();
                    icon.rectTransform.localScale = Vector3.one * 3f;
                    icon.SetAlpha(1);
                    // 徽章砸下
                    icon.rectTransform.DOScale(1, 0.3f).SetEase(Ease.InExpo).OnComplete(() =>
                    {
                        progressValue2.fillAmount = 0;
                        progressValue1.fillAmount = GetProgress(ownPoint);
                        lightPoint.localEulerAngles = Vector3.zero;
                        progressValue2.DOFillAmount(progressValue1.fillAmount, animTime).OnComplete(() => end?.Invoke()).OnUpdate(() =>
                        {
                            lightPoint.localEulerAngles = new Vector3(0, -180, progressValue2.fillAmount * 360);
                        });
                        DOTween.To(value => OnDelChangePoint?.Invoke((int)value), ownPoint - level, 0, animTime);
                    });
                });
            }).OnUpdate(() =>
            {
                lightPoint.localEulerAngles = new Vector3(0, -180, progressValue2.fillAmount * 360);
            });
            DOTween.To(value => OnDelChangePoint?.Invoke((int)value), changePoint, ownPoint - level, animTime);
            light.DOFlash(3, animTime / 3f / 2f, animTime / 3f / 2f);
        }

        // 获得对应级别的进度值
        private float GetProgress(int point)
        {
            if (point <= 0) return 0;

            int totalPoint = Player.AchievementManager.GetTotalPoint(AchievementType.All);

            if (point >= totalPoint) return 1;

            if (point < level1)
            {
                return (float)point / level1;
            }
            else if (point < level2)
            {
                return (float)(point - level1) / (level2 - level1);
            }
            else
            {
                return (float)(point - level2) / (totalPoint - level2);
            }
        }
    }
}