using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Globalization;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using Babu;
using GameConfig;
using System.Linq;
using GameConfig.Config;

namespace BigBang.UI
{
    public class RecruitGiftUI : APanelController
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private ShopGiftPad shoppad;

        //private ArenaExShopUIAnim Anim; 
        //public RecruitGiftUIAnim Anim;

        protected override void AddListeners()
        {
            closeButton.onClick.AddListener(OnClickCloseBtn);
        }

        protected override void RemoveListeners()
        {
            closeButton.onClick.RemoveListener(OnClickCloseBtn);
        }

        

        protected override void OnPropertiesSet(){
            shoppad.giftType = 2;
            shoppad.SetData();
        }
       
        private void OnClickCloseBtn()
        {
            UIController.Instance.HidePanel<RecruitGiftUI>();
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        }
    }
}