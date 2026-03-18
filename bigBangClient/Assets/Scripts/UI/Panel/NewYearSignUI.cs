using System;
using System.Collections.Generic;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System.Linq;
using Coffee.UIEffects;
using GameConfig.Config;
using Babu.Config;
using Babu;
using Utils.GameItem;
using Protocol;
using GameItem = Utils.GameItem.GameItem;
using DG.Tweening;

namespace BigBang.UI
{
    public class NewYearSignUI : APanelController
    {
        #region 初始化
        [SerializeField] private TMP_Text leftTimeText = null;
        [SerializeField] private BabuButton helpButton = null;
        [SerializeField] private Button closeBtn = null;
        [SerializeField] private List<NewYearSignItem> newYearSignItemList = null;
        [SerializeField] private TMP_Text signProgressText = null;
        [SerializeField] private BabuButton signButton = null;

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
            helpButton.OnClick += OnClickHelpButton;
            signButton.OnClick += OnClickSignButton;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
            EventManager.Instance.Register(EventID.OnNewYearSignSelectItemSet, OnNewYearSignSelectItemSet);
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
            helpButton.OnClick -= OnClickHelpButton;
            signButton.OnClick -= OnClickSignButton;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
            EventManager.Instance.Unregister(EventID.OnNewYearSignSelectItemSet, OnNewYearSignSelectItemSet);
        }
        #endregion

        #region 屏幕适配

        private void Start()
        {
            ScreenFix();
        }

        private float topPanelY219 = -273.348f;
        private float topPanelY169 = -142f;
        [SerializeField] private RectTransform topPanel = null;

        private float signProgressTextY219 = -463.5f;
        private float signProgressTextY169 = -356f;
        //[SerializeField] private TMP_Text signProgressText = null;

        private float signButtonY219 = -540.3f;
        private float signButton169 = -429f;
        //[SerializeField] private BabuButton signButton = null;

        private void ScreenFix()
        {
            float t = UIFrame.GetFixScreenLerpT();

            float topPanelY = Mathf.Lerp(topPanelY169, topPanelY219, t);
            topPanel.SetAnchoredPositionY(topPanelY);

            float signProgressTextY = Mathf.Lerp(signProgressTextY169, signProgressTextY219, t);
            (signProgressText.transform as RectTransform).SetAnchoredPositionY(signProgressTextY);

            float signButtonY = Mathf.Lerp(signButton169, signButtonY219, t);
            (signButton.transform as RectTransform).SetAnchoredPositionY(signButtonY);
        }

        #endregion

        #region 按钮回调
        private void OnClose()
        {
            closeBtn.GetComponent<ButtonAnim>().PlayBack(() => UIController.Instance.HidePanel<NewYearSignUI>(), playAudio: () =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            });
        }
        private void OnClickHelpButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<NewYearSignHelpUI>();
        }
        private void OnClickSignButton(BabuButton _)
        {
            if (ActivityController.Instance.wishSignRewards.Count >= 5)
            {
                Tips.PopTips("您已获得该活动的全部奖励");
                return;
            }
            int lightIndex = ActivityController.Instance.wishSignRewards.Count;
            NewYearSignItem lightItem = newYearSignItemList[lightIndex];
            if (!lightItem.isOpen)
            {
                Tips.PopTips("请等待下一许愿签开启");
                return;
            }
            if (!lightItem.isSetReward)
            {
                Tips.PopTips("请设置下一许愿签");
                return;
            }
            if (Player.TaskManager.DailyTasks.Point - (ActivityController.Instance.todayWishTimes * 100) < 100)
            {
                Tips.PopTips("祈愿值不足");
                return;
            }
            NetworkManager.Instance.GetWishSignReward(activityData.cfg.Id, lightIndex + 1, (GetWishSignRewardResponse getWishSignRewardResponse) =>
            {
                UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(lightItem.gameItem));
                ActivityController.Instance.todayWishTimes++;
                ActivityController.Instance.wishSignRewards.Add(lightItem.wishIndex);
                ActivityController.Instance.RefreshNewYearSignRedDot();
                EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
                RefreshUI();
            });
        }
        #endregion

        #region 界面刷新

        private void OnNewYearSignSelectItemSet(object[] args)
        {
            RefreshUI();
        }

        protected override void OnPropertiesSet()
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            RefreshActivityData();
            RefreshSignItem();
            RefreshSignProgressText();
        }

        private ActivityData activityData = null;
        private WishSignConfig wishSignConfig = null;
        private void RefreshActivityData()
        {
            if (ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.NewYearSign) == false)
            {
                Debug.LogWarning("NewYearSignUI , RefreshActivityData , ActivityController.Instance.OnlineActivityDic.ContainsKey(ActivityID.NewYearSign) == false");
                UIController.Instance.HidePanel<NewYearSignUI>();
                return;
            }
            activityData = ActivityController.Instance.OnlineActivityDic[ActivityID.NewYearSign];
            wishSignConfig = Configs.WishSign.GetConfig(ActivityID.NewYearSign);
            if (wishSignConfig == null)
            {
                Debug.LogError("NewYearSignUI , RefreshTreeItem , wishSignConfig == null , ActivityID.NewYearSign = {0}".SafeFormat(ActivityID.NewYearSign));
                UIController.Instance.HidePanel<NewYearSignUI>();
                return;
            }
        }

        private void RefreshSignProgressText()
        {
            signProgressText.text = "祈愿值: <color=#43FF6D>{0}</color>/100".SafeFormat(Player.TaskManager.DailyTasks.Point - (ActivityController.Instance.todayWishTimes * 100));
        }

        private void RefreshSignItem()
        {
            for (int i = 0; i < newYearSignItemList.Count; i++)
            {
                newYearSignItemList[i].SetData(activityData, wishSignConfig, i + 1);
            }
        }
        private void RefreshLeftTime()
        {
            if (activityData == null) return;
            long leftTime = activityData.EndTime - Utils.DataConvUtil.ServerTime;
            leftTimeText.text = "剩余时间：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
        }
        #endregion
    }
}