using DG.Tweening;
using UnityEngine;
using Utils;

namespace BigBang.Animation
{
    /// <summary>
    /// 虚影放大动效,仅对UI物体有效
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class IllusionAnim : MonoBehaviour
    {
        private Sequence sequence;

        private GameObject clone;

        /// <summary>
        /// 虚影效果
        /// </summary>
        /// <param name="endScale">目标缩放</param>
        /// <param name="endAlpha">目标透明度</param>
        /// <param name="duration">持续时间</param>
        public Sequence Play(float endScale, float endAlpha, float duration)
        {
            sequence?.Kill();
            sequence = DOTween.Sequence();
            //克隆
            clone = Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.parent);
            //设置图层
            clone.transform.SetSiblingIndex(Mathf.Max(0, gameObject.transform.GetSiblingIndex() - 1));
            sequence.Append(clone.DOFade(endAlpha, duration));
            sequence.Insert(0, clone.transform.DOScale(endScale, duration));
            //销毁物体
            Destroy(clone, duration + 0.5f);
            return sequence;
        }

        public void PlayLoop(float endScale, float endAlpha, float duration, float gap)
        {
            StopLoop();
            sequence = DOTween.Sequence();
            //克隆
            clone = Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.parent);
            //设置图层
            clone.transform.SetSiblingIndex(Mathf.Max(0, gameObject.transform.GetSiblingIndex() - 1));
            sequence.AppendCallback(() =>
            {
                clone.SetAlpha(1);
                clone.transform.localScale = transform.localScale;
            });
            sequence.Append(clone.DOFade(endAlpha, duration));
            sequence.Insert(0, clone.transform.DOScale(endScale, duration));
            sequence.AppendInterval(gap);
            sequence.SetLoops(-1);
        }

        public void StopLoop()
        {
            Destroy(clone);
            sequence?.Kill();
        }

        private void OnDestroy()
        {
            sequence?.Kill();
        }
    }
}