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
    public class ActivityFestivalMainUIProperties : PanelProperties
    {
        public PlayerCard playerCard { get; set; }
        public int ActivityId;
        public ActivityFestivalMainUIProperties(int activityId = 0)
        {
            ActivityId = activityId;
        }
    }

    public class ActivityFestivalMainUI : APanelController<ActivityFestivalMainUIProperties>
    {

        [SerializeField]
        private GameObject activityTogglePrefab;

        [SerializeField]
        private GameObject padContainer;
        [SerializeField]
        private ScrollRect scrollRect;

        private int selectedIndex = -1;
        private object locker;



        private Dictionary<int, GameObject> padState = new();
        #region 初始化

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
            EventManager.Instance.Register(EventID.OnRefreshActivityTab, RefreshActivityTab);
            bottomToggleGroup.OnValueChanged += OnToggleChanged;
        }

        private void RefreshRedDot(object[] args)
        {
            var toggleList = bottomToggleGroup.transform.GetChildren().ToList();
            var activityList = ActivityController.Instance.OnlineActivityDic;
            foreach (var activityData in activityList.Values)
            {
                //只加载定时活动
                if (activityData.cfg.StartTime != 0)
                {
                    //奖励领完的不加载
                    if (activityData.IsGotAllRewards) continue;
                    var toggleIndex = 0;
                    if (activityData.cfg.ClientType == (int)ActivityClientType.FirstPay)
                    {
                        toggleIndex = 1;
                        GameObject gameObj1 = toggleList[toggleIndex].gameObject;
                        RedDotNode node1 = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FestivalTotalPay, "/" + activityData.cfg.Id);
                        node1.IsRed(gameObj1.GetComponent<ActivityToggle>().reddot.transform);
                    }
                    else if (activityData.cfg.ClientType == (int)ActivityClientType.NationalDayLogin)
                    {
                        toggleIndex = 0;
                        GameObject gameObj = toggleList[toggleIndex].gameObject;
                        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_FestivalLogin, "");
                        node.IsRed(gameObj.GetComponent<ActivityToggle>().reddot.transform);
                    }
                }
            }
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
            EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
            EventManager.Instance.Unregister(EventID.OnRefreshActivityTab, RefreshActivityTab);
            bottomToggleGroup.OnValueChanged -= OnToggleChanged;
        }

        protected override void OnPropertiesSet()
        {
            //在refreshUI里，根据Properties选过selectedIndex
            RefreshActivityTab();
            RefreshRedDot(null);
        }

        private void RefreshSelectTab(object[] args = null)
        {
            if (bottomToggleGroup.EnableToggle == null) bottomToggleGroup.EnableToggle = bottomToggleGroup.GetComponentInChildren<BabuToggle>();
            selectedIndex = Utility.KeepInRange(selectedIndex, 0, toggleCounter - 1);
            bottomToggleGroup.Switch(selectedIndex);
            int total = ActivityController.Instance.OnlineActivityDic.Values.Count<ActivityData>((p) => p.cfg.ServerDays != -1);
            scrollRect.horizontalNormalizedPosition = (float)selectedIndex / (total - 1);
        }

        private int toggleCounter = 0;
        private void RefreshActivityTab(object[] args = null)
        {
            AudioManager.Instance.PlaySound(AudioNames.ENT_DETAILS_SHOW_UP);
            var toggleList = bottomToggleGroup.transform.GetChildren().ToList();

            var toggleCount = toggleList.Count;
            for (var index = 0; index < toggleList.Count; index++)
            {
                toggleList[index].gameObject.SetActive(false);
            }

            var activityList = ActivityController.Instance.OnlineActivityDic;
            toggleCounter = 0;
            foreach (var activityData in activityList.Values)
            {
                if (InWhiteList(activityData) == false) continue;
                //只加载定时活动，但是不加载节日签到
                if (activityData.cfg.StartTime != 0 && activityData.cfg.ClientType != (int)ActivityClientType.NationalDayLogin)
                {
                    //奖励领完的不加载
                    if (activityData.IsGotAllRewards) continue;
                    //如果在properties上传入过activityid，则默认选中
                    if (Properties.ActivityId != 0 && Properties.ActivityId == activityData.cfg.Id)
                    {
                        selectedIndex = toggleCounter;
                        Properties.ActivityId = 0;
                    }

                    GameObject gameObj;
                    if (toggleCounter > toggleCount - 1)
                    {
                        Debug.LogWarning("------- Instantiate " + activityData.cfg.Name);
                        gameObj = Instantiate(activityTogglePrefab, bottomToggleGroup.transform);
                    }
                    else
                    {
                        gameObj = toggleList[toggleCounter].gameObject;
                    }

                    ActivityToggle activityToggle = gameObj.GetComponent<ActivityToggle>();
                    activityToggle.cfg = activityData.cfg;
                    activityToggle.RefreshShow();
                    gameObj.GetComponent<BabuToggle>().group = bottomToggleGroup;
                    gameObj.SetActive(true);

                    //底部toggle的小红点。
                    RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + activityData.cfg.Id);
                    node.IsRed(activityToggle.reddot.transform);
                    toggleCounter++;
                }
            }

            RefreshSelectTab();
        }
        private bool InWhiteList(ActivityData activityData)
        {
            if (activityData.cfg.ClientType == (int)ActivityClientType.NationalDayLogin) return true;
            if (activityData.cfg.Id == (int)ActivityClientType.ZhouQiBack) return true;
            if (activityData.cfg.Id == (int)ActivityClientType.ZhouQiGift) return true;
            return false;
        }
        #endregion

        #region 关闭界面
        [SerializeField] private Button closeBtn;
        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            UIController.Instance.HidePanel<ActivityFestivalMainUI>();
        }
        #endregion

        #region 切换页签

        [SerializeField] private BabuToggleGroup bottomToggleGroup;
        [SerializeField] private ScrollRect bottomToggleScroll;

        [NonSerialized] private List<GameObject> padList;

        [NonSerialized]
        private List<string> padPathList = new List<string>(){
            "Prefabs/Pad/AsyncPad/FirstPayPad.prefab",
            "Prefabs/Pad/AsyncPad/DailyChargePad.prefab",
            "Prefabs/Pad/AsyncPad/TotalPayPad.prefab",
            "Prefabs/Pad/AsyncPad/RankAwardPad.prefab",
            "Prefabs/Pad/AsyncPad/SeasonPassPad.prefab",
            "",
            "Prefabs/Pad/AsyncPad/RecruitPad.prefab",
            "Prefabs/Pad/AsyncPad/DailyGiftPad.prefab",
            "Prefabs/Pad/AsyncPad/FestivalPad.prefab"
        };

        private int getPadIndexByActivityConfigId(int actId)
        {
            ActivityConfig cfg = Configs.Activity.GetConfig(actId);
            if (cfg != null)
            {
                if (cfg.Id == 1001)
                {
                    //首充
                    return 0;
                }
                else if (cfg.ClientType == (int)ActivityClientType.FirstPay)
                {
                    //累充
                    return 2;
                }
                else if (cfg.ClientType == (int)ActivityClientType.DailyPay)
                {
                    //每日充值，循环用
                    return 1;
                }
                else if (cfg.ClientType == (int)ActivityClientType.RankAwards)
                {
                    //排行类
                    return 3;
                }
                else if (cfg.ClientType == (int)ActivityClientType.BattlePass)
                {
                    //战令类，循环用。
                    return 4;
                }
                else if (cfg.ClientType == (int)ActivityClientType.TimeRecruit)
                {
                    //限时抽卡。
                    return 6;
                }
                else if (cfg.ClientType == (int)ActivityClientType.AllStarTimeRecruit)
                {
                    //全明星限时抽卡。
                    return 6;
                }
                else if (cfg.ClientType == (int)ActivityClientType.GiftPay)
                {
                    //每日小额礼包。
                    return 7;
                }
                else if (cfg.ClientType == (int)ActivityClientType.NationalDayLogin)
                {
                    //每日小额礼包。
                    return 8;
                }
            }
            return 0;
        }

        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            oldToggle?.GetComponent<StatusControl>().SetStatus(false);
            newToggle?.GetComponent<StatusControl>().SetStatus(true);
            selectedIndex = bottomToggleGroup.EnableIndex;
            HideAllPad();

            var key = oldToggle.GetComponent<ActivityToggle>();
            var key1 = newToggle.GetComponent<ActivityToggle>();
            var activityData = ActivityController.Instance.OnlineActivityDic[newToggle.GetComponent<ActivityToggle>().cfg.Id];

            initPad(getPadIndexByActivityConfigId(activityData.cfg.Id), activityData);
        }

        private async void initPad(int padIndex, ActivityData activityData)
        {
            //if (padList[padIndex] == null)
            if (!padState.ContainsKey(padIndex))
            {
                //padState.Add(padIndex, Instantiate(padList[padIndex], padContainer.transform));
                var padtask = await CBAUtils.GetPrefab(padPathList[padIndex], padContainer.transform);
                padState.Add(padIndex, padtask);
            }

            padState[padIndex].SetActive(true);
            padState[padIndex].GetComponent<IActivity>().LoadActivity(activityData);
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
