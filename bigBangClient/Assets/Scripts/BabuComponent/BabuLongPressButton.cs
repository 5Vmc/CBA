using Babu;
using BigBang.Animation;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BigBang.UI
{
    /// <summary>
    /// 长按时以固定间隔触发回调的按钮
    /// </summary>
    [AddComponentMenu("UI/LongPressButton", 30)]
    public class BabuLongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {

        /// <summary>
        /// 点击动效
        /// </summary>
        public IBabuButtonAnim Anim { get; set; } = LongPressButtonAnim.Instance;

        /// <summary>
        /// 点击音效
        /// </summary>
        public string Sound { get; set; } = AudioNames.BTN_CLICK;

        /// <summary>
        /// 点击事件
        /// 返回的bool值为true时表示可以继续下一次点击
        /// </summary>
        public event Func<BabuLongPressButton, bool> onClick;

        /// <summary>
        /// 点击结束事件
        /// 一串点击结束后调用
        /// </summary>
        public event Action<BabuLongPressButton> onClickEnd;

        /// <summary>
        /// 连续点击开始的第一个的时间，此时间之前松开和普通点击一样，此时间后开始连续点击
        /// </summary>
        [Tooltip("连续点击开始的第一个的时间，此时间之前松开和普通点击一样，此时间后开始连续点击")]
        public float FirstClickTime = 0.5f;

        /// <summary>
        /// 连续点击的间隔时间(刚开始)
        /// </summary>
        [Tooltip("连续点击的间隔时间(刚开始)")]
        public float SecondClickTimeMax = 0.2f;

        /// <summary>
        /// 连续点击的间隔时间(最后)
        /// </summary>
        [Tooltip("连续点击的间隔时间(最后)")]
        public float SecondClickTimeMin = 0.05f;

        /// <summary>
        /// 连续点击的速度，间隔时间从长到短，所需要的时间
        /// </summary>
        [Tooltip("速度变化时间时间")]
        public float SecondClickSpeedUpTime = 2f;

        /// <summary>
        /// 使用点击动画
        /// </summary>
        [Tooltip("使用点击动画")]
        public bool UseAnim = true;


        private void OnEnable()
        {
            OnTrigEnd();
            if (UseAnim && Anim != null) Anim.ResetAnim();
        }
        private void OnDisable()
        {
            OnTrigEnd();
        }

        private bool isDown = false;
        private bool isTrigOnce = false;
        /// <summary>请勿手动调用，请使用onClick或者onClickEnd回调</summary>
        [Obsolete("请勿手动调用，请使用onClick或者onClickEnd回调", true)]
        public void OnPointerDown(PointerEventData eventData)
        {
            isDown = true;
            isTrigOnce = false;
            time = 0;
        }
        /// <summary>请勿手动调用，请使用onClick或者onClickEnd回调</summary>
        [Obsolete("请勿手动调用，请使用onClick或者onClickEnd回调", true)]
        public void OnPointerUp(PointerEventData eventData)
        {
            OnTrigEnd();
            if (isTrigOnce == false)
            {
                OnTrig(true);
                onClickEnd?.Invoke(this);
            }
        }
        /// <summary>请勿手动调用，请使用onClick或者onClickEnd回调</summary>
        [Obsolete("请勿手动调用，请使用onClick或者onClickEnd回调", true)]
        public void OnPointerExit(PointerEventData eventData)
        {
            OnTrigEnd();
        }


        private float time = 0;
        private void Update()
        {
            if (!isDown) return;
            time += Time.deltaTime;
            if (isTrigOnce)
            {
                if (time > SecondClickTime) OnTrig(false);
            }
            else
            {
                if (time > FirstClickTime) OnTrig(true);
            }
        }

        private float SecondClickTime = 0;
        private DateTime firstTrigDT;
        private void OnTrig(bool isFirstTrig)
        {
            if (isFirstTrig)
            {
                SecondClickTime = SecondClickTimeMax;
                firstTrigDT = Utils.DataConvUtil.ServerDateTime;
            }
            else
            {
                DateTime nowTrigDT = Utils.DataConvUtil.ServerDateTime;
                float t = (float)(nowTrigDT - firstTrigDT).TotalSeconds / SecondClickSpeedUpTime;
                SecondClickTime = Mathf.Lerp(SecondClickTimeMax, SecondClickTimeMin, t);
            }
            //Debug.Log("SecondClickTime = " + SecondClickTime);
            time = 0;
            isTrigOnce = true;
            bool? isCanTrigNext = onClick?.Invoke(this);
            if (isCanTrigNext.HasValue && (bool)(isCanTrigNext) == false)
            {
                OnTrigEnd();
            }

            if (Anim == null || !UseAnim)
            {
                AudioManager.Instance.PlaySound(Sound);
                onClick?.Invoke(this);
                return;
            }
            if (UseAnim && !Anim.IsPlaying)
            {
                // 播放音效
                AudioManager.Instance.PlaySound(Sound);
                // 播放动效
                Anim.Play(transform, () =>
                {

                });
            }
        }
        private void OnTrigEnd()
        {
            time = 0;
            if (isDown == true && isTrigOnce == true) onClickEnd?.Invoke(this);
            isDown = false;
        }


    }
}