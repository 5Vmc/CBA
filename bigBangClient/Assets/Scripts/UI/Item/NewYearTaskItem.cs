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
using GameItem = Utils.GameItem.GameItem;

public class NewYearTaskItem : MonoBehaviour
{
    [SerializeField] private RectTransform newYearTaskItem = null;
    [SerializeField] private Image bgImage = null;
    [SerializeField] private Image lightImage = null;
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

    public FestivalTaskInfo festivalTaskInfo = null;
    public FestivalTaskConfig festivalTaskConfig = null;
    public void SetData(FestivalTaskInfo festivalTaskInfo)
    {
        this.festivalTaskInfo = festivalTaskInfo;

        RefreshDataNormal();
    }

    private void RefreshDataNormal()
    {
        if (festivalTaskInfo == null)
        {
            Debug.LogError("ChristmasTaskItem , SetData , festivalTaskInfo == null");
            return;
        }
        this.festivalTaskConfig = Configs.FestivalTask.GetConfig(festivalTaskInfo.Id);
        if (festivalTaskConfig == null)
        {
            Debug.LogError("ChristmasTaskItem , SetData , festivalTaskConfig == null , festivalTaskInfo.Id = {0} , festivalTaskInfo.Id = {1}".SafeFormat(festivalTaskInfo.Id, festivalTaskInfo.Id));
            return;
        }
        List<GameItem> gameItemList = GameItemUtils.CreateGameItems(festivalTaskConfig.Reward).ToList();
        if (gameItemList.Count == 0 || gameItemList[0] == null)
        {
            Debug.LogError("ChristmasTaskItem , SetData , gameItemList.Count == 0 , festivalTaskInfo.Id = {0} , festivalTaskInfo.Id = {1} , festivalTaskConfig.Reward = {2}".SafeFormat(festivalTaskInfo.Id, festivalTaskInfo.Id, festivalTaskConfig.Reward));
            return;
        }
        for (int i = 0; i < inventoryItemList.Count; i++)
        {
            InventoryItem inventoryItem = inventoryItemList[i];
            if (i < gameItemList.Count)
            {
                GameItem gameItem = gameItemList[i];
                inventoryItem.SetData(gameItem);
                inventoryItem.gameObject.SetActive(true);
            }
            else
            {
                inventoryItem.gameObject.SetActive(false);
            }
        }
        titleText.text = festivalTaskConfig.Name.SafeFormat(festivalTaskInfo.Current, festivalTaskConfig.Target);


        hasGetImage.gameObject.SetActive(false);
        waitBtn.gameObject.SetActive(false);
        getBtn.gameObject.SetActive(false);
        if (festivalTaskInfo.Obtain == true)
        {
            hasGetImage.gameObject.SetActive(true);
        }
        else
        {
            bool isCanGet = festivalTaskInfo.Current >= festivalTaskConfig.Target;
            waitBtn.gameObject.SetActive(!isCanGet);
            getBtn.gameObject.SetActive(isCanGet);
        }
    }


    private void OnClickWaitBtn(BabuButton _)
    {
        Tips.PopTips("该任务未完成");
    }
    private void OnClickGetBtn(BabuButton _)
    {
        NetworkManager.Instance.GetFestivalTaskReward(festivalTaskConfig.Id, (GetFestivalTaskRewardResponse getFestivalTaskRewardResponse) =>
        {
            if (getFestivalTaskRewardResponse.Succeed == false)
            {
                EventManager.Instance.Dispatch(EventID.OnFestivalTaskDataChange);
            }
            else
            {
                UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(festivalTaskConfig.Reward));
            }
        });
    }
}
