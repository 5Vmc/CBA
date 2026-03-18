using UnityEngine;
using System;
using DG.Tweening;

namespace BigBang.Animation
{
    public class TrainUISwitchOutAnim : MonoBehaviour, AnimOut
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private CanvasGroup canvasGroup1;
        [SerializeField] private TrainUIAnim anim;

        public void Play(Action callback)
        {
            anim.SwitchOut();
            DOTween.To(value => canvasGroup1.alpha = value, 1, 0, 0.35f).OnComplete(() =>
            {
                callback?.Invoke();
                canvasGroup1.alpha = 1;
            });
        }
    }
}