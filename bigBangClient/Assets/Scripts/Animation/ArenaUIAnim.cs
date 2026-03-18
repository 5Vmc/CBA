
using UnityEngine;
using DG.Tweening;
using Utils;
using BigBang.UI;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityTimer;
using TMPro;

namespace BigBang.Animation
{
    public class ArenaUIAnim : AnimBase
    {
        [SerializeField] private RectTransform bottomRect;

        public override void Init()
        {
            base.Init();
            bottomRect.gameObject.SetAlpha(0);
        }

        void OnEnable()
        {
            this.PlayEnter();
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);
            // 顶部栏下移

            if (bottomRect)
            {
                // 底部栏上移
                tweens.Add(bottomRect.DoRelativeAnchorPosY(-100, 0.3f).From());
                // 底部栏淡入
                tweens.Add(bottomRect.gameObject.DOFade(1, 0.3f).OnComplete(() =>
                {

                }));
            }

        }
    }
}