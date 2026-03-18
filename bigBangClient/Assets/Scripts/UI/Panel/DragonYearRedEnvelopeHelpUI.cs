using System.Linq;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using Google.Protobuf.WellKnownTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class DragonYearRedEnvelopeHelpUI : AWindowController
    {
        [SerializeField] private BabuButton closeBtn;
        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClose;
        }
        protected override void RemoveListeners()
        {
            closeBtn.OnClick -= OnClose;
        }
        private void OnClose(BabuButton _)
        {
            UIController.Instance.CloseWindow<DragonYearRedEnvelopeHelpUI>();
        }

        [SerializeField] private string contentTextStr = "";
        [SerializeField] private TMP_Text contentText = null;
        [SerializeField] private VerticalLayoutGroup layout = null;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            int minutes = 5;
            if (RedEnvlopeManager.Instance.serverData != null && RedEnvlopeManager.Instance.serverData.LastTime >= 60)
            {
                minutes = RedEnvlopeManager.Instance.serverData.LastTime / 60;
            }
            contentText.text = contentTextStr.SafeFormat(minutes);

            foreach (var item in layout.transform.GetChildren<RectTransform>())
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout.transform as RectTransform);
        }
    }
}