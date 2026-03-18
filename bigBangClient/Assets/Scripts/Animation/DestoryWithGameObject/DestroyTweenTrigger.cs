
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTweenTrigger : MonoBehaviour
{
    public List<Tween> AllTweenList = new List<Tween>();

    public void ClearAllTween()
    {
        for (int i = 0; i < AllTweenList.Count; i++)
        {
            var tempTween = AllTweenList[i];
            if (tempTween.IsActive())
            {
                tempTween.Kill();
            }
        }
        AllTweenList.Clear();
    }

    private void OnDestroy()
    {
        ClearAllTween();
    }
}
