using BigBang.Animation;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class SkillTrainRoomPad : MonoBehaviour
    {
        [SerializeField] private List<SkillTrainRoomItem> skillTrainRoomList;
        [SerializeField] private RectTransform top;
        public static System.Action talkAllowClick;

        private void OnEnable()
        {
            skillTrainRoomList.ForEach(item => item.OnLockChanged += SetLockVisible);
        }

        private void OnDisable()
        {
            skillTrainRoomList.ForEach(item => item.OnLockChanged -= SetLockVisible);
        }

        public void SetData()
        {
            if(CardUI.isTurnSkillTrainOnce)
            {
                gameObject.SetAlpha(0);
                gameObject.DOFade(1, 0.3f);
            }
            bool flag = true;
            foreach (var item in skillTrainRoomList)
            {
                item.SetLockVisible(flag);
                item.SetData();
                if(CardUI.isTurnSkillTrainOnce)
                {
                    item.PlayFadeInAnim();
                    CardUI.isTurnSkillTrainOnce = false;
                }               
                flag = item.IsUnlock();
            }
        }

        private void SetLockVisible()
        {
            bool flag = true;
            foreach (var item in skillTrainRoomList)
            {
                item.SetLockVisible(flag);
                flag = item.IsUnlock();
            }
        }
        
    }
}