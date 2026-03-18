
using UnityEngine;
using DG.Tweening;
using Utils;
using BigBang.UI;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityTimer;
using TMPro;
using UnityEngine.UI;
using System;

namespace BigBang.Animation
{
    public class BigBoxUIAnim : MonoBehaviour
    {
        

        [SerializeField] private Image bigBoxDarkImage = null;
        [SerializeField] private Image bigBoxCloseImage = null;
        [SerializeField] private Image bigBoxOpenImage = null;
        [SerializeField] private TMP_Text bigBoxTipText = null;
        [SerializeField] private BabuButton bigBoxOpenButton = null;
        [SerializeField] private RectTransform bigBoxPanel = null;

        private Sequence bigBoxSeq = null;
        public void PlayBigBoxAnim()
        {
            bigBoxSeq?.Kill();
            AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);
            bigBoxPanel.gameObject.SetAlpha(1);
            bigBoxDarkImage.SetAlpha(0);
            bigBoxCloseImage.SetAlpha(1);
            bigBoxOpenImage.SetAlpha(1);
            RectTransform boxCloseRect = bigBoxCloseImage.GetComponent<RectTransform>();
            boxCloseRect.SetLocalPositionY(-300);
            boxCloseRect.localScale = Vector3.zero;
            bigBoxTipText.SetAlpha(0);
            bigBoxOpenButton?.gameObject.SetAlpha(0);

            bigBoxSeq = DOTween.Sequence();
            bigBoxSeq.Append(bigBoxDarkImage.DOFade(1, 0.3f));
            bigBoxSeq.Append(boxCloseRect.DOScale(1, 0.6f));
            bigBoxSeq.Join(boxCloseRect.DOLocalMoveY(-34, 0.6f).SetEase(Ease.OutBack));
            bigBoxSeq.Append(bigBoxTipText.DOFade(1, 0.3f));
            bigBoxSeq.Append(bigBoxOpenButton.gameObject.DOFade(1, 0.3f));
        }
        public void HideBigBoxAnim(Action callback = null)
        {
            bigBoxSeq?.Kill();
            bigBoxSeq = DOTween.Sequence();
            bigBoxSeq.Append(bigBoxPanel.gameObject.DOFade(0, 0.3f));
            bigBoxSeq.AppendCallback(() =>
            {
                bigBoxPanel.gameObject.SetActive(false);
                callback?.Invoke();
            });
        }

    }
}