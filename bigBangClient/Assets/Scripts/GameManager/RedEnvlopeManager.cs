using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Babu;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Google.Protobuf.Collections;
using Protocol;
using UnityEngine;
using Utils;

namespace BigBang
{
    /// <summary>
    /// 红包的管理类
    /// </summary>
    public class RedEnvlopeManager : BabuSingleton<RedEnvlopeManager>
    {

        private bool isInited = false;
        /// <summary>
        /// 配置表加载完成后，对数据进行预处理
        /// </summary>
        public void InitOnce(bool forceInit = true)
        {
            updateDataTimer?.Cancel();
            updateDataTimer = null;
            savedDateTime = new();

            if (isInited && !forceInit) return;
            isInited = true;
        }

        public DateTime savedDateTime;
        public DateTime serverDataDateTime;
        public GetRedPacketInfoResponse serverData = null;
        public void GetNewData(Action action = null)
        {
            NetworkManager.Instance.GetRedPacketInfo(ActivityID.DragonYearRedEnvelope, (GetRedPacketInfoResponse getRedPacketInfoResponse) =>
            {
                serverData = getRedPacketInfoResponse;
                serverDataDateTime = DataConvUtil.ServerDateTime;
                RefreshTimer();
                action?.Invoke();
                ActivityController.Instance.RefreshActivityRedDotByClientType(ActivityClientType.DragonYearRedEnvelope);
                CheckNeedShowToast();
            });
        }
        private void CheckNeedShowToast()
        {
            if (serverData == null) return;
            DateTime openTime = TimeUtils.ToDateTime(RedEnvlopeManager.Instance.serverData.OpenTime);
            DateTime closeTime = TimeUtils.ToDateTime(RedEnvlopeManager.Instance.serverData.CloseTime);
            bool isGeting = openTime < DataConvUtil.ServerDateTime && DataConvUtil.ServerDateTime < closeTime;
            bool isCanGet = isGeting && RedEnvlopeManager.Instance.serverData.TotalPacketCount > 0;
            if (isCanGet == true && savedDateTime != openTime)
            {
                ShowToast(openTime);
            }
            else
            {
                DateTime nextOpenTime = TimeUtils.ToDateTime(serverData.NextOpenTime);
                if (nextOpenTime <= DataConvUtil.ServerDateTime && savedDateTime != nextOpenTime)
                {
                    ShowToast(nextOpenTime);
                }
            }
        }
        public bool isDragonYearRedEnvelopePadShow = false;
        private void ShowToast(DateTime openTime)
        {
            if (GuideManager.InForceGuide) return;
            if (isDragonYearRedEnvelopePadShow) return;
            savedDateTime = openTime;
            UIController.Instance.OpenWindow<DragonYearRedEnvelopeTipUI>();
        }


        public void AddLike(string gbid)
        {
            if (gbid == Player.GbId)
            {
                serverData.MyRank.LikePacket += 1;
            }
            foreach (var item in serverData.Ranks)
            {
                if (item.Gbid == gbid)
                {
                    item.LikePacket += 1;
                }
            }
        }
        public void ResetRankDataByUp()
        {
            RedPacketRankInfo myInRank = null;
            List<RedPacketRankInfo> redPacketRankInfoList = new(serverData.Ranks);
            int index = -1;
            for (int i = 0; i < redPacketRankInfoList.Count; i++)
            {
                var item = redPacketRankInfoList[i];
                if (item.Gbid == Player.GbId)
                {
                    index = i;
                    myInRank = item;
                    break;
                }
            }
            if (myInRank == null)
            {
                serverData.Ranks.Add(serverData.MyRank.Clone());
            }
            else
            {
                redPacketRankInfoList[index] = serverData.MyRank.Clone();
            }

            redPacketRankInfoList = redPacketRankInfoList
                .OrderByDescending(item => item.SendPacket)
                .ThenByDescending(item => item.LikePacket)
                .ThenByDescending(item => item.Gbid != Player.GbId)
                .Take(200)
                .ToList();
            for (int i = 0; i < redPacketRankInfoList.Count; i++)
            {
                redPacketRankInfoList[i].Rank = i + 1;
                if (redPacketRankInfoList[i].Gbid == Player.GbId) serverData.MyRank.Rank = redPacketRankInfoList[i].Rank;
            }
            serverData.Ranks.Clear();
            serverData.Ranks.AddRange(redPacketRankInfoList);
        }

        private UnityTimer.Timer updateDataTimer;
        private void RefreshTimer()
        {
            if (serverData == null) return;
            DateTime openTime = TimeUtils.ToDateTime(serverData.OpenTime);
            DateTime closeTime = TimeUtils.ToDateTime(serverData.CloseTime);
            bool isGeting = openTime < DataConvUtil.ServerDateTime && DataConvUtil.ServerDateTime < closeTime;
            DateTime nextOpenTime = TimeUtils.ToDateTime(serverData.NextOpenTime);
            DateTime nextChangeDateTime = isGeting ? closeTime : nextOpenTime;
            int leftTime = (int)(nextChangeDateTime - Utils.DataConvUtil.ServerDateTime).TotalSeconds;
            updateDataTimer?.Cancel();
            updateDataTimer = null;
            if (leftTime > 0)
            {
                updateDataTimer = UnityTimer.Timer.Register(this.gameObject, leftTime, () =>
                {
                    updateDataTimer?.Cancel();
                    updateDataTimer = null;
                    ActivityController.Instance.RefreshActivityRedDotByClientType(ActivityClientType.DragonYearRedEnvelope);
                    CheckNeedShowToast();
                    RefreshTimer();
                });
            }
        }

        private Queue<MarqueeInfo> marqueeInfoQueue = new();
        public void GetNoticeDate()
        {
            marqueeInfoQueue.Clear();
            ActivityData activityData = ActivityController.Instance.GetAllActivityDataByType(ActivityClientType.DragonYearRedEnvelope)[0];
            NetworkManager.Instance.GetRedPacketMarquees(activityData.cfg.Id, (GetRedPacketMarqueesResponse getRedPacketMarqueesResponse) =>
            {
                AddToMarqueeInfoQueue(getRedPacketMarqueesResponse.MarqueeList);
            });

            //RepeatedField<MarqueeInfo> marqueeInfoListTest = new();
            //for (int i = 0; i < 100; i++)
            //{
            //    MarqueeInfo marqueeInfo = new()
            //    {
            //        ExpiredTime = (int)DataConvUtil.ServerTime + 60 * 60,
            //        Type = 1,
            //        Name = Configs.RandomClubName.GetConfigList()[Utility.GetRandomInt(0, Configs.RandomClubName.GetConfigList().Count - 1)].Name,
            //        Rank = Utility.GetRandomInt(1, 3),
            //        ServerId = Utility.GetRandomInt(1, 40),
            //    };
            //    marqueeInfoListTest.Add(marqueeInfo);
            //}
            //AddToMarqueeInfoQueue(marqueeInfoListTest);
        }
        public void AddToMarqueeInfoQueue(RepeatedField<MarqueeInfo> marqueeInfoList)
        {
            foreach (MarqueeInfo marqueeInfo in marqueeInfoList)
            {
                marqueeInfoQueue.Enqueue(marqueeInfo);
            }
        }
        public MarqueeInfo GetMarqueeInfo()
        {
            int nowTime = (int)DataConvUtil.ServerTime;
            while (marqueeInfoQueue.Count > 0)
            {
                MarqueeInfo marqueeInfo = marqueeInfoQueue.Dequeue();
                if (marqueeInfo.ExpiredTime > nowTime)
                {
                    return marqueeInfo;
                }
            }
            return null;
        }

    }





}