using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using GameConfig;
using GameConfig.Config;
using Google.Protobuf.Collections;
using Protocol;
using UnityEngine;

namespace BigBang
{
    /// <summary>
    /// 剧情推图的管理类
    /// </summary>
    public class HeroManager : BabuSingleton<HeroManager>
    {
        private bool isInited = false;
        /// <summary>
        /// 配置表加载完成后，对数据进行预处理
        /// </summary>
        public void InitOnce(bool forceInit = true)
        {
            if (isInited && !forceInit) return;
            isInited = true;
            InitHeroChapterDataDic();
            InitHeroClubDataDic();
        }

        /// <summary>
        /// 初始化挑战数据
        /// </summary>
        /// <param name="chapterList"></param>
        public void ResetPassData(RepeatedField<ChapterInfo> chapterList)
        {
            foreach (var item in chapterList)
            {
                var hasconfig = Configs.ChallengeHeroChapter.GetConfig(item.Id);
                if (hasconfig != null)
                {
                    heroChapterDataDic[item.Id].PassAll = item.PassAll;
                    heroChapterDataDic[item.Id].Rewards = item.Rewards;
                    heroChapterDataDic[item.Id].Star = item.Star;
                    heroChapterDataDic[item.Id].PassData = item.PassData.ToList();
                }
            }
        }

        /// <summary>
        /// 零点清空关卡挑战次数数据
        /// </summary>
        public void ClearChallengeCount()
        {
            foreach (var _chapter in heroChapterDataDic.Values)
            {
                foreach (var pData in _chapter.PassData)
                {
                    pData.ChallengeTimes = 0;
                }
            }
        }

        public void UpdatePassData(int chapterId, ChallengeStartHeroResponse resp)
        {
            var existPassData = heroChapterDataDic[chapterId].PassData.FirstOrDefault(p => p.Id == resp.PassData.Id);
            if (existPassData != null)
            {
                //星数有增加就全更新，没有增加就只更新挑战次数
                if (existPassData.Stars.Sum() < resp.PassData.Stars.Sum())
                {
                    existPassData = resp.PassData;
                }
                else
                {
                    existPassData.ChallengeTimes = resp.PassData.ChallengeTimes;
                }
            }
            else
            {
                heroChapterDataDic[chapterId].PassData.Add(resp.PassData);
            }


            if (heroChapterDataDic[chapterId].PassAll == false && resp.PassData.Id == Configs.ChallengeHero.GetConfigList().FindAll(p => p.Chapter == chapterId).Last().Id)
            {
                heroChapterDataDic[chapterId].PassAll = true;
            }

        }

        public void CheckRedDot()
        {
            if (!TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.ClassicHero, false)) return;
            foreach (HeroChapterData chapterInfo in heroChapterDataDic.Values)
            {
                bool isRed = false;
                var config = chapterInfo.challengeHeroChapterConfig;
                isRed |= (chapterInfo.Star >= config.Star3 && chapterInfo.Rewards[2] == 0); //3星
                isRed |= (chapterInfo.Star >= config.Star2 && chapterInfo.Rewards[1] == 0); //2星
                isRed |= (chapterInfo.Star >= config.Star1 && chapterInfo.Rewards[0] == 0); //1星

                RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FBClassicHero, "/" + config.Hero.ToString() + "/" + chapterInfo.Star.ToString() + "/box");
                node.AddValue(isRed ? 1 : -1);
            }
        }


        #region 剧情章节

        #region 外部接口
        /// <summary>
        /// 从服务器获取剧情所需数据
        /// </summary>
        public void GetHeroChapterData(Action callback)
        {
            NetworkManager.Instance.GetChallengeHeroChapterData((GetChallengeHeroChapterDataResponse resp) =>
            {
                ProcessHeroChapterData(resp);
                callback?.Invoke();
            });
        }

        #endregion

        #region 数据处理

        public class HeroChapterData
        {
            public ChallengeHeroChapterConfig challengeHeroChapterConfig;
            public CardModelConfig cardModelConfig;
            public ChapterMapInfo chapterMapInfo;//可能为空
            public bool isFinish = false;//所有关卡通关//不需要--且三星奖励已经领完
            public bool isNeedShowInChapter = false;//应当在章节列表显示
            public bool IsOpen { get { return chapterMapInfo != null; } }
            public int IsOpenInt { get { return chapterMapInfo != null ? 1 : 0; } }

            public bool PassAll;
            public RepeatedField<int> Rewards = new RepeatedField<int>() { 0, 0, 0 };
            public int Star;
            public List<PassData> PassData = new();
        }
        public Dictionary<int, HeroChapterData> heroChapterDataDic = new();//所有章节的数据，key为章节id
        public Dictionary<int, List<HeroChapterData>> heroChapterDataStarListDic = new();//每个英雄的4个章节数据，key为heroId
        public List<HeroChapterData> heroChapterDataNeedShowList = new();//需要显示在剧情章节界面的章节数据
        private void InitHeroChapterDataDic()
        {
            heroChapterDataDic.Clear();
            heroChapterDataStarListDic.Clear();
            heroChapterDataNeedShowList.Clear();
            foreach (ChallengeHeroChapterConfig ChallengeHeroChapterConfig in Configs.ChallengeHeroChapter.GetConfigList())
            {
                HeroChapterData heroChapterData = new();
                heroChapterData.challengeHeroChapterConfig = ChallengeHeroChapterConfig;
                heroChapterData.cardModelConfig = Configs.CardModel.GetConfig(heroChapterData.challengeHeroChapterConfig.Hero);
                if (heroChapterData.cardModelConfig == null)
                {
                    Debug.LogWarningFormat("HeroManager , InitHeroChapterDataDic , heroChapterData.challengePlayerConfig == null , heroChapterData.challengeHeroChapterConfig.Hero = {0}", heroChapterData.challengeHeroChapterConfig.Hero);
                }
                heroChapterDataDic.Add(ChallengeHeroChapterConfig.Id, heroChapterData);

                if (heroChapterDataStarListDic.ContainsKey(ChallengeHeroChapterConfig.Hero) == false)
                {
                    heroChapterDataStarListDic.Add(ChallengeHeroChapterConfig.Hero, new());
                }
                heroChapterDataStarListDic[ChallengeHeroChapterConfig.Hero].Add(heroChapterData);
                //注册结构为 剧情关卡/英雄id/星级章节
                RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FBClassicHero, "/Normal/" + ChallengeHeroChapterConfig.Hero.ToString() + "/" + ChallengeHeroChapterConfig.Star.ToString());
            }
        }
        private void ProcessHeroChapterData(GetChallengeHeroChapterDataResponse resp)
        {
            heroChapterDataNeedShowList.Clear();

            foreach (ChallengeHeroChapterConfig challengeHeroChapterConfig in Configs.ChallengeHeroChapter.GetConfigList())
            {
                HeroChapterData heroChapterData = heroChapterDataDic[challengeHeroChapterConfig.Id];
                heroChapterData.chapterMapInfo = null;
                heroChapterData.isFinish = false;
                heroChapterData.isNeedShowInChapter = false;
            }
            foreach (ChapterMapInfo chapterMapInfo in resp.Chapters)
            {
                if (heroChapterDataDic.ContainsKey(chapterMapInfo.Id) == false)
                {
                    Debug.LogWarningFormat("HeroManager , ProcessHeroChapterData , heroChapterDataDic.ContainsKey(chapterMapInfo.Id) == false , chapterMapInfo.Id = {0}", chapterMapInfo.Id);
                    continue;
                }
                HeroChapterData heroChapterData = heroChapterDataDic[chapterMapInfo.Id];
                heroChapterData.chapterMapInfo = chapterMapInfo;
                bool isAllClubPass = heroChapterData.challengeHeroChapterConfig.Number <= heroChapterData.chapterMapInfo.Pass;
                bool isAllStarGet = heroChapterData.challengeHeroChapterConfig.Number * 3 <= heroChapterData.chapterMapInfo.Star;
                bool isAllGiftGet = heroChapterDataDic[chapterMapInfo.Id].Rewards.Sum() == 3;//TODO 需要服务器数据
                heroChapterData.isFinish = isAllClubPass && isAllStarGet && isAllGiftGet;
            }

            HashSet<int> playerSet = new();
            foreach (ChallengeHeroChapterConfig challengeHeroChapterConfig in Configs.ChallengeHeroChapter.GetConfigList())
            {
                HeroChapterData heroChapterData = heroChapterDataDic[challengeHeroChapterConfig.Id];
                bool isOpen = heroChapterData.chapterMapInfo != null;
                bool isFirstInChapter = heroChapterData.challengeHeroChapterConfig.Prev == 0;

                if (heroChapterData.isFinish) continue;

                if (!isFirstInChapter && !isOpen) continue;

                if (!isFirstInChapter && isOpen)
                {
                    bool isPreFinish = heroChapterDataDic[heroChapterData.challengeHeroChapterConfig.Prev].isFinish;
                    if (!isPreFinish) continue;
                }

                heroChapterData.isNeedShowInChapter = true;
                playerSet.Add(heroChapterData.challengeHeroChapterConfig.Hero);
                heroChapterDataNeedShowList.Add(heroChapterData);
            }
            foreach (ChallengeHeroChapterConfig challengeHeroChapterConfig in Configs.ChallengeHeroChapter.GetConfigList())
            {
                //if (challengeHeroChapterConfig.Star != 5) continue;
                if (playerSet.Contains(challengeHeroChapterConfig.Hero)) continue;
                HeroChapterData heroChapterData = heroChapterDataDic[challengeHeroChapterConfig.Id];

                heroChapterData.isNeedShowInChapter = true;
                playerSet.Add(heroChapterData.challengeHeroChapterConfig.Hero);
                heroChapterDataNeedShowList.Add(heroChapterData);
            }
            HashSet<int> cardIdSetMine = new();
            foreach (PlayerCard card in Player.CardManager.CardList)
            {
                cardIdSetMine.Add(card.CardId);
            }
            heroChapterDataNeedShowList = heroChapterDataNeedShowList
                .Where((heroChapterData) =>
                {
                    return heroChapterData.cardModelConfig.IsRetire == 0 || cardIdSetMine.Contains(heroChapterData.cardModelConfig.Id);
                })
                .OrderByDescending(item => item.IsOpenInt)
                .ThenBy(item => item.challengeHeroChapterConfig.Id)
                .ToList();
        }
        #endregion

        #endregion


        #region 关卡列表

        #region 外部接口

        /// <summary>
        /// 从服务器获取剧情所需数据
        /// </summary>
        public void GetHeroClubData(int chapterId, Action callback)
        {
            NetworkManager.Instance.GetChallengeData(chapterId, (GetChallengeDataResponse resp) =>
            {
                ProcessClubTaskData(chapterId, resp);
                ProcessClubClubData(chapterId, resp);
                callback?.Invoke();
            });
        }

        //public Dictionary<int, List<HeroClubData>> clubDataListDic = new();//某个章节里面的所有关卡信息，key为章节id
        public List<HeroClubData> GetClubDataList(int countryId)
        {
            //if (clubDataListDic.ContainsKey(countryId))
            //{
            //    return clubDataListDic[countryId];
            //}

            List<ChallengeHeroConfig> clubList = heroClubCfgByChapterListDic[countryId];
            List<HeroClubData> heroClubDataList = new();
            //clubDataListDic[countryId] = heroClubDataList;
            HeroClubData selectHeroClubData = null;
            for (int i = 0; i < clubList.Count; i++)
            {
                ChallengeHeroConfig challengeHeroConfig = clubList[i];
                HeroClubData heroClubData = new();
                heroClubData.challengeHeroConfig = challengeHeroConfig;
                heroClubData.index = i;
                if (passDataInfoDic.ContainsKey(challengeHeroConfig.Id) == false)
                {
                    heroClubData.passData = new();
                    heroClubData.passData.Id = challengeHeroConfig.Id;
                    heroClubData.passData.Stars.Add(0);
                    heroClubData.passData.Stars.Add(0);
                    heroClubData.passData.Stars.Add(0);
                    heroClubData.passData.ChallengeTimes = 0;
                }
                else
                {
                    heroClubData.passData = passDataInfoDic[challengeHeroConfig.Id];
                }

                heroClubData.isOpen = false;
                if (i == 0)
                {
                    heroClubData.isOpen = true;
                }
                else
                {
                    if (heroClubDataList[i - 1].passData.Stars[0] >= 1)
                    {
                        heroClubData.isOpen = true;
                    }
                }
                if (heroClubData.isOpen) selectHeroClubData = heroClubData;

                heroClubDataList.Add(heroClubData);
            }
            if (selectHeroClubData != null) selectHeroClubData.isSelect = true;

            return heroClubDataList;
        }

        #endregion

        #region 数据定义与预处理
        public class HeroClubData
        {
            public ChallengeHeroConfig challengeHeroConfig;
            public int index = 0;
            public PassData passData;
            public bool isOpen = false;
            public bool isSelect = false;
        }

        public Dictionary<int, List<ChallengeHeroConfig>> heroClubCfgByChapterListDic = new();//某个章节里面的所有关卡配置，key为章节id
        private void InitHeroClubDataDic()
        {
            heroClubCfgByChapterListDic.Clear();
            foreach (ChallengeHeroConfig challengeHeroConfig in Configs.ChallengeHero.GetConfigList())
            {
                if (heroClubCfgByChapterListDic.ContainsKey(challengeHeroConfig.Chapter) == false)
                {
                    heroClubCfgByChapterListDic.Add(challengeHeroConfig.Chapter, new());
                }
                heroClubCfgByChapterListDic[challengeHeroConfig.Chapter].Add(challengeHeroConfig);
            }
        }

        #endregion

        #region 任务数据

        public Dictionary<int, ChapterInfo> chapterInfoDic = new();//缓存服务器发来的所有章节详细数据
        private void ProcessClubTaskData(int countryId, GetChallengeDataResponse resp)
        {
            if (chapterInfoDic.ContainsKey(countryId) == false)
            {
                chapterInfoDic.Add(countryId, null);
            }
            chapterInfoDic[countryId] = resp.ChapterInfo;
        }

        #endregion

        #region 关卡数据
        public Dictionary<int, PassData> passDataInfoDic = new();//缓存服务器发来的所有关卡详细数据
        private void ProcessClubClubData(int countryId, GetChallengeDataResponse resp)
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
        #endregion

        #endregion

    }
}