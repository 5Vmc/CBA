using System;
using BigBang.Animation;
using BigBang.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class DragonYearRedEnvelopeTipUIAnim : MonoBehaviour
{
    [SerializeField] private Sequence sequence = null;
    [SerializeField] private RectTransform redEnvelopeImage = null;
    [SerializeField] private RectTransform moveTrans = null;

    public void OnEnable()
    {
        PlayShowAni();
    }

    public void PlayShowAni()
    {
        sequence?.Kill();
        sequence = DOTween.Sequence();
        sequence.AddTo(this.gameObject);
        moveTrans.SetAnchoredPositionY(180f);
        redEnvelopeImage.SetLocalRotationZ(0f);

        sequence.Append(moveTrans.DOAnchorPosY(-180f, 1.0f).SetEase(Ease.OutBack));
        sequence.Append(redEnvelopeImage.DOSpin(15, 6, 0.1f));
        sequence.AppendInterval(0.4f);
        sequence.Append(redEnvelopeImage.DOSpin(15, 6, 0.1f));
        sequence.AppendInterval(0.4f);
        sequence.Append(redEnvelopeImage.DOSpin(15, 6, 0.1f));
        sequence.AppendInterval(0.4f);
        sequence.Append(moveTrans.DOAnchorPosY(180f, 1.0f).SetEase(Ease.InBack));
        sequence.AppendCallback(() => { UIController.Instance.CloseWindow<DragonYearRedEnvelopeTipUI>(); });
    }

}
