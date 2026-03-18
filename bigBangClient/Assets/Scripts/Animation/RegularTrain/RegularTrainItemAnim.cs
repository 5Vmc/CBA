using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using Utils;

namespace BigBang.Animation
{
    public class RegularTrainItemAnim : MonoBehaviour
    {
        private List<Tween> tweens = new List<Tween>();

        public void Init()
        {
            transform.localScale = Vector3.one;
            gameObject.SetAlpha(0);
        }

        public void Play(float delay = 0)
        {
            //Webgl不支持Task.Delay
            Babu.DelayTaskService.Instance.Run(this.gameObject, delay, () =>
            {
                Kill();
                Init();
                //淡入
                tweens.Add(DOTween.To(value => gameObject.SetAlpha(value), 0, 1, 0.25f).SetDelay(delay));
                //由大变小
                tweens.Add(transform.DOScale(1.1f, 0.25f).From().SetDelay(delay));
            });
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
