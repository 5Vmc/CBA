using System;
using UnityEngine;

namespace Babu
{
    public static class CoroutineExtensions
    {

        public static Coroutine AddTo(this Coroutine disposable, GameObject targetObJ)
        {
            if (targetObJ == null)
            {
                return disposable;
            }
            var markTest = targetObJ.GetComponent<DestroyTaskTrigger>();
            if (markTest == null)
            {
                markTest = targetObJ.AddComponent<DestroyTaskTrigger>();
            }
            markTest.AllCoroutineList.Add(disposable);
            return disposable;
        }
    }
}
