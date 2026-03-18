using System;
using UnityEngine;

namespace BigBang.Animation
{
    public interface IBabuButtonAnim
    {
        public bool IsPlaying { get; }
        public void Play(Transform transform, Action callback);
        public void ClearAnim();
        public void ResetAnim();
    }
}