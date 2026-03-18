using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang.UI
{
    public class QuickExchangeUIProperties : WindowProperties
    {
        //game_item_shop(道具商城表的 ID)
        public GameItemShopConfig gameItemShopConfig { get; private set; }
        public int needCount { get; private set; }

        public QuickExchangeUIProperties(int getItemId, int needCount)
        {
            this.needCount = needCount;
            this.gameItemShopConfig = Configs.GameItemShop.GetConfigList().FirstOrDefault(item => item.Type == 10 && item.Item.Split(':')[1] == getItemId.ToString());
            if (gameItemShopConfig == null)
            {
                Debug.LogWarning("QuickExchangeUIProperties , QuickExchangeUIProperties , gameItemShopConfig == null , getItemId = " + getItemId);
                return;
            }
        }
    }

    public class QuickExchangeUI : AWindowController<QuickExchangeUIProperties>
    {
        [SerializeField] private Button confirmBtn;
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button addBtn;
        [SerializeField] private Button add10Btn;
        [SerializeField] private Button subBtn;
        [SerializeField] private InventoryItem selectItem;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text getCountText;
        [SerializeField] private TMP_Text propText;
        [SerializeField] private TMP_Text propDescText;

        [SerializeField] public QuickExchangeUIAnim Anim;

        private Color[] qualityColor = { Color.white, Color.green, Color.blue, new Color(1, 0, 1, 1), Color.yellow, Color.red };

        protected override void AddListeners()
        {
            base.AddListeners();
            confirmBtn.onClick.AddListener(OnConfirm);
            closeBtn.onClick.AddListener(OnClose);
            addBtn.onClick.AddListener(OnAdd);
            add10Btn.onClick.AddListener(OnAdd10);
            subBtn.onClick.AddListener(OnSub);
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            confirmBtn.onClick.RemoveListener(OnConfirm);
            closeBtn.onClick.RemoveListener(OnClose);
            addBtn.onClick.RemoveListener(OnAdd);
            add10Btn.onClick.RemoveListener(OnAdd10);
            subBtn.onClick.RemoveListener(OnSub);
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        GameItem getGameItem = null;
        List<GameItem> costItemList = new();
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            if (Properties.gameItemShopConfig == null)
            {
                return;
            }

            getGameItem = GameItemUtils.CreateGameItem(Properties.gameItemShopConfig.Item);
            if (getGameItem == null)
            {
                Debug.LogWarning("QuickExchangeUI , OnPropertiesSet , getGameItem == null , Properties.gameItemShopConfig.Id = " + Properties.gameItemShopConfig.Id);
                return;
            }

            selectItem.SetData(getGameItem, false);

            costItemList = GameItemUtils.CreateGameItems(Properties.gameItemShopConfig.Cost).ToList();

            // 设置为整型值
            slider.wholeNumbers = true;
            // 设置滑动条的最小值
            slider.minValue = 1;
            // 设置滑动条的最大值
            slider.maxValue = 100;
            // 设置滑动条初始值
            int startValue = Utility.KeepInRange(Properties.needCount - getGameItem.GetPlayerCount(), 1, 100);
            slider.value = startValue;
            // 设置物品名称
            propText.text = getGameItem.GetName();
            // 设置物品已拥有数量
            countText.text = "已拥有：{0}".SafeFormat(getGameItem.GetPlayerCount());
            //　设置物品名称颜色
            propText.color = CBAColorUtil.Instance.GetColor(getGameItem.GetQuality());
            // 设置物品描述
            propDescText.text = getGameItem.GetDescription();
            // 设置滑动条文本
            getCountText.text = startValue.ToString();

            Anim.PlayEnter();
        }
        private void OnAdd()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_3);
            if (slider.value < 100)
            {
                slider.value += 1;
            }
            else
            {
                Tips.PopError("已达最大数量");
            }
        }
        private void OnAdd10()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_3);
            if (slider.value < 100)
            {
                slider.value = Utility.KeepInRange(slider.value + 10, 1, 100);
            }
            else
            {
                Tips.PopError("已达最大数量");
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

        private void OnConfirm()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            int count = Mathf.RoundToInt(slider.value);
            string error = Player.PackageManager.IsGameItemsEnough(costItemList, false, count);
            if (error != "")
            {
                Tips.PopTips(error);
                return;
            }
            Player.ShopManager.ExChangeItem(Properties.gameItemShopConfig, count, () =>
            {
                UIController.Instance.CloseWindow<QuickExchangeUI>();
                getGameItem.Count = count;
                UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(getGameItem));
            });
        }

        private void OnSliderValueChanged(float value)
        {
            // 当前拥有
            getCountText.text = $"{value}";
            RefreshCost();
        }

        [SerializeField] private List<CostItem> costList;
        private void RefreshCost()
        {
            for (var index = 0; index < 3; index++)
            {
                if (index <= costItemList.Count - 1)
                {
                    costList[index].gameObject.SetActive(true);
                    costList[index].SetData(costItemList[index], true, Mathf.RoundToInt(slider.value));
                }
                else
                {
                    costList[index].gameObject.SetActive(false);
                }
                costList[index].ForceRebuildLayout();
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(costList[0].transform.parent as RectTransform);
        }

        private void OnClose()
        {
            Anim.PlayExit(() =>
            {
                UIController.Instance.CloseWindow<QuickExchangeUI>();
            });
        }
    }
}