using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 长按触发器
/// </summary>
[RequireComponent(typeof(Image))]
public class LongPressTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool IsDown = false;
    private float sumTime = 0;

    // 长按事件(触发1次)
    public event Action OnLongPress;
    // 按下开始
    public event Action OnPressStart;
    // 松开事件
    public event Action OnRelease;
    // 长按触发时间
    public float TriggerTime = 3;
    // 长按进度
    public float Progress = 0;

    public void OnPointerDown(PointerEventData eventData)
    {
        IsDown = true;
        OnPressStart?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsDown = false;
        OnRelease?.Invoke();
        sumTime = 0;
        Progress = 0;
    }

    private void Update()
    {
        if (IsDown)
        {
            Progress = sumTime / TriggerTime;
            if (sumTime >= TriggerTime)
            {
                OnLongPress?.Invoke();
                sumTime = 0;
                IsDown = false;
            }
            sumTime += Time.deltaTime;
        }
    }
}
