using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using TMPro;

namespace BigBang.Animation
{
    public class SkillResultUIAnim : AnimBase
    {
        [SerializeField] private RectTransform icon;
        [SerializeField] private Image playerImg;
        [SerializeField] private TMP_Text descText;
        [SerializeField] private RectTransform valueRect;

        public override void Init()
        {
            base.Init();
            // 初始化位置
            icon.SetAnchoredPositionY(26);
            icon.rotation = Quaternion.Euler(0, 0, 0);
            // 初始化透明度
            playerImg.SetAlpha(0);
            icon.gameObject.SetAlpha(0);
            descText.SetAlpha(0);
            valueRect.gameObject.SetAlpha(0);
        }

        [EditorButton("播放动画")]
        public override void PlayEnter()
        {
            base.PlayEnter();
            // 图标下移
            tweens.Add(icon.DoRelativeAnchorPosY(200, 0.3f).From().OnComplete(() =>
            {
                // 头像淡入
                tweens.Add(playerImg.DOFade(1, 0.3f));
                tweens.Add(descText.DOFade(1, 0.3f));
                tweens.Add(valueRect.gameObject.DOFade(1, 0.3f));

            }));
            // 图标淡入
            tweens.Add(icon.gameObject.DOFade(1, 0.3f));
            // 图标旋转
            tweens.Add(icon.DORotate(Vector3.up * 360 * 2, 0.3f, RotateMode.LocalAxisAdd));
        }
    }
}