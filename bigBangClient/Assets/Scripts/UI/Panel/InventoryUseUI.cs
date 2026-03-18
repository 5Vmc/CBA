using System.Linq;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class InventoryUseUIProperties : WindowProperties
    {
        public GoodsData Data { get; private set; }

        public InventoryUseUIProperties(GoodsData data)
        {
            Data = data;
        }
    }

    public class InventoryUseUI : AWindowController<InventoryUseUIProperties>
    {
        [SerializeField] private Button openBtn;
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button addBtn;
        [SerializeField] private Button subBtn;
        [SerializeField] private InventoryItem selectItem;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text propText;
        [SerializeField] private TMP_Text propDescText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private Image clockImg;

        [SerializeField] public InventoryUseUIAnim Anim;

        private Color[] qualityColor = { Color.white, Color.green, Color.blue, new Color(1, 0, 1, 1), Color.yellow, Color.red };

        protected override void AddListeners()
        {
            base.AddListeners();
            openBtn.onClick.AddListener(OnOpen);
            closeBtn.onClick.AddListener(OnClose);
            addBtn.onClick.AddListener(OnAdd);
            subBtn.onClick.AddListener(OnSub);
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            openBtn.onClick.RemoveListener(OnOpen);
            closeBtn.onClick.RemoveListener(OnClose);
            addBtn.onClick.RemoveListener(OnAdd);
            subBtn.onClick.RemoveListener(OnSub);
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            selectItem.SetData(Properties.Data, false);
            // 设置为整型值
            slider.wholeNumbers = true;
            // 设置滑动条的最小值
            slider.minValue = 1;
            // 设置滑动条的最大值
            slider.maxValue = Mathf.Min(99, selectItem.Data.Count);
            // 设置滑动条初始值
            slider.value = 1;
            // 设置物品名称
            propText.text = selectItem.Data.Config.Name;
            //　设置物品名称颜色
            propText.color = CBAColorUtil.Instance.GetColor(Properties.Data.Config.Quality);
            // 设置物品描述
            propDescText.text = selectItem.Data.Config.Desc;
            // 设置滑动条文本
            countText.text = $"{1}/{selectItem.Data.Count}";
            var expiration = selectItem.Data.Config.ExpirationTime;
            // 判断是否已经过期
            if (expiration <= 0)
            {
                clockImg.gameObject.SetActive(false);
                timeText.gameObject.SetActive(false);
            }
            else
            {
                timeText.text = Lang.Get(LangID.ExpirationTimeText).Replace("{time}", TimeUtils.GetTimeString(expiration));
            }
            Anim.PlayEnter();
        }
        private void OnAdd()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_3);
            if (slider.value < 99 && slider.value < selectItem.Data.Count)
            {
                slider.value += 1;
            }
        }

        private void OnSub()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_3);
            if (slider.value > 1)
            {
                slider.value -= 1;
            }
            else
            {
                Tips.PopError(ErrorID.InventoryUseMinNumber);
            }
        }

        private void OnOpen()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            NetworkManager.Instance.OpenBox(Properties.Data.Config.Id, (int)slider.value, OnOpenSucceed);
            // 关闭当前界面
            UIController.Instance.CloseWindow<InventoryUseUI>();
        }

        public void OnOpenSucceed(OpenBoxResponse response)
        {
            //Debug.LogError(response.AddList.ToList().Count);
            // 出现收益界面
            UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(response.AddList.ToList()));
            EventManager.Instance.Dispatch(EventID.RefreshInventoryProp, Properties.Data.Config.Id);
        }

        private void OnSliderValueChanged(float value)
        {
            // 当前拥有
            countText.text = $"{value}/{selectItem.Data.Count}";
        }

        private void OnClose()
        {
            Anim.PlayExit(() =>
            {
                UIController.Instance.CloseWindow<InventoryUseUI>();
            });
        }
    }
}