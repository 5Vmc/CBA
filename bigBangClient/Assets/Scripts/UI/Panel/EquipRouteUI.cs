using System;
using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using GameConfig;
using System.Collections.Generic;
using GameConfig.Config;
using Babu;
using Utils.GameItem;
using static BigBang.ClassicManager;
using Protocol;
using System.Linq;

namespace BigBang.UI
{
    public class EquipRouteUIProperties : WindowProperties
    {
        public GoodsData goods;
        public Utils.GameItem.GameItem costItem;
        public int cardid;
        public int lv;
        public int partIndex;

        public bool ItemEnough
        {
            get => goods.Count >= costItem.Count;
        }

        public EquipRouteUIProperties(Utils.GameItem.GameItem _costItem, int _lv, int _partIndex, int _cardid)
        {
            costItem = _costItem;
            partIndex = _partIndex;
            cardid = _cardid;
            lv = _lv;
            //goods = Player.PackageManager.GetGoodsEx(_costItem.Id);
        }
    }
    public class EquipRouteUI :  AWindowController<EquipRouteUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Image energyIcon;
        [SerializeField] private TMP_Text energyValue;
        [SerializeField] private TMP_Text txtCombine;
        [SerializeField] private InventoryItem resIcon;
        [SerializeField] private TMP_Text txtNeed;
        [SerializeField] private TMP_Text txtHave;
        [SerializeField] private BabuButton btnCombine;

        [SerializeField] private EquipRouteAdapter adapter;
        [SerializeField] private GameObject Obj_nodungeons;
        [SerializeField] private BabuButton btn_jump;

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
            EventManager.Instance.Register(EventID.RefreshWindow, onRefreshWindow);
            btn_jump.OnClick += ToDungeon;
            btnCombine.OnClick += DoCombine;
            EventManager.Instance.Register(EventID.OnResourceChange, UpdateEnergy);
        }

        private void ToDungeon(BabuButton obj)
        {
            UIController.Instance.CloseWindow<EquipRouteUI>();
            TriggerManager.Instance.JumpPanel(TriggerModuleType.ClassicPVE);
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
            EventManager.Instance.Unregister(EventID.RefreshWindow, onRefreshWindow);
            btnCombine.OnClick -= DoCombine;
            btn_jump.OnClick -= ToDungeon;
            EventManager.Instance.Unregister(EventID.OnResourceChange, UpdateEnergy);
        }

        private void UpdateEnergy(object[] args)
        {
            energyValue.text = Player.PackageManager.Energy.ToString();
            adapter.UpdateEnergy();
        }

        private void DoCombine(BabuButton obj)
        {
            if (Properties.ItemEnough)
            {
                PlayerCard card = Player.CardManager.GetCard(Properties.cardid);
                Player.CardManager.CardEquipLevelUp(card, Properties.partIndex, () => {
                    UIController.Instance.CloseWindow<EquipRouteUI>();
                });
            }
            else {
                Tips.PopTips("材料不足");
            }
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            onRefreshWindow();
        }

        private void onRefreshWindow(object[] args = null) {

            showMaterial();
            loadClubs();
            energyValue.text = Player.PackageManager.Energy.ToString();
        }

        [SerializeField] private Color btnCombineTxtColorEnable;
        [SerializeField] private Color btnCombineTxtColordisable;
        /// <summary>
        /// 设置材料当前状况
        /// </summary>
        private async void showMaterial() {
            Properties.goods = Player.PackageManager.GetGoodsEx(Properties.costItem.Id);
            resIcon.SetData(Properties.goods);
            resIcon.countText.gameObject.SetActive(false);
            resIcon.ShowSelectBorder();

            string HexColor = "";
            if (Properties.ItemEnough)
            {
                HexColor = CBAColorUtil.Instance.GetHexColor(CBAColor.Green); ;
                btnCombine.image.sprite = await SpriteProxy.YellowBtnEnable;
                txtCombine.color = btnCombineTxtColorEnable;
            }
            else {
                HexColor = CBAColorUtil.Instance.GetHexColor(CBAColor.Red);
                btnCombine.image.sprite = await SpriteProxy.YellowBtnDisable;
                txtCombine.color = btnCombineTxtColordisable;
            }
            txtCombine.text = Properties.lv == 1 ? "合  成" : "升  级";
            txtHave.text = string.Format("拥有：<color={1}>{0}</color>", Properties.goods.Count, HexColor);
            txtNeed.text = string.Format("需要：{0}", Properties.costItem.Count);
        }

        private void loadClubs() {
            List<PassData> list = ClassicManager.Instance.GetPassedDataByItemId(Properties.costItem.Id).OrderByDescending(p=>p.Stars.Sum()).ToList();
            adapter.SetData(list, Properties.costItem.Id);

            Obj_nodungeons.SetActive(list.Count == 0);
        }

        private void OnClose()
        {
            //Debug.Log("OnClose");
            //TouchManager.Instance.DisableTouch();
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<EquipRouteUI>();
            //下层界面监听了窗体刷新，这里可能进行了无数次扫荡，用这个传回值来确定上层要不要刷
            EventManager.Instance.Dispatch(EventID.RefreshWindow, 1);
        }
    }
}