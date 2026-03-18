using UnityEngine;
using DG.Tweening;
using Utils;
using TMPro;
using Coffee.UIEffects;
using System.Collections.Generic;

namespace BigBang.Animation
{
    public class GamePreviewPadAnim : AnimBase
    {
        [SerializeField] private RectTransform changeTimeBtn;
        [SerializeField] private RectTransform formationBtn;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private UIShiny shiny1;
        [SerializeField] private UIShiny shiny2;
        [SerializeField] private List<GameResultIconAnim> leftResult;
        [SerializeField] private List<GameResultIconAnim> rightResult;
        [SerializeField] private List<MyGamePadMiddleItemAnim> middleItems;

        [SerializeField] private List<GameObject> fadeGroup;

        [SerializeField] private RectTransform left;
        [SerializeField] private RectTransform right;

        private void Awake()
        {
            //独立字体材质球
            //timeText.fontMaterial = Instantiate(timeText.fontMaterial);
            //timeText.fontMaterial.EnableKeyword("GLOW_ON");
            //timeText.fontMaterial.SetVector("_GlowColor", Color.white);
            //timeText.fontMaterial.SetFloat("_GlowOffset", 0.1f);
            //timeText.fontMaterial.SetFloat("_GlowInner", 0.05f);
            //timeText.fontMaterial.SetFloat("_GlowOuter", 0.75f);
            //timeText.fontMaterial.SetFloat("_GlowPower", 0.5f);
        }

        public override void Init()
        {
            base.Init();
            timeText.fontMaterial.SetFloat("_GlowOuter", 0.4f);
            // 初始化位置
            left.SetAnchoredPositionX(-200.5f);
            right.SetAnchoredPositionX(200.5f);
            // 初始化透明度
            changeTimeBtn.gameObject.SetAlpha(0);
            formationBtn.gameObject.SetAlpha(0);
            timeText.maxVisibleCharacters = 0;
            shiny1.effectFactor = 0;
            shiny2.effectFactor = 0;
            fadeGroup.ForEach(
                (item) => {
                    item.SetActive(false);
                    item.SetAlpha(0);
                }
            );
            leftResult.ForEach(item => item.Init());
            rightResult.ForEach(item => item.Init());
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            // 左右飞入
            left.DoRelativeAnchorPosX(-400f, 0.3f).From();
            right.DoRelativeAnchorPosX(400f, 0.3f).From().OnComplete(() =>
            {
                // 俱乐部牌翻转一次
                tweens.Add(shiny1.transform.DOScale(new Vector3(0, 0.6f, 0.6f), 0.15f).OnComplete(() =>
                {
                    shiny1.transform.DOScale(Vector3.one * 0.6f, 0.3f);
                }));
                // 扫光一次
                tweens.Add(DOTween.To(value => shiny1.effectFactor = value, 0, 1, 1f).SetDelay(0.25f));
                // 俱乐部牌翻转一次
                tweens.Add(shiny2.transform.DOScale(new Vector3(0, 0.6f, 0.6f), 0.15f).OnComplete(() =>
                {
                    tweens.Add(shiny2.transform.DOScale(Vector3.one * 0.6f, 0.3f));
                }));
                // 扫光一次
                tweens.Add(DOTween.To(value => shiny2.effectFactor = value, 0, 1, 1f).SetDelay(0.25f));
                // 修改阵形按钮原地淡入
                tweens.Add(formationBtn.gameObject.DOFade(1, 0.3f));
                // 打字机效果
                tweens.Add(timeText.DOText(0.3f).OnComplete(() =>
                {
                    // 调整时间按钮原地淡入
                    tweens.Add(changeTimeBtn.gameObject.DOFade(1, 0.3f));
                }));
                // 由发光变为不发光
                //tweens.Add(DOTween.To(value => timeText.fontMaterial.SetFloat("_GlowOuter", value), 0.75f, 0, 0.3f));

                // 赛果小圆圈原地翻转淡入
                for (int i = 0; i < leftResult.Count; ++i)
                {
                    leftResult[i].Play(i * 0.1f);
                }
                for (int i = 0; i < rightResult.Count; ++i)
                {
                    rightResult[i].Play(i * 0.1f);
                }
                // 进度条
                //middleItems.ForEach(item => item.PlayEnter());

                // 淡入
                fadeGroup.ForEach((item) => {
                    item.SetActive(true);
                    item.DOFade(1, 0.3f);
                });
            });
        }
    }
}