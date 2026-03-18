using System;
using System.Collections.Generic;
using BigBang.UI;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace BigBang.Animation
{
    public class StrengthenItemAnim : MonoBehaviour
    {
        [SerializeField] private StrengthenItemComponent com;

        private List<Tween> tweens = new List<Tween>();
        private RectTransform rectTransform;

        public void Init()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            gameObject.SetAlpha(0);
            rectTransform.SetAnchoredPositionX(1000);
        }

        public void Play(float delay)
        {
            Kill();
            Init();
            //设置透明度
            tweens.Add(DOTween.To(value => gameObject.SetAlpha(value), 0, 1, 0.3f).SetEase(Ease.InQuint).SetDelay(delay));
            //从侧边滑入
            tweens.Add(rectTransform.DOAnchorPosX(0, 0.3f).SetDelay(delay));
        }

        //播放点击动画
        public void PlayClick(float delay = 0, Action callback = null)
        {
            //按钮动画
            com.BtnAnim.Play(playAudio: false);
            //背景闪烁
            tweens.Add(DOTween.To(value => com.FlashImg.SetAlpha(value), 0, 0.2f, 0.1f).SetDelay(delay).OnComplete(() =>
            {
                tweens.Add(DOTween.To(value => com.FlashImg.SetAlpha(value), 0.2f, 0, 0.1f));
            }));
            //图片闪烁
            tweens.Add(DOTween.To(value => com.IconEffect.colorFactor = value, 0, 0.4f, 0.1f).SetDelay(delay).OnComplete(() =>
            {
                tweens.Add(DOTween.To(value => com.IconEffect.colorFactor = value, 0.4f, 0, 0.1f).OnComplete(() =>
                {
                    //点击之后变成灰色图片
                    com.CostText.color = new Color(187 / 255f, 48 / 255f, 49 / 255f, 1);
                    SpriteManager.GetSprite(AtlasNames.Public, SpriteNames.Public.BlackBtnImg, (s) =>
                    {
                        com.StrengthenBtn.image.sprite = s;
                    });
                    callback?.Invoke();
                }));
            }));
        }

        public void Kill()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }
    }
}