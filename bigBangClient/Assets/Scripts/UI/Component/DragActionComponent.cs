using System;
using Spine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DragActionComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
{
    public Action<PointerEventData> DragBeginAction;
    public Action<PointerEventData> DragMoveAction;
    public Action<PointerEventData> DragEndAction;

    int pointerId = -1;
    private void OnEnable()
    {
        pointerId = -1;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (pointerId != -1) return;
        pointerId = eventData.pointerId;
        DragBeginAction?.Invoke(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (pointerId != eventData.pointerId) return;
        DragMoveAction?.Invoke(eventData);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (pointerId != eventData.pointerId) return;
        DragEndAction?.Invoke(eventData);
        pointerId = -1;
    }


    public Action<PointerEventData> PointerClickAction;
    public Action<PointerEventData> PointerDownAction;
    public Action<PointerEventData> PointerUpAction;
    public void OnPointerClick(PointerEventData eventData)
    {
        PointerClickAction?.Invoke(eventData);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        PointerDownAction?.Invoke(eventData);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        PointerUpAction?.Invoke(eventData);
    }
}
