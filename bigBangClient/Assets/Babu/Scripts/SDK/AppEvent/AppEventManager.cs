using System;
using System.Collections.Generic;
using UnityEngine;

namespace Babu.SDK
{
    class AppEventManager : BabuSingleton<AppEventManager>
    {
        public class Event
        {
            public const string LOGIN = "LOGIN";                       // 登录
            public const string LOGOUT = "LOGOUT";                     // 登出
            public const string CREATE_ROLE = "CREATE_ROLE";           // 创角
            public const string ENTER_GAME = "ENTER_GAME";             // 角色进入游戏
            public const string LEAVE_GAME = "LEAVE_GAME";             // 角色离开游戏
            public const string COMPLETE_GUIDE = "COMPLETE_GUIDE";     // 完成新手引导
            public const string MONEY_CHANGED = "MONEY_CHANGED";       // 货币修改
            public const string RECHARGE = "RECHARGE";                 // 充值
            public const string UPDATE_ONLINE_TIME = "ONLINE_TIME";    // 更新在线时长

            public const string REWARD_VIDEO_AD_PREPARE_SUCC = "REWARD_VIDEO_AD_PREPARE_SUCC";             // 激励广告准备成功
            public const string REWARD_VIDEO_AD_PREPARE_FAILED = "REWARD_VIDEO_AD_PREPARE_FAILED";         // 激励广告准备失败
            public const string REWARD_VIDEO_AD_BUTTON_SHOWED = "REWARD_VIDEO_AD_BUTTON_SHOWED";           // 激励广告按钮显示
            public const string REWARD_VIDEO_AD_BUTTON_CLICKED = "REWARD_VIDEO_AD_BUTTON_CLICKED";         // 激励广告按钮点击
            public const string REWARD_VIDEO_AD_OPENED = "REWARD_VIDEO_AD_OPENED";                         // 激励广告被打开
            public const string REWARD_VIDEO_AD_CLOSED = "REWARD_VIDEO_AD_CLOSED";                         // 激励广告被关闭
            public const string REWARD_VIDEO_AD_ERROR = "REWARD_VIDEO_AD_ERROR";                           // 激励广告出错
            public const string REWARD_VIDEO_AD_CLICKED = "REWARD_VIDEO_AD_CLICKED";                       // 激励广告被点击
            public const string REWARD_VIDEO_AD_REWARDED = "REWARD_VIDEO_AD_REWARDED";                     // 激励广告被奖励

            public const string BANNER_AD_PREPARE_SUCC = "BANNER_AD_PREPARE_SUCC";                         // 横幅广告准备成功
            public const string BANNER_AD_PREPARE_FAILED = "BANNER_AD_PREPARE_FAILED";                     // 横幅广告准备失败
            public const string BANNER_AD_OPENED = "BANNER_AD_OPENED";                                     // 横幅广告被点击
            public const string BANNER_AD_CLOSED = "BANNER_AD_CLOSED";                                     // 横幅广告点击之后关闭

            public const string STAGE_ENTER = "STAGE_ENTER"; //进入关卡
            public const string STAGE_PASS = "STAGE_PASS"; //胜利
            public const string STAGE_FAIL = "STAGE_FAIL"; //失败
        }

        public enum LoginType
        {
            STAND_ALONE
        };

        private Dictionary<string, object> _args = new Dictionary<string, object>();
        private string _curAccountId = string.Empty;
        private string _curUserId = string.Empty;
        private int _curProfessionId = -1;

        private List<AppEventDispatcher> _appEventDispatchers = new List<AppEventDispatcher>();

        public void AddDispatcher(AppEventDispatcher dispatcher)
        {
            Debug.Log("Add Dispatcher: " + dispatcher.getDispatcherName());
            _appEventDispatchers.Add(dispatcher);
        }

        public void Login(string accountId, LoginType loginType, string deviceId, Dictionary<string, object> ext = null)
        {
            _curAccountId = accountId;

            _args.Clear();
            _args.Add("event", Event.LOGIN);
            _args.Add("account_id", accountId);
            _args.Add("login_type", loginType);
            _args.Add("device_id", deviceId);
            AddExt(ext);
            DispachEvent();
        }

        public void Logout(Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.LOGOUT);
            AddExt(ext);
            DispachEvent();

            _curAccountId = string.Empty;
        }

        public void CreateRole(string userId, string rolename, int profession, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.CREATE_ROLE);
            _args.Add("user_id", userId);
            _args.Add("rolename", rolename);
            _args.Add("profession", profession);
            AddExt(ext);
            DispachEvent();
        }

        public void EnterGame(string userId, string rolename, int profession, int level, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.ENTER_GAME);
            _args.Add("user_id", userId);
            _args.Add("rolename", rolename);
            _args.Add("profession", profession);
            _args.Add("level", level);
            AddExt(ext);
            DispachEvent();

            _curUserId = userId;
            _curProfessionId = profession;
        }

        public void LeaveGame(int onlineTime, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.LEAVE_GAME);
            _args.Add("online_time", onlineTime);
            AddExt(ext);
            DispachEvent();

            _curUserId = string.Empty;
            _curProfessionId = -1;
        }

        public void CompleteGuide(string guideId, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.COMPLETE_GUIDE);
            _args.Add("guide_id", guideId);
            AddExt(ext);
            DispachEvent();
        }

        public void ChangeMoney(string moneyType, int old, int delta, int current, string reason, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.MONEY_CHANGED);
            _args.Add("money_type", moneyType);
            _args.Add("old", old);
            _args.Add("delta", delta);
            _args.Add("current", current);
            _args.Add("reason", reason);
            AddExt(ext);
            DispachEvent();
        }

        public void Recharge(int price, string currencyCode, string orderId, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.RECHARGE);
            _args.Add("price", price);
            _args.Add("currency_code", currencyCode);
            _args.Add("order_id", orderId);
            AddExt(ext);
            DispachEvent();
        }

        public void UpdateOnlineTime(int seconds, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.UPDATE_ONLINE_TIME);
            _args.Add("online_time", seconds);
            AddExt(ext);
            DispachEvent();
        }

        public void RewardVideoAdPrepareSucc(Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.REWARD_VIDEO_AD_PREPARE_SUCC);
            AddExt(ext);
            DispachEvent();
        }

        public void RewardVideoAdPrepareFiled(int errorId, string desc, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.REWARD_VIDEO_AD_PREPARE_FAILED);
            _args.Add("error_id", errorId);
            _args.Add("desc", desc);
            AddExt(ext);
            DispachEvent();
        }

        public void RewardVideoAdButtonShowed(string from, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.REWARD_VIDEO_AD_BUTTON_SHOWED);
            _args.Add("from", from);
            AddExt(ext);
            DispachEvent();
        }

        public void RewardVideoAdButtonClicked(string from, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.REWARD_VIDEO_AD_BUTTON_CLICKED);
            _args.Add("from", from);
            AddExt(ext);
            DispachEvent();
        }

        public void RewardVideoAdOpened(string from, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.REWARD_VIDEO_AD_OPENED);
            _args.Add("from", from);
            AddExt(ext);
            DispachEvent();
        }

        public void RewardVideoAdClosed(string from, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.REWARD_VIDEO_AD_CLOSED);
            _args.Add("from", from);
            AddExt(ext);
            DispachEvent();
        }

        public void RewardVideoAdError(string from, int errorId, string desc, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.REWARD_VIDEO_AD_ERROR);
            _args.Add("from", from);
            _args.Add("error_id", errorId);
            _args.Add("desc", desc);
            AddExt(ext);
            DispachEvent();
        }

        public void RewardVideoAdClicked(string from, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.REWARD_VIDEO_AD_CLOSED);
            _args.Add("from", from);
            AddExt(ext);
            DispachEvent();
        }

        public void RewardVideoAdRewarded(string from, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.REWARD_VIDEO_AD_REWARDED);
            _args.Add("from", from);
            AddExt(ext);
            DispachEvent();
        }

        public void BannerAdPrepareSucc(Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.BANNER_AD_PREPARE_SUCC);
            AddExt(ext);
            DispachEvent();
        }

        public void BannerAdPrepareFiled(int errorId, string desc, Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.BANNER_AD_PREPARE_FAILED);
            _args.Add("error_id", errorId);
            _args.Add("desc", desc);
            AddExt(ext);
            DispachEvent();
        }

        public void BannerAdOpened(Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.BANNER_AD_OPENED);
            AddExt(ext);
            DispachEvent();
        }

        public void BannerAdClosed(Dictionary<string, object> ext = null)
        {
            _args.Clear();
            _args.Add("event", Event.BANNER_AD_CLOSED);
            AddExt(ext);
            DispachEvent();
        }

        public void StageEnter(int stageId, Dictionary<string, object> args = null)
        {
            Debug.Log("App Event: " + Event.STAGE_ENTER);
            _args.Clear();
            _args.Add("event", Event.STAGE_ENTER);
            _args.Add("stageId", stageId);
            AddExt(args);
            DispachEvent();
        }

        public void StagePass(int stageId, float battleTime, Dictionary<string, object> args = null)
        {
            _args.Clear();
            _args.Add("event", Event.STAGE_PASS);
            _args.Add("stageId", stageId);
            _args.Add("battleTime", battleTime);
            AddExt(args);
            DispachEvent();
        }

        public void StageFail(int stageId, float battleTime, Dictionary<string, object> args = null)
        {
            _args.Clear();
            _args.Add("event", Event.STAGE_FAIL);
            _args.Add("stageId", stageId);
            _args.Add("battleTime", battleTime);
            AddExt(args);
            DispachEvent();
        }

        public void CustomEvent(Dictionary<string, object> args)
        {
            _args.Clear();
            AddExt(args);
            DispachEvent();
        }

        private void DispachEvent()
        {
            foreach (var dispatcher in _appEventDispatchers)
            {
                try
                {
                    dispatcher.Dispatch(_args);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Dispatcher: {dispatcher.getDispatcherName()}, Catch Exception: " + e.Message);
                }
            }
            _args.Clear();
        }

        private void AddExt(Dictionary<string, object> ext)
        {
            if (_curAccountId != string.Empty && _args.ContainsKey("account_id") == false)
            {
                _args.Add("account_id", _curAccountId);
            }

            if (_curUserId != string.Empty && _args.ContainsKey("user_id") == false)
            {
                _args.Add("user_id", _curUserId);
            }

            if (_curProfessionId != -1)
            {
                _args.Add("profession_id", _curProfessionId);
            }

            if (ext != null)
            {
                foreach (var iter in ext)
                {
                    _args.Add(iter.Key, iter.Value);
                }
            }
        }
    }
}
