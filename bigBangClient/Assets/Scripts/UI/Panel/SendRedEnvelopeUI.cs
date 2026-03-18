using System.Linq;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang.UI
{
    public class SendRedEnvelopeUIProperties : WindowProperties
    {
        public ActivityData activityData { get; private set; }

        public SendRedEnvelopeUIProperties(ActivityData activityData)
        {
            this.activityData = activityData;
        }
    }

    public class SendRedEnvelopeUI : AWindowController<SendRedEnvelopeUIProperties>
    {
        [SerializeField] private BabuButton openBtn;
        [SerializeField] private BabuButton closeBtn;
        [SerializeField] private BabuButton addBtn;
        [SerializeField] private BabuButton subBtn;
        [SerializeField] private InventoryItem selectItem;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text propText;
        [SerializeField] private TMP_Text propDescText;
        [SerializeField] private BabuButton subBtnGray = null;
        [SerializeField] private BabuButton addBtnGray = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            openBtn.OnClick += OnOpen;
            closeBtn.OnClick += OnClose;
            addBtn.OnClick += OnAdd;
            subBtn.OnClick += OnSub;
            addBtnGray.OnClick += OnAdd;
            subBtnGray.OnClick += OnSub;
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            openBtn.OnClick -= OnOpen;
            closeBtn.OnClick -= OnClose;
            addBtn.OnClick -= OnAdd;
            subBtn.OnClick -= OnSub;
            addBtnGray.OnClick -= OnAdd;
            subBtnGray.OnClick -= OnSub;
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
        GameItem gameItem = null;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, Properties.activityData.cfg.Param1, 0);
            selectItem.SetData(gameItem);
            selectItem.SetCountTextActive(false);
            // 设置为整型值
            slider.wholeNumbers = true;
            // 设置滑动条的最小值
            slider.minValue = 1;
            // 设置滑动条的最大值
            slider.maxValue = gameItem.GetPlayerCount();
            // 设置滑动条初始值
            slider.value = 1;
            // 设置物品名称
            propText.text = gameItem.GetName();
            //　设置物品名称颜色
            propText.color = CBAColorUtil.Instance.GetColor(gameItem.GetQuality());
            // 设置物品描述
            propDescText.text = gameItem.GetDescription();
            // 设置滑动条文本
            RefreshItemCount();

            LayoutRebuilder.ForceRebuildLayoutImmediate(propText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(propText.transform.parent as RectTransform);
        }
        private void OnAdd(BabuButton _)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_3);
            if (Mathf.RoundToInt(slider.value) < gameItem.GetPlayerCount())
            {
                slider.value = Mathf.RoundToInt(slider.value) + 1;
            }
        }

        private void OnSub(BabuButton _)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_3);
            if (Mathf.RoundToInt(slider.value) > 1)
            {
                slider.value -= 1;
            }
            else
            {
                Tips.PopError(ErrorID.InventoryUseMinNumber);
            }
        }

        private void OnOpen(BabuButton _)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);

            int sendCount = Mathf.RoundToInt(slider.value);
            NetworkManager.Instance.SendRedPacket(Properties.activityData.cfg.Id, sendCount, (SendRedPacketResponse sendRedPacketResponse) =>
            {
                if (sendRedPacketResponse.Success)
                {
                    RedEnvlopeManager.Instance.serverData.MyRank.SendPacket += sendCount;
                    RedEnvlopeManager.Instance.serverData.TotalPacketCount = sendRedPacketResponse.TotalPacketCount;
                    RedEnvlopeManager.Instance.ResetRankDataByUp();
                    EventManager.Instance.Dispatch(EventID.OnAfterSendRedEnvlope);
                    Tips.PopTips("您的红包已放入红包池");
                }
                UIController.Instance.CloseWindow<SendRedEnvelopeUI>();
            });
        }

        private void OnSliderValueChanged(float value)
        {
            RefreshItemCount();
        }
        private void RefreshItemCount()
        {
            countText.text = "<color=#F16918>{0}</color>/{1}".SafeFormat(slider.value, gameItem.GetPlayerCount());
            bool canSub = Mathf.RoundToInt(slider.value) > 1;
            bool canAdd = Mathf.RoundToInt(slider.value) < gameItem.GetPlayerCount();
            addBtn.gameObject.SetActive(canAdd);
            subBtn.gameObject.SetActive(canSub);
            addBtnGray.gameObject.SetActive(!canAdd);
            subBtnGray.gameObject.SetActive(!canSub);
        }

        private void OnClose(BabuButton _)
        {
            UIController.Instance.CloseWindow<SendRedEnvelopeUI>();
        }
    }
}