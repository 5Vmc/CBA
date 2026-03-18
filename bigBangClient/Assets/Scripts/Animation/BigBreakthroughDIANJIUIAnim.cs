using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using BigBang.UI;
using System.Collections.Generic;
using Utils;

namespace BigBang.Animation
{
    public class BigBreakthroughDIANJIUIAnim : MonoBehaviour
    {
        [SerializeField] private BigBreakthroughDIANJIUIComponent com;

        private List<Tween> tweens = new List<Tween>();

        private void Init()
        {
            com.BlackImg.SetAlpha(0);
            com.Boxing.transform.localScale = Vector3.zero;
            com.PlayerAddTypeModule.SetAlpha(0);
            com.TrainItemAddTypeModule.SetAlpha(0);
            com.PlayerAddTypeModule.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            com.TrainItemAddTypeModule.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            com.LevelBtn1.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            com.LevelBtn2.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            com.LevelBtn3.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            com.LevelBtn4.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            com.LevelBtn1.SetAlpha(0);
            com.LevelBtn2.SetAlpha(0);
            com.LevelBtn3.SetAlpha(0);
            com.LevelBtn4.SetAlpha(0);
            com.levelToggleTab.ForEach(item => item.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f));
            //com.ArrowGroup.ForEach(item => item.SetAlpha(0));
            for(int i = 0; i < com.ArrowGroup.Count; i++)
            {
                com.ArrowGroup[i].SetAlpha(0);
            }
            //com.ArrowImage1.SetAlpha(0);
            //com.ArrowImage2.SetAlpha(0);
            //com.ArrowImage3.SetAlpha(0);
            com.Background.SetAlpha(0);
            com.BreakLevelText.SetAlpha(0);
            com.DescText.SetAlpha(0);
            com.TrainItemText.SetAlpha(0);
            com.BreakItemNameText.SetAlpha(0);
            com.PropertsCountsText.ForEach(item => item.SetAlpha(0));
            com.PropertsNameText.SetAlpha(0);
            com.ItemAddNameText.SetAlpha(0);
            com.ItemTrainCountsText.ForEach(item => item.SetAlpha(0));
            com.Boxing.gameObject.SetAlpha(0);
            com.CloseText.gameObject.SetAlpha(0);
        }

        public void Play(Action callback)
        {
            Kill();
            Init();
            //黑幕淡入
            tweens.Add(com.Background.DOFade(1,0.3f));
            //大icon放大进入
            tweens.Add(com.Boxing.DOScale(1, 0.3f));
            tweens.Add(com.BlackImg.DOFade(0.9f, 0.2f));
            //标题和正文说明右侧飞入
            tweens.Add(com.DescText.GetComponent<RectTransform>().DoRelativeAnchorPosX(100, 0.3f).From().SetDelay(0.2f));
            tweens.Add(DOTween.To(value => com.DescText.SetAlpha(value), 0, 1, 0.3f).SetDelay(0.3f));
            tweens.Add(com.BreakLevelText.GetComponent<RectTransform>().DoRelativeAnchorPosX(300, 0.3f).From().SetDelay(0));
            //收益块放大淡入
            tweens.Add(com.PlayerAddTypeModule.transform.DOScale(1, 0.2f).SetDelay(0.3f));
            tweens.Add(com.PlayerAddTypeModule.DOFade(1, 0.2f).SetDelay(0.3f));
            tweens.Add(com.PropertsNameText.DOFade(1, 0.2f).SetDelay(0.3f));
            for (int i = 0; i < com.PropertsCountsText.Count; i++)
            {
                tweens.Add(com.PropertsCountsText[i].DOFade(1, 0.2f).SetDelay(i*0.3f));
            }
            tweens.Add(com.TrainItemAddTypeModule.transform.DOScale(1, 0.2f).SetDelay(0.3f));
            tweens.Add(com.TrainItemAddTypeModule.DOFade(1, 0.2f).SetDelay(0.3f));
            tweens.Add(com.ItemAddNameText.DOFade(1, 0.2f).SetDelay(0.3f).OnComplete(() =>
            {
               //等级圆放大淡入
               for (int i = 0; i < com.levelToggleTab.Count; i++)
               {
                    tweens.Add(com.levelToggleTab[i].transform.DOScale(1, 0.5f).SetDelay(i * 0.1f));
               }
               tweens.Add(DOTween.To(value => com.LevelBtn1.SetAlpha(value), 0, 1, 0.3f));
               tweens.Add(DOTween.To(value => com.LevelBtn2.SetAlpha(value), 0, 1, 0.3f).SetDelay(0.15f));
               tweens.Add(DOTween.To(value => com.LevelBtn3.SetAlpha(value), 0, 1, 0.3f).SetDelay(0.15f));
               tweens.Add(DOTween.To(value => com.LevelBtn4.SetAlpha(value), 0, 1, 0.3f).SetDelay(0.15f).OnComplete(() =>
               {
                    tweens.Add(com.CloseText.gameObject.DOFade(1,0.3f));
                    callback?.Invoke();
               }));                             
               //方向箭头淡入                               
               for (int i = 0; i < com.ArrowGroup.Count; i++)
               {
                    tweens.Add(com.ArrowGroup[i].DOFade(1, 0.01f).SetDelay(i*0.05f+0.05f));
               }
            }));
            for (int i = 0; i < com.ItemTrainCountsText.Count; i++)
            {
                tweens.Add(com.ItemTrainCountsText[i].DOFade(1, 0.5f).SetDelay(i*0.3f));
            }
            tweens.Add(DOTween.To(value => com.BreakLevelText.SetAlpha(value), 0, 1, 0.3f).SetDelay(0.2f));
            tweens.Add(DOTween.To(value => com.TrainItemText.SetAlpha(value), 0, 1, 0.3f).SetDelay(0.15f));
            tweens.Add(DOTween.To(value => com.BreakItemNameText.SetAlpha(value), 0, 1, 0.3f).SetDelay(0.15f));               
            tweens.Add(com.Boxing.gameObject.DOFade(1, 0.3f));
        }

        public void PlayNext(Action callback)
        {
            TouchManager.Instance.DisableTouch();
            //标题
            tweens.Add(DOTween.To(value => com.DescText.SetAlpha(value), 1, 0, 0.3f));
            tweens.Add(com.BreakLevelText.DOFade(0, 0.3f));
            tweens.Add(DOTween.To(value => com.TrainItemText.SetAlpha(value), 1, 0, 0.3f));
            tweens.Add(DOTween.To(value => com.BreakItemNameText.SetAlpha(value), 1, 0, 0.3f));
            //背景
            tweens.Add(com.BlackImg.DOFade(0, 0.2f));
            tweens.Add(com.Background.DOFade(0, 0.2f));
            //icon
            tweens.Add(com.Boxing.gameObject.DOFade(0, 0.2f));
            //栏目
            tweens.Add(com.PlayerAddTypeModule.DOFade(0, 0.3f));
            tweens.Add(com.PropertsNameText.DOFade(0, 0.3f));
            for (int i = 0; i < com.PropertsCountsText.Count; i++)
            {
                tweens.Add(com.PropertsCountsText[i].DOFade(0, 0.3f));
            }
            tweens.Add(com.TrainItemAddTypeModule.DOFade(0, 0.3f));
            tweens.Add(com.ItemAddNameText.DOFade(0, 0.3f));
            for (int i = 0; i < com.ItemTrainCountsText.Count; i++)
            {
                tweens.Add(com.ItemTrainCountsText[i].DOFade(0, 0.3f));
            }
            //方向箭头
            com.ArrowGroup.ForEach(item => tweens.Add(item.DOFade(0, 0.3f).SetDelay(0.1f)));
            //圆圈
            for (int i = 0; i < com.levelToggleTab.Count; i++)
            {
                tweens.Add(com.levelToggleTab[i].gameObject.DOFade(0, 0.3f).OnComplete(() =>
                {
                    callback?.Invoke();
                }));
            }
        }
        private void Kill()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }
    }
}