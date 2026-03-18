using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Utils;
using System.Collections.Generic;
using UnityTimer;
using System;

namespace BigBang.Animation
{
    public class CreateIconPadAnim : AnimBase
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private Image clubIconItem;
        [SerializeField] private Image randomBtn;
        [SerializeField] private Image nextBtn;
        [SerializeField] private Image previousBtn;
        [SerializeField] private RectTransform toggleGroup;
        [SerializeField] private List<RectTransform> shapeTitle;
        [SerializeField] private List<RectTransform> colorTitle;
        [SerializeField] private List<RectTransform> shapeGroup;
        [SerializeField] private List<RectTransform> backgroundGroup;
        [SerializeField] private List<RectTransform> patternGroup;
        [SerializeField] private List<Image> breathLight;
        [SerializeField] private GameObject panel;
        [SerializeField] private List<RectTransform> selectionBoards;

        private Timer breathLightTime;
        private float loopTime = 3;
        private List<float> shapeX = new List<float>();
        private List<float> colorX = new List<float>();
        private List<float> boardX = new List<float>();

        private void Awake()
        {
            shapeTitle.ForEach(item => shapeX.Add(item.anchoredPosition.x));
            colorTitle.ForEach(item => colorX.Add(item.anchoredPosition.x));
            selectionBoards.ForEach(item => boardX.Add(item.anchoredPosition.x));
        }

        public override void Init()
        {
            base.Init();
            InitShape();
            // 初始化位置
            title.rectTransform.SetAnchoredPositionY(526);
            randomBtn.rectTransform.SetAnchoredPositionX(256);
            clubIconItem.rectTransform.SetAnchoredPositionX(0);
            toggleGroup.SetAnchoredPositionY(193.3f);
            // 初始化透明度
            panel.SetAlpha(1);
            clubIconItem.SetAlpha(0);
            randomBtn.SetAlpha(0);
            title.SetAlpha(0);
            toggleGroup.gameObject.SetAlpha(0);
            nextBtn.gameObject.SetAlpha(0);
            previousBtn.gameObject.SetAlpha(0);
            breathLightTime = Timer.Register(this.gameObject, loopTime, null, OnBreathLight, true);
        }

        private void OnDisable()
        {
            breathLightTime?.Cancel();
        }

        // 呼吸变亮 循环动画
        private void OnBreathLight(float progress)
        {
            float partCount = 4;
            foreach (var item in breathLight)
            {
                var border = item.transform.GetChild(0);
                var add = Vector3.zero;
                item.SetAlpha(0);
                if (progress < loopTime / partCount)
                {

                    add += Vector3.one * PeriodicFunction.Trigonometric(progress / (loopTime / partCount)) * 0.05f;
                    item.SetAlpha(0.2f * PeriodicFunction.Trigonometric(progress / (loopTime / partCount)));
                }
                border.localScale = Vector3.one + add;
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
            // 标题淡入
            tweens.Add(title.DOFade(1, 0.15f));
            // 标题下移
            tweens.Add(title.rectTransform.DoRelativeAnchorPosY(10, 0.15f).From().OnComplete(() =>
            {
                // Icon淡入
                tweens.Add(clubIconItem.DOFade(1, 0.15f));
                // Icon移入
                tweens.Add(clubIconItem.rectTransform.DoRelativeAnchorPosX(10, 0.15f).From());
                // 随机按钮淡入
                tweens.Add(randomBtn.DOFade(1, 0.15f));
                // 随机按钮移入
                tweens.Add(randomBtn.rectTransform.DoRelativeAnchorPosX(10, 0.15f).From().OnComplete(() =>
                {
                    PlayShape();
                    Timer.Register(this.gameObject, 0.3f, () =>
                    {
                        // 导航条淡入
                        tweens.Add(toggleGroup.gameObject.DOFade(1, 0.15f));
                        // 导航条上浮
                        tweens.Add(toggleGroup.DoRelativeAnchorPosY(-10, 0.15f).From().OnComplete(() =>
                        {
                            tweens.Add(nextBtn.gameObject.DOFade(1, 0.15f));
                            tweens.Add(previousBtn.gameObject.DOFade(1, 0.15f).OnComplete(() =>
                            {
                                TouchManager.Instance.EnableTouch();
                            }));
                        }));
                    });
                }));
            }));
        }

        public void InitShape()
        {
            int index = 0;
            shapeTitle.ForEach(item =>
            {
                item.SetAnchoredPositionX(shapeX[index++]);
                item.gameObject.SetAlpha(0);
            });
            shapeGroup.ForEach(item =>
            {
                item.gameObject.SetAlpha(0);
                item.localScale = Vector3.one * 0.7f;
            });
            backgroundGroup.ForEach(item =>
            {
                item.gameObject.SetAlpha(0);
                item.localScale = Vector3.one * 0.7f;
            });
            patternGroup.ForEach(item =>
            {
                item.gameObject.SetAlpha(0);
                item.localScale = Vector3.one * 0.7f;
            });
        }

        public void InitColor()
        {
            int index = 0;
            colorTitle.ForEach(item =>
            {
                item.SetAnchoredPositionX(colorX[index++]);
                item.gameObject.SetAlpha(0);
            });
            index = 0;
            selectionBoards.ForEach(item =>
            {
                item.SetAnchoredPositionX(boardX[index++]);
                item.gameObject.SetAlpha(0);
            });
        }

        public void PlayShape()
        {
            ClearAnim();
            float time = 0.15f;
            for (int i = 0; i < shapeTitle.Count; i++)
            {
                tweens.Add(shapeTitle[i].gameObject.DOFade(1, time).SetDelay(i * time));
                tweens.Add(shapeTitle[i].DoRelativeAnchorPosX(10, time).From().SetDelay(i * time));
            }
            Timer.Register(this.gameObject, time, () =>
            {
                AudioManager.Instance.PlaySound(AudioNames.ENT_DETAILS_SHOW_UP);
                for (int i = 0; i < shapeGroup.Count; i++)
                {
                    tweens.Add(shapeGroup[i].gameObject.DOFade(1, time).SetDelay(i * 0.1f));
                    tweens.Add(shapeGroup[i].DOScale(1, time).SetDelay(i * 0.1f));
                }
                for (int i = 0; i < backgroundGroup.Count; i++)
                {
                    tweens.Add(backgroundGroup[i].gameObject.DOFade(1, time).SetDelay(0.1f + i * 0.1f));
                    tweens.Add(backgroundGroup[i].DOScale(1, time).SetDelay(0.1f + i * 0.1f));
                }
                for (int i = 0; i < patternGroup.Count; i++)
                {
                    tweens.Add(patternGroup[i].gameObject.DOFade(1, time).SetDelay(0.2f + i * 0.1f));
                    tweens.Add(patternGroup[i].DOScale(1, time).SetDelay(0.2f + i * 0.1f));
                }
            });
        }

        public void PlayColor()
        {
            ClearAnim();
            float time = 0.15f;
            for (int i = 0; i < colorTitle.Count; i++)
            {
                tweens.Add(colorTitle[i].gameObject.DOFade(1, time).SetDelay(i * time));
                tweens.Add(colorTitle[i].DoRelativeAnchorPosX(10, time).From().SetDelay(i * time));
            }
            Timer.Register(this.gameObject, time, () =>
            {
                for (int i = 0; i < selectionBoards.Count; i++)
                {
                    tweens.Add(selectionBoards[i].gameObject.DOFade(1, time).SetDelay(i * 0.1f));
                    tweens.Add(selectionBoards[i].DoRelativeAnchorPosX(50, time).From().SetDelay(i * 0.1f));
                }
            });
        }
    }
}