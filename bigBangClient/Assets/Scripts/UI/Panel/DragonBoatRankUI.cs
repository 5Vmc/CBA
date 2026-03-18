using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BigBang.UI
{
    public class DragonBoatRankUI : AWindowController
    {
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private List<GameObject> panelList = new();
        [SerializeField] private DragonBoatRankRewardUIAdapter dragonBoatRankRewardUIAdapter = null;
        [SerializeField] private RectTransform noDataPanel = null;
        [SerializeField] private RectTransform subTitlePanel = null;
        [SerializeField] private DragonBoatRankUIAdapter dragonBoatRankUIAdapter = null;
        [SerializeField] private TMP_Text titleText = null;
        [SerializeField] private TMP_Text tipText = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
            toggleGroup.OnValueChanged += OnToggleChanged;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
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
            subTitlePanel.gameObject.SetActive(false);
            tipText.gameObject.SetActive(false);
            dragonBoatRankUIAdapter.gameObject.SetActive(false);
            noDataPanel.gameObject.SetActive(false);
            if (selectIndex == 0)
            {
                titleText.text = "排名";
                bool hasData = DragonBoatFestivalManager.Instance.courseData != null && DragonBoatFestivalManager.Instance.courseData.Ranks.Count > 0;
                noDataPanel.gameObject.SetActive(!hasData);
                subTitlePanel.gameObject.SetActive(hasData);
                tipText.gameObject.SetActive(hasData);
                dragonBoatRankUIAdapter.gameObject.SetActive(hasData);
                if (hasData)
                {
                    dragonBoatRankUIAdapter.SetData(DragonBoatFestivalManager.Instance.courseData.Ranks.ToList());
                }
            }
            if (selectIndex == 1)
            {
                dragonBoatRankRewardUIAdapter.SetData();
                dragonBoatRankRewardUIAdapter.ScrollTo(0);
                titleText.text = "排名奖励";
            }
        }


        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            toggleGroup.Switch(1);
        }

        private void OnClose(BabuButton _)
        {
            UIController.Instance.CloseWindow<DragonBoatRankUI>();
        }
    }
}