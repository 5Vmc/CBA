using UnityEngine;
using BigBang.UI;

namespace BigBang.Animation
{
    public class MyCoursePadAnim : AnimBase
    {
        [SerializeField] private MyCoursePadAdapter adapter;

        public override void Init()
        {
            base.Init();
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            for (int i = 0; i < adapter.VisibleItemsCount; i++)
            {
                adapter.GetItemViewsHolder(i).PlayAnim(i * 0.03f);
            }
        }
    }
}