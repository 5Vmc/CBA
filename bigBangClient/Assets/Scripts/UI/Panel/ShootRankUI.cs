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
using static BigBang.SpriteNames;
using Protocol;

namespace BigBang.UI
{
    public class ShootRankUI : AWindowController
    {
        [SerializeField] private BabuButton closeBtn = null;
        [SerializeField] private BabuToggleGroup bottomToggleGroup = null;
        [SerializeField] private BabuButton helpButton = null;
        [SerializeField] private ShootRankItem myShootRankItem = null;

        protected override void AddListeners()
        {
            closeBtn.OnClick += OnClose;
            bottomToggleGroup.OnValueChanged += OnToggleChanged;
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTimeOneSec);
            helpButton.OnClick += OnClickHelpButton;
        }
        protected override void RemoveListeners()
        {
            closeBtn.OnClick += OnClose;
            bottomToggleGroup.OnValueChanged += OnToggleChanged;
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTimeOneSec);
            helpButton.OnClick -= OnClickHelpButton;
        }

        private void OnClickHelpButton(BabuButton button)
        {
            string content = "1.每日排行榜：取当天最佳成绩计入每日排行，每天0点重置榜单。\n2.每周排行榜：取每日最佳成绩累计计入每周排行，每周一0点重置榜单。";
            UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties(content, "确定", null, "排行榜说明"));
        }

        private void OnClose(BabuButton _)
        {
            UIController.Instance.CloseWindow<ShootRankUI>();
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            bottomToggleGroup.Switch(0);
            RefreshDayEndTime();
            RefreshWeekEndTime();
            RefreshLeftTimeOneSec();
        }

        #region 页签

        private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            int selectedIndex = bottomToggleGroup.EnableIndex;
            ShowPad(selectedIndex);
        }

        [SerializeField] private TMP_Text txtLeftTime = null;
        [SerializeField] private ShootRankAdapter shootRankAdapter = null;
        private readonly float adapterBottomHasMy = 265.6507f;
        private readonly float adapterBottomNoMy = 146.001f;
        private void ShowPad(int selectedIndex)
        {
            RefreshLeftTimeOneSec();
            NetworkManager.Instance.GetAllRankList(selectedIndex + 1, 1, (GetAllRankListResponse getAllRankListResponse) =>
            {
                List<AllRankInfo> allRankInfoList = getAllRankListResponse.Ranks.ToList();
                shootRankAdapter.SetData(allRankInfoList);
                AllRankInfo myAllRankInfo = allRankInfoList.FirstOrDefault(a => a.IsSelf);
                myShootRankItem.gameObject.SetActive(myAllRankInfo != null);
                if (myAllRankInfo != null)
                {
                    myShootRankItem.SetData(myAllRankInfo);
                    myShootRankItem.SetSelf();
                }
                shootRankAdapter.transform.GetComponent<RectTransform>().SetBottom(myAllRankInfo == null ? adapterBottomNoMy : adapterBottomHasMy);
            });
        }

        #endregion

        private DateTime dayEndTime;

        private void RefreshDayEndTime()
        {
            DateTime dt = Utils.DataConvUtil.ServerDateTime;
            dayEndTime = dt.Date.AddDays(1);
        }
        private DateTime weekEndTime;
        private void RefreshWeekEndTime()
        {
            DateTime dt = Utils.DataConvUtil.ServerDateTime;
            DateTime begintime = (dt.AddDays(1 - Convert.ToInt32(dt.DayOfWeek.ToString("d")))).Date;
            weekEndTime = begintime.AddDays(7);
        }

        private void RefreshLeftTimeOneSec()
        {
            DateTime endTime;
            if (bottomToggleGroup.EnableIndex == 0)
            {
                endTime = dayEndTime;
            }
            else
            {
                endTime = weekEndTime;
            }
            long leftTime = TimeUtils.ToUnixStamp(endTime) - Utils.DataConvUtil.ServerTime;
            if (leftTime < 0)
            {
                if (bottomToggleGroup.EnableIndex == 0)
                {
                    RefreshDayEndTime();
                }
                else
                {
                    RefreshWeekEndTime();
                }
                return;
            }
            txtLeftTime.text = "活动剩余时间：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
        }
    }
}