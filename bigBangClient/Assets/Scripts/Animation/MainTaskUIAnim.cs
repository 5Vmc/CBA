using UnityEngine;
using DG.Tweening;
using System;
using Utils;
using System.Collections.Generic;
using UnityTimer;
using UnityEngine.UI;

namespace BigBang.Animation
{
    public class MainTaskUIAnim : AnimBase
    {
        [SerializeField] private RectTransform resourceTitle;
        [SerializeField] private RectTransform paper1;
        [SerializeField] private RectTransform paper2;
        [SerializeField] private GameObject getBtn;
        [SerializeField] private GameObject undoneBtn;
        [SerializeField] private Image closeBtn;
        [SerializeField] private List<RectTransform> tabs;

        private Timer getBtnLoopAnim;

        private void OnEnable()
        {
            getBtnLoopAnim = Timer.Register(this.gameObject, 3, null, GetButtonLoopAnim, isLooped: true);
            getBtnLoopAnim.Pause();
        }

        private void OnDisable()
        {
            getBtnLoopAnim.Cancel();
        }

        private void GetButtonLoopAnim(float time)
        {
            if (time >= 0 && time < 1)
            {
                getBtn.transform.localScale = Vector3.one + Vector3.one * PeriodicFunction.Trigonometric(time) * 0.1f;
            }
        }

        public override void Init()
        {
            base.Init();
            // 初始化位置
            resourceTitle.SetAnchoredPositionY(-60);
            tabs.ForEach(item => item.SetAnchoredPositionY(0));
            closeBtn.rectTransform.SetAnchoredPositionY(-467.5f);
            paper1.SetAnchoredPositionX(1);
            paper2.SetAnchoredPositionX(0);
            paper1.localRotation = Quaternion.Euler(0, 0, 1.57f);
            paper2.localRotation = Quaternion.Euler(0, 0, 0);
            // 初始化透明度
            closeBtn.SetAlpha(0);
            paper1.gameObject.SetAlpha(0);
            paper2.gameObject.SetAlpha(0);
            resourceTitle.gameObject.SetAlpha(0);
            getBtn.SetAlpha(0);
            undoneBtn.SetAlpha(0);
            tabs.ForEach(item => item.gameObject.SetAlpha(0));
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP);
            Timer.Register(this.gameObject, 0.2f, () => AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP));
            // TouchManager.Instance.DisableTouch();
            // 经济栏下滑
            resourceTitle.DORelativePositionY(200, 0.2f).From();
            // 经济栏淡入
            resourceTitle.gameObject.DOFade(1, 0.2f);
            // 纸张飞入
            paper1.DOAnchorPosX(200, 0.3f).From();
            paper2.DOAnchorPosX(200, 0.3f).From().SetDelay(0.1f);
            // 纸张淡入
            paper1.gameObject.DOFade(1, 0.3f);
            paper2.gameObject.DOFade(1, 0.3f).SetDelay(0.1f);
            // 纸张旋转
            paper1.DORotate(new Vector3(0, 0, -5), 0.3f, RotateMode.LocalAxisAdd).From();
            paper2.DORotate(new Vector3(0, 0, -5), 0.3f, RotateMode.LocalAxisAdd).From().SetDelay(0.1f);
            // 标签栏出现
            Timer.Register(this.gameObject, 0.15f, () =>
            {
                // 领取按钮放大
                getBtn.transform.DOScale(0.8f, 0.3f).From();
                undoneBtn.transform.DOScale(0.9f, 0.3f).From();
                // 领取按钮淡入
                getBtn.DOFade(1, 0.3f);
                undoneBtn.DOFade(1, 0.3f);
                getBtnLoopAnim?.Resume();
                // 标签栏依次出现
                for (int i = 0; i < tabs.Count; i++)
                {
                    // 淡入
                    tabs[i].gameObject.DOFade(1, 0.3f).SetDelay(i * 0.07f);
                    // 下移
                    tabs[i].DoRelativeAnchorPosY(100, 0.3f).From().SetDelay(i * 0.07f);
                    // 缩放
                    tabs[i].DOScale(0.7f, 0.3f).From().SetDelay(i * 0.07f);
                }
                // 返回按钮下移
                closeBtn.rectTransform.DoRelativeAnchorPosY(-100, 0.3f).SetDelay(0.4f).OnStart(() =>
                {
                    closeBtn.SetAlpha(1);
                });
            });
        }

        public override void PlayExit(Action callback)
        {
            base.PlayExit(callback);
            paper1.gameObject.DOFade(0, 0.2f);
            paper2.gameObject.DOFade(0, 0.2f);
            closeBtn.DOFade(0, 0.2f);
            resourceTitle.DORelativePositionY(200, 0.2f).OnComplete(() =>
            {
                callback?.Invoke();
            });
        }
    }
}