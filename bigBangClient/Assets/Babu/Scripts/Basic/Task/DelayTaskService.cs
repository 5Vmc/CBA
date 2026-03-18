using UnityEngine;
using System.Collections;

namespace Babu
{
    // 延时任务服务
    public class DelayTaskService : BabuSingleton<DelayTaskService>
    {
        public delegate void TaskCallback();

        public Coroutine Run(GameObject bindGameObject, TaskCallback callback)
        {
            Coroutine coroutine = StartCoroutine(DelayRun(callback));
            coroutine.AddTo(bindGameObject);
            return coroutine;
        }
        public Coroutine Run(GameObject bindGameObject, float delayTime, TaskCallback callback)
        {
            Coroutine coroutine = StartCoroutine(DelayRun(delayTime, callback));
            coroutine.AddTo(bindGameObject);
            return coroutine;
        }
        public Coroutine RunWithNoBindGameObject(TaskCallback callback)
        {
            return StartCoroutine(DelayRun(callback));
        }
        public Coroutine RunWithNoBindGameObject(float delayTime, TaskCallback callback)
        {
            return StartCoroutine(DelayRun(delayTime, callback));
        }

        public void StopTask(Coroutine coroutine)
        {
            StopCoroutine(coroutine);
        }

        private IEnumerator DelayRun(float delayTime, TaskCallback callback)
        {
            yield return new WaitForSecondsRealtime(delayTime);
            callback?.Invoke();
        }

        private IEnumerator DelayRun(TaskCallback callback)
        {
            yield return null;
            callback?.Invoke();
        }
    }
}