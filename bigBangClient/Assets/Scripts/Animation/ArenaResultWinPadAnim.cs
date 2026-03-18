
using UnityEngine;
using DG.Tweening;
using Utils;
using BigBang.UI;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityTimer;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Coffee.UIEffects;

namespace BigBang.Animation
{
    public class ArenaResultWinPadAnim : AnimBase
    {
        public Action onAniEnd;

        [SerializeField] private List<Image> FadeInImageList;
        [SerializeField] private RectTransform WinTextBgImageTrans;
        [SerializeField] private List<RectTransform> scaleTransListY;
        [SerializeField] private UIShiny shinyWin;

        [SerializeField] private RectTransform UpStageTitlePanelTrans;
        [SerializeField] private RectTransform BadgeImageTransOld;
        [SerializeField] private RectTransform BadgeImageTransNew;
        [SerializeField] private UIShiny shinyBadge;

        public override void Init()
        {
            base.Init();

            shinyWin.Stop();
            shinyBadge.Stop();
            foreach (Image fadeInImage in FadeInImageList)
            {
                fadeInImage.SetAlpha(0);
            }
            int oldSatge = Player.BattleManager.oldArenaInfo.ArenaStage;
            int newSatge = Player.BattleManager.newArenaInfo.ArenaStage;
            bool isUpSatge = oldSatge < newSatge;

            if (isUpSatge == false)
            {
                WinTextBgImageTrans.gameObject.SetAlpha(0);
                WinTextBgImageTrans.localScale = Vector3.one * 5.0f;
            }
            else
            {
                UpStageTitlePanelTrans.gameObject.SetAlpha(0);
                UpStageTitlePanelTrans.localScale = Vector3.one * 5.0f;
                BadgeImageTransOld.SetLocalScaleX(1f);
                BadgeImageTransNew.SetLocalScaleX(0f);
            }

            foreach (RectTransform sclaeTrans in scaleTransListY)
            {
                sclaeTrans.localScale = new Vector3(1, 0, 1);
            }
        }

        public override void PlayEnter()
        {
            base.PlayEnter();

            AudioManager.Instance.PlayMusic(AudioNames.BGM_HOME);

            foreach (Image fadeInImage in FadeInImageList)
            {
                tweens.Add(fadeInImage.DOFade(1, 0.5f).SetDelay(0.4f));
            }

            int oldSatge = Player.BattleManager.oldArenaInfo.ArenaStage;
            int newSatge = Player.BattleManager.newArenaInfo.ArenaStage;
            bool isUpSatge = oldSatge < newSatge;

            tweens.Add(DOTween.Sequence().AppendInterval(0.0f).AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON); }));
            tweens.Add(DOTween.Sequence().AppendInterval(0.4f + 0.3f + 0.1f).AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.EVENT_COMMONHIT); }));

            float hitTime = 0f;
            if (isUpSatge == false)
            {
                tweens.Add(WinTextBgImageTrans.gameObject.DOFade(1, 0.3f).SetDelay(0.4f + 0.3f));
                tweens.Add(WinTextBgImageTrans.DOScale(1, 0.6f).SetDelay(0.4f + 0.3f).SetEase(Ease.OutBack));
                hitTime = 0.4f + 0.3f + 0.6f;
            }
            else
            {
                tweens.Add(UpStageTitlePanelTrans.gameObject.DOFade(1, 0.3f).SetDelay(0.4f + 0.3f));
                tweens.Add(UpStageTitlePanelTrans.DOScale(1, 0.6f).SetDelay(0.4f + 0.3f).SetEase(Ease.OutBack));

                Sequence rotateSeq = DOTween.Sequence();
                rotateSeq.AppendInterval(0.4f + 0.3f + 0.6f);
                rotateSeq.AppendInterval(0.3f);
                rotateSeq.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.EVENT_BREAK); });
                rotateSeq.Append(BadgeImageTransOld.DOScaleX(0f, 0.8f).SetEase(Ease.InBack));
                rotateSeq.Append(BadgeImageTransNew.DOScaleX(1f, 0.8f).SetEase(Ease.OutBack));
                rotateSeq.AppendInterval(0.3f);
                tweens.Add(rotateSeq);
                hitTime = rotateSeq.Duration();
            }

            Sequence scaleYSeq = DOTween.Sequence();
            scaleYSeq.AppendInterval(hitTime);
            int scaleTransListYShowCount = 0;
            for (int i = 0; i < scaleTransListY.Count; i++)
            {
                if (scaleTransListY[i].gameObject.activeSelf == false) continue;
                scaleTransListYShowCount++;
                scaleYSeq.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP); });
                scaleYSeq.Append(scaleTransListY[i].DOScaleY(1, 0.15f).SetEase(Ease.OutBack));
            }
            tweens.Add(scaleYSeq);

            Sequence waitSeq = DOTween.Sequence();
            waitSeq.AppendInterval(hitTime + scaleTransListYShowCount * 0.15f);
            waitSeq.AppendCallback(() =>
            {
                onAniEnd?.Invoke();
                shinyWin.Play(true);
                shinyBadge.Play(true);
            });
            tweens.Add(waitSeq);

        }
    }
}