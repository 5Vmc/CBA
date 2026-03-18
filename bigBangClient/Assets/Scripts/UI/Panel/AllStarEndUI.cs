using System.Collections.Generic;
using System.Linq;
using Babu;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using static BigBang.AllStarManager;

namespace BigBang.UI
{
    public class AllStarEndUI : AWindowController
    {
        [SerializeField] private BabuButton closeButton = null;
        [SerializeField] private BabuButton confirmBtn = null;
        protected override void AddListeners()
        {
            closeButton.OnClick += OnClickCloseButton;
            confirmBtn.OnClick += OnClickConfirmBtn;
        }
        protected override void RemoveListeners()
        {
            closeButton.OnClick -= OnClickCloseButton;
            confirmBtn.OnClick -= OnClickConfirmBtn;
        }

        protected override void OnPropertiesSet()
        {
            RefreshWin();
            UnityEngine.PlayerPrefs.SetInt(PlayerPrefsKeys.AllStar2024ShowEnd + Player.GbId, 1);
        }

        private void OnClickCloseButton(BabuButton _)
        {
            UIController.Instance.CloseWindow<AllStarEndUI>();
        }
        private void OnClickConfirmBtn(BabuButton _)
        {
            UIController.Instance.CloseWindow<AllStarEndUI>();
        }

        [SerializeField] private List<Image> northImageList;
        [SerializeField] private List<Image> southImageList;
        [SerializeField] private ImageFont southImageFont = null;
        [SerializeField] private ImageFont northImageFont = null;
        [SerializeField] private Image southWinImage = null;
        [SerializeField] private Image northWinImage = null;
        [SerializeField] public List<InventoryItem> inventoryItemList = new();
        private void RefreshWin()
        {
            southImageFont.text = AllStarManager.Instance.serverData.South.ToString("N0");
            northImageFont.text = AllStarManager.Instance.serverData.North.ToString("N0");
            bool isNorthWin = AllStarManager.Instance.serverData.North >= AllStarManager.Instance.serverData.South;
            bool isSouthWin = AllStarManager.Instance.serverData.South >= AllStarManager.Instance.serverData.North;
            northWinImage.gameObject.SetActive(isNorthWin);
            southWinImage.gameObject.SetActive(isSouthWin);

            Area posterArea = Area.North;
            if (isNorthWin && isSouthWin)
            {
                posterArea = (Area)AllStarManager.Instance.serverData.Area;
            }
            else if (isNorthWin)
            {
                posterArea = Area.North;
            }
            else
            {
                posterArea = Area.South;
            }
            foreach (var item in northImageList)
            {
                item.gameObject.SetActive(posterArea == Area.North);
            }
            foreach (var item in southImageList)
            {
                item.gameObject.SetActive(posterArea == Area.South);
            }

            bool isSelfWin = (isNorthWin && AllStarManager.Instance.serverData.Area == (int)Area.North) || (isSouthWin && AllStarManager.Instance.serverData.Area == (int)Area.South);
            AllStarRewardConfig allStarRewardConfig = null;
            if (isSelfWin)
            {
                allStarRewardConfig = Configs.AllStarReward.GetConfigList().FirstOrDefault(cfg => cfg.Type == 1 && cfg.Option == 1);
            }
            else
            {
                allStarRewardConfig = Configs.AllStarReward.GetConfigList().FirstOrDefault(cfg => cfg.Type == 1 && cfg.Option == 2);
            }
            List<GameItem> gameItemList = GameItemUtils.CreateGameItems(allStarRewardConfig.Rewards).ToList();
            GameItemUtils.SetRewards(inventoryItemList, gameItemList);
        }
    }
}