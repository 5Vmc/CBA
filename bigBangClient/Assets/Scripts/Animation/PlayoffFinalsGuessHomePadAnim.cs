using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.Animation
{
    public class PlayoffFinalsGuessHomePadAnim : AnimBase
    {
        [SerializeField] private List<PlayoffFinalsGuessHomePadPlayerAnim> iconAnimListNorth = new();
        [SerializeField] private List<PlayoffFinalsGuessHomePadPlayerAnim> iconAnimListSouth = new();
        [SerializeField] private GameObject vSRoot = null;
        [SerializeField] private List<GameObject> fadeList = new();


        public override void Init()
        {
            base.Init();
            // 初始化位置
            fadeList.ForEach(go => go?.SetAlpha(0));
            foreach (var item in iconAnimListNorth)
            {
                item?.Init();
            }
            foreach (var item in iconAnimListSouth)
            {
                item?.Init();
            }
            vSRoot?.gameObject.SetAlpha(0);
            vSRoot?.transform.SetLocalScale(0);
        }

        public void PlayEnter(Action endCallback)
        {
            base.PlayEnter();
            float delay = 0.2f;
            for (int i = 0; i < 6; i++)
            {
                if (i < iconAnimListNorth.Count) iconAnimListNorth[i]?.PlayEnter(delay);
                if (i < iconAnimListSouth.Count) iconAnimListSouth[i]?.PlayEnter(delay);
                delay += 0.2f;
            }

            tweens.Add(vSRoot?.gameObject.DOFade(1f, 0.2f).SetDelay(0.45f));
            tweens.Add(vSRoot?.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetDelay(0.45f));
            foreach (var item in fadeList)
            {
                tweens.Add(item?.DOFade(1f, 0.8f).SetDelay(0f));
            }
            tweens.Add(DOTween.To(v => { }, 0, 0, 2.0f).OnComplete(() => endCallback?.Invoke()));
        }
    }
}
