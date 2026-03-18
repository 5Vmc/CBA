using System;
using System.Collections.Generic;
using BigBang.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class GuideTalkUIAnim : AnimBase
    {
        [SerializeField] private BabuButton closeButton = null;
        [SerializeField] private TMP_Text contentText = null;
        [SerializeField] private TMP_Text clickTipText = null;

        private string sourceString;

        public override void Init()
        {
            base.Init();

            contentText.maxVisibleCharacters = 0;
            sourceString = contentText.text;
            closeButton.interactable = false;
            clickTipText.SetAlpha(0);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();

            contentText.DOText(sourceString, sourceString.Length * 0.05f).SetEase(Ease.Linear).OnComplete(() =>
            {
                closeButton.interactable = true;
                clickTipText.TweenAlpha(1, 0.5f);
            });
        }

    }
}
