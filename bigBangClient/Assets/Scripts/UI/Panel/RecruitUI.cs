using System;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static BigBang.AllStarManager;
using static BigBang.UI.RecruitUI;

namespace BigBang.UI
{
    [Serializable]
    public class RecruitUIProperties : PanelProperties
    {
        public SubUIID subUIID = SubUIID.Auto;
        public Area area = Area.North;
        public RecruitUIProperties(SubUIID subUIID, Area area = Area.North)
        {
            this.subUIID = subUIID;
            this.area = area;
        }
    }
    public class RecruitUI : APanelController<RecruitUIProperties>
    {
        [SerializeField] RecruitPad pad;
        private int poolId = 1;

        [SerializeField] private RecruitUIGuide recruitUIGuide;
        [SerializeField] private Toggle timeToggle = null;
        [SerializeField] private Toggle allStarToggle = null;
        protected override void OnPropertiesSet()
        {
            // 播放音效
            AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);
            ActivityData allStarTimeRecruitActivityDataNorth = ActivityController.Instance.FindAllStar2024NorthTimeRecruit;
            ActivityData allStarTimeRecruitActivityDataSouth = ActivityController.Instance.FindAllStar2024SouthTimeRecruit;
            bool isAllStarOpen = allStarTimeRecruitActivityDataNorth != null && allStarTimeRecruitActivityDataSouth != null;
            allStarToggle.gameObject.SetActive(isAllStarOpen);
            ActivityData normalTimeRecruitActivityData = ActivityController.Instance.FindTimeRecruitActivity;
            timeToggle.gameObject.SetActive(normalTimeRecruitActivityData != null);
            if (Properties == null || Properties.subUIID == SubUIID.Auto)
            {
                if (isAllStarOpen)
                {
                    subUIID = SubUIID.AllStar;
                    Properties.subUIID = subUIID;
                }
                else if (normalTimeRecruitActivityData != null)
                {
                    subUIID = SubUIID.Time;
                    Properties.subUIID = subUIID;
                }
                else
                {
                    subUIID = SubUIID.Normal;
                    Properties.subUIID = subUIID;
                }
            }
            else
            {
                subUIID = Properties.subUIID;
            }
            if (GuideManager.IsStarterGuide || normalTimeRecruitActivityData == null) subUIID = SubUIID.Normal;
            bottomToggleGroup.Switch((int)subUIID);
            RefreshRedDot(null);
            recruitUIGuide.CheckGuide();
        }

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClickClose;
            bottomToggleGroup.OnValueChanged += OnToggleChanged;
            EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
            EventManager.Instance.Register(EventID.OnRecruitChangeArea, OnRecruitChangeArea);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClickClose;
            bottomToggleGroup.OnValueChanged -= OnToggleChanged;
            EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
            EventManager.Instance.Register(EventID.OnRecruitChangeArea, OnRecruitChangeArea);
        }
        private void OnRecruitChangeArea(object[] args)
        {
            Properties.area = (Area)args[0];
        }

        [SerializeField] private BabuButton closeBtn = null;
        private void OnClickClose(BabuButton _)
        {
            UIController.Instance.HidePanel<RecruitUI>();
        }

        #region 切换页签

        public enum SubUIID
        {
            /// <summary> 普通招募 </summary>
            Normal = 0,
            /// <summary> 限时招募 </summary>
            Time = 1,
            /// <summary> 全明星招募 </summary>
            AllStar = 2,

            Auto = 999,
        }
        SubUIID subUIID = SubUIID.Normal;

        [SerializeField] private BabuToggleGroup bottomToggleGroup = null;
        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            int selectedIndex = bottomToggleGroup.EnableIndex;
            ShowPad((SubUIID)selectedIndex);
        }
        private void ShowPad(SubUIID padIndex)
        {
            Properties.subUIID = padIndex;
            switch (padIndex)
            {
                case SubUIID.Normal: OnShowNormal(); break;
                case SubUIID.Time: OnShowTime(); break;
                case SubUIID.AllStar: OnShowAllStar(); break;
            }
        }

        private void OnShowNormal()
        {
            pad.enabled = false;
            pad.enabled = true;
            pad.LoadActivity(poolId);
        }
        private void OnShowTime()
        {
            if (GuideManager.InForceGuide)
            {
                subUIID = SubUIID.Normal;
                bottomToggleGroup.Switch((int)subUIID);
                return;
            }
            ActivityData activityData = ActivityController.Instance.FindTimeRecruitActivity;
            if (activityData == null)
            {
                Tips.PopTips("限时招募活动已结束");
                subUIID = SubUIID.Normal;
                bottomToggleGroup.Switch((int)subUIID);
                return;
            }
            pad.enabled = false;
            pad.enabled = true;
            pad.GetComponent<IActivity>().LoadActivity(activityData);
        }
        private void OnShowAllStar()
        {
            if (GuideManager.InForceGuide)
            {
                subUIID = SubUIID.Normal;
                bottomToggleGroup.Switch((int)subUIID);
                return;
            }
            ActivityData allStarTimeRecruitActivityDataNorth = ActivityController.Instance.FindAllStar2024NorthTimeRecruit;
            ActivityData allStarTimeRecruitActivityDataSouth = ActivityController.Instance.FindAllStar2024SouthTimeRecruit;
            bool isAllStarOpen = allStarTimeRecruitActivityDataNorth != null && allStarTimeRecruitActivityDataSouth != null;
            if (isAllStarOpen == false)
            {
                Tips.PopTips("全明星招募活动已结束");
                subUIID = SubUIID.Normal;
                bottomToggleGroup.Switch((int)subUIID);
                return;
            }
            pad.enabled = false;
            pad.enabled = true;
            pad.GetComponent<IActivity>().LoadActivity(Properties.area == Area.North ? allStarTimeRecruitActivityDataNorth : allStarTimeRecruitActivityDataSouth);
        }

        [SerializeField] private Image normalRedDot = null;
        [SerializeField] private Image timeRedDot = null;
        [SerializeField] private Image allStarRedDot = null;
        private void RefreshRedDot(object[] _)
        {
            ActivityData activityData = ActivityController.Instance.FindTimeRecruitActivity;
            if (activityData != null)
            {
                RedDotNode timeNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + activityData.cfg.Id);
                if (timeRedDot.IsDestroyed() == false) timeNode.IsRed(timeRedDot.transform);
            }
            else
            {
                if (normalRedDot.IsDestroyed() == false) normalRedDot.gameObject.SetActive(false);
            }

            RedDotNode NormalNode = RedDotManager.Instance.ConfirmNode(PanelNodePath.Recruit, "/1");
            if (normalRedDot.IsDestroyed() == false) NormalNode.IsRed(normalRedDot.transform);

            bool isAllStarRed = false;
            {
                ActivityData allStarTimeRecruitActivityDataNorth = ActivityController.Instance.FindAllStar2024NorthTimeRecruit;
                if (allStarTimeRecruitActivityDataNorth != null)
                {
                    RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Recruit, "/" + allStarTimeRecruitActivityDataNorth.cfg.Param1);
                    isAllStarRed |= node.IsRed(null);
                }
            }
            {
                ActivityData allStarTimeRecruitActivityDataSouth = ActivityController.Instance.FindAllStar2024SouthTimeRecruit;
                if (allStarTimeRecruitActivityDataSouth != null)
                {
                    RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Recruit, "/" + allStarTimeRecruitActivityDataSouth.cfg.Param1);
                    isAllStarRed |= node.IsRed(null);
                }
            }
            allStarRedDot.gameObject.SetActive(isAllStarRed);

        }

        #endregion
    }
}