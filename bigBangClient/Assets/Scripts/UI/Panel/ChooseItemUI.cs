using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ChooseItemUIProperties : WindowProperties
    {
        public GoodsData Data { get; private set; }

        public ChooseItemUIProperties(GoodsData data)
        {
            Data = data;
        }
    }

    public class ChooseItemUI : AWindowController<ChooseItemUIProperties>
    {
        [SerializeField] private Button btnUse;
        [SerializeField] private Button btnClose;
        [SerializeField] private Button btnAdd;
        [SerializeField] private Button btnDec;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text txtCount;
        [SerializeField] private TMP_Text txtItemName;
        [SerializeField] private TMP_Text txtInfo;
        [SerializeField] private ChooseItemGridAdapter adapter;
        [SerializeField] private TMP_Text nowCount = null;

        [SerializeField] private ConfirmationBoxUIAnim anim = null;

        private ChooseItemGridModel selectedGridModel;
        protected override void AddListeners()
        {
            base.AddListeners();
            btnUse.onClick.AddListener(OnUse);
            btnClose.onClick.AddListener(OnClose);
            btnAdd.onClick.AddListener(OnAdd);
            btnDec.onClick.AddListener(OnSub);
            slider.onValueChanged.AddListener(OnSliderValueChanged);
            adapter.OnSelect += OnItemSelect;
        }

        private void OnItemSelect(ChooseItemGridModel selectItem)
        {
            selectedGridModel = selectItem;
            RefreshPlayerCount();
        }
        private void RefreshPlayerCount()
        {
            nowCount.text = "已拥有：{0}".SafeFormat(selectedGridModel.Data.GetPlayerCount());
            ForceRebuildLayout();
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            btnUse.onClick.RemoveListener(OnUse);
            btnClose.onClick.RemoveListener(OnClose);
            btnAdd.onClick.RemoveListener(OnAdd);
            btnDec.onClick.RemoveListener(OnSub);
            adapter.OnSelect -= OnItemSelect;
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            if (Properties.Data.Config.Type == (int)GoodsType.SelectProp)
            {
                txtInfo.gameObject.SetActive(true);
            }
            else
            {
                txtInfo.gameObject.SetActive(false);
            }

            // 设置滑动条的最小值
            slider.minValue = 1;

            setAdapterData(Properties.Data.Config.Type);
            if (adapter.chooseItemGridModel.Count > 0)
            {
                adapter.SetSelection(0);
            }

            // 设置滑动条文本
            txtCount.text = "选择<color=#77400b>{0}</color>/{1}个".SafeFormat(1, Properties.Data.Count);
            slider.maxValue = Properties.Data.Count;
            slider.value = 1;
            txtItemName.text = "使用【{0}】".SafeFormat(Properties.Data.Config.Name);
            ForceRebuildLayout();
            anim.PlayEnter();
        }
        [SerializeField] private RectTransform tipPanel = null;
        [SerializeField] private RectTransform nowCountBgImage = null;
        [SerializeField] private RectTransform nowCountText = null;
        [SerializeField] private RectTransform tipInfoText = null;
        //强制重建布局
        private void ForceRebuildLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(nowCountText);
            LayoutRebuilder.ForceRebuildLayoutImmediate(nowCountBgImage);
            LayoutRebuilder.ForceRebuildLayoutImmediate(tipInfoText);
            LayoutRebuilder.ForceRebuildLayoutImmediate(tipPanel);
        }

        private void setAdapterData(int type)
        {
            if (type == (int)GoodsType.SelectProp)
            {
                SetSelectProp();
            }
            else if (type == (int)GoodsType.SelectBoxProp)
            {
                SetSelectBoxProp();
            }
        }

        private void SetSelectProp()
        {
            //只能筛选同品质的道具，goods表的字段不够用了。
            //玩家必须有这个卡，默认筛选目标品质与道具品质相同
            var cardList = Player.CardManager.GetCardList();
            var result = Player.PackageManager.GoodsFilter(Properties.Data.Config.Quality, 3).ConvertAll<GameItem>(t => GameItemUtils.CreateGameItem(GameItemType.Goods, t.Id, 1));
            var _firstGameItem = result.First();
            selectedGridModel = new ChooseItemGridModel() { GridID = _firstGameItem.Id, Data = _firstGameItem, Count = _firstGameItem.Count };
            adapter.SetData(result, 0);
            RefreshPlayerCount();
        }
        private void SetSelectBoxProp()
        {
            var result = Configs.Box.GetConfigList().Where<BoxConfig>(p => p.BoxId == Properties.Data.Config.Param1).ToList();
            var _firstBoxItem = result.First();
            var _firstGameItem = GameItemUtils.CreateGameItem((GameItemType)_firstBoxItem.RewardType, _firstBoxItem.RewardId, _firstBoxItem.RewardNum);
            selectedGridModel = new ChooseItemGridModel() { GridID = _firstBoxItem.Id, Data = _firstGameItem, Count = _firstGameItem.Count };
            adapter.SetData(result, 0);
            RefreshPlayerCount();
        }

        private void OnAdd()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_3);
            if (slider.value < 99 && slider.value < Properties.Data.Count)
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

        private void OnUse()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);

            if (selectedGridModel == null)
            {
                Tips.PopTips("没有选择任何奖励");
                return;
            }


            var _selectedItemId = 0;
            if (Properties.Data.Config.Type == (int)GoodsType.SelectBoxProp)
            {
                _selectedItemId = selectedGridModel.GridID;
            }
            else
            {
                _selectedItemId = selectedGridModel.Data.Id;
            }


            int num = (int)slider.value;
            NetworkManager.Instance.GetOptionalRewards(Properties.Data.Config.Id, _selectedItemId, (int)slider.value, (resp) =>
            {
                OnOpenSucceed(resp, num);
            });


            //NetworkManager.Instance.OpenBox(Properties.Data.Config.Id, (int)slider.value, OnOpenSucceed);
            // 关闭当前界面
            UIController.Instance.CloseWindow<ChooseItemUI>();
        }

        public void OnOpenSucceed(Protocol.OpenOptionalBoxResponse response, int num)
        {
            if (Properties.Data.Config.Type == (int)GoodsType.SelectBoxProp)
            {

                var item = GameItemUtils.CreateGameItem(selectedGridModel.Data.Type, selectedGridModel.Data.Id, num * selectedGridModel.Data.Count);
                UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(item));
            }
            else
            {
                var item = GameItemUtils.CreateGameItem(selectedGridModel.Data.Type, selectedGridModel.Data.Id, num * selectedGridModel.Data.Count);
                UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(item));
            }
            EventManager.Instance.Dispatch(EventID.RefreshInventoryProp, Properties.Data.Config.Id);
        }

        private void OnSliderValueChanged(float value)
        {
            // 当前拥有
            txtCount.text = $"选择{value}/{Properties.Data.Count}个";
        }

        private void OnClose()
        {
            UIController.Instance.CloseWindow<ChooseItemUI>();
        }
    }
}