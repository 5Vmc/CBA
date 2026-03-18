using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public class LinkItem : MonoBehaviour
    {
        public RectTransform InPosition;
        public RectTransform OutPosition;

        [SerializeField] private Image startLine;
        [SerializeField] private Image middleLine;
        [SerializeField] private Image endLine;
        [IgnoreNullWarning]
        [SerializeField] private RectTransform target;

        private Camera uiCamera;
        private float lineWidth;
        private Vector3 reverseX = new Vector3(-1, 1, 1);
        private Vector3 reverseY = new Vector3(1, -1, 1);

        private void Awake()
        {
            lineWidth = startLine.rectTransform.sizeDelta.y;
            uiCamera = UIController.Instance.GetCamera();
        }

        // 设置连接的目标
        public void SetTarget(RectTransform target)
        {
            this.target = target;
            startLine.gameObject.SetActive(true);
            middleLine.gameObject.SetActive(true);
            endLine.gameObject.SetActive(true);
        }

        private void Update()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (target == null)
            {
                startLine.gameObject.SetActive(false);
                middleLine.gameObject.SetActive(false);
                endLine.gameObject.SetActive(false);
                return;
            }

            startLine.gameObject.SetActive(true);
            middleLine.gameObject.SetActive(true);
            endLine.gameObject.SetActive(true);

            bool targetIsLeft = OutPosition.position.x < target.position.x;
            bool targetIsTop = OutPosition.position.y < target.position.y;
            float width = Mathf.Abs(uiCamera.WorldToScreenPoint(OutPosition.position).x - uiCamera.WorldToScreenPoint(target.position).x) / 2f + lineWidth / 2;
            float height = Mathf.Abs(uiCamera.WorldToScreenPoint(OutPosition.position).y - uiCamera.WorldToScreenPoint(target.position).y) + (targetIsTop ? lineWidth : 0);
            width /= UIController.Instance.Canvas.scaleFactor;
            height /= UIController.Instance.Canvas.scaleFactor;
            startLine.rectTransform.localScale = targetIsLeft ? Vector3.one : reverseX;
            middleLine.rectTransform.localScale = targetIsTop ? Vector3.one : reverseY;
            startLine.rectTransform.sizeDelta = endLine.rectTransform.sizeDelta = new Vector2(width, lineWidth);
            middleLine.rectTransform.sizeDelta = new Vector2(lineWidth, height);
        }

        // 设置整条线段的颜色
        public void SetLineColor(Color start, Color middle, Color end)
        {
            startLine.color = start;
            middleLine.color = middle;
            endLine.color = end;
        }
    }
}