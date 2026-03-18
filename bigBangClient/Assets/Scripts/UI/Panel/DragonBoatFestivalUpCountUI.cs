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
using static BigBang.DragonBoatFestivalManager;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang.UI
{
    public class DragonBoatFestivalUpCountUI : AWindowController
    {
        [SerializeField] private TMP_Text titleText = null;
        [SerializeField] private BabuButton openBtn;
        [SerializeField] private BabuButton closeBtn;
        [SerializeField] private BabuButton addBtn;
        [SerializeField] private BabuButton subBtn;
        [SerializeField] private InventoryItem selectItem;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text propText;
        [SerializeField] private TMP_Text propDescText;

        protected override void AddListeners()
        {
            base.AddListeners();
            openBtn.OnClick += OnOpen;
            closeBtn.OnClick += OnClose;
            addBtn.OnClick += OnAdd;
            subBtn.OnClick += OnSub;
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            openBtn.OnClick -= OnOpen;
            closeBtn.OnClick -= OnClose;
            addBtn.OnClick -= OnAdd;
            subBtn.OnClick -= OnSub;
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            Stage stage = DragonBoatFestivalManager.Instance.GetStage();
            if (stage != Stage.NormalPlaying)
            {
                Tips.PopTips("已过可用时间");
                UIController.Instance.CloseWindow<DragonBoatFestivalUpCountUI>();
                return;
            }
            ActivityData activityData = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalHome);
            if (activityData == null) return;
            GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, activityData.cfg.Param1, 0);
            selectItem.SetData(gameItem, false);
            titleText.text = "";
            if(DragonBoatFestivalManager.Instance.myTeam == Team.Left) titleText.text = "为<#ACFF35>甜粽龙舟队</color>助力";
            if (DragonBoatFestivalManager.Instance.myTeam == Team.Right) titleText.text = "为<#FFED42>咸粽龙舟队</color>助力";
            // 设置为整型值
            slider.wholeNumbers = true;
            // 设置滑动条的最小值
            slider.minValue = 1;
            // 设置滑动条的最大值
            slider.maxValue = selectItem.gameItem.GetPlayerCount();
            // 设置滑动条初始值
            slider.value = selectItem.gameItem.GetPlayerCount();
            // 设置物品名称
            propText.text = selectItem.gameItem.GetName();
            //　设置物品名称颜色
            propText.color = CBAColorUtil.Instance.GetColor(gameItem.GetQuality());
            // 设置物品描述
            propDescText.text = selectItem.gameItem.GetDescription();
            // 设置滑动条文本
            countText.text = $"{selectItem.gameItem.GetPlayerCount()}/{selectItem.gameItem.GetPlayerCount()}";
        }
        private void OnAdd(BabuButton _)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_3);
            if (slider.value < 99 && slider.value < selectItem.gameItem.GetPlayerCount())
            {
                slider.value += 1;
            }
        }

        private void OnSub(BabuButton _)
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

        private void OnOpen(BabuButton _)
        {
            Stage stage = DragonBoatFestivalManager.Instance.GetStage();
            if(stage != Stage.NormalPlaying)
            {
                Tips.PopTips("已过可用时间");
                UIController.Instance.CloseWindow<DragonBoatFestivalUpCountUI>();
                return;
            }
            DragonBoatFestivalManager.Instance.AddDragonBoatMeters(Mathf.RoundToInt(slider.value));
            UIController.Instance.CloseWindow<DragonBoatFestivalUpCountUI>();
        }

        private void OnSliderValueChanged(float value)
        {
            // 当前拥有
            countText.text = $"{value}/{selectItem.gameItem.GetPlayerCount()}";
        }

        private void OnClose(BabuButton _)
        {
            UIController.Instance.CloseWindow<DragonBoatFestivalUpCountUI>();
        }
    }
}