
using UnityEngine;
using DG.Tweening;
using Utils;
using BigBang.UI;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityTimer;
using TMPro;
using System;

namespace BigBang.Animation
{
    public class TacticsSetPadAnim : AnimBase
    {
        [SerializeField] private RectTransform defTitle;
        [SerializeField] private RectTransform atkTitle;
        [SerializeField] private RectTransform[] defItems;
        [SerializeField] private RectTransform[] atkItems;
        [SerializeField] private RectTransform titleLevelPanel; //help
        [SerializeField] private RectTransform useButton;

        [SerializeField] private RectTransform costPanel;
        [SerializeField] private RectTransform resourceTitle;

        //UpgradePanel
        [SerializeField] private RectTransform upgradePanelDetailTitleText;
        [SerializeField] private RectTransform upgradePanelNextLevelText;
        [SerializeField] private RectTransform upgradePanelArrow;
        [SerializeField] private RectTransform[] upgradePanelDetailItems;


        //MaxPanel
        [SerializeField] private RectTransform[] maxPanelDetailItems;
        [SerializeField] private RectTransform maxText; //"已满级"图片

        //MinPanel
        [SerializeField] private RectTransform minPanelDetailTitleText;
        [SerializeField] private RectTransform minPanelNextLevelText;

        [SerializeField] private RectTransform lockPanel;
        [SerializeField] private RectTransform minPanelArrow;
        [SerializeField] private RectTransform[] minPanelDetailItems;

        public override void Init()
        {
            base.Init();
            foreach (RectTransform tf in defItems)
            {
                tf.gameObject.SetAlpha(0);
                tf.localScale = Vector3.one;
            }

            atkTitle.gameObject.SetAlpha(0);
            foreach (RectTransform tf in atkItems)
            {
                tf.gameObject.SetAlpha(0);
                tf.localScale = Vector3.one;
            }
            atkTitle.SetAnchoredPositionX(-233f);
            defTitle.SetAnchoredPositionX(-233f);

            titleLevelPanel.gameObject.SetAlpha(0);
            useButton.gameObject.SetAlpha(0);
            costPanel.gameObject.SetAlpha(0);
            resourceTitle.SetAnchoredPositionY(-60);
            resourceTitle.gameObject.SetAlpha(0);

            //这个是UpgradePanel
            upgradePanelDetailTitleText.gameObject.SetAlpha(0);
            upgradePanelNextLevelText.gameObject.SetAlpha(0);
            upgradePanelArrow.gameObject.SetAlpha(0);
            foreach (RectTransform tf in upgradePanelDetailItems)
            {
                tf.gameObject.SetAlpha(0);
            }

            //maxpanel
            maxText.gameObject.SetAlpha(0);
            foreach (RectTransform tf in maxPanelDetailItems)
            {
                tf.gameObject.SetAlpha(0);
            }

            //mimpanel
            minPanelDetailTitleText.gameObject.SetAlpha(0);
            minPanelNextLevelText.gameObject.SetAlpha(0);
            minPanelArrow.gameObject.SetAlpha(0);
            lockPanel.gameObject.SetAlpha(0);
            foreach (RectTransform tf in minPanelDetailItems)
            {
                tf.gameObject.SetAlpha(0);
            }
        }
        public override void PlayEnter()
        {
            base.PlayEnter();

            // 顶部栏下移
            tweens.Add(resourceTitle.DoRelativeAnchorPosY(200, 0.3f).From());
            // 顶部栏淡入
            tweens.Add(resourceTitle.gameObject.DOFade(1, 0.3f));

            //防守阵型动画
            tweens.Add(defTitle.DoRelativeAnchorPosX(-200, 0.5f).From());
            int index = 1;
            float delayTime = 0.08f;
            foreach (RectTransform tf in defItems)
            {
                tweens.Add(tf.gameObject.DOFade(1, 0.08f).SetDelay(index * delayTime));
                tweens.Add(tf.DOScale(0.7f, 0.1f).From().SetDelay(index * delayTime));
                index++;
            }

            //进攻阵型动画
            atkTitle.gameObject.SetAlpha(1);
            tweens.Add(atkTitle.DoRelativeAnchorPosX(-200, 0.5f).From());
            index = 1;
            delayTime = 0.08f;
            foreach (RectTransform tf in atkItems)
            {
                tweens.Add(tf.gameObject.DOFade(1, 0.08f).SetDelay(index * delayTime));
                tweens.Add(tf.DOScale(0.7f, 0.1f).From().SetDelay(index * delayTime));
                index++;
            }

            //这个是UpgradePanel
            Timer.Register(this.gameObject, 0.6f, () =>
            {
                tweens.Add(upgradePanelDetailTitleText.gameObject.DOFade(1, 0.1f));
                tweens.Add(upgradePanelNextLevelText.gameObject.DOFade(1, 0.1f).SetDelay(0.1f));
                index = 1;
                delayTime = 0.05f;
                foreach (RectTransform tf in upgradePanelDetailItems)
                {
                    tweens.Add(tf.gameObject.DOFade(1, 0.08f).SetDelay(index * delayTime));
                    tweens.Add(tf.DOScale(0.8f, 0.1f).From().SetDelay(index * delayTime));
                    index++;
                }

                tweens.Add(upgradePanelArrow.gameObject.DOFade(1, 0.1f).SetDelay(0.4f));
            });

            //这个是MaxPanel anim
            Timer.Register(this.gameObject, 0.6f, () =>
            {
                index = 1;
                delayTime = 0.07f;
                foreach (RectTransform tf in maxPanelDetailItems)
                {
                    tweens.Add(tf.gameObject.DOFade(1, 0.08f).SetDelay(index * delayTime));
                    tweens.Add(tf.DOScale(0.8f, 0.1f).From().SetDelay(index * delayTime));
                    index++;
                }
                tweens.Add(maxText.gameObject.DOFade(1, 0.1f).SetDelay(0.5f));
            });

            //这个是MinPanel Anim
            Timer.Register(this.gameObject, 0.6f, () =>
            {
                tweens.Add(minPanelDetailTitleText.gameObject.DOFade(1, 0.1f));
                tweens.Add(minPanelNextLevelText.gameObject.DOFade(1, 0.1f).SetDelay(0.1f));

                tweens.Add(lockPanel.gameObject.DOFade(1, 0.1f).SetDelay(0.2f));
                index = 1;
                delayTime = 0.08f;
                foreach (RectTransform tf in minPanelDetailItems)
                {
                    tweens.Add(tf.gameObject.DOFade(1, 0.08f).SetDelay(index * delayTime));
                    tweens.Add(tf.DOScale(0.8f, 0.1f).From().SetDelay(index * delayTime));
                    index++;
                }

                tweens.Add(minPanelArrow.gameObject.DOFade(1, 0.1f).SetDelay(0.5f));
            });

            //接下去help，使用按钮，消耗panel
            Timer.Register(this.gameObject, 0.4f, () =>
            {
                tweens.Add(titleLevelPanel.gameObject.DOFade(1, 0.3f));
                tweens.Add(useButton.gameObject.DOFade(1, 0.3f).SetDelay(0.3f));
                tweens.Add(costPanel.gameObject.DOFade(1, 0.3f).SetDelay(0.7f));
            });
        }

        /// <summary>
        /// 播放退出动画
        /// </summary>
        public override void PlayExit()
        {
            base.PlayExit();
            tweens.Add(resourceTitle.DORelativePositionY(200, 0.2f));
        }

        public override void PlayExit(Action callback)
        {
            base.PlayExit();
            tweens.Add(resourceTitle.DORelativePositionY(200, 0.2f));
        }

    }
}