using System;
using DG.Tweening;
using UnityEngine;

namespace deVoid.UIFramework
{
    /// <summary>
    /// Properties common to all windows
    /// </summary>
    [System.Serializable]
    public class WindowProperties : IWindowProperties
    {
        [SerializeField]
        protected bool hideOnForegroundLost = true;

        [SerializeField]
        protected WindowPriority windowQueuePriority = WindowPriority.ForceForeground;

        [SerializeField]
        protected bool isPopup = false;

        [SerializeField]
        protected bool isTop = false;

        public WindowProperties()
        {
            hideOnForegroundLost = true;
            windowQueuePriority = WindowPriority.ForceForeground;
            isPopup = false;
            isTop = false;
        }

        /// <summary>
        /// How should this window behave in case another window
        /// is already opened?
        /// </summary>
        /// <value>Force Foreground opens it immediately, Enqueue queues it so that it's opened as soon as
        /// the current one is closed. </value>
        public WindowPriority WindowQueuePriority
        {
            get { return windowQueuePriority; }
            set { windowQueuePriority = value; }
        }

        /// <summary>
        /// Should this window be hidden when other window takes its foreground?
        /// </summary>
        /// <value><c>true</c> if hide on foreground lost; otherwise, <c>false</c>.</value>
        public bool HideOnForegroundLost
        {
            get { return hideOnForegroundLost; }
            set { hideOnForegroundLost = value; }
        }

        /// <summary>
        /// When properties are passed in the Open() call, should the ones
        /// configured in the viewPrefab be overwritten?
        /// </summary>
        /// <value><c>true</c> if suppress viewPrefab properties; otherwise, <c>false</c>.</value>
        public bool SuppressPrefabProperties { get; set; }

        /// <summary>
        /// Popups are displayed with a black background behind them and
        /// in front of all other Windows
        /// </summary>
        /// <value><c>true</c> if this window is a popup; otherwise, <c>false</c>.</value>
        public bool IsPopup
        {
            get { return isPopup; }
            set { isPopup = value; }
        }

        public bool IsTop
        {
            get { return isTop; }
            set { isTop = value; }
        }

        public WindowProperties(bool suppressPrefabProperties = false)
        {
            WindowQueuePriority = WindowPriority.ForceForeground;
            HideOnForegroundLost = false;
            SuppressPrefabProperties = suppressPrefabProperties;
        }

        public WindowProperties(WindowPriority priority, bool hideOnForegroundLost = false, bool suppressPrefabProperties = false)
        {
            WindowQueuePriority = priority;
            HideOnForegroundLost = hideOnForegroundLost;
            SuppressPrefabProperties = suppressPrefabProperties;
        }

        #region 窗口动画

        /// <summary>请使用方法来保证风格一致</summary>
        public bool ShowUseMagic { get; set; } = false;
        /// <summary>请使用方法来保证风格一致</summary>
        public bool HideUseMagic { get; set; } = false;
        /// <summary>请使用方法来保证风格一致</summary>
        public Transform ShowMagicTargetTrans { get; set; } = null;
        /// <summary>请使用方法来保证风格一致</summary>
        public Transform HideMagicTargetTrans { get; set; } = null;
        /// <summary>请使用方法来保证风格一致</summary>
        public float ShowMoveTime { get; set; } = 1.0f;
        /// <summary>请使用方法来保证风格一致</summary>
        public float HideMoveTime { get; set; } = 1.0f;
        /// <summary>请使用方法来保证风格一致</summary>
        public Ease ShowEase { get; set; } = Ease.OutQuad;
        /// <summary>请使用方法来保证风格一致</summary>
        public Ease HideEase { get; set; } = Ease.OutQuad;

        /// <summary>
        /// 使用展现动画，请在代码中手动调用
        /// 窗口中需要同时拖拽上背景物体和飞行物体
        /// 跳出礼包
        /// </summary>
        /// <param name="ShowMagicTargetTrans">窗口从哪里飞出来</param>
        /// <param name="ShowMoveTime">飞行时间</param>
        /// <param name="ShowEase">缓动曲线</param>
        private void SetMagicShowAnim(Transform ShowMagicTargetTrans, float ShowMoveTime = 1.0f, Ease ShowEase = Ease.OutQuad)
        {
            this.ShowUseMagic = true;
            this.ShowMagicTargetTrans = ShowMagicTargetTrans;
            this.ShowMoveTime = ShowMoveTime;
            this.ShowEase = ShowEase;
        }

        /// <summary>
        /// 使用隐藏动画，请在代码中手动调用
        /// 窗口中需要同时拖拽上背景物体和飞行物体
        /// </summary>
        /// <param name="HideMagicTargetTrans">窗口从哪里飞出来</param>
        /// <param name="HideMoveTime">飞行时间</param>
        /// <param name="HideEase">缓动曲线</param>
        private void SetHideAnim(Transform HideMagicTargetTrans, float HideMoveTime = 1.0f, Ease HideEase = Ease.OutQuad)
        {
            this.HideUseMagic = true;
            this.HideMagicTargetTrans = HideMagicTargetTrans;
            this.HideMoveTime = HideMoveTime;
            this.HideEase = HideEase;
        }

        /// <summary>
        /// 设置动画效果
        /// </summary>
        /// <param name="magicTargetTrans">窗口从哪里飞出来</param>
        private void SetAnim(Transform magicTargetTrans)
        {
            SetMagicShowAnim(magicTargetTrans, 0.8f, DG.Tweening.Ease.OutBack);
            SetHideAnim(magicTargetTrans, 0.3f);
        }

        /// <summary>
        /// 使用开启与关闭窗口的动画
        /// 设置窗口从哪里飞出来
        /// </summary>
        public Transform MagicTargetTrans
        {
            set
            {
                SetAnim(value);
            }
            get
            {
                return ShowMagicTargetTrans;
            }
        }

        #endregion

    }
}
