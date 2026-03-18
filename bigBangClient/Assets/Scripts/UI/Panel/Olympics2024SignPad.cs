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
    public class Olympics2024SignPad : MonoBehaviour, IActivity
    {
        #region 初始化
        [SerializeField] private TMP_Text leftTimeText = null;
        [SerializeField] private BabuButton helpButton = null;
        [SerializeField] private List<NewYearSignItem> newYearSignItemList = null;
        [SerializeField] private TMP_Text signProgressText = null;
        [SerializeField] private BabuButton signButton = null;

        private ActivityData activityData = null;
        public void LoadActivity(ActivityData _data)
        {
            activityData = _data;
            RefreshActivityData();
            RefreshUI();
        }

        private void OnEnable()
        {
            helpButton.OnClick += OnClickHelpButton;
            signButton.OnClick += OnClickSignButton;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
            EventManager.Instance.Register(EventID.OnNewYearSignSelectItemSet, OnNewYearSignSelectItemSet);
        }

        private void OnDisable()
        {
            helpButton.OnClick -= OnClickHelpButton;
            signButton.OnClick -= OnClickSignButton;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
            EventManager.Instance.Unregister(EventID.OnNewYearSignSelectItemSet, OnNewYearSignSelectItemSet);
        }
        #endregion

        #region 按钮回调
        private void OnClickHelpButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<Olympics2024SignHelpUI>();
        }
        private void OnClickSignButton(BabuButton _)
        {
            if (ActivityController.Instance.wishSignRewards.Count >= 7)
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
                ActivityController.Instance.RefreshRedDot(activityData);
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

        private void RefreshUI()
        {
            RefreshSignItem();
            RefreshSignProgressText();
        }

        private WishSignConfig wishSignConfig = null;
        private void RefreshActivityData()
        {
            wishSignConfig = Configs.WishSign.GetConfig(activityData.cfg.Id);
            if (wishSignConfig == null)
            {
                Debug.LogError("Olympics2024SignPad , RefreshActivityData , wishSignConfig == null , activityData.cfg.Id = {0}".SafeFormat(activityData.cfg.Id));
                this.gameObject.SetActive(false);
                return;
            }
        }

        private void RefreshSignProgressText()
        {
            signProgressText.text = "祈愿值: <color=#A62214>{0}</color>/100".SafeFormat(Player.TaskManager.DailyTasks.Point - (ActivityController.Instance.todayWishTimes * 100));
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