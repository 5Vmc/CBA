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

public class ChristmasTaskItem : MonoBehaviour
{
    [SerializeField] private InventoryItem inventoryItem = null;
    [SerializeField] private TMP_Text taskTipText = null;
    [SerializeField] private Image hasGetImage = null;
    [SerializeField] private BabuButton goBtn = null;
    [SerializeField] private BabuButton getBtn = null;

    public FestivalTaskInfo festivalTaskInfo = null;
    public FestivalTaskConfig festivalTaskConfig = null;
    public ActivityData activityData = null;
    public int index = 0;
    public void SetData(ActivityData activityData, FestivalTaskInfo festivalTaskInfo, int index)
    {
        this.activityData = activityData;
        this.festivalTaskInfo = festivalTaskInfo;
        this.index = index;
        if (index == 0)
        {
            RefreshDataZero();
        }
        else
        {
            RefreshDataNormal();
        }
    }

    private void RefreshDataZero()
    {
        if (activityData == null)
        {
            Debug.LogError("ChristmasTaskItem , SetData , activityData == null");
            return;
        }
        List<GameItem> gameItemList = GameItemUtils.CreateGameItems(activityData.cfg.DailyGift).ToList();
        if (gameItemList.Count == 0 || gameItemList[0] == null)
        {
            Debug.LogError("ChristmasTaskItem , SetData , gameItemList.Count == 0 , festivalTaskInfo.Id = {0} , festivalTaskInfo.ActivityId = {1} , festivalTaskConfig.Reward = {2}".SafeFormat(festivalTaskInfo.Id, festivalTaskInfo.Id, festivalTaskConfig.Reward));
            return;
        }
        GameItem gameItem = gameItemList[0];
        inventoryItem.SetData(gameItem);
        taskTipText.text = activityData.cfg.DailyGiftDesc;

        bool hasGetFreeBox = ActivityController.Instance.dailyGiftReceivedActivityIdSet.Contains(activityData.cfg.Id);
        hasGetImage.gameObject.SetActive(hasGetFreeBox);
        getBtn.gameObject.SetActive(!hasGetFreeBox);
        goBtn.gameObject.SetActive(false);
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
        GameItem gameItem = gameItemList[0];
        inventoryItem.SetData(gameItem);
        taskTipText.text = festivalTaskConfig.Name + "({0}/{1})".SafeFormat(festivalTaskInfo.Current, festivalTaskConfig.Target);

        hasGetImage.gameObject.SetActive(false);
        goBtn.gameObject.SetActive(false);
        getBtn.gameObject.SetActive(false);
        if (festivalTaskInfo.Obtain == true)
        {
            hasGetImage.gameObject.SetActive(true);
        }
        else
        {
            bool isCanGet = festivalTaskInfo.Current >= festivalTaskConfig.Target;
            goBtn.gameObject.SetActive(!isCanGet);
            getBtn.gameObject.SetActive(isCanGet);
        }
    }

    private void OnEnable()
    {
        goBtn.OnClick += OnClickGoBtn;
        getBtn.OnClick += OnClickGetBtn;
    }
    private void OnDisable()
    {
        goBtn.OnClick -= OnClickGoBtn;
        getBtn.OnClick -= OnClickGetBtn;
    }
    private void OnClickGoBtn(BabuButton _)
    {
        UIController.Instance.CloseWindow<ChristmasTaskUI>();
        var moduleOpen = TriggerManager.Instance.CheckModuleOpen(festivalTaskConfig.ModuleId, true);
        if (moduleOpen)
        {
            TriggerManager.Instance.JumpPanel((TriggerModuleType)festivalTaskConfig.ModuleId);
        }
    }
    private void OnClickGetBtn(BabuButton _)
    {
        if (index == 0)
        {
            GetZero();
        }
        else
        {
            GetNormal();
        }
    }
    private void GetNormal()
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
    private void GetZero()
    {
        NetworkManager.Instance.ReceiveDailyGift(activityData.cfg.Id, (resp) =>
        {
            if (resp.ReceiveSucceed == false)
            {
                Tips.PopTips("领取失败");
                Debug.LogWarningFormat("DailyGiftItem , OnClickGetBtn , GetZero , resp.ReceiveSucceed == false , activityData.cfg.Id = {0}", activityData.cfg.Id);
                return;
            }
            ActivityController.Instance.dailyGiftReceivedActivityIdSet.Add(activityData.cfg.Id);
            var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(activityData.cfg.DailyGift).ToList());
            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
            ActivityController.Instance.RefreshRedDot(activityData);
            EventManager.Instance.Dispatch(EventID.OnFestivalTaskDataChange);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        });
    }

}
