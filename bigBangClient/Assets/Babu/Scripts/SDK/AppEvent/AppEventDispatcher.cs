using System.Collections.Generic;

namespace Babu.SDK
{
    abstract class AppEventDispatcher
    {
        public abstract string getDispatcherName();

        public void Dispatch(Dictionary<string, object> args)
        {
            string eventId = args["event"] as string;
            switch (eventId)
            {
                case AppEventManager.Event.LOGIN: OnLogin(args); break;
                case AppEventManager.Event.LOGOUT: OnLogout(args); break;
                case AppEventManager.Event.CREATE_ROLE: OnCreateRole(args); break;
                case AppEventManager.Event.ENTER_GAME: OnEnterGame(args); break;
                case AppEventManager.Event.LEAVE_GAME: OnLeaveGame(args); break;
                case AppEventManager.Event.COMPLETE_GUIDE: OnComplateGuide(args); break;
                case AppEventManager.Event.MONEY_CHANGED: OnMoneyChanged(args); break;
                case AppEventManager.Event.RECHARGE: OnRecharge(args); break;
                case AppEventManager.Event.UPDATE_ONLINE_TIME: OnUpdateOnlineTime(args); break;

                case AppEventManager.Event.REWARD_VIDEO_AD_PREPARE_SUCC: OnRewardVideoAdPrepareSucc(args); break;
                case AppEventManager.Event.REWARD_VIDEO_AD_PREPARE_FAILED: OnRewardVideoAdPrepareFiled(args); break;
                case AppEventManager.Event.REWARD_VIDEO_AD_BUTTON_SHOWED: OnRewardVideoAdButtonShowed(args); break;
                case AppEventManager.Event.REWARD_VIDEO_AD_BUTTON_CLICKED: OnRewardVideoAdButtonClicked(args); break;
                case AppEventManager.Event.REWARD_VIDEO_AD_OPENED: OnRewardVideoAdOpened(args); break;
                case AppEventManager.Event.REWARD_VIDEO_AD_CLOSED: OnRewardVideoAdClosed(args); break;
                case AppEventManager.Event.REWARD_VIDEO_AD_ERROR: OnRewardVideoAdError(args); break;
                case AppEventManager.Event.REWARD_VIDEO_AD_CLICKED: OnRewardVideoAdClicked(args); break;
                case AppEventManager.Event.REWARD_VIDEO_AD_REWARDED: OnRewardVideoAdRewarded(args); break;

                case AppEventManager.Event.BANNER_AD_PREPARE_SUCC: OnBannerAdPrepareSucc(args); break;
                case AppEventManager.Event.BANNER_AD_PREPARE_FAILED: OnBannerAdPrepareFiled(args); break;
                case AppEventManager.Event.BANNER_AD_OPENED: OnBannerAdOpened(args); break;
                case AppEventManager.Event.BANNER_AD_CLOSED: OnBannerAdClosed(args); break;

                case AppEventManager.Event.STAGE_ENTER: OnStageEnter(args); break;
                case AppEventManager.Event.STAGE_PASS: OnStagePass(args); break;
                case AppEventManager.Event.STAGE_FAIL: OnStageFail(args); break;
                default: OnCustomEvent(args); break;
            }
        }

        protected virtual void OnLogin(Dictionary<string, object> args)
        {
        }

        protected virtual void OnLogout(Dictionary<string, object> args)
        {
        }

        protected virtual void OnCreateRole(Dictionary<string, object> args)
        {
        }

        protected virtual void OnEnterGame(Dictionary<string, object> args)
        {
        }

        protected virtual void OnLeaveGame(Dictionary<string, object> args)
        {
        }

        protected virtual void OnComplateGuide(Dictionary<string, object> args)
        {
        }

        protected virtual void OnMoneyChanged(Dictionary<string, object> args)
        {
        }

        protected virtual void OnRecharge(Dictionary<string, object> args)
        {
        }

        protected virtual void OnUpdateOnlineTime(Dictionary<string, object> args)
        {
        }

        protected virtual void OnRewardVideoAdPrepareSucc(Dictionary<string, object> args)
        {
        }

        protected virtual void OnRewardVideoAdPrepareFiled(Dictionary<string, object> args)
        {
        }

        protected virtual void OnRewardVideoAdButtonShowed(Dictionary<string, object> args)
        {
        }

        protected virtual void OnRewardVideoAdButtonClicked(Dictionary<string, object> args)
        {
        }

        protected virtual void OnRewardVideoAdOpened(Dictionary<string, object> args)
        {
        }

        protected virtual void OnRewardVideoAdClosed(Dictionary<string, object> args)
        {
        }

        protected virtual void OnRewardVideoAdError(Dictionary<string, object> args)
        {
        }

        protected virtual void OnRewardVideoAdClicked(Dictionary<string, object> args)
        {
        }

        protected virtual void OnRewardVideoAdRewarded(Dictionary<string, object> args)
        {
        }

        protected virtual void OnBannerAdPrepareSucc(Dictionary<string, object> args)
        {
        }

        protected virtual void OnBannerAdPrepareFiled(Dictionary<string, object> args)
        {
        }

        protected virtual void OnBannerAdOpened(Dictionary<string, object> args)
        {
        }

        protected virtual void OnBannerAdClosed(Dictionary<string, object> args)
        {
        }

        protected virtual void OnCustomEvent(Dictionary<string, object> args)
        {
        }

        protected virtual void OnStageEnter(Dictionary<string, object> args)
        {

        }

        protected virtual void OnStagePass(Dictionary<string, object> args)
        {

        }

        protected virtual void OnStageFail(Dictionary<string, object> args)
        {

        }
    }
}
