using System;
using System.Collections;
using BigBang.Animation;
using deVoid.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class BuySthUIProperty : WindowProperties
    {
        public string Message = "";
        public Action Callback;
        public GameItem priceItem;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="itemstr">type:id:num  的格式，只支持1个消耗</param>
        /// <param name="callback"></param>
        public BuySthUIProperty(string message, string itemstr, Action callback)
        {
            Message = message;
            Callback = callback;
            priceItem = GameItemUtils.CreateGameItem(itemstr);
        }

        public BuySthUIProperty(string message, GameItem _priceItem, Action callback)
        {
            Message = message;
            Callback = callback;
            priceItem = _priceItem;
        }
    }

    public class BuySthUI : AWindowController<BuySthUIProperty>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button confirmBtn;
        [SerializeField] private Image imgIcon;
        [SerializeField] private TMP_Text txtPrice;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private ConfirmationBoxUIAnim anim;
        [SerializeField] private RectTransform _layouttrans;

        Action confirmCallback;

        private int diamondNeed = 0;
        protected override async void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            anim.PlayEnter();

            this.contentText.text = Properties.Message;
            this.txtPrice.text = Properties.priceItem.Count.ToString();
            this.imgIcon.sprite = await Properties.priceItem.GetIcon();
            this.confirmCallback = Properties.Callback;

            StartCoroutine(rebuildLayOut());
            
        }

        IEnumerator rebuildLayOut() {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_layouttrans);
        }

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
            confirmBtn.onClick.AddListener(onConfirm);
           
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
            confirmBtn.onClick.RemoveListener(onConfirm);
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            anim.PlayExit(() =>
            {
                UIController.Instance.CloseWindow<BuySthUI>();
            });
        }

        private void onConfirm()
        {
            string error = Player.PackageManager.IsGameItemEnough(Properties.priceItem);
            if (error != "") {
                //Tips.PopTips(error);
                return;
            }

            this.confirmCallback?.Invoke();
            this.OnClose();
        }
    }
}