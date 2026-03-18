using UnityEngine;
using BigBang.UI;
using Utils;
using UnityTimer;

namespace BigBang.Animation
{
    public class LeagueCoursePadAnim : AnimBase
    {
        [SerializeField] private LeagueCourseAdapter adapter;

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
            for (int i = 0; i < adapter.VisibleItemsCount; i++)
            {
                var holder = adapter.GetItemViewsHolder(i);
                holder.PlayAnim(i * 0.03f);
            }
            Timer.Register(this.gameObject, adapter.VisibleItemsCount * 0.03f, TouchManager.Instance.EnableTouch);
        }
    }
}