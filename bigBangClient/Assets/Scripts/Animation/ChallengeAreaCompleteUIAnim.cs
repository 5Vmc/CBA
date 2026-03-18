using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;

namespace BigBang.Animation
{
    public class ChallengeAreaCompleteUIAnim : AnimBase
    {
        [SerializeField] private Image blackImg;
        [SerializeField] private Image backgroundImg;
        [SerializeField] private RectTransform goBtn;
        [SerializeField] private TMP_Text stageClearTxt;
        [SerializeField] private TMP_Text desTxt1;
        [SerializeField] private TMP_Text desTxt2;
        [SerializeField] private Image desImg;
        [SerializeField] private Image areaLeft;
        [SerializeField] private Image areaRight;
        [SerializeField] private Image areaLight;
        [SerializeField] private Image globalLeft;
        [SerializeField] private Image globalRight;
        [SerializeField] private Image globalBorder;
        [SerializeField] private Image globalLight;
        [SerializeField] private Image centerBorder;
        [SerializeField] private RectTransform starBox = null;

        private Sequence stageClearSequence;

        private void OnDisable()
        {
            stageClearSequence?.Kill();
        }

        public override void Init()
        {
            base.Init();
            // 初始化位置
            areaLeft.rectTransform.SetAnchoredPositionX(-700);
            areaRight.rectTransform.SetAnchoredPositionX(700);
            globalLeft.rectTransform.SetAnchoredPositionX(-700);
            globalRight.rectTransform.SetAnchoredPositionX(700);
            // 初始化缩放
            centerBorder.rectTransform.localScale = Vector3.zero;
            globalBorder.rectTransform.localScale = Vector3.zero;
            // 初始化透明度
            desTxt1.maxVisibleCharacters = 0;
            desTxt2.maxVisibleCharacters = 0;
            blackImg.SetAlpha(0);
            backgroundImg.SetAlpha(0);
            goBtn.gameObject.SetAlpha(0);
            stageClearTxt.SetAlpha(0);
            desImg.SetAlpha(0);
            globalLight.SetAlpha(0);
            areaLight.SetAlpha(0);
            starBox.gameObject.SetAlpha(0);
        }

        [EditorButton("播放动画")]
        public void PlayEnter(bool hasRewards)
        {
            base.PlayEnter();
            // 黑色背景淡入
            blackImg.DOFade(1, 0.3f);
            // 背景淡入
            backgroundImg.DOFade(1, 0.3f).OnComplete(() =>
            {
                // 中心方格放大
                centerBorder.rectTransform.DOScale(1, 0.3f);
                globalBorder.rectTransform.DOScale(1, 0.35f).OnComplete(() =>
                {
                    // 文字淡入
                    stageClearTxt.DOFade(1, 0.3f).OnComplete(PlayStageClearTxtAnim);
                    areaLight.DOFade(224 / 255f, 0.3f);
                    globalLight.DOFade(186 / 255f, 0.3f);
                });
                // 左右条飞入
                areaLeft.rectTransform.DOAnchorPosX(-218, 0.3f);
                areaRight.rectTransform.DOAnchorPosX(218, 0.3f);
                globalLeft.rectTransform.DOAnchorPosX(-218, 0.3f);
                globalRight.rectTransform.DOAnchorPosX(218, 0.3f).OnComplete(() =>
                {
                    desImg.DOFade(0.5f, 0.3f).OnComplete(() =>
                    {
                        desTxt1.DOText(0.3f).OnComplete(() =>
                        {
                            desTxt2.DOText(0.3f).OnComplete(() =>
                            {
                                if (hasRewards)
                                {
                                    starBox.gameObject.DOFade(1.0f, 0.3f).OnComplete(() =>
                                    {
                                        // 按钮淡入
                                        goBtn.gameObject.DOFade(1, 0.3f);
                                    });
                                }
                                else
                                {
                                    // 按钮淡入
                                    goBtn.gameObject.DOFade(1, 0.3f);
                                }
                            });
                        });
                    });
                });
            });
        }

        private void PlayStageClearTxtAnim()
        {
            stageClearSequence = DOTween.Sequence();
            stageClearSequence.Append(stageClearTxt.rectTransform.DOScale(1, 1f).SetEase(Ease.OutExpo));
            stageClearSequence.Append(stageClearTxt.rectTransform.DOScale(0.9f, 1f));
            stageClearSequence.SetLoops(-1);
        }
    }
}