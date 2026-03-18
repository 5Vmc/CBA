using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    /// <summary>
    /// 若隐若现动画
    /// </summary>
    public class LoomAnim : MonoBehaviour
    {
        private Sequence sequence;

        /// <summary>
        /// 若隐若现动画
        /// </summary>
        /// <param name="time">淡入淡出1次的时长</param>
        /// <param name="waitTime">等待时长</param>
        /// <param name="minAlpha">最小透明度</param>
        /// <param name="maxAlpha">嘴大透明度</param>
        public void Play(float time, float waitTime, float minAlpha = 0.5f, float maxAlpha = 1f)
        {
            sequence?.Kill();
            sequence = DOTween.Sequence();
            sequence.Append(gameObject.DOFade(minAlpha, time));
            sequence.Append(gameObject.DOFade(maxAlpha, time));
            sequence.AppendInterval(waitTime);
            sequence.SetLoops(-1);
        }

        /// <summary>
        /// 若隐若现动画
        /// </summary>
        /// <param name="time">淡入淡出1次的时长</param>
        /// <param name="waitTime">等待时长</param>
        /// <param name="minAlpha">最小透明度</param>
        /// <param name="maxAlpha">嘴大透明度</param>
        public void PlayImage(float time, float waitTime, float minAlpha = 0.5f, float maxAlpha = 1f)
        {
            sequence?.Kill();
            sequence = DOTween.Sequence();
            var img = GetComponent<Image>();
            sequence.AppendCallback(() => img.SetAlpha(maxAlpha));
            sequence.Append(img.DOFade(minAlpha, time));
            sequence.Append(img.DOFade(maxAlpha, time));
            sequence.AppendInterval(waitTime);
            sequence.SetLoops(-1);
        }

        /// <summary>
        /// 若隐若现动画
        /// </summary>
        /// <param name="time">淡入淡出1次的时长</param>
        /// <param name="waitTime">等待时长</param>
        /// <param name="minAlpha">最小透明度</param>
        /// <param name="maxAlpha">嘴大透明度</param>
        public void PlayText(float time, float waitTime, float minAlpha = 0.5f, float maxAlpha = 1f)
        {
            sequence?.Kill();
            sequence = DOTween.Sequence();
            var text = GetComponent<TMP_Text>();
            sequence.Append(text.DOFade(minAlpha, time));
            sequence.Append(text.DOFade(maxAlpha, time));
            sequence.AppendInterval(waitTime);
            sequence.SetLoops(-1);
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void Stop()
        {
            sequence?.Kill();
        }
    }

    public static class LoomAnimExtension
    {
        /// <summary>
        /// 若隐若现动画
        /// </summary>
        /// <param name="time">淡入淡出1次的时长</param>
        /// <param name="waitTime">等待时长</param>
        /// <param name="minAlpha">最小透明度</param>
        /// <param name="maxAlpha">嘴大透明度</param>
        public static Sequence DOLoom(this GameObject obj, float time, float waitTime, float minAlpha = 0.5f, float maxAlpha = 1f)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(obj.DOFade(minAlpha, time));
            sequence.Append(obj.DOFade(maxAlpha, time));
            sequence.AppendInterval(waitTime);
            sequence.SetLoops(-1);
            return sequence;
        }

        /// <summary>
        /// 若隐若现动画
        /// </summary>
        /// <param name="time">淡入淡出1次的时长</param>
        /// <param name="waitTime">等待时长</param>
        /// <param name="minAlpha">最小透明度</param>
        /// <param name="maxAlpha">嘴大透明度</param>
        public static Sequence DOLoom(this Image img, float time, float waitTime, float minAlpha = 0.5f, float maxAlpha = 1f)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(img.DOFade(minAlpha, time));
            sequence.Append(img.DOFade(maxAlpha, time));
            sequence.AppendInterval(waitTime);
            sequence.SetLoops(-1);
            return sequence;
        }

        /// <summary>
        /// 若隐若现动画
        /// </summary>
        /// <param name="time">淡入淡出1次的时长</param>
        /// <param name="waitTime">等待时长</param>
        /// <param name="minAlpha">最小透明度</param>
        /// <param name="maxAlpha">嘴大透明度</param>
        public static Sequence DOLoom(this TMP_Text text, float time, float waitTime, float minAlpha = 0.5f, float maxAlpha = 1f)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(text.DOFade(minAlpha, time));
            sequence.Append(text.DOFade(maxAlpha, time));
            sequence.AppendInterval(waitTime);
            sequence.SetLoops(-1);
            return sequence;
        }
    }
}
