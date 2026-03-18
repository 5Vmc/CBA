using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class DragonYearRedEnvelopeHistoryUIProperties : WindowProperties
    {
        public ActivityData activityData { get; private set; }

        public DragonYearRedEnvelopeHistoryUIProperties(ActivityData activityData)
        {
            this.activityData = activityData;
        }
    }

    public class DragonYearRedEnvelopeHistoryUI : AWindowController<DragonYearRedEnvelopeHistoryUIProperties>
    {
        [SerializeField] private BabuButton closeBtn;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
        }
        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
        }
        [SerializeField] private DragonYearRedEnvelopeHistoryAdapter dragonYearRedEnvelopeHistoryAdapter = null;
        [SerializeField] private RectTransform emptyPanel = null;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            NetworkManager.Instance.GetRedPacketLogs(Properties.activityData.cfg.Id, (GetRedPacketLogsResponse getRedPacketLogsResponse) =>
            {
                List<RedPacketLogInfo> redPacketLogInfoList = getRedPacketLogsResponse.Logs.ToList();
                redPacketLogInfoList.Reverse();
                emptyPanel.gameObject.SetActive(redPacketLogInfoList.Count <= 0);
                dragonYearRedEnvelopeHistoryAdapter.SetData(redPacketLogInfoList);
            });
        }
        private void OnClose(BabuButton _)
        {
            UIController.Instance.CloseWindow<DragonYearRedEnvelopeHistoryUI>();
        }
    }
}