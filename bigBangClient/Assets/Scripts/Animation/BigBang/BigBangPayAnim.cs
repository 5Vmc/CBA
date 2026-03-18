using System;
using BigBang.UI;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

namespace BigBang.Animation
{
    public class BigBangPayAnim : MonoBehaviour
    {
        [SerializeField] private BigBangPayUIComponent com;

        private List<Tween> tweens = new List<Tween>();

        private void Init()
        {
            Kill();
            com.Panel.localScale = Vector3.zero;
        }

        public void Play()
        {
            Init();
            // 面板弹出音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_POP);
            tweens.Add(com.Panel.DOScale(1, 0.2f));
        }

        public void PlayNext(Action callback)
        {
            tweens.Add(com.Panel.DOScale(0, 0.2f).OnComplete(() =>
            {
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