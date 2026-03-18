using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class TacticHelpUIProperties : WindowProperties
    {
        public int TacticConfigId { get; private set; }

        public TacticHelpUIProperties(int TacticConfigId)
        {
            this.TacticConfigId = TacticConfigId;
        }
    }

    public class TacticHelpUI : AWindowController<TacticHelpUIProperties>
    {
        protected override void OnPropertiesSet()
        {
            AudioManager.Instance.PlaySound(AudioNames.ANI_TIPS);
            RefreshHelpPanel();
        }
        private void OnEnable()
        {
            HelpPanelCloseButton.onClick.AddListener(OnClickHelpCloseButton);
        }
        private void OnDisable()
        {
            HelpPanelCloseButton.onClick.RemoveListener(OnClickHelpCloseButton);
        }

        [SerializeField] private RectTransform HelpPanel;
        [SerializeField] private Button HelpPanelCloseButton;
        [SerializeField] private TMP_Text TacticsNameText;
        [SerializeField] private TMP_Text TacticsDesc;
        [SerializeField] private TMP_Text RestrainDesc;
        TacticsConfig tacticConfig = null;
        public void RefreshHelpPanel()
        {
            if(Configs.Tactics.GetDataDictionary().ContainsKey(Properties.TacticConfigId) == false)
            {
                Debug.LogError("未知的TacticConfigId：" + Properties.TacticConfigId);
                this.UI_Close();
                return;
            }
            tacticConfig = Configs.Tactics.GetDataDictionary()[Properties.TacticConfigId];
            TacticsNameText.text = tacticConfig.Name;
            TacticsDesc.text = tacticConfig.Desc;
            RestrainDesc.text = tacticConfig.Restrain;
        }
        public void OnClickHelpCloseButton()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            this.UI_Close();
        }

    }
}
