using System.Collections.Generic;
using BigBang.UI;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.Animation
{
    public class InventoryItemAnim : MonoBehaviour
    {
       // [SerializeField] private UIParticle particle;
        private List<Tween> tweens = new List<Tween>();
        private void Init()
        {
            ClearAnim();
            // 初始化缩放
           // transform.localScale = Vector3.one * 1.1f;
            // 初始化透明度
            gameObject.SetAlpha(0);
           
        }

        public void PlayEnter(float delay)
        {
            Init();
            tweens.Add(gameObject.DOFade(1, 0.15f).SetDelay(delay).SetEase(Ease.Linear));
           // tweens.Add(selfRect.DOScale(Vector3.one, 0.15f).SetDelay(delay));
           
        }

        private void ClearAnim()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }
        

    }
}