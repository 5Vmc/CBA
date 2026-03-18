using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BigBang;
using BigBang.UI;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

public class AllStarCambatRewardItem : MonoBehaviour
{
    [SerializeField] public List<InventoryItem> inventoryItemList = new();
    [SerializeField] public BabuButton getButton = null;
    [SerializeField] public Image hasGetImage = null;
    [SerializeField] public BabuButton notSuccessButton = null;

    public AllStarRewardConfig allStarRewardConfig = null;
    public void SetData(AllStarRewardConfig allStarRewardConfig)
    {
        this.allStarRewardConfig = allStarRewardConfig;
        RefreshShow();
        RefreshItem();
    }
    private void RefreshItem()
    {
        List<GameItem> gameItemList = GameItemUtils.CreateGameItems(allStarRewardConfig.Rewards).ToList();
        GameItemUtils.SetRewards(inventoryItemList, gameItemList);
    }
    private void RefreshShow()
    {
        notSuccessButton.gameObject.SetActive(false);
        getButton.gameObject.SetActive(false);
        hasGetImage.gameObject.SetActive(false);
        bool isAchieveTheGoal = allStarRewardConfig.Option <= AllStarManager.Instance.savedTotalMaxCombatInServer;
        if (isAchieveTheGoal == false)
        {
            notSuccessButton.gameObject.SetActive(true);
        }
        else
        {
            bool hasGet = AllStarManager.Instance.strengthRewardGotOptionSet.Contains(allStarRewardConfig.Option);
            if (hasGet)
            {
                hasGetImage.gameObject.SetActive(true);
            }
            else
            {
                getButton.gameObject.SetActive(true);
            }
        }
    }

    private void OnEnable()
    {
        getButton.OnClick += OnClickGetButton;
        notSuccessButton.OnClick += OnClickNotSuccessButton;
    }
    private void OnDisable()
    {
        getButton.OnClick -= OnClickGetButton;
        notSuccessButton.OnClick -= OnClickNotSuccessButton;
    }

    private void OnClickGetButton(BabuButton _)
    {
        NetworkManager.Instance.GetAllStarStrengthReward(allStarRewardConfig.Option, (getAllStarStrengthRewardResponse) =>
        {
            if (getAllStarStrengthRewardResponse.Success)
            {
                if (AllStarManager.Instance.strengthRewardGotOptionSet.Contains(allStarRewardConfig.Option) == false)
                {
                    AllStarManager.Instance.strengthRewardGotOptionSet.Add(allStarRewardConfig.Option);
                }
                UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(allStarRewardConfig.Rewards));
                RefreshShow();
            }
            else
            {
                UIController.Instance.HidePanel<AllStarCombatRewardUI>();
            }
        });
    }
    private void OnClickNotSuccessButton(BabuButton _)
    {
        Tips.PopTips("提供战力达{0}后可领取此奖励".SafeFormat(allStarRewardConfig.Option));
    }

}
