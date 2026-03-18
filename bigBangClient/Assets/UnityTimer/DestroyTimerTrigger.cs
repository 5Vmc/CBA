
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;

namespace UnityTimer
{
    public class DestroyTimerTrigger : MonoBehaviour
    {
        public List<Timer> AllTimerList = new List<Timer>();

        private void OnDestroy()
        {
            for (int i = 0; i < AllTimerList.Count; i++)
            {
                var tempTimer = AllTimerList[i];
                tempTimer.Cancel();
            }
        }
    }
}