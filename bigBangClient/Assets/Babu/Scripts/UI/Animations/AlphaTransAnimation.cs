using DG.Tweening;
using System;
using UnityEngine;

namespace Babu.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    internal class AlphaTransAnimation : TransAnimation
    {
        [SerializeField] bool _ignoreFirstEnable = true;

        [Header("Trans In")]
        [SerializeField] bool _enableInTrans = true;
        [SerializeField] float _transInStartAlpha = 0;
        [SerializeField] float _transInEndAlpha = 1;
        [SerializeField] float _transInTime = 0.25f;

        [Header("Trans Out")]
        [SerializeField] bool _enableOutTrans = true;
        [SerializeField] float _transOutStartAlpha = 1;
        [SerializeField] float _transOutEndAlpha = 0;
        [SerializeField] float _transOutTime = 0.25f;

        private bool _initialized = false;
        private CanvasGroup _canvasGroup;
        private Tween _transInTween;
        private Tween _transOutTween;

        private void Awake()
        {
            if (_ignoreFirstEnable == false)
            {
                _initialized = true;
            }

            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public override void In(Action completeCallback)
        {
            _transInTween?.Kill();
            _transOutTween?.Kill();

            if (_enableInTrans)
            {
                if (_initialized)
                {
                    _canvasGroup.alpha = _transInEndAlpha;
                    _initialized = true;
                }
                else
                {
                    _canvasGroup.alpha = _transInStartAlpha;
                    _transInTween = DOTween.To(value => _canvasGroup.alpha = value, _transInStartAlpha, _transInEndAlpha, _transInTime).OnComplete(() => completeCallback?.Invoke());
                }
            }
            else
            {
                completeCallback?.Invoke();
            }
        }

        public override void Out(Action completeCallback)
        {
            _transInTween?.Kill();
            _transOutTween?.Kill();

            if (_enableOutTrans)
            {
                if (_initialized)
                {
                    _canvasGroup.alpha = _transOutEndAlpha;
                    _initialized = true;
                }
                else
                {
                    _canvasGroup.alpha = _transOutStartAlpha;
                    _transInTween = DOTween.To(value => _canvasGroup.alpha = value, _transOutStartAlpha, _transOutEndAlpha, _transOutTime).OnComplete(() => completeCallback?.Invoke());
                }
            }
            else
            {
                completeCallback?.Invoke();
            }
        }

        private void OnDisable()
        {
            if (_ignoreFirstEnable)
            {
                _initialized = false;
            }
        }
    }
}
