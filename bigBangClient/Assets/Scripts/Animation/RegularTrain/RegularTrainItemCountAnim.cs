using System.Collections.Generic;
using BigBang.UI;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace BigBang.Animation
{
    public class RegularTrainItemCountAnim : MonoBehaviour
    {
        [SerializeField] private RegularTrainItemComponent com;

        List<Tween> tweens = new List<Tween>();

        private void Init()
        {
            com.CountImgCopy.gameObject.SetActive(true);
            com.CountImgCopy.SetAlpha(1);
            com.CountImg.rectTransform.SetAnchoredPositionY(35.7f);
        }

        public void Play()
        {
            Kill();
            Init();
            //新标签下移
            tweens.Add(com.CountImg.rectTransform.DoRelativeAnchorPosY(10, 0.15f).From());
            //新标签淡入
            tweens.Add(com.CountImg.DOFade(1,0.15f));
            //旧标签淡出
            tweens.Add(com.CountImgCopy.DOFade(0,0.15f).OnComplete(() => com.CountImgCopy.gameObject.SetActive(false)));
        }

        private void Kill()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }

        private void OnDisable()
        {
            Kill();
        }
    }
}