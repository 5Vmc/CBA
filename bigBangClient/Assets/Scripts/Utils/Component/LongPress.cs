using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class LongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action Press;
    public event Action Release;
    public event Action Click;

    private bool isPress = false;
    private bool isEnter = false;

    private void Update()
    {
        if (isPress)
        {
            Press?.Invoke();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPress = false;
        Release?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPress = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isEnter = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isEnter = false;
    }
}
