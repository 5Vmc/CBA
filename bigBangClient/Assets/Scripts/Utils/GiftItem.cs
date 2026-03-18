using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using BigBang.Animation;
using System;

namespace BigBang.UI
{
    public class GiftItem : MonoBehaviour
    {
        [SerializeField] private Image giftIcon;
        [SerializeField] private RectTransform discountImg;

        public async void SetData(int shopItemID)
        {
            giftIcon.sprite = await SpriteProxy.GetShopItem(shopItemID);
        }

        public void FadeInDiscount(Action callback = null)
        {
            // 折扣淡入
            discountImg.gameObject.DOFade(1, 0.1f);
            // 折扣缩小放大
            discountImg.DOScale(0.9f, 0.1f).OnComplete(() =>
            {
                discountImg.DOScale(1, 0.1f).OnComplete(() => callback?.Invoke());
            });
        }

        public void ResetDiscountAnim()
        {
            discountImg.gameObject.SetAlpha(1);
            discountImg.localScale = Vector3.one;
        }

        public void FadeOutDiscount(Action callvack = null)
        {
            discountImg.gameObject.DOFade(0, 0.1f).OnComplete(() => callvack?.Invoke());
        }

        public void ShopDiscount()
        {
            discountImg.gameObject.SetAlpha(1);
        }

        public void HidDiscount()
        {
            discountImg.gameObject.SetAlpha(0);
        }
    }
}
