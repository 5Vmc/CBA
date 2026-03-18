using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using Utils;
using BigBang.UI;
using UnityTimer;

namespace BigBang.Animation
{
    public class Guide2UIAnim : AnimBase
    {
        [SerializeField] private List<RectTransform> portraits;
        [SerializeField] private List<GuideSelectionItem> selectionItem;
        [SerializeField] private RectTransform topRect;
        [SerializeField] private CanvasGroup selectionCanvas;

        private RectTransform rect;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        public override void Init()
        {
            base.Init();
            rect.SetAnchoredPositionX(1000);
            topRect.SetAnchoredPositionY(100);
            portraits.ForEach(item =>
            {
                item.gameObject.SetAlpha(0);
                item.localScale = Vector3.one * 0.3f;
            });
        }

        public void PlayEnter(Action callback)
        {
            Init();
            AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);
            // 右移
            rect.DOAnchorPosX(0, 0.3f).OnComplete(() =>
            {
                // 顶条从上方移入
                topRect.DOAnchorPosY(-172f, 0.3f).OnComplete(() =>
                {
                    AudioManager.Instance.PlaySound(AudioNames.ENT_FLOPS);
                    for (int i = 0; i < portraits.Count; i++)
                    {
                        portraits[i].DOScale(1, 0.3f).SetDelay(i * 0.1f);
                        portraits[i].gameObject.DOFade(1, 0.3f).SetDelay(i * 0.1f);
                    }
                    Timer.Register(this.gameObject, 0.8f, callback);
                });
            });
        }

        public void HideSelection()
        {
            selectionCanvas.gameObject.SetActive(false);
            selectionCanvas.interactable = false;
            selectionCanvas.alpha = 0;
        }

        public void ShowSelection()
        {
            AudioManager.Instance.PlaySound(AudioNames.MATCHVSINFO_POP);
            selectionCanvas.gameObject.SetActive(true);
            selectionCanvas.interactable = true;
            selectionCanvas.alpha = 1;
            for (int i = 0; i < selectionItem.Count; i++)
            {
                selectionItem[i].PlayEnter(i * 0.1f);
            }
        }

        public override void PlayExit(Action callback)
        {
            rect.DOAnchorPosX(1000, 0.3f).OnComplete(() => callback?.Invoke());
        }
    }
}