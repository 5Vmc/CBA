using System.Collections.Generic;
using BigBang.UI;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;

namespace BigBang.Animation
{
    public class RegularTrainItemBreakThroughAnim : MonoBehaviour
    {
        [SerializeField] private RegularTrainItemComponent com;

        private List<Tween> tweens = new List<Tween>();

        public void Play()
        {
            Kill();

            float value = GetComponent<RegularTrainItem>().Item.GetBreakThroughProgress();
            if (com.BreakProgress.fillAmount <= value)
            {
                tweens.Add(com.BreakProgress.DOFillAmount(value, 0.3f));
                //拳头
                tweens.Add(com.Boxing.DOFade(1, 0.15f).OnComplete(() =>
                {
                    tweens.Add(com.Boxing.DOFade(0, 0.15f));
                }));
            }
            else
            {
                tweens.Add(com.BreakProgress.DOFillAmount(1, 0.3f).OnComplete(() =>
                {
                    com.BreakProgress.fillAmount = 0;
                    tweens.Add(com.BreakProgress.DOFillAmount(value, 0.3f));
                }));
                //拳头
                tweens.Add(com.BoxingYellow.DOFade(1, 0.15f).OnComplete(() =>
                {
                    tweens.Add(com.BoxingYellow.DOFade(0, 0.15f));
                }));
            }
            tweens.Add(com.FlashBackground.DOFade(1, 0.15f).OnComplete(() =>
            {
                tweens.Add(com.FlashBackground.DOFade(0, 0.15f));
            }));
            //黄色圆圈
            tweens.Add(com.YellowProgress.DOFade(1, 0.15f).OnComplete(() =>
            {
                tweens.Add(com.YellowProgress.DOFade(0, 0.15f));
            }));
            //星星出现
            tweens.Add(com.Star.DOFade(1, 0.15f).OnComplete(() =>
            {
                tweens.Add(com.Star.DOFade(0, 0.15f));
            }));
        }

        private void Kill()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }

        private void Update()
        {
            //设置星星位置
            com.Star.rectTransform.localRotation = Quaternion.Euler(0, 0, com.BreakProgress.fillAmount * 360 - 90);
        }

        private void OnDisable()
        {
            Kill();
        }
    }
}