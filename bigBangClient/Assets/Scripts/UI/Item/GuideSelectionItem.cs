using TMPro;
using UnityEngine;
using Utils;
using DG.Tweening;
using BigBang.Animation;
using Coffee.UIEffects;

namespace BigBang.UI
{
    public class GuideSelectionItem : MonoBehaviour
    {
        [SerializeField] private RectTransform itemRect;

        [SerializeField] private TMP_Text content;
        [SerializeField] private TMP_Text content2;

        [SerializeField] public BabuButton Btn;

        public void SetContent(string txt)
        {
            content.text = content2.text = txt;
        }

        // 播放进入动画
        public void PlayEnter(float delay)
        {
            // 初始化
            itemRect.SetAnchoredPositionY(-50f);
            itemRect.gameObject.SetAlpha(0);
            itemRect.localScale = Vector3.one;

            itemRect.DOAnchorPosY(0, 0.3f).SetDelay(delay);
            itemRect.gameObject.DOFade(1, 0.3f).SetDelay(delay);
        }

        public void PlaySelected()
        {
            var effect = itemRect.GetComponent<UIEffect>();
            DOTween.To(value => effect.colorFactor = PeriodicFunction.Trigonometric(value) * 0.5f, 0, 1, 0.3f).OnComplete(() =>
            {
                itemRect.gameObject.DOFade(0, 0.3f);
            });
        }

        public void PlayExit()
        {
            itemRect.DOScale(0.5f, 0.3f);
            itemRect.gameObject.DOFade(0, 0.3f);
        }
    }
}