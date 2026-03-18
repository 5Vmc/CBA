using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace BigBang.Animation
{
    public static class AnchorPosAnim 
    {
        /// <summary>
        /// 相对位移
        /// </summary>
        /// <param name="vector">方向向量</param>
        /// <param name="duration">持续时间</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DoRelativeAnchorPos(this RectTransform rect, Vector2 vector, float duration, bool snapping = false)
        {
            return rect.DOAnchorPos(rect.anchoredPosition + vector, duration, snapping);
        }

        /// <summary>
        /// 相对位移
        /// </summary>
        /// <param name="displacement">距离</param>
        /// <param name="duration">持续时间</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DoRelativeAnchorPosX(this RectTransform rect, float displacement, float duration, bool snapping = false)
        {
            return rect.DOAnchorPosX(rect.anchoredPosition.x + displacement, duration, snapping);
        }

        /// <summary>
        /// 相对位移
        /// </summary>
        /// <param name="displacement">距离</param>
        /// <param name="duration">持续时间</param>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DoRelativeAnchorPosY(this RectTransform rect, float displacement, float duration, bool snapping = false)
        {
            return rect.DOAnchorPosY(rect.anchoredPosition.y + displacement, duration, snapping);
        }
    }
}