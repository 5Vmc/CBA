using BigBang;
using GameConfig;
using GameConfig.Config;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.GameItem;

public class HonourGroupData
{
    public int clientGroup = 0;

    public List<AchievementData> list = new();

}

public class AchievementGroupData
{
    public int Index;
    public int funid;

    /// <summary>
    /// 检查任务是否全部完成，任务是线性的，简化判断：只查最后这个是否已经领取了
    /// 数据错误的时候，在线性中间状态的任务未领取检查不出来。。。
    /// </summary>
    public bool AllFinish
    {
        get
        {
            if (Index == list.Count - 1 && list[list.Count - 1].Received == 1)
            {
                return true;
            }
            return false;
        }
    }

    public void Next(Protocol.AchievemtData remoteData)
    {
        //if (Index < list.Count - 1)
        //{
        //    Index++;
        //    _currendData = list[Index];
        //}
        //else {
        //    _currendData = list[list.Count - 1];
        //}
        if (remoteData != null)
        {
            var count = list.Count;
            for (var index = 0; index < list.Count; index++)
            {
                if (list[index].ID == remoteData.Id)
                {
                    Index = index;
                    //属性构造器内部检查过了哪些用服务端数据那些不用
                    list[index].Current = remoteData.Current;
                    _currendData = list[index];
                }
            }

        }
    }

    public List<AchievementData> list;

    private AchievementData _currendData;
    public AchievementData CurrentData
    {
        get
        {
            //没有初始化过
            if (_currendData == null)
            {
                _currendData = list[Index];
            }
            return _currendData;
        }
    }


    public AchievementGroupData()
    {
        Index = 0;
        list = new List<AchievementData>();
    }

    /// <summary>
    /// 查找满足id的data
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public (int, AchievementData) GetAchievementData(int id)
    {
        var count = list.Count;
        for (var index = 0; index < count; index++)
        {
            if (list[index].ID == id)
            {
                return (index, list[index]);
            }
        }

        return (-1, null);
    }
}

public class AchievementData
{
    public AchievementConfig Config;

    public long time = 0;

    // 成就ID
    public int ID { get => Config.Id; }
    // 成就类型
    public AchievementType Type { get => (AchievementType)Config.Type; }

    /// <summary>
    /// 0已完成已领取，1未完成，2已完成未领取
    /// </summary>
    public int Status
    {
        get
        {
            if (Received == 1)
            {
                return 0;
            }
            else
            {
                return IsComplete ? 2 : 1;
            }
        }
    }
    /// <summary>
    /// 领过是1
    /// </summary>
    public int Received = 0;
    // 奖励
    public IEnumerable<GameItem> Rewards { get => GameItemUtils.CreateGameItems(Config.Reward); }

    private int _current;
    // 当前进度
    public int Current
    {
        get
        {
            #region 客户端判断的部分自己算, 在其他地方不做触发机制，所以这里要实时算。
            if (Config.Fungroup == 1034)
            {
                //N名M等级球员
                return Player.CardManager.GetCardList().Count(p => p.Level >= Config.Target[1]);
            }
            else if (Config.Fungroup == 1035)
            {
                if (Config.Target.Length == 3)
                {
                    //N名X品质M星球员
                    return Player.CardManager.GetCardList().Count(p => p.Star >= Config.Target[1] && p.Quality >= Config.Target[2]);
                }
                //N名M星球员
                return Player.CardManager.GetCardList().Count(p => p.Star >= Config.Target[1]);
            }
            else if (Config.Fungroup == 1036)
            {
                //N名M品质球员
                return Player.CardManager.GetCardList().Count(p => p.Quality >= Config.Target[1]);
            }
            else if (Config.Fungroup == 1037)
            {
                //N名战力超过M的球员
                return Player.CardManager.GetCardList().Count(p => p.FightPoint >= Config.Target[1]);
            }
            else if (Config.Fungroup == 1038)
            {
                //俱乐部战力达到
                return Player.Strength;
            }
            else if (Config.Fungroup == 1039)
            {
                //N名球员
                return Player.CardManager.GetCardList().Count;
            }
            else if (Config.Fungroup == 1040)
            {
                //N名装备突破M阶的球员
                return Player.CardManager.GetCardList().Count(p => p.EquipGrade >= Config.Target[1]);
            }
            else if (Config.Fungroup == 1041)
            {
                //完成N名球员剧情挑战
                var chapterDict = HeroManager.Instance.heroChapterDataStarListDic;
                //todo://heroChapterDataStarListDic
                var finishCount = 0;
                foreach (var listData in chapterDict.Values)
                {
                    bool anyNotPassed = listData.Exists(p => p.PassAll == false);
                    if (!anyNotPassed)
                    {
                        finishCount++;
                    }
                }
                return finishCount;
            }
            else if (Config.Fungroup == 1043)
            {
                //解锁N个战术
                return Player.FightManager.FormationController.TacticsLevelDic.Count;
            }
            else if (Config.Fungroup == 1044)
            {
                //拥有N个M级战术
                return Player.FightManager.FormationController.TacticsLevelDic.Count(p => p.Value >= Config.Target[1]);
            }
            else
            {
                return _current;
            }
            #endregion

        }
        set
        {
            if (Config.ClientCheck == 0)
                _current = value;
            else
            {
                _current = 0;
            }
        }
    }

    public int HonourCurrentShow
    {
        get
        {
            if (Config.MaxShowTimes <= 0)
            {
                return Current;
            }
            else
            {
                return Mathf.Min(Current, Config.MaxShowTimes);
            }
        }
    }

    public bool IsComplete
    {
        get
        {
            if (Config.ClientCheck != 1)
            {
                //以N节爆发的N星球队出战K场，判断K值
                if (Config.Fungroup >= 1045 && Config.Fungroup <= 1048)
                {
                    return Current >= Config.Target[2];
                }
                else
                {
                    return Current >= Config.Target[0];
                }
            }
            else
            {
                return Current >= Config.Target[0];
            }

        }
    }

    public AchievementData(int id, int current, int received = 0, long time = 0)
    {
        Config = Configs.Achievement.GetConfig(id);
        Current = current;
        Received = received;
        this.time = time;
    }
}
