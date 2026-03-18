using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class TimeGiftUIProperties : WindowProperties
    {
        public GiftItemData data;

        public TimeGiftUIProperties(GiftItemData _data)
        {
            data = _data;
        }
    }

    public class TimeGiftUI : AWindowController<TimeGiftUIProperties>
    {

        [SerializeField] private TimeGiftItem item;
        [SerializeField] private Button btnClose;

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            AudioManager.Instance.PlaySound(AudioNames.ANI_TIPS);

            item.SetData(Properties.data);
        }

        protected void OnEnable()
        {
            btnClose.onClick.AddListener(PlayExit);
            EventManager.Instance.Register(EventID.OnRefreshGiftShop, CloseWindow);
            EventManager.Instance.Register(EventID.OnTimeGiftTimeEnd, OnTimeGiftTimeEnd);
        }

        protected void OnDisable()
        {
            btnClose.onClick.RemoveListener(PlayExit);
            EventManager.Instance.Unregister(EventID.OnRefreshGiftShop, CloseWindow);
            EventManager.Instance.Unregister(EventID.OnTimeGiftTimeEnd, OnTimeGiftTimeEnd);
        }

        private void CloseWindow(object[] args = null)
        {
            UIController.Instance.CloseWindow<TimeGiftUI>(false);
        }

        private void OnTimeGiftTimeEnd(object[] args = null)
        {
            TimeGiftItem timeGiftItemEnd = args[0] as TimeGiftItem;
            if (item == timeGiftItemEnd)
            {
                PlayExit();
            }
        }

        private void PlayExit()
        {
            UIController.Instance.CloseWindow<TimeGiftUI>();
        }
    }
}
