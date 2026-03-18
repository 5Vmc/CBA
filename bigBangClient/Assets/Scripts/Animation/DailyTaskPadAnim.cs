using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Utils;
using BigBang.UI;
using System.Collections.Generic;
using UnityTimer;

namespace BigBang.Animation
{
    public class DailyTaskPadAnim : AnimBase
    {
        [SerializeField] private RectTransform titles;
        [SerializeField] private TaskProgressItem taskProgressItem;
        [SerializeField] private TaskUIAdapter adapter;
        [SerializeField] private List<GameObject> fadeGroup;

        public override void Init()
        {
            base.Init();
            adapter.InitAnim();
            taskProgressItem.Anim.Init();
        }

        public void PlayEnter(int point, bool savePad = false)
        {
            Init();
            AudioManager.Instance.PlaySound(AudioNames.ENT_REG);
            TouchManager.Instance.DisableTouch();
            Timer.Register(this.gameObject, 1, TouchManager.Instance.EnableTouch);

            if (savePad)
            {
                // 标题淡入
                titles.gameObject.DOFade(1, 0.2f).OnComplete(() =>
                {
                    // 任务块放大
                    adapter.PlayAnim();
                });
            }
            else
            {
                // 整体淡入
                this.gameObject.DOFade(1, 0.2f).OnComplete(() =>
                {
                    // 任务块放大
                    adapter.PlayAnim();
                });
            }

            taskProgressItem.Anim.PlayAnim(point);
        }

        public override void PlayExit(Action callback)
        {
            adapter.PlayExit();
            taskProgressItem.Anim.PlayExit();
            this.gameObject.DOFade(0, 0.2f).OnComplete(() => { callback?.Invoke(); });
        }

        public void PlayFade(Action callback)
        {
            fadeGroup.ForEach(item => item.DOFade(0, 0.05f));
            Timer.Register(this.gameObject, 0.05f, () =>
            {
                callback?.Invoke();
                fadeGroup.ForEach(item => item.DOFade(1, 0.3f));
            });
        }
    }
}