using DG.Tweening;
using System;
using UnityEngine;
using Utils;

namespace BigBang.Animation
{
    public class CardUIAnim : AnimBase
    {
        [SerializeField] private RectTransform top;
        [SerializeField] private RectTransform bottom;
        [SerializeField] private CanvasGroup osa;

        public override void Init()
        {
            base.Init();
            // 初始化位置
            top.SetAnchoredPositionY(UICommon.TopBarHideY);
            bottom.SetAnchoredPositionY(94);
            osa.alpha = 1;
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            // 顶部栏下移
            tweens.Add(top.DOAnchorPosY(UICommon.TopBarShowY, 0.3f));
            // 底部栏上移
            tweens.Add(bottom.DOAnchorPosY(194, 0.25f));
        }

        public void PlayNext(Action callback)
        {

            // 顶部栏上移
            tweens.Add(top.DOAnchorPosY(UICommon.TopBarHideY, 0.2f));
            // 底部栏下移
            tweens.Add(bottom.DoRelativeAnchorPosY(-400, 0.3f));
            tweens.Add(osa.DOFade(0, 0.3f).OnComplete(() =>
            {
                callback?.Invoke();
            }));
        }
    }
}
