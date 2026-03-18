using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System;
using System.Collections.Generic;

namespace BigBang.UI
{
    public class DragonBoatFestivalHomeHelpUI : AWindowController
    {
        [SerializeField] private BabuButton closeButton = null;
        [SerializeField] private ScrollRect tipScrollView = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeButton.OnClick += OnClickCloseButton;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeButton.OnClick -= OnClickCloseButton;
        }


        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            tipScrollView.enabled = false;
            UnityTimer.Timer.Register(this.gameObject, 0.2f, () =>
            {
                tipScrollView.enabled = true;
                tipScrollView.ScroolToTop(0);
            });
        }

        private void OnClickCloseButton(BabuButton _)
        {
            UIController.Instance.CloseWindow<DragonBoatFestivalHomeHelpUI>();
        }
    }
}