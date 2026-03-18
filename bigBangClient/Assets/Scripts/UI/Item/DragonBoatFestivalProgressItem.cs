using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using static BigBang.DragonBoatFestivalManager;
using GameItem = Utils.GameItem.GameItem;

public class DragonBoatFestivalProgressItem : MonoBehaviour
{
    [SerializeField] private RectTransform dragonBoatFestivalProgressItem = null;
    [SerializeField] private Image bgImage = null;
    [SerializeField] private Image hasGetImage = null;
    [SerializeField] private BabuButton getBtn = null;
    [SerializeField] private BabuButton waitBtn = null;
    [SerializeField] private TMP_Text titleText = null;
    [SerializeField] private HorizontalLayoutGroup goodsPanel = null;
    [SerializeField] private List<InventoryItem> inventoryItemList = null;

    private void OnEnable()
    {
        waitBtn.OnClick += OnClickWaitBtn;
        getBtn.OnClick += OnClickGetBtn;
    }
    private void OnDisable()
    {
        waitBtn.OnClick -= OnClickWaitBtn;
        getBtn.OnClick -= OnClickGetBtn;
    }

    public DragonBoatRewardConfig dragonBoatRewardConfig = null;
    public void SetData(DragonBoatRewardConfig dragonBoatRewardConfig)
    {
        this.dragonBoatRewardConfig = dragonBoatRewardConfig;

        RefreshDataNormal();
    }

    private void RefreshDataNormal()
    {
        if (dragonBoatRewardConfig == null)
        {
            Debug.LogError("DragonBoatFestivalProgressItem , SetData , dragonBoatRewardConfig == null");
            return;
        }
        GameItemUtils.SetRewards(inventoryItemList, dragonBoatRewardConfig.Rewards);
        titleText.text = "所在龙舟队累计前进{0}".SafeFormat(dragonBoatRewardConfig.Name);


        hasGetImage.gameObject.SetActive(false);
        waitBtn.gameObject.SetActive(false);
        getBtn.gameObject.SetActive(false);

        RewardState rewardState = DragonBoatFestivalManager.Instance.GetRewardState(dragonBoatRewardConfig);
        hasGetImage.gameObject.SetActive(rewardState == RewardState.HasGot);
        waitBtn.gameObject.SetActive(rewardState == RewardState.CanNotGet);
        getBtn.gameObject.SetActive(rewardState == RewardState.CanGet);
    }


    private void OnClickWaitBtn(BabuButton _)
    {
        Tips.PopTips("该任务未完成");
    }
    private void OnClickGetBtn(BabuButton _)
    {
        DragonBoatFestivalManager.Instance.GetDragonBoatMetersReward(dragonBoatRewardConfig, () =>
        {
            UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(dragonBoatRewardConfig.Rewards));
            RefreshDataNormal();
        });
    }
}
