using deVoid.UIFramework;
using UnityEngine;

namespace BigBang.UI
{
    public abstract class SimpleWindowBaseUI<T> : AWindowController
    {
        [SerializeField] public BabuButton closeBtn;
        protected override void AddListeners()
        {
            if (closeBtn != null) closeBtn.OnClick += OnClose;
        }
        protected override void RemoveListeners()
        {
            if (closeBtn != null) closeBtn.OnClick -= OnClose;
        }
        private void OnClose(BabuButton _)
        {
            UIController.Instance.CloseWindow<T>();
        }
    }
}