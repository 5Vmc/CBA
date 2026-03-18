using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.Animation
{
    public class InviteMatchItemAnim : AnimBase
    {
        [SerializeField] private RectTransform borderImg;
        [SerializeField] private RectTransform panelImg;
        [SerializeField] private RectTransform cdContent;
        [SerializeField] private GameObject startBtn;
        [SerializeField] private Image icon;
        [SerializeField] private Image innerImg;
        [SerializeField] private Image outterImg;
        [SerializeField] private TMP_Text startBtnText;
        [SerializeField] private List<TMP_Text> txts;
        [SerializeField] private List<Image> fadeInGroup;
        [SerializeField] private GameObject placeObj;
        [SerializeField] private GameObject opponentsObj;

        public override void Init()
        {
            //设置位置
            cdContent.SetAnchoredPositionY(0);
            icon.rectTransform.SetAnchoredPositionY(45f);
            //设置缩放
            borderImg.sizeDelta = new Vector2(710, 50);
            panelImg.sizeDelta = new Vector2(710, 30);
            startBtn.transform.localScale = Vector3.zero;
            //设置透明度
            gameObject.SetAlpha(0);
            icon.SetAlpha(0);
            txts.ForEach(item => item.maxVisibleCharacters = 0);
            fadeInGroup.ForEach(item => item.SetAlpha(0));
            startBtn.SetAlpha(0);
            cdContent.gameObject.SetAlpha(0);
            opponentsObj.SetAlpha(0);
            placeObj.SetAlpha(0);
        }

        public void Play(float delay, bool isCDPad)
        {
            base.PlayEnter();
            Timer.Register(this.gameObject, delay, () =>
            {
                gameObject.SetAlpha(1);
                //开始按钮文本呼吸效果
                tweens.Add(startBtnText.DOBreath(1, 1.1f, 1, 0.3f).SetLoops(-1));
                //按钮文淡入
                tweens.Add(startBtn.DOFade(1, 0.3f));
                //内圈旋转
                tweens.Add(innerImg.rectTransform.DORotate(360 * Vector3.forward, 8, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1));
                //外圈旋转
                tweens.Add(outterImg.rectTransform.DORotate(-360 * Vector3.forward, 8, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1));
                tweens.Add(DOTween.To(value => panelImg.sizeDelta = new Vector2(710, value), 30, 324, 0.3f));
                //AudioManager.Instance.PlaySound(AudioNames.ANI_TECHBOARDPOP);
                //上下拉开
                tweens.Add(DOTween.To(value => borderImg.sizeDelta = new Vector2(710, value), 50, 324, 0.3f).OnComplete(() =>
                {
                    //初始化透明度
                    txts.ForEach(item => item.SetAlpha(1));
                    //图标淡入
                    tweens.Add(icon.DOFade(1, 0.3f));
                    //图标下移
                    tweens.Add(icon.rectTransform.DoRelativeAnchorPosY(50, 0.3f).From());
                    //文字类打字机效果
                    txts.ForEach(item => tweens.Add(item.DOText(item.text, 0.3f)));
                    //图片淡入
                    fadeInGroup.ForEach(item => tweens.Add(item.DOFade(1, 0.3f)));
                    //按钮放大
                    tweens.Add(startBtn.transform.DOScale(0.5f, 0.3f));
                    //按钮淡入
                    tweens.Add(startBtn.DOFade(1, 0.3f));
                    if (!isCDPad)
                        AudioManager.Instance.PlaySound(AudioNames.ANI_BBBTNPOP);
                    //CD向下
                    tweens.Add(cdContent.DOAnchorPosY(50, 0.3f).From());
                    //CD淡入
                    tweens.Add(cdContent.gameObject.DOFade(1, 0.3f));
                    tweens.Add(placeObj.DOFade(1, 0.3f));
                    tweens.Add(opponentsObj.DOFade(1, 0.3f));
                }));
            });
        }

        public void PlayClose(TweenCallback callback)
        {

            fadeInGroup.ForEach(item => tweens.Add(item.DOFade(0, 0.3f)));
            txts.ForEach(item => tweens.Add(item.DOFade(0, 0.3f)));
            tweens.Add(icon.DOFade(0, 0.3f));
            tweens.Add(placeObj.DOFade(0, 0.3f));
            tweens.Add(opponentsObj.DOFade(0, 0.3f));
            tweens.Add(cdContent.gameObject.DOFade(0, 0.3f).OnComplete(() =>
            {
                //板子关闭
                tweens.Add(DOTween.To(value => borderImg.sizeDelta = new Vector2(710, value), 324, 50, 0.3f).OnComplete(() =>
                {
                    //边框闪烁
                    tweens.Add(borderImg.GetComponent<Image>().DOFlash(3, 0.01f, 0.1f, 0.1f, 0f, 1f));
                    //底板闪烁
                    tweens.Add(panelImg.GetComponent<Image>().DOFlash(3, 0.01f, 0.1f, 0.1f, 0f, 1f).OnComplete(callback));
                }));
                tweens.Add(DOTween.To(value => panelImg.sizeDelta = new Vector2(710, value), 324, 30, 0.3f));
            }));
        }

        public void PlaySwitch()
        {
            fadeInGroup.ForEach(item => item.SetAlpha(0));
            txts.ForEach(item => item.SetAlpha(0));
            icon.SetAlpha(0);
            cdContent.gameObject.SetAlpha(0);
            startBtn.SetAlpha(0);
            //板子关闭
            tweens.Add(DOTween.To(value => borderImg.sizeDelta = new Vector2(710, value), 324, 50, 0.3f));
        }

        public void PlayAccept(TweenCallback callback)
        {
            tweens.Add(startBtnText.GetComponent<IllusionAnim>().Play(2, 0, 0.3f).OnComplete(callback));
            tweens.Add(startBtn.transform.DOScale(0, 0.3f));
            tweens.Add(startBtn.DOFade(0, 0.3f));
        }
    }
}