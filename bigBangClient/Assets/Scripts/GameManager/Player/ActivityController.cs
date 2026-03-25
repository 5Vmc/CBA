using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using Babu.Config;
using Babu.SDK;
using BigBang;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Google.Protobuf.Collections;
using Protocol;
using UnityEngine;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

/// <summary>
/// 对应活动表的ClientType
/// </summary>
public enum ActivityClientType
{
    Unknow = 0,

    /// <summary>首充</summary>
    FirstPay = 1,
    /// <summary>每日充值</summary>
    DailyPay = 2,
    /// <summary>排行奖励</summary>
    RankAwards = 3,
    /// <summary>累计充值</summary>
    TotalPay = 4,
    /// <summary>战令</summary>
    BattlePass = 5,
    /// <summary>触发礼包</summary>
    TimeGift = 6,
    /// <summary>限时抽卡</summary>
    TimeRecruit = 7,
    /// <summary>小额礼包</summary>
    GiftPay = 8,
    /// <summary>7日签到</summary>
    Sign7Day = 9,
    /// <summary>30日签到</summary>
    Sign30Day = 10,
    /// <summary>新手目标</summary>
    NoviceTarget = 11,
    /// <summary>月卡</summary>
    MonthCard = 12,
    /// <summary>国庆签到</summary>
    NationalDayLogin = 13,
    /// <summary>周琦回归</summary>
    ZhouQiBack = 14,
    /// <summary>魔王好礼</summary>
    ZhouQiGift = 15,
    /// <summary>圣诞宝箱</summary>
    ChristmasTree = 16,
    /// <summary>圣诞任务</summary>
    ChristmasTask = 17,
    /// <summary>元旦主页</summary>
    NewYearMain = 18,
    /// <summary>元旦任务</summary>
    NewYearTask = 19,
    /// <summary>新年礼包</summary>
    NewYearGift = 20,
    /// <summary>跨年签到</summary>
    NewYearWish = 21,
    /// <summary>教练训话</summary>
    EnergyCenter = 22,
    /// <summary>弹出礼包</summary>
    TimeGiftCollection = 23,
    /// <summary>春节礼包</summary>
    SpringFestivalGift = 24,
    /// <summary>春节任务</summary>
    SpringFestivalTask = 25,
    /// <summary>龙年红包</summary>
    DragonYearRedEnvelope = 26,
    /// <summary>春节签到</summary>
    SpringFestivalWish = 27,
    /// <summary>杨瀚森主页</summary>
    YangHanSenMainPage = 28,
    /// <summary>李晓旭主页</summary>
    LiXiaoXuMainPage = 29,
    /// <summary>全明星主页</summary>
    AllStarHome = 30,
    /// <summary>全明星礼包</summary>
    AllStarGift = 31,
    /// <summary>全明星任务</summary>
    AllStarTask = 32,
    /// <summary>五一礼包</summary>
    LabourDayGift = 33,
    /// <summary>五一任务</summary>
    LabourDayTask = 34,
    /// <summary>五一祈愿</summary>
    LabourDaySign = 35,
    /// <summary>五一棋盘</summary>
    LabourDayHome = 36,
    /// <summary>全明星限时抽卡</summary>
    AllStarTimeRecruit = 37,
    /// <summary>季后赛总决赛竞猜2024-主页</summary>
    PlayoffFinalsGuessHome = 38,
    /// <summary>季后赛总决赛竞猜2024-每场比赛预测</summary>
    PlayoffFinalsGuessSingle = 39,
    /// <summary>端午节龙舟赛2024-主页</summary>
    DragonBoatFestivalHome = 40,
    /// <summary>端午节龙舟赛2024-任务</summary>
    DragonBoatFestivalTask = 41,
    /// <summary>端午节龙舟赛2024-祈愿</summary>
    DragonBoatFestivalSign = 42,
    /// <summary>端午节龙舟赛2024-礼包</summary>
    DragonBoatFestivalGift = 43,
    /// <summary>奥运会2024-祈愿</summary>
    Olympics2024Sign = 44,
    /// <summary>奥运会2024-礼包</summary>
    Olympics2024Gift = 45,
}

public class BuyRecord
{
    public int receiveCount = 0;
    public int payCount = 0;
}

public class ActivityData
{
    /// <summary>
    /// 循环活动才有意义，当前是第几届，从1开始。
    /// </summary>
    public long Index;
    /// <summary>
    /// 开始时间
    /// </summary>
    public long StartTime;
    /// <summary>
    /// 结束时间
    /// </summary>
    public long EndTime;
    /// <summary>
    /// 消失时间
    /// </summary>
    public long HideTime;

    /// <summary>
    /// 模板文件
    /// </summary>
    public ActivityConfig cfg { get; set; }
    /// <summary>
    /// 用户参与的数据
    /// </summary>
    public ActivityPayInfoData payData { get; set; }

    public List<RankAwardItemData> RankData { get; set; }
    public RankAwardItemData MyRankData { get; set; }

    public ActivityClientType clientType { get; set; }

    public bool IsEnd
    {
        get
        {
            return EndTime < Utils.DataConvUtil.ServerTime;
        }
    }
    public bool IsHide
    {
        get
        {
            return HideTime < Utils.DataConvUtil.ServerTime;
        }
    }

    /// <summary>
    /// 是否已经领取完了所有奖励，领取完就不需要展示了。
    /// </summary>
    public bool IsGotAllRewards
    {
        get
        {
            if (payData == null) return false;
            if (cfg == null) return false;
            if (cfg.ClientType == (int)ActivityClientType.FirstPay)
            {
                int count = Configs.ActivityPayReward.GetConfigList().Count(cfg => cfg.ActivityId == this.cfg.Id);
                return payData.ReceiveSet.Count >= count;
            }
            else if (cfg.ClientType == (int)ActivityClientType.NationalDayLogin)
            {
                int count = Configs.FestivalLogin.GetConfigList().Count(cfg => cfg.ActivityId == this.cfg.Id);
                return payData.ReceiveSet.Count >= count;
            }
            else if (cfg.ClientType == (int)ActivityClientType.DailyPay)
            {
                return false;
            }
            else if (cfg.ClientType == (int)ActivityClientType.BattlePass)
            {
                return false;
            }
            else if (cfg.ClientType == (int)ActivityClientType.GiftPay)
            {
                return false;
            }
            return false;
        }
    }
    private ActivityData() { }
    public ActivityData(ActivityConfig _cfg)
    {
        cfg = _cfg;
        payData = new ActivityPayInfoData();
        MyRankData = new();
        RankData = new();
        try
        {
            clientType = (ActivityClientType)cfg.ClientType;
        }
        catch
        {
            if (cfg != null)
            {
                Debug.LogWarning("ActivityData , new(ActivityConfig _cfg) , unknow ClientType , cfg.ID = " + cfg.Id + " , clientType = " + cfg.Type);
            }
            else
            {
                Debug.LogWarning("ActivityData , new(ActivityConfig _cfg) , unknow ClientType , cfg == null");
            }
        }
    }
}

public class ActivityPayInfoData
{
    public ActivityPayInfoData()
    {

    }
    public ActivityPayInfoData(ActivityPointInfo info)
    {
        ActivityId = info.ActivityId;
        TotalPay = info.Point;
        TodayPay = info.TodayPoint;
        Days = info.Point;
        TaskPoint = info.Point;
        hasBuy = info.BonusUnlock;
        ResetReceive(info.ReceiveList.ToList());
    }

    public int ActivityId { get; set; }
    /// <summary>
    /// 总充值，累充活动用
    /// </summary>
    public int TotalPay { get; set; }
    /// <summary>
    /// 今日充值
    /// </summary>
    public int TodayPay { get; set; }
    /// <summary>
    /// 完成充值目标的天数,每日充值类活动用
    /// </summary>
    public int Days { get; set; }
    /// <summary>
    /// 战令点数
    /// </summary>
    public int TaskPoint { get; set; }
    /// <summary>
    /// 是否已购买
    /// </summary>
    public bool hasBuy { get; set; }

    /// <summary>
    /// 奖励是否已经被领取
    /// </summary>
    public bool HasReceive(int receiveId)
    {
        return receiveSet.Contains(receiveId);
    }

    private HashSet<int> receiveSet = new();
    public HashSet<int> ReceiveSet
    {
        get
        {
            return receiveSet;
        }
    }

    public Dictionary<int, BuyRecord> BuyRecordDict = new();

    /// <summary>
    /// 领奖后服务器不会推送更新，需修改内存记录
    /// </summary>
    public void AddReceive(int receiveId)
    {
        if (receiveSet.Contains(receiveId) == true) return;
        receiveSet.Add(receiveId);
    }
    /// <summary>
    /// 领奖后服务器不会推送更新，需修改内存记录
    /// </summary>
    public void AddReceive(List<int> receiveIdList)
    {
        foreach (var receiveId in receiveIdList)
        {
            AddReceive(receiveId);
        }
    }
    public void ResetReceive(List<int> receiveList)
    {
        receiveSet.Clear();
        foreach (int receiveId in receiveList)
        {
            AddReceive(receiveId);
        }
    }

}

public class ActivityToggleData
{
    public ActivityConfig activityConfig = null;
    public ActivityData activityData = null;
}

public class ActivityController : Singleton<ActivityController>
{

    public Dictionary<int, int> EnergyRecord = new Dictionary<int, int>() { { 1, 0 }, { 2, 0 }, { 3, 0 } };
    private List<(int, int)> EnergyTime = new() { (12, 18), (18, 21), (21, 23) };

    private void ResetEnergyRecord()
    {
        EnergyRecord = new Dictionary<int, int>() { { 1, 0 }, { 2, 0 }, { 3, 0 } };
        ActivityController.Instance.RefreshClientRedDot(ActivityClientType.EnergyCenter);
        EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
    }

    public List<ActivityToggleData> GetActivityToggleDataList(List<ActivityClientType> wantShowTypeList)
    {
        List<ActivityToggleData> toggleDataList = new();

        foreach (ActivityClientType eActivityType in wantShowTypeList)
        {
            List<ActivityData> activityDataList = ActivityController.Instance.GetAllActivityDataByType(eActivityType);
            if (activityDataList.Count > 0)
            {
                foreach (ActivityData activityData in activityDataList)
                {
                    //if (activityData.IsGotAllRewards) continue;
                    if (IsNeedHideInToggle(activityData.cfg)) continue;
                    ActivityToggleData toggleData = new();
                    toggleData.activityData = activityData;
                    toggleData.activityConfig = activityData.cfg;
                    toggleDataList.Add(toggleData);
                }
            }
            else
            {
                List<ActivityConfig> activityConfigList = ActivityController.Instance.GetAllConfigByType(eActivityType);
                if (activityConfigList != null && activityConfigList.Count > 0)
                {
                    foreach (ActivityConfig activityConfig in activityConfigList)
                    {
                        if (activityConfig.ServerDays != -1) continue;
                        if (IsNeedHideInToggle(activityConfig)) continue;
                        ActivityToggleData toggleData = new();
                        toggleData.activityConfig = activityConfig;
                        toggleDataList.Add(toggleData);
                    }
                }
            }
        }
        return toggleDataList;
    }
    public bool IsNeedHideInToggle(ActivityConfig cfg)
    {
        if (cfg.ServerDays == -1)
        {
            if (cfg.ClientType == (int)ActivityClientType.Sign7Day)
            {
                //7日签到领完了
                if (Player.ActivityManager.GetIsSevenSignFinish()) return true;
            }
            if (cfg.ClientType == (int)ActivityClientType.TimeGiftCollection)
            {
                //弹出礼包为空
                if (!TimeGiftController.Instance.HasGift) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 当前是领第几顿体力
    /// </summary>
    /// <returns></returns>
    public int GetCurrentEnergyStatus()
    {
        DateTime time = DateTimeOffset.FromUnixTimeSeconds((long)Utils.DataConvUtil.ServerTime).DateTime.ToLocalTime();

        //哪个可以领取
        var enableIndex = -1;

        for (var index = 0; index < EnergyTime.Count; index++)
        {
            if (time.Hour >= EnergyTime[index].Item1 && time.Hour < EnergyTime[index].Item2)
            {
                enableIndex = index;
                break;
            }
        }
        return enableIndex;
    }

    /// <summary>
    /// 领取体力奖励
    /// </summary>
    /// <param name="index">每天的第几次</param>
    /// <param name="callback"></param>
    public void GetEnergyReward(int index, Action<GetEnergyTimeLimitResponse> callback)
    {
        NetworkManager.Instance.GetEnergyReward(index, callback);
    }



    /// <summary>
    /// 更新本地体力领取记录
    /// </summary>
    /// <param name="dailyEnergyRewards"></param>
    public void UpdateEnergy(RepeatedField<int> dailyEnergyRewards)
    {
        ResetEnergyRecord();
        foreach (var value in dailyEnergyRewards)
        {
            EnergyRecord[value] = 1;
        }
        ActivityController.Instance.RefreshClientRedDot(ActivityClientType.EnergyCenter);
        EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
    }

    public void UpdatePointList(RepeatedField<ActivityPointInfo> pointList)
    {
        foreach (var pinfo in pointList)
        {
            if (OnlineActivityDic.ContainsKey(pinfo.ActivityId))
            {
                if (OnlineActivityDic[pinfo.ActivityId].Index != pinfo.Season) continue;
                ActivityData activityData = OnlineActivityDic[pinfo.ActivityId];
                activityData.payData = new ActivityPayInfoData(pinfo);
                EventManager.Instance.Dispatch(EventID.RefreshWindow, activityData.cfg.Id);
                RefreshRedDot(activityData);
            }
        }
    }

    public void UpdatePayMicroList(RepeatedField<ActivityPayMicroInfo> payMicroList)
    {
        foreach (var pinfo in payMicroList)
        {
            if (OnlineActivityDic.ContainsKey(pinfo.ActivityId))
            {
                ActivityData activityData = OnlineActivityDic[pinfo.ActivityId];
                if (activityData.payData != null)
                {
                    activityData.payData = new();
                }
                activityData.payData.BuyRecordDict.Clear();
                foreach (var item in pinfo.Gifts)
                {
                    activityData.payData.BuyRecordDict.Add(item.GiftId, new() { receiveCount = item.ReceiveCount, payCount = item.PayCount });
                }
                EventManager.Instance.Dispatch(EventID.RefreshWindow, activityData.cfg.Id);
                RefreshRedDot(activityData);
            }
        }
    }

    public void UpdateDailyGiftActivities(RepeatedField<int> dailyGiftActivities)
    {
        dailyGiftReceivedActivityIdSet.Clear();
        foreach (int activityId in dailyGiftActivities)
        {
            if (dailyGiftReceivedActivityIdSet.Contains(activityId) == true)
            {
                Debug.LogWarningFormat("ActivityManager , UnPack , dailyGiftReceivedActivityIdSet.Contains(activityId) == true , item = {0}", activityId);
                continue;
            }
            dailyGiftReceivedActivityIdSet.Add(activityId);
            if (OnlineActivityDic.ContainsKey(activityId))
            {
                ActivityData activityData = OnlineActivityDic[activityId];
                EventManager.Instance.Dispatch(EventID.RefreshWindow, activityData.cfg.Id);
                RefreshRedDot(activityData);
            }
        }
        Player.ShopManager.CheckRedDot();
        EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
        //EventManager.Instance.Dispatch(EventID.OnRefreshNavigationUIRedDot);
    }
    public void RefreshAllRedDot()
    {
        foreach (ActivityData activityData in OnlineActivityDic.Values)
        {
            RefreshRedDot(activityData);
        }
        foreach (var item in Configs.Activity.GetConfigList().Where(item => item.ServerDays == -1))
        {
            ActivityData activityData = new(item);
            RefreshRedDot(activityData);
        }
        Player.ActivityManager.RefreshChallengeRedDot();
    }

    public HashSet<int> dailyGiftReceivedActivityIdSet = new();

    private Dictionary<int, ActivityData> _onlineActivityList = new();

    /// <summary>
    /// 具有时间线的活动的模板信息
    /// </summary>
    public Dictionary<int, ActivityData> OnlineActivityDic
    {
        get
        {
            return _onlineActivityList;
        }
    }

    /// <summary>
    /// 开服时间，用来计算活动是不是开启
    /// </summary>
    public long ServerOpenTime = 1681228800;

    /// <summary>
    /// 领奖后更新单个活动数据，刷新小红点，抛出刷新小红点的事件。
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="receiveList"></param>
    public void UpdateOneActivityDataPoint(int activityId, List<int> receiveList)
    {
        if (OnlineActivityDic.ContainsKey(activityId))
        {
            var activityData = OnlineActivityDic[activityId];
            activityData.payData.AddReceive(receiveList);
            RefreshRedDot(activityData);
            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
            EventManager.Instance.Dispatch(EventID.RefreshWindow, activityId);
        }
    }
    /// <summary>
    /// 获取处于活动周期内的活动配置
    /// </summary>
    public void InitOnlineActivityList(bool needClear = true)
    {
        if (needClear) _onlineActivityList.Clear();
        List<ActivityConfig> list = Configs.Activity.GetConfigList();
        //调试用
        //ServerOpenTime = 1683043200;
        foreach (var cfg in list)
        {
            //if (cfg.Id == 21001)
            //{
            //    Debug.Log("cfg.Id = " + cfg.Id);
            //}
            //ServerDays >0 才是开服活动，否则是触发型的活动。
            var actData = new ActivityData(cfg);
            if (cfg.ServerDays == 0 && cfg.LastTime == 0)
            {
                actData.StartTime = 0;
                actData.EndTime = 0;
                actData.HideTime = 0;
                actData.Index = 1;
                if (_onlineActivityList.ContainsKey(cfg.Id) == false) _onlineActivityList.Add(cfg.Id, actData);
            }
            else
            {
                var loopTime = 0;
                long startTime = 0;
                long openTime = ServerOpenTime + cfg.ServerDays * 86400;
                if (cfg.StartTime == 0)
                {
                    startTime = openTime;
                }
                else if (cfg.StartTime + cfg.ServerDays * 86400 < ServerOpenTime && cfg.ServerFilter > 0)
                {
                    continue;
                }
                else
                {
                    startTime = cfg.StartTime;
                }

                //开始时间戳为0的时候，活动时间为 开服第几天~开服第几天+持续时间
                if (cfg.LoopTime > -1)
                {
                    //落在第几届的区间（把持续时间和间隔时间看做一个整体）
                    actData.Index = (long)System.Math.Ceiling((double)(Utils.DataConvUtil.ServerTime - startTime) / (cfg.LastTime + cfg.LoopTime));
                    loopTime = cfg.LoopTime;
                    if (actData.Index <= 0) actData.Index = 1;
                }
                else
                {
                    actData.Index = 1;
                }

                actData.StartTime = startTime + (cfg.LastTime + loopTime) * (actData.Index - 1);
                actData.EndTime = actData.StartTime + cfg.LastTime;
                actData.HideTime = actData.EndTime + cfg.RewardTime;

                if (Utils.DataConvUtil.ServerTime >= actData.StartTime && Utils.DataConvUtil.ServerTime <= actData.HideTime)
                {
                    if (_onlineActivityList.ContainsKey(cfg.Id) == false) _onlineActivityList.Add(cfg.Id, actData);
                }
            }
        }
    }

    public bool isActivityCloseByType(ActivityClientType eActivityType)
    {
        return false;
    }
    public List<ActivityConfig> GetAllConfigByType(ActivityClientType eActivityType)
    {
        return Configs.Activity.GetConfigList().Where(item => item.ClientType == (int)eActivityType).ToList();
    }

    ///// <summary> 领奖一个接口 </summary>
    public void GetRewards(int activityId, int rewardId, Action callback)
    {
        GetRewards(activityId, new List<int>() { rewardId }, callback);
    }
    ///// <summary> 领奖多个奖励的接口 </summary>
    public void GetRewards(int activityId, List<int> rewardsList, Action callback)
    {
        if (OnlineActivityDic.ContainsKey(activityId) == false)
        {
            return;
        }
        ActivityConfig activityConfig = Configs.Activity.GetConfig(activityId);
        if (activityConfig == null || rewardsList.Count <= 0)
        {
            Debug.LogWarningFormat("ActivityController , GetRewards , activityConfig == null , activityId = {0} , rewardsList.Count = {1}", activityId, rewardsList.Count);
            return;
        }
        NetworkManager.Instance.ReceivePointReward(activityId, rewardsList, (resp) =>
        {
            if (resp.ReceiveSucceed)
            {
                UpdateOneActivityDataPoint(activityId, rewardsList);
                callback?.Invoke();
            }
        });
    }

    private bool isLoading = false;
    /// <summary>
    /// 准备登录要弹的各种窗体
    /// </summary>
    public void PrepareStartWindow()
    {
        if (!LoginManager.Instance.ColdStartWithGiftWindow || Player.ShopManager.SumCost <= 0)
        {
            LoginManager.Instance.ColdStartWithGiftWindow = true;

            if (IsNeedShowFirstCharge())
            {
                UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.FirstPay, new() { ActivityClientType.FirstPay }), false);
            }
            if (!Player.ActivityManager.GetIsSevenSignFinish())
            {
                bool todaySign = Player.ActivityManager.SevenRewardList.Any(item => item.GetState() == (int)RewardStates.COLLECT);
                if (todaySign)
                {
                    //七日签到
                    //UIController.Instance.OpenWindow<SevenDaysLoginUI>(new SevenDaysLoginUIProperties(ActivityID.SevenDaysLogin), false);
                    //TriggerManager.Instance.JumpPanel(TriggerModuleType.Welfare);
                    UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.Sign7Day, new() { ActivityClientType.Sign7Day, ActivityClientType.Sign30Day, ActivityClientType.EnergyCenter }), false);
                }
            }
        }
    }

    public bool IsNeedShowFirstCharge()
    {
        bool isActivityOpen = ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.FirstPay);
        if (!isActivityOpen)
        {
            return false;
        }
        ActivityData activityData = ActivityController.Instance.OnlineActivityDic[ActivityID.FirstPay];
        List<ActivityPayRewardConfig> tmp = Configs.ActivityPayReward.GetConfigList().FindAll(p => p.ActivityId == activityData.cfg.Id);
        if (tmp.Count == activityData.payData.ReceiveSet.Count)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 客户端计算的红点，七日签到这种
    /// </summary>
    /// <param name="eActivityType"></param>
    public void RefreshClientRedDot(ActivityClientType eActivityType)
    {
        List<ActivityConfig> activityConfigList = GetAllConfigByType(eActivityType);
        foreach (ActivityConfig activityConfig in activityConfigList)
        {
            ActivityData activityData = new(activityConfig);
            RefreshRedDot(activityData);
        }
    }
    /// <summary>
    /// 刷新小红点
    /// </summary>
    /// <param name="type"></param>
    /// <param name="cfg"></param>
    public void RefreshRedDot(ActivityData data)
    {
        if (data == null) return;
        if (data.cfg.ClientType == (int)ActivityClientType.FirstPay)
        {
            CheckRedDot_1(data);
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.DailyPay)
        {
            CheckRedDot_2(data);
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.RankAwards)
        {
            CheckRedDot_3(data);
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.BattlePass)
        {
            CheckRedDot_4(data);
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.TimeRecruit)
        {
            CheckRedDot_6(data);
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.AllStarTimeRecruit)
        {
            CheckRedDot_6(data);
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.GiftPay)
        {
            CheckRedDot_7(data);
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.Sign7Day)
        {
            CheckRedDot_8(data.cfg);
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.Sign30Day)
        {
            CheckRedDot_9(data.cfg);
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.NoviceTarget)
        {
            CheckRedDot_10(data.cfg);
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.MonthCard)
        {
            CheckRedDot_11(data.cfg);
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.NewYearMain)
        {
            RefreshNewYearChallengeRedDot();
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.NationalDayLogin)
        {
            CheckRedDot_13(data);
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.ChristmasTree || data.cfg.ClientType == (int)ActivityClientType.ChristmasTask)
        {
            RefreshChristmasBoxRedDot(null);
            RefreshChristmasTaskRedDot();
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.NewYearTask)
        {
            RefreshNewYearTaskRedDot();
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.NewYearGift)
        {
            RefreshNewYearGiftRedDot();
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.NewYearWish)
        {
            RefreshNewYearSignRedDot();
        }
        else if (data.cfg.ClientType == (int)ActivityClientType.EnergyCenter)
        {
            CheckRedDot_18(data.cfg);
        }
        else if (data.clientType == ActivityClientType.SpringFestivalWish)
        {
            CheckRedDot_DragonWish(data);
        }
        else if (data.clientType == ActivityClientType.DragonYearRedEnvelope)
        {
            CheckRedDot_DragonRedEnvelope(data);
        }
        else if (data.clientType == ActivityClientType.SpringFestivalTask)
        {
            CheckRedDot_DragonTask(data);
        }
        else if (data.clientType == ActivityClientType.SpringFestivalGift)
        {
            CheckRedDot_DragonGift(data);
        }
        else if (data.clientType == ActivityClientType.AllStarHome)
        {
            CheckRedDot_AllStarHome(data);
        }
        else if (data.clientType == ActivityClientType.AllStarGift)
        {
            CheckRedDot_AllStarGift(data);
        }
        else if (data.clientType == ActivityClientType.AllStarTask)
        {
            CheckRedDot_AllStarTask(data);
        }
        else if (data.clientType == ActivityClientType.LabourDayHome)
        {
            CheckRedDot_LabourDayHome(data);
        }
        else if (data.clientType == ActivityClientType.LabourDayGift)
        {
            CheckRedDot_LabourDayGift(data);
        }
        else if (data.clientType == ActivityClientType.LabourDayTask)
        {
            CheckRedDot_LabourDayTask(data);
        }
        else if (data.clientType == ActivityClientType.LabourDaySign)
        {
            CheckRedDot_LabourDaySign(data);
        }
        else if (data.clientType == ActivityClientType.PlayoffFinalsGuessHome)
        {
            CheckRedDot_PlayoffFinalsGuessHome(data);
        }
        else if (data.clientType == ActivityClientType.PlayoffFinalsGuessSingle)
        {
            CheckRedDot_PlayoffFinalsGuessSingle(data);
        }
        else if (data.clientType == ActivityClientType.DragonBoatFestivalHome)
        {
            CheckRedDot_DragonBoatFestivalHome(data);
        }
        else if (data.clientType == ActivityClientType.DragonBoatFestivalGift)
        {
            CheckRedDot_DragonBoatFestivalGift(data);
        }
        else if (data.clientType == ActivityClientType.DragonBoatFestivalTask)
        {
            CheckRedDot_DragonBoatFestivalTask(data);
            ActivityData dragonBoatFestivalHomeActivityData = GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalHome);
            CheckRedDot_DragonBoatFestivalHome(dragonBoatFestivalHomeActivityData);
        }
        else if (data.clientType == ActivityClientType.DragonBoatFestivalSign)
        {
            CheckRedDot_DragonBoatFestivalSign(data);
        }
        else if (data.clientType == ActivityClientType.Olympics2024Gift)
        {
            CheckRedDot_Olympics2024Gift(data);
        }
        else if (data.clientType == ActivityClientType.Olympics2024Sign)
        {
            CheckRedDot_Olympics2024Sign(data);
        }
    }

    public bool HasRedDot(int activityId)
    {
        //if (OnlineActivityDic.ContainsKey(activityId) == false)
        //{
        //    return false;
        //}

        ActivityToggleData activityToggleData = new();
        if (OnlineActivityDic.ContainsKey(activityId))
        {
            ActivityData activityData = OnlineActivityDic[activityId];
            activityToggleData.activityData = activityData;
            activityToggleData.activityConfig = activityData.cfg;
        }
        if (activityToggleData.activityData == null)
        {
            activityToggleData.activityConfig = Configs.Activity.GetConfig(activityId);
        }


        if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.FirstPay)
        {
            return CheckRedDot_1(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.DailyPay)
        {
            return CheckRedDot_2(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.RankAwards)
        {
            return CheckRedDot_3(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.BattlePass)
        {
            return CheckRedDot_4(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.TimeRecruit)
        {
            return CheckRedDot_6(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.AllStarTimeRecruit)
        {
            return CheckRedDot_6(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.GiftPay)
        {
            return CheckRedDot_7(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.Sign7Day)
        {
            return CheckRedDot_8(activityToggleData.activityConfig);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.Sign30Day)
        {
            return CheckRedDot_9(activityToggleData.activityConfig);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.NoviceTarget)
        {
            return CheckRedDot_10(activityToggleData.activityConfig);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.MonthCard)
        {
            return CheckRedDot_11(activityToggleData.activityConfig);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.NationalDayLogin)
        {
            return CheckRedDot_13(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.EnergyCenter)
        {
            return CheckRedDot_18(activityToggleData.activityConfig);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.SpringFestivalWish)
        {
            return CheckRedDot_DragonWish(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.DragonYearRedEnvelope)
        {
            return CheckRedDot_DragonRedEnvelope(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.SpringFestivalTask)
        {
            return CheckRedDot_DragonTask(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.SpringFestivalGift)
        {
            return CheckRedDot_DragonGift(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.AllStarHome)
        {
            return CheckRedDot_AllStarHome(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.AllStarGift)
        {
            return CheckRedDot_AllStarGift(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.AllStarTask)
        {
            return CheckRedDot_AllStarTask(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.LabourDayHome)
        {
            return CheckRedDot_LabourDayHome(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.LabourDayGift)
        {
            return CheckRedDot_LabourDayGift(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.LabourDayTask)
        {
            return CheckRedDot_LabourDayTask(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.LabourDaySign)
        {
            return CheckRedDot_LabourDaySign(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.PlayoffFinalsGuessHome)
        {
            return CheckRedDot_PlayoffFinalsGuessHome(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.PlayoffFinalsGuessSingle)
        {
            return CheckRedDot_PlayoffFinalsGuessSingle(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.DragonBoatFestivalHome)
        {
            return CheckRedDot_DragonBoatFestivalHome(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.DragonBoatFestivalGift)
        {
            return CheckRedDot_DragonBoatFestivalGift(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.DragonBoatFestivalTask)
        {
            return CheckRedDot_DragonBoatFestivalTask(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.DragonBoatFestivalSign)
        {
            return CheckRedDot_DragonBoatFestivalSign(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.Olympics2024Gift)
        {
            return CheckRedDot_Olympics2024Gift(activityToggleData.activityData);
        }
        else if (activityToggleData.activityConfig.ClientType == (int)ActivityClientType.Olympics2024Sign)
        {
            return CheckRedDot_Olympics2024Sign(activityToggleData.activityData);
        }
        return false;
    }

    #region 小红点    

    /// <summary>
    /// 首充类型活动的小红点
    /// </summary>
    /// <param name="cfg"></param>
    public bool CheckRedDot_1(ActivityData _data)
    {
        var RewardsConfigList = Configs.ActivityPayReward.GetConfigList().FindAll(p => p.ActivityId == _data.cfg.Id);

        var anyRed = false;
        for (var index = 0; index < RewardsConfigList.Count; index++)
        {
            var isRed = RewardsConfigList[index].Option <= _data.payData.TotalPay && !_data.payData.HasReceive(RewardsConfigList[index].Id);
            anyRed |= isRed;
        }

        if (!string.IsNullOrWhiteSpace(_data.cfg.DailyGift)) anyRed |= !dailyGiftReceivedActivityIdSet.Contains(_data.cfg.Id);

        if (_data.cfg.StartTime == 0)
        {
            RedDotNode node1 = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + RewardsConfigList[0].ActivityId);
            node1.AddValue(anyRed ? 1 : -1);
        }
        else
        {
            RedDotNode node2 = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FestivalTotalPay, "/" + RewardsConfigList[0].ActivityId);
            node2.AddValue(anyRed ? 1 : -1);
        }

        return anyRed;
    }

    /// <summary>
    /// 每日充值活动的小红点
    /// </summary>
    /// <param name="cfg"></param>
    public bool CheckRedDot_2(ActivityData _data)
    {

        var RewardsConfigList = Configs.ActivityPayDailyReward.GetConfigList().FindAll(p => p.ActivityId == _data.cfg.Id);

        //达到充值条件的天数，大于领取奖励的次数
        var isRed = _data.payData.Days > _data.payData.ReceiveSet.Count && _data.payData.ReceiveSet.Count <= 5;

        //有每日奖励可领
        if (!string.IsNullOrWhiteSpace(_data.cfg.DailyGift)) isRed |= !dailyGiftReceivedActivityIdSet.Contains(_data.cfg.Id);

        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + RewardsConfigList[0].ActivityId);
        node.AddValue(isRed ? 1 : -1);
        return isRed;
    }

    /// <summary>
    /// 排行活动小红点
    /// </summary>
    /// <param name="cfg"></param>
    public bool CheckRedDot_3(ActivityData _data)
    {

        return false;
    }

    /// <summary>
    /// 战令小红点
    /// </summary>
    /// <param name="cfg"></param>
    public bool CheckRedDot_4(ActivityData _data)
    {
        bool isRed = false;
        var RewardsConfigList = Configs.ActivityBattlePassReward.GetConfigList().FindAll(p => p.ActivityId == _data.cfg.Id);
        foreach (var config in RewardsConfigList)
        {
            ActivityData activityData = _data;
            int taskPoint = activityData.payData.TaskPoint;
            bool isLockByPurchase = !activityData.payData.hasBuy;
            bool isLockByLevel = taskPoint < config.Option;
            bool freeHasRecieve = activityData.payData.HasReceive(config.Id * 10 + 1);
            bool freeHasGoods = string.IsNullOrEmpty(config.Rewards1) == false;
            bool freeCanGet = !isLockByLevel && !freeHasRecieve && freeHasGoods;
            bool payHasRecieve = activityData.payData.HasReceive(config.Id * 10 + 2);
            bool payHasGoods = string.IsNullOrEmpty(config.Rewards2) == false || string.IsNullOrEmpty(config.RewardsStep) == false;
            bool payCanGet = !isLockByLevel && !payHasRecieve && !isLockByPurchase && payHasGoods;
            if (freeCanGet || payCanGet)
            {
                isRed = true;
                break;
            }
            if (isLockByLevel) break;
        }

        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);
        node.AddValue(isRed ? 1 : -1);

        return isRed;
    }

    /// <summary>
    /// 教练训话小红点
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private bool CheckRedDot_18(ActivityConfig activityConfig)
    {
        bool isRed = Player.ActivityManager.CheckFunRed(1802);
        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + activityConfig.Id);
        node.AddValue(isRed ? 1 : -1);
        return isRed;
    }

    /// <summary>
    /// 节日签到
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private bool CheckRedDot_13(ActivityData _data)
    {
        var RewardsConfigList = Configs.FestivalLogin.GetConfigList().FindAll(p => p.ActivityId == _data.cfg.Id);

        var anyRed = false;
        for (var index = 0; index < RewardsConfigList.Count; index++)
        {
            var isRed = RewardsConfigList[index].Option <= _data.payData.TotalPay && !_data.payData.HasReceive(RewardsConfigList[index].Id);
            anyRed |= isRed;
        }

        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FestivalLogin, "");
        node.AddValue(anyRed ? 1 : -1);
        return anyRed;
    }

    /// <summary>
    /// 月卡小红点
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private bool CheckRedDot_11(ActivityConfig activityConfig)
    {
        bool isRed = Player.ActivityManager.GetIsMonthCardRedDot();
        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + activityConfig.Id);
        node.AddValue(isRed ? 1 : -1);
        return isRed;
    }
    /// <summary>
    /// 新手目标小红点
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private bool CheckRedDot_10(ActivityConfig activityConfig)
    {
        bool isRed = Player.ActivityManager.GetIsNoviceRedDot();
        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + activityConfig.Id);
        node.AddValue(isRed ? 1 : -1);
        return isRed;
    }
    /// <summary>
    /// 30日签到小红点
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private bool CheckRedDot_9(ActivityConfig activityConfig)
    {
        bool isRed = Player.ActivityManager.GetIsMonthSignRedDot();
        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + activityConfig.Id);
        node.AddValue(isRed ? 1 : -1);
        return isRed;
    }
    /// <summary>
    /// 七日签到小红点
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private bool CheckRedDot_8(ActivityConfig activityConfig)
    {
        bool isRed = Player.ActivityManager.GetIsSevenSignRedDot();
        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + activityConfig.Id);
        node.AddValue(isRed ? 1 : -1);
        return isRed;
    }
    /// <summary>
    /// 小额礼包
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private bool CheckRedDot_7(ActivityData data)
    {
        var isRed = false;
        foreach (var buyRecord in data.payData.BuyRecordDict.Values)
        {
            if (buyRecord.receiveCount > 0)
            {
                isRed = true;
                break;
            }
        }

        //有每日奖励可领
        if (!string.IsNullOrWhiteSpace(data.cfg.DailyGift)) isRed |= !dailyGiftReceivedActivityIdSet.Contains(data.cfg.Id);

        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + data.cfg.Id);
        node.AddValue(isRed ? 1 : -1);
        return isRed;

    }
    /// <summary>
    /// 限时抽卡
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private bool CheckRedDot_6(ActivityData data)
    {
        //param1是卡池ID
        return Player.CardManager.RecruitController.CheckRedData(data.cfg.Param1);
    }
    #endregion
    ///// <summary>
    ///// 获取首页展示的活动。
    ///// </summary>
    ///// <returns></returns>
    //public List<ActivityConfig> GetHomeUIActivity()
    //{
    //    //具有首页显示标签的，并且当前处于活动中的。
    //    var list = Configs.Activity.GetConfigList();
    //    List<ActivityConfig> homeList = new();
    //    #region 处理所有在线活动，奖励领完了都不展示了
    //    foreach (var _data in _onlineActivityList.Values)
    //    {
    //        if (_data.cfg.ShowInHome == 1)
    //        {
    //            //特殊处理充值活动
    //            if (_data.cfg.ClientType == (int)EActivityType.FirstPay)
    //            {
    //                List<ActivityPayRewardConfig> tmp = Configs.ActivityPayReward.GetConfigList().FindAll(p => p.ActivityId == _data.cfg.Id);
    //                if (tmp.Count == _data.payData.ReceiveSet.Count)
    //                {
    //                    //奖励领完了，不需要首页再展示
    //                    continue;
    //                }
    //            }
    //            else if (_data.cfg.ClientType == (int)EActivityType.DailyPay)
    //            {
    //                List<ActivityPayDailyRewardConfig> tmp = Configs.ActivityPayDailyReward.GetConfigList().FindAll(p => p.ActivityId == _data.cfg.Id);
    //                if (tmp.Count == _data.payData.ReceiveSet.Count)
    //                {
    //                    //奖励领完了，不需要首页再展示
    //                    continue;
    //                }
    //            }
    //            else if (_data.cfg.ClientType == (int)EActivityType.BattlePass)
    //            {
    //                List<ActivityBattlePassRewardConfig> tmp = Configs.ActivityBattlePassReward.GetConfigList().FindAll(p => p.ActivityId == _data.cfg.Id);
    //                if (tmp.Count == _data.payData.ReceiveSet.Count)
    //                {
    //                    //奖励领完了，不需要首页再展示
    //                    continue;
    //                }
    //            }
    //            homeList.Add(_data.cfg);
    //        }
    //    }
    //    #endregion

    //    foreach (var cfg in list)
    //    {
    //        if (cfg.ShowInHome == 1 && cfg.ServerDays == -1)
    //        {
    //            if (cfg.ClientType == (int)EActivityType.Sign7Day)
    //            {
    //                //7日签到领完了
    //                if (Player.ActivityManager.GetIsSevenSignFinish()) continue;
    //            }
    //            else if (cfg.ClientType == (int)EActivityType.NoviceTarget)
    //            {
    //                //个人目标完成了
    //                if (Player.NoviceTaskManager.IsNoviceTaskFinish()) continue;
    //            }
    //            homeList.Add(cfg);
    //        }
    //    }

    //    return homeList;
    //}

    /// <summary>
    /// 购买战令
    /// </summary>
    public void PurchaseSeasonPass(ActivityData _data)
    {
        int puechaseConfigId = _data.cfg.Param1;
        GiftShopConfig giftShopConfig = Configs.GiftShop.GetConfigList().FirstOrDefault(cfg => cfg.Type == _data.cfg.Id);
        if (giftShopConfig == null)
        {
            Debug.LogWarningFormat("ActivityController , PurchaseSeasonPass , giftShopConfig == null , puechaseConfigId = {0} , _data.cfg.Id = {1}", puechaseConfigId, _data.cfg.Id);
            return;
        }
        PurchaseInfo info = DataConvUtil.NewPurchase(giftShopConfig.ProductId, giftShopConfig.Name, giftShopConfig.Rmb, giftShopConfig.Id);
#if USER_DEBUG && UNITY_EDITOR
        PurchaseUtil.TestBuyInEditor(info.ShopItemId);
#else
        PurchaseServiceManager.Instance.Purchase(info);
#endif
    }

    public void GetRankInfo(ActivityData _data)
    {
        NetworkManager.Instance.GetActivityRankList(_data.cfg.Id, _data.cfg.Param1, (resp) =>
        {
            _data.RankData.Clear();
            foreach (ActivityRankInfo activityRankInfo in resp.Ranks)
            {
                RankAwardItemData rankAwardItemData = new();
                rankAwardItemData.activityData = _data;
                rankAwardItemData.activityRankInfo = activityRankInfo;
                _data.RankData.Add(rankAwardItemData);
            }

            //虚位以待
            int maxShowCount = Configs.ActivityTopReward.GetConfigList().Where(cfg => cfg.ActivityId == _data.cfg.Id).OrderBy(cfg => cfg.Max).ToList()[^2].Max;
            for (int i = _data.RankData.Count; i < maxShowCount; i++)
            {
                RankAwardItemData rankAwardItemData = new();
                rankAwardItemData.activityData = _data;
                rankAwardItemData.virtualRank = i + 1;
                _data.RankData.Add(rankAwardItemData);
            }
        });
    }
    public bool IsTypeOpen(ActivityClientType eActivityType)
    {
        var festivalCfg = ConfigManager.Instance.GetTable<ActivityConfigTable>().GetConfigList().FindAll(p => p.ClientType == (int)eActivityType);
        if (festivalCfg == null || festivalCfg.Count <= 0) return false;
        foreach (var item in festivalCfg)
        {
            if (OnlineActivityDic.ContainsKey(item.Id)) return true;
        }
        return false;
    }
    public bool IsActivityOpen(ActivityConfig activityConfig)
    {
        if (activityConfig == null)
        {
            Debug.Log("ActivityController , IsActivityOpen , activityConfig == null");
            return false;
        }
        if (IsNeedHideInToggle(activityConfig)) return false;
        if (OnlineActivityDic.ContainsKey(activityConfig.Id) && OnlineActivityDic[activityConfig.Id].IsHide == false) return true;
        if (activityConfig == null || activityConfig.ServerDays != -1) return false;
        return true;
    }
    public bool IsActivityOpen(int activityId)
    {
        ActivityConfig activityConfig = Configs.Activity.GetConfig(activityId);
        if (activityConfig == null)
        {
            Debug.Log("ActivityController , IsActivityOpen , activityConfig == null , activityId = " + activityId);
            return false;
        }
        return IsActivityOpen(activityConfig);
    }
    public ActivityData GetOneActivityDataByType(ActivityClientType eActivityType)
    {
        var festivalCfg = ConfigManager.Instance.GetTable<ActivityConfigTable>().GetConfigList().FindAll(p => p.ClientType == (int)eActivityType);
        if (festivalCfg == null || festivalCfg.Count <= 0) return null;
        foreach (var item in festivalCfg)
        {
            if (OnlineActivityDic.ContainsKey(item.Id)) return OnlineActivityDic[item.Id];
        }
        return null;
    }
    public List<ActivityData> GetAllActivityDataByType(ActivityClientType eActivityType)
    {
        List<ActivityData> activityDataList = new();
        var festivalCfg = ConfigManager.Instance.GetTable<ActivityConfigTable>().GetConfigList().FindAll(p => p.ClientType == (int)eActivityType);
        if (festivalCfg == null || festivalCfg.Count <= 0) return activityDataList;
        foreach (var item in festivalCfg)
        {
            if (OnlineActivityDic.ContainsKey(item.Id)) activityDataList.Add(OnlineActivityDic[item.Id]);
        }
        return activityDataList;
    }

    private Dictionary<ActivityClientType, List<ActivityConfig>> activityClientTypeDictionary = new();
    public List<ActivityConfig> GetConfigListByType(ActivityClientType eActivityType)
    {
        if (activityClientTypeDictionary.Count == 0)
        {
            foreach (ActivityConfig activityConfig in Configs.Activity.GetConfigList())
            {
                ActivityClientType type = (ActivityClientType)activityConfig.ClientType;
                if (activityClientTypeDictionary.ContainsKey(type) == false)
                {
                    activityClientTypeDictionary.Add(type, new());
                }
                activityClientTypeDictionary[type].Add(activityConfig);
            }
        }
        if (activityClientTypeDictionary.ContainsKey(eActivityType) == false) return new();
        return activityClientTypeDictionary[eActivityType];
    }

    public ActivityData FindTimeRecruitActivity
    {
        get
        {
            return GetOneActivityDataByType(ActivityClientType.TimeRecruit);
        }
    }
    public ActivityData FindAllStar2024NorthTimeRecruit
    {
        get
        {
            return GetOneActivityDataByTypeAndParam3(ActivityClientType.AllStarTimeRecruit, "north");
        }
    }
    public ActivityData FindAllStar2024SouthTimeRecruit
    {
        get
        {
            return GetOneActivityDataByTypeAndParam3(ActivityClientType.AllStarTimeRecruit, "south");
        }
    }
    public ActivityData GetOneActivityDataByTypeAndParam3(ActivityClientType eActivityType, string param3)
    {
        var festivalCfg = ConfigManager.Instance.GetTable<ActivityConfigTable>().GetConfigList().FindAll(p => p.ClientType == (int)eActivityType && p.Param3 == param3);
        if (festivalCfg == null || festivalCfg.Count <= 0) return null;
        foreach (var item in festivalCfg)
        {
            if (OnlineActivityDic.ContainsKey(item.Id)) return OnlineActivityDic[item.Id];
        }
        return null;
    }

    /// <summary>
    /// 任务改变触发签到红点变化
    /// </summary>
    public void RefreshTaskAboutActivityRedDot()
    {
        RefreshNewYearSignRedDot();

        List<ActivityClientType> activityClientTypeList = new() { ActivityClientType.SpringFestivalWish, ActivityClientType.LabourDaySign };
        foreach (var item in activityClientTypeList)
        {
            RefreshActivityRedDotByClientType(item);
        }
    }
    /// <summary>
    /// 刷新某一类型的活动
    /// </summary>
    /// <param name="clientType"></param>
    public void RefreshActivityRedDotByClientType(ActivityClientType clientType)
    {
        List<ActivityData> activityDataList = GetAllActivityDataByType(clientType);
        foreach (var data in activityDataList)
        {
            RefreshRedDot(data);
        }
    }
    /// <summary>
    /// 活动推送收到后
    /// </summary>
    public void CheckGetMoreInformation()
    {
        if (IsTypeOpen(ActivityClientType.DragonYearRedEnvelope))
        {
            RedEnvlopeManager.Instance.GetNewData();
            RedEnvlopeManager.Instance.GetNoticeDate();
        }
    }

    #region 圣诞树

    private Dictionary<int, FestivalTaskInfo> festivalTaskInfoListDic = new();
    public void RebuildFeativalTaskData(RepeatedField<FestivalTaskInfo> festivalTaskInfos)
    {
        festivalTaskInfoListDic.Clear();
        foreach (var item in festivalTaskInfos)
        {
            if (festivalTaskInfoListDic.ContainsKey(item.Id))
            {
                Debug.LogWarning("ActivityController , RebuildFeativalTaskData , festivalTaskInfoListDic.ContainsKey(item.Id) , item.Id = {0}".SafeFormat(item.Id));
                continue;
            }
            festivalTaskInfoListDic.Add(item.Id, item);
        }
        RefreshChristmasTaskRedDot();
        RefreshNewYearTaskRedDot();
        if (IsTypeOpen(ActivityClientType.SpringFestivalTask)) CheckRedDot_DragonTask(GetOneActivityDataByType(ActivityClientType.SpringFestivalTask));
        if (IsTypeOpen(ActivityClientType.DragonBoatFestivalTask)) CheckRedDot_DragonBoatFestivalTask(GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalTask));
        if (IsTypeOpen(ActivityClientType.DragonBoatFestivalHome)) CheckRedDot_DragonBoatFestivalHome(GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalHome));
        EventManager.Instance.Dispatch(EventID.OnFestivalTaskDataChange);
        EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
    }
    public void RefreshFeativalTaskData(RepeatedField<FestivalTaskInfo> festivalTaskInfos)
    {
        foreach (var item in festivalTaskInfos)
        {
            if (festivalTaskInfoListDic.ContainsKey(item.Id) == false)
            {
                Debug.LogWarning("ActivityController , RefreshFeativalTaskData , festivalTaskInfoListDic.ContainsKey(item.Id) == false , item.Id = {0}".SafeFormat(item.Id));
                continue;
            }
            festivalTaskInfoListDic[item.Id] = item;
        }
        RefreshChristmasTaskRedDot();
        RefreshNewYearTaskRedDot();
        if (IsTypeOpen(ActivityClientType.SpringFestivalTask)) CheckRedDot_DragonTask(GetOneActivityDataByType(ActivityClientType.SpringFestivalTask));
        if (IsTypeOpen(ActivityClientType.DragonBoatFestivalTask)) CheckRedDot_DragonBoatFestivalTask(GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalTask));
        if (IsTypeOpen(ActivityClientType.DragonBoatFestivalHome)) CheckRedDot_DragonBoatFestivalHome(GetOneActivityDataByType(ActivityClientType.DragonBoatFestivalHome));
        EventManager.Instance.Dispatch(EventID.OnFestivalTaskDataChange);
        EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
    }
    public List<FestivalTaskInfo> GetFestivalTaskInfoList(int taskActivityId)
    {
        List<FestivalTaskInfo> festivalTaskInfoListByActivityId = new();
        foreach (var item in festivalTaskInfoListDic.Values)
        {
            FestivalTaskConfig festivalTaskConfig = Configs.FestivalTask.GetConfig(item.Id);
            if (festivalTaskConfig == null)
            {
                Debug.LogWarning("ActivityController , GetFestivalTaskInfoList , festivalTaskConfig == null , item.Id = {0}".SafeFormat(item.Id));
                continue;
            }
            if (festivalTaskConfig.ActivityId == taskActivityId)
            {
                festivalTaskInfoListByActivityId.Add(item);
            }
        }
        return festivalTaskInfoListByActivityId;
    }

    public void RefreshChristmasTaskRedDot()
    {
        bool isRed = false;
        RedDotNode TaskRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Christmas, "/" + ActivityID.ChristmasTask);

        //有每日奖励可领
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.ChristmasTask) == true)
        {
            ActivityData _data = ActivityController.Instance.OnlineActivityDic[ActivityID.ChristmasTask];
            if (!string.IsNullOrWhiteSpace(_data.cfg.DailyGift)) isRed |= !dailyGiftReceivedActivityIdSet.Contains(_data.cfg.Id);
        }

        //有任务奖励可领
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.ChristmasTask))
        {
            List<FestivalTaskInfo> festivalTaskInfoList = GetFestivalTaskInfoList(ActivityID.ChristmasTask);
            bool isFindCanGetTask = festivalTaskInfoList.FirstOrDefault((FestivalTaskInfo festivalTaskInfo) =>
            {
                FestivalTaskConfig festivalTaskConfig = Configs.FestivalTask.GetConfig(festivalTaskInfo.Id);
                if (festivalTaskConfig == null)
                {
                    Debug.Log("ActivityController , RefreshChristmasTaskRedDot , festivalTaskConfig == null , festivalTaskInfo.Id = " + festivalTaskInfo.Id);
                    return false;
                }
                return festivalTaskInfo.Obtain == false && festivalTaskInfo.Current >= festivalTaskConfig.Target;
            }) != null;
            isRed |= isFindCanGetTask;
        }

        TaskRedDotNode.AddValue(isRed ? 1 : -1);

        Debug.Log("RefreshChristmasTaskRedDot , isRed = " + isRed);
    }
    public void RefreshChristmasBoxRedDot(object[] _)
    {
        bool isRed = false;
        RedDotNode BoxRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Christmas, "/" + ActivityID.ChristmasTree);

        //有兑换道具还没用
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.ChristmasTree))
        {
            FestivalBoxConfig festivalBoxConfig = Configs.FestivalBox.GetConfig(ActivityID.ChristmasTree);
            Utils.GameItem.GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, festivalBoxConfig.KeyId, 0);
            bool isFindCanGetReward = gameItem.GetPlayerCount() > 0;
            isRed |= isFindCanGetReward;
        }

        BoxRedDotNode.AddValue(isRed ? 1 : -1);

        Debug.Log("RefreshChristmasBoxRedDot , isRed = " + isRed);
    }

    #endregion

    #region 元旦

    public void RefreshNewYearChallengeRedDot()//元旦挑战红点
    {
        bool isRed = false;
        RedDotNode BoxRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_NewYear, "/Challenge");

        //投篮小游戏一次没玩
        if (TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Games, false) && ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.NewYearChallenge))
        {
            bool challengeNotPlay = Player.ActivityManager.ShootGameTimesLeft >= GameConst.ChallengeTimes;
            isRed |= challengeNotPlay;
        }

        BoxRedDotNode.AddValue(isRed ? 1 : -1);

        Debug.Log("RefreshNewYearChallengeRedDot , isRed = " + isRed);
    }
    public void RefreshNewYearTaskRedDot()//元旦任务红点
    {
        bool isRed = false;
        RedDotNode BoxRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_NewYear, "/Task");

        //有已完成但未领取的任务
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.NewYearTask))
        {
            List<FestivalTaskInfo> festivalTaskInfoList = GetFestivalTaskInfoList(ActivityID.NewYearTask);
            bool isFindCanGetTask = festivalTaskInfoList.FirstOrDefault((FestivalTaskInfo festivalTaskInfo) =>
            {
                FestivalTaskConfig festivalTaskConfig = Configs.FestivalTask.GetConfig(festivalTaskInfo.Id);
                if (festivalTaskConfig == null)
                {
                    Debug.Log("ActivityController , RefreshNewYearTaskRedDot , festivalTaskConfig == null , festivalTaskInfo.Id = " + festivalTaskInfo.Id);
                    return false;
                }
                return festivalTaskInfo.Obtain == false && festivalTaskInfo.Current >= festivalTaskConfig.Target;
            }) != null;
            isRed |= isFindCanGetTask;
        }

        BoxRedDotNode.AddValue(isRed ? 1 : -1);

        Debug.Log("RefreshNewYearTaskRedDot , isRed = " + isRed);
    }
    public void RefreshNewYearGiftRedDot()//元旦礼包红点
    {
        bool isRed = false;
        RedDotNode BoxRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_NewYear, "/Gift");

        //有每日免费礼物没领取
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.NewYearGift) == true)
        {
            ActivityData _data = ActivityController.Instance.OnlineActivityDic[ActivityID.NewYearGift];
            if (!string.IsNullOrWhiteSpace(_data.cfg.DailyGift)) isRed |= !dailyGiftReceivedActivityIdSet.Contains(_data.cfg.Id);
        }

        BoxRedDotNode.AddValue(isRed ? 1 : -1);

        Debug.Log("RefreshNewYearGiftRedDot , isRed = " + isRed);
    }

    #endregion

    #region 跨年签到（许愿）

    public int todayWishTimes = 0;
    public List<int> wishSignRewards = new();
    public List<int> wishSigns = new();

    public void RefreshNewYearSignRedDot()//跨年签到（许愿）红点
    {
        bool isRed = false;
        RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_NewYearSign, "");

        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.NewYearSign) == true)
        {
            ActivityData _data = ActivityController.Instance.OnlineActivityDic[ActivityID.NewYearSign];

            DateTime serverDT = Utils.DataConvUtil.ServerDateTime;
            DateTime startDT = DateTimeOffset.FromUnixTimeSeconds(_data.StartTime).DateTime.ToLocalTime();
            TimeSpan timeSpan = serverDT - startDT;
            int openDayCount = timeSpan.Days;
            int openItemCount = Utility.KeepInRange(openDayCount + 1, 1, 5);

            bool isHasSignNotSet = openItemCount > ActivityController.Instance.wishSigns.Count;//许愿签未设置
            isRed |= isHasSignNotSet;

            bool isHasSignNotGet = false;//许愿签可领取
            for (int i = 0; i < 5; i++)
            {
                int itemIndex = i + 1;
                if (itemIndex > openItemCount) continue;//没开
                if (itemIndex > ActivityController.Instance.wishSigns.Count) continue;//没设置东西
                if (itemIndex <= ActivityController.Instance.wishSignRewards.Count) continue;//领取过了
                if (Player.TaskManager.DailyTasks.Point - (ActivityController.Instance.todayWishTimes * 100) < 100) continue;//活越点数不足
                isHasSignNotGet = true;
                break;
            }
            isRed |= isHasSignNotGet;
        }

        redDotNode.AddValue(isRed ? 1 : -1);
        Debug.Log("RefreshNewYearSignRedDot , isRed = " + isRed);
    }


    #endregion

    #region 龙年红包

    /// <summary>龙年新年红包</summary>
    private bool CheckRedDot_DragonRedEnvelope(ActivityData _data)
    {
        bool isRed = false;
        RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true && RedEnvlopeManager.Instance.serverData != null)
        {
            //在领取时间内，红包池还有红包
            DateTime openTime = TimeUtils.ToDateTime(RedEnvlopeManager.Instance.serverData.OpenTime);
            DateTime closeTime = TimeUtils.ToDateTime(RedEnvlopeManager.Instance.serverData.CloseTime);
            bool isGeting = openTime < DataConvUtil.ServerDateTime && DataConvUtil.ServerDateTime < closeTime;
            bool isCanGet = isGeting && RedEnvlopeManager.Instance.serverData.TotalPacketCount > 0;
            isRed |= isCanGet;

            //在下一波领取时间内
            DateTime nextOpenTime = TimeUtils.ToDateTime(RedEnvlopeManager.Instance.serverData.NextOpenTime);
            if (!isGeting && nextOpenTime <= DataConvUtil.ServerDateTime)
            {
                isRed |= true;
            }
            else
            {
                //不在发放时间内，有待发送的红包道具
                GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, _data.cfg.Param1, 0);
                bool isCanSend = !isGeting && gameItem.GetPlayerCount() > 0;
                isRed |= isCanSend;
            }
        }

        redDotNode.AddValue(isRed ? 1 : -1);
        Debug.Log("CheckRedDot_DragonRedEnvelope , isRed = " + isRed);

        return isRed;
    }

    /// <summary>龙年新年祈愿签到/summary>
    private bool CheckRedDot_DragonWish(ActivityData _data)
    {
        bool isRed = false;
        RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true)
        {
            int rewardCount = 5;
            WishSignConfig wishSignConfig = Configs.WishSign.GetConfig(_data.cfg.Id);
            if (wishSignConfig == null)
            {
                Debug.LogError("CheckRedDot_DragonWish , wishSignConfig == null , _data.cfg.Id = {0}".SafeFormat(_data.cfg.Id));
            }
            else
            {
                rewardCount = wishSignConfig.RewardCount;
            }

            DateTime serverDT = Utils.DataConvUtil.ServerDateTime;
            DateTime startDT = DateTimeOffset.FromUnixTimeSeconds(_data.StartTime).DateTime.ToLocalTime();
            TimeSpan timeSpan = serverDT - startDT;
            int openDayCount = timeSpan.Days;
            int openItemCount = Utility.KeepInRange(openDayCount + 1, 1, rewardCount);

            bool isHasSignNotSet = openItemCount > ActivityController.Instance.wishSigns.Count;//许愿签未设置
            isRed |= isHasSignNotSet;

            bool isHasSignNotGet = false;//许愿签可领取
            for (int i = 0; i < rewardCount; i++)
            {
                int itemIndex = i + 1;
                if (itemIndex > openItemCount) continue;//没开
                if (itemIndex > ActivityController.Instance.wishSigns.Count) continue;//没设置东西
                if (itemIndex <= ActivityController.Instance.wishSignRewards.Count) continue;//领取过了
                if (Player.TaskManager.DailyTasks.Point - (ActivityController.Instance.todayWishTimes * 100) < 100) continue;//活越点数不足
                isHasSignNotGet = true;
                break;
            }
            isRed |= isHasSignNotGet;
        }

        redDotNode.AddValue(isRed ? 1 : -1);
        Debug.Log("CheckRedDot_DragonWish , isRed = " + isRed);

        return isRed;
    }

    /// <summary>龙年任务红点/summary>
    public bool CheckRedDot_DragonTask(ActivityData _data)
    {
        bool isRed = false;
        RedDotNode BoxRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        //有已完成但未领取的任务
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id))
        {
            List<FestivalTaskInfo> festivalTaskInfoList = GetFestivalTaskInfoList(_data.cfg.Id);
            bool isFindCanGetTask = festivalTaskInfoList.FirstOrDefault((FestivalTaskInfo festivalTaskInfo) =>
            {
                FestivalTaskConfig festivalTaskConfig = Configs.FestivalTask.GetConfig(festivalTaskInfo.Id);
                if (festivalTaskConfig == null)
                {
                    Debug.Log("ActivityController , CheckRedDot_DragonTask , festivalTaskConfig == null , festivalTaskInfo.Id = " + festivalTaskInfo.Id);
                    return false;
                }
                return festivalTaskInfo.Obtain == false && festivalTaskInfo.Current >= festivalTaskConfig.Target;
            }) != null;
            isRed |= isFindCanGetTask;
        }

        BoxRedDotNode.AddValue(isRed ? 1 : -1);

        Debug.Log("CheckRedDot_DragonTask , isRed = " + isRed);
        return isRed;
    }
    /// <summary>龙年礼包红点/summary>
    public bool CheckRedDot_DragonGift(ActivityData _data)
    {
        bool isRed = false;
        RedDotNode BoxRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        //有每日免费礼物没领取
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true)
        {
            if (!string.IsNullOrWhiteSpace(_data.cfg.DailyGift)) isRed |= !dailyGiftReceivedActivityIdSet.Contains(_data.cfg.Id);
        }

        BoxRedDotNode.AddValue(isRed ? 1 : -1);

        Debug.Log("CheckRedDot_DragonGift , isRed = " + isRed);
        return isRed;
    }

    #endregion

    #region 全明星2024

    /// <summary>
    /// 全明星2024主页（南北战力比拼）
    /// 1 若未选择过阵营，则“活动入口”红点提示。
    /// 2 若有新的“每日奖励”可领取，则“活动入口”和“领奖按钮”红点提示。
    /// 3 若有新的“个人战力奖励”可领取，则“活动入口”和“个人战力入口”红点提示。
    /// 4 若比拼阶段结束、完成结算，则“活动入口”红点提示。
    /// 5 若球员战力需要更新，则“活动入口”和“更新战力按钮”红点提示。
    /// </summay>
    public bool CheckRedDot_AllStarHome(ActivityData _data)
    {
        bool isRed = false;
        if (_data == null) return false;
        RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        if (_data != null && ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true)
        {
            bool isSign = AllStarManager.Instance.savedTotalNowCombatInServer > 0;//1 若未选择过阵营，则“活动入口”红点提示。
            bool canGetDaily = !dailyGiftReceivedActivityIdSet.Contains(_data.cfg.Id);//2 若有新的“每日奖励”可领取，则“活动入口”和“领奖按钮”红点提示。
            bool canGetCombatReward = AllStarManager.Instance.IsCombatRewardCanGet(); //3 若有新的“个人战力奖励”可领取，则“活动入口”和“个人战力入口”红点提示。
            bool isNeedShowEnd = AllStarManager.Instance.IsNeedShowEnd;// 4 若比拼阶段结束、完成结算，则“活动入口”红点提示。
            int newCombat = AllStarManager.Instance.GetNewTotalCombat();
            bool needRefresh = AllStarManager.Instance.savedTotalNowCombatInServer < newCombat;// 5 若球员战力需要更新，则“活动入口”和“更新战力按钮”红点提示。

            isRed = !isSign || canGetDaily || canGetCombatReward || isNeedShowEnd || needRefresh;
        }

        redDotNode.AddValue(isRed ? 1 : -1);
        Debug.Log("CheckRedDot_AllStarHome , isRed = " + isRed);

        return isRed;
    }

    /// <summary>全明星2024任务红点/summary>
    public bool CheckRedDot_AllStarTask(ActivityData _data)
    {
        if (_data == null) return false;
        bool isRed = false;
        RedDotNode BoxRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        //有已完成但未领取的任务
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id))
        {
            List<FestivalTaskInfo> festivalTaskInfoList = GetFestivalTaskInfoList(_data.cfg.Id);
            bool isFindCanGetTask = festivalTaskInfoList.FirstOrDefault((FestivalTaskInfo festivalTaskInfo) =>
            {
                FestivalTaskConfig festivalTaskConfig = Configs.FestivalTask.GetConfig(festivalTaskInfo.Id);
                if (festivalTaskConfig == null)
                {
                    Debug.Log("ActivityController , CheckRedDot_AllStarTask , festivalTaskConfig == null , festivalTaskInfo.Id = " + festivalTaskInfo.Id);
                    return false;
                }
                return festivalTaskInfo.Obtain == false && festivalTaskInfo.Current >= festivalTaskConfig.Target;
            }) != null;
            isRed |= isFindCanGetTask;
        }

        BoxRedDotNode.AddValue(isRed ? 1 : -1);

        Debug.Log("CheckRedDot_AllStarTask , isRed = " + isRed);
        return isRed;
    }
    /// <summary>全明星2024礼包红点/summary>
    public bool CheckRedDot_AllStarGift(ActivityData _data)
    {
        if (_data == null) return false;
        bool isRed = false;
        RedDotNode BoxRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        //有每日免费礼物没领取
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true)
        {
            if (!string.IsNullOrWhiteSpace(_data.cfg.DailyGift)) isRed |= !dailyGiftReceivedActivityIdSet.Contains(_data.cfg.Id);
        }

        BoxRedDotNode.AddValue(isRed ? 1 : -1);

        Debug.Log("CheckRedDot_AllStarGift , isRed = " + isRed);
        return isRed;
    }

    #endregion

    #region 劳动节2024

    /// <summary>劳动节2024旅行地图红点/summary>
    public bool CheckRedDot_LabourDayHome(ActivityData _data)
    {
        bool isRed = false;
        if (_data == null) return false;
        RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        if (_data != null && ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true)
        {
            bool isGetAllReward = LabourDayManager.Instance.IsGetAllReward;//若所有奖励已领取，则关闭“活动入口”红点提示。
            if (!isGetAllReward)
            {
                GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, int.Parse(_data.cfg.Param2), 0);
                int diceCount = gameItem.GetPlayerCount();
                bool isHasNotUseDice = diceCount > 0;//若有未使用的骰子，则“活动入口”红点提示。
                isRed = isHasNotUseDice;
            }

        }

        redDotNode.AddValue(isRed ? 1 : -1);
        Debug.Log("CheckRedDot_AllStarHome , isRed = " + isRed);

        return isRed;
    }

    /// <summary>劳动节2024任务红点/summary>
    public bool CheckRedDot_LabourDayTask(ActivityData _data)
    {
        if (_data == null) return false;
        bool isRed = false;
        RedDotNode BoxRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        //有已完成但未领取的任务
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id))
        {
            List<FestivalTaskInfo> festivalTaskInfoList = GetFestivalTaskInfoList(_data.cfg.Id);
            bool isFindCanGetTask = festivalTaskInfoList.FirstOrDefault((FestivalTaskInfo festivalTaskInfo) =>
            {
                FestivalTaskConfig festivalTaskConfig = Configs.FestivalTask.GetConfig(festivalTaskInfo.Id);
                if (festivalTaskConfig == null)
                {
                    Debug.Log("ActivityController , CheckRedDot_LabourDayTask , festivalTaskConfig == null , festivalTaskInfo.Id = " + festivalTaskInfo.Id);
                    return false;
                }
                return festivalTaskInfo.Obtain == false && festivalTaskInfo.Current >= festivalTaskConfig.Target;
            }) != null;
            isRed |= isFindCanGetTask;
        }

        BoxRedDotNode.AddValue(isRed ? 1 : -1);

        Debug.Log("CheckRedDot_LabourDayTask , isRed = " + isRed);
        return isRed;
    }
    /// <summary>劳动节2024礼包红点/summary>
    public bool CheckRedDot_LabourDayGift(ActivityData _data)
    {
        if (_data == null) return false;
        bool isRed = false;
        RedDotNode BoxRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        //有每日免费礼物没领取
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true)
        {
            if (!string.IsNullOrWhiteSpace(_data.cfg.DailyGift)) isRed |= !dailyGiftReceivedActivityIdSet.Contains(_data.cfg.Id);
        }

        BoxRedDotNode.AddValue(isRed ? 1 : -1);

        Debug.Log("CheckRedDot_LabourDayGift , isRed = " + isRed);
        return isRed;
    }

    /// <summary>劳动节2024祈愿签到红点/summary>
    private bool CheckRedDot_LabourDaySign(ActivityData _data)
    {
        bool isRed = false;
        RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true)
        {
            int rewardCount = 5;
            WishSignConfig wishSignConfig = Configs.WishSign.GetConfig(_data.cfg.Id);
            if (wishSignConfig == null)
            {
                Debug.LogError("CheckRedDot_LabourDayWish , wishSignConfig == null , _data.cfg.Id = {0}".SafeFormat(_data.cfg.Id));
            }
            else
            {
                rewardCount = wishSignConfig.RewardCount;
            }

            DateTime serverDT = Utils.DataConvUtil.ServerDateTime;
            DateTime startDT = DateTimeOffset.FromUnixTimeSeconds(_data.StartTime).DateTime.ToLocalTime();
            TimeSpan timeSpan = serverDT - startDT;
            int openDayCount = timeSpan.Days;
            int openItemCount = Utility.KeepInRange(openDayCount + 1, 1, rewardCount);

            bool isHasSignNotSet = openItemCount > ActivityController.Instance.wishSigns.Count;//许愿签未设置
            isRed |= isHasSignNotSet;

            bool isHasSignNotGet = false;//许愿签可领取
            for (int i = 0; i < rewardCount; i++)
            {
                int itemIndex = i + 1;
                if (itemIndex > openItemCount) continue;//没开
                if (itemIndex > ActivityController.Instance.wishSigns.Count) continue;//没设置东西
                if (itemIndex <= ActivityController.Instance.wishSignRewards.Count) continue;//领取过了
                if (Player.TaskManager.DailyTasks.Point - (ActivityController.Instance.todayWishTimes * 100) < 100) continue;//活越点数不足
                isHasSignNotGet = true;
                break;
            }
            isRed |= isHasSignNotGet;
        }

        redDotNode.AddValue(isRed ? 1 : -1);
        Debug.Log("CheckRedDot_LabourDayWish , isRed = " + isRed);

        return isRed;
    }

    #endregion

    #region 季后赛总决赛竞猜2024

    /// <summary>季后赛总决赛竞猜2024 冠军和MVP/summary>
    public bool CheckRedDot_PlayoffFinalsGuessHome(ActivityData _data)
    {
        bool isRed = false;
        if (_data == null) return false;
        RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        if (_data != null && ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true)
        {
            //结束后有每日免费礼物没领取
            if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true)
            {
                if (!string.IsNullOrWhiteSpace(_data.cfg.DailyGift))
                {
                    bool rewardNotGet = !dailyGiftReceivedActivityIdSet.Contains(_data.cfg.Id);
                    bool isGuessEnd = PlayoffFinalsGuessManager.Instance.isGuessEnd;
                    isRed |= rewardNotGet && isGuessEnd;
                }
            }
            PlayoffFinalsGuessManager.Stage stage = PlayoffFinalsGuessManager.Instance.GetStage();
            //可以猜测冠军
            isRed |= stage == PlayoffFinalsGuessManager.Stage.CanSelectTeam;
            //可以猜测MVP
            isRed |= stage == PlayoffFinalsGuessManager.Stage.CanSelectMVP;

            //有冠军和MVP奖励没领取
            bool isEndRewardCanGet = PlayoffFinalsGuessManager.Instance.isEndRewardCanGet;
            isRed |= isEndRewardCanGet;
        }

        redDotNode.AddValue(isRed ? 1 : -1);
        Debug.Log("CheckRedDot_PlayoffFinalsGuessHome , isRed = " + isRed);

        return isRed;
    }

    /// <summary>季后赛总决赛竞猜2024 单场预测和幸运数字/summary>
    public bool CheckRedDot_PlayoffFinalsGuessSingle(ActivityData _data)
    {
        bool isRed = false;
        if (_data == null) return false;
        RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        List<FinalsGuessCourseConfig> finalsGuessCourseConfigList = PlayoffFinalsGuessManager.Instance.GetCanShowCourse();
        foreach (FinalsGuessCourseConfig finalsGuessCourseConfig in finalsGuessCourseConfigList)
        {
            //可以领取胜利奖励
            isRed |= PlayoffFinalsGuessManager.Instance.HasSingleRewardCanGet(finalsGuessCourseConfig.Id);
            //可以领取幸运数字奖励
            isRed |= PlayoffFinalsGuessManager.Instance.HasLuckyNumberRewardCanGet(finalsGuessCourseConfig.Id);
            //可以猜测胜利
            bool isInCanGuessTime = finalsGuessCourseConfig.MatchTime > Utils.DataConvUtil.ServerTime;
            bool isGuessSingle = PlayoffFinalsGuessManager.Instance.GetGuessSingle(finalsGuessCourseConfig.Id) != null;
            isRed |= isInCanGuessTime && !isGuessSingle;
            //可以猜测幸运数字
            bool isGuessLuckyNumber = PlayoffFinalsGuessManager.Instance.GetGuessLuckyNumber(finalsGuessCourseConfig.Id) != null;
            isRed |= isInCanGuessTime && !isGuessLuckyNumber;
        }

        redDotNode.AddValue(isRed ? 1 : -1);
        Debug.Log("CheckRedDot_PlayoffFinalsGuessSingle , isRed = " + isRed);

        return isRed;
    }

    #endregion

    #region 端午节赛龙舟2024

    /// <summary>端午节赛龙舟2024主页红点/summary>
    public bool CheckRedDot_DragonBoatFestivalHome(ActivityData _data)
    {
        bool isRed = false;
        if (_data == null) return false;
        RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        if (_data != null && ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true)
        {
            //若可以选择队伍，则“活动入口”红点提示。
            isRed |= DragonBoatFestivalManager.Instance.CanSelectTeam;

            //若有未使用的龙舟鼓，则“活动入口”红点提示。
            isRed |= DragonBoatFestivalManager.Instance.CanUseDrum;

            //若有已完成的限时任务
            isRed |= DragonBoatFestivalManager.Instance.CanCollectTask;

            //若有已完成的里程碑
            isRed |= DragonBoatFestivalManager.Instance.CanCollectProgress;
        }

        redDotNode.AddValue(isRed ? 1 : -1);
        Debug.Log("CheckRedDot_DragonBoatFestivalHome , isRed = " + isRed);

        return isRed;
    }

    /// <summary>端午节赛龙舟2024任务红点/summary>
    public bool CheckRedDot_DragonBoatFestivalTask(ActivityData _data)
    {
        if (_data == null) return false;
        bool isRed = false;
        RedDotNode BoxRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        //有已完成但未领取的任务
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id))
        {
            DragonBoatFestivalManager.Stage stage = DragonBoatFestivalManager.Instance.GetStage();
            if (stage == DragonBoatFestivalManager.Stage.NormalPlaying)
            {
                List<FestivalTaskInfo> festivalTaskInfoList = GetFestivalTaskInfoList(_data.cfg.Id);
                bool isFindCanGetTask = festivalTaskInfoList.FirstOrDefault((FestivalTaskInfo festivalTaskInfo) =>
                {
                    FestivalTaskConfig festivalTaskConfig = Configs.FestivalTask.GetConfig(festivalTaskInfo.Id);
                    if (festivalTaskConfig == null)
                    {
                        Debug.Log("ActivityController , CheckRedDot_DragonBoatFestivalTask , festivalTaskConfig == null , festivalTaskInfo.Id = " + festivalTaskInfo.Id);
                        return false;
                    }
                    return festivalTaskInfo.Obtain == false && festivalTaskInfo.Current >= festivalTaskConfig.Target;
                }) != null;
                isRed |= isFindCanGetTask;
            }
        }

        BoxRedDotNode.AddValue(isRed ? 1 : -1);

        Debug.Log("CheckRedDot_DragonBoatFestivalTask , isRed = " + isRed);
        return isRed;
    }
    /// <summary>端午节赛龙舟2024礼包红点/summary>
    public bool CheckRedDot_DragonBoatFestivalGift(ActivityData _data)
    {
        if (_data == null) return false;
        bool isRed = false;
        RedDotNode BoxRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        //有每日免费礼物没领取
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true)
        {
            if (!string.IsNullOrWhiteSpace(_data.cfg.DailyGift)) isRed |= !dailyGiftReceivedActivityIdSet.Contains(_data.cfg.Id);
        }

        BoxRedDotNode.AddValue(isRed ? 1 : -1);

        Debug.Log("CheckRedDot_DragonBoatFestivalGift , isRed = " + isRed);
        return isRed;
    }

    /// <summary>端午节赛龙舟2024祈愿签到红点/summary>
    private bool CheckRedDot_DragonBoatFestivalSign(ActivityData _data)
    {
        if (_data == null) return false;
        bool isRed = false;
        RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true)
        {
            int rewardCount = 5;
            WishSignConfig wishSignConfig = Configs.WishSign.GetConfig(_data.cfg.Id);
            if (wishSignConfig == null)
            {
                Debug.LogError("CheckRedDot_DragonBoatFestivalSign , wishSignConfig == null , _data.cfg.Id = {0}".SafeFormat(_data.cfg.Id));
            }
            else
            {
                rewardCount = wishSignConfig.RewardCount;
            }

            DateTime serverDT = Utils.DataConvUtil.ServerDateTime;
            DateTime startDT = DateTimeOffset.FromUnixTimeSeconds(_data.StartTime).DateTime.ToLocalTime();
            TimeSpan timeSpan = serverDT - startDT;
            int openDayCount = timeSpan.Days;
            int openItemCount = Utility.KeepInRange(openDayCount + 1, 1, rewardCount);

            bool isHasSignNotSet = openItemCount > ActivityController.Instance.wishSigns.Count;//许愿签未设置
            isRed |= isHasSignNotSet;

            bool isHasSignNotGet = false;//许愿签可领取
            for (int i = 0; i < rewardCount; i++)
            {
                int itemIndex = i + 1;
                if (itemIndex > openItemCount) continue;//没开
                if (itemIndex > ActivityController.Instance.wishSigns.Count) continue;//没设置东西
                if (itemIndex <= ActivityController.Instance.wishSignRewards.Count) continue;//领取过了
                if (Player.TaskManager.DailyTasks.Point - (ActivityController.Instance.todayWishTimes * 100) < 100) continue;//活越点数不足
                isHasSignNotGet = true;
                break;
            }
            isRed |= isHasSignNotGet;
        }

        redDotNode.AddValue(isRed ? 1 : -1);
        Debug.Log("CheckRedDot_DragonBoatFestivalSign , isRed = " + isRed);

        return isRed;
    }

    #endregion

    #region 端午节赛龙舟2024

    /// <summary>端午节赛龙舟2024礼包红点/summary>
    public bool CheckRedDot_Olympics2024Gift(ActivityData _data)
    {
        if (_data == null) return false;
        bool isRed = false;
        RedDotNode BoxRedDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        //有每日免费礼物没领取
        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true)
        {
            if (!string.IsNullOrWhiteSpace(_data.cfg.DailyGift)) isRed |= !dailyGiftReceivedActivityIdSet.Contains(_data.cfg.Id);
        }

        BoxRedDotNode.AddValue(isRed ? 1 : -1);

        Debug.Log("CheckRedDot_Olympics2024Gift , isRed = " + isRed);
        return isRed;
    }

    /// <summary>端午节赛龙舟2024祈愿签到红点/summary>
    private bool CheckRedDot_Olympics2024Sign(ActivityData _data)
    {
        if (_data == null) return false;
        bool isRed = false;
        RedDotNode redDotNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + _data.cfg.Id);

        if (ActivityController.Instance.OnlineActivityDic.ContainsKey(_data.cfg.Id) == true)
        {
            int rewardCount = 5;
            WishSignConfig wishSignConfig = Configs.WishSign.GetConfig(_data.cfg.Id);
            if (wishSignConfig == null)
            {
                Debug.LogError("CheckRedDot_Olympics2024Sign , wishSignConfig == null , _data.cfg.Id = {0}".SafeFormat(_data.cfg.Id));
            }
            else
            {
                rewardCount = wishSignConfig.RewardCount;
            }

            DateTime serverDT = Utils.DataConvUtil.ServerDateTime;
            DateTime startDT = DateTimeOffset.FromUnixTimeSeconds(_data.StartTime).DateTime.ToLocalTime();
            TimeSpan timeSpan = serverDT - startDT;
            int openDayCount = timeSpan.Days;
            int openItemCount = Utility.KeepInRange(openDayCount + 1, 1, rewardCount);

            bool isHasSignNotSet = openItemCount > ActivityController.Instance.wishSigns.Count;//许愿签未设置
            isRed |= isHasSignNotSet;

            bool isHasSignNotGet = false;//许愿签可领取
            for (int i = 0; i < rewardCount; i++)
            {
                int itemIndex = i + 1;
                if (itemIndex > openItemCount) continue;//没开
                if (itemIndex > ActivityController.Instance.wishSigns.Count) continue;//没设置东西
                if (itemIndex <= ActivityController.Instance.wishSignRewards.Count) continue;//领取过了
                if (Player.TaskManager.DailyTasks.Point - (ActivityController.Instance.todayWishTimes * 100) < 100) continue;//活越点数不足
                isHasSignNotGet = true;
                break;
            }
            isRed |= isHasSignNotGet;
        }

        redDotNode.AddValue(isRed ? 1 : -1);
        Debug.Log("CheckRedDot_Olympics2024Sign , isRed = " + isRed);

        return isRed;
    }

    #endregion

}
