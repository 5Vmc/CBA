using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using BigBang.Animation;
using GameConfig.Config;

namespace BigBang.UI
{
    public class MergeCardUIProperties : WindowProperties
    {
        public CardModelConfig Cfg;
        public MergeCardUIProperties(CardModelConfig config)
        {
            Cfg = config;
        }
    }

    public class MergeCardUI : AWindowController<MergeCardUIProperties>
    {
        [SerializeField] private CardItem cardItem;
        [SerializeField] private Button closeBtn;

        public MergeCardUIAnim Anim;

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
            cardItem.SetConfigShow(Properties.Cfg);
            Anim.PlayEnter();
        }

        private void OnClose()
        {
            UIController.Instance.CloseWindow<MergeCardUI>();
        }
    }
}