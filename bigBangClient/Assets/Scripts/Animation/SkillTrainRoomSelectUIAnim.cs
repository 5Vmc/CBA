using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using System;
using TMPro;

namespace BigBang.Animation
{
    public class SkillTrainRoomSelectUIAnim : AnimBase
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private LoomAnim noneSkillImg;
        [SerializeField] private LoomAnim nonePlayImg;
        [SerializeField] private Image clearImg;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text playerTitle;
        [SerializeField] private TMP_Text skillTitle;
        [SerializeField] private GameObject cardPad;
        [SerializeField] private GameObject skillPad;
        [SerializeField] private RectTransform skillContent;
        [SerializeField] private RectTransform cardContent;
        [SerializeField] private TMP_Text descText;
        [SerializeField] private TMP_Text estimatedTime;
        [SerializeField] private Image DescTextBackground;

        private Image background;

        private void Awake()
        {
            background = GetComponent<Image>();
        }

        public void PlaySkillLoom()
        {
            noneSkillImg.PlayImage(0.5f, 0);
        }

        public void StopSkillLoom()
        {
            noneSkillImg.Stop();
            noneSkillImg.GetComponent<Image>().SetAlpha(1);

        }

        public void PlayPlayerLoom()
        {
            nonePlayImg.PlayImage(0.5f, 0);
        }

        public void StopPlayerLoom()
        {
            nonePlayImg.Stop();
            nonePlayImg.GetComponent<Image>().SetAlpha(1);
        }


        public override void Init()
        {
            base.Init();
            panel.gameObject.SetAlpha(1);
            panel.localScale = Vector3.one;
            panel.SetAnchoredPositionX(0);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            tweens.Add(panel.DoRelativeAnchorPosX(500, 0.15f).From());
            tweens.Add(background.DOFade(200 / 255f, 0.15f));
        }

        // 关闭特技选中窗口动画
        public override void PlayExit(Action callback)
        {
            // 缩小至80%并淡出
            panel.DOScale(0.8f, 0.3f).OnComplete(() =>
            {
                callback?.Invoke();
            });
            panel.gameObject.DOFade(0, 0.3f);
            background.DOFade(0, 0.3f);
        }

        public void PlaySelectCardAnim()
        {
            nameText.SetAlpha(0);
            // 球员姓名淡入
            nameText.DOFade(1, 0.3f);
        }

        public void PlaySelectSkillAnim()
        {
            if (!estimatedTime.gameObject.activeInHierarchy)
            {
                descText.SetAlpha(0);
                DescTextBackground.SetAlpha(0);
                estimatedTime.SetAlpha(0);

                descText.DOFade(1, 0.3f);
                DescTextBackground.DOFade(1, 0.3f);
                estimatedTime.DOFade(1, 0.3f);
            }
        }

        public void PlayShowCardPadAnim(bool flag)
        {
            if (flag)
            {
                TouchManager.Instance.DisableTouch();

                playerTitle.gameObject.SetActive(true);
                skillTitle.gameObject.SetActive(true);
                cardPad.SetActive(true);
                skillPad.SetActive(true);
                cardPad.SetAlpha(0);
                skillPad.SetAlpha(1);
                playerTitle.SetAlpha(0);
                skillTitle.SetAlpha(1);
                playerTitle.rectTransform.SetAnchoredPositionY(442);
                skillTitle.rectTransform.SetAnchoredPositionY(442);
                skillContent.SetAnchoredPositionX(0);
                cardContent.SetAnchoredPositionX(0);

                skillPad.DOFade(0, 0.15f).OnComplete(() =>
                {
                    cardContent.DoRelativeAnchorPosX(-500, 0.15f).From();
                    cardPad.DOFade(1, 0.15f);
                    skillPad.SetActive(false);
                });
                Sequence s = DOTween.Sequence();
                s.Append(skillTitle.DOFade(0, 0.15f));
                s.Append(playerTitle.DOFade(1, 0.15f));
                // 球员面板出现
                s.Join(playerTitle.rectTransform.DORelativePositionY(10, 0.15f).From());
                s.AppendCallback(() =>
                {
                    TouchManager.Instance.EnableTouch();
                    skillTitle.gameObject.SetActive(false);
                });
            }
            else
            {
                playerTitle.SetAlpha(1);
                skillTitle.SetAlpha(0);
                playerTitle.gameObject.SetActive(true);
                skillTitle.gameObject.SetActive(false);
                cardPad.SetActive(true);
                skillPad.SetActive(true);
                cardPad.SetAlpha(1);
                skillPad.SetAlpha(0);
                Babu.DelayTaskService.Instance.Run(this.gameObject, () => skillPad.SetActive(false));
            }
        }

        public void PlayShowSkillPadAnim(bool flag)
        {
            if (flag)
            {
                TouchManager.Instance.DisableTouch();

                playerTitle.gameObject.SetActive(true);
                skillTitle.gameObject.SetActive(true);
                cardPad.SetActive(true);
                skillPad.SetActive(true);
                playerTitle.SetAlpha(1);
                skillTitle.SetAlpha(0);
                playerTitle.rectTransform.SetAnchoredPositionY(442);
                skillTitle.rectTransform.SetAnchoredPositionY(442);
                skillContent.SetAnchoredPositionX(0);
                cardContent.SetAnchoredPositionX(0);
                cardPad.DOFade(0, 0.15f).OnComplete(() =>
                {
                    // 技能面板出现
                    skillContent.DoRelativeAnchorPosX(500, 0.15f).From();
                    skillPad.DOFade(1, 0.15f);
                    cardPad.SetActive(false);
                });
                Sequence s = DOTween.Sequence();
                s.Append(playerTitle.DOFade(0, 0.15f));
                s.Append(skillTitle.DOFade(1, 0.15f));
                // 标题下滑
                s.Join(skillTitle.rectTransform.DoRelativeAnchorPosY(10, 0.15f).From());
                s.AppendCallback(() =>
                {
                    TouchManager.Instance.EnableTouch();
                    playerTitle.gameObject.SetActive(false);
                });
            }
            else
            {
                playerTitle.SetAlpha(0);
                skillTitle.SetAlpha(1);
                playerTitle.gameObject.SetActive(false);
                skillTitle.gameObject.SetActive(true);
                cardPad.SetActive(true);
                skillPad.SetActive(true);
                cardPad.SetAlpha(0);
                skillPad.SetAlpha(1);
                Babu.DelayTaskService.Instance.Run(this.gameObject, () => cardPad.SetActive(false));
            }
        }
    }
}