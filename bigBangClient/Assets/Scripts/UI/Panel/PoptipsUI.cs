using System.Linq;
using System.Threading.Tasks;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class PoptipsUIProperties : WindowProperties
    {
        public string desc;
        public Transform positionTransform;
        public Vector3 positionOffset;
        public TextAlignmentOptions textAlignmentOptions;
        public PoptipsUIProperties(string desc, Transform positionTransform, Vector3 positionOffset, TextAlignmentOptions textAlignmentOptions = TextAlignmentOptions.Midline)
        {
            this.desc = desc;
            this.positionTransform = positionTransform;
            this.positionOffset = positionOffset;
            this.textAlignmentOptions = textAlignmentOptions;
        }
    }

    public class PoptipsUI : AWindowController<PoptipsUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private TMP_Text descText = null;
        [SerializeField] private Transform panel = null;

        public PoptipsUIAnim Anim;

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

        private readonly int maxX = 360 - 85;
        private readonly int minX = -360 + 85;
        protected override async void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            descText.text = Properties.desc;
            descText.alignment = Properties.textAlignmentOptions;

            Vector3 localPosition = Vector3.zero;
            if (Properties.positionTransform != null)
            {
                localPosition = Utils.Utility.ConvertLocalPosition(Properties.positionTransform, Vector3.zero, panel.parent);
            }
            localPosition += Properties.positionOffset;

            if (localPosition.x > maxX) localPosition.x = maxX;
            if (localPosition.x < minX) localPosition.x = minX;

            panel.localPosition = localPosition;

            Anim.PlayEnter();
        }

        private void OnClose()
        {
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<PoptipsUI>();
        }
    }
}