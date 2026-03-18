using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Utils
{
    public abstract class Slot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public UnityAction dropAction;
        public UnityAction pointerEnterAction;
        public UnityAction pointerExitAction;

        public abstract void Init();
        public void OnDrop(PointerEventData eventData)
        {
            dropAction?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointerEnterAction?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerExitAction?.Invoke();
        }
    }

}
