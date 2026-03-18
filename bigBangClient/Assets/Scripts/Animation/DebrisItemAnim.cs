using UnityEngine;
using DG.Tweening;
using Utils;

namespace BigBang.Animation
{
    public class DebrisItemAnim : AnimBase
    {
        public override void Init()
        {
            base.Init();
            // 初始化缩放
            transform.localScale = Vector3.zero;
            // 初始化透明度
            gameObject.SetAlpha(0);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            // 放大
            transform.DOScale(Vector3.one, 0.3f);
            // 淡入
            gameObject.DOFade(1, 0.3f);
        }

        public override void PlayExit()
        {
            base.PlayExit();
        }
    }
}