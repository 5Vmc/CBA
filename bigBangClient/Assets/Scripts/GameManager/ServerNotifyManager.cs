using Babu;
using Babu.SDK;
using BigBang.UI;
using Protocol;
using System;
using System.Linq;
using UnityEngine;
using Utils;

namespace BigBang
{
    public class ServerNotifyManager : Singleton<ServerNotifyManager>
    {
        public ServerNotifyManager()
        {
        }
        public void Init()
        {
            ServerNotificationCenter.Instance.Register(this);
        }
        #region notify

        [ServerNotification("sc_updatePVEInfo")]
        public void SyncClassicCityData(UpdatePVEInfoNotify data)
        {
            ClassicManager.Instance.ResetPassData(data.Chapters);
            HeroManager.Instance.ResetPassData(data.Chapters);
            FBTowerController.Instance.UnPack(data);
        }

        [ServerNotification("sc_dailyRefresh")]
        public void DailyRefresh(DailyRefreshNotify data)
        {
            //todo:零点清空
            //ClassicManager.Instance.UpdatePVERedDot(data.Chapters);
            ClassicManager.Instance.ClearChallengeCount();
            HeroManager.Instance.ClearChallengeCount();
        }
        [ServerNotification("sc_updatePVPInfo")]
        public void UpdatePVPInfo(UpdatePVPInfoNotify data)
        {
            Player.PVPManager.UnPack(data);
        }


        [ServerNotification("sc_updateAchievement")]
        public void UpdateAchievement(AchievementsNotify data)
        {
            Player.AchievementManager.UnPack(data.Achievements);
        }

        [ServerNotification("sc_errorTips")]
        public void OnErrorTips(ErrorNotify error)
        {
            Debug.Log("on error tips " + error.ErrorId);
            Tips.PopError((ErrorID)error.ErrorId);
        }

        [ServerNotification("sc_updateTrainInfo")]
        void OnUpdateTrainInfo(TrainInfoNotify data)
        {
            Player.TrainManager.UnPack(data);
        }

        //同步今天的清除cd 时间
        [ServerNotification("sc_refreshBigBangInfo")]
        void OnRefreshBigBangInfo(RefreshBigbangInfoNotify data)
        {
            Player.TrainManager.BigBangController.UnPack(data.BigBang);
        }

        [ServerNotification("sc_updatePlayerInfo")]
        public void UpdateData(BasicPlayerInfoNotify data)
        {
            Player.UnPack(data);
        }

        //打开 GM
        [ServerNotification("sc_openGM")]
        public void OpenGM(OpenGMNotify data)
        {
            Player.isDeveloper = true;
        }

        [ServerNotification("sc_updateCardInfo")]
        public void UpdateCardInfo(ModuleCardInfoNotify data)
        {
            Player.CardManager.UnPack(data);
        }

        [ServerNotification("sc_notifyShopModule")]
        public void UpdateShopInfo(ShopModuleNotify data)
        {
            Player.ShopManager.UnPack(data);
        }

        [ServerNotification("sc_notifyRecruitCountInfo")]
        public void UpdateRecruitCount(RecruitCountInfoNotify data)
        {
            Player.CardManager.RecruitController.TotalRecruitCount = data.TotalRecruitCount;
        }

        //获得一个球员卡
        [ServerNotification("sc_createOneCard")]
        public void CreateOneCard(CreateOneCardNotify data)
        {
            Player.CardManager.AddNewCard(data.Card.CardId, data.Card);
        }

        //更新package
        [ServerNotification("sc_refreshPackageInfo")]
        public void RefreshPackageInfo(RefreshPackageInfoNotify data)
        {
            Player.PackageManager.UnPack(data.PackageInfo);
        }

        //更新资源
        [ServerNotification("sc_refreshResource")]
        public void RefreshResource(RefreshResourceNotify data)
        {
            Player.PackageManager.RefreshResource(data.Money, data.Diamond, data.Energy, data.EnergyLastUpdateTime);
        }

        [ServerNotification("sc_addMinExpReward")]
        public void AddMinExpReward(AddMinExpRewardInfoNotify data)
        {
            Player.TrainManager.AddExp(data.AddExp.ToBigNumber());
        }

        //更新道具
        [ServerNotification("sc_refreshGoods")]
        public void RefreshGoods(RefreshGoodsNotify data)
        {
            Player.PackageManager.RefreshGoods(data.Goods.ToList());
        }

        // [ServerNotification("sc_FightBeginData")]
        // public void UpdateFightBeginData(FightBeginDataNotify data)
        // {
        //     Player.FightManager.FightDataController.UpdateFightBeginData(data);
        // }

        // [ServerNotification("sc_updateFightFrameData")]
        // public void UpdateFightFrameData(FightFrameDataNotify data)
        // {

        //     Player.FightManager.FightDataController.UpdateFightFrameData(data);
        // }


        [ServerNotification("sc_fightRecalculateFrame")]
        public void NotifyRecalculateFrame(FightRecalculateframeNotify data)
        {
            Player.FightManager.FightDataController.NotifyRecalculateFrame(data);
        }


        [ServerNotification("sc_FightResultData")]
        public void UpdateFightResultData(FightBeginDataNotify data)
        {
            Player.FightManager.FightDataController.UpdateFightBeginData(data);
        }

        [ServerNotification("sc_FightReportReady")]
        public void NotifyFightReportReady(FightReportReadyNotify data)
        {
            if (data.IsReady)
            {
                Player.FightManager.FightDataController.NotifyReportReady(data);
            }
        }

        //同步招募信息
        [ServerNotification("sc_refreshRecruitInfo")]
        void OnRefreshRecruitInfo(RefreshRecruitInfoNotify data)
        {
            Player.CardManager.RecruitController.UnPack(data.RecruitController);
        }

        //训练室完成
        [ServerNotification("sc_trainRoomComplete")]
        void OnTrainRoomComplete(TrainRoomCompleteNotify data)
        {
            Player.CardManager.SkillController.OnTrainRoomComplete(data.RoomId, data.SkillId, data.CardId);
        }

        [ServerNotification("sc_notifyPlayerCardNumberChanged")]
        void OnChangePlayerCardNumber(PlayerCardNumberChangedNotify data)
        {
            Player.CardManager.ChangeCardNumber(data.CardId, data.PlayerCardNumber);
        }

        [ServerNotification("sc_notifyFightModuleInfo")]
        void OnUpdateFightInfo(ModuleFightInfoNotify data)
        {
            Player.FightManager.UnPack(data.ModuleFightInfo);
        }

        [ServerNotification("sc_notifyNewEmail")]
        void OnNotifyUnReadEmail(ModuleEmailNewNotify data)
        {
            Player.EmailManager.Add(data);
        }

        [ServerNotification("sc_notifyAllEmails")]
        void OnNotifyAllEmails(ModuleEmailAllNotify data)
        {
            Player.EmailManager.UnPack(data);
        }

        [ServerNotification("sc_notifyChallenge")]
        void OnNotifyChallenge(ChallengeDataNotify data)
        {
            Player.ChallengeManager.UnPack(data);
        }
        [ServerNotification("sc_updateTaskInfo")]
        void OnUpdateTaskInfo(ModuleTaskInfoNotify moduleTaskInfo)
        {
            Player.TaskManager.UnPack(moduleTaskInfo);
            BountyTaskManager.Instance.UpdateBountyTaskInfo(moduleTaskInfo.BountyTaskInfo);
        }
        [ServerNotification("sc_updateBatchTask")]
        void OnUpdateBatchTask(TaskInfoBatchNotify task)
        {
            Player.TaskManager.BatchUpdateTask(task);
        }
        [ServerNotification("sc_updateNormalCompletedTasks")]
        void OnUpdateNormalCompletedTasks(NormalCompletedTaskInfoNotify normalCompletedTask)
        {
            Player.TaskManager.NormalTasks.UpdateCompletedTasks(normalCompletedTask);
        }
        [ServerNotification("sc_updateNormalCompletedTaskGroups")]
        void OnUpdateNormalCompletedTaskGroups(NormalCompletedTaskGroupInfoNotify normalCompletedTaskGroupInfo)
        {
            Player.TaskManager.NormalTasks.UpdateCompletedTaskGroups(normalCompletedTaskGroupInfo);
        }
        [ServerNotification("sc_removeTask")]
        void OnRemoveTask(RemoveTaskNotify data)
        {
            Player.TaskManager.RemoveTask(data.TaskId);
        }
        [ServerNotification("sc_updateCyclicTaskPoint")]
        void OnUpdateCyclicTaskPoint(UpdateCyclicTaskPointNotify data)
        {
            Player.TaskManager.UpdateCyclicTaskPoint(data.Type, data.Point);
        }
        [ServerNotification("sc_updateCyclicTaskCollectedBoxes")]
        void OnUpdateCyclicTaskCollectedBoxes(UpdateCyclicTaskCollectedBoxesNotify data)
        {
            Player.TaskManager.UpdateCyclicTaskCollectedBoxes(data.Type, data.CollectedBoxes);
        }
        [ServerNotification("sc_updateCyclicTaskInfo")]
        void OnUpdateCyclicTaskInfo(CyclicTaskInfoNotify data)
        {
            Player.TaskManager.UpdateCyclicTask(data);
        }
        [ServerNotification("sc_updateOnoffInfo")]
        void UpdateOnoffInfo(OnoffInfoNotify data)
        {
            Player.OnoffManager.UnPack(data);
        }
        [ServerNotification("sc_notifyOnoffChanged")]
        void NotifyOnoffChanged(OnoffChangedNotify data)
        {
            Player.OnoffManager.OnOnoffChanged(data);
        }
        [ServerNotification("sc_notifyReviseName")]
        void ReviseName(ReviseNameNotify data)
        {
            Player.ReviseName(data.Name);
        }
        [ServerNotification("sc_notifyLeagueTrophyCount")]
        void RefreshLeagueTrophyCount(LeagueTrophyCountNotify data)
        {
            Player.PVPManager.UnPack(data);
        }
        [ServerNotification("sc_notifycupTrophyCount")]
        void RefreshCupTrophyCount(CupTrophyCountNotify data)
        {
            Player.PVPManager.UnPack(data);
        }
        [ServerNotification("sc_updateArenaInfo")]
        void UpdateArenaInfoNotify(UpdateArenaInfoNotify data)
        {
            Player.BattleManager.AddArenaInfo(data.Info);
        }

        //新手目标
        [ServerNotification("sc_updateNoviceTask")]
        void RefreshNoviceTask(UpdateNoviceTaskNotify data)
        {
            //  Debug.Log("xxxx RefreshNoviceTask=" + data.Days);
            if (data.Days > 0)
            {
                Player.NoviceTaskManager.Days = data.Days;
            }
            Player.NoviceTaskManager.UpdateData(data.Tasks.ToList<NoviceTaskInfo>());
        }

        [ServerNotification("sc_notifyPurchaseDiamondSuccess")]
        void RefreshPurchaseDiamond(PurchaseDiamondSuccessNotify data)
        {
            EventManager.Instance.Dispatch(EventID.CHARGE_SUCCESS);
            EventManager.Instance.Dispatch(PurchaseServiceManager.Event.PurchaseResult, PurchaseServiceManager.Error.Succ, data.ShopItemId);
            Player.ShopManager.ReportChargeSuccess(data.ShopItemId, data.OrderNo);
        }

        //充值月卡
        [ServerNotification("sc_notifyPurchaseMonthCardSuccess")]
        void RefreshPurchaseMonthCard(PurchaseMonthCardSuccessNotify data)
        {
            EventManager.Instance.Dispatch(EventID.CHARGE_SUCCESS);
            EventManager.Instance.Dispatch(PurchaseServiceManager.Event.PurchaseResult, PurchaseServiceManager.Error.Succ, data.ShopItemId);
            Player.ShopManager.ReportChargeSuccess(data.ShopItemId, data.OrderNo);
        }
        //充值礼包
        [ServerNotification("sc_notifyPurchaseGiftSuccess")]
        void RefreshPurchaseGift(PurchaseGiftSuccessNotify data)
        {
            EventManager.Instance.Dispatch(EventID.CHARGE_SUCCESS);
            EventManager.Instance.Dispatch(PurchaseServiceManager.Event.PurchaseResult, PurchaseServiceManager.Error.Succ, data.ShopItemId);
            Player.ShopManager.ReportChargeSuccess(data.ShopItemId, data.OrderNo);
        }
        [ServerNotification("sc_notifyPurchaseGiftSuccess2")]
        void RefreshPurchaseGiftNoShow(PurchaseGiftSuccess2Notify data)
        {
            EventManager.Instance.Dispatch(EventID.CHARGE_SUCCESS);
            Player.ShopManager.ReportChargeSuccess(data.ShopItemId, data.OrderNo);
        }

        #region 悬赏任务

        //刷新悬赏
        [ServerNotification("sc_updateBountyTaskInfo")]
        void UpdateBountyTaskInfo(BountyTaskInfoNotify bountyTaskInfoNotify)
        {
            BountyTaskManager.Instance.UpdateBountyTaskInfo(bountyTaskInfoNotify);
        }

        //增加悬赏
        [ServerNotification("sc_addBountyTask")]
        void AddBountyTaskInfo(AddBountyTaskNotify addBountyTaskNotify)
        {
            BountyTaskManager.Instance.AddBountyTaskInfo(addBountyTaskNotify.Tasks);
        }

        //减少悬赏
        [ServerNotification("sc_removeBountyTask")]
        void RemoveBountyTaskInfo(RemoveBountyTaskNotify removeBountyTaskNotify)
        {
            BountyTaskManager.Instance.RemoveBountyTaskInfo(removeBountyTaskNotify.TaskId);
        }

        #endregion

        #region 未使用的推送接口

        //当抽卡等获得英雄时，剧情挑战可能会有新开的关卡，如果剧情挑战有完整数据且增量修改数据，应当使用此接口开增量修改。目前，每次打开剧情界面会重新去服务器取数据，所以暂时不需要使用此接口。
        [ServerNotification("sc_updateChapterInfo")]
        void UpdateChapterInfo(UpdateChapterInfoNotify updateChapterInfoNotify)
        {

        }

        #endregion

        #region 充值活动

        [ServerNotification("sc_notifySignActivityModule")]
        void UpdateSignActivityInfo(SignActivityModuleNotify data)
        {
            Player.ActivityManager.UnPack(data);
        }

        [ServerNotification("sc_notifyActivityPointInfo")]
        void UpdateActivityPointInfo(ActivityPointInfoNotify data)
        {
            ActivityController.Instance.UpdatePointList(data.PointList);
        }

        [ServerNotification("sc_notifyPayMicroGiftInfo")]
        void UpdatePayMicroGiftInfo(PayMicroGiftInfoNotify data)
        {
            ActivityController.Instance.UpdatePayMicroList(data.PayMicroList);
        }

        [ServerNotification("sc_notifyActivityPayTriggerInfo")]
        void UpdateActivityPayTriggerInfo(ActivityPayTriggerInfoNotify data)
        {
            TimeGiftController.Instance.Update(data.PayTriggerList, data.NewGiftId, false);
        }

        #endregion

        #region 账号被挤掉

        [ServerNotification("sc_kickOff")]
        public void KickOff(KickOffNotify data)
        {
            LoginManager.Instance.IsBackByKickOff = true;
            LoginManager.Instance.BackToLogin();
            if (data.Reason == 1)
            {
                UIController.Instance.OpenWindow<DialogueBoxUI>(new DialogueBoxUIProperties("您的账号已在异地登录！\n有其它设备登录了您的账号，当前设备已下线。如果这不是您本人操作，那么您的密码可能已经泄露，请尽快修改密码。", "确定", () =>
                {

                }));
            }
        }

        #endregion

        #region 圣诞树

        [ServerNotification("sc_updateFestivalTask")]
        void UpdateFestivalTask(UpdateFestivalTaskNotify data)
        {
            ActivityController.Instance.RefreshFeativalTaskData(data.Tasks);//更新部分已有任务的状态
        }

        [ServerNotification("sc_refreshFestivalTaskNotify")]
        void RefreshFestivalTask(RefreshFestivalTaskNotify data)
        {
            ActivityController.Instance.RebuildFeativalTaskData(data.Tasks);//任务全部重新发送
        }

        #endregion

        #region 龙年红包

        [ServerNotification("sc_notifyRedPacketMarquees")]
        void RedPacketMarquees(RedPacketMarqueesNotify data)
        {
            RedEnvlopeManager.Instance.AddToMarqueeInfoQueue(data.MarqueeList);
            EventManager.Instance.Dispatch(EventID.OnAfterReceiveRedEnvlopeNotify);
        }

        #endregion

        #endregion

    }
}
