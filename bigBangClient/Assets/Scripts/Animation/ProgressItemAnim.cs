using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityTimer;
using Utils;
using TMPro;
using BigBang.UI;
using System.Collections.Generic;
using Coffee.UIEffects;

namespace BigBang.Animation
{
    public class ProgressItemAnim : AnimBase
    {
        [SerializeField] private Image progressValue;

        [SerializeField] private List<UIShiny> shinys;

        public override void Init()
        {
            base.Init();
            progressValue.fillAmount = 0;
        }

        /// <summary>
        /// 按照增量设置进度条；point大于1就走多遍。不会出现进度倒退的现象。
        /// </summary>
        /// <param name="point">增量</param>
        public void PlayAnimIncrement(float point)
        {
            // 初始化动画
            point += progressValue.fillAmount;
            int times = (int)point;
            point = point % 1;
            Sequence seq = DOTween.Sequence();
            for (int i = 0; i < times; i++)
            {
                seq.Append(progressValue.DOFillAmount(point, 0.3f).SetDelay(0.1f));
            }
            seq.Append(progressValue.DOFillAmount(point, 0.3f).SetDelay(0.4f));
            tweens.Add(seq);
        }

        /// <summary>
        /// 按照全量设置进度条，point<=1
        /// </summary>
        /// <param name="point"></param>
        /// <param name="allowBack">是否允许倒退，例如目前是0.9，设置为0.4，则先0.1走到1，再从0走到0.4</param>
        public void PlayAnim(float point, bool allowBack = false)
        {
            // 初始化动画
            Sequence seq = DOTween.Sequence();
            if (!allowBack && (progressValue.fillAmount > point))
            {
                seq.Append(progressValue.DOFillAmount(1, 0.3f).SetDelay(0.1f));
            }
            seq.Append(progressValue.DOFillAmount(point, 0.3f).SetDelay(0.4f));
            tweens.Add(seq);
        }

        public override void PlayExit()
        {
            tweens.Add(gameObject.DOFade(0, 0.2f));
        }
    }
}