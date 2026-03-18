using UnityEngine;
using deVoid.UIFramework;
using Utils;
using BigBang.Animation;

namespace BigBang.UI
{
    public class BigBangPayUI : AWindowController
    {
        [SerializeField] private BigBangPayUIComponent com;
        [SerializeField] private BigBangPayAnim anim;

        protected override void AddListeners()
        {
            com.CloseBtn.onClick.AddListener(OnClose);
            com.ConfirmBtn.onClick.AddListener(OnConfirm);
        }

        protected override void RemoveListeners()
        {
            com.CloseBtn.onClick.RemoveListener(OnClose);
            com.ConfirmBtn.onClick.RemoveListener(OnConfirm);
        }

        protected override void OnPropertiesSet()
        {
            com.ContentText.text = Lang.Get(LangID.ClearBigBangCDContent)
                .Replace("{Cost}", Player.TrainManager.BigBangController.GetClearBigBangCDDiamond().ToString());
            anim.Play();

        }

        private void OnClose()
        {
            TouchManager.Instance.DisableTouch();
            // 取消音效
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACKBG);
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            anim.PlayNext(() =>
            {
                TouchManager.Instance.EnableTouch();
                UIController.Instance.CloseWindow<BigBangPayUI>();
            });
        }

        private void OnConfirm()
        {
            TouchManager.Instance.DisableTouch();
            // 确认音效
            AudioManager.Instance.PlaySound(AudioNames.BTN_CFM);
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            //关闭窗口动画
            anim.PlayNext(() =>
            {
                TouchManager.Instance.EnableTouch();
                Babu.EventManager.Instance.Dispatch(EventID.OnBigBangPadPay);
                UIController.Instance.CloseWindow<BigBangPayUI>();
            });
        }
    }
}