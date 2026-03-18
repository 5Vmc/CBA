using System.Collections.Generic;
using BigBang.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Utils;

namespace BigBang.Animation
{
    class InventoryUIAnim : PanelBaseAnim
    {
        [SerializeField] private InventoryGridAdapter osa;
        [SerializeField] private GameObject inventorySelectedPad;
        [SerializeField] private GameObject switchMenu;
        public override void PlayContent()
        {
            int count = osa.VisibleItemsCount;
            float delayTime = count * 0.1f;
            inventorySelectedPad.DOFade(1, 0.1f).SetDelay(delayTime);
            switchMenu.DOFade(1, 0.1f).SetDelay(delayTime + 0.1f);
        }

        public override void Init()
        {
            base.Init();
            
            inventorySelectedPad.SetAlpha(0);
            switchMenu.SetAlpha(0);
        }
    }
}