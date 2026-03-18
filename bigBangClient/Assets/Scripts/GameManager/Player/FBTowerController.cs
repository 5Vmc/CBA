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

public class TowerBuffData
{
    public int buffType;
    public int buffPos;
    public int buffValue;

    public TowerBuffData(int type, int pos, int value)
    {
        buffType = type;
        buffPos = pos;
        buffValue = value;
    }
}

public class TowerFBData
{

    /// <summary>
    /// 位置， ability, value
    /// </summary>
    public Dictionary<int, Dictionary<int, int>> buffs = new();
    /// <summary>
    /// 通关数据
    /// </summary>
    public List<TowerPassData> PassData = new();
    /// <summary>
    /// 每日失败次数
    /// </summary>
    public int failCount;
    /// <summary>
    /// 爬塔充值次数，为0才可以重置
    /// </summary>
    public int resetCount;
    /// <summary>
    /// 当前关卡指针，指向关卡ID（应该打，还没打的关卡）
    /// </summary>
    public int currentDungeonId;
    /// <summary>
    /// 最大星数
    /// </summary>
    public int totalStar = 0;
    /// <summary>
    /// 当前累计的星星
    /// </summary>
    public int currentStar = 0;
    /// <summary>
    /// 已经领取的奖励
    /// </summary>
    public int getRewardsId = 0;

    /// <summary>
    /// 可以扫荡
    /// </summary>
    public bool isCanBatchBattle = false;

    /// <summary>
    /// 打通关所有关卡
    /// </summary>
    public bool isAllPass = false;
    /// <summary>
    /// 当前关卡配置
    /// </summary>
    public TowerConfig currentLevelConfig = null;
    /// <summary>
    /// 当前章节配置
    /// </summary>
    public TowerChapterConfig currentChapterConfig = null;
    /// <summary>
    /// 所有奖励被领取了
    /// </summary>
    public bool isAllRewardGet = false;
    /// <summary>
    /// 下一个没领取领取的总星数奖励配置
    /// </summary>
    public TowerStarRewardConfig currentTowerStarRewardConfig = null;
}

public class TowerLevelData
{
    public TowerLevelData()
    {

    }
    public TowerLevelData(TowerConfig towerConfig)
    {
        this.towerConfig = towerConfig;
    }

    public TowerConfig towerConfig;
    public TowerPassData passData
    {
        get
        {
            TowerPassData _towerPassData = FBTowerController.Instance.GetPassData(towerConfig);
            if (_towerPassData == null)
            {
                _towerPassData = new();
                _towerPassData.Stars.Clear();
                _towerPassData.Stars.Add(0);
                _towerPassData.Stars.Add(0);
                _towerPassData.Stars.Add(0);
                _towerPassData.Id = towerConfig.Id;
                _towerPassData.Buffer = 0;
            }
            return _towerPassData;
        }
    }
    public TowerTypeState towerTypeState
    {
        get
        {
            return FBTowerController.Instance.GetTypeState(towerConfig);
        }
    }
    public TowerOpenState towerOpenState
    {
        get
        {
            return FBTowerController.Instance.GetOpenState(towerConfig);
        }
    }
    public TowerChapterConfig towerChapterConfig
    {
        get
        {
            return Configs.TowerChapter.GetConfig(towerConfig.Chapter);
        }
    }
    public List<GameItem> rewardGameItemList
    {
        get
        {
            return GameItemUtils.CreateGameItems(towerConfig.Reward).ToList();
        }
    }
    public bool isBuff
    {
        get
        {
            return !string.IsNullOrWhiteSpace(towerConfig.Buff);
        }
    }
}

public enum TowerTypeState
{
    UnKnown,
    Normal,
    Buff,
}
public enum TowerOpenState
{
    Lock,
    Now,
    Pass,
}

public class FBTowerController : Singleton<FBTowerController>
{
    public TowerFBData FBData = new();
    /// <summary>
    /// 每日最大重置次数
    /// </summary>
    public const int MaxResetCount = 1;
    /// <summary>
    /// 每日最大失败次数
    /// </summary>
    public const int MaxDailyFailCount = 3;
    /// <summary>
    /// 扫荡默认选择的buffId
    /// </summary>
    const int defaultBuffId = 3;
    /// <summary>
    /// 战斗后，只对最后1个据点显示动画
    /// </summary>
    public bool OnlyEnableNewDuntionAni;
    /// <summary>
    /// 整章通关，播放动画
    /// </summary>
    public bool OneChapterFinish;

    public TowerLevelData GetTowerLevelData(int towerConfigId)
    {
        TowerConfig levelconfig = Configs.Tower.GetConfig(towerConfigId);
        if (levelconfig == null)
        {
            Debug.LogWarningFormat("FBTowerController , GetTowerLevelData , levelconfig == null , towerConfigId = {0}", towerConfigId);
            return null;
        }
        return GetTowerLevelData(levelconfig);
    }
    public TowerLevelData GetTowerLevelData(TowerConfig levelconfig)
    {
        TowerLevelData towerLevelData = new();
        towerLevelData.towerConfig = levelconfig;
        return towerLevelData;
    }
    public TowerPassData GetPassData(TowerConfig levelconfig)
    {
        return FBData.PassData.FirstOrDefault(p => p.Id == levelconfig.Id);
    }
    public TowerOpenState GetOpenState(TowerConfig levelconfig)
    {
        TowerOpenState towerOpenState = TowerOpenState.Lock;
        if (levelconfig.Id < FBTowerController.Instance.FBData.currentLevelConfig.Id || FBTowerController.Instance.FBData.isAllPass)
        {
            towerOpenState = TowerOpenState.Pass;
        }
        else if (levelconfig == FBTowerController.Instance.FBData.currentLevelConfig)
        {
            towerOpenState = TowerOpenState.Now;
        }
        else
        {
            towerOpenState = TowerOpenState.Lock;
        }
        return towerOpenState;
    }
    public TowerTypeState GetTypeState(TowerConfig levelconfig)
    {
        TowerTypeState towerTypeState = TowerTypeState.Normal;
        if (string.IsNullOrWhiteSpace(levelconfig.Buff) == false)
        {
            towerTypeState = TowerTypeState.Buff;
        }
        else
        {
            towerTypeState = TowerTypeState.Normal;
        }
        return towerTypeState;
    }

    #region 推送解包
    /// <summary>
    /// 解包关卡状态、星级奖励领取状态。
    /// </summary>
    public void UnPack(UpdatePVEInfoNotify data)
    {
        //解包服务端推送和传回来的东西
        FBData = new();
        FBData.PassData = data.TowerPassData.OrderBy(item => item.Id).ToList();
        FBData.failCount = data.TowerDayFailCount;
        FBData.resetCount = data.TowerDayResetCount;
        FBData.currentStar = data.TowerCurrentStar;
        FBData.totalStar = data.TowerSumStar;
        FBData.getRewardsId = data.TowerSumStarReward;
        FBData.buffs = new Dictionary<int, Dictionary<int, int>> { { 1, new() }, { 2, new() }, { 3, new() }, { 4, new() }, { 5, new() } };

        FBData.isAllPass = data.TowerCurrentId == 9999999;
        if (FBData.isAllPass)
        {
            data.TowerCurrentId = Configs.Tower.GetConfigList()[^1].Id;
        }
        FBData.currentDungeonId = data.TowerCurrentId;
        FBData.currentLevelConfig = Configs.Tower.GetConfig(data.TowerCurrentId);
        FBData.currentChapterConfig = Configs.TowerChapter.GetConfig(FBData.currentLevelConfig.Chapter);

        FBData.isAllRewardGet = FBData.getRewardsId >= Configs.TowerStarReward.GetConfigList()[^1].Id;
        if (FBData.isAllRewardGet == false)
        {
            FBData.currentTowerStarRewardConfig = Configs.TowerStarReward.GetConfig(FBData.getRewardsId + 1);
        }

        foreach (TowerPassData _data in FBData.PassData)
        {
            var cfg = Configs.Tower.GetConfig(_data.Id);
            if (cfg.Id < FBData.currentDungeonId && cfg.Buff != "")
            {
                addBuff(_data.Buffer, cfg);
            }
        }

        RefreshIsCanBatchBattle();

        TowerPassData firstNot3Star = FBData.PassData.FirstOrDefault(p => p.Stars.Sum() != 3);
        if (firstNot3Star != null)
        {
            var cfg = Configs.Tower.GetConfig(firstNot3Star.Id);
        }

        EventManager.Instance.Dispatch(EventID.AfterGetFBTowerData);
    }
    #endregion

    private void RefreshIsCanBatchBattle()
    {
        if (FBData.currentLevelConfig.Buff != "")
        {
            FBData.isCanBatchBattle = false;
            return;
        }
        TowerPassData towerPassData = GetPassData(FBData.currentLevelConfig);
        if (towerPassData == null || towerPassData.Stars.Sum() < 3)
        {
            FBData.isCanBatchBattle = false;
            return;
        }
        if (towerPassData != null && FBData.currentLevelConfig.NextId == 9999999)
        {
            FBData.isCanBatchBattle = false;
            return;
        }
        FBData.isCanBatchBattle = true;
    }

    /// <summary>
    /// 数据添加到buff字典中。
    /// </summary>
    /// <param name="buffId"></param>
    /// <param name="cfg"></param>
    /// <returns>要消耗的星星数量</returns>
    private int addBuff(int buffId, TowerConfig cfg)
    {
        if (cfg.Buff != "")
        {
            int _buffpos, _bufftype, _buffvalue;
            var _buff = cfg.Buff.Split("|")[buffId - 1];
            string[] _buffparam = _buff.Split(":");
            _buffpos = int.Parse(_buffparam[0]);
            _bufftype = int.Parse(_buffparam[1]);
            _buffvalue = int.Parse(_buffparam[2]);

            if (FBData.buffs[_buffpos].ContainsKey(_bufftype))
            {
                FBData.buffs[_buffpos][_bufftype] += _buffvalue;
            }
            else
            {
                FBData.buffs[_buffpos][_bufftype] = _buffvalue;
            }
            return int.Parse(_buffparam[3]);
        }
        return 0;
    }

    /// <summary>
    /// 把组装的passdata追加
    /// </summary>
    /// <param name="newPassData"></param>
    private void addPassData(TowerPassData newPassData)
    {
        var passedDataOld = FBData.PassData.FirstOrDefault(p => p.Id == newPassData.Id);
        int newStars = newPassData.Stars.Sum();
        var cfg = Configs.Tower.GetConfig(newPassData.Id);
        int dungeonId;

        if (passedDataOld == null)
        {
            FBData.PassData.Add(newPassData);
            FBData.currentStar += newStars;
            FBData.totalStar += newStars;
            dungeonId = newPassData.Id;
        }
        else
        {
            //星数更大才更新
            var oldStars = passedDataOld.Stars.Sum();
            if (newStars > oldStars)
            {
                passedDataOld.Buffer = newPassData.Buffer;
                passedDataOld.Stars.Clear();
                passedDataOld.Stars.AddRange(newPassData.Stars);
                FBData.totalStar += newStars - oldStars;
                oldStars = newStars;
            }

            FBData.currentStar += newStars;

            dungeonId = passedDataOld.Id;
        }

        if (cfg.Id >= FBData.currentDungeonId)
        {
            if (newPassData.Id % 10 == 0)
            {
                //最后1关
                OneChapterFinish = true;
            }
            else
            {
                OnlyEnableNewDuntionAni = true;
            }
        }

        //更新当前关卡
        FBData.isAllPass = cfg.NextId == 9999999;
        if (FBData.isAllPass)
        {
            FBData.currentDungeonId = Configs.Tower.GetConfigList()[^1].Id;
        }
        else
        {
            FBData.currentDungeonId = cfg.NextId;
        }
        FBData.currentLevelConfig = Configs.Tower.GetConfig(FBData.currentDungeonId);
        FBData.currentChapterConfig = Configs.TowerChapter.GetConfig(FBData.currentLevelConfig.Chapter);
    }

    /// <summary>
    /// 客户端构建passData;
    /// </summary>
    /// <param name="challengeId"></param>
    /// <param name="buffId"></param>
    /// <param name="star"></param>
    /// <returns></returns>
    private TowerPassData buildPassData(int challengeId, int buffId, int star)
    {
        var _passdata = new TowerPassData();
        _passdata.Id = challengeId;
        _passdata.Buffer = buffId;

        for (var i = 1; i <= 3; i++)
        {
            if (i <= star)
            {
                _passdata.Stars.Add(1);
            }
            else
            {
                _passdata.Stars.Add(0);
            }
        }
        return _passdata;
    }


    /// <summary>
    /// 领取星级奖励
    /// </summary>
    /// <param name="rewardId"></param>
    public void GetRewards(int rewardsId, Action callback)
    {
        var cfg = Configs.TowerStarReward.GetConfig(rewardsId);
        if (cfg.Number > FBData.totalStar)
        {
            Tips.PopTips("最大星数不足，不能领取奖励");
            return;
        }

        if (rewardsId != FBData.getRewardsId + 1)
        {
            Tips.PopTips("请依次领取奖励");
            return;
        }

        NetworkManager.Instance.FBTowerGetRewards(rewardsId, (resp) =>
        {

            EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
            EventManager.Instance.Dispatch(EventID.RefreshWindow);
            //获得奖励浮层
            var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItems(cfg.Reward).ToList());
            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);

            FBData.getRewardsId++;

            FBData.isAllRewardGet = FBData.getRewardsId >= Configs.TowerStarReward.GetConfigList()[^1].Id;
            if (FBData.isAllRewardGet == false)
            {
                FBData.currentTowerStarRewardConfig = Configs.TowerStarReward.GetConfig(FBData.getRewardsId + 1);
            }

            //刷新小红点存储
            CheckRedDot();
            callback?.Invoke();
        });
    }

    /// <summary>
    /// 重置接口
    /// </summary>
    /// <param name="callback"></param>
    public void ResetBattle(Action callback)
    {
        if (LeftResetCount <= 0)
        {
            Tips.PopTips("重置次数已达上限，请明日再来！");
            return;
        }

        if (IsCanRaid)
        {
            Tips.PopTips("请先完成扫荡再重置，否则会丢失大量奖励！");
            return;
        }

        if (LeftResetCount > 0)
        {
            UIController.Instance.OpenWindow<ConfirmationBoxUI>(new ConfirmationBoxUIProperties("您还有{0}次重置机会，重置后将回到初始位置，确认重置游戏吗？".SafeFormat(LeftResetCount), () =>
            {
                FBTowerReset(callback);
            }));
        }
        else
        {
            FBTowerReset(callback);
        }
    }
    private void FBTowerReset(Action callback)
    {
        NetworkManager.Instance.FBTowerReset((resp) =>
        {
            FBData.resetCount++;
            FBData.buffs = new Dictionary<int, Dictionary<int, int>> { { 1, new() }, { 2, new() }, { 3, new() }, { 4, new() }, { 5, new() } };
            FBData.failCount = 0;
            FBData.currentStar = 0;
            FBData.currentDungeonId = Configs.Tower.GetConfigList().First().Id;
            FBData.isAllPass = false;
            FBData.currentLevelConfig = Configs.Tower.GetConfig(FBData.currentDungeonId);
            FBData.currentChapterConfig = Configs.TowerChapter.GetConfig(FBData.currentLevelConfig.Chapter);
            RefreshIsCanBatchBattle();
            callback?.Invoke();
        });
    }

    /// <summary>
    /// 返回动画index
    /// </summary>
    /// <param name="nowCfgId"></param>
    /// <returns></returns>
    public int GetAniDungeonIndex(int nowCfgId)
    {
        if (FBData.currentLevelConfig.Chapter == nowCfgId)
        {
            return (FBTowerController.Instance.FBData.currentDungeonId - 1) % 10;
        }
        else
        {
            return 9;
        }
    }


    /// <summary>
    /// 选择buff,传1,2,3
    /// </summary>
    public void ChooseBuff(int buffId, Action callback)
    {
        var cfg = Configs.Tower.GetConfig(FBData.currentDungeonId);
        if (cfg.Buff == "")
        {
            Tips.PopTips("当前不是buff选择的关卡");
            return;
        }

        NetworkManager.Instance.FBTowerSelectBuff(buffId, (resp) =>
        {
            int useStar = addBuff(buffId, cfg);
            FBData.currentStar -= useStar;
            //buff关卡，要构造一个passData
            var _passdata = buildPassData(cfg.Id, buffId, 0);
            addPassData(_passdata);
            RefreshIsCanBatchBattle();
            callback?.Invoke();
        });
    }

    /// <summary>
    /// 扫荡接口，回调中带了奖励的list
    /// </summary>
    /// <param name="callback"></param>
    public void BatchBattle(Action<List<TowerLevelData>> callback)
    {
        if (!IsCanRaid)
        {
            Tips.PopTips("当前处于最大关卡，无法扫荡");
            return;
        }

        NetworkManager.Instance.FBTowerBatchBattle((resp) =>
        {
            List<TowerConfig> cfgs = Configs.Tower.GetConfigList().Where(cfg => cfg.Id >= FBData.currentDungeonId && cfg.Id < resp.TowerCurrentId).ToList();
            List<TowerLevelData> towerLevelDataList = new List<TowerLevelData>();
            foreach (var cfg in cfgs)
            {

                //构建passdata并设置当前推进的ID
                var passdata = buildPassData(cfg.Id, defaultBuffId, 3);
                FBData.PassData.Add(passdata);

                if (string.IsNullOrWhiteSpace(cfg.Buff))
                {
                    TowerLevelData towerLevelData = new(cfg);
                    towerLevelDataList.Add(towerLevelData);
                    //非BUFF关，加星星
                    FBData.currentStar += 3;
                }
                else
                {
                    //BUFF关，组织buff
                    int useStar = addBuff(3, cfg);
                    FBData.currentStar -= useStar;
                }
            }
            FBData.isCanBatchBattle = false;
            FBData.isAllPass = resp.TowerCurrentId == 9999999;
            if (FBData.isAllPass)
            {
                FBData.currentDungeonId = Configs.Tower.GetConfigList()[^1].Id;
            }
            else
            {
                FBData.currentDungeonId = resp.TowerCurrentId;
            }
            FBData.currentLevelConfig = Configs.Tower.GetConfig(FBData.currentDungeonId);
            FBData.currentChapterConfig = Configs.TowerChapter.GetConfig(FBData.currentLevelConfig.Chapter);
            CheckRedDot();
            callback?.Invoke(towerLevelDataList);
        });
    }

    public bool isFirstPass = true;
    /// <summary>
    /// 挑战接口
    /// </summary>
    /// <param name="challengeId"></param>
    /// <param name="callback"></param>
    public void StartBattle(Action<StartTowerChallengeResponse> callback)
    {
        var passedDataOld = FBData.PassData.FirstOrDefault(p => p.Id == FBData.currentDungeonId);
        if (passedDataOld == null)
        {
            isFirstPass = true;
        }
        else
        {
            if (passedDataOld.Stars[0] > 0)
            {
                isFirstPass = false;
            }
            else
            {
                isFirstPass = true;
            }
        }

        NetworkManager.Instance.FBTowerBattle((resp) =>
        {
            if (resp.Succeed)
            {
                //构建passdata并设置当前ID
                addPassData(resp.PassData);
                RefreshIsCanBatchBattle();
                CheckRedDot();
            }
            else
            {
                FBData.failCount++;
            }
            callback?.Invoke(resp);
        });
    }

    /// <summary>
    /// 刷新小红点，只处理累计奖励小红点和扫荡小红点。
    /// </summary>
    public void CheckRedDot()
    {
        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FBTower, "/star");
        bool isRed = IsCanGetStarRewards;//有最大星星奖励可以领取
        node.AddValue(isRed ? 1 : -1);

        node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FBTower, "/batchbattle");
        isRed = UnityEngine.PlayerPrefs.GetString(PlayerPrefsKeys.FBTowerHomeDailyRedDot + Player.GbId, "") != DataConvUtil.ServerDateTime.ToStringUseFormat3();
        node.AddValue(isRed ? 1 : -1);
    }
    /// <summary>
    /// 可以扫荡
    /// </summary>
    public bool IsCanRaid
    {
        get
        {
            return FBData.isCanBatchBattle;
        }
    }
    /// <summary>
    /// 剩余的重置次数
    /// </summary>
    public int LeftResetCount
    {
        get
        {
            return MaxResetCount - FBData.resetCount;
        }
    }
    /// <summary>
    /// 有最大星星奖励可以领取
    /// </summary>
    public bool IsCanGetStarRewards
    {
        get
        {
            return Configs.TowerStarReward.GetConfigList().Exists(p => p.Number <= FBData.totalStar && p.Id > FBData.getRewardsId);
        }
    }
}
