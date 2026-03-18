using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using Protocol;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static BigBang.ClassicManager;

namespace BigBang.UI
{
    [Serializable]
    public class ClassicCountryUIProperties : PanelProperties
    {
        public int countryId;

        public int scrollIndex = -1;
        /// <summary>
        /// 从装备指引跳转过来要定位的icon
        /// </summary>
        public int lookItemId;
        public ClassicCountryUIProperties(int countryId, int _scrollIndex = -1, int _itemid = 0)
        {
            this.countryId = countryId;
            scrollIndex = _scrollIndex;
            lookItemId = _itemid;
            ClassicManager.Instance.NeedShowClassicCountryUI = true;
        }
    }

    public class ClassicCountryUI : APanelController<ClassicCountryUIProperties>
    {
        [SerializeField] private Button closeBtn;


        protected override void Awake()
        {
            base.Awake();
        }

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            EventManager.Instance.Register(EventID.ClassicCountryUIOnClickCountryButton, ClassicCountryUIOnClickCountryButton);
            EventManager.Instance.Register(EventID.ClassicCountryUIOnClickClallengeButton, ClassicCountryUIOnClickClallengeButton);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            EventManager.Instance.Unregister(EventID.ClassicCountryUIOnClickCountryButton, ClassicCountryUIOnClickCountryButton);
            EventManager.Instance.Unregister(EventID.ClassicCountryUIOnClickClallengeButton, ClassicCountryUIOnClickClallengeButton);
        }

        private void OnClose()
        {
            TouchManager.Instance.DisableTouch();
            anim.PlayExit(() =>
            {
                TouchManager.Instance.EnableTouch();
                UIController.Instance.HidePanel<ClassicCountryUI>();
            });
            //closeBtn.GetComponent<ButtonAnim>().PlayBack(() =>
            //{

            //});
        }

        private int mapId = 0;
        private int level = 0;
        private int countryId = 0;
        [SerializeField] private ClassicCountryUIAnim anim;
        [SerializeField] private ClassicCountryUIGuide classicCountryUIGuide;
        protected override void OnPropertiesSet()
        {
            //Debug.LogWarning("ClassicCountryUI");
            if (ClassicManager.Instance.NeedShowClassicCountryUI == false) return;
            ClassicManager.Instance.NeedShowClassicCountryUI = false;

            base.OnPropertiesSet();
            ChallengeCountryConfig challengeCountryConfig = Configs.ChallengeCountry.GetConfig(Properties.countryId);
            mapId = challengeCountryConfig.Map;
            level = challengeCountryConfig.Level;
            countryId = Properties.countryId;

            SetLevelTabCallBack();
            RefreshLevelTab();
            SetProgress();
            SetCountryList();
            classicCountryItemAdapter.ScrollToSelect();
            SetTeamList();
            SelectLastOpenTeam();

            ChapterInfo chapterInfo = ClassicManager.Instance.chapterInfoDic[countryId];
            anim.PlayEnter(chapterInfo.Star, challengeCountryConfig.Star3);
            classicCountryUIGuide.CheckGuide();
        }

        #region 刷新进度条
        [SerializeField] private ClassicTaskProgressItem classicTaskProgressItem;
        private void SetProgress()
        {
            ChapterInfo chapterInfo = ClassicManager.Instance.chapterInfoDic[countryId];
            ChallengeCountryConfig challengeCountryConfig = Configs.ChallengeCountry.GetConfig(chapterInfo.Id);
            //这里不要调位置，要先赋值node路径
            classicTaskProgressItem.RedDotPath = PanelNodePath.Home_ClassicPVE + "/" + level.ToString() + "/" + mapId.ToString() + "/" + countryId.ToString() + "/box";
            classicTaskProgressItem.SetClassicData(chapterInfo.Id, chapterInfo.Star, challengeCountryConfig.Star3, chapterInfo.Rewards, false);
        }
        #endregion

        #region 刷新球队列表
        [SerializeField] private ClassicTeamItemAdapter classicTeamItemAdapter;
        private List<ClassicTeamData> classicTeamDataList = new();
        private void SetTeamList()
        {
            classicTeamDataList = ClassicManager.Instance.GetTeamDataList(countryId);
            classicTeamItemAdapter.SetData(classicTeamDataList, Properties.lookItemId);
            Player.BattleManager.classicCountryLevelData = ClassicManager.Instance.classicCountryLevelDataDic[countryId];
            Properties.countryId = countryId;
        }
        private void SelectLastOpenTeam()
        {
            if (classicTeamDataList.Count < 0) return;
            try
            {
                int lastIndex = 0;
                if (Properties.scrollIndex == -1)
                {
                    for (int i = classicTeamDataList.Count - 1; i >= 0; i--)
                    {
                        if (classicTeamDataList[i].isOpen)
                        {
                            lastIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    lastIndex = Properties.scrollIndex;
                }

                if (Player.BattleManager.classicTeamData != null)
                {
                    bool isCountrySame = classicTeamDataList[^1].challengeClubConfig.Country == Player.BattleManager.classicTeamData.challengeClubConfig.Country;
                    bool isNotLastOne = classicTeamDataList.Count <= lastIndex || classicTeamDataList[lastIndex].challengeClubConfig.Id != Player.BattleManager.classicTeamData.challengeClubConfig.Id;
                    bool isNotLastTwo = classicTeamDataList.Count <= lastIndex - 1 || classicTeamDataList[lastIndex - 1].challengeClubConfig.Id != Player.BattleManager.classicTeamData.challengeClubConfig.Id;
                    if (isCountrySame && isNotLastOne && isNotLastTwo)
                    {
                        classicTeamItemAdapter.SetNormalizedPosition(Player.BattleManager.classicTeamItemAdapterNormalizedPosition);
                        Properties.scrollIndex = -1;
                        return;
                    }
                }

                classicTeamItemAdapter.ScrollTo(lastIndex);
                Properties.scrollIndex = -1;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void ClassicCountryUIOnClickClallengeButton(object[] args)
        {
            Player.BattleManager.classicTeamItemAdapterNormalizedPosition = classicTeamItemAdapter.GetNormalizedPosition();
        }

        #endregion

        #region 国家选择

        [SerializeField] private ClassicCountryItemAdapter classicCountryItemAdapter;
        private void SetCountryList()
        {
            List<ClassicCountryLevelData> classicCountryLevelDataList = ClassicManager.Instance.classicCountryLevelDataListDic[level];
            List<ClassicCountryLevelData> classicCountryLevelDataListSelectByMap = new();
            foreach (ClassicCountryLevelData classicCountryLevelData in classicCountryLevelDataList)
            {
                if (classicCountryLevelData.challengeCountryConfig.Map != mapId) continue;
                if (!classicCountryLevelData.isOpen) continue;
                classicCountryLevelData.isSelect = classicCountryLevelData.challengeCountryConfig.Id == countryId;
                classicCountryLevelDataListSelectByMap.Add(classicCountryLevelData);
            }
            classicCountryItemAdapter.SetData(classicCountryLevelDataListSelectByMap);
        }

        private void ClassicCountryUIOnClickCountryButton(object[] args)
        {
            Player.BattleManager.classicTeamData = null;
            ClassicCountryLevelData data = args[0] as ClassicCountryLevelData;
            this.countryId = data.challengeCountryConfig.Id;
            ClassicManager.Instance.ChangeClassicCountryCountry(countryId, () =>
            {
                SetProgress();
                SetTeamList();
                SelectLastOpenTeam();
                classicTeamItemAdapter.InitAnim();
                classicTeamItemAdapter.PlayAnim();
                ChapterInfo chapterInfo = ClassicManager.Instance.chapterInfoDic[countryId];
                ChallengeCountryConfig challengeCountryConfig = Configs.ChallengeCountry.GetConfig(chapterInfo.Id);
                classicTaskProgressItem.Anim.PlayAnim(chapterInfo.Star, challengeCountryConfig.Star3, 0.0f);
            });
        }

        #endregion

        #region 难度选择

        [SerializeField] List<ClassicMapLevelTabItem> LevelTabItemList = new();
        private void SetLevelTabCallBack()
        {
            foreach (var item in LevelTabItemList)
            {
                item.SetCallBack(OnClickLevelTab);
            }
        }
        private void OnClickLevelTab(int level)
        {
            int newLevelNum = level;
            int newMapNum = countryId / 100 % 100;
            int newCountryNum = countryId % 100;
            int newCountryId = newLevelNum * 10000 + newMapNum * 100 + newCountryNum;
            this.mapId = newCountryId / 100;
            this.countryId = newCountryId;
            this.level = level;
            RefreshLevelTab();
            ClassicManager.Instance.ChangeClassicCountryCountry(countryId, () =>
            {
                SetProgress();
                SetCountryList();
                classicCountryItemAdapter.SetSelect(countryId);
                classicCountryItemAdapter.ScrollToSelect();
                SetTeamList();
                SelectLastOpenTeam();
                classicTeamItemAdapter.InitAnim();
                classicTeamItemAdapter.PlayAnim();
                ChapterInfo chapterInfo = ClassicManager.Instance.chapterInfoDic[countryId];
                ChallengeCountryConfig challengeCountryConfig = Configs.ChallengeCountry.GetConfig(chapterInfo.Id);
                classicTaskProgressItem.Anim.PlayAnim(chapterInfo.Star, challengeCountryConfig.Star3, 0.0f);
            });
        }
        [SerializeField] private RectTransform teamPanel = null;
        private void RefreshLevelTab()
        {
            foreach (var item in LevelTabItemList)
            {
                item.SetLight(this.level == item.level);
            }
            bool isLevelTabShow = ClassicManager.Instance.classicCountryLevelDataListDic[2][0].isOpen;

            teamPanel.SetTop(isLevelTabShow ? 309.549f : 256.5691f);
            //设置简单困难菜单可见性
            //var p = ClassicManager.Instance.classicCountryLevelDataListDic;
            LevelTabItemList[1].transform.parent.gameObject.SetActive(isLevelTabShow);
            //LevelTabItemList[2].gameObject.SetActive(ClassicManager.Instance.classicCountryLevelDataListDic[this.level][3].isOpen);
            LevelTabItemList[2].gameObject.SetActive(ClassicManager.Instance.classicCountryLevelDataListDic[3][0].isOpen);
        }

        #endregion

    }
}