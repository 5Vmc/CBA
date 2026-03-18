using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class HundredGuessUIAnim : AnimBase
    {
        [SerializeField] private RectTransform loadingPanel = null;
        [SerializeField] private RectTransform contentPanel = null;
        [SerializeField] private RectTransform switchBgImage = null;

        public override void Init()
        {
            base.Init();

            loadingPanel.gameObject.SetActive(true);
            contentPanel.gameObject.SetActive(false);
            switchBgImage.gameObject.SetActive(false);
            loadingPanel.gameObject.SetAlpha(1);
            contentPanel.gameObject.SetAlpha(0);
            switchBgImage.gameObject.SetAlpha(0);
            switchBgImage.SetAnchoredPositionY(-578.4f);
        }

        public void PlayEnter(bool showSwitch)
        {
            base.PlayEnter();

            contentPanel.gameObject.SetActive(true);
            if(showSwitch) switchBgImage.gameObject.SetActive(true);
            tweens.Add(loadingPanel.gameObject.DOFade(0f, 0.2f));
            tweens.Add(contentPanel.gameObject.DOFade(1f, 0.5f));
            if (showSwitch) tweens.Add(switchBgImage.gameObject.DOFade(1f, 0.5f));
            if (showSwitch) tweens.Add(switchBgImage.DOAnchorPosY(-540.7f, 0.5f));
        }
    }
}
