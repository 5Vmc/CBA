using Babu;
using BigBang.Animation;
using CBA;
using Coffee.UIEffects;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang.UI
{
    public class DragonYearRedEnvelopePad : MonoBehaviour, IActivity
    {
        protected void OnEnable()
        {
            SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
            sendButton.OnClick += OnClickSendButton;
            getButton.OnClick += OnClickGetButton;
            historyButton.OnClick += OnClickHistoryButton;
            helpButton.OnClick += OnClickHelpButton;
            EventManager.Instance.Register(EventID.OnAfterOpenRedEnvlope, OnAfterOpenRedEnvlope);
            EventManager.Instance.Register(EventID.OnAfterSendRedEnvlope, OnAfterSendRedEnvlope);
            RedEnvlopeManager.Instance.isDragonYearRedEnvelopePadShow = true;
            AudioManager.Instance.PlayMusic(AudioNames.SPRINGFESTIVALBG);
        }
        protected void OnDisable()
        {
            SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
            sendButton.OnClick -= OnClickSendButton;
            getButton.OnClick -= OnClickGetButton;
            historyButton.OnClick -= OnClickHistoryButton;
            helpButton.OnClick -= OnClickHelpButton;
            EventManager.Instance.Unregister(EventID.OnAfterOpenRedEnvlope, OnAfterOpenRedEnvlope);
            EventManager.Instance.Unregister(EventID.OnAfterSendRedEnvlope, OnAfterSendRedEnvlope);
            RedEnvlopeManager.Instance.isDragonYearRedEnvelopePadShow = false;
            AudioManager.Instance.PlayMusic(AudioNames.BGM_HOME);
        }

        private ActivityData activityData = null;
        private GetRedPacketInfoResponse serverData = null;
        public void LoadActivity(ActivityData _data)
        {
            leftTimeRefreshCdTimer?.Cancel();
            leftTimeRefreshCdTimer = null;
            activityData = _data;
            RefreshLeftTime();
            RefreshData();
            if (sendCdTimer != null)
            {
                sendCdTimer.Cancel();
                sendCdTimer = null;
            }
            isInCd = false;
        }
        private void RefreshData()
        {
            Debug.Log("DragonYearRedEnvelopePad , RefreshData");
            RedEnvlopeManager.Instance.GetNewData(() =>
            {
                serverData = RedEnvlopeManager.Instance.serverData;
                SetNotice();
                SetRank();
                SetMyInfo();
                SetGoldmine();
            });
        }
        [SerializeField] private TMP_Text leftTimeText = null;
        private void RefreshLeftTime()
        {
            RefreshActivityLeftTime();
            RefreshStageLeftTime();
        }
        private void RefreshActivityLeftTime()
        {
            if (activityData == null) return;
            long leftTime = activityData.EndTime - Utils.DataConvUtil.ServerTime;
            leftTimeText.text = "剩余时间:{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
            if (leftTime < 0) UIController.Instance.HidePanel<ActivityMainUI>();
        }
        private void SetNotice()
        {

        }
        [SerializeField] private RedEnvelopeSendRankAdapter redEnvelopeSendRankAdapter = null;
        [SerializeField] private RectTransform emptyPanel = null;
        private void SetRank()
        {
            List<RedPacketRankInfo> redPacketRankInfoList = serverData.Ranks.ToList();
            emptyPanel.gameObject.SetActive(redPacketRankInfoList.Count <= 0);
            redEnvelopeSendRankAdapter.SetData(redPacketRankInfoList);
        }
        [SerializeField] private TMP_Text rankText = null;
        [SerializeField] private TMP_Text noRankText = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private TMP_Text sendCountText = null;
        [SerializeField] private TMP_Text upCountText = null;
        [SerializeField] private Image rank2Image = null;
        [SerializeField] private Image rank3Image = null;
        [SerializeField] private Image rank1Image = null;
        private void SetMyInfo()
        {
            if (serverData.MyRank == null)
            {
                serverData.MyRank = new();
                serverData.MyRank.Gbid = Player.GbId;
                serverData.MyRank.LikePacket = 0;
                serverData.MyRank.Name = Player.Name;
                serverData.MyRank.Rank = 99999;
                serverData.MyRank.SendPacket = 0;
                serverData.MyRank.ServerId = Player.ServerData.Id;
            }
            bool isFirst3 = serverData.MyRank.Rank <= 3;
            bool isOut200 = serverData.MyRank.Rank > 200;
            rankText.gameObject.SetActive(!isFirst3 && !isOut200);
            noRankText.gameObject.SetActive(isOut200);
            if (!isFirst3) rankText.text = serverData.MyRank.Rank.ToString();
            rank1Image.gameObject.SetActive(serverData.MyRank.Rank == 1);
            rank2Image.gameObject.SetActive(serverData.MyRank.Rank == 2);
            rank3Image.gameObject.SetActive(serverData.MyRank.Rank == 3);
            sendCountText.text = serverData.MyRank.SendPacket.ToString();
            upCountText.text = serverData.MyRank.LikePacket.ToString();
            nameText.text = "[{0}区]{1}".SafeFormat(serverData.MyRank.ServerId, serverData.MyRank.Name);
        }
        [SerializeField] private Image sendInfo = null;
        [SerializeField] private TMP_Text redEnvelopCountText1 = null;
        [SerializeField] private HorizontalLayoutGroup nextTimePanel = null;
        [SerializeField] private TMP_Text nextTimeText1 = null;
        [SerializeField] private Image getInfo = null;
        [SerializeField] private TMP_Text redEnvelopCountText2 = null;
        [SerializeField] private TMP_Text nextTimeText2 = null;
        [SerializeField] private BabuButton sendButton = null;
        [SerializeField] private BabuButton getButton = null;
        [SerializeField] private UIShiny sendButtonUIShiny = null;
        [SerializeField] private UIShiny getButtonUIShiny = null;
        [SerializeField] private BabuButton historyButton = null;
        [SerializeField] private BabuButton helpButton = null;
        [SerializeField] private Image fullScreenImage = null;

        private bool isGeting = false;
        private DateTime nextChangeDateTime;
        [SerializeField] private SkeletonGraphic goldmineSpine = null;
        private void SetGoldmine()
        {
            DateTime openTime = TimeUtils.ToDateTime(serverData.OpenTime);
            Debug.Log("openTime = " + openTime.ToString());
            DateTime closeTime = TimeUtils.ToDateTime(serverData.CloseTime);
            Debug.Log("closeTime = " + closeTime.ToString());
            isGeting = openTime < DataConvUtil.ServerDateTime && DataConvUtil.ServerDateTime < closeTime;
            DateTime nextOpenTime = TimeUtils.ToDateTime(serverData.NextOpenTime);
            Debug.Log("nextOpenTime = " + nextOpenTime.ToString());
            Debug.Log("DataConvUtil.ServerDateTime = " + DataConvUtil.ServerDateTime.ToString());
            nextChangeDateTime = isGeting ? closeTime : nextOpenTime;

            RefreshStageLeftTime();
            sendButton.gameObject.SetActive(!isGeting);
            getButton.gameObject.SetActive(isGeting);

            //fullScreenImage.gameObject.SetActive(isGeting);

            sendInfo.gameObject.SetActive(!isGeting);
            if (!isGeting)
            {
                GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, activityData.cfg.Param1, 0);
                sendButtonUIShiny.enabled = !isGeting && gameItem.GetPlayerCount() > 0;
                ClearSendTextAnim();
                //ClearYellowAnim();
                redEnvelopCountText1.text = "<color=#F3E28A>当前累计红包:</color>{0}个".SafeFormat(serverData.TotalPacketCount);
                LayoutRebuilder.ForceRebuildLayoutImmediate(redEnvelopCountText1.transform as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(nextTimeText1.transform as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(nextTimePanel.transform as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(sendInfo.transform as RectTransform);
                goldmineSpine.AnimationState.SetAnimation(0, "animation2", true);
            }

            getInfo.gameObject.SetActive(isGeting);
            if (isGeting)
            {
                getButtonUIShiny.enabled = isGeting && serverData.TotalPacketCount > 0;
                PlaySendTextAnim();
                //PlayYellowAnim();
                redEnvelopCountText2.text = "<color=#F3E28A>剩余</color>{0}个".SafeFormat(serverData.TotalPacketCount);
                goldmineSpine.AnimationState.SetAnimation(0, "animation", true);
            }
        }

        private UnityTimer.Timer leftTimeRefreshCdTimer = null;
        private bool isTimeRefreshInCd = false;
        private void RefreshStageLeftTime()
        {
            TMP_Text text = isGeting ? nextTimeText2 : nextTimeText1;
            int leftTime = (int)(nextChangeDateTime - Utils.DataConvUtil.ServerDateTime).TotalSeconds;
            if (leftTime < 0)
            {
                text.text = "即将开始";
            }
            else
            {
                text.text = "{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn(leftTime));
            }
            if (!isGeting)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(redEnvelopCountText1.transform as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(nextTimeText1.transform as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(nextTimePanel.transform as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(sendInfo.transform as RectTransform);
            }
            if (leftTime < 0 && isTimeRefreshInCd == false)
            {
                isTimeRefreshInCd = true;
                leftTimeRefreshCdTimer = UnityTimer.Timer.Register(this.gameObject, 5f, () => { isTimeRefreshInCd = false; });
                RefreshData();
            }
        }

        //private Sequence yellowSeq = null;
        //private void PlayYellowAnim()
        //{
        //    if (yellowSeq != null) return;
        //    yellowSeq = DOTween.Sequence();
        //    yellowSeq.SetTarget(fullScreenImage);
        //    yellowSeq.AddTo(this.gameObject);
        //    yellowSeq.AppendCallback(() =>
        //    {
        //        fullScreenImage.gameObject.SetAlpha(0);
        //    });
        //    yellowSeq.Append(fullScreenImage.gameObject.DOFade(1f, 1.5f));
        //    yellowSeq.Append(fullScreenImage.gameObject.DOFade(0f, 1.5f));
        //    yellowSeq.AppendInterval(0.5f);
        //    yellowSeq.SetLoops(-1);
        //}
        //private void ClearYellowAnim()
        //{
        //    yellowSeq?.Kill();
        //    yellowSeq = null;
        //}

        [SerializeField] private List<RectTransform> sendTextList = new();
        [SerializeField] private List<RectTransform> sendLightList = new();
        private void PlaySendTextAnim()
        {
            if (sendSeq != null) return;
            sendSeq = DOTween.Sequence();
            sendSeq.SetTarget(sendTextList[0].parent.parent);
            sendSeq.AddTo(this.gameObject);
            sendSeq.AppendCallback(() =>
            {
                foreach (RectTransform sendText in sendTextList)
                {
                    sendText.SetAnchoredPositionY(0f);
                }
                foreach (RectTransform sendLight in sendLightList)
                {
                    sendLight.gameObject.SetAlpha(0f);
                }
            });
            for (int i = 0; i < sendTextList.Count; i++)
            {
                sendSeq.Append(sendTextList[i].DOAnchorPosY(10f, 0.2f));
                sendSeq.Append(sendTextList[i].DOAnchorPosY(0f, 0.2f));
            }
            for (int i = 0; i < sendLightList.Count; i++)
            {
                sendSeq.Insert(i * 0.4f + 0.2f, sendLightList[i].gameObject.DOFade(1f, 0.1f));
                sendSeq.Insert(i * 0.4f + 0.4f, sendLightList[i].gameObject.DOFade(0f, 0.2f));
            }
            sendSeq.AppendInterval(0.5f);
            sendSeq.SetLoops(-1);
        }
        private Sequence sendSeq = null;
        private void ClearSendTextAnim()
        {
            sendSeq?.Kill();
            sendSeq = null;
            foreach (RectTransform sendText in sendTextList)
            {
                sendText.SetAnchoredPositionY(0f);
            }
            foreach (RectTransform sendLight in sendLightList)
            {
                sendLight.gameObject.SetAlpha(0f);
            }
        }

        private void OnClickSendButton(BabuButton _)
        {
            GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, activityData.cfg.Param1, 0);
            if (gameItem.GetPlayerCount() <= 0)
            {
                Tips.PopError("红包不足");
                return;
            }
            UIController.Instance.OpenWindow<SendRedEnvelopeUI>(new SendRedEnvelopeUIProperties(activityData));
        }
        private void OnAfterSendRedEnvlope(object[] args)
        {
            RefreshData();
        }

        private UnityTimer.Timer sendCdTimer = null;
        private bool isInCd = false;
        private void OnClickGetButton(BabuButton _)
        {
            if (serverData.TotalPacketCount <= 0)
            {
                Tips.PopError("本轮红包已发完");
                return;
            }
            if (isInCd) return;
            sendCdTimer = UnityTimer.Timer.Register(this.gameObject, 6f, () => { isInCd = false; });
            isInCd = true;
            NetworkManager.Instance.SnatchRedPacket(activityData.cfg.Id, (SnatchRedPacketResponse snatchRedPacketResponse) =>
            {
                RedEnvlopeManager.Instance.serverData.TotalPacketCount = snatchRedPacketResponse.TotalPacketCount;
                SetGoldmine();
                if (snatchRedPacketResponse.Success)
                {
                    UIController.Instance.OpenWindow<OpenRedEnvelopeUI>(new OpenRedEnvelopeUIProperties(activityData, snatchRedPacketResponse));
                }
                else
                {
                    switch (snatchRedPacketResponse.FailReason)
                    {
                        case 1: Tips.PopError("活动已结束"); break;
                        case 2: Tips.PopError("抢的太快了，喘口气吧"); break;
                        case 3: Tips.PopError("手慢了，没有抢到红包"); break;
                    }
                    RefreshData();
                }
            });
        }
        private void OnAfterOpenRedEnvlope(object[] args)
        {
            SetRank();
            SetMyInfo();
            SetGoldmine();
        }

        private void OnClickHelpButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<DragonYearRedEnvelopeHelpUI>();
        }
        private void OnClickHistoryButton(BabuButton _)
        {
            UIController.Instance.OpenWindow<DragonYearRedEnvelopeHistoryUI>(new DragonYearRedEnvelopeHistoryUIProperties(activityData));
        }

    }
}