using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using BigBang.UI;
using Utils;
using System.Collections.Generic;

namespace BigBang.Animation
{
    public class HomeUIAnim : AnimBase
    {
        [SerializeField] private RectTransform top;
        [SerializeField] private List<HomeLineItemAnim> lineAnims;
        public override void Init()
        {
            base.Init();
            // 初始化位置
            top.SetAnchoredPositionY(UICommon.TopBarHideY);
        }

        public void PlayEnter(bool isNeedBoxAni)
        {
            base.PlayEnter();
            // 顶部栏下移
            tweens.Add(top.DOAnchorPosY(UICommon.TopBarShowY, 0.3f));
            if (isNeedBoxAni)
            {
                for (int i = 0; i < lineAnims.Count; i++)
                {
                    lineAnims[i].PlayEnter(i * 0.08f);
                }
            }
            else
            {
                for (int i = 0; i < lineAnims.Count; i++)
                {
                    lineAnims[i].ForceShow();
                }
            }
        }

        public override void PlayExit(Action callback)
        {
            // 顶部栏上移
            tweens.Add(top.DoRelativeAnchorPosY(UICommon.TopBarHideY, 0.3f).OnComplete(() =>
            {
                callback?.Invoke();
            }));
        }
    }
}