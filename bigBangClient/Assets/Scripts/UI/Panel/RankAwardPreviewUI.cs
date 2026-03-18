using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System.Collections.Generic;
using GameConfig.Config;

namespace BigBang.UI
{
    public class RankAwardPreviewUIProperties : WindowProperties
    {
        public List<ActivityTopRewardConfig> dataList;
        public string RankName;
        public RankAwardPreviewUIProperties(string rankName, List<ActivityTopRewardConfig> _data)
        {
            dataList = _data;
            RankName = rankName;
        }
    }

    public class RankAwardPreviewUI : AWindowController<RankAwardPreviewUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private RankAwardPreviewAdapter adapter;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            titleText.text = Properties.RankName;
            adapter.SetData(Properties.dataList);

        }

        private void OnClose()
        {
            UIController.Instance.CloseWindow<RankAwardPreviewUI>();
        }
    }
}