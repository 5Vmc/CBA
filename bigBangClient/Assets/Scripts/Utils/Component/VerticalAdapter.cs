using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    [ExecuteAlways]
    public class VerticalAdapter : MonoBehaviour
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
        [Header("是否变短")]
        public bool shorten = true;
        private List<float> targetPos = new List<float>();
        private float maxHeight = 0;
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

        public void Clear()
        {
            maxHeight = 0;
            targetPos.Clear();
        }

        /// <summary>
        /// 计算布局
        /// </summary>
        public void Calculate()
        {
            float sumHeight = 0;
            int activityCount = 0;
            for (int i = 0; i < SelfRect.childCount; i++)
            {
                if (i >= targetPos.Count) targetPos.Add(0);

                if (SelfRect.GetChild(i).gameObject.activeInHierarchy)
                {
                    activityCount++;
                    var childRect = SelfRect.GetChild(i) as RectTransform;
                    sumHeight += childRect.rect.height;

                    if (activityCount > 1) sumHeight += Gap;

                    if (targetPos[i] >= sumHeight - childRect.rect.height)
                    {
                        if (Animaction)
                        {
                            targetPos[i] = Mathf.Lerp(targetPos[i], sumHeight - childRect.rect.height, Speed);
                        }
                        else
                        {
                            targetPos[i] = sumHeight - childRect.rect.height;
                        }
                    }
                    else
                    {
                        targetPos[i] = sumHeight - childRect.rect.height;
                    }
                    childRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, targetPos[i], childRect.rect.height);
                }
            }
            maxHeight = Mathf.Max(maxHeight, sumHeight);
            if (shorten)
            {
                SelfRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sumHeight);
            }
            else
            {
                SelfRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxHeight);
            }
        }
    }
}