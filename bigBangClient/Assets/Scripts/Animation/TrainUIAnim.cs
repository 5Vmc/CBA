using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Utils;
using System;
using BigBang.UI;

namespace BigBang.Animation
{
    public class TrainUIAnim : AnimBase
    {
        [SerializeField] private TrainUIComponent com;
        [SerializeField] private InviteMatchItemAnim inviteAnim1;
        [SerializeField] private InviteMatchItemAnim inviteAnim2;
        [SerializeField] private BigBangInfoPadAnim bigAnim;

        [SerializeField] private CanvasGroup speedBtnCanvasGroup;

        public override void Init()
        {
            base.Init();
            com.TopItem.SetAnchoredPositionY(UICommon.TopBarHideY);
            com.BottomItem.SetAnchoredPositionY(94);
            com.SpeedBtn.gameObject.SetAlpha(0);
            (com.SpeedBtn.transform as RectTransform).SetAnchoredPositionY(262);
            speedBtnCanvasGroup.blocksRaycasts = false;
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            //顶部栏下移
            tweens.Add(com.TopItem.DOAnchorPosY(UICommon.TopBarShowY, 0.3f));
            //底部栏上移
            tweens.Add(com.BottomItem.DOAnchorPosY(194, 0.25f));
        }

        public void ShowSpeedBtn()
        {
            ClearSpeedShowAnim();
            tweens.Add(tweenSpeedShow1 = com.SpeedBtn.gameObject.DOFade(1, 0.25f).SetDelay(0.3f));
            tweens.Add(tweenSpeedShow2 = (com.SpeedBtn.transform as RectTransform).DOAnchorPosY(342, 0.25f).SetDelay(0.3f));
            speedBtnCanvasGroup.blocksRaycasts = true;
        }

        public void HidSpeedBtn()
        {
            ClearSpeedShowAnim();
            tweens.Add(tweenSpeedShow1 = com.SpeedBtn.gameObject.DOFade(0, 0.25f));
            tweens.Add(tweenSpeedShow2 = (com.SpeedBtn.transform as RectTransform).DOAnchorPosY(262, 0.25f));
            speedBtnCanvasGroup.blocksRaycasts = false;
        }
        private Tween tweenSpeedShow1 = null;
        private Tween tweenSpeedShow2 = null;
        private void ClearSpeedShowAnim()
        {
            tweenSpeedShow1?.Kill();
            tweenSpeedShow2?.Kill();
            tweenSpeedShow1 = null;
            tweenSpeedShow2 = null;
        }

        //播放倍率按钮切换动画
        public void PlaySpeedBtnSwitch(float targetAngle)
        {
            com.SpeedBtn.image.color = new Color(200 / 255f, 200 / 255f, 200 / 255f, 1);
            com.SpeedTransform.DORotate(Vector3.forward * targetAngle, 0.3f, RotateMode.Fast).OnComplete(() =>
            {
                com.SpeedBtn.image.color = Color.white;
            });
        }

        public void SwitchOut(Action callback = null)
        {
            inviteAnim1?.PlaySwitch();
            inviteAnim2?.PlaySwitch();
            bigAnim?.PlaySwitch();
            //顶部栏上移
            tweens.Add(com.TopItem.DOAnchorPosY(UICommon.TopBarHideY, 0.3f));
            //底部栏下移
            tweens.Add(com.BottomItem.DoRelativeAnchorPosY(-100, 0.3f).OnComplete(() =>
            {
                callback?.Invoke();
            }));
        }
    }
}