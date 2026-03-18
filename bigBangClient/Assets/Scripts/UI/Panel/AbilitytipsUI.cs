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
    public class AbilitytipsUIProperties : WindowProperties
    {
        public CardAbilityConfig cardAbilityConfig;
        public AbilitytipsUIProperties(CardAbilityConfig cardAbilityConfig)
        {
            this.cardAbilityConfig = cardAbilityConfig;
        }

        public bool useCuntomPos = false;
        public Transform positionTransform;
        public Vector3 positionOffset;
        public void SetPos(Transform positionTransform, Vector3 positionOffset)
        {
            this.useCuntomPos = true;
            this.positionTransform = positionTransform;
            this.positionOffset = positionOffset;
        }
    }

    public class AbilitytipsUI : AWindowController<AbilitytipsUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private TMP_Text titleText = null;
        [SerializeField] private TMP_Text descText = null;

        public AbilitytipsUIAnim Anim;

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

        protected override async void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            titleText.text = "{0}：".SafeFormat(Properties.cardAbilityConfig.Name);
            descText.text = Properties.cardAbilityConfig.Desc;

            CheckSetPos();

            Anim.PlayEnter();
        }

        private readonly int maxX = 360 - 138;
        private readonly int minX = -360 + 138;
        [SerializeField] private RectTransform panel = null;
        private void CheckSetPos()
        {
            if (Properties.useCuntomPos == false)
            {
                panel.pivot = new(0.5f, 0.5f);
                panel.localPosition = new Vector3(0, 150f, 0);
                return;
            }

            panel.pivot = new(0.5f, 1.0f);
            Vector3 localPosition = Vector3.zero;
            if (Properties.positionTransform != null)
            {
                localPosition = Utils.Utility.ConvertLocalPosition(Properties.positionTransform, Vector3.zero, panel.parent);
            }
            localPosition += Properties.positionOffset;

            if (localPosition.x > maxX) localPosition.x = maxX;
            if (localPosition.x < minX) localPosition.x = minX;

            panel.localPosition = localPosition;
        }

        private void OnClose()
        {
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<AbilitytipsUI>();
        }
    }
}