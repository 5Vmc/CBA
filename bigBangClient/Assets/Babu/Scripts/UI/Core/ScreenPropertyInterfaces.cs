using DG.Tweening;
using UnityEngine;

namespace deVoid.UIFramework
{
    /// <summary>
    /// Base interface for all the screen properties
    /// </summary>
    public interface IScreenProperties { }

    /// <summary>
    /// Base interface for all Panel properties
    /// </summary>
    public interface IPanelProperties : IScreenProperties
    {
        PanelPriority Priority { get; set; }
        PanelSibling Sibling { get; set; }
    }

    /// <summary>
    /// Base interface for Window properties.
    /// </summary>
    public interface IWindowProperties : IScreenProperties
    {
        WindowPriority WindowQueuePriority { get; set; }
        bool HideOnForegroundLost { get; set; }
        bool IsPopup { get; set; }
        bool SuppressPrefabProperties { get; set; }
        bool IsTop { get; set; }

        //动画控制
        public bool ShowUseMagic { get; set; }
        public bool HideUseMagic { get; set; }
        public Transform ShowMagicTargetTrans { get; set; }
        public Transform HideMagicTargetTrans { get; set; }
        public float ShowMoveTime { get; set; }
        public float HideMoveTime { get; set; }
        public Ease ShowEase { get; set; }
        public Ease HideEase { get; set; }
    }
}
