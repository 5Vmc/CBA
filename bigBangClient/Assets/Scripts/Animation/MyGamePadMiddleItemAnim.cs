using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace BigBang.Animation
{
    public class MyGamePadMiddleItemAnim : AnimBase
    {
        [SerializeField] private Image leftProgress;
        [SerializeField] private Image rightProgress;
        [SerializeField] private TMP_Text leftText;
        [SerializeField] private TMP_Text rightText;

        private float leftProgressValue;
        private float rightProgressValue;
        private int leftValue;
        private int rightValue;

        public override void Init()
        {
            base.Init();
            leftValue = int.Parse(leftText.text);
            rightValue = int.Parse(rightText.text);
            leftProgressValue = leftProgress.fillAmount;
            rightProgressValue = rightProgress.fillAmount;

            leftText.text = "0";
            rightText.text = "0";
            leftProgress.fillAmount = 0;
            rightProgress.fillAmount = 0;
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            leftProgress.DOFillAmount(leftProgressValue, 0.3f);
            rightProgress.DOFillAmount(rightProgressValue, 0.3f);
            DOTween.To(value => leftText.text = ((int)value).ToString(), 0, leftValue, 0.3f);
            DOTween.To(value => rightText.text = ((int)value).ToString(), 0, rightValue, 0.3f);
        }
    }
}