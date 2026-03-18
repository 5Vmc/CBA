using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DragComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private bool isDragging = false;
    private Vector2 startPos;
    public UnityAction dragBeginAction;
    public UnityAction<float> dragAction;
    public UnityAction<bool> dragEndAction;
    public event Action OnRelease;
    public float draggingTime = 0;
    private float TheLastOffset;//记录 拖拽中阶段 最后的差值  用于判定拖拽结束的临界值

    public bool IsDragging { get => isDragging; }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        startPos = eventData.pointerCurrentRaycast.screenPosition;
        dragBeginAction?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;
        var pos = eventData.pointerCurrentRaycast.screenPosition;
        float offset = pos.y - startPos.y;
        dragAction?.Invoke(offset);
        startPos = eventData.pointerCurrentRaycast.screenPosition;
        TheLastOffset = offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float offset = eventData.pointerCurrentRaycast.screenPosition.y - startPos.y;
        if (draggingTime < 0.3f && (offset != 0))
        {
            dragEndAction?.Invoke(offset < 0);
        }
        else if (offset == 0 && (draggingTime < 0.3f))
        {

            var up = TheLastOffset < 0 ? true : false;
            dragEndAction?.Invoke(up);
        }

        isDragging = false;
        draggingTime = 0;
        OnRelease?.Invoke();
    }

    private void Update()
    {
        if (isDragging)
            draggingTime += Time.deltaTime;
    }
}
