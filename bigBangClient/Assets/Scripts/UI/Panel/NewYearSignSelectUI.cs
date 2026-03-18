using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using Utils;
using BigBang.Animation;
using System;
using System.Collections.Generic;
using static BigBang.Battle.ShootUI;
using Utils.GameItem;
using System.Linq;
using Protocol;
using GameItem = Utils.GameItem.GameItem;
using Babu;

namespace BigBang.UI
{
    public class NewYearSignSelectUIProperties : WindowProperties
    {
        public NewYearSignItem newYearSignItem;
        public NewYearSignSelectUIProperties(NewYearSignItem newYearSignItem)
        {
            this.newYearSignItem = newYearSignItem;
        }
    }
    public class NewYearSignSelectUI : AWindowController<NewYearSignSelectUIProperties>
    {
        [SerializeField] private Button closeBtn = null;
        [SerializeField] private List<NewYearSignSelectItem> selectItemList = null;
        [SerializeField] private List<Image> darkImage = null;
        [SerializeField] private List<Image> selectImage = null;
        [SerializeField] private BabuButton startButton = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            startButton.OnClick += OnClickStartButton;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            startButton.OnClick -= OnClickStartButton;
        }

        private void OnClickStartButton(BabuButton button)
        {
            UIController.Instance.OpenWindow<ConfirmationBoxUI>(new ConfirmationBoxUIProperties("奖励选择后不可更改，是否确定？", () =>
            {
                NetworkManager.Instance.SetWishSign(Properties.newYearSignItem.activityData.cfg.Id, selectItenIndex, (SetWishSignResponse setWishSignResponse) =>
                {
                    ActivityController.Instance.wishSigns.Add(selectItenIndex);
                    OnClose();
                    EventManager.Instance.Dispatch(EventID.OnNewYearSignSelectItemSet);
                    ActivityController.Instance.RefreshNewYearSignRedDot();
                    EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
                });
            }));
        }

        private void OnClose()
        {
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);

            UIController.Instance.CloseWindow<NewYearSignSelectUI>();
        }
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            Debug.Log("NewYearSignSelectUI , OnPropertiesSet , Properties.newYearSignItem.itemIndex = " + Properties.newYearSignItem.itemIndex);

            InitDataOnce();

            RefreshItemDark();
            selectItenIndex = GetFirstCanUseItem().itemIndex;
            RefreshItemSelect();
        }

        private bool isInitDataOnce = false;
        private void InitDataOnce()
        {
            if (isInitDataOnce) return;

            List<GameItem> gameItemList = GameItemUtils.CreateGameItems(Properties.newYearSignItem.wishSignConfig.Rewards).ToList();
            for (int i = 0; i < 8; i++)
            {
                selectItemList[i].SetDataOnce(gameItemList[i], i + 1, OnClickNewYearSignSelectItem);
            }
        }

        private int selectItenIndex = 1;//选中的物品序号，从 1 开始
        private void OnClickNewYearSignSelectItem(NewYearSignSelectItem newYearSignSelectItem)
        {
            selectItenIndex = newYearSignSelectItem.itemIndex;
            RefreshItemSelect();
        }

        private void RefreshItemDark()
        {
            foreach (NewYearSignSelectItem newYearSignSelectItem in selectItemList)
            {
                newYearSignSelectItem.RefreshDark();
            }
        }
        private NewYearSignSelectItem GetFirstCanUseItem()
        {
            return selectItemList.FirstOrDefault(item => !item.isDark);
        }
        private void RefreshItemSelect()
        {
            for (int i = 0; i < 8; i++)
            {
                selectItemList[i].SetSelect(i + 1 == selectItenIndex);
            }
        }
    }
}