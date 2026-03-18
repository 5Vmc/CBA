using UnityEngine;
using DG.Tweening;
using TMPro;
using Utils;
using System;

namespace BigBang.Animation
{
    public class CreateNamePadAnim : AnimBase
    {
        [SerializeField] private TMP_Text createNameTitle;
        [SerializeField] private TMP_Text createNameInfo;
        [SerializeField] private RectTransform inputField;
        [SerializeField] private RectTransform createNameNextBtn;

        public override void PlayExit(Action callback)
        {
            ClearAnim();
            createNameTitle.DOFade(0, 0.3f);
            inputField.gameObject.DOFade(0, 0.3f);
            createNameNextBtn.gameObject.DOFade(0, 0.3f);
            createNameInfo.DOFade(0, 0.3f).OnComplete(() => callback?.Invoke());
        }
    }
}