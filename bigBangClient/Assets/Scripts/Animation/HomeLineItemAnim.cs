using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using TMPro;

namespace BigBang.Animation
{
    public class HomeLineItemAnim : AnimBase
    {
        [SerializeField] private RectTransform rect;

        public override void Init()
        {
            base.Init();
            // 初始化缩放
            rect.localScale = Vector3.one * 1.3f;
            rect.gameObject.SetAlpha(0);
        }

        public void PlayEnter(float delay)
        {
            base.PlayEnter();
            // 缩小
            tweens.Add(rect.DOScale(Vector3.one, 0.2f).SetDelay(delay).OnStart(() =>
            {
                AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP);
            }));
            // 淡入
            tweens.Add(rect.gameObject.DOFade(1, 0.3f).SetDelay(delay));
        } 

        public void ForceShow()
        {
            rect.localScale = Vector3.one;
            rect.gameObject.SetAlpha(1);
        }
    }
}