using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace BigBang.UI
{
    public class ColorSelectItem : MonoBehaviour
    {
        [SerializeField] private Drawable cursor;
        [SerializeField] private Image selection;
        [SerializeField] private RectTransform colorRect;
        [SerializeField] private List<RectTransform> colorList;
        public event Action<int, int> OnValueChanged;
        private int selectionIndex = 0;

        public event Action OnReleaseItem;

        private void OnEnable()
        {
            cursor.OnDragStart += OnDragStart;
            cursor.OnDrag += OnDrag;
            cursor.OnRelease += OnRelease;
        }

        private void OnDisable()
        {
            cursor.OnDragStart -= OnDragStart;
            cursor.OnDrag -= OnDrag;
            cursor.OnRelease -= OnRelease;
        }

        private void OnDragStart(Drawable sender)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_2);
        }

        private void OnRelease()
        {
            // 指针移动到选定的颜色位置
            cursor.Rect.DOAnchorPosX(colorList[selectionIndex].anchoredPosition.x, 0.05f);
            OnReleaseItem?.Invoke();
        }

        private void OnDrag(Drawable drawable)
        {
            Vector3[] corners = new Vector3[4];
            colorRect.GetWorldCorners(corners);
            // 限定拖动范围
            if (drawable.Rect.position.x < corners[0].x)
            {
                drawable.Rect.position = new Vector3(corners[0].x, drawable.Rect.position.y, drawable.Rect.position.z);
            }
            if (drawable.Rect.position.x > corners[3].x)
            {
                drawable.Rect.position = new Vector3(corners[3].x, drawable.Rect.position.y, drawable.Rect.position.z);
            }
            var index = colorList.FindIndex(item => Mathf.Abs(item.anchoredPosition.x - drawable.Rect.anchoredPosition.x) <= 33.5f);
            if (index != -1 && index != selectionIndex)
            {
                selection.rectTransform.DOAnchorPosX(colorList[index].anchoredPosition.x, 0.05f);
                OnValueChanged?.Invoke(selectionIndex, index);
                selectionIndex = index;
            }
        }

        public void SetSelection(int index)
        {
            if (index != -1 && selectionIndex != index)
            {
                cursor.Rect.DOKill();
                selection.rectTransform.DOAnchorPosX(colorList[index].anchoredPosition.x, 0.05f);
                cursor.Rect.DOAnchorPosX(colorList[index].anchoredPosition.x, 0.05f);
                OnValueChanged?.Invoke(selectionIndex, index);
                selectionIndex = index;
            }
        }
    }
}
