using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;
using Utils;
using UnityTimer;
using System;

namespace BigBang.Animation
{
    public class SelectPlayerPadAnim : AnimBase
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] Image nextBtn;
        [SerializeField] Image previousBtn;
        [SerializeField] private List<RectTransform> cardItems;
        [SerializeField] private Image randomBtn;
        [SerializeField] private TMP_Text randomTxt;
        [SerializeField] private GameObject panel;

        private Sequence randomSequence;

        private Timer resetAnim;

        private List<float> sourceY = new List<float>();

        private void Awake()
        {
            foreach (var item in cardItems)
            {
                sourceY.Add(item.anchoredPosition.y);
            }
        }

        public override void Init()
        {
            base.Init();
            // 初始化位置
            title.rectTransform.SetAnchoredPositionY(526);
            // 初始化透明度
            panel.SetAlpha(1);
            title.SetAlpha(0);
            randomBtn.gameObject.SetAlpha(0);
            int index = 0;
            cardItems.ForEach(item =>
            {
                item.gameObject.SetAlpha(0);
                item.SetAnchoredPositionY(sourceY[index++]);
            });
            randomTxt.SetAlpha(0);
            randomSequence?.Kill();
            nextBtn.gameObject.SetAlpha(0);
            previousBtn.gameObject.SetAlpha(0);

            resetAnim?.Cancel();
            resetAnim = Timer.Register(this.gameObject, 3, null, ResetLoopAnim, true);
            resetAnim.Pause();
        }

        private void OnDisable()
        {
            resetAnim?.Cancel();
            randomBtn.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        public void ResetLoopAnim(float progress)
        {
            float sumTime = 3f;
            float partCount = 8;
            if (progress >= 0 && progress < sumTime * (1f / partCount))
            {
                var value = progress / (sumTime / partCount);
                randomBtn.transform.localRotation = Quaternion.Euler(0, 0, PeriodicFunction.Linear(value) * -30);
            }
            else if (progress >= sumTime * (1f / partCount) && progress < sumTime * (2f / partCount))
            {
                var value = (progress - (sumTime * (1f / partCount))) / (sumTime / partCount);
                randomBtn.transform.localRotation = Quaternion.Euler(0, 0, PeriodicFunction.Linear(value) * 30);
            }
            else if (progress >= sumTime * (2f / partCount) && progress < sumTime * (3f / partCount))
            {
                var value = (progress - (sumTime * (2f / partCount))) / (sumTime / partCount);
                randomBtn.transform.localRotation = Quaternion.Euler(0, 0, PeriodicFunction.Linear(value) * -12);
            }
        }

        public override void PlayExit(Action callback)
        {
            panel.DOFade(0, 0.3f).OnComplete(() => callback?.Invoke());
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            TouchManager.Instance.DisableTouch();
            AudioManager.Instance.PlaySound(AudioNames.ENT_PLAYER);
            // 标题淡入
            title.DOFade(1, 0.3f);
            // 标题下滑
            title.rectTransform.DoRelativeAnchorPosY(10, 0.3f).From().OnComplete(() =>
            {
                for (int i = 0; i < cardItems.Count; i++)
                {
                    cardItems[i].gameObject.DOFade(1, 0.2f).SetDelay(i * 0.05f);
                    cardItems[i].DoRelativeAnchorPosY(-50, 0.2f).From().SetDelay(i * 0.05f);
                }
                randomBtn.gameObject.DOFade(1, 0.3f).SetDelay(0.8f).OnComplete(() =>
                {
                    // 按钮循环旋转动画
                    //PlayRandomBtnAnim();
                    resetAnim.Resume();
                    nextBtn.gameObject.DOFade(1, 0.3f);
                    previousBtn.gameObject.DOFade(1, 0.3f).OnComplete(() =>
                    {
                        TouchManager.Instance.EnableTouch();
                    });
                });
                randomTxt.DOFade(1, 0.3f).SetDelay(1);
            });
        }
    }
}