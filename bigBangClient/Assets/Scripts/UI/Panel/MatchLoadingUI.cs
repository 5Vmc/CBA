using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System;
using DG.Tweening;
using UnityTimer;
using Utils;

namespace BigBang.UI
{
    public class MatchLoadingUIProperties : WindowProperties
    {
        public Action Callback;

        public MatchLoadingUIProperties(Action callback)
        {
            Callback = callback;
        }
    }

    public class MatchLoadingUI : AWindowController<MatchLoadingUIProperties>
    {
        [SerializeField] private Image ball;

        private float toMinimizeTime = 0.6f;
        private float toMaximizeTime = 0.6f;

        protected override void AddListeners()
        {
            base.AddListeners();
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            ball.transform.localScale = Vector3.one * 55f;
            TouchManager.Instance.DisableTouch();
            // 最小化
            ToMinimize(() =>
            {
                Properties.Callback?.Invoke();
                Timer.Register(this.gameObject, 1, () =>
                {
                    // 最大化
                    ToMaximize(() =>
                    {
                        TouchManager.Instance.EnableTouch();
                        UIController.Instance.CloseWindow<MatchLoadingUI>();
                    });
                });
            });
        }

        public void ToMinimize(Action callback)
        {
            AudioManager.Instance.PlaySound(AudioNames.LOADING_IN);
            ball.rectTransform.DOScale(0, toMinimizeTime).SetEase(Ease.OutExpo).OnComplete(() => callback?.Invoke());
        }

        public void ToMaximize(Action callback)
        {
            AudioManager.Instance.PlaySound(AudioNames.LOADING_OUT);
            ball.rectTransform.DOScale(55, toMaximizeTime).SetEase(Ease.InExpo).OnComplete(() => callback?.Invoke());
        }

        [EditorButton("最小化")]
        private void ToMinimize()
        {
            ToMinimize(null);
        }

        [EditorButton("最大化")]
        private void ToMaximize()
        {
            ToMaximize(null);
        }
    }
}