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
using static BigBang.SpriteNames;
using GameItem = Utils.GameItem.GameItem;

public class LabourDayTaskPad : MonoBehaviour, IActivity
{
    [SerializeField] private NewYearTaskPadAdapter newYearTaskPadAdapter = null;
    [SerializeField] private TMP_Text leftTimeText = null;

    private void OnEnable()
    {
        SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
        EventManager.Instance.Register(EventID.OnFestivalTaskDataChange, OnFestivalTaskDataChange);
        EventManager.Instance.Register(EventID.OnRefreshGoods, RefreshDiceCount);
        diceCountPanel.OnClick += OnClickDiceCountPanel;
        helpButton.OnClick += OnClickHelpButton;
    }
    private void OnDisable()
    {
        SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
        EventManager.Instance.Unregister(EventID.OnFestivalTaskDataChange, OnFestivalTaskDataChange);
        EventManager.Instance.Unregister(EventID.OnRefreshGoods, RefreshDiceCount);
        diceCountPanel.OnClick -= OnClickDiceCountPanel;
        helpButton.OnClick -= OnClickHelpButton;
    }
    private ActivityData activityData = null;
    public void LoadActivity(ActivityData _data)
    {
        activityData = _data;
        RefreshLeftTimeOneSec();
        RefreshTaskList();
        RefreshDiceCount();
    }
    private void RefreshLeftTimeOneSec()
    {
        if (activityData == null) return;
        long leftTime = activityData.EndTime - Utils.DataConvUtil.ServerTime;
        leftTimeText.text = "剩余时间：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
    }
    private void OnFestivalTaskDataChange(object[] args)
    {
        RefreshTaskList();
    }
    private void RefreshTaskList()
    {
        List<FestivalTaskInfo> festivalTaskInfoList = ActivityController.Instance.GetFestivalTaskInfoList(activityData.cfg.Id).OrderBy(a => a.Id).ToList(); ;
        if (festivalTaskInfoList == null || festivalTaskInfoList.Count <= 0)
        {
            Debug.LogWarning("LabourDayTaskPad , RefreshTaskList ,  festivalTaskInfoList.Count <= 0 , activityData.cfg.Id = " + activityData.cfg.Id);
        }
        newYearTaskPadAdapter.SetData(festivalTaskInfoList);
    }



    #region 按钮回调
    [SerializeField] private BabuButton helpButton = null;
    private void OnClickHelpButton(BabuButton button)
    {
        UIController.Instance.OpenWindow<LabourDayHomeHelpUI>();
    }

    [SerializeField] private BabuButton diceCountPanel = null;
    private void OnClickDiceCountPanel(BabuButton _)
    {
        ActivityData activityDataLabourDayHome = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.LabourDayHome);
        if (activityDataLabourDayHome == null)
        {
            Debug.LogWarning("LabourDayTaskPad , OnClickDiceCountPanel , activityDataLabourDayHome == null");
            return;
        }
        GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, int.Parse(activityDataLabourDayHome.cfg.Param2), 0);
        ItemtipsUIProperties itemtipsUIProperties = new ItemtipsUIProperties(gameItem);
        itemtipsUIProperties.SetPos(diceCountPanel.transform, new Vector3(0, -20f, 0));
        UIController.Instance.OpenWindow<ItemtipsUI>(itemtipsUIProperties);
    }
    [SerializeField] private TMP_Text diceNumText = null;
    private void RefreshDiceCount(object[] _ = null)
    {
        ActivityData activityDataLabourDayHome = ActivityController.Instance.GetOneActivityDataByType(ActivityClientType.LabourDayHome);
        if (activityDataLabourDayHome == null)
        {
            Debug.LogWarning("LabourDayTaskPad , OnClickDiceCountPanel , activityDataLabourDayHome == null");
            return;
        }
        GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, int.Parse(activityDataLabourDayHome.cfg.Param2), 0);
        int diceCount = gameItem.GetPlayerCount();
        diceNumText.text = diceCount.ToString();
    }

    #endregion
}
