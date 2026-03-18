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
    public class HundredHelpUI : AWindowController
    {
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private Button closeBtn = null;
        [SerializeField] private List<GameObject> panelList = new();
        [SerializeField] private HundredRewardUIAdapter hundredRewardUIAdapter = null;
        [SerializeField] private ScrollRect tipScrollView = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            toggleGroup.OnValueChanged += OnToggleChanged;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            toggleGroup.OnValueChanged -= OnToggleChanged;
        }

        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            int selectIndex = toggleGroup.EnableIndex;
            for (int i = 0; i < panelList.Count; i++)
            {
                panelList[i].SetActive(i == selectIndex);
            }
            if (selectIndex >= 1 && selectIndex <= 3)
            {
                hundredRewardUIAdapter.gameObject.SetActive(true);
                hundredRewardUIAdapter.SetData((HundredProgress)selectIndex);
                hundredRewardUIAdapter.ScrollTo(0);
            }
            else
            {
                hundredRewardUIAdapter.gameObject.SetActive(false);
                tipScrollView.ScroolToTop(0);
            }
        }


        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            toggleGroup.Switch(0);
            tipScrollView.enabled = false;
            UnityTimer.Timer.Register(this.gameObject, 0.2f, () =>
            {
                tipScrollView.enabled = true;
            });
        }

        private void OnClose()
        {
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);

            UIController.Instance.CloseWindow<HundredHelpUI>();
        }
    }
}