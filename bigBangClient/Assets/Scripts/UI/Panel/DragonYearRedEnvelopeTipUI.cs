using deVoid.UIFramework;
using UnityEngine;

namespace BigBang.UI
{
    public class DragonYearRedEnvelopeTipUI : AWindowController
    {
        [SerializeField] public BabuButton goButton;
        protected override void AddListeners()
        {
            goButton.OnClick += OnClickGoButton;
        }
        protected override void RemoveListeners()
        {
            goButton.OnClick -= OnClickGoButton;
        }
        private void OnClickGoButton(BabuButton _)
        {
            UIController.Instance.CloseAllPanelAndWindow();
            TriggerManager.Instance.JumpPanel(TriggerModuleType.DragonYearRedEnvelope);
        }
    }
}