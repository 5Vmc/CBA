using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    [System.Serializable]
    public class ActivityMainUIProperties : PanelProperties
    {
        public PlayerCard playerCard { get; set; }
        public ActivityClientType selectType;
        public List<ActivityClientType> wantShowTypeList;
        public ActivityMainUIProperties(ActivityClientType selectType, List<ActivityClientType> wantShowTypeList)
        {
            this.selectType = selectType;
            this.wantShowTypeList = wantShowTypeList;
        }
    }

    public class ActivityMainUI : APanelController<ActivityMainUIProperties>
    {

        [SerializeField]
        private GameObject activityTogglePrefab;

        [SerializeField]
        private GameObject padContainer;
        [SerializeField]
        private ScrollRect scrollRect;

        private int selectedIndex = -1;
        private object locker;



        private Dictionary<ActivityClientType, GameObject> padState = new();
        #region 初始化

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
            EventManager.Instance.Register(EventID.OnRefreshActivityTab, RefreshActivityTab);
            bottomToggleGroup.OnValueChanged += OnToggleChanged;
        }

        protected override void RemoveListeners()
        {
            if (closeBtn != null) closeBtn.onClick.RemoveListener(OnClose);
            EventManager.Instance?.Unregister(EventID.OnRefreshActivityTab, RefreshActivityTab);
            if (bottomToggleGroup != null) bottomToggleGroup.OnValueChanged -= OnToggleChanged;
        }

        protected override void OnPropertiesSet()
        {
            HideAllPad();
            //在refreshUI里，根据Properties选过selectedIndex
            RefreshActivityTab();
        }

        private void RefreshSelectTab(object[] args = null)
        {
            if (bottomToggleGroup.EnableToggle == null) bottomToggleGroup.EnableToggle = bottomToggleGroup.GetComponentInChildren<BabuToggle>();
            selectedIndex = Utility.KeepInRange(selectedIndex, 0, toggleCounter - 1);
            bottomToggleGroup.Switch(selectedIndex);
            int total = ActivityController.Instance.OnlineActivityDic.Values.Count<ActivityData>((p) => p.cfg.ServerDays != -1);
            scrollRect.horizontalNormalizedPosition = (float)selectedIndex / (total - 1);
            if (toggleCounter == 1)
            {
                var toggleList = bottomToggleGroup.transform.GetChildren().ToList();
                for (var index = 0; index < toggleList.Count; index++)
                {
                    toggleList[index].gameObject.SetActive(false);
                }
            }
        }



        private int toggleCounter = 0;
        private void RefreshActivityTab(object[] args = null)
        {
            var toggleList = bottomToggleGroup.transform.GetChildren().ToList();

            var toggleCount = toggleList.Count;
            for (var index = 0; index < toggleList.Count; index++)
            {
                toggleList[index].gameObject.SetActive(false);
            }

            toggleCounter = 0;

            List<ActivityToggleData> toggleDataList = ActivityController.Instance.GetActivityToggleDataList(Properties.wantShowTypeList);

            foreach (ActivityToggleData toggleData in toggleDataList)
            {
                //非时间性活动不加载
                //if (activityData.cfg.ServerDays == -1) continue;

                //定时活动不加载
                //if (activityData.cfg.StartTime != 0) continue;

                //奖励领完的不加载
                //if (activityData.IsGotAllRewards) continue;

                //如果在properties上传入过activityid，则默认选中
                if (Properties.selectType != ActivityClientType.Unknow && Properties.selectType == (ActivityClientType)toggleData.activityConfig.ClientType)
                {
                    selectedIndex = toggleCounter;
                }

                GameObject gameObj;
                if (toggleCounter > toggleCount - 1)
                {
                    gameObj = Instantiate(activityTogglePrefab, bottomToggleGroup.transform);
                }
                else
                {
                    gameObj = toggleList[toggleCounter].gameObject;
                }

                ActivityToggle activityToggle = gameObj.GetComponent<ActivityToggle>();
                activityToggle.cfg = toggleData.activityConfig;
                activityToggle.RefreshShow();
                gameObj.GetComponent<BabuToggle>().group = bottomToggleGroup;
                gameObj.SetActive(true);

                //底部toggle的小红点。
                //RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + toggleData.activityConfig.Id);
                //node.IsRed(activityToggle.reddot.transform);
                toggleCounter++;
            }

            if (toggleCounter <= 0)
            {
                UIController.Instance.HidePanel<ActivityMainUI>();
                return;
            }

            RefreshSelectTab();
        }

        #endregion

        #region 关闭界面
        [SerializeField] private Button closeBtn;
        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            UIController.Instance.HidePanel<ActivityMainUI>();
        }
        #endregion

        #region 切换页签

        [SerializeField] private BabuToggleGroup bottomToggleGroup;
        [SerializeField] private ScrollRect bottomToggleScroll;

        [NonSerialized] private List<GameObject> padList;
        [NonSerialized]
        private readonly Dictionary<ActivityClientType, string> padPathDic = new()
        {
            { ActivityClientType.FirstPay , "Prefabs/Pad/AsyncPad/FirstPayPad.prefab" } ,
            { ActivityClientType.DailyPay , "Prefabs/Pad/AsyncPad/DailyChargePad.prefab" } ,
            { ActivityClientType.TotalPay , "Prefabs/Pad/AsyncPad/TotalPayPad.prefab" } ,
            { ActivityClientType.RankAwards , "Prefabs/Pad/AsyncPad/RankAwardPad.prefab" } ,
            { ActivityClientType.BattlePass , "Prefabs/Pad/AsyncPad/SeasonPassPad.prefab" } ,
            { ActivityClientType.TimeRecruit , "Prefabs/Pad/AsyncPad/RecruitPad.prefab" } ,
            { ActivityClientType.AllStarTimeRecruit , "Prefabs/Pad/AsyncPad/RecruitPad.prefab" } ,
            { ActivityClientType.GiftPay , "Prefabs/Pad/AsyncPad/DailyGiftPad.prefab" } ,
            { ActivityClientType.Sign7Day , "Prefabs/Pad/AsyncPad/SevenDaysLoginUI.prefab" } ,
            { ActivityClientType.Sign30Day , "Prefabs/Pad/AsyncPad/MonthSignUI.prefab" } ,
            { ActivityClientType.EnergyCenter , "Prefabs/Pad/AsyncPad/EnergyCenterUI.prefab" } ,
            { ActivityClientType.TimeGiftCollection , "Prefabs/Pad/AsyncPad/TimeGiftCollectionUI.prefab" } ,
            { ActivityClientType.DragonYearRedEnvelope , "Prefabs/Pad/AsyncPad/DragonYearRedEnvelopePad.prefab" } ,
            { ActivityClientType.SpringFestivalWish , "Prefabs/Pad/AsyncPad/DragonYearSignPad.prefab" } ,
            { ActivityClientType.SpringFestivalTask , "Prefabs/Pad/AsyncPad/DragonYearTaskPad.prefab" } ,
            { ActivityClientType.SpringFestivalGift , "Prefabs/Pad/AsyncPad/DragonYearGiftPad.prefab" } ,
            { ActivityClientType.YangHanSenMainPage , "Prefabs/Pad/AsyncPad/YangHanSenMainPagePad.prefab" } ,
            { ActivityClientType.LiXiaoXuMainPage , "Prefabs/Pad/AsyncPad/LiXiaoXuMainPagePad.prefab" } ,
            { ActivityClientType.AllStarHome , "Prefabs/Pad/AsyncPad/AllStarTaskPad.prefab" } ,
            { ActivityClientType.AllStarGift , "Prefabs/Pad/AsyncPad/AllStarGiftPad.prefab" } ,
            { ActivityClientType.AllStarTask , "Prefabs/Pad/AsyncPad/AllStarHomePad.prefab" } ,
            { ActivityClientType.LabourDayGift , "Prefabs/Pad/AsyncPad/LabourDayGiftPad.prefab" } ,
            { ActivityClientType.LabourDayTask , "Prefabs/Pad/AsyncPad/LabourDayTaskPad.prefab" } ,
            { ActivityClientType.LabourDaySign , "Prefabs/Pad/AsyncPad/LabourDaySignPad.prefab" } ,
            { ActivityClientType.PlayoffFinalsGuessHome , "Prefabs/Pad/AsyncPad/PlayoffFinalsGuessHomePad.prefab" } ,
            { ActivityClientType.PlayoffFinalsGuessSingle , "Prefabs/Pad/AsyncPad/PlayoffFinalsGuessSinglePad.prefab" } ,
            { ActivityClientType.DragonBoatFestivalHome , "Prefabs/Pad/AsyncPad/DragonBoatFestivalHomePad.prefab" } ,
            { ActivityClientType.Olympics2024Sign , "Prefabs/Pad/AsyncPad/Olympics2024SignPad.prefab" } ,
            { ActivityClientType.Olympics2024Gift , "Prefabs/Pad/AsyncPad/Olympics2024GiftPad.prefab" } ,
        };

        private ActivityClientType getActivityClientTypeByActivityConfig(ActivityConfig activityConfig)
        {
            if (activityConfig == null)
            {
                Debug.LogWarning("ActivityMainUI , getActivityClientTypeByActivityConfig , activityConfig == null");
                return ActivityClientType.Sign30Day;
            }
            ActivityClientType activityClientType;
            try
            {
                activityClientType = (ActivityClientType)activityConfig.ClientType;
            }
            catch
            {
                Debug.LogWarning("ActivityMainUI , getActivityClientTypeByActivityConfig , activityConfig.ClientType convert to ActivityClientType occur error , activityConfig.ClientType = " + activityConfig.ClientType);
                return ActivityClientType.Sign30Day;
            }
            return activityClientType;
        }

        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            oldToggle?.GetComponent<StatusControl>().SetStatus(false);
            newToggle?.GetComponent<StatusControl>().SetStatus(true);
            selectedIndex = bottomToggleGroup.EnableIndex;

            //ActivityToggle key = oldToggle.GetComponent<ActivityToggle>();
            ActivityToggle key1 = newToggle.GetComponent<ActivityToggle>();

            ActivityData activityData = null;
            if (ActivityController.Instance.OnlineActivityDic.ContainsKey(key1.cfg.Id))
                activityData = ActivityController.Instance.OnlineActivityDic[key1.cfg.Id];

            Properties.selectType = (ActivityClientType)key1.cfg.ClientType;
            initPad(getActivityClientTypeByActivityConfig(key1.cfg), activityData, key1.cfg);
        }

        private async void initPad(ActivityClientType activityClientType, ActivityData activityData, ActivityConfig activityConfig)
        {
            if (!padState.ContainsKey(activityClientType))
            {
                if (!padPathDic.ContainsKey(activityClientType))
                {
                    Debug.LogWarning("ActivityMainUI , initPad , padPathDic not contains key , activityClientType = " + activityClientType);
                    HideAllPad();
                    return;
                }
                var padtask = await CBAUtils.GetPrefab(padPathDic[activityClientType], padContainer.transform);
                padState.Add(activityClientType, padtask);
            }
            AudioManager.Instance.PlaySound(AudioNames.ENT_DETAILS_SHOW_UP);
            HideAllPad();
            padState[activityClientType].SetActive(true);
            if (activityData != null) padState[activityClientType].GetComponent<IActivity>()?.LoadActivity(activityData);
            if (activityConfig != null) padState[activityClientType].GetComponent<IActivityClient>()?.LoadActivityClient(activityConfig);
        }

        private void HideAllPad()
        {
            foreach (var pad in padState.Values)
            {
                pad.SetActive(false);
            }
        }

        #endregion

    }
}
