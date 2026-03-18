using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class AchievementPadAnim : AnimBase
    {
        [SerializeField] private RectTransform team;
        [SerializeField] private RectTransform player;
        [SerializeField] private RectTransform develop;
        [SerializeField] private TMP_Text addTxt;
        [SerializeField] private TMP_Text title;
        [SerializeField] private Image progressValue1;
        [SerializeField] private Image progressValue2;
        [SerializeField] private GameObject teamSelection;
        [SerializeField] private GameObject playerSelection;
        [SerializeField] private GameObject developSelection;
        [SerializeField] private RectTransform pointTitle;
        [SerializeField] private RectTransform toggleGroup = null;

        public override void Init()
        {
            base.Init();
            team.gameObject.SetAlpha(0);
            player.gameObject.SetAlpha(0);
            develop.gameObject.SetAlpha(0);
            team.SetAnchoredPositionY(-100);
            player.SetAnchoredPositionY(-100);
            develop.SetAnchoredPositionY(-100);
            title.SetAlpha(0);
            addTxt.SetAlpha(0);
            addTxt.rectTransform.localScale = Vector3.one * 1.5f;
            toggleGroup.SetAnchoredPositionY(60);
            toggleGroup.gameObject.SetAlpha(0f);
        }

        public void PlayEnter(Action callback)
        {
            Init();
            tweens.Add(addTxt.DOFade(1, 0.3f));
            tweens.Add(addTxt.rectTransform.DOScale(1, 0.3f));

            tweens.Add(title.DOFade(1, 0.3f).OnComplete(() =>
            {
                tweens.Add(team.gameObject.DOFade(1, 0.3f));
                tweens.Add(player.gameObject.DOFade(1, 0.3f));
                tweens.Add(develop.gameObject.DOFade(1, 0.3f));
                tweens.Add(player.DOAnchorPosY(0, 0.3f));
                tweens.Add(team.DOAnchorPosY(0, 0.3f));
                tweens.Add(develop.DOAnchorPosY(0, 0.3f).OnComplete(() => callback?.Invoke()));
            }));

            tweens.Add(toggleGroup.DOAnchorPosY(195, 0.3f));
            tweens.Add(toggleGroup.gameObject.DOFade(1f, 0.3f));
        }

        public void PlayTeamAnim(Action callback)
        {
            tweens.Add(teamSelection.DOFade(1, 0.15f).OnComplete(() =>
            {
                callback?.Invoke();
                teamSelection.SetAlpha(0);
                PlayListPadAnim();
            }));
        }

        public void PlayPlayerAnim(Action callback)
        {
            tweens.Add(playerSelection.DOFade(1, 0.15f).OnComplete(() =>
            {
                callback?.Invoke();
                playerSelection.SetAlpha(0);
                PlayListPadAnim();
            }));
        }

        public void PlayDevelopAnim(Action callback)
        {
            tweens.Add(developSelection.DOFade(1, 0.15f).OnComplete(() =>
            {
                callback?.Invoke();
                developSelection.SetAlpha(0);
                PlayListPadAnim();
            }));
        }

        public void PlayListPadAnim()
        {
            pointTitle.SetAnchoredPositionY(60);
            tweens.Add(pointTitle.DOAnchorPosY(-60, 0.3f));
        }

        public override void PlayExit(Action callback)
        {
            base.PlayExit(callback);
        }
    }
}