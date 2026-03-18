using UnityEngine;
using UnityEngine.UI;
using Utils;
using BigBang.Animation;
using DG.Tweening;

namespace BigBang.UI
{
    public class StarItem : MonoBehaviour
    {
        [SerializeField] private Image backgroundImg;
        [SerializeField] private Image starImg;
        [SerializeField] private Image lightImg;

        [SerializeField] private Sprite normalStar;
        [SerializeField] private Sprite colorfulStar;

        private Sequence sequence;

        private bool state;

        public async void SetStar(int quality)
        {
            backgroundImg.sprite = await SpriteProxy.GetCardQualitySprite(SpriteNames.Card.BackStar, quality);
        }

        public void HideBackground()
        {
            backgroundImg.gameObject.SetActive(false);
        }

        public void SetState(bool flag)
        {
            state = flag;
            starImg.SetAlpha(state ? 1 : 0);
        }

        public void SetStarAsNormal()
        {
            starImg.sprite = normalStar;
        }

        public void SetStarAsColorful()
        {
            starImg.sprite = colorfulStar;
        }

        public bool GetState()
        {
            return state;
        }

        // 播放星星砸入动画
        public void PlayStar()
        {
            state = true;
            // 停止当前星星的虚影效果
            StopFlash();
            sequence?.Kill();
            sequence = DOTween.Sequence();
            lightImg.SetAlpha(0);
            starImg.rectTransform.localScale = Vector3.one * 2f;
            // 星星淡入
            sequence.Append(starImg.DOFade(1, 0.3f));
            // 星星抖动
            sequence.Insert(0, starImg.rectTransform.DOSpin(10, 5, 0.05f));
            // 星星砸入
            sequence.Append(starImg.rectTransform.DOScale(0.66f, 0.1f).SetEase(Ease.InQuint));
            sequence.AppendCallback(() =>
            {
                lightImg.SetAlpha(1);
                // 砸下去的虚影
                lightImg.GetComponent<IllusionAnim>().Play(2f, 0, 1f);
                // 放到下一帧执行
                Babu.DelayTaskService.Instance.Run(this.gameObject, () =>
                {
                    lightImg.SetAlpha(0);
                });
            });
        }

        // 播放虚影动画
        public void PlayFlash()
        {
            lightImg.SetAlpha(1);
            lightImg.GetComponent<IllusionAnim>().PlayLoop(2f, 0, 1f, 0.5f);
        }

        // 暂停虚影动画
        public void StopFlash()
        {
            lightImg.GetComponent<IllusionAnim>().StopLoop();
        }

        public void HidLightStar()
        {
            lightImg.SetAlpha(0);
        }
    }
}