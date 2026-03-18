using BigBang.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils;

namespace BigBang.Animation
{
    public class MonthSignUIAnim : AnimBase
    {
        [SerializeField] private RectTransform titleRect;
        [SerializeField] private List<ArrowItem> arrows;
        [SerializeField] private TMP_Text countTxt;
        [SerializeField] private MonthSignUIAdapter adapter;
        [SerializeField] private List<RectTransform> boxs;
        [SerializeField] private RectTransform rewardItem;
        [SerializeField] private TMP_Text signCountTxt;

        private int targetValue;

        public override void Init()
        {
            base.Init();
            titleRect.gameObject.SetAlpha(0);
            arrows.ForEach(item => item.Hide());
            int.TryParse(countTxt.text, out targetValue);
            rewardItem.gameObject.SetAlpha(1);
            boxs.ForEach(item => item.gameObject.SetAlpha(0));
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            AudioManager.Instance.PlaySound(AudioNames.ANI_TECHBOARDPOP);
            titleRect.gameObject.DOFade(1, 0.15f);
            for (int i = 0; i < arrows.Count; i++)
            {
                arrows[i].PlayAnim(0.3f, 0.3f, i * 0.15f);
            }
            // 宝箱动画
            for (int i = 0; i < boxs.Count; i++)
            {
                var scale = 0.8f;
                // 淡入
                boxs[i].gameObject.DOFade(1, 0.3f).SetDelay(i * 0.08f);
                // 放大
                boxs[i].DOScale(scale + 0.5f, 0.15f).SetDelay(i * 0.08f);
                // 缩小
                boxs[i].DOScale(scale, 0.15f).SetDelay(0.15f + i * 0.08f);
            }
            // 数字涨动动画
            DOTween.To(value => countTxt.text = ((int)value).ToString(), 0, targetValue, 0.3f).SetEase(Ease.Linear);
            // 列表动画 
            adapter.PlayEnter();
        }

        public void PlaySignCountTxt(string target)
        {
            float sourceY = 13;
            float moveY = 13;
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_MAX_4);
            signCountTxt.DOFade(0, 0.15f);
            signCountTxt.rectTransform.DOAnchorPosY(sourceY - 13, 0.15f).OnComplete(() =>
            {
                signCountTxt.text = target;
                signCountTxt.rectTransform.SetAnchoredPositionY(sourceY + moveY);
                signCountTxt.rectTransform.DOAnchorPosY(sourceY, 0.15f);
                signCountTxt.DOFade(1, 0.15f);
            });
        }

        public override void PlayExit(Action callback)
        {
            titleRect.gameObject.DOFade(0, 0.15f);
            rewardItem.gameObject.DOFade(0, 0.15f).OnComplete(() => callback?.Invoke());
        }
    }
}