using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using Protocol;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static BigBang.HeroManager;

namespace BigBang.UI
{

    [Serializable]
    public class HeroClubUIProperties : PanelProperties
    {
        public int chapterId;
        public int heroId;

        public HeroClubUIProperties(int chapterId, int heroId)
        {
            this.chapterId = chapterId;
            this.heroId = heroId;
        }
    }

    public class HeroClubUI : APanelController<HeroClubUIProperties>
    {

        #region 初始化
        protected override void Awake()
        {
            base.Awake();
        }
        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
        }

        [SerializeField] private Button closeBtn;
        private void OnClose()
        {
            anim.PlayExit(() => UIController.Instance.HidePanel<HeroClubUI>());
        }

        [SerializeField] private HeroClubItemAdapter heroClubItemAdapter;
        [SerializeField] private HeroClubUIAnim anim;
        [SerializeField] List<Image> reddotList;
        private List<HeroChapterData> heroChapterDataList;
        HeroChapterData heroChapterDataNow;
        private int chapterId = 0;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            heroChapterDataList = HeroManager.Instance.heroChapterDataStarListDic[Properties.heroId];
            heroChapterDataNow = heroChapterDataList[0];

            chapterId = Properties.chapterId;

            SetLevelTabCallBack();
            RefreshLevelTabActive();
            RefreshLevelTabLight();
            anim.SetDataSuccess(false);
            UpdatePlayerInfo();
            UpdateTopHeroInfo();
            RefreshPassCount();
            SetStrength();
            anim.PlayEnter();

            heroClubItemAdapter.SetData(new());
            RefreshClubData();
        }
        #endregion

        #region 刷新关卡数据

        private void RefreshClubData()
        {
            HeroManager.Instance.GetHeroClubData(chapterId, () =>
            {
                int passCount = 0;
                foreach (HeroClubData heroClubData in HeroManager.Instance.GetClubDataList(chapterId))
                {
                    if(heroClubData.passData.Stars[0] > 0)
                    {
                        passCount++;
                    }
                }
                heroChapterDataNow.chapterMapInfo.Pass = passCount;
                RefreshPassCount();

                heroClubItemAdapter.SetData(HeroManager.Instance.GetClubDataList(chapterId));
                heroClubItemAdapter.InitAnim();
                ChapterInfo chapterInfo = HeroManager.Instance.chapterInfoDic[chapterId];
                ChallengeHeroChapterConfig challengeHeroChapterConfig = Configs.ChallengeHeroChapter.GetConfig(chapterInfo.Id);
                anim.SetDataSuccess(true, chapterInfo.Star, challengeHeroChapterConfig.Star3);
                SetProgress();

                if (Player.BattleManager.lastChallengeHeroConfig != null && Player.BattleManager.lastChallengeHeroConfig.Chapter == chapterId)
                {
                    int scrollIndex = Player.BattleManager.lastChallengeHeroConfig.Id % 100 - 1;
                    heroClubItemAdapter.ScrollTo(scrollIndex);
                    Player.BattleManager.lastChallengeHeroConfig = null;
                }

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
            HeroChapterData heroChapterData = heroChapterDataList[level - 1];
            if (!heroChapterData.IsOpen)
            {
                Tips.PopTips("需要{0}达到{1}星后解锁".SafeFormat(PlayerCard.GetFullName(heroChapterData.cardModelConfig), heroChapterData.challengeHeroChapterConfig.Star));
                return;
            }
            heroChapterDataNow = heroChapterData;
            chapterId = heroChapterData.challengeHeroChapterConfig.Id;
            RefreshClubData();
            RefreshLevelTabLight();
        }
        private void RefreshLevelTabActive()
        {
            for (int i = 0; i < 4; i++)
            {
                ClassicMapLevelTabItem classicMapLevelTabItem = LevelTabItemList[i];
                classicMapLevelTabItem.level = i + 1;
                HeroChapterData heroChapterData = null;
                if (heroChapterDataList.Count > i) heroChapterData = heroChapterDataList[i];
                bool isFirst = i == 0;
                if (isFirst)
                {
                    classicMapLevelTabItem.gameObject.SetActive(true);
                }
                else
                {
                    if (heroChapterData == null)
                    {
                        classicMapLevelTabItem.gameObject.SetActive(false);
                    }
                    else
                    {
                        HeroChapterData heroChapterDataPrev = heroChapterDataList[i - 1];
                        bool isPrevPass = heroChapterDataPrev.isFinish;
                        classicMapLevelTabItem.gameObject.SetActive(isPrevPass);
                    }
                }
                if (heroChapterData != null)
                {
                    RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FBClassicHero, "/" + heroChapterData.cardModelConfig.Id.ToString() + "/" + heroChapterData.challengeHeroChapterConfig.Star.ToString());
                    node.IsRed(reddotList[i].transform);
                }
            }
        }
        private void RefreshLevelTabLight()
        {
            for (int i = 0; i < 4; i++)
            {
                if (i < heroChapterDataList.Count)
                {
                    HeroChapterData heroChapterData = heroChapterDataList[i];
                    ClassicMapLevelTabItem classicMapLevelTabItem = LevelTabItemList[i];
                    classicMapLevelTabItem.SetLight(heroChapterData.challengeHeroChapterConfig.Id == chapterId);
                }
            }
        }

        #endregion

        #region 个人信息

        [SerializeField] private TMP_Text clubNameText;//玩家俱乐部名
        [SerializeField] private ClubIconItem clubIcon;//玩家俱乐部图标
        public void UpdatePlayerInfo()
        {
            clubNameText.text = Player.Name;
            clubIcon.SetIcon(Player.Icon);
        }

        [SerializeField] private TMP_Text clubScoreText;//当前玩家战力
        private void SetStrength()
        {
            clubScoreText.text = Player.Strength.ToString();
        }

        #endregion

        #region 刷新宝箱进度条
        [SerializeField] private ClassicTaskProgressItem classicTaskProgressItem;
        private void SetProgress()
        {
            ChapterInfo chapterInfo = HeroManager.Instance.chapterInfoDic[chapterId];
            ChallengeHeroChapterConfig challengeHeroChapterConfig = Configs.ChallengeHeroChapter.GetConfig(chapterInfo.Id);

            classicTaskProgressItem.RedDotPath = PanelNodePath.Home_FBClassicHero + "/" + Properties.heroId.ToString() + "/" + heroChapterDataNow.challengeHeroChapterConfig.Star.ToString() + "/box";
            classicTaskProgressItem.SetHeroData(chapterInfo.Id, chapterInfo.Star, challengeHeroChapterConfig.Star3, chapterInfo.Rewards, false);
        }
        #endregion

        #region 顶部关卡信息

        [SerializeField] private Image heroImage;
        [SerializeField] private TMP_Text heroName;
        [SerializeField] private TMP_Text levelProgress;
        private async void UpdateTopHeroInfo()
        {
            heroImage.sprite = await SpriteProxy.GetHeroIcon(heroChapterDataNow.challengeHeroChapterConfig.Hero.ToString());
            heroName.text = PlayerCard.GetFullName(heroChapterDataNow.cardModelConfig);
        }
        private void RefreshPassCount()
        {
            levelProgress.text = "<color=#13b237>{0}</color>/{1}".SafeFormat(heroChapterDataNow.chapterMapInfo.Pass, heroChapterDataNow.challengeHeroChapterConfig.Number);
        }

        #endregion

    }
}