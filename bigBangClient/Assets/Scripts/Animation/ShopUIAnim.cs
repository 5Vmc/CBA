using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using TMPro;
using System;
using System.Collections.Generic;
using BigBang.UI;

namespace BigBang.Animation
{
    public class ShopUIAnim : AnimBase
    {
        [SerializeField] private Image backgroundImg;
        [SerializeField] private RectTransform resourceTitle;
        [SerializeField] private Image bottom;
        [SerializeField] private TMP_Text giftTxt;
        [SerializeField] private Image giftIcon;
        [SerializeField] private Image discountImg;
        [SerializeField] private Button costBtn;
        [SerializeField] private TMP_Text limitTxt;
        [SerializeField] private List<ShopDiamondItem> diamondItems;
        [SerializeField] private List<ShopTrainItem> trainItems;
        [SerializeField] private RectTransform item1Btn;
        [SerializeField] private RectTransform item2Btn;

        [SerializeField] private ClassicShopItemAdapter adapter;
        public override void Init()
        {
            base.Init();
            // 初始化位置
            resourceTitle.SetAnchoredPositionY(0);
            bottom.rectTransform.SetAnchoredPositionY(220);
            // 初始化透明度
            backgroundImg.SetAlpha(0.1f);
            resourceTitle.gameObject.SetAlpha(0);
            bottom.gameObject.SetAlpha(1);
        }

        //public void PlayDiamondPadAnim()
        //{
        //    backgroundImg.SetAlpha(0.1f);
        //    AudioManager.Instance.PlaySound(AudioNames.ENT_PLAYER);
        //    diamondItems.ForEach(item =>
        //    {
        //        item.gameObject.SetAlpha(0);
        //        item.transform.localScale = Vector3.one * 0.6f;
        //    });
        //    // 背景底板10%淡入
        //    tweens.Add(backgroundImg.DOFade(1, 0.3f));
        //    for (int i = 0; i < diamondItems.Count; i++)
        //    {
        //        tweens.Add(diamondItems[i].transform.DOScale(1, 0.1f).SetDelay(i * 0.05f));
        //        tweens.Add(diamondItems[i].gameObject.DOFade(1, 0.3f).SetDelay(i * 0.05f));
        //        diamondItems[i].PlayPointAnim(i * 0.05f + 1);
        //    }
        //}

        //public void PlayTrainPadAnim()
        //{
        //    backgroundImg.SetAlpha(0.1f);
        //    AudioManager.Instance.PlaySound(AudioNames.ENT_PLAYER);
        //    trainItems.ForEach(item =>
        //    {
        //        item.gameObject.SetAlpha(0);
        //        item.transform.localScale = Vector3.one * 0.6f;
        //    });
        //    // 背景底板10%淡入
        //    tweens.Add(backgroundImg.DOFade(1, 0.3f));
        //    for (int i = 0; i < trainItems.Count; i++)
        //    {
        //        tweens.Add(trainItems[i].transform.DOScale(1, 0.1f).SetDelay(i * 0.05f));
        //        tweens.Add(trainItems[i].gameObject.DOFade(1, 0.3f).SetDelay(i * 0.05f));
        //        trainItems[i].PlayPointAnim(i * 0.05f + 1);
        //    }
        //}

        //public void PlayArenaPadAnim()
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.ENT_PLAYER);
        //    adapter.PlayAnim();
        //}

        public override void PlayEnter()
        {
            base.PlayEnter();
            AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);
            // 背景底板10%淡入
            tweens.Add(backgroundImg.DOFade(1, 0.3f));
            // 导航栏上浮出现
            tweens.Add(bottom.rectTransform.DORelativePositionY(-200, 0.3f).From());
            // 经济栏淡入
            tweens.Add(resourceTitle.gameObject.DOFade(1, 0.3f));
            // 经济栏下滑
            tweens.Add(resourceTitle.DORelativePositionY(200, 0.3f).From());
        }

        public override void PlayExit(Action callback)
        {
            base.PlayExit();
            // 导航栏淡出
            tweens.Add(bottom.gameObject.DOFade(0, 0.3f));
            // 经济狼上滑
            tweens.Add(resourceTitle.DORelativePositionY(200, 0.3f));
            // 经济狼淡出
            tweens.Add(resourceTitle.gameObject.DOFade(0, 0.3f).OnComplete(() =>
            {
                callback?.Invoke();
            }));
        }
    }
}