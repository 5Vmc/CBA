using System;
using UnityEngine;

namespace Babu.UI
{
    internal class TransAnimation : MonoBehaviour
    {
        public virtual void In(Action completeCallback)
        {
            completeCallback?.Invoke();
        }

        public virtual void Out(Action completeCallback)
        {
            completeCallback?.Invoke();
        }
    }
}
