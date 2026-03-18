using DG.Tweening;
using Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.Animation
{
    public class FloatAnim : MonoBehaviour
    {
        private float scope = 7f;
        private float time = 3f;
        private RectTransform rect;
        private Image img;
        //初始位置
        private Vector2 sourcePosition;
        private Tween floatTween;
        private Tween fadeTween;
        private Tween sizeTween;

        private Sequence sequence;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            img = GetComponent<Image>();
            sourcePosition = rect.anchoredPosition;
        }
        private void Start()
        {
            Play();
        }

        private void Play()
        {
            //位移变化
            floatTween = rect.DOAnchorPos(sourcePosition + new Vector2(Random.Range(-scope, scope), Random.Range(-scope, scope)), Random.Range(time - 0.5f, time + 0.5f))
             .SetEase(Ease.InOutQuad).OnComplete(Play).AddTo(this.gameObject);
            //透明度变化
            fadeTween = DOTween.To(value => img.SetAlpha(value), img.color.a, Random.Range(0.2f, 1), 2f).AddTo(this.gameObject);
            //大小变化
            sizeTween = DOTween.To(value => rect.localScale = Vector3.one * value, rect.localScale.x, Random.Range(0.8f, 1), 2f).AddTo(this.gameObject);
        }

        private void OnDestroy()
        {
            StopPlay();
        }

        public void StopPlay()
        {
            floatTween?.Kill();
            fadeTween?.Kill();
            floatTween = null;
            fadeTween = null;
            rect.anchoredPosition = sourcePosition;
            img.SetAlpha(1);
            rect.localScale = Vector3.one;
        }
        public void StartPlay()
        {
            if (floatTween == null)
            {
                Play();
            }
        }

    }
}
