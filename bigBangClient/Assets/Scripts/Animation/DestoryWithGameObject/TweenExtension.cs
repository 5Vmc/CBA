using DG.Tweening;
using UnityEngine;

public static partial class TweenExtension
{
    public static T AddTo<T>(this T disposable, GameObject targetObJ) where T : Tween
    {
        var markTest = targetObJ.GetComponent<DestroyTweenTrigger>();
        if (markTest == null)
        {
            markTest = targetObJ.AddComponent<DestroyTweenTrigger>();
        }
        markTest.AllTweenList.Add(disposable);
        return disposable;
    }
}