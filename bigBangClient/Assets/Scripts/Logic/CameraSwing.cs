using UnityEngine;
using DG.Tweening;
using UnityTimer;

public class CameraSwing : MonoBehaviour
{
    private Vector3 originLocalRotation;
    private Timer timer;
    Tween tween;

    private void Awake()
    {
        originLocalRotation = transform.localRotation.eulerAngles;
    }

    private void Start()
    {
        PlayAnim(true);
    }

    private void PlayAnim(bool direction)
    {
        var targerRotation = originLocalRotation + new Vector3(0, 1 * (direction ? 1 : -1), 0);
        tween = transform.DOLocalRotateQuaternion(Quaternion.Euler(targerRotation), 5).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            timer = Timer.Register(this.gameObject, 2, () =>
            {
                PlayAnim(!direction);
            }, autoDestroyOwner: this);
        });
    }

    private void OnDestroy()
    {
        tween?.Kill();
        timer?.Cancel();
    }
}
