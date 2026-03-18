using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.Animation
{
    public static class BreathAnim
    {
        /// <summary>
        /// 呼吸效果，呼吸一次，循环呼吸需设置SetLoops
        /// </summary>
        /// <param name="minScale">最小缩放</param>
        /// <param name="maxScale">最大缩放</param>
        /// <param name="pauseTime">呼吸间隔</param>
        public static Sequence DOBreath(this GameObject obj, float minScale, float maxScale, float breathTime, float pauseTime)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.SetTarget(obj);
            obj.transform.localScale = Vector3.one * minScale;
            sequence.Append(obj.transform.DOScale(maxScale, breathTime));
            sequence.Append(obj.transform.DOScale(minScale, breathTime));
            sequence.AppendInterval(pauseTime);
            
            return sequence;
        }

        /// <summary>
        /// 呼吸效果，呼吸一次，循环呼吸需设置SetLoops
        /// </summary>
        /// <param name="minScale">最小缩放</param>
        /// <param name="maxScale">最大缩放</param>
        /// <param name="pauseTime">呼吸间隔</param>
        public static Sequence DOBreath(this Image img, float minScale, float maxScale, float breathTime, float pauseTime)
        {
            Sequence sequence = DOTween.Sequence();
            img.transform.localScale = Vector3.one * minScale;
            sequence.Append(img.transform.DOScale(maxScale, breathTime));
            sequence.Append(img.transform.DOScale(minScale, breathTime));
            sequence.AppendInterval(pauseTime);
            return sequence;
        }

        /// <summary>
        /// 呼吸效果，呼吸一次，循环呼吸需设置SetLoops
        /// </summary>
        /// <param name="minScale">最小缩放</param>
        /// <param name="maxScale">最大缩放</param>
        /// <param name="pauseTime">呼吸间隔</param>
        public static Sequence DOBreath(this TMP_Text txt, float minScale, float maxScale, float breathTime, float pauseTime)
        {
            Sequence sequence = DOTween.Sequence();
            txt.transform.localScale = Vector3.one * minScale;
            sequence.Append(txt.transform.DOScale(maxScale, breathTime));
            sequence.Append(txt.transform.DOScale(minScale, breathTime));
            sequence.AppendInterval(pauseTime);
            return sequence;
        }
    }
}