using BigBang.UI;
using Coffee.UIEffects;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;
using Utils;

namespace BigBang.Animation
{
    public class SevenDaysLoginUIAnim : AnimBase
    {
        [SerializeField] private RectTransform title;
        [SerializeField] private List<SevenDayRewardItem> itemList;
        [SerializeField] private RectTransform obtainBtn;

        private Timer timer1;
        private Timer timer2;

        private void OnDisable()
        {
            timer1?.Cancel();
            timer2?.Cancel();
        }

        public override void Init()
        {
            base.Init();
            // 初始化透明度
            title.gameObject.SetAlpha(0);
            obtainBtn.gameObject.SetAlpha(0);
            foreach (var item in itemList)
            {
                item.gameObject.SetAlpha(0);
                item.transform.localScale = Vector3.one * 0.5f;
            }
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            AudioManager.Instance.PlaySound(AudioNames.TECHBOARD_POP);
            // 标题淡入
            title.gameObject.DOFade(1, 0.2f).OnComplete(() =>
            {
                // 放大淡入
                foreach (var item in itemList)
                {
                    item.gameObject.DOFade(1, 0.3f);
                    item.transform.DOScale(1, 0.3f);
                }
                timer1 = Timer.Register(this.gameObject, 0.3f, () =>
                {
                    obtainBtn.gameObject.DOFade(1, 0.3f);
                    foreach (var item in itemList)
                    {
                        item.PlayShiny(0, false);
                    }
                });
                timer2 = Timer.Register(this.gameObject, 3, () =>
                {
                    for (int i = 0; i < itemList.Count; i++)
                    {
                        itemList[i].PlayShiny(i * 0.2f, true);
                    }
                });
            });
        }

        public override void PlayExit(Action callback)
        {
            ClearAnim();
            TouchManager.Instance.DisableTouch();
            title.gameObject.DOFade(0, 0.3f);
            itemList.ForEach(item => item.gameObject.DOFade(0, 0.3f));
            obtainBtn.gameObject.DOFade(0, 0.3f).OnComplete(() =>
            {
                TouchManager.Instance.EnableTouch();
                callback?.Invoke();
            });
        }
    }
}