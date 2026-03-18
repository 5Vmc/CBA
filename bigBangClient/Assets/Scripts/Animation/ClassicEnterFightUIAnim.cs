using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using System;
using System.Collections.Generic;
using BigBang.UI;

namespace BigBang.Animation
{
    public class ClassicEnterFightUIAnim : AnimBase
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private Image blackImg;

        [SerializeField] private List<ClassicFightEnterPlayerItem> playerItemListBlue;
        [SerializeField] private List<ClassicFightEnterPlayerItem> playerItemListRed;

        [SerializeField] private RectTransform fightPointBluePanel = null;
        [SerializeField] private RectTransform fightPointRedPanel = null;
        [SerializeField] private ImageFont fightPointNumImageFontBlue = null;
        [SerializeField] private ImageFont fightPointNumImageFontRed = null;

        [SerializeField] private Image clubIconBlue = null;
        [SerializeField] private Image clubIconRed = null;
        [SerializeField] private Image fightMidImage = null;
        [SerializeField] private Image fightLeftImage = null;
        [SerializeField] private Image fightRightImage = null;

        [SerializeField] private RectTransform linePanel = null;

        private readonly int fightPointPanelX = 270;
        public override void Init()
        {
            base.Init();
            blackImg.SetAlpha(0);
            panel.localScale = Vector3.zero;
            for (int i = 0; i < 5; i++)
            {
                ClassicFightEnterPlayerItem itemBlue = playerItemListBlue[i];
                ClassicFightEnterPlayerItem itemRed = playerItemListRed[i];
                itemBlue.PlayInit();
                itemRed.PlayInit();
            }
            fightPointBluePanel.gameObject.SetAlpha(0);
            fightPointRedPanel.gameObject.SetAlpha(0);
            fightPointBluePanel.SetLocalPositionX(-fightPointPanelX - 100);
            fightPointRedPanel.SetLocalPositionX(fightPointPanelX + 100);
            fightPointNumImageFontBlue.text = "0";
            fightPointNumImageFontRed.text = "0";
            clubIconBlue.transform.localScale = Vector3.zero;
            clubIconRed.transform.localScale = Vector3.zero;
            clubIconBlue.SetAlpha(1);
            clubIconRed.SetAlpha(1);
            fightMidImage.SetAlpha(0);
            fightLeftImage.SetAlpha(0);
            fightLeftImage.transform.SetLocalPositionX(-15.6f - 10f);
            fightRightImage.SetAlpha(0);
            fightRightImage.transform.SetLocalPositionX(21.6f + 10f);
            linePanel.gameObject.SetAlpha(0);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            // 面板弹出音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_POP);
            Sequence sequence = DOTween.Sequence();
            tweens.Add(sequence);

            sequence.Append(panel.DOScale(1, 0.15f));
            sequence.Join(blackImg.DOFade(0.5f, 0.15f));

            sequence.Append(clubIconBlue.transform.DOScale(0.8f, 0.3f).SetEase(Ease.OutBack));
            sequence.Join(clubIconBlue.DOFade(0.2f, 0.3f));
            sequence.Join(clubIconRed.transform.DOScale(0.8f, 0.3f).SetEase(Ease.OutBack));
            sequence.Join(clubIconRed.DOFade(0.2f, 0.3f));

            sequence.Append(fightLeftImage.transform.DOLocalMoveX(-15.6f, 0.3f).SetEase(Ease.OutBack));
            sequence.Join(fightLeftImage.DOFade(1f, 0.3f));
            sequence.Join(fightRightImage.transform.DOLocalMoveX(21.6f, 0.3f).SetEase(Ease.OutBack));
            sequence.Join(fightRightImage.DOFade(1f, 0.3f));

            sequence.Insert(sequence.Duration() - 0.1f, fightMidImage.DOFade(1, 0.15f));
            sequence.Insert(sequence.Duration() - 0.1f, linePanel.gameObject.DOFade(1, 0.15f));


            int blueTotalFightPoint = 0;
            int redTotalFightPoint = 0;
            for (int i = 0; i < 5; i++)
            {
                if (i == 0)
                {
                    sequence.AppendInterval(0.05f);
                }
                else
                {
                    sequence.AppendInterval(0.15f);
                }
                ClassicFightEnterPlayerItem itemBlue = playerItemListBlue[i];
                ClassicFightEnterPlayerItem itemRed = playerItemListRed[i];
                blueTotalFightPoint += itemBlue.fightPoint;
                redTotalFightPoint += itemRed.fightPoint;
                sequence.AppendCallback(() =>
                {
                    itemBlue.PlayEnter();
                    itemRed.PlayEnter();
                });
            }

            sequence.AppendInterval(0.6f);

            sequence.Append(fightPointBluePanel.gameObject.DOFade(1, 0.3f));
            sequence.Join(fightPointBluePanel.DOLocalMoveX(-fightPointPanelX, 0.3f).SetEase(Ease.OutBack));
            sequence.Join(fightPointRedPanel.gameObject.DOFade(1, 0.3f));
            sequence.Join(fightPointRedPanel.DOLocalMoveX(fightPointPanelX, 0.3f).SetEase(Ease.OutBack));

            sequence.Append(fightPointNumImageFontBlue.DOChangeNumber(blueTotalFightPoint, 0.3f).SetEase(Ease.Linear));
            sequence.Join(fightPointNumImageFontRed.DOChangeNumber(redTotalFightPoint, 0.3f).SetEase(Ease.Linear));
        }

        public override void PlayExit(Action callback)
        {
            base.PlayExit();
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            tweens.Add(panel.DOScale(0, 0.15f));
            tweens.Add(blackImg.DOFade(0, 0.15f).OnComplete(() =>
            {
                callback?.Invoke();
            }));
        }
    }
}