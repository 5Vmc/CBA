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

public class AllStarTaskPad : MonoBehaviour, IActivity
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
    private ActivityData activityData = null;
    public void LoadActivity(ActivityData _data)
    {
        activityData = _data;
        RefreshLeftTimeOneSec();
        RefreshTaskList();
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
        List<FestivalTaskInfo> festivalTaskInfoList = ActivityController.Instance.GetFestivalTaskInfoList(activityData.cfg.Id).OrderBy(a => a.Id).ToList(); ;
        if (festivalTaskInfoList == null || festivalTaskInfoList.Count <= 0)
        {
            Debug.LogWarning("AllStarTaskPad , RefreshTaskList ,  festivalTaskInfoList.Count <= 0 , activityData.cfg.Id = " + activityData.cfg.Id);
        }
        newYearTaskPadAdapter.SetData(festivalTaskInfoList);
    }
}
