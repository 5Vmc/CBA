using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;

namespace BigBang.UI
{
    public class RecoverHelpUI : AWindowController
    {
        [SerializeField] private Button closeBtn = null;
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
        [SerializeField] private ScrollRect tipScrollView = null;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            tipScrollView.enabled = false;
            UnityTimer.Timer.Register(this.gameObject, 0.2f, () =>
            {
                tipScrollView.enabled = true;
            });
        }
        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<RecoverHelpUI>();
        }
    }
}