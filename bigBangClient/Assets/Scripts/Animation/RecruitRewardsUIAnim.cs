using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utils;
using TMPro;
using System;
using System.Collections.Generic;
using BigBang.UI;

namespace BigBang.Animation
{
    public class RecruitRewardsUIAnim : AnimBase
    {

        [SerializeField] private RecruitRewardsItemAdapter adapter;
        public override void Init()
        {
            base.Init();
        }

        public void PlayArenaPadAnim()
        {
            AudioManager.Instance.PlaySound(AudioNames.ENT_PLAYER);
            adapter.PlayAnim();
        }

        public override void PlayEnter()
        {
            base.PlayEnter();
            AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);
            
        }

        public override void PlayExit(Action callback)
        {
            base.PlayExit();
            
        }
    }
}