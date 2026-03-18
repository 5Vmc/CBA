using System.Collections.Generic;
using BigBang.UI;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace BigBang.Animation
{
    public class BigBangResultAnim : MonoBehaviour
    {
        [SerializeField] private BigBangResultUIComponent com;

        private List<Tween> tweens = new List<Tween>();

        private void Init()
        {
            //初始化缩放
            com.IncomeText.rectTransform.localScale = Vector3.one * 4;
            //初始化位置
            com.AddForce.SetAnchoredPositionX(0);
            com.DescText.rectTransform.SetAnchoredPositionX(0);
            com.Board.SetAnchoredPositionY(160);
            com.Board.rotation = Quaternion.Euler(0, 0, 0);
            //初始化透明度
            com.IncomeText.SetAlpha(0);
            com.AddForce.gameObject.SetAlpha(0);
            com.DescText.SetAlpha(0);
            com.TitleText.SetAlpha(0);
            com.Board.gameObject.SetAlpha(0);
        }

        public void Play()
        {
            Kill();
            Init();
            //图标下移
            tweens.Add(com.Board.DoRelativeAnchorPosY(200, 0.3f).From().OnComplete(() =>
            {
                tweens.Add(com.TitleText.DOFade(1, 0.3f).SetEase(Ease.InQuint).OnComplete(() =>
                {
                    tweens.Add(com.AddForce.DoRelativeAnchorPosX(500, 0.3f).From().OnComplete(() =>
                    {
                        tweens.Add(com.DescText.DOFade(1, 0.3f).SetEase(Ease.InQuint).OnComplete(() =>
                        {
                            //文本砸入
                            tweens.Add(com.IncomeText.DOFade(1, 0.5f).SetEase(Ease.InQuint));
                            tweens.Add(com.IncomeText.rectTransform.DOScale(1, 0.5f).SetEase(Ease.InQuint));
                        }));
                        tweens.Add(com.DescText.rectTransform.DoRelativeAnchorPosX(500, 0.15f).From());
                    }));
                    tweens.Add(DOTween.To(value => com.AddForce.gameObject.SetAlpha(value), 0, 1, 0.3f));
                }));
            }));
            //图标旋转
            tweens.Add(com.Board.DORotate(Vector3.up * 360 * 2, 0.3f, RotateMode.LocalAxisAdd));
            //图标淡入
            tweens.Add(DOTween.To(value => com.Board.gameObject.SetAlpha(value), 0, 1, 0.3f));
        }

        private void Kill()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }
    }
}