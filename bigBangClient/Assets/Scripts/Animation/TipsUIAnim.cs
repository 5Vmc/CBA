using System;
using System.Collections.Generic;
using BigBang.UI;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace BigBang.Animation
{
    public class TipsUIAnim : MonoBehaviour
    {
        [SerializeField] private RectTransform com;

        private List<Tween> tweens = new List<Tween>();

        private void Init()
        {
            tweens.ForEach(item => item?.Kill());
            tweens.Clear();
            //com.BackgroundImg.SetAlpha(0);
            //com.Content.SetAlpha(0);
            //com.BackgroundImg.rectTransform.SetAnchoredPositionY(0);

            com.gameObject.SetAlpha(0);
            com.SetAnchoredPositionY(240);
        }

        public void Play(Action callback)
        {
            Init();
            //淡入


            tweens.Add(com.gameObject.DOFade(1, 0.3f));
            //tweens.Add(com.Content.DOFade(1, 0.3f));
            //上移
            tweens.Add(com.DoRelativeAnchorPosY(-100, 0.2f).From().OnComplete(() =>
            {
                tweens.Add(com.DoRelativeAnchorPosY(100, 0.3f).SetDelay(2.5f));
                //淡出
                tweens.Add(com.gameObject.DOFade(0, 0.3f).SetDelay(1f).OnComplete(() => callback?.Invoke())); ;
                //tweens.Add(com.Content.DOFade(0, 0.3f).SetDelay(1f)
            }));
        }
    }
}
