using BigBang.Animation;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;

namespace BigBang.UI
{
    // 取代原始的Button
    [AddComponentMenu("UI/Button", 30)]
    public class BabuButton : Button
    {
        public bool IsDelayButton = false;
        [Obsolete("使用OnClick代替", true)]
        public new ButtonClickedEvent onClick { get => base.onClick; }
        // 点击动效
        public IBabuButtonAnim Anim { get; set; } =new DefaultButtonAnim();
        // 点击音效
        public string Sound { get; set; } = AudioNames.BTN_CLICK;

        // 点击前事件，收到点击后，在动画播放前调用
        public event Action<BabuButton> OnPreClick;

        // 点击事件，点击后播放一个缩放动画后调用
        public event Action<BabuButton> OnClick;
        /// <summary>
        /// 
        /// </summary>
        private Timer timer;
        private bool isClicked = false;

        protected override void OnEnable()
        {
            // 添加事件
            base.onClick.AddListener(Click);
        }

        protected override void OnDisable()
        {
            // 移除事件
            base.onClick.RemoveListener(Click);
            isClicked = false;
            timer?.Cancel();
        }

        // 触发点击事件
        private void Click()
        {
            if (IsDelayButton)
            {
                if (isClicked) return;
                isClicked = true;
                timer = Timer.Register(this.gameObject, 1f, () => { isClicked = false; });
            }


            if (Anim == null)
            {
                AudioManager.Instance.PlaySound(Sound);
                OnPreClick?.Invoke(this);
                OnClick?.Invoke(this);
                return;
            }
            if (!Anim.IsPlaying)
            {
                // 播放音效
                AudioManager.Instance.PlaySound(Sound);

                OnPreClick?.Invoke(this);

                // 播放动效
                Anim.Play(transform, () =>
                {
                    OnClick?.Invoke(this);
                });
            }
        }


    }
}