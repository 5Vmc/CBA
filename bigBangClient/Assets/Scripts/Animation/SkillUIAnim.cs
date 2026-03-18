using UnityEngine;
using DG.Tweening;
using Utils;
using System;

namespace BigBang.Animation
{
    public class SkillUIAnim : AnimBase
    {
        [SerializeField] private RectTransform top;
        [SerializeField] private RectTransform bottom;
        [SerializeField] private RectTransform selection;
        [SerializeField] private RectTransform infoBlock;
        [SerializeField] private RectTransform skillTrainRoomPad;
        [SerializeField] private CanvasGroup skillListGroup;

        public override void Init()
        {
            base.Init();
            // 初始化位置
            top.SetAnchoredPositionY(-60);
            selection.SetAnchoredPositionY(94);
            selection.gameObject.SetAlpha(0);
            bottom.SetAnchoredPositionY(73);
            // 初始化透明度
            top.gameObject.SetAlpha(0);
            infoBlock.gameObject.SetAlpha(0);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            // 顶部栏下移淡入
            tweens.Add(top.DoRelativeAnchorPosY(200, 0.3f).From());
            tweens.Add(top.gameObject.DOFade(1, 0.3f));
            // 底部栏上移
            tweens.Add(bottom.DoRelativeAnchorPosY(-200, 0.3f).From().OnComplete(() =>
            {
                tweens.Add(selection.DOAnchorPosY(194, 0.25f));
                // 选择栏快速淡入
                tweens.Add(selection.gameObject.DOFade(1, 0.3f));
                // 信息块快速淡入
                infoBlock.gameObject.DOFade(1, 0.3f);
            }));
        }

        public void PlayEnterII()
        {
            ClearAnim();
            skillTrainRoomPad.gameObject.SetAlpha(0);
            tweens.Add(skillTrainRoomPad.gameObject.DOFade(1, 0.3f));
        }

        public void PlayExit(Action callback)
        {
            tweens.Add(skillListGroup.DOFade(0, 0.1f).OnComplete(() =>
            {
                skillListGroup.alpha = 1;
                callback?.Invoke();
            }));
        }
    }
}