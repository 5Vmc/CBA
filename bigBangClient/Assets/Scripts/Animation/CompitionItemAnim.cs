using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using TMPro;

namespace BigBang.Animation
{
    public class CompitionItemAnim : AnimBase
    {
        [SerializeField] private Image rewardImg;
        [SerializeField] private RectTransform rect;
        [SerializeField] private TMP_Text titleText;

        public override void Init()
        {
            base.Init();
            // 初始化缩放
            rect.localScale = Vector3.one * 1.5f;
            // 初始化透明度
            rewardImg.SetAlpha(0);
            rect.gameObject.SetAlpha(0);
        }

        public void PlayEnter(float delay)
        {
            base.PlayEnter();
            // 缩小
            tweens.Add(rect.DOScale(Vector3.one, 0.3f).SetDelay(delay).OnStart(() =>
            {
                AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP);
            }).OnComplete(() =>
            {
                // 奖励按钮原地淡入
                rewardImg.DOFade(1, 0.3f);
            }));
            // 淡入
            tweens.Add(rect.gameObject.DOFade(1, 0.3f).SetDelay(delay));
        }
    }
}