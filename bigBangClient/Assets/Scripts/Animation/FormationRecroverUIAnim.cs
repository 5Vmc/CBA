using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class FormationRecroverUIAnim : AnimBase
    {
        [SerializeField] private RectTransform topBar = null;
        [SerializeField] private RectTransform panel1 = null;
        [SerializeField] private RectTransform panel2 = null;
        [SerializeField] private RectTransform panel3 = null;

        public override void Init()
        {
            base.Init();
            // 初始化位置
            topBar.SetAnchoredPositionY(UICommon.TopBarHideY);
            panel1.gameObject.SetAlpha(0);
            panel2.gameObject.SetAlpha(0);
            panel3.gameObject.SetAlpha(0);
        }

        public void PlayEnter(Action action)
        {
            base.PlayEnter();

            tweens.Add(topBar.DOAnchorPosY(UICommon.TopBarShowY, 0.3f));
            tweens.Add(panel1.gameObject.DOFade(1f, 0.1f).SetDelay(0.1f));
            tweens.Add(panel2.gameObject.DOFade(1f, 0.1f).SetDelay(0.15f));
            tweens.Add(panel3.gameObject.DOFade(1f, 0.1f).SetDelay(0.2f).OnComplete(() => { action?.Invoke(); }));
        }
    }
}
