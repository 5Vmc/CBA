using UnityEngine;
using UnityEngine.EventSystems;

namespace Utils
{
    public abstract class DragableItemManager : MonoBehaviour //: BabuSingleton<DragableItemManager>
    {
        [SerializeField] protected RectTransform dragLayer;
        public bool IsDragging { get; private set; } = false;
        public DragableItem DraggingItem { get; private set; }

        public virtual void PickItem(PointerEventData eventData, DragableItem item)
        {
            if (IsDragging)
                return;

            IsDragging = true;
            DraggingItem = item;
            DraggingItem.root.SetParent(dragLayer);
        }

        public virtual void DropItem(PointerEventData eventData, DragableItem item)
        {
            DraggingItem = null;
            IsDragging = false;
        }

        public virtual void DragItem(PointerEventData eventData, DragableItem item)
        {
            if (!IsDragging) return;
            DraggingItem.root.position = eventData.pointerCurrentRaycast.worldPosition;
        }
    }
}
