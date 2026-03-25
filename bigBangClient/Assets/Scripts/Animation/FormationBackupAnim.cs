using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Utils;

namespace BigBang.Animation
{
    public class FormationBackupAnim : AnimBase
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform backupContainer;
        [SerializeField] private RectTransform BottomTip;
        [SerializeField] private RectTransform Title;
        [SerializeField] private RectTransform LeftArrow;
        [SerializeField] private RectTransform RightArrow;
        [SerializeField] private RectTransform hideSoccerField;
        [SerializeField] private RectTransform hideMainContainer;
        [SerializeField] private RectTransform cardBg;
        [SerializeField] private List<RectTransform> confirmBtns;
        [SerializeField] private RectTransform limitPanel;

        //private bool animReady = true;
        private bool inited = false;

        public void InitAnim(bool isBounty)
        {
            if (inited) return;
            inited = true;
            if (!isBounty)
            {
                tweens.Add(backupContainer.gameObject.DOFade(0, 0));
            }
            else
            {
                tweens.Add(backupContainer.gameObject.DOFade(1, 0));
            }
            backupContainer.SetLocalPositionY(93.92735f - 100);
            tweens.Add(cardBg.gameObject.DOFade(0, 0));
            Title.SetLocalPositionX(-443f);
            LeftArrow.SetLocalPositionX(-309 - 120);
            RightArrow.SetLocalPositionX(309 + 120);
            BottomTip.gameObject.SetAlpha(0);
            BottomTip.SetLocalPositionY(-199.9274f - 50f);

            foreach (var confirmBtn in confirmBtns)
            {
                tweens.Add(confirmBtn.gameObject.DOFade(0, 0f));
                tweens.Add(confirmBtn.DOScale(0.5f, 0));
            }
        }

        public void PlayEnter(System.Action callback, bool isBounty)
        {
            //if (!animReady) return;
            //animReady = false;
            //limitPanel.gameObject.DOFade(0, 0);
            tweens.Add(cardBg.gameObject.DOFade(1, 0.3f));
            if (!isBounty)
            {
                tweens.Add(hideSoccerField.gameObject.DOFade(0, 0));
                tweens.Add(hideMainContainer.gameObject.DOFade(0, 0));
            }
            backupContainer.gameObject.DOFade(1, 0.3f);
            tweens.Add(backupContainer.DOLocalMoveY(93.92735f, 0.3f));

            tweens.Add(Title.DOLocalMoveX(-271, 0.2f).SetDelay(0.2f));
            tweens.Add(LeftArrow.DOLocalMoveX(-309, 0.2f).SetDelay(0.2f));
            tweens.Add(RightArrow.DOLocalMoveX(309, 0.2f).SetDelay(0.2f));
            tweens.Add(BottomTip.DOLocalMoveY(-199.9274f, 0.2f).SetDelay(0.2f));
            tweens.Add(BottomTip.gameObject.DOFade(1, 0.2f).SetDelay(0.2f).OnComplete(() =>
            {
                callback?.Invoke();
                //animReady = true;
            }));

            foreach (var confirmBtn in confirmBtns)
            {
                tweens.Add(confirmBtn.gameObject.DOFade(1, 0.3f).SetDelay(0.3f));
                tweens.Add(confirmBtn.DOScale(1, 0.3f).SetDelay(0.3f));
            }
        }

        public override void PlayExit(System.Action callback)
        {
            //if (!animReady) return;
            //animReady = false;
            tweens.Add(hideSoccerField.gameObject.DOFade(1, 0));
            tweens.Add(hideMainContainer.gameObject.DOFade(1, 0));
            //limitPanel.gameObject.DOFade(1, 0);
            cardBg.gameObject.DOFade(0, 0.3f);

            tweens.Add(backupContainer.gameObject.DOFade(0, 0.3f).OnComplete(() =>
            {
                callback?.Invoke();
                //animReady = true;
            }));
            tweens.Add(backupContainer.DOLocalMoveY(93.92735f - 100, 0.3f));
            tweens.Add(Title.DOLocalMoveX(-443, 0.2f));
            tweens.Add(LeftArrow.DOLocalMoveX(-309 - 120, 0.2f));
            tweens.Add(RightArrow.DOLocalMoveX(309 + 120, 0.2f));
            tweens.Add(BottomTip.gameObject.DOFade(0, 0.2f));
            tweens.Add(BottomTip.DOLocalMoveY(-199.9274f - 50, 0.2f));

            foreach (var confirmBtn in confirmBtns)
            {
                tweens.Add(confirmBtn.gameObject.DOFade(0, 0.3f).OnComplete(() =>
                {
                    confirmBtn.DOScale(0.5f, 0f);
                }));
                tweens.Add(confirmBtn.DOScale(1, 0.3f).SetDelay(0.3f));
            }
        }

        public void SetHideState()
        {
            ClearAnim();
            hideSoccerField.gameObject.SetAlpha(1);
            hideMainContainer.gameObject.SetAlpha(1);
            cardBg.gameObject.SetAlpha(0);
            backupContainer.gameObject.SetAlpha(0);
            backupContainer.SetLocalPositionY(93.92735f - 100);
            Title.SetLocalPositionX(-443);
            LeftArrow.SetLocalPositionX(-309 - 120);
            RightArrow.SetLocalPositionX(309 + 120);
            BottomTip.gameObject.SetAlpha(0);
            BottomTip.SetLocalPositionY(-199.9274f - 50);
            foreach (var confirmBtn in confirmBtns)
            {
                confirmBtn.gameObject.SetAlpha(0);
                confirmBtn.SetLocalScale(1);
            }
        }
    }
}
