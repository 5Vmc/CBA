using UnityEngine;
using BigBang.Animation;
using UnityEngine.UI;
using DG.Tweening;
using Utils;

namespace BigBang.UI
{
    public class ArrowItem : MonoBehaviour
    {
        [SerializeField] private Image img1;
        [SerializeField] private Image img2;
        [SerializeField] private Image img3;

        // 播放动画
        public void PlayAnim(float time1, float time2, float delay)
        {
            img1.SetAlpha(0);
            img2.SetAlpha(0);
            img3.SetAlpha(0);
            img1.DOFade(1, time1).OnComplete(() =>
            {
                img1.DOFade(0.7f, time2);
            }).SetDelay(delay);
            img2.DOFade(0.8f, time1).OnComplete(() =>
            {
                img2.DOFade(0.5f, time2);
            }).SetDelay(delay + 0.1f);
            img3.DOFade(0.6f, time1).OnComplete(() =>
            {
                img3.DOFade(0.3f, time2);
            }).SetDelay(delay + 0.2f);
        }

        // 隐藏
        public void Hide()
        {
            img1.SetAlpha(0);
            img2.SetAlpha(0);
            img3.SetAlpha(0);
        }

        // 显示
        public void Show()
        {
            img1.SetAlpha(0.8f);
            img2.SetAlpha(0.6f);
            img3.SetAlpha(0.4f);
        }
    }
}