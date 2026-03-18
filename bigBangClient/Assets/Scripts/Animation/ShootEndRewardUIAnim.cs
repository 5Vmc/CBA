using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class ShootEndRewardUIAnim : AnimBase
    {
        [SerializeField] private TMP_Text CloseTipText = null;
        [SerializeField] private RectTransform SuccessPanelTrans = null;
        [SerializeField] private RectTransform LosePanelTrans = null;
        [SerializeField] private List<RectTransform> scaleItemList = new();
        [SerializeField] private Image darkImage;

        public override void Init()
        {
            base.Init();

            darkImage.SetAlpha(0);

            RectTransform titlePanelTrans = isWin ? SuccessPanelTrans : LosePanelTrans;
            titlePanelTrans.gameObject.SetAlpha(0);
            titlePanelTrans.localScale = Vector3.one * 5.0f;

            CloseTipText.SetAlpha(0);
            foreach (RectTransform scaleItem in scaleItemList)
            {
                if (scaleItem.gameObject.activeSelf == false) continue;
                scaleItem.SetLocalScaleY(0);
            }
        }

        private bool isWin = true;
        public void SetData(bool isWin)
        {
            this.isWin = isWin;
        }

        public override void PlayEnter()
        {
            base.PlayEnter();

            Sequence seq = DOTween.Sequence();

            tweens.Add(darkImage.DOFade(0.94f, 0.5f).SetDelay(0.4f));
            seq.AppendInterval(0.4f + 0.5f);
            seq.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.EVENT_COMMONHIT); });

            RectTransform titlePanelTrans = isWin ? SuccessPanelTrans : LosePanelTrans;
            seq.Append(titlePanelTrans.gameObject.DOFade(1, 0.3f));
            seq.Join(titlePanelTrans.DOScale(1, 0.6f).SetEase(Ease.OutBack));

            foreach (RectTransform scaleItem in scaleItemList)
            {
                if (scaleItem.gameObject.activeSelf == false) continue;
                seq.AppendCallback(() =>
                {
                    AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP);
                });
                seq.Append(scaleItem.DOScaleY(1f, 0.1f).SetEase(Ease.OutBack));
                tweens.Add(seq);
            }
            seq.AppendCallback(OnAniEnd);

            tweens.Add(CloseTipText.DOFade(1f, 0.5f).SetDelay(seq.Duration()));
        }
        private void OnAniEnd()
        {
            TouchManager.Instance.EnableTouch();
        }
    }
}