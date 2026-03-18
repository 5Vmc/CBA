using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using static BigBang.UI.FormationRecoverUI;

namespace BigBang.UI
{
    public class FormationRecoverUIProperties : PanelProperties
    {
        public FormationBase formation = null;
        public SubUIID subUIID = SubUIID.Auto;
        public FormationRecoverUIProperties(FormationBase formation, SubUIID subUIID = SubUIID.Auto)
        {
            this.formation = formation;
            this.subUIID = subUIID;
        }
    }

    public class FormationRecoverUI : APanelController<FormationRecoverUIProperties>
    {

        #region 初始化

        public enum SubUIID
        {
            Auto,
            State,
            Injury,
            Energy,
        }


        protected override void Awake()
        {
            base.Awake();
        }
        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
            bottomToggleGroup.OnValueChanged += OnToggleChanged;
            Babu.EventManager.Instance.Register(EventID.OnClickFormationRecoverCardItem, OnClickFormationRecoverCardItem);
            normalAddButton.OnClick += OnClickNormalAddButton;
            advanceAddButton.OnClick += OnClickAdvanceAddButton;
            helpButton.OnClick += OnClickHelpButton;
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
            bottomToggleGroup.OnValueChanged -= OnToggleChanged;
            Babu.EventManager.Instance.Unregister(EventID.OnClickFormationRecoverCardItem, OnClickFormationRecoverCardItem);
            normalAddButton.OnClick -= OnClickNormalAddButton;
            advanceAddButton.OnClick -= OnClickAdvanceAddButton;
            helpButton.OnClick -= OnClickHelpButton;
        }

        [SerializeField] private FormationRecroverUIAnim formationRecroverUIAnim = null;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            selectItem = null;
            RefreshCardSelect();
            if (Properties.subUIID == SubUIID.Auto)
            {
                Properties.subUIID = SubUIID.State;
            }
            SetPlayerCard();
            bottomToggleGroup.Switch((int)Properties.subUIID - 1);
            selectItem = firstItemList[0];
            formationRecroverUIAnim?.PlayEnter(RefreshCardSelect);
        }

        [SerializeField] private BabuButton helpButton = null;
        private void OnClickHelpButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<RecoverHelpUI>();
        }

        #endregion

        #region 刷新界面

        private FormationRecoverCardItem selectItem = null;
        private void OnClickFormationRecoverCardItem(object[] args)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);// 点击卡牌音效
            selectItem = args[0] as FormationRecoverCardItem;
            RefreshCardSelect();
        }
        private void RefreshCardSelect()//刷新高亮
        {
            foreach (var item in firstItemList)
            {
                item.SetSelect(selectItem == item);
            }
            foreach (var item in benchItemList)
            {
                item.SetSelect(selectItem == item);
            }
        }

        [SerializeField] private List<FormationRecoverCardItem> firstItemList = new();
        [SerializeField] private List<FormationRecoverCardItem> benchItemList = new();

        private void SetPlayerCard()
        {
            List<PlayerCard> startCardList = Properties.formation.GetStarterCards();
            for (int i = 0; i < 5; i++)
            {
                FormationRecoverCardItem item = firstItemList[i];
                PlayerCard card = startCardList[i];
                item.SetCardData(card);
            }
            List<PlayerCard> benchCardList = Properties.formation.GetSubstituteCards();
            benchCardList = benchCardList.Where(item => item != null).ToList();
            for (int i = 0; i < 7; i++)
            {
                FormationRecoverCardItem item = benchItemList[i];
                PlayerCard card = i < benchCardList.Count ? benchCardList[i] : null;
                item.SetCardData(card);
            }
        }
        FormationRecoverCardItemType itemType = FormationRecoverCardItemType.State;
        private void SetType(FormationRecoverCardItemType type)
        {
            this.itemType = type;
            foreach (var item in firstItemList)
            {
                item.SetType(type);
            }
            foreach (var item in benchItemList)
            {
                item.SetType(type);
            }
        }

        #endregion

        #region 关闭界面
        [SerializeField] private BabuButton closeBtn = null;
        private void OnClose(BabuButton _)
        {
            UIController.Instance.HidePanel<FormationRecoverUI>();
        }
        #endregion

        #region 切换页签
        [SerializeField] private BabuToggleGroup bottomToggleGroup = null;
        [SerializeField] private ResourceTitle resourceTitle = null;

        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            int selectedIndex = bottomToggleGroup.EnableIndex + 1;
            ShowPad((SubUIID)selectedIndex);
        }
        private void ShowPad(SubUIID padIndex)
        {
            switch (padIndex)
            {
                case SubUIID.State: OnShowState(); break;
                case SubUIID.Injury: OnShowInjury(); break;
                case SubUIID.Energy: OnShowEnergy(); break;
            }
        }

        [SerializeField] private Image normalIconImage = null;
        [SerializeField] private Image advanceIconImage = null;
        [SerializeField] private RectTransform buttonTipStatePanel = null;
        [SerializeField] private RectTransform buttonTipInjuryPanel = null;
        [SerializeField] private RectTransform buttonTipEnergyPanel = null;

        GoodsGameItem coachQuotesGameItem = null;
        GoodsGameItem advCoachQuotesGameItem = null;
        private async void OnShowState()
        {
            Debug.Log("OnShowState");
            resourceTitle.SetOnlyShowGoodsList(new List<int>() { GoodsId.CoachQuotes, GoodsId.AdvCoachQuotes });
            SetType(FormationRecoverCardItemType.State);
            coachQuotesGameItem = new(GoodsId.CoachQuotes, 1);
            advCoachQuotesGameItem = new(GoodsId.AdvCoachQuotes, 1);
            normalIconImage.sprite = await coachQuotesGameItem.GetIcon();
            advanceIconImage.sprite = await advCoachQuotesGameItem.GetIcon();
            buttonTipStatePanel.gameObject.SetActive(true);
            buttonTipInjuryPanel.gameObject.SetActive(false);
            buttonTipEnergyPanel.gameObject.SetActive(false);
        }
        GoodsGameItem medicalBoxGameItem = null;
        GoodsGameItem advMedicalBoxGameItem = null;
        private async void OnShowInjury()
        {
            Debug.Log("OnShowInjury");
            resourceTitle.SetOnlyShowGoodsList(new List<int>() { GoodsId.MedicalBox, GoodsId.AdvMedicalBox });
            SetType(FormationRecoverCardItemType.Injury);
            medicalBoxGameItem = new(GoodsId.MedicalBox, 1);
            advMedicalBoxGameItem = new(GoodsId.AdvMedicalBox, 1);
            normalIconImage.sprite = await medicalBoxGameItem.GetIcon();
            advanceIconImage.sprite = await advMedicalBoxGameItem.GetIcon();
            buttonTipStatePanel.gameObject.SetActive(false);
            buttonTipInjuryPanel.gameObject.SetActive(true);
            buttonTipEnergyPanel.gameObject.SetActive(false);
        }
        GoodsGameItem energyDrinkGameItem = null;
        GoodsGameItem advEnergyDrinkGameItem = null;
        private async void OnShowEnergy()
        {
            Debug.Log("OnShowEnergy");
            resourceTitle.SetOnlyShowGoodsList(new List<int>() { GoodsId.EnergyDrink, GoodsId.AdvEnergyDrink });
            SetType(FormationRecoverCardItemType.Energy);
            energyDrinkGameItem = new(GoodsId.EnergyDrink, 1);
            advEnergyDrinkGameItem = new(GoodsId.AdvEnergyDrink, 1);
            normalIconImage.sprite = await energyDrinkGameItem.GetIcon();
            advanceIconImage.sprite = await advEnergyDrinkGameItem.GetIcon();
            buttonTipStatePanel.gameObject.SetActive(false);
            buttonTipInjuryPanel.gameObject.SetActive(false);
            buttonTipEnergyPanel.gameObject.SetActive(true);
        }

        #endregion

        #region 使用道具

        [SerializeField] private BabuButton normalAddButton = null;
        [SerializeField] private BabuButton advanceAddButton = null;

        private void OnClickNormalAddButton(BabuButton _)
        {
            OnClickAddButton(true);
        }
        private void OnClickAdvanceAddButton(BabuButton _)
        {
            OnClickAddButton(false);
        }
        private bool CheckNeedUse(bool isNormal)
        {
            if (selectItem == null)
            {
                Tips.PopTips("请选择需要恢复状态的球员");
                return false;
            }
            switch (itemType)
            {
                case FormationRecoverCardItemType.State:
                    {
                        if (selectItem.playerCard.Status == PlayerCardStatus.VeryGood)
                        {
                            Tips.PopTips("当前球员状态爆棚");
                            return false;
                        }
                        if (isNormal && (selectItem.playerCard.Status == PlayerCardStatus.Good || selectItem.playerCard.Status == PlayerCardStatus.Ordinary))
                        {
                            Tips.PopTips("当前球员状态良好");
                            return false;
                        }
                    }
                    break;
                case FormationRecoverCardItemType.Injury:
                    {
                        if (selectItem.playerCard.IsHurt() == false)
                        {
                            Tips.PopTips("当前球员没有伤病");
                            return false;
                        }
                    }
                    break;
                case FormationRecoverCardItemType.Energy:
                    {
                        if (selectItem.playerCard.Energy >= GameConst.CardInitEnergy)
                        {
                            Tips.PopTips("当前球员能量满满");
                            return false;
                        }
                    }
                    break;
            }
            return true;
        }
        private void OnClickAddButton(bool isNormal)
        {
            if (CheckNeedUse(isNormal) == false) return;
            GoodsGameItem goodsGameItem = null;
            switch (itemType)
            {
                case FormationRecoverCardItemType.State: goodsGameItem = isNormal ? coachQuotesGameItem : advCoachQuotesGameItem; break;
                case FormationRecoverCardItemType.Injury: goodsGameItem = isNormal ? medicalBoxGameItem : advMedicalBoxGameItem; break;
                case FormationRecoverCardItemType.Energy: goodsGameItem = isNormal ? energyDrinkGameItem : advEnergyDrinkGameItem; break;
            }
            string error = Player.PackageManager.IsGameItemEnough(goodsGameItem);
            if (error != "") return;
            NetworkManager.Instance.RecoverPlayer(selectItem.playerCard.CardId, goodsGameItem.Id, (resp) =>
            {
                SetType(itemType);
            });
        }

        #endregion

    }
}