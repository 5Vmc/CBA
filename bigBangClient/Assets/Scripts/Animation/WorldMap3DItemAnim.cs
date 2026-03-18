using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BigBang
{
    public class WorldMap3DItemAnim : MonoBehaviour
    {
        [SerializeField] private Transform aniNode;

        private Sequence shakeSequence;

        //private List<Tween> circleLoopTween = new List<Tween>();

        private void OnDisable()
        {
            shakeSequence?.Kill();
        }

        public void PlayShakeLoopAnim()
        {
            shakeSequence?.Kill();
            float tick = 0.5f;
            var scale = new Vector3(0.9f, 1, 1);
            aniNode.rotation = Quaternion.Euler(0, 180, 0);
            aniNode.localScale = scale;

            shakeSequence = DOTween.Sequence();

            shakeSequence.Append(aniNode.DORotate(new Vector3(0, 20, 0), tick, RotateMode.WorldAxisAdd).SetEase(Ease.OutCubic));
            shakeSequence.Insert(0, aniNode.DOScale(scale * 1.05f, tick).SetEase(Ease.OutCubic));

            shakeSequence.Append(aniNode.DORotate(new Vector3(0, -20, 0), tick, RotateMode.WorldAxisAdd).SetEase(Ease.InCubic));
            shakeSequence.Insert(tick, aniNode.DOScale(scale, tick).SetEase(Ease.InCubic));

            shakeSequence.Append(aniNode.DORotate(new Vector3(0, -20, 0), tick, RotateMode.WorldAxisAdd).SetEase(Ease.OutCubic));
            shakeSequence.Insert(tick * 2, aniNode.DOScale(scale * 1.05f, tick).SetEase(Ease.OutCubic));

            shakeSequence.Append(aniNode.DORotate(new Vector3(0, 20, 0), tick, RotateMode.WorldAxisAdd).SetEase(Ease.InCubic));
            shakeSequence.Insert(tick * 3, aniNode.DOScale(scale, tick).SetEase(Ease.InCubic));

            shakeSequence.SetLoops(-1);
        }

        // 播放圆圈循环动画
        public void PlayCircleLoopAnim()
        {
            //outCircle.GetComponent<SpriteRenderer>().color = Color.white;
            //innerCircles.ForEach(item => item.GetComponent<SpriteRenderer>().color = Color.white);

            //outCircle.localScale = Vector3.zero;
            //outCircle.DOScale(1.2f, 0.5f);

            //for (int i = 0; i < innerCircles.Count; i++)
            //{
            //    innerCircles[i].localScale = Vector3.zero;
            //    circleLoopTween.Add(innerCircles[i].DOScale(2f, 2f).SetDelay(1f * i).SetEase(Ease.Linear).SetLoops(-1));
            //}
        }

        //public void ShowNameTxtAnim()
        //{
        //    StartCoroutine(nameText.GetComponent<RevealText>().FadeInText());
        //}

        public void ShowName()
        {
            //nameText.DOFade(1, 0.3f);
        }

        public void HideName()
        {
            //nameText.DOFade(0, 0.3f);
        }

        // 关闭圆圈循环动画
        public void StopCircleLoopAnim()
        {
            //circleLoopTween.ForEach(item => item?.Kill());
            //circleLoopTween.Clear();
        }

        // 圆圈淡出动画
        public void PlayCircleFadeOut()
        {
            //StopCircleLoopAnim();
            //innerCircles.ForEach(item => item.GetComponent<SpriteRenderer>().DOFade(0, 0.5f));
            //outCircle.GetComponent<SpriteRenderer>().DOFade(0, 0.5f);
        }

        public void InitIconAnim()
        {
            //outCircle.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
            //innerCircles.ForEach(item => item.GetComponent<SpriteRenderer>().color = Color.white);
            //icon.color = Color.white;
            //progressText.color = Color.white;
            aniNode.localScale = Vector3.zero;
            aniNode.rotation = Quaternion.Euler(0, 180, 0);
        }

        // 播放出现动画
        public void PlayAppearAnim()
        {
            aniNode.rotation = Quaternion.Euler(0, 180, 0);
            ShowName();
            PlayCircleLoopAnim();
            // 队徽放大
            aniNode.DOScale(new Vector3(0.9f, 1, 1), 0.5f);
            // 队徽旋转
            aniNode.DOLocalRotate(new Vector3(0, 180, 0), 0.25f, RotateMode.LocalAxisAdd).OnComplete(() =>
            {
                // 队徽旋转
                aniNode.DOLocalRotate(new Vector3(0, 180, 0), 0.25f, RotateMode.LocalAxisAdd).OnComplete(() =>
                {
                    //nameText.DOFade(1, 0.3f);
                    PlayShakeLoopAnim();
                });
            });
        }

        // 播放消失动画
        public void PlayDisappearAnim(Action callback)
        {
            shakeSequence?.Kill();
            HideName();
            PlayCircleFadeOut();
            aniNode.rotation = Quaternion.Euler(0, 180, 0);
            aniNode.localScale = Vector3.one;
            aniNode.DOScale(0, 0.5f);
            aniNode.DOLocalRotate(new Vector3(0, 180, 0), 0.25f, RotateMode.LocalAxisAdd).OnComplete(() =>
            {
                aniNode.DOLocalRotate(new Vector3(0, 180, 0), 0.25f, RotateMode.LocalAxisAdd).OnComplete(() => callback?.Invoke());
            });
        }
    }
}