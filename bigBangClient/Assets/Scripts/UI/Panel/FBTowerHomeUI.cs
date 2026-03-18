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
using TMPro;

namespace BigBang.UI
{
    [Serializable]
    // public class FBTowerHomeUIProperties : PanelProperties
    // {
    //     public int countryId;

    //     public int scrollIndex = -1;
    //     /// <summary>
    //     /// 从装备指引跳转过来要定位的icon
    //     /// </summary>
    //     public int lookItemId;
    //     public FBTowerHomeUIProperties(int countryId, int _scrollIndex = -1, int _itemid = 0)
    //     {
    //         this.countryId = countryId;
    //         scrollIndex = _scrollIndex;
    //         lookItemId = _itemid;
    //         ClassicManager.Instance.NeedShowClassicCountryUI = true;
    //     }
    // }

    public class FBTowerHomeUI : APanelController//<FBTowerHomeUIProperties>
    {
        #region 初始化
        [SerializeField] private Button closeBtn;
        [SerializeField] private BabuButton leftPageLightButton = null;
        [SerializeField] private BabuButton leftPageDarkButton = null;
        [SerializeField] private BabuButton rightPageLightButton = null;
        [SerializeField] private BabuButton rightPageDarkButton = null;
        [SerializeField] private TMP_Text chapterNameText = null;
        [SerializeField] private BabuButton buffButton = null;
        [SerializeField] private BabuButton formationButton = null;
        [SerializeField] private BabuButton raidButton = null;
        [SerializeField] private TMP_Text resetTimesText = null;
        [SerializeField] private BabuButton resetButton = null;
        [SerializeField] private BabuButton starRewardButton = null;
        [SerializeField] private TMP_Text starRewardCountText = null;
        [SerializeField] private Image starRewardDotNodeImg = null;
        [SerializeField] private BabuButton shopButton = null;
        [SerializeField] private List<FBTowerPathLineItem> lineItemList = new();
        [SerializeField] private List<FBTowerLevelItem> levelItemList = new();
        [SerializeField] private TMP_Text loseTimesText = null;
        [SerializeField] private BabuButton fightButton = null;
        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            EventManager.Instance.Register(EventID.OnClickFBTowerLevelItem, OnClickFBTowerLevelItem);
            buffButton.OnClick += OnClickBuffButton;
            shopButton.OnClick += OnClickShopButton;
            starRewardButton.OnClick += OnClickStarRewardButton;
            formationButton.OnClick += OnClickFormationButton;
            raidButton.OnClick += OnClickRaidButton;
            resetButton.OnClick += OnClickResetButton;
            leftPageLightButton.OnClick += OnClickLeftPageLightButton;
            rightPageLightButton.OnClick += OnClickRightPageLightButton;
            fightButton.OnClick += OnClickFightButton;
            EventManager.Instance.Register(EventID.AfterGetFBTowerBuff, AfterGetFBTowerBuff);
            EventManager.Instance.Register(EventID.AfterGetFBTowerData, AfterGetFBTowerData);
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
            EventManager.Instance.Register(EventID.OnTowerRaid, DoRaid);
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            EventManager.Instance.Unregister(EventID.OnClickFBTowerLevelItem, OnClickFBTowerLevelItem);
            buffButton.OnClick -= OnClickBuffButton;
            shopButton.OnClick -= OnClickShopButton;
            starRewardButton.OnClick -= OnClickStarRewardButton;
            formationButton.OnClick -= OnClickFormationButton;
            raidButton.OnClick -= OnClickRaidButton;
            resetButton.OnClick -= OnClickResetButton;
            leftPageLightButton.OnClick -= OnClickLeftPageLightButton;
            rightPageLightButton.OnClick -= OnClickRightPageLightButton;
            fightButton.OnClick -= OnClickFightButton;
            EventManager.Instance.Unregister(EventID.AfterGetFBTowerBuff, AfterGetFBTowerBuff);
            EventManager.Instance.Unregister(EventID.AfterGetFBTowerData, AfterGetFBTowerData);
            EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
            EventManager.Instance.Unregister(EventID.OnTowerRaid, DoRaid);
        }

        [SerializeField] private FBTowerHomeUIAnim anim;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            nowShowChapterConfig = FBTowerController.Instance.FBData.currentChapterConfig;
            anim.CurrentChapterId = nowShowChapterConfig.Id;
            anim.PlayEnter();
            RefreshShow();
            UnityEngine.PlayerPrefs.SetString(PlayerPrefsKeys.FBTowerHomeDailyRedDot + Player.GbId, DataConvUtil.ServerDateTime.ToStringUseFormat3());
            FBTowerController.Instance.CheckRedDot();
        }
        #endregion

        #region 按钮回调
        private void OnClose()
        {
            TouchManager.Instance.DisableTouch();
            anim.PlayExit(() =>
            {
                TouchManager.Instance.EnableTouch();
                UIController.Instance.HidePanel<FBTowerHomeUI>();
            });
        }
        private void OnClickFBTowerLevelItem(object[] args)
        {
            FBTowerLevelItem item = args[0] as FBTowerLevelItem;
            EnterLevel(item.towerLevelData);
        }
        private void OnClickFightButton(BabuButton _)
        {
            TowerLevelData towerLevelData = null;
            towerLevelData = FBTowerController.Instance.GetTowerLevelData(FBTowerController.Instance.FBData.currentLevelConfig);
            EnterLevel(towerLevelData);
        }
        private void EnterLevel(TowerLevelData towerLevelData)
        {
            if (towerLevelData.isBuff)
            {
                UIController.Instance.OpenWindow<FBTowerSelectBuffUI>(new FBTowerSelectBuffUIProperties(towerLevelData));
                return;
            }
            if (towerLevelData.towerConfig.Lv > Player.Level)
            {
                Tips.PopTips(towerLevelData.towerConfig.Lv + "级可挑战");
                return;
            }
            if (FBTowerController.Instance.FBData.failCount >= FBTowerController.MaxDailyFailCount)
            {
                Tips.PopTips("失败次数已达上限({0}/{1})".SafeFormat(FBTowerController.Instance.FBData.failCount, FBTowerController.MaxDailyFailCount));
                return;
            }
            UIController.Instance.OpenWindow<ClassicEnterFightUI>(new ClassicEnterFightUIProperties(towerLevelData));
        }
        private void OnClickBuffButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<FBTowerBuffUI>();
        }
        private void OnClickShopButton(BabuButton _)
        {
            UIController.Instance.ShowPanel<FBTowerShopUI>();
        }
        private void OnClickStarRewardButton(BabuButton _)
        {
            UIController.Instance.ShowPanel<FBRewardsUI>();
        }
        private void OnClickFormationButton(BabuButton _)
        {
            Player.FightManager.FormationController.GetAndCheckDefaultFormation(FormationID.TOWER, formation =>
            {
                UIController.Instance.ShowPanel<FormationUI>(new FormationProperties(formation, false, FormationUI.FormationShowType.Formation, FormationID.TOWER));
            });
        }
        private void OnClickRaidButton(BabuButton _)
        {
            DoRaid(null);
        }
        private void DoRaid(object[] _)
        {
            FBTowerController.Instance.BatchBattle((List<TowerLevelData> towerLevelDataList) =>
            {
                nowShowChapterConfig = FBTowerController.Instance.FBData.currentChapterConfig;
                anim.CurrentChapterId = nowShowChapterConfig.Id;
                RefreshShow();
                UIController.Instance.OpenWindow<FBTowerRaidResultUI>(new FBTowerRaidResultUIProperties(towerLevelDataList));
            });
        }
        private void OnClickResetButton(BabuButton _)
        {
            FBTowerController.Instance.ResetBattle(() =>
            {
                nowShowChapterConfig = FBTowerController.Instance.FBData.currentChapterConfig;
                anim.CurrentChapterId = nowShowChapterConfig.Id;
                RefreshShow();
            });
        }
        private void OnClickLeftPageLightButton(BabuButton _)
        {
            bool isCanLeft = nowShowChapterConfig.Id > Configs.TowerChapter.GetConfigList()[0].Id;
            if (!isCanLeft) return;
            nowShowChapterConfig = Configs.TowerChapter.GetConfig(nowShowChapterConfig.Id - 1);
            anim.CurrentChapterId = nowShowChapterConfig.Id;
            RefreshChapter();
        }
        private void OnClickRightPageLightButton(BabuButton _)
        {
            bool isCanRight = nowShowChapterConfig.Id < FBTowerController.Instance.FBData.currentChapterConfig.Id && nowShowChapterConfig.Id < Configs.TowerChapter.GetConfigList()[^1].Id;
            if (!isCanRight) return;
            nowShowChapterConfig = Configs.TowerChapter.GetConfig(nowShowChapterConfig.Id + 1);
            anim.CurrentChapterId = nowShowChapterConfig.Id;
            RefreshChapter();
        }
        private void AfterGetFBTowerBuff(object[] objs)
        {
            FBTowerSelectBuffItem buffItem = objs[0] as FBTowerSelectBuffItem;
            string info = "[{0}] {1} +{2}%".SafeFormat(Configs.SeparatedPosition.GetConfig(buffItem.buffPos).Name,
                Configs.CardAbility.GetConfig(buffItem.buffType).Name,
                buffItem.buffValue
                );
            Tips.PopTips(info);



            //加个buff飞入动画？
            if (FBTowerController.Instance.FBData.currentChapterConfig == nowShowChapterConfig)
            {
                RefreshChapter(false);
            }
            else
            {
                nowShowChapterConfig = FBTowerController.Instance.FBData.currentChapterConfig;
                RefreshChapter();
            }
            EventManager.Instance.Dispatch(EventID.OnResourceChange);
        }
        private void AfterGetFBTowerData(object[] objs)
        {
            nowShowChapterConfig = FBTowerController.Instance.FBData.currentChapterConfig;
            RefreshShow();
            //加个动画？

        }
        #endregion

        #region 刷新显示
        private void RefreshShow()
        {
            RefreshResetTimes();
            RefreshRaidBtnState();
            RefreshStarRewardBtnState();
            RefreshLoseTimes();

            RefreshChapter();

            RefreshRedDot();

            EventManager.Instance.Dispatch(EventID.OnResourceChange);
        }
        private void RefreshRedDot(object[] args = null)
        {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FBTower, "/star");
            node.IsRed(starRewardDotNodeImg.transform);
        }

        private void RefreshResetTimes()
        {
            int leftResetCount = FBTowerController.Instance.LeftResetCount;
            resetTimesText.text = "今日可重置{0}次".SafeFormat(leftResetCount);
        }
        private void RefreshLoseTimes()
        {
            if (FBTowerController.Instance.FBData.failCount < FBTowerController.MaxDailyFailCount)
            {
                loseTimesText.text = "失败次数<color=#fddc31>{0}</color>/{1}".SafeFormat(FBTowerController.Instance.FBData.failCount, FBTowerController.MaxDailyFailCount);
            }
            else
            {
                loseTimesText.text = "失败次数<color=#ff3737>{0}</color>/{0}".SafeFormat(FBTowerController.MaxDailyFailCount);
            }
        }
        private void RefreshRaidBtnState()
        {
            bool isCanRaid = FBTowerController.Instance.IsCanRaid;
            raidButton.gameObject.SetActive(isCanRaid);
            fightButton.gameObject.SetActive(!isCanRaid && !FBTowerController.Instance.FBData.isAllPass);
            loseTimesText.gameObject.SetActive(!isCanRaid && !FBTowerController.Instance.FBData.isAllPass);
        }
        private void RefreshStarRewardBtnState()
        {
            starRewardButton.gameObject.SetActive(!FBTowerController.Instance.FBData.isAllRewardGet);
            if (FBTowerController.Instance.FBData.isAllRewardGet == false)
            {
                int nowStartcount = FBTowerController.Instance.FBData.totalStar;
                int needStarCount = FBTowerController.Instance.FBData.currentTowerStarRewardConfig.Number;
                starRewardCountText.text = "<color=#fddc31>{0}</color>/{1}".SafeFormat(nowStartcount, needStarCount);
            }
        }

        private TowerChapterConfig nowShowChapterConfig = null;
        private List<TowerLevelData> nowTowerLevelDataList = new();//这一章所有关卡的数据

        /// <summary>
        /// 
        /// </summary>
        /// <param name="useAnim"></param>
        /// <param name="onlyNextNodeAni">是否只针对下一个节点播放动画</param>
        private void RefreshChapter(bool useAnim = true)
        {
            RefreshChapterData();
            RefreshChapterBtnState();
            RefreshChapterName();
            RefreshPath();
            RefreshLevel();
            anim.PlayShowChapterAnim();
        }
        private void RefreshChapterData()
        {
            nowTowerLevelDataList.Clear();
            List<TowerConfig> nowChapterLevelConfigList = Configs.Tower.GetConfigList().FindAll(config => config.Chapter == nowShowChapterConfig.Id);
            for (int i = 0; i < nowChapterLevelConfigList.Count; i++)
            {
                TowerConfig nowLevelConfig = nowChapterLevelConfigList[i];
                TowerLevelData nowLevelData = FBTowerController.Instance.GetTowerLevelData(nowLevelConfig);
                nowTowerLevelDataList.Add(nowLevelData);
            }
        }
        private void RefreshChapterBtnState()
        {
            bool isCanLeft = nowShowChapterConfig.Id > Configs.TowerChapter.GetConfigList()[0].Id;
            bool isCanRight = nowShowChapterConfig.Id < FBTowerController.Instance.FBData.currentChapterConfig.Id && nowShowChapterConfig.Id < Configs.TowerChapter.GetConfigList()[^1].Id;
            leftPageLightButton.gameObject.SetActive(isCanLeft);
            rightPageLightButton.gameObject.SetActive(isCanRight);
            leftPageDarkButton.gameObject.SetActive(!isCanLeft);
            rightPageDarkButton.gameObject.SetActive(!isCanRight);
        }
        private void RefreshChapterName()
        {
            chapterNameText.text = nowShowChapterConfig.Name;
        }
        private void RefreshPath()
        {
            for (int i = 0; i < lineItemList.Count; i++)
            {
                FBTowerPathLineItem lineItem = lineItemList[i];
                TowerLevelData towerLevelData = nowTowerLevelDataList[i];
                bool isPass = towerLevelData.towerOpenState == TowerOpenState.Pass;
                lineItem.darkImage.gameObject.SetActive(!isPass);
                lineItem.lightImage.gameObject.SetActive(isPass);
            }
        }

        private void RefreshLevel()
        {
            for (int i = 0; i < levelItemList.Count; i++)
            {
                FBTowerLevelItem levelItem = levelItemList[i];
                levelItem.index = i;
                TowerLevelData towerLevelData = nowTowerLevelDataList[i];
                levelItem.RefreshShow(towerLevelData);
            }
        }

        #endregion

    }
}