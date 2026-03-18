using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    [ExecuteAlways]
    public class HorizontalAdapter : MonoBehaviour
    {
        [Header("是否每一帧都计算")]
        public bool CalculateEveryFrame = true;
        [Header("间隙")]
        public float Gap = 0;
        [Header("缓动动画")]
        public bool Animaction = true;
        [Header("缓动速度")]
        [Range(0, 1)]
        public float Speed = 0.1f;
        private List<float> targetPos = new List<float>();
        private RectTransform selfRect;
        private RectTransform SelfRect
        {
            get
            {
                if (selfRect == null)
                {
                    selfRect = GetComponent<RectTransform>();
                }
                return selfRect;
            }
        }

        private void Update()
        {
            if (CalculateEveryFrame)
            {
                Calculate();
            }
        }

        /// <summary>
        /// 计算布局
        /// </summary>
        public void Calculate()
        {
            float sumWidth = 0;
            int activityCount = 0;
            for (int i = 0; i < SelfRect.childCount; i++)
            {
                if (i >= targetPos.Count) targetPos.Add(0);

                if (SelfRect.GetChild(i).gameObject.activeInHierarchy)
                {
                    activityCount++;
                    var childRect = SelfRect.GetChild(i).GetComponent<RectTransform>();
                    sumWidth += childRect.rect.width * childRect.localScale.x;

                    if (activityCount > 1) sumWidth += Gap;

                    targetPos[i] = Mathf.Lerp(targetPos[i], sumWidth - childRect.rect.width, Animaction ? Speed : 1);
                    childRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, targetPos[i], childRect.rect.width);
                }
            }
            SelfRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sumWidth);
        }
    }
}