using BigBang.Animation;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.Animation
{
    public static class FlashAnim
    {
        /// <summary>
        /// 闪烁动画
        /// </summary>
        /// <param name="img">图片</param>
        /// <param name="flashCount">闪烁次数</param>
        /// <param name="fadeInTime">淡入时间</param>
        /// <param name="fadeOutTime">淡出时间</param>
        /// <param name="pauseTime">暂停时间</param>
        /// <param name="minAlpha">最小透明度</param>
        /// <param name="maxAlpha">最大透明度</param>
        public static Sequence DOFlash(this Image img, int flashCount, float fadeInTime, float fadeOutTime, float pauseTime = 0, float minAlpha = 0, float maxAlpha = 1)
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < flashCount; i++)
            {
                sequence.Append(img.DOFade(minAlpha, fadeOutTime));
                sequence.AppendInterval(pauseTime);
                sequence.Append(img.DOFade(maxAlpha, fadeInTime));
            }
            return sequence;
        }

        public static Sequence DOFlash2(this Image img, int flashCount, float fadeInTime, float fadeOutTime, float pauseTime = 0, float minAlpha = 0, float maxAlpha = 1)
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < flashCount; i++)
            {
                sequence.Append(img.DOFade(minAlpha, fadeOutTime));
                sequence.AppendInterval(pauseTime);
                sequence.Append(img.DOFade(maxAlpha, fadeInTime));
                sequence.AppendInterval(pauseTime);
            }
            return sequence;
        }

        /// <summary>
        /// 闪烁动画
        /// </summary>
        /// <param name="txt">文本</param>
        /// <param name="flashCount">闪烁次数</param>
        /// <param name="fadeInTime">淡入时间</param>
        /// <param name="fadeOutTime">淡出时间</param>
        /// <param name="pauseTime">暂停时间</param>
        /// <param name="minAlpha">最小透明度</param>
        /// <param name="maxAlpha">最大透明度</param>
        public static Sequence DOFlash(this TMP_Text txt, int flashCount, float fadeInTime, float fadeOutTime, float pauseTime = 0, float minAlpha = 0, float maxAlpha = 1)
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < flashCount; i++)
            {
                sequence.Append(txt.DOFade(minAlpha, fadeOutTime));
                sequence.AppendInterval(pauseTime);
                sequence.Append(txt.DOFade(maxAlpha, fadeInTime));
            }
            return sequence;
        }

        public static Sequence DOFlash2(this TMP_Text txt, int flashCount, float fadeInTime, float fadeOutTime, float pauseTime = 0, float minAlpha = 0, float maxAlpha = 1)
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < flashCount; i++)
            {
                sequence.Append(txt.DOFade(minAlpha, fadeOutTime));
                sequence.AppendInterval(pauseTime);
                sequence.Append(txt.DOFade(maxAlpha, fadeInTime));
                sequence.AppendInterval(pauseTime);
            }
            return sequence;
        }

        /// <summary>
        /// 闪烁动画
        /// </summary>
        /// <param name="obj">UI物体</param>
        /// <param name="flashCount">闪烁次数</param>
        /// <param name="fadeInTime">淡入时间</param>
        /// <param name="fadeOutTime">淡出时间</param>
        /// <param name="pauseTime">暂停时间</param>
        /// <param name="minAlpha">最小透明度</param>
        /// <param name="maxAlpha">最大透明度</param>
        public static Sequence DOFlash(this GameObject obj, int flashCount, float fadeInTime, float fadeOutTime, float pauseTime = 0, float minAlpha = 0, float maxAlpha = 1)
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < flashCount; i++)
            {
                sequence.Append(obj.DOFade(minAlpha, fadeOutTime));
                sequence.AppendInterval(pauseTime);
                sequence.Append(obj.DOFade(maxAlpha, fadeInTime));
            }
            return sequence;
        }
    }
}