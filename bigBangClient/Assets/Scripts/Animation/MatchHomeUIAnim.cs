using UnityEngine;
using BigBang.UI;
using System.Collections.Generic;

namespace BigBang.Animation
{
    public class MatchHomeUIAnim : AnimBase
    {
        [SerializeField] private List<CompitionItem> compitions;
        public override void Init()
        {
            base.Init();
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            for (int i = 0; i < compitions.Count; i++)
            {
                compitions[i].Anim.PlayEnter(i * 0.1f);
            }
        }
    }
}