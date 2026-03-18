using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Collections.Generic;
using Utils;
using UnityTimer;

namespace BigBang.Animation
{
    public class SelectClothesPadAnim : AnimBase
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private List<RectTransform> jerseys;
        [SerializeField] private List<Image> images;
        [SerializeField] private List<RectTransform> titles;
        [SerializeField] private List<RectTransform> shapeGroup;
        [SerializeField] private List<TMP_Text> jerseyTxts;
        [SerializeField] private RectTransform selectionBoard1;
        [SerializeField] private RectTransform selectionBoard2;
        [SerializeField] private Image nextBtn;
        [SerializeField] private Image previousBtn;
        [SerializeField] private List<Image> breath;
        [SerializeField] private GameObject panel;

        public event Action ToBig;

        private List<float> sourceX = new List<float>();
        private Tween nextBtnLoopAnim;

        private float loopTime = 3f;
        private Timer breathTimer;

        private void Awake()
        {
            jerseys.ForEach(item => sourceX.Add(item.anchoredPosition.x));
            titles.ForEach(item => sourceX.Add(item.anchoredPosition.x));
            sourceX.Add(selectionBoard1.anchoredPosition.x);
            sourceX.Add(selectionBoard2.anchoredPosition.x);
        }

        private void OnDisable()
        {
            // 关闭循环呼吸变亮动画
            nextBtnLoopAnim?.Kill();
            breathTimer?.Cancel();
        }

        public override void Init()
        {
            base.Init();
            panel.SetAlpha(1);
            nextBtnLoopAnim?.Kill();
            // 初始化位置
            title.rectTransform.SetAnchoredPositionY(526);
            // 初始化透明度
            title.SetAlpha(0);
            int index = 0;
            jerseyTxts.ForEach(item => item.SetAlpha(0));
            images.ForEach(item => item.SetAlpha(0));
            selectionBoard1.gameObject.SetAlpha(0);
            selectionBoard2.gameObject.SetAlpha(0);
            shapeGroup.ForEach(item =>
            {
                item.gameObject.SetAlpha(0);
                item.localScale = Vector3.one;
            });
            jerseys.ForEach(item =>
            {
                item.gameObject.SetAlpha(0);
                item.SetAnchoredPositionX(sourceX[index++]);
            });
            titles.ForEach(item =>
            {
                item.gameObject.SetAlpha(0);
                item.SetAnchoredPositionX(sourceX[index++]);
            });
            selectionBoard1.SetAnchoredPositionX(sourceX[index++]);
            selectionBoard2.SetAnchoredPositionX(sourceX[index++]);
            nextBtn.gameObject.SetAlpha(0);
            previousBtn.gameObject.SetAlpha(0);
        }

        public override void PlayExit(Action callback)
        {
            panel.DOFade(0, 0.3f).OnComplete(() => callback?.Invoke());
        }


        // 呼吸变亮 循环动画
        private void OnBreathLight(float progress)
        {
            float partCount = 4;
            foreach (var item in breath)
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

        public override void PlayEnter()
        {
            base.PlayEnter();
            TouchManager.Instance.DisableTouch();
            // 循环呼吸变亮动画(变动范围0-0.3f)
            breathTimer = Timer.Register(this.gameObject, loopTime, null, OnBreathLight, true);
            title.DOFade(1, 0.3f);
            title.rectTransform.DoRelativeAnchorPosY(10, 0.3f).From().OnComplete(() =>
            {
                AudioManager.Instance.PlaySound(AudioNames.ENT_DETAILS_SHOW_UP);
                for (int i = 0; i < jerseys.Count; i++)
                {
                    // 右侧飞入
                    jerseys[i].DoRelativeAnchorPosX(50, 0.3f).SetDelay(i * 0.1f).From();
                    // 淡入
                    jerseys[i].gameObject.DOFade(1, 0.3f).SetDelay(i * 0.1f);
                    jerseyTxts[i].DOFade(1, 0.3f).SetDelay(i * 0.1f);
                }
                images.ForEach(item => item.DOFade(1, 0.3f).SetDelay(0.6f));
                float time = 0.15f;
                Timer.Register(this.gameObject, 0.6f, ToBig);
                // 行标题淡入
                for (int i = 0; i < titles.Count; i++)
                {
                    titles[i].gameObject.DOFade(1, time).SetDelay(i * time);
                    titles[i].DoRelativeAnchorPosX(50, time).SetDelay(i * time).From();
                }
                Timer.Register(this.gameObject, time, () =>
                {
                    for (int i = 0; i < shapeGroup.Count; i++)
                    {
                        shapeGroup[i].gameObject.DOFade(1, time).SetDelay(i * 0.1f);
                        shapeGroup[i].DOScale(0.7f, time).From().SetDelay(i * 0.1f);
                    }
                    Timer.Register(this.gameObject, time, () =>
                    {
                        // 颜色行飞入
                        selectionBoard1.DoRelativeAnchorPosX(50, time).From();
                        selectionBoard2.DoRelativeAnchorPosX(50, time).SetDelay(0.1f).From();
                        // 颜色行淡入
                        selectionBoard1.gameObject.DOFade(1, time);
                        selectionBoard2.gameObject.DOFade(1, time).SetDelay(0.1f).OnComplete(() =>
                        {
                            nextBtn.gameObject.DOFade(1, 0.3f);
                            previousBtn.gameObject.DOFade(1, 0.3f).OnComplete(() =>
                            {
                                TouchManager.Instance.EnableTouch();
                            });
                        });
                    });
                });
            });
        }
    }
}