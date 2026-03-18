using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using GameConfig;
using System.Collections.Generic;
using GameConfig.Config;
using System.Text;
using System;
using Babu;

namespace BigBang.UI
{
    [Serializable]
    public class FBTowerRaidResultUIProperties : WindowProperties
    {
        public List<TowerLevelData> towerLevelDataList = null;
        public FBTowerRaidResultUIProperties(List<TowerLevelData> towerLevelDataList)
        {
            this.towerLevelDataList = towerLevelDataList;
        }
    }
    public class FBTowerRaidResultUI : AWindowController<FBTowerRaidResultUIProperties>
    {
        [SerializeField] private TMP_Text titleText = null;
        [SerializeField] private FBTowerRaidResultItemAdapter osa = null;
        [SerializeField] private BabuButton confirmButton = null;
        [SerializeField] private Button closeBtn = null;

        [SerializeField] private FBTowerRaidResultUIAnim anim = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            confirmButton.OnClick += OnClickConfirmButton;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            confirmButton.OnClick -= OnClickConfirmButton;
        }
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            SetData();
            anim.PlayEnter();
        }
        private void OnClose()
        {
            UIController.Instance.CloseWindow<FBTowerRaidResultUI>();
        }
        private void OnClickConfirmButton(BabuButton _)
        {
            UIController.Instance.CloseWindow<FBTowerRaidResultUI>();
        }

        private void SetData()
        {
            osa.SetData(Properties.towerLevelDataList);
        }

    }
}