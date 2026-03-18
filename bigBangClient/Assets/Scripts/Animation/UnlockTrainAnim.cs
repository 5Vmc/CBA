using BigBang.UI;
using UnityEngine;
using DG.Tweening;
using Utils;
using System.Collections.Generic;

namespace BigBang.Animation
{
    public class UnlockTrainAnim : MonoBehaviour
    {
        [SerializeField] private UnlockTrainItemComponent com;

        private List<Tween> tweens = new List<Tween>();

        private void Init()
        {
            //初始化位置
            com.TrainImgs.ForEach(item => item.rectTransform.SetAnchoredPositionX(0));
            com.DescText.rectTransform.SetAnchoredPositionX(7);
            com.TitleLight.rectTransform.SetAnchoredPositionX(-127f);
            com.ProjectLight1.rectTransform.SetAnchoredPositionX(6);
            com.ProjectLight2.rectTransform.SetAnchoredPositionX(6);
            //初始化透明度
            com.TitleText.SetAlpha(0);
            com.DescText.SetAlpha(0);
            com.LockImg.SetAlpha(0);
            com.TitleBackground.SetAlpha(0);
            com.ProjectLight1.SetAlpha(0);
            com.ProjectLight2.SetAlpha(0);
            com.ProjectYellowLight.SetAlpha(0);
            com.ProjectYellowLight2.SetAlpha(0);
            com.MoveUpImg.ForEach(item => item.SetAlpha(0));
            com.MoveDownImg.ForEach(item => item.SetAlpha(0));
            //设置缩放
            com.LockImg.rectTransform.localScale = Vector3.one * 0.5f;
            //去除浮动
            com.MoveUpImg.ForEach(item => Destroy(item.GetComponent<FloatAnim>()));
            com.MoveDownImg.ForEach(item => Destroy(item.GetComponent<FloatAnim>()));
            //设置图片
            SpriteManager.GetSprite(AtlasNames.Unlock, "0", (s) => { com.LockImg.sprite = s; });
        }

        public void Play()
        {
            Kill();
            Init();
            TouchManager.Instance.DisableTouch();
            //图片从侧边滑入(耗时4*0.2f+0.08f =0.88f)
            for (int i = 0; i < com.TrainImgs.Count; i++)
            {
                tweens.Add(com.TrainImgs[i].rectTransform.DoRelativeAnchorPosX(1000, 0.2f).SetDelay(0.08f * i).From());
            }
            //彩条淡入
            com.MoveUpImg.ForEach(item => tweens.Add(item.DOFade(1, 0.5f).SetDelay(0.4f)));
            com.MoveDownImg.ForEach(item => tweens.Add(item.DOFade(1, 0.5f).SetDelay(0.4f)));
            //彩条位移（耗时 ~=2s)
            for (int i = 0; i < com.MoveUpImg.Count; i++)
            {
                tweens.Add(com.MoveUpImg[i].rectTransform.DoRelativeAnchorPos(Vector2.one * -200, 0.15f).SetEase(Ease.InExpo).From().SetDelay(i * 0.05f + 0.4f));
            }
            for (int i = 0; i < com.MoveDownImg.Count; i++)
            {
                tweens.Add(com.MoveDownImg[i].rectTransform.DoRelativeAnchorPos(Vector2.one * 200, 0.14f).SetEase(Ease.InExpo).From().SetDelay(i * 0.02f + 0.6f));
            }
            //标题淡入
            tweens.Add(com.TitleText.DOFade(1, 0.2f).SetDelay(0.8f).OnComplete(() =>
            {
                //添加浮动
                com.MoveUpImg.ForEach(item => item.gameObject.AddComponent<FloatAnim>());
                com.MoveDownImg.ForEach(item => item.gameObject.AddComponent<FloatAnim>());
            }));
            //标题光效侧滑
            tweens.Add(com.TitleLight.rectTransform.DoRelativeAnchorPosX(1000, 0.2f).SetDelay(0.8f).From().OnComplete(() =>
            {
                //标题光效淡入
                tweens.Add(DOTween.To(value => com.TitleBackground.SetAlpha(value), 0, 0.85f, 0.3f));
                tweens.Add(com.TitleBackground.DOFade(1, 0.3f));
                //项目淡入
                tweens.Add(com.DescText.DOFade(1, 0.3f));
                //项目光效侧滑
                tweens.Add(com.ProjectLight1.rectTransform.DoRelativeAnchorPosX(1000, 0.3f).From());
                tweens.Add(com.ProjectLight2.rectTransform.DoRelativeAnchorPosX(1000, 0.3f).From());
                //项目侧滑
                tweens.Add(com.DescText.rectTransform.DoRelativeAnchorPosX(1000, 0.3f).From().OnComplete(() =>
                {
                    //项目光效淡入
                    tweens.Add(DOTween.To(value => com.ProjectYellowLight.SetAlpha(value), 0, 1, 0.3f));
                    tweens.Add(DOTween.To(value => com.ProjectYellowLight2.SetAlpha(value), 0, 0.5f, 0.3f));
                    //锁图片淡入
                    tweens.Add(com.LockImg.DOFade(1, 0.3f));
                    //锁图片砸入
                    tweens.Add(com.LockImg.rectTransform.DOScale(1.5f, 0.3f).From().OnComplete(() =>
                    {
                        tweens.Add(com.LockImg.rectTransform.DOScale(0.6f, 0.1f).OnComplete(() =>
                        {
                            tweens.Add(com.LockImg.rectTransform.DOScale(0.5f, 0.1f).OnComplete(() =>
                            {
                                TouchManager.Instance.EnableTouch();
                                com.UnlockAnim.Play("Play");
                            }));
                        }));
                    }));
                }));
            }));
        }

        private void Kill()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }
    }
}