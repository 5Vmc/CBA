using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;

namespace BigBang.Animation
{
    public class ConfirmationPolicyPanelAnim : MonoBehaviour
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private Image blackImg;

        public void Init()
        {
            // 初始化缩放
            panel.localScale = Vector3.zero;
            // 初始化透明度
            blackImg.color = new Color(0, 0, 0, 0);
        }

        List<Tween> tweens = new List<Tween>();

        public void PlayEnter()
        {
            // 黑色背景淡入
            tweens.Add(blackImg.DOFade(0.5f, 0.15f));
            // 面板缩放
            tweens.Add(panel.DOScale(1, 0.15f));
        }

        public void PlayExit(Action callback)
        {
            // 面板缩放
            tweens.Add(panel.DOScale(0, 0.15f));
            // 背景淡出
            tweens.Add(blackImg.DOFade(0, 0.15f).OnComplete(() =>
            {
                callback?.Invoke();
            }));
        }

        public void ClearAnim()
        {
            foreach (var tween in tweens)
            {
                tween.Kill();
            }
            tweens.Clear();
        }
    }
}