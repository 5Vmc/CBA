using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class BigBangConfirmUI : AWindowController
    {
        [SerializeField] private BigBangConfirmUIComponent com;
        [SerializeField] private BigBangConfirmAnim anim;

        protected override void AddListeners()
        {
            com.CloseBtn.onClick.AddListener(OnClose);
            com.BigBangBtn.onClick.AddListener(OnBigBang);
            com.SuperBigBangBtn.onClick.AddListener(OnSuperBigBang);
        }

        protected override void RemoveListeners()
        {
            com.CloseBtn.onClick.RemoveListener(OnClose);
            com.BigBangBtn.onClick.RemoveListener(OnBigBang);
            com.SuperBigBangBtn.onClick.RemoveListener(OnSuperBigBang);
        }

        protected override void OnPropertiesSet()
        {
            anim.Play();
            AudioManager.Instance.PlaySound(AudioNames.ANI_TECHBOARDPOP);

            com.UpdateUI();
        }

        private void OnClose()
        {
            TouchManager.Instance.DisableTouch();
            AudioManager.Instance.PlaySound(AudioNames.ANI_TECHBOARDSHUT);
            anim.PlayNext(() =>
            {
                UIController.Instance.CloseWindow<BigBangConfirmUI>();
                TouchManager.Instance.EnableTouch();
                EventManager.Instance.Dispatch(EventID.OnRemakeBigBangStartButton);
            });

        }

        private void OnBigBang()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            UIController.Instance.CloseWindow<BigBangConfirmUI>();
            Babu.EventManager.Instance.Dispatch(EventID.OnBigBangStart);
        }

        private void OnSuperBigBang()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            UIController.Instance.CloseWindow<BigBangConfirmUI>();
            Babu.EventManager.Instance.Dispatch(EventID.OnSuperBigBang);
        }
    }
}