using System;
using System.Collections.Generic;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System.Linq;
using Coffee.UIEffects;
using GameConfig.Config;
using Babu.Config;
using Babu;
using static BigBang.SpriteNames;
using static BigBang.AllStarManager;

namespace BigBang.UI
{
    public class AllStarFormationUI : APanelController
    {
        #region 初始化
        protected override void AddListeners()
        {
            helpButton.OnClick += OnClickHelp;
            closeBtn.OnClick += OnClickClose;
            EventManager.Instance.Register(EventID.OnClickAllStarFormationCardItem, OnClickAllStarFormationCardItem);
        }
        protected override void RemoveListeners()
        {
            helpButton.OnClick -= OnClickHelp;
            closeBtn.OnClick -= OnClickClose;
            EventManager.Instance.Unregister(EventID.OnClickAllStarFormationCardItem, OnClickAllStarFormationCardItem);
        }
        #endregion

        #region 按钮回调
        [SerializeField] private BabuButton closeBtn = null;
        private void OnClickClose(BabuButton _)
        {
            AllStarManager.Instance.SyncAllStarData(() =>
            {
                UIController.Instance.HidePanel<AllStarFormationUI>();
            });
        }
        [SerializeField] private BabuButton helpButton = null;
        private void OnClickHelp(BabuButton _)
        {
            UIController.Instance.OpenWindow<AllStarFormationHelpUI>();
        }
        private bool isLockChange = false;
        private void OnClickAllStarFormationCardItem(object[] args)
        {
            AllStarFormationCardItem item = args[0] as AllStarFormationCardItem;
            if (item.isShowUp == true)
            {
                selectedUpItem = item;
                RefreshSelectItem();
                RefreshPlayerTypeText();
                RefreshAdapter();
            }
            else
            {
                if (isLockChange) return;
                isLockChange = true;
                AllStarManager.Instance.UseCard(item.playerCard, (PositionSeparatedType)selectedUpItem.upPosition, () =>
                {
                    isLockChange = false;
                    RefreshTotalCombatImageFont();
                    RefreshUpItem();
                    RefreshSelectItem();
                    RefreshAdapter();
                });
            }
        }
        #endregion

        #region 刷新内容
        protected override void OnPropertiesSet()
        {
            isLockChange = false;
            RefreshArea();
            RefreshTotalCombatImageFont();
            RefreshUpItem();
            SelectDefaultItem();
            RefreshSelectItem();
            RefreshPlayerTypeText();
            RefreshAdapter();
        }

        private AllStarFormationCardItem selectedUpItem = null;
        private void SelectDefaultItem()
        {
            foreach (var item in upItemList)
            {
                if (item.playerCard == null)
                {
                    selectedUpItem = item;
                    return;
                }
            }
            selectedUpItem = upItemList[0];
        }
        private void RefreshSelectItem()
        {
            foreach (var item in upItemList)
            {
                item.SetSelect(item == selectedUpItem);
            }
        }

        [SerializeField] private Image northLogoImage = null;
        [SerializeField] private Image southLogoImage = null;
        [SerializeField] private TMP_Text northCombatTitleText = null;
        [SerializeField] private TMP_Text southCombatTitleText = null;
        [SerializeField] private ImageFont totalCombatImageFont = null;
        private void RefreshArea()
        {
            northLogoImage.gameObject.SetActive(AllStarManager.Instance.serverData.Area == (int)Area.North);
            southLogoImage.gameObject.SetActive(AllStarManager.Instance.serverData.Area == (int)Area.South);
            northCombatTitleText.gameObject.SetActive(AllStarManager.Instance.serverData.Area == (int)Area.North);
            southCombatTitleText.gameObject.SetActive(AllStarManager.Instance.serverData.Area == (int)Area.South);
        }
        private void RefreshTotalCombatImageFont()
        {
            totalCombatImageFont.text = AllStarManager.Instance.savedTotalNowCombatInServer.ToString("N0");
        }

        [SerializeField] private List<AllStarFormationCardItem> upItemList = new();
        private void RefreshUpItem()
        {
            foreach (var item in upItemList)
            {
                if (AllStarManager.Instance.usingCardPositionIdDic.ContainsKey((PositionSeparatedType)item.upPosition))
                {
                    item.SetData(AllStarManager.Instance.usingCardPositionIdDic[(PositionSeparatedType)item.upPosition], true, PositionSeparatedType.All);
                }
                else
                {
                    item.SetData(null, true, PositionSeparatedType.All);
                }
            }
        }

        [SerializeField] private TMP_Text playerTypeText = null;
        private void RefreshPlayerTypeText()
        {
            playerTypeText.text = PlayerCard.GetAdaptPositionAbbreviation((PositionSeparatedType)selectedUpItem.upPosition);
        }

        [SerializeField] private AllStarFormationGridAdapter allStarFormationGridAdapter = null;
        private void RefreshAdapter()
        {
            List<AllStarAdditionConfig> allStarAdditionConfigList = Configs.AllStarAddition.GetConfigList()
                .Where((AllStarAdditionConfig allStarAdditionConfig) =>
                {
                    PlayerCard playerCard = Player.CardManager.GetCard(allStarAdditionConfig.Id);
                    bool hasPlayer = playerCard != null;
                    if (hasPlayer == false) return false;
                    bool samePosition = allStarAdditionConfig.AdaptPosition.Contains(selectedUpItem.upPosition);
                    return samePosition;
                })
                .OrderBy(cfg => cfg.Area != AllStarManager.Instance.serverData.Area)
                .ThenByDescending((AllStarAdditionConfig allStarAdditionConfig) =>
                {
                    PlayerCard playerCard = Player.CardManager.GetCard(allStarAdditionConfig.Id);
                    return playerCard.FightPoint * allStarAdditionConfig.Addition;
                })
                .ToList();
            List<PlayerCard> playerCardList = new();
            foreach (AllStarAdditionConfig allStarAdditionConfig in allStarAdditionConfigList)
            {
                PlayerCard playerCard = Player.CardManager.GetCard(allStarAdditionConfig.Id);
                playerCardList.Add(playerCard);
            }
            allStarFormationGridAdapter.SetData(playerCardList, (PositionSeparatedType)selectedUpItem.upPosition);
        }
        #endregion
    }
}