using System;
using DG.Tweening;
using UnityEngine;

namespace BigBang.Animation

{
    public class PanelBaseAnim : AnimBase
    {
        [SerializeField] private RectTransform topRect;
        [SerializeField] private RectTransform bottomRect;

        private string enterSound;
         
        public virtual void PlayContent()
        {

        }

        protected virtual void InitEnterSound(string sound)
        {
            enterSound = sound;
        }
        public override void Init()
        {
            base.Init();
        }

        /// <summary>
        /// 播放进入动画
        /// </summary>
        public override void PlayEnter()
        {
            base.PlayEnter();
            if(enterSound==null)
                AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);
            else{
                 AudioManager.Instance.PlaySound(enterSound);
            }

            // 顶部栏下移
            topRect?.DoRelativeAnchorPosY(200, 0.3f).From();

            if(bottomRect){
                // 底部栏上移
                bottomRect.DoRelativeAnchorPosY(-100, 0.3f).From();
                // 底部栏淡入
                bottomRect.gameObject.DOFade(1, 0.3f).OnComplete(() =>
                {
                   this.PlayContent();
                });
            }
        }

        /// <summary>
        /// 播放退出动画
        /// </summary>
        public override void PlayExit()
        {
            base.PlayExit();

        }
    }
}