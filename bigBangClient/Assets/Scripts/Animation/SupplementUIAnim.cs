using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace BigBang.Animation
{

    public class SupplementUIAnim : MonoBehaviour
    {
        private RectTransform panelRect;

        private List<Tween> tweens = new List<Tween>();

        private void Awake()
        {
            panelRect = transform.GetComponentAtPath<RectTransform>("Background");
        }

        private void Init()
        {
            // 初始化缩放
            panelRect.localScale = Vector3.one * 0.5f;
            // 初始化透明度
            panelRect.gameObject.SetAlpha(0);
        }

        // 播放进入动画
        public void PlayEnter()
        {
            Kill();
            Init();
            tweens.Add(panelRect.DOScale(Vector3.one, 0.2f));
            tweens.Add(panelRect.gameObject.DOFade(1, 0.2f));
        }

        private void Kill()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }
    }
}