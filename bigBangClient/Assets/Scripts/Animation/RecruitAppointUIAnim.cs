using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;

namespace BigBang.Animation
{
    public class RecruitAppointUIAnim : AnimBase
    {
        [SerializeField] private Image backgroundImg;
        [SerializeField] private Image blackImg;

        public override void Init()
        {
            base.Init();
            // 初始化缩放
            backgroundImg.rectTransform.localScale = Vector3.one * 0.8f;
            // 初始化透明度
            blackImg.SetAlpha(0);
            backgroundImg.gameObject.SetAlpha(0);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            // 禁用触摸
            TouchManager.Instance.DisableTouch();
            // 背景淡入
            blackImg.DOFade(1f, 0.3f).SetDelay(0.4f).OnComplete(() =>
            {
                tweens.Add(backgroundImg.rectTransform.DOScale(1, 0.3f));
                tweens.Add(backgroundImg.gameObject.DOFade(1, 0.3f).OnComplete(() =>
                {
                    // 启用触摸
                    TouchManager.Instance.EnableTouch();
                }));
            });
        }
    }
}