using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utils
{
    public abstract class DragableItem : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [HideInInspector] public RectTransform root;
        private Image rootImg;
        protected UnityAction<PointerEventData> dragAction;
        protected UnityAction dragBeginAction;
        protected UnityAction dragEndAction;
        protected UnityAction<PointerEventData> pointerUpAction;
        protected UnityAction<PointerEventData> pointerDownAction;
        protected UnityAction pointerClickAction;
        //private bool IsDragging = false;
        public virtual void Init()
        {
            root = transform as RectTransform;
            rootImg = root.GetComponent<Image>();
        }

        public void SetRaycastTarget(bool value)
        {
            rootImg.raycastTarget = value;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            SetRaycastTarget(false);
            dragBeginAction?.Invoke();
            //IsDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragAction?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragEndAction?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDownAction?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SetRaycastTarget(true);
            //IsDragging = false;
            pointerUpAction?.Invoke(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            pointerClickAction?.Invoke();
        }
    }
}
