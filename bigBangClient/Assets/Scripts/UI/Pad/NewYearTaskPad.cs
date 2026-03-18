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
using static BigBang.SpriteNames;

public class NewYearTaskPad : MonoBehaviour
{
    [SerializeField] private NewYearTaskPadAdapter newYearTaskPadAdapter = null;
    [SerializeField] private TMP_Text leftTimeText = null;

    private void OnEnable()
    {
        SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
        EventManager.Instance.Register(EventID.OnFestivalTaskDataChange, OnFestivalTaskDataChange);
    }
    private void OnDisable()
    {
        SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
        EventManager.Instance.Unregister(EventID.OnFestivalTaskDataChange, OnFestivalTaskDataChange);
    }
    public void OnShow()
    {
        RefreshActivityData();
        RefreshLeftTimeOneSec();
        RefreshTaskList();
    }
    private ActivityData activityData = null;
    private FestivalBoxConfig festivalBoxConfig = null;
    private void RefreshActivityData()
    {
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.NewYearTask) == false)
        {
            Debug.LogWarning("NewYearTaskPad , RefreshActivityData , ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.NewYearTask) == false");
            activityData = null;
            return;
        }
        activityData = ActivityController.Instance.OnlineActivityDic[ActivityID.NewYearTask];
    }
    private void RefreshLeftTimeOneSec()
    {
        if (activityData == null) return;
        long leftTime = activityData.EndTime - Utils.DataConvUtil.ServerTime;
        leftTimeText.text = "活动剩余时间：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
    }
    private void OnFestivalTaskDataChange(object[] args)
    {
        RefreshTaskList();
    }
    private void RefreshTaskList()
    {
        List<FestivalTaskInfo> festivalTaskInfoList = ActivityController.Instance.GetFestivalTaskInfoList(ActivityID.NewYearTask).OrderBy(a => a.Id).ToList(); ;
        if (festivalTaskInfoList == null || festivalTaskInfoList.Count <= 0)
        {
            Debug.LogWarning("NewYearTaskPad , RefreshTaskList ,  festivalTaskInfoList.Count <= 0 , ActivityID.NewYearTask = " + ActivityID.NewYearTask);
        }
        newYearTaskPadAdapter.SetData(festivalTaskInfoList);
    }
}
