
using DG.Tweening;
using UnityEngine;

namespace BigBang.Animation
{
    public enum AnimLoopType
    {
        Scale = 1,
        UpDown = 2,
        LefRight = 3,
        ClockwiseRotation = 4,
        CounterClockwiseRotation = 5,
        LockShake = 6,
    }
    public class LoopAnim : MonoBehaviour
    {
        [SerializeField] private AnimLoopType loopType;
        [SerializeField] private bool playOnStart;


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

        void Start()
        {
            if (playOnStart == false) return;
            switch (loopType)
            {
                case AnimLoopType.Scale:
                    Scale();
                    break;
                case AnimLoopType.UpDown:
                    UpDown();
                    break;
                case AnimLoopType.LefRight:
                    LefRight();
                    break;
                case AnimLoopType.ClockwiseRotation:
                    ClockwiseRotation();
                    break;
                case AnimLoopType.CounterClockwiseRotation:
                    CounterClockwiseRotation();
                    break;
                case AnimLoopType.LockShake:
                    LockShake();
                    break;
            }
        }
        private void UpDown()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(this.rect.DoRelativeAnchorPosY(2, 0.3f));
            sequence.Append(this.rect.DoRelativeAnchorPosY(-2, 0.3f));
            sequence.SetLoops(-1);
            sequence.AddTo(this.gameObject);
        }

        private void LefRight()
        {

            Sequence sequence = DOTween.Sequence();
            sequence.Append(this.rect.DoRelativeAnchorPosX(4, 0.4f));
            sequence.Append(this.rect.DoRelativeAnchorPosX(0, 0.2f));
            sequence.Append(this.rect.DoRelativeAnchorPosX(-4, 0.4f));
            sequence.Append(this.rect.DoRelativeAnchorPosX(0, 0.2f));
            sequence.SetLoops(-1);
            sequence.AddTo(this.gameObject);
        }

        private void Scale()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(this.rect.DOScale(1.2f, 1f));
            sequence.Append(this.rect.DOScale(1f, 1f)).SetDelay(1f);

            sequence.SetLoops(-1);
            sequence.AddTo(this.gameObject);
        }

        public float rotateSpeed = 1f;
        private void ClockwiseRotation()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(this.rect.DOLocalRotate(new Vector3(0, 0, -360), rotateSpeed, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
            sequence.SetLoops(-1);
            sequence.AddTo(this.gameObject);
        }
        private void CounterClockwiseRotation()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(this.rect.DOLocalRotate(new Vector3(0, 0, 360), rotateSpeed, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
            sequence.SetLoops(-1);
            sequence.AddTo(this.gameObject);
        }

        Sequence lockShakeSeq = null;
        public void LockShake()
        {
            if (lockShakeSeq != null) return;
            lockShakeSeq = DOTween.Sequence();
            //锁上移
            lockShakeSeq.Insert(0, this.rect.DoRelativeAnchorPosY(1.5f, 0.1f));
            //锁旋转
            //左30度
            lockShakeSeq.Append(this.rect.DORotate(Vector3.forward * -30, 0.15f));
            //右30度
            lockShakeSeq.Append(this.rect.DORotate(Vector3.forward * 30, 0.15f));
            //左20度
            lockShakeSeq.Append(this.rect.DORotate(Vector3.forward * -20, 0.15f));
            //右10度
            lockShakeSeq.Append(this.rect.DORotate(Vector3.forward * 10, 0.15f));
            //归位
            lockShakeSeq.Append(this.rect.DORotate(Vector3.zero, 0.15f));
            //锁下移
            lockShakeSeq.Insert(0.6f, this.rect.DoRelativeAnchorPosY(-1.5f, 0.1f));
            lockShakeSeq.AppendInterval(1f);
            lockShakeSeq.SetLoops(-1);
            lockShakeSeq.AddTo(this.gameObject);
        }
        public void ClearLockShake()
        {
            lockShakeSeq?.Kill();
            lockShakeSeq = null;
            if (this.rect != null)
            {
                this.rect.anchoredPosition = anchoredPositionOld;
                this.rect.localRotation = Quaternion.identity;
            }
        }

    }
}