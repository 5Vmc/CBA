using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using DG.Tweening;
using System;
using Coffee.UIEffects;
using BigBang.UI;
using UnityTimer;

namespace BigBang.Animation
{
    public class FBTowerHomeUIAnim : AnimBase
    {
        [SerializeField] private RectTransform topTitle;
        [SerializeField] private RectTransform bottomRect;

        public override void Init()
        {
            base.Init();
            // 初始化位置
            topTitle.SetAnchoredPositionY(0);
            bottomRect.SetAnchoredPositionY(73);
            // 初始化透明度
            topTitle.gameObject.SetAlpha(0);
            bottomRect.gameObject.DOFade(0, 0.3f);
        }

        public override void PlayEnter()
        {
            Init();
            base.PlayEnter();
            AudioManager.Instance.PlaySound(AudioNames.ENT_REG);
            // 顶部栏下移
            tweens.Add(topTitle.DoRelativeAnchorPosY(200, 0.3f).From());
            // 顶部栏淡入
            tweens.Add(topTitle.gameObject.DOFade(1, 0.3f));
            // 底部栏上移
            tweens.Add(bottomRect.DoRelativeAnchorPosY(-100, 0.3f).From());
            // 底部栏淡入
            tweens.Add(bottomRect.gameObject.DOFade(1, 0.3f).OnComplete(() =>
            {

            }));
            //PlayShowChapterAnim();
        }

        public override void PlayExit(Action callback)
        {
            topTitle.DORelativePositionY(200, 0.2f);
            bottomRect.gameObject.DOFade(0, 0.2f).OnComplete(() => callback?.Invoke());
        }


        [SerializeField] private List<FBTowerPathLineItem> lineItemList = new();
        [SerializeField] private List<FBTowerLevelItem> levelItemList = new();
        private List<Tween> tweenList = new();
        public int CurrentChapterId;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="showIndex"></param>
        public void PlayShowChapterAnim()
        {
            var showIndex = FBTowerController.Instance.GetAniDungeonIndex(CurrentChapterId);
            float delayTime = 0;

            for (int i = 0; i < tweenList.Count; i++)
            {
                tweenList[i].Kill();
            }
            tweenList.Clear();
            if (FBTowerController.Instance.OneChapterFinish) {
                PlayHideAll();
                FBTowerController.Instance.OneChapterFinish = false;
                return;
            }

            if (FBTowerController.Instance.FBData.currentLevelConfig.Chapter == CurrentChapterId)
            {
                if (!FBTowerController.Instance.OnlyEnableNewDuntionAni)
                {
                    PlayLessThanShowIndex(showIndex, delayTime);
                }
                else {
                    PlayShowIndex(showIndex);
                    FBTowerController.Instance.OnlyEnableNewDuntionAni = false;
                }
            }
            else {
                for (int i = 0; i < 10; i++)
                {
                    levelItemList[i].transform.localScale = Vector3.one;
                    if (i < lineItemList.Count)
                    {
                        lineItemList[i].gameObject.SetAlpha(1f);
                    }   
                }
            }
        }

        private void PlayHideAll()
        {
            for (int i = 0; i < 10; i++)
            {
                FBTowerLevelItem levelItem = levelItemList[i];
                levelItem.transform.localScale = Vector3.one;
                tweenList.Add(levelItem.transform.DOScale(0f, 0.3f).SetEase(Ease.OutBack).SetDelay(i * 0.1f + 0.1f));

                if (i < lineItemList.Count)
                {
                    FBTowerPathLineItem lineItem = lineItemList[i];
                    lineItem.gameObject.SetAlpha(1f);
                    tweenList.Add(lineItem.gameObject.DOFade(0f, 0.1f).SetEase(Ease.Linear).SetDelay(i * 0.1f));
                }
            }

            tweenList.Add(levelItemList[0].transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(1.5f));
            for (var index = 0; index < tweenList.Count; index++)
            {
                tweens.Add(tweenList[index]);
            }
        }


        /// <summary>
        /// 小于showIndex的才播放，其他隐藏
        /// </summary>
        /// <param name="showIndex"></param>
        private void PlayLessThanShowIndex(int showIndex, float delayTime = 0f) {
            for (int i = 0; i < 10; i++)
            {
                FBTowerLevelItem levelItem = levelItemList[i];
                levelItem.transform.localScale = Vector3.zero;
                //tweenList.Add(levelItem.transform.DOScale(0f, 0f));
                if (i <= showIndex)
                {
                    
                    tweenList.Add(levelItem.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(delayTime + i * 0.1f));
                }

                if (i < lineItemList.Count)
                {
                    FBTowerPathLineItem lineItem = lineItemList[i];
                    lineItem.gameObject.SetAlpha(0f);
                    //lineItem.gameObject.DOFade(0f, 0f);
                    if (i <= showIndex - 1)
                    {
                        tweenList.Add(lineItem.gameObject.DOFade(1, 0.3f).SetEase(Ease.Linear).SetDelay(delayTime + i * 0.1f + 0.2f));
                    }
                }
            }

            for (var index = 0; index < tweenList.Count; index++) {
                tweens.Add(tweenList[index]);
            }
        }

        /// <summary>
        /// 只播放showIndex的动画
        /// </summary>
        /// <param name="showIndex"></param>
        private void PlayShowIndex(int showIndex)
        {
            for (int i = 0; i < 10; i++)
            {
                FBTowerLevelItem levelItem = levelItemList[i];

                if (i < showIndex)
                {
                    levelItem.transform.localScale = Vector3.one;
                }
                else if (i == showIndex)
                {
                    tweenList.Add(levelItem.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(0.2f));
                }
                else {
                    levelItem.transform.localScale = Vector3.zero;
                }

                if (i < lineItemList.Count)
                {
                    FBTowerPathLineItem lineItem = lineItemList[i];
                    if (i < showIndex - 1)
                    {
                        lineItem.gameObject.SetAlpha(1f);
                    }
                    else if (i == showIndex - 1)
                    {
                        tweenList.Add(lineItem.gameObject.DOFade(1, 0.3f).SetEase(Ease.Linear).SetDelay(0.2f + 0.2f));
                    }
                    else {
                        lineItem.gameObject.SetAlpha(0f);
                    }
                }
            }
            tweens.AddRange(tweenList);
        }
    }
}
