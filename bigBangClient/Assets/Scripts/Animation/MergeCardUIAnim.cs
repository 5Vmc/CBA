using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using BigBang.UI;

namespace BigBang.Animation
{
    public class MergeCardUIAnim : AnimBase
    {
        [SerializeField] private Image lightImg;
        [SerializeField] private Image borderImg;
        [SerializeField] private CardItem cardItem;

        public override void Init()
        {
            base.Init();
            // 初始化缩放
            cardItem.transform.localScale = Vector3.zero;
            // 初始化透明度
            lightImg.SetAlpha(0);
            borderImg.SetAlpha(0);
            cardItem.gameObject.SetAlpha(0.5f);
        }

        [EditorButton("播放动画")]
        public override void PlayEnter()
        {
            base.PlayEnter();
            // 金框淡入
            tweens.Add(borderImg.DOFade(1, 0.3f));
            // 金光淡入
            tweens.Add(lightImg.DOFade(1, 0.3f));
            // 卡片半透明变大
            tweens.Add(cardItem.transform.DOScale(2.3f, 0.3f));
            tweens.Add(cardItem.gameObject.DOFade(1, 0.3f));
            // 金光淡出
            tweens.Add(lightImg.DOFade(0, 0.3f).SetDelay(2));
        }
    }
}