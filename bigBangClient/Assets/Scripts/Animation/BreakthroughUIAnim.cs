using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using BigBang.UI;
using Utils;

namespace BigBang.Animation
{
    public class BreakthroughUIAnim : MonoBehaviour
    {
        [SerializeField] private BreakthroughComponent com;

        public static Vector2 sourcePosition = Vector2.zero;
        public static Vector2 targetPosition = Vector2.zero;
        public static GameObject source = null;

        private List<Tween> tweens = new List<Tween>();

        private void Init()
        {
            //关闭原始
            source?.SetActive(false);
            com.Boxing.localPosition = new Vector3(-163f, 92f, 0);
            //设置透明度
            com.Boxing.gameObject.SetAlpha(1);
            com.Title.SetAlpha(0);
            com.Background.SetAlpha(0);
            com.NameText.SetAlpha(0);
            com.Txts.ForEach(item => item.SetAlpha(0));
            com.Txts.ForEach(item => item.rectTransform.localScale = Vector3.one);
            com.YellowBoxing.SetAlpha(1);
            com.FlashBoard.SetAlpha(1);
            com.HighlightImage.SetAlpha(1);
            com.Ghost1.rectTransform.localScale = Vector3.one;
            com.Ghost1.SetAlpha(0);
            com.Ghost2.SetAlpha(0);
            com.Boxing.rotation = Quaternion.Euler(0, 0, 0);

            com.Image0.SetAlpha(0);
            com.Image1.SetAlpha(0);

            com.NameText1.SetAlpha(0);

            com.Txts1.ForEach((item) => item.SetAlpha(0));
            com.Txts1.ForEach(item => item.rectTransform.localScale = Vector3.one);
            com.Texts1.SetAlpha(0);
            com.Texts1.transform.localScale = Vector3.one;

        }

        public void Play(Action callback)
        {
            Kill();
            Init();
            //模糊
            //tweens.Add(DOTween.To(value => com.Blur.material.SetFloat("_Radius", value), 0, 5, 0.3f));
            //黄色拳头
            tweens.Add(com.YellowBoxing.DOFade(0, 1.5f).SetEase(Ease.InQuint));
            //亮底板淡出
            tweens.Add(com.FlashBoard.DOFade(0, 1.5f).SetEase(Ease.InQuint));
            tweens.Add(com.HighlightImage.DOFade(0, 1.5f).SetEase(Ease.InQuint));
            //圆圈缩放
            com.Boxing.localScale = Vector3.one * 0.5f;
            tweens.Add(com.Boxing.DOScale(Vector3.one * 0.8f, 1f));
            //圆圈旋转
            tweens.Add(com.Boxing.DOLocalRotate(Vector3.up * 360 * 2, 1f, RotateMode.LocalAxisAdd));
            //圆圈移动
            RectTransformUtility.ScreenPointToLocalPointInRectangle(com.Background.rectTransform, sourcePosition, UIController.Instance.GetCamera(), out var p);
            tweens.Add(com.Boxing.DOAnchorPos(p, 0.8f).From().OnComplete(() =>
            {
                //播放粒子特效
                com.Particle.Play();
                //砸入
                tweens.Add(com.Boxing.DOScale(Vector3.one * 0.7f, 0.05f).OnComplete(() =>
                {

                    //标题淡入
                    tweens.Add(com.Title.GetComponent<RectTransform>().DoRelativeAnchorPosX(100, 0.3f).From().OnComplete(() =>
                    {
                        //栏目背景淡入
                        //tweens.Add(DOTween.To(value => com.Image0.SetAlpha(value), 0, 1, 0.3f).SetDelay(0.15f));
                        //tweens.Add(DOTween.To(value => com.Image1.SetAlpha(value), 0, 1, 0.3f).SetDelay(0.15f));
                        //tweens.Add(com.NameText.DOFade(1, 1f));
                        //tweens.Add(com.NameText.DOFade(1, 0.3f).SetDelay(0));
                        //tweens.Add(com.NameText.rectTransform.DoRelativeAnchorPosX(100, 0.3f).From());
                        //tweens.Add(com.NameText.rectTransform.DoRelativeAnchorPosX(100, 0.3f).From().OnComplete(() =>
                        //{
                        //    //字体单独砸入
                        //    for (int i = 0; i < com.Txts.Count; i++)
                        //    {
                        //        if (!com.Txts[i].gameObject.activeInHierarchy)
                        //        {
                        //            tweens.Add(com.Txts[i].DOFade(1, 0.1f).SetDelay(i * 0.25f + 0.2f));
                        //            tweens.Add(com.Txts[i].rectTransform.DOScale(3f, 0.1f).From().SetDelay(i * 0.25f + 0.2f).OnComplete(() =>
                        //            {
                        //                callback?.Invoke();
                        //            }));
                        //        }
                        //        else
                        //        {
                        //            tweens.Add(com.Txts[i].DOFade(1, 0.1f).SetDelay(i * 0.25f + 0.2f));
                        //            tweens.Add(com.Txts[i].rectTransform.DOScale(3f, 0.1f).OnStart(() => AudioManager.Instance.PlaySound(AudioNames.NUMBER_PUNCH)).From().SetDelay(i * 0.25f + 0.2f));
                        //        }
                        //    }
                        //}));
                        tweens.Add(com.Image0.transform.DOScale(1, 0.3f));
                        tweens.Add(com.Image1.transform.DOScale(1, 0.3f).SetDelay(0.1f));
                        tweens.Add(DOTween.To(value => com.Image0.SetAlpha(value), 0, 1, 0.3f));
                        tweens.Add(com.NameText.DOFade(1, 0.25f).SetDelay(0.2f));
                        tweens.Add(DOTween.To(value => com.Image1.SetAlpha(value), 0, 1, 0.3f).SetDelay(0.1f).OnComplete(() =>
                        {

                            tweens.Add(com.NameText1.DOFade(1, 0.25f).OnComplete(() =>
                            {
                                
                                for (int i = 0; i < com.TextsGroup.Count; i++)
                                {
                                    // Debug.Log("    " + com.TxtsGroup[i].name+"   "+i);
                                    if (!com.TextsGroup[i].gameObject.activeSelf)
                                    {

                                    }
                                    else
                                    {
                                        tweens.Add(com.TextsGroup[i].DOFade(1, 0.1f).SetDelay(i * 0.3f));
                                        tweens.Add(com.TextsGroup[i].transform.DOScale(3f, 0.1f).OnStart(() => AudioManager.Instance.PlaySound(AudioNames.EVENT_COMMONHIT)).From().SetDelay(i * 0.3f));
                                        for (int j = 0; j < com.Txts.Count; j++)
                                        {
                                            tweens.Add(com.Txts[j].DOFade(1, 0.1f).SetDelay(i * 0.3f).OnComplete(() =>
                                            {
                                                callback?.Invoke();
                                            }));
                                        }
                                        for (int t = 0; t < com.Txts1.Count; t++)
                                        {
                                            tweens.Add(com.Txts1[t].DOFade(1, 0.1f).SetDelay(i * 0.3f));
                                        }
                                    }
                                }
                            }));
                        }));
                    }).SetDelay(0.15f));
                    tweens.Add(DOTween.To(value => com.Title.SetAlpha(value), 0, 1, 0.3f).SetDelay(0.15f));
                    //虚影
                    com.Ghost1.SetAlpha(1);
                    com.Ghost1.rectTransform.DOScale(2f, 0.3f);
                    tweens.Add(com.Ghost1.DOFade(0, 0.3f).SetEase(Ease.InQuad));
                    tweens.Add(com.Ghost2.DOFade(1, 0.3f));
                }));
            }));
            tweens.Add(com.Background.DOFade(1, 0.5f));
        }

        public void PlayNext(Action callback)
        {
            //tweens.Add(DOTween.To(value => com.Blur.material.SetFloat("_Radius", value), 5, 0, 0.3f));
            TouchManager.Instance.DisableTouch();
            tweens.Add(DOTween.To(value => com.Title.SetAlpha(value), 1, 0, 0.3f));
            tweens.Add(com.Background.DOFade(0, 0.2f));
            tweens.Add(com.NameText.DOFade(0, 0.2f));
            tweens.Add(com.NameText1.DOFade(0, 0.2f));
            com.Txts.ForEach(item => tweens.Add(item.DOFade(0, 0.2f)));
            com.Txts1.ForEach(item => tweens.Add(item.DOFade(0, 0.2f)));
            tweens.Add(com.Image0.DOFade(0,0.2f));
            tweens.Add(com.Image1.DOFade(0, 0.2f));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), targetPosition, UIController.Instance.GetCamera(), out var p);
            //圆圈淡出
            tweens.Add(DOTween.To(value => com.Boxing.gameObject.SetAlpha(value), 1, 0, 0.5f));
            //圆圈缩小
            tweens.Add(com.Boxing.DOScale(0.5f, 0.5f).OnComplete(() =>
            {
                source?.SetActive(true);
                callback?.Invoke();
            }));
        }

        private void Kill()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }
    }
}