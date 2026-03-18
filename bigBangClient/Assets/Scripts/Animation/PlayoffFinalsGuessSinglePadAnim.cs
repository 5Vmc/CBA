using System;
using System.Collections;
using System.Collections.Generic;
using BigBang.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class PlayoffFinalsGuessSinglePadAnim : AnimBase
    {
        [SerializeField] private PlayoffFinalsGuessSingleAdapter playoffFinalsGuessSingleAdapter = null;
        [SerializeField] private List<GameObject> fadeList = new();


        public override void Init()
        {
            base.Init();
            // 初始化位置
            fadeList.ForEach(go => go?.SetAlpha(0));
            playoffFinalsGuessSingleAdapter.InitAnim();
        }

        public void PlayEnter(Action endCallback)
        {
            base.PlayEnter();
            foreach (var item in fadeList)
            {
                tweens.Add(item?.DOFade(1f, 0.8f).SetDelay(0f));
            }
            playoffFinalsGuessSingleAdapter.PlayAnim();
            tweens.Add(DOTween.To(v => { }, 0, 0, 1.0f).OnComplete(() => endCallback?.Invoke()));
        }
    }
}
