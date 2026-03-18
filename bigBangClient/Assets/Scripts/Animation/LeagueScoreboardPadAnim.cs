using UnityEngine;
using BigBang.UI;
using Utils;
using UnityTimer;
using DG.Tweening;

namespace BigBang.Animation
{
    public class LeagueScoreboardPadAnim : AnimBase
    {
        [SerializeField] private LeagueScoreboardAdapter adapter;

        public override void Init()
        {
            base.Init();
            for (int i = 0; i < adapter.VisibleItemsCount; i++)
            {
                var holder = adapter.GetItemViewsHolder(i);
                holder.InitAnim();
            }
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            TouchManager.Instance.DisableTouch();
            AudioManager.Instance.PlaySound(AudioNames.ENT_FLOPS);
            for (int i = 0; i < adapter.VisibleItemsCount; i++)
            {
                var holder = adapter.GetItemViewsHolder(i);
                holder.PlayAnim(i * 0.03f);
            }
            Timer.Register(this.gameObject, adapter.VisibleItemsCount * 0.03f, TouchManager.Instance.EnableTouch);
        }
    }
}