using BigBang.UI;
using Coffee.UIEffects;
using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class SuperCardUIAnim : AnimBase
    {
        [SerializeField] private Image leftWing1;
        [SerializeField] private Image leftWing2;
        [SerializeField] private Image leftWing3;
        [SerializeField] private Image rightWing1;
        [SerializeField] private Image rightWing2;
        [SerializeField] private Image rightWing3;

        [SerializeField] private Image yellowLight;
        [SerializeField] private Image badge;
        [SerializeField] private Image backImg;
        [SerializeField] private CardItem cardItem;
        [SerializeField] private List<Sprite> badgeSequence;
        [SerializeField] private RectTransform cardRect;

        private UIEffect badgeEffect;
        private UIEffect backImgEffect;

        private void Awake()
        {
            badgeEffect = badge.GetComponent<UIEffect>();
            backImgEffect = backImg.GetComponent<UIEffect>();
        }

        public override void Init()
        {
            base.Init();
            badge.rectTransform.SetAnchoredPositionY(500);
            badge.SetAlpha(1);
            cardItem.gameObject.SetAlpha(0);
            backImg.SetAlpha(0);
            badge.sprite = badgeSequence[0];
            cardRect.localScale = Vector3.one;
            badgeEffect.colorFactor = 0;
            backImgEffect.colorFactor = 0;
            yellowLight.gameObject.SetAlpha(0);
        }

        [EditorButton("播放动画")]
        public override void PlayEnter()
        {
            base.PlayEnter();
            Task.Run(async () =>
            {

                badge.DOFade(1, 0.3f);
                // 徽章序列帧动画
                PlayBadgeAnim();
                // 徽章下落 
                await badge.rectTransform.DOAnchorPosY(0, 0.3f).AsyncWaitForCompletion();
                await backImg.DOFade(1, 0.3f).AsyncWaitForCompletion();
                cardItem.gameObject.SetAlpha(1);
                cardRect.DOScale(1.2f, 1f);
                await DOTween.To(value =>
                {
                    badgeEffect.colorFactor = value;
                    backImgEffect.colorFactor = value;
                }, 0, 0.5f, 1f).AsyncWaitForCompletion();
                PlayWingAnim();
                cardRect.DOScale(1, 0.3f);
                badge.DOFade(0, 0.3f);
                backImg.DOFade(0, 0.3f);
            });
        }

        // private void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Space))
        //     {
        //         PlayEnter();
        //     }
        // }

        private void PlayWingAnim()
        {
            AudioManager.Instance.PlaySound(AudioNames.ENT_STREN);

            yellowLight.gameObject.DOFade(1, 0.3f).OnComplete(
                ()=>{  yellowLight.gameObject.DOFade(0, 0.1f);  }
            );
            float time1 = 1.2f;
            float time2 = 2;
            float delay = 0.1f;
            DOTween.To(value => leftWing1.material.SetFloat("_Value", value), 0, 1.2f, time1);
            DOTween.To(value => leftWing2.material.SetFloat("_Value", value), 0, 1.2f, time2).SetDelay(delay);
            DOTween.To(value => leftWing3.material.SetFloat("_Value", value), 0, 1.2f, time1);

            DOTween.To(value => rightWing1.material.SetFloat("_Value", value), 0, 1.2f, time1);
            DOTween.To(value => rightWing2.material.SetFloat("_Value", value), 0, 1.2f, time2).SetDelay(delay);
            DOTween.To(value => rightWing3.material.SetFloat("_Value", value), 0, 1.2f, time1);
        }

        private async void PlayBadgeAnim()
        {
            foreach (var item in badgeSequence)
            {
                badge.sprite = item;
                await Task.Delay(30);
            }
        }
    }
}