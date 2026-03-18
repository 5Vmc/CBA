using UnityEngine;
using DG.Tweening;
using BigBang.UI;
using Utils;
using UnityTimer;

namespace BigBang.Animation
{
    public class CardAndDebrisItemAnim : AnimBase
    {
        [SerializeField] private RectTransform cardItemRect;
        [SerializeField] private RectTransform debrisItemRect;
        [SerializeField] private ParticleSystem particle1;
        [SerializeField] private ParticleSystem particle2;
        [SerializeField] private ParticleSystem particle3;

        public void InitChangeAnim()
        {
            // 初始化缩放
            cardItemRect.localScale = Vector3.one;
            debrisItemRect.localScale = Vector3.zero;
            // 初始化透明度
            cardItemRect.gameObject.SetAlpha(1);
            debrisItemRect.gameObject.SetAlpha(0);
        }

        // 播放卡牌转换成碎片动画
        [EditorButton("播放转换碎片动画")]
        public void PlayChangeAnim()
        {
            ClearAnim();
            InitChangeAnim();
            var cardAndDebrisItem = GetComponent<CardAndDebrisItem>();
            var cardItem = cardItemRect.GetComponent<CardItem>();

            // 翻回背面
            cardItem.Anim.PlayReverse(0.5f, () => { cardItem.peakImage.peakImage.SetAlpha(0); cardItem.ShowBack(); cardItem.Anim.HidColorImg(); }).OnComplete(() =>
            {
                // 闪白
                cardItem.BackToWhite();
                Timer.Register(this.gameObject, 0.5f, () =>
                {
                    // 播放爆炸粒子特效
                    PlayParticle();
                    // 浅色卡牌放大淡出
                    cardItem.Anim.PlayFadeBackground();
                    // 原卡牌淡出
                    cardItem.gameObject.DOFade(0, 0.3f);
                    // 碎片淡入
                    debrisItemRect.gameObject.DOFade(1, 0.3f);
                    // 碎片放大
                    debrisItemRect.DOScale(0.47f, 0.3f);
                });
            });
        }

        [EditorButton("播放粒子特效")]
        public void PlayParticle()
        {
            particle1.Play();
            particle2.Play();
            particle3.Play();
        }
    }
}