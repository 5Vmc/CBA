using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class ArenaEndRewardUIAnim : AnimBase
    {
        [SerializeField] private TMP_Text CloseTipText = null;

        public override void Init()
        {
            base.Init();
            CloseTipText.SetAlpha(0);
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            tweens.Add(CloseTipText.DOFade(1f, 0.5f));
        }
    }
}