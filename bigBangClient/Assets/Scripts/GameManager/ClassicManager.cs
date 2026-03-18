using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Google.Protobuf.Collections;
using Protocol;
using UnityEngine;
using Utils;
using Utils.GameItem;

namespace BigBang
{
    /// <summary>
    /// 常规赛的管理类
    /// </summary>
    public class ClassicManager : BabuSingleton<ClassicManager>
    {

        private bool isInited = false;
        /// <summary>
        /// 配置表加载完成后，对数据进行预处理
        /// </summary>
        public void InitOnce(bool forceInit = true)
        {
            if (isInited && !forceInit) return;
            isInited = true;
            ClearCountryCachedData();
            ClearChallengeCount();
            ClearWorldMapCachedLevelData();
            InitClassicMapLevelDataListDic();
            InitClassicCountryLevelDataListDic();
            InitTeamByCountryDic();
            InitEquipRouteDict();
        }

        #region 大地图

        #region 外部接口

        public void RouteToCountryPanel(int countryid, int teamIndex, int itemid = 0)
        {
            OpenClassicMapUI();
            OpenClassicCountryUI(countryid, teamIndex % 100 - 1, null, itemid);
        }

        /// <summary>
        /// 打开大地图（常规赛）
        /// </summary>
        public void OpenClassicMapUI()
        {

            if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicPVE)) return;

            ClearWorldMapCachedLevelData();
            TouchManager.Instance.DisableTouch();
            GetMapData(() =>
            {
                TouchManager.Instance.EnableTouch();
                UIController.Instance.ShowPanel<WorldMapUI>(new WorldMapUIProperties());
            });
        }
        /// <summary>
        /// 从服务器获取大地图所需数据
        /// </summary>
        /// <param name="callback">回调</param>
        private void GetMapData(Action callback)
        {
            if (cacheMap == false)
            {
                NetworkManager.Instance.GetChallengeMapData(1, (GetChallengeMapDataResponse resp) =>
                {
                    cacheMap = true;
                    ProcessLevelMapData(resp);
                    ProcessLevelCountryData(resp);
                    callback?.Invoke();
                });
            }
            else
            {
                callback?.Invoke();
            }
        }

        /// <summary>
        /// 清除大地图缓存，下次用新数据
        /// </summary>
        public void ClearWorldMapCachedLevelData()
        {
            cacheMap = false;
        }

        private bool cacheMap = false;

        #endregion

        #region 大洲数据（大地图底部页签所需数据）
        public class ClassicMapLevelData
        {
            public ChallengeMapConfig challengeMapConfig;
            public int totalCountry = 0;
            public int passCountry = 0;
            public bool isOpen = false;
            public bool isSelect = false;
            public bool isLastOpen = false;
            public int targetCountryId = 0;
        }
        public Dictionary<int, ClassicMapLevelData> classicMapLevelDataDic = new();//所有大洲的数据
        public Dictionary<int, List<ClassicMapLevelData>> classicMapLevelDataListDic = new();//不同难度每个大洲的数据
        public Dictionary<int, int> mapClubCountDic = new();//每个大洲有多少俱乐部
        private void InitClassicMapLevelDataListDic()
        {
            mapClubCountDic.Clear();
            classicMapLevelDataDic.Clear();
            classicMapLevelDataListDic.Clear();
            foreach (ChallengeMapConfig challengeMapConfig in Configs.ChallengeMap.GetConfigList())
            {
                if (classicMapLevelDataListDic.ContainsKey(challengeMapConfig.Level) == false)
                {
                    classicMapLevelDataListDic.Add(challengeMapConfig.Level, new());
                }
                List<ClassicMapLevelData> classicMapLevelDataList = classicMapLevelDataListDic[challengeMapConfig.Level];
                ClassicMapLevelData classicMapLevelData = new();
                classicMapLevelDataList.Add(classicMapLevelData);
                classicMapLevelData.challengeMapConfig = challengeMapConfig;
                classicMapLevelDataDic.Add(challengeMapConfig.Id, classicMapLevelData);
            }
            mapClubCountDic.Clear();
            foreach (ChallengeCountryConfig challengeCountryConfig in Configs.ChallengeCountry.GetConfigList())
            {
                if (mapClubCountDic.ContainsKey(challengeCountryConfig.Map) == false)
                {
                    mapClubCountDic.Add(challengeCountryConfig.Map, 0);
                }
                mapClubCountDic[challengeCountryConfig.Map]++;
            }
            foreach (List<ClassicMapLevelData> classicMapLevelDataList in classicMapLevelDataListDic.Values)
            {
                foreach (ClassicMapLevelData classicMapLevelData in classicMapLevelDataList)
                {
                    if (mapClubCountDic.ContainsKey(classicMapLevelData.challengeMapConfig.Id) == false)
                    {
                        Debug.LogWarningFormat("ClassicManager , InitClassicMapLevelDataListDic , no total country find , challengeMapConfig.Id = {0}", classicMapLevelData.challengeMapConfig.Id);
                        classicMapLevelData.totalCountry = 20;
                        continue;
                    }
                    classicMapLevelData.totalCountry = mapClubCountDic[classicMapLevelData.challengeMapConfig.Id];
                }
            }
        }

        public ClassicMapLevelData classicMapLevelDataLastOpen;
        private void ProcessLevelMapData(GetChallengeMapDataResponse resp)
        {
            for (int i = 1; i <= 3; i++)
            {
                List<ClassicMapLevelData> classicMapLevelDataList = classicMapLevelDataListDic[i];
                foreach (ClassicMapLevelData classicMapLevelData in classicMapLevelDataList)
                {
                    classicMapLevelData.passCountry = 0;
                    classicMapLevelData.isOpen = false;
                    classicMapLevelData.isSelect = false;
                    classicMapLevelData.isLastOpen = false;
                    classicMapLevelData.targetCountryId = 0;
                }
            }

            HashSet<int> passCountrySet = new();
            foreach (ChapterMapInfo chapterMapInfo in resp.Chapters)
            {
                ChallengeCountryConfig challengeCountryConfig = Configs.ChallengeCountry.GetConfig(chapterMapInfo.Id);
                if (challengeCountryConfig == null)
                {
                    Debug.LogWarningFormat("ClassicManager , ProcessLevel , no country config find , chapterMapInfo.Id = {0}", chapterMapInfo.Id);
                    continue;
                }
                bool isFinish = chapterMapInfo.Pass >= challengeCountryConfig.Number;
                if (isFinish)
                {
                    passCountrySet.Add(chapterMapInfo.Id);
                    if (classicMapLevelDataDic.ContainsKey(challengeCountryConfig.Map) == false)
                    {
                        Debug.LogWarningFormat("ClassicManager , ProcessLevel , classicMapLevelDataDic no this map , challengeCountryConfig.Map = {0}", challengeCountryConfig.Map);
                        continue;
                    }
                    ClassicMapLevelData classicMapLevelData = classicMapLevelDataDic[challengeCountryConfig.Map];
                    classicMapLevelData.passCountry++;
                }
            }
            foreach (ChallengeCountryConfig challengeCountryConfig in Configs.ChallengeCountry.GetConfigList())
            {
                if (passCountrySet.Contains(challengeCountryConfig.Id) || passCountrySet.Contains(challengeCountryConfig.Unlock) || challengeCountryConfig.Unlock == 0)
                {
                    if (classicMapLevelDataDic.ContainsKey(challengeCountryConfig.Map) == false)
                    {
                        Debug.LogWarningFormat("ClassicManager , ProcessLevel , classicMapLevelDataDic no this map , challengeCountryConfig.Map = {0}", challengeCountryConfig.Map);
                        continue;
                    }
                    ClassicMapLevelData classicMapLevelData = classicMapLevelDataDic[challengeCountryConfig.Map];
                    classicMapLevelData.isOpen = true;
                }
            }

            classicMapLevelDataLastOpen = null;
            for (int j = 3; j >= 1; j--)
            {
                List<ClassicMapLevelData> classicMapLevelDataList = classicMapLevelDataListDic[j];
                for (int i = classicMapLevelDataList.Count - 1; i >= 0; i--)
                {
                    ClassicMapLevelData classicMapLevelData = classicMapLevelDataList[i];
                    if (classicMapLevelData.isOpen)
                    {
                        classicMapLevelData.isSelect = true;
                        classicMapLevelData.isLastOpen = true;//每个难度的最后一个开启的大洲
                        if (classicMapLevelDataLastOpen == null) classicMapLevelDataLastOpen = classicMapLevelData;//最后开启的难度的最后一个开启的大洲
                        break;
                    }
                }
            }

        }
        #endregion

        #region 国家数据（大地图3DUI所需数据）
        public enum ClassicCountryLockReason
        {
            Unknow,
            PreCountry,
            UserLevel,
        }
        public class ClassicCountryLevelData
        {
            public ClassicCountryLockReason lockReason = ClassicCountryLockReason.Unknow;
            public ChallengeCountryConfig challengeCountryConfig;
            public ChapterMapInfo chapterMapInfo;
            public bool isOpen = false;
            public bool isSelect = false;

            public bool PassAll;
            public RepeatedField<int> Rewards = new RepeatedField<int>() { 0, 0, 0 };
            public int Star;
            public List<PassData> PassData;
        }
        public Dictionary<int, int> mapClubFirstDic = new();//每个大洲的第一个国家，大洲id，国家id
        public Dictionary<int, ClassicCountryLevelData> classicCountryLevelDataDic = new();//所有国家的数据
        public Dictionary<int, List<ClassicCountryLevelData>> classicCountryLevelDataListDic = new();//不同难度每个国家的数据
        private void InitClassicCountryLevelDataListDic()
        {
            mapClubFirstDic.Clear();
            classicCountryLevelDataDic.Clear();
            classicCountryLevelDataListDic.Clear();
            foreach (ChallengeCountryConfig challengeCountryConfig in Configs.ChallengeCountry.GetConfigList())
            {
                if (mapClubFirstDic.ContainsKey(challengeCountryConfig.Map) == false)
                {
                    mapClubFirstDic.Add(challengeCountryConfig.Map, challengeCountryConfig.Id);
                }
                if (classicCountryLevelDataListDic.ContainsKey(challengeCountryConfig.Level) == false)
                {
                    classicCountryLevelDataListDic.Add(challengeCountryConfig.Level, new());
                }
                List<ClassicCountryLevelData> classicCountryLevelDataList = classicCountryLevelDataListDic[challengeCountryConfig.Level];
                ClassicCountryLevelData classicCountryLevelData = new();
                classicCountryLevelDataList.Add(classicCountryLevelData);
                classicCountryLevelData.challengeCountryConfig = challengeCountryConfig;
                classicCountryLevelData.PassData = new List<PassData>();
                classicCountryLevelDataDic.Add(challengeCountryConfig.Id, classicCountryLevelData);
            }
        }
        private void ProcessLevelCountryData(GetChallengeMapDataResponse resp)
        {
            for (int i = 1; i <= 3; i++)
            {
                List<ClassicCountryLevelData> classicCountryLevelDataList = classicCountryLevelDataListDic[i];
                foreach (ClassicCountryLevelData classicCountryLevelData in classicCountryLevelDataList)
                {
                    classicCountryLevelData.chapterMapInfo = null;
                    classicCountryLevelData.isOpen = false;
                    classicCountryLevelData.isSelect = false;
                    classicCountryLevelData.lockReason = ClassicCountryLockReason.PreCountry;
                }
            }

            HashSet<int> passCountrySet = new();
            foreach (ChapterMapInfo chapterMapInfo in resp.Chapters)
            {
                ChallengeCountryConfig challengeCountryConfig = Configs.ChallengeCountry.GetConfig(chapterMapInfo.Id);
                if (challengeCountryConfig == null)
                {
                    Debug.LogWarningFormat("ClassicManager , ProcessLevel , no country config find , chapterMapInfo.Id = {0}", chapterMapInfo.Id);
                    continue;
                }
                classicCountryLevelDataDic[challengeCountryConfig.Id].chapterMapInfo = chapterMapInfo;
                bool isFinish = chapterMapInfo.Pass >= challengeCountryConfig.Number;
                if (isFinish)
                {
                    passCountrySet.Add(chapterMapInfo.Id);
                }
            }
            foreach (ChallengeCountryConfig challengeCountryConfig in Configs.ChallengeCountry.GetConfigList())
            {
                if (classicCountryLevelDataDic.ContainsKey(challengeCountryConfig.Id) == false)
                {
                    Debug.LogWarningFormat("ClassicManager , ProcessLevel , classicCountryLevelDataDic no this country , challengeCountryConfig.Id = {0}", challengeCountryConfig.Id);
                    continue;
                }
                ClassicCountryLevelData classicCountryLevelData = classicCountryLevelDataDic[challengeCountryConfig.Id];

                if (challengeCountryConfig.UserLevel > Player.Level)
                {
                    classicCountryLevelData.isOpen = false;
                    classicCountryLevelData.lockReason = ClassicCountryLockReason.UserLevel;
                }
                else
                {
                    if (passCountrySet.Contains(challengeCountryConfig.Id) || passCountrySet.Contains(challengeCountryConfig.Unlock) || challengeCountryConfig.Unlock == 0)
                    {
                        classicCountryLevelData.isOpen = true;
                        classicCountryLevelData.lockReason = ClassicCountryLockReason.Unknow;
                    }
                    else
                    {
                        classicCountryLevelData.isOpen = false;
                        classicCountryLevelData.lockReason = ClassicCountryLockReason.PreCountry;
                    }
                }
            }

            for (int j = 1; j <= 3; j++)
            {
                List<ClassicCountryLevelData> classicCountryLevelDataList = classicCountryLevelDataListDic[j];
                for (int i = classicCountryLevelDataList.Count - 1; i >= 0; i--)
                {
                    ClassicCountryLevelData classicCountryLevelData = classicCountryLevelDataList[i];
                    if (classicCountryLevelData.isOpen)
                    {
                        classicCountryLevelData.isSelect = true;
                        classicMapLevelDataDic[classicCountryLevelData.challengeCountryConfig.Map].targetCountryId = classicCountryLevelData.challengeCountryConfig.Id;
                        break;
                    }
                }
            }

        }

        #endregion

        #endregion

        #region 经典推图关卡列表

        #region 外部接口

        public bool NeedShowClassicCountryUI { get; set; } = false;

        /// <summary>
        /// 打开挑战界面
        /// </summary>
        public void OpenClassicCountryUI(int countryId, int scrollIndex = -1, Action beforeOpenCallBack = null, int itemid = 0)
        {
            ClearCountryCachedData();
            TouchManager.Instance.DisableTouch();
            GetCountryData(countryId, (countryId) =>
            {
                TouchManager.Instance.EnableTouch();
                beforeOpenCallBack?.Invoke();
                UIController.Instance.ShowPanel<ClassicCountryUI>(new ClassicCountryUIProperties(countryId, scrollIndex, itemid));
            });
        }

        /// <summary>
        /// 从服务器获取挑战界面所需数据
        /// </summary>
        /// <param name="countryId">国家ID</param>
        /// <param name="callback">回调</param>
        private void GetCountryData(int countryId, Action<int> callback)
        {
            if (cachedTeamList.Contains(countryId) == false)
            {
                NetworkManager.Instance.GetChallengeData(countryId, (GetChallengeDataResponse resp) =>
                {
                    if (cachedTeamList.Contains(countryId) == false)
                    {
                        cachedTeamList.Add(countryId);
                    }
                    ProcessCountryTaskData(countryId, resp);
                    ProcessCountryTeamData(countryId, resp);
                    callback?.Invoke(countryId);
                });
            }
            else
            {
                callback?.Invoke(countryId);
            }
        }

        /// <summary>
        /// 从国家界面返回大地图
        /// </summary>
        public void BackToClassicCountryFormBattle(int countryId, Action callback)
        {
            TouchManager.Instance.DisableTouch();
            GetMapData(() =>
            {
                GetCountryData(countryId, (countryId) =>
                {
                    TouchManager.Instance.EnableTouch();
                    callback?.Invoke();
                });
            });
        }

        /// <summary>
        /// 在挑战界面内更换国家
        /// </summary>
        public void ChangeClassicCountryCountry(int countryId, Action callback)
        {
            TouchManager.Instance.DisableTouch();
            GetCountryData(countryId, (countryId) =>
            {
                TouchManager.Instance.EnableTouch();
                callback?.Invoke();
            });
        }

        public void ClearCountryCachedData()
        {
            cachedTeamList.Clear();
            chapterInfoDic.Clear();
            passDataInfoDic.Clear();
            teamDataListDic.Clear();
        }

        private HashSet<int> cachedTeamList = new();

        /// <summary>
        /// 建立1个索引，在材料掉落的地方还用这个来取数据，不用遍历关卡
        /// id是关卡id
        /// </summary>
        private Dictionary<int, PassData> allPassData = new();           //

        /// <summary>
        /// 获取通过的最后一关
        /// </summary>
        public int GetLastPassedLevel()
        {
            int maxLevelIndex = -1;
            foreach (var item in allPassData.Values)
            {
                if (item.Stars[0] > 0)
                {
                    if (maxLevelIndex < item.Id)
                    {
                        maxLevelIndex = item.Id;
                    }
                }
            }
            return maxLevelIndex;
        }

        /// <summary>
        /// 获取总星数
        /// </summary>
        public int GetTotalStarCount()
        {
            int totalStatCount = 0;
            foreach (PassData passData in allPassData.Values)
            {
                foreach (int star in passData.Stars)
                {
                    if (star > 0)
                    {
                        totalStatCount++;
                    }
                }
            }
            return totalStatCount;
        }

        /// <summary>
        /// 登录推送建立所有passdata
        /// </summary>
        /// <param name="chapterList"></param>
        public void ResetPassData(Google.Protobuf.Collections.RepeatedField<Protocol.ChapterInfo> chapterList)
        {
            //notifyData = chapterList;
            BuildPassData(chapterList);
        }

        public void BuildPassData(Google.Protobuf.Collections.RepeatedField<Protocol.ChapterInfo> chapterList)
        {
            allPassData.Clear();
            foreach (var countryItem in chapterList)
            {
                bool hasConfig = Configs.ChallengeCountry.GetConfigList().Exists(p => p.Id == countryItem.Id);
                if (hasConfig)
                {
                    var passList = countryItem.PassData.ToList();
                    classicCountryLevelDataDic[countryItem.Id].PassAll = countryItem.PassAll;
                    classicCountryLevelDataDic[countryItem.Id].Rewards = countryItem.Rewards;
                    classicCountryLevelDataDic[countryItem.Id].Star = countryItem.Star;
                    classicCountryLevelDataDic[countryItem.Id].PassData = passList;
                    foreach (var passdata in passList)
                    {
                        allPassData.Add(passdata.Id, passdata);
                    }
                }
            }
        }

        /// <summary>
        /// 扫荡的时候更新挑战次数，服务端只回传了1个次数id
        /// </summary>
        /// <param name="countryId"></param>
        /// <param name="clubId"></param>
        /// <param name="challengeTimes"></param>
        public void UpdatePassData(int countryId, int clubId, int challengeTimes)
        {
            var passData = classicCountryLevelDataDic[countryId].PassData.FirstOrDefault(p => p.Id == clubId);
            if (passData != null)
            {
                passData.ChallengeTimes = challengeTimes;
                allPassData[passData.Id] = passData;
            }
        }

        public bool UpdatePassData(int countryId, PassData pData)
        {
            bool isFirstPass = false;
            var existPassData = classicCountryLevelDataDic[countryId].PassData.FirstOrDefault(p => p.Id == pData.Id);
            if (existPassData != null)
            {
                //星数有增加就全更新，没有增加就只更新挑战次数
                if (existPassData.Stars.Sum() < pData.Stars.Sum())
                {
                    for (var index = 0; index < 3; index++)
                    {
                        existPassData.Stars[index] = pData.Stars[index];
                    }
                    //追加小红点检测
                    //CheckOneCountryRedDot(classicCountryLevelDataDic[countryId]);
                }
                existPassData.ChallengeTimes = pData.ChallengeTimes;
                isFirstPass = false;
            }
            else
            {
                existPassData = pData;
                classicCountryLevelDataDic[countryId].PassData.Add(existPassData);
                Player.BattleManager.isFirstPass = true;
                isFirstPass = true;
            }
            allPassData[existPassData.Id] = existPassData;
            return isFirstPass;
        }

        /// <summary>
        /// 从推送来更新小红点
        /// </summary>
        public void CheckRedDot()
        {
            if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicPVE, false)) return;
            foreach (ClassicCountryLevelData chapterInfo in classicCountryLevelDataDic.Values)
            {
                CheckOneCountryRedDot(chapterInfo);
            }
        }

        /// <summary>
        /// 更新1个城池的小红点
        /// </summary>
        /// <param name="chapterInfo"></param>
        public void CheckOneCountryRedDot(ClassicCountryLevelData chapterInfo)
        {
            bool isRed = false;
            var config = chapterInfo.challengeCountryConfig;
            isRed |= (chapterInfo.Star >= config.Star3 && chapterInfo.Rewards[2] == 0); //第3个宝箱还没领
            isRed |= (chapterInfo.Star >= config.Star2 && chapterInfo.Rewards[1] == 0); //第2个宝箱还没领
            isRed |= (chapterInfo.Star >= config.Star1 && chapterInfo.Rewards[0] == 0); //第1个宝箱还没领

            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVE, "/" + config.Level.ToString() + "/" + config.Map.ToString() + "/" + chapterInfo.challengeCountryConfig.Id + "/box");
            node.AddValue(isRed ? 1 : -1);
        }

        #endregion

        #region 任务数据

        public Dictionary<int, ChapterInfo> chapterInfoDic = new();
        private void ProcessCountryTaskData(int countryId, GetChallengeDataResponse resp)
        {
            if (chapterInfoDic.ContainsKey(countryId) == false)
            {
                chapterInfoDic.Add(countryId, null);
            }
            chapterInfoDic[countryId] = resp.ChapterInfo;
        }

        #endregion

        #region 关卡数据
        public Dictionary<int, List<ChallengeClubConfig>> teamByCountryDic = new();
        private void InitTeamByCountryDic()
        {
            teamByCountryDic.Clear();
            passDataInfoDic.Clear();
            foreach (ChallengeClubConfig challengeClubConfig in Configs.ChallengeClub.GetConfigList())
            {
                if (teamByCountryDic.ContainsKey(challengeClubConfig.Country) == false)
                {
                    teamByCountryDic.Add(challengeClubConfig.Country, new());
                }
                teamByCountryDic[challengeClubConfig.Country].Add(challengeClubConfig);
            }
        }

        public Dictionary<int, PassData> passDataInfoDic = new();
        private void ProcessCountryTeamData(int countryId, GetChallengeDataResponse resp)
        {
            foreach (PassData passData in resp.ChapterInfo.PassData)
            {
                if (passDataInfoDic.ContainsKey(passData.Id) == false)
                {
                    passDataInfoDic.Add(passData.Id, null);
                }
                passDataInfoDic[passData.Id] = passData;
            }
        }

        public class ClassicTeamData
        {
            public ChallengeClubConfig challengeClubConfig;
            /// <summary>
            /// 奖励=> itemid,data
            /// </summary>
            public Dictionary<int, Utils.GameItem.GameItem> rewards;
            public PassData passData;
            public bool isOpen = false;
            public ClassicCountryLockReason countryLockReason = ClassicCountryLockReason.Unknow;
            public int countryUnlockLevel = 10;
        }

        public Dictionary<int, List<ClassicTeamData>> teamDataListDic = new();
        public List<ClassicTeamData> GetTeamDataList(int countryId)
        {
            if (teamDataListDic.ContainsKey(countryId))
            {
                return teamDataListDic[countryId];
            }

            List<ChallengeClubConfig> teamList = teamByCountryDic[countryId];
            List<ClassicTeamData> classicTeamDataList = new();
            teamDataListDic[countryId] = classicTeamDataList;
            foreach (ChallengeClubConfig challengeClubConfig in teamList)
            {

                ClassicTeamData classicTeamData = new();
                classicTeamData.challengeClubConfig = challengeClubConfig;
                if (passDataInfoDic.ContainsKey(challengeClubConfig.Id) == false)
                {
                    classicTeamData.passData = new();
                    classicTeamData.passData.Id = challengeClubConfig.Id;
                    classicTeamData.passData.Stars.Add(0);
                    classicTeamData.passData.Stars.Add(0);
                    classicTeamData.passData.Stars.Add(0);
                    classicTeamData.passData.ChallengeTimes = 0;
                }
                else
                {
                    classicTeamData.passData = passDataInfoDic[challengeClubConfig.Id];
                }

                classicTeamDataList.Add(classicTeamData);
            }

            ClassicCountryLevelData classicCountryLevelData = classicCountryLevelDataDic[countryId];
            for (int i = 0; i < classicTeamDataList.Count; i++)
            {
                ClassicTeamData classicTeamData = classicTeamDataList[i];
                classicTeamData.countryLockReason = classicCountryLevelData.lockReason;
                classicTeamData.countryUnlockLevel = classicCountryLevelData.challengeCountryConfig.UserLevel;
                if (classicTeamData.passData.Stars[0] > 0)
                {
                    classicTeamData.isOpen = true;
                    continue;
                }
                if (classicTeamData.countryLockReason != ClassicCountryLockReason.Unknow)
                {
                    classicTeamData.isOpen = false;
                }
                else
                {
                    if (i == 0)
                    {
                        if (classicCountryLevelData.isOpen) classicTeamData.isOpen = true;

                    }
                    else
                    {
                        if (classicTeamDataList[i - 1].passData.Stars[0] > 0) classicTeamData.isOpen = true;
                    }
                }
            }
            return classicTeamDataList;
        }

        #endregion

        #endregion

        //public 

        public class EquipRouteItemData
        {
            public int StarCount;
            public int chanllegeCount;
            public int itemCount;
            public int id;
        }

        /// <summary>
        /// 从材料角度分的关卡数据<itemid, List<关卡id>>，这一份包含所有关卡      
        /// </summary>
        public Dictionary<int, List<int>> ItemRouteDict;

        /// <summary>
        /// 建立 材料=>关卡list 的数据结构，包含所有关卡
        /// </summary>
        public void InitEquipRouteDict()
        {
            ItemRouteDict = new Dictionary<int, List<int>>();
            Configs.ChallengeClub.GetConfigList().ForEach(config =>
            {
                List<Utils.GameItem.GameItem> _itemList = GameItemUtils.CreateGameItems(config.Reward).ToList();
                _itemList.ForEach(_item =>
                {
                    GoodsConfig _cfg = Configs.Goods.GetConfig(_item.Id);
                    if (_cfg != null && _cfg.Type == 6)
                    {
                        if (ItemRouteDict.ContainsKey(_item.Id))
                        {
                            var _passDataList = ItemRouteDict[_item.Id];
                            if (!_passDataList.Exists(p => p == config.Id)) _passDataList.Add(config.Id);
                            ItemRouteDict[_item.Id] = _passDataList;
                        }
                        else
                        {
                            var _passDataList = new List<int>();
                            _passDataList.Add(config.Id);
                            ItemRouteDict[_item.Id] = _passDataList;
                        }
                    }
                });
            });
            //理论上，关卡id越大，掉落越多
            foreach (var list in ItemRouteDict.Values)
            {
                list.Sort((a, b) =>
                {
                    return -a.CompareTo(b);
                });
            }
        }

        /// <summary>
        /// 清空所有挑战次数
        /// </summary>
        public void ClearChallengeCount()
        {
            //零点清空挑战次数
            foreach (var pData in allPassData.Values)
            {
                pData.ChallengeTimes = 0;
            }
        }

        /// <summary>
        /// 根据材料ID返回所有关卡数据，并且按照指定材料掉落的多少排序
        /// </summary>
        /// <param name="itemid"></param>
        /// <returns></returns>
        public List<PassData> GetPassedDataByItemId(int itemid)
        {
            List<int> teamList = ItemRouteDict[itemid];
            List<PassData> _list = new List<PassData>();
            foreach (var teamId in teamList)
            {
                if (allPassData.ContainsKey(teamId))
                {
                    _list.Add(allPassData[teamId]);
                }
            }
            return _list;
        }

        public void FastChallenge(int clubid, int times, bool rewardsTip = true, Action<int> callback = null)
        {
            ChallengeClubConfig _config = Configs.ChallengeClub.GetConfig(clubid);

            int energyTimes = Player.PackageManager.Energy / GameConst.BattleEnergy;
            if (times > energyTimes) times = energyTimes;
            if (times == 0)
            {
                Player.PackageManager.AskBuyEnergy(null);
                return;
            }

            NetworkManager.Instance.ChallengeStartFast(clubid, times, (ChallengeStartFastResponse resp) =>
            {
                ClassicManager.Instance.UpdatePassData(_config.Country, _config.Id, resp.ChallengeTimes);
                if (rewardsTip)
                {
                    var rewardsList = GameItemUtils.CreateGameItems(_config.Reward).ToList();
                    rewardsList.ForEach(P => P.Count *= times);

                    var properties = new InventoryObtainedUIProperties(rewardsList);
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);// 打开通用收益界面
                }
                callback?.Invoke(resp.ChallengeTimes);
            });
        }
    }
}