
using Babu;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Babu
{
    public class DestroyTaskTrigger : MonoBehaviour
    {
        public List<Coroutine> AllCoroutineList = new List<Coroutine>();

        private void OnDestroy()
        {
            for (int i = 0; i < AllCoroutineList.Count; i++)
            {
                var tempCoroutine = AllCoroutineList[i];
                DelayTaskService.Instance.StopTask(tempCoroutine);
            }
        }
    }
}