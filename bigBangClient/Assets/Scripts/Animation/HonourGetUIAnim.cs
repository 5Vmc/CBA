using Coffee.UIEffects;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class HonourGetUIAnim : AnimBase
    {
        [SerializeField] private Image DarkImage = null;
        public void PlayDark(bool isFirst)
        {
            if (isFirst) DarkImage.SetAlpha(0);
            tweens.Add(DarkImage.DOFade(0.75f, 0.3f));
        }

        [SerializeField] private RectTransform cupPanel = null;
        [SerializeField] private RectTransform cupImageRoot = null;
        [SerializeField] private ParticleSystem paperLeft = null;
        [SerializeField] private ParticleSystem paperRight = null;
        [SerializeField] private RectTransform namePanel = null;
        [SerializeField] private TMP_Text closeTipText = null;

        [SerializeField] private RectTransform getTopLightImage = null;
        [SerializeField] private RectTransform getBottomLightImage = null;
        [SerializeField] private RectTransform getTitleImage = null;
        [SerializeField] private UIShiny getTopLightImageUIShiny = null;
        [SerializeField] private UIShiny getBottomLightImageUIShiny = null;
        [SerializeField] private UIShiny getTitleImageUIShiny = null;

        [SerializeField] private RectTransform topRightLightImage = null;
        [SerializeField] private RectTransform topLeftLightImage = null;

        [SerializeField] private Image cupRightLightImage = null;
        [SerializeField] private Image cupLeftLightImage = null;
        [SerializeField] private RectTransform leftArrowPool = null;
        [SerializeField] private RectTransform rightArrowPool = null;
        private ComponentPool<RectTransform> leftArrowPoolComponent = new();
        private ComponentPool<RectTransform> rightArrowPoolComponent = new();

        private void Awake()
        {
            cupRightLightImage.gameObject.SetActive(false);
            cupLeftLightImage.gameObject.SetActive(false);
            leftArrowPoolComponent.InitComponentPool(cupLeftLightImage.gameObject, 3, leftArrowPool);
            rightArrowPoolComponent.InitComponentPool(cupRightLightImage.gameObject, 3, rightArrowPool);
        }
        public override void Init()
        {
            base.Init();

            cupPanel.SetLocalScaleX(0);
            cupImageRoot.gameObject.SetActive(false);
            namePanel.SetLocalScaleY(0);
            namePanel.gameObject.SetAlpha(0);
            paperLeft.Stop();
            paperLeft.Clear();
            paperRight.Stop();
            paperRight.Clear();
            closeTipText.gameObject.SetAlpha(0);

            getTitleImage.SetLocalScale(10);
            getTitleImage.gameObject.SetAlpha(0);
            getTopLightImage.SetAnchoredPositionX(637);//66.6
            getBottomLightImage.SetAnchoredPositionX(-629);//-53.7
            getTopLightImage.gameObject.SetAlpha(1);
            getBottomLightImage.gameObject.SetAlpha(1);
            getTopLightImageUIShiny.Stop();
            getTopLightImageUIShiny.effectFactor = 0;
            getBottomLightImageUIShiny.Stop();
            getBottomLightImageUIShiny.effectFactor = 0;
            getTitleImageUIShiny.Stop();
            getTitleImageUIShiny.effectFactor = 0;

            topRightLightImage.SetLocalRotationZ(-66);
            topLeftLightImage.SetLocalRotationZ(66);

            leftArrowPoolComponent.ClearOutComponent();
            rightArrowPoolComponent.ClearOutComponent();
        }

        [Range(0, 20)][SerializeField] private int cupFlipTimes = 5;
        [Range(0.01f, 0.3f)][SerializeField] private float cupFlipStartTime = 0.05f;
        [Range(0.01f, 0.2f)][SerializeField] private float cupFlipAddTime = 0.02f;
        [Range(0.3f, 1.0f)][SerializeField] private float cupFlipEndStepTime = 0.5f;
        public override void PlayEnter()
        {
            base.PlayEnter();

            TouchManager.Instance.DisableTouch();

            AudioManager.Instance.PlaySound(AudioNames.ANI_SCOUT10_SHOW_UP);

            Sequence cupSeq = DOTween.Sequence();
            tweens.Add(cupSeq);
            for (int i = 0; i < cupFlipTimes; i++)
            {
                cupSeq.AppendCallback(() => { cupImageRoot.gameObject.SetActive(true); });
                cupSeq.Append(cupPanel.DOScaleX(1, cupFlipStartTime + cupFlipAddTime * i));
                cupSeq.Append(cupPanel.DOScaleX(0, cupFlipStartTime + cupFlipAddTime * i));
                cupSeq.AppendCallback(() => { cupImageRoot.gameObject.SetActive(false); });
                cupSeq.Append(cupPanel.DOScaleX(1, cupFlipStartTime + cupFlipAddTime * i));
                cupSeq.Append(cupPanel.DOScaleX(0, cupFlipStartTime + cupFlipAddTime * i));
            }
            cupSeq.AppendCallback(() => { cupImageRoot.gameObject.SetActive(true); });
            cupSeq.AppendCallback(() => { paperLeft.Play(); });
            cupSeq.AppendCallback(() => { paperRight.Play(); });
            cupSeq.Append(cupPanel.DOScaleX(1, cupFlipEndStepTime));
            cupSeq.Join(namePanel.gameObject.DOFade(1, cupFlipEndStepTime));
            cupSeq.Join(namePanel.DOScaleY(1, cupFlipEndStepTime).SetEase(Ease.OutBack));
            cupSeq.AppendCallback(PlayArrowAnim);
            cupSeq.AppendInterval(1.0f);
            cupSeq.AppendCallback(() => { TouchManager.Instance.EnableTouch(); });
            cupSeq.Append(closeTipText.gameObject.DOFade(1, 0.3f));

            Sequence titleSeq = DOTween.Sequence();
            tweens.Add(titleSeq);
            titleSeq.Append(getTitleImage.DOScale(0.9f, 0.8f).SetEase(Ease.OutSine));
            titleSeq.Join(getTitleImage.gameObject.DOFade(1, 0.8f));
            titleSeq.Append(getTitleImage.DOScale(1.0f, 0.3f).SetEase(Ease.InSine));
            titleSeq.AppendCallback(() => { getTitleImageUIShiny.Play(); });
            titleSeq.AppendCallback(() => { getTopLightImageUIShiny.Play(); });
            titleSeq.AppendCallback(() => { getBottomLightImageUIShiny.Play(); });
            titleSeq.Append(getTopLightImage.DOAnchorPosX(66.6f, 0.3f));
            titleSeq.Join(getBottomLightImage.DOAnchorPosX(-53.7f, 0.3f));
            titleSeq.Append(getTopLightImage.DOAnchorPosX(66.6f - 100f, 1.3f));
            titleSeq.Join(getBottomLightImage.DOAnchorPosX(-53.7f + 100f, 1.3f));
            titleSeq.Append(getTopLightImage.DOAnchorPosX(66.6f - 100f - 10, 0.3f));
            titleSeq.Join(getBottomLightImage.DOAnchorPosX(-53.7f + 100f + 10, 0.3f));
            titleSeq.Join(getTopLightImage.gameObject.DOFade(0, 0.3f));
            titleSeq.Join(getBottomLightImage.gameObject.DOFade(0, 0.3f));

            tweens.Add(topRightLightImage.DOLocalRotate(new Vector3(0, 0, 23), 1.2f).SetDelay(0.3f));
            tweens.Add(topLeftLightImage.DOLocalRotate(new Vector3(0, 0, -23), 1.2f).SetDelay(0.3f).OnComplete(PlayLightMoveLoop));
        }

        private readonly float lightMoveTome = 3.5f;
        private void PlayLightMoveLoop()
        {
            Sequence rightLoopSeq = DOTween.Sequence();
            rightLoopSeq.Append(topRightLightImage.DOLocalRotate(new Vector3(0, 0, -10), lightMoveTome));
            rightLoopSeq.Append(topRightLightImage.DOLocalRotate(new Vector3(0, 0, 23), lightMoveTome));
            rightLoopSeq.SetLoops(-1);
            tweens.Add(rightLoopSeq);

            Sequence leftLoopSeq = DOTween.Sequence();
            leftLoopSeq.Append(topLeftLightImage.DOLocalRotate(new Vector3(0, 0, 10), lightMoveTome));
            leftLoopSeq.Append(topLeftLightImage.DOLocalRotate(new Vector3(0, 0, -23), lightMoveTome));
            leftLoopSeq.SetLoops(-1);
            tweens.Add(leftLoopSeq);
        }


        private void PlayArrowAnim()
        {
            Sequence arrowSeq = DOTween.Sequence();
            tweens.Add(arrowSeq);
            arrowSeq.AppendInterval(1f);
            arrowSeq.AppendCallback(SendArrowOnce);
            arrowSeq.SetLoops(-1);
        }
        private void SendArrowOnce()
        {
            RectTransform leftArrow = leftArrowPoolComponent.GetComponentFormPool();
            Sequence leftArrowSeq = DOTween.Sequence();
            leftArrow.SetAnchoredPositionX(-96);
            leftArrow.gameObject.SetAlpha(1);
            leftArrowSeq.Append(leftArrow.gameObject.DOFade(0, 3.0f).SetEase(Ease.InQuart));
            leftArrowSeq.Join(leftArrow.DOAnchorPosX(-296, 3.0f).SetEase(Ease.Linear));
            leftArrowSeq.AppendCallback(() => { leftArrowPoolComponent.ReturnComponentToPool(leftArrow); });

            RectTransform rightArrow = rightArrowPoolComponent.GetComponentFormPool();
            Sequence rightArrowSeq = DOTween.Sequence();
            rightArrow.SetAnchoredPositionX(96);
            rightArrow.gameObject.SetAlpha(1);
            rightArrowSeq.Append(rightArrow.gameObject.DOFade(0, 3.0f).SetEase(Ease.InQuart));
            rightArrowSeq.Join(rightArrow.DOAnchorPosX(296, 3.0f).SetEase(Ease.Linear));
            rightArrowSeq.AppendCallback(() => { rightArrowPoolComponent.ReturnComponentToPool(rightArrow); });
        }

    }
}