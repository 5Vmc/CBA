
using DG.Tweening;
using UnityEngine;

namespace Babu
{
    public enum AnimLoopType
    {
        Scale = 1,
        ClockwiseRotation = 4,
        CounterClockwiseRotation = 5,
    }
    public class LoopAnimInInit : MonoBehaviour
    {
        [SerializeField] private AnimLoopType loopType;

        private RectTransform rect;
        private float oldX;
        private float oldY;
        private Vector2 anchoredPositionOld;
        void Awake()
        {
            this.rect = gameObject.GetComponent<RectTransform>();
            oldX = this.rect.position.x;
            oldY = this.rect.position.y;
            anchoredPositionOld = this.rect.anchoredPosition;
        }

        private void OnEnable()
        {
            switch (loopType)
            {
                case AnimLoopType.Scale:
                    Scale();
                    break;
                case AnimLoopType.ClockwiseRotation:
                    ClockwiseRotation();
                    break;
                case AnimLoopType.CounterClockwiseRotation:
                    CounterClockwiseRotation();
                    break;
            }
        }
        private void OnDisable()
        {
            sequence?.Kill();
            sequence = null;
        }

        Sequence sequence = null;
        private void Scale()
        {
            sequence = DOTween.Sequence();
            sequence.Append(this.rect.DOScale(1.2f, 1f));
            sequence.Append(this.rect.DOScale(1f, 1f)).SetDelay(1f);

            sequence.SetLoops(-1);
        }

        private void ClockwiseRotation()
        {
            sequence = DOTween.Sequence();
            sequence.Append(this.rect.DOLocalRotate(new Vector3(0, 0, -360), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
            sequence.SetLoops(-1);
        }
        private void CounterClockwiseRotation()
        {
            sequence = DOTween.Sequence();
            sequence.Append(this.rect.DOLocalRotate(new Vector3(0, 0, 360), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
            sequence.SetLoops(-1);
        }

    }
}