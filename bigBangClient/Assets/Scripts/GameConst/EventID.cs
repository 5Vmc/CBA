namespace BigBang
{
    public class EventID
    {
        public static string OnApplicationFocusTrue = "OnApplicationFocusTrue";

        public static string OnSpeedChange = "OnSpeedChange";
        public static string OnStrengthen = "OnStrengthen";
        public static string OnExpChanged = "OnExpChanged";     //经验改变事件

        public static string OnStrenthChanged = "OnStrenthChanged";   //实力变化
        public static string OnResourceChange = "OnResourceChange";

        public static string OnServerPushPackageChange = "OnServerPushPackageChange";//背包物品变化
        public static string OnBigBangRefresh = "OnBigBangRefresh";
        public static string OnUnlockInviteMatch = "OnUnlockInviteMatch"; //解锁邀请赛
        public static string OnInviteMatchRefresh = "OnInviteMatchRefresh";
        public static string OnBigBangStart = "OnBigBangStart";   //超训开启
        public static string OnSuperBigBang = "OnSuperBigBang";
        public static string OnBigBangPadPay = "OnBigBangPadPay";
        public static string OnBigBangResultClose = "OnBigBangResultClose";
        public static string OnTrainAllCompleted = "OnTrainAllCompleted";
        public static string OnCardUpgradeStar = "OnCardUpgradeStar";
        public static string OnCardRefreshData = "OnCardRefreshData";
        public static string OnSkillUISelectSkill = "OnSkillUISelectSkill";//特技UI选择
        public static string OnRemakeBigBangStartButton = "OnRemakeBigBangStartButton";//恢复开启按钮

        //点击学习中按钮OnClickTrainingBtn
        public static string OnClickTrainingBtn = "OnClickTrainingBtn";
        // 训练室解锁
        public static string OnUnlockSkillTrainRoom = "OnUnlockSkillTrainRoom";

        //招募池子信息变化
        public static string OnRecruitPoolRefresh = "OnRecruitPoolRefresh";
        public static string OnRecruitSuccess = "OnRecruitSuccess";

        // 初始化招募界面动画
        public static string InitRecruitUIModelAnim = "InitRecruitUIAnim";
        // 显示招募结果
        public static string ShowRecruitResult = "ShowRecruitResult";
        // 显示招募信息
        public static string RecruitUIShowInfo = "RecruitUIShowInfo";

        public static string OnLineupChangeSelectModel = "OnLineupChangeSelectModel";
        public static string OnRefreshInventoryAdapter = "OnRefreshInventoryAdapter";

        public static string OnSelectFormationTemplate = "OnSelectFormationTemplate";
        public static string OnChangeFormation = "OnChangeFormation";
        public static string OnUseFormationTemplate = "OnUseFormationTemplate";

        public static string OnOpenMyLastGamePad = "OnOpenMyLastGamePad";
        public static string OpenLeagueMyLastGamePad = "OpenLeagueMyLastGamePad";

        public static string OnShowChallengeFlag = "OnShowChallengeFlag";
        public static string OnHideChallengeFlag = "OnHideChallengeFlag";
        public static string OnChallengeTest = "OnChallengeTest";

        public static string OnFightEvent = "OnFightEvent";
        public static string OnShootOutPrepare = "OnShootOutPrepare";
        public static string OnShootOut = "OnShootOut";

        public static string OnRefreshFormationName = "OnRefreshFormationName";

        public static string OnRefreshEmail = "OnRefreshEmail";
        public static string OnReceiveEmailDetail = "OnReceiveEmailDetail";
        public static string OnRefreshTrainShop = "OnRefreshTrainShop";
        public static string OnRefreshDiamondShop = "OnRefreshDiamondShop";
        public static string OnRefreshGiftShop = "OnRefreshGiftShop";
        public static string OnRefreshItemShop = "onRefreshItemShop";

        public static string OnStudySkill = "OnStudySkill";
        public static string RefreshInventoryProp = "RefreshInventoryProp";

        public static string OnRefreshTaskUI = "OnRefreshTaskUI";
        public static string OnRefreshTaskProgressItem = "OnRefreshTaskProgressItem";

        //public static string OnRefreshSevenDayLoginRewardUI = "OnRefreshSevenDayLoginRewardUI";//刷新七天奖励显示
        public static string OnRefreshMonthSiginUI = "OnRefreshMoonSiginUI";

        public static string OnMainTaskItemSelected = "OnMainTaskItemSelected";

        public static string CheckGuide = "CheckGuide";

        public static string SetCourseTimeSucceed = "SetCourseTimeSucceed";
        public static string FightFormationUIClosed = "FightFormationUIClosed";
        public static string CancelSetFormation = "CancelSetFormation";
        public static string OnFormationSetted = "OnFormationSetted";

        //战斗事件
        public static string OnShoot = "OnShoot";
        public static string OnFirstHalfStart = "OnFirstHalfStart";
        public static string OnSecondHalfStart = "OnSecondHalfStart";
        public static string OnTimeStop = "OnTimeStop";
        public static string OnTimeStart = "OnTimeStart";
        public static string OnHalfTimeBreak = "OnHalfTimeBreak";
        public static string OnGoal = "OnGoal";//进球
        public static string OnPunishCard = "OnPunishCard";
        public static string OnPenalty = "OnPenalty";
        public static string OnExtraPrepareToStart = "OnExtraPrepareToStart";
        public static string OnExtraHalfBreak = "OnExtraHalfBreak";
        //战斗：伤停补时预约
        public static string OnFirstHalfInjuryForecast = "OnFirstHalfInjuryForecast";
        public static string OnSecondHalfInjuryForecast = "OnSecondHalfInjuryForecast";
        public static string OnExtraFirstHalfInjuryForecast = "OnExtraFirstHalfInjuryForecast";
        public static string OnExtraSecondHalfInjuryForecast = "OnExtraSecondHalfInjuryForecast";

        public static string OnFlagIn = "OnFlagIn";
        public static string OnFlagOut = "OnFlagOut";

        public static string OnRefreshGoods = "OnRefreshGoods";

        public static string OnClickWorldUIItem = "OnClickWorldUIItem";

        //显示新的国家
        public static string OnNewCountry = "OnNewCountry";


        public static string OnClickChangeServerBtn = "OnClickChangeServerBtn"; //换服务器

        //球员系统-解雇
        public static string OnClickWillFireMe = "OnClickWillFireMe";

        public static string OnClickFrameDebugButton = "OnClickFrameDebugButton";
        /// <summary>
        /// 玩家改名事件
        /// </summary>
        public static string OnPlayerHeadChange = "OnPlayerHeadChange";
        /// <summary>
        /// 刷新小红点
        /// </summary>
        public static string RefreshUIRedDot = "RefreshUIRedDot";
        /// <summary>
        /// 刷新大数值的小红点，大数值是后台一直在算的，所以这里的小红点要单独刷，目前用的是15秒更新一次。
        /// </summary>
        public static string RefreshBigBangUIRedDot = "RefreshBigBangUIRedDot";
        //登录后小红点数据算完的事件，首页接收
        //public static string OnHomeUIRedDotReady = "OnHomeUIRedDotReady";
        //底部导航小红点
        //public static string OnRefreshNavigationUIRedDot = "OnNavigationUIRedDotReady";
        /// <summary>
        /// 刷新界面，用于在界面之上的弹窗操作完成后通知下层界面刷新；下层界面要手动监听。
        /// </summary>
        public static string RefreshWindow = "RefreshWindow";

        #region 首充 月卡
        public static string OnGetFirstChargeRewardSucceed = "OnGetFirstChargeRewardSucceed";
        public static string OnRefreshMonthCard = "OnRefreshMonthCard";
        #endregion

        #region network mask
        public static string NETWORK_SENDING = "NETWORK_SENDING";
        public static string NETWORK_CALLBACK = "NETWORK_CALLBACK";
        #endregion

        #region 充值(与非热更部分对应)
        //开始发起充值
        public static string CHARGE_START = "CHARGE_START";
        //充值失败
        public static string CHARGE_FAIL = "CHARGE_FAIL";
        //充值成功
        public static string CHARGE_SUCCESS = "CHARGE_SUCCESS";
        #endregion

        //心跳超时
        public static string HEART_BEAT_OVERTIME = "HEART_BEAT_OVERTIME";

        //Quick初始化结束（可能成功或者失败）
        //public static string QUICK_INIT_END = "QUICK_INIT_END";
        //Quick登录成功
        public static string QUICK_LOGIN_SUCCESS = "QUICK_LOGIN_SUCCESS";
        //Quick登录失败
        public static string QUICK_LOGIN_FAIL = "QUICK_LOGIN_FAIL";

        //Quick 切换账号
        public static string QUICK_SWITCH_ACCOUNT = "QUICK_SWITCH_ACCOUNT";

        //Quick 注销账号
        public static string QUICK_LOGIN_OUT = "QUICK_LOGIN_OUT";

        public static string AutoBattleChangeLast = "AutoBattleChangeLast";
        public static string AutoBattleStop = "AutoBattleStop";

        public static string ClassicCountryUIOnClickCountryButton = "ClassicCountryUIOnClickCountryButton";
        public static string ClassicCountryUIOnClickClallengeButton = "ClassicCountryUIOnClickClallengeButton";

        //购买道具后的回调，刷新面板
        public static string ClassicShopUIItemBuy = "ClassicShopUIItemBuy";

        #region 悬赏任务

        public static string OnBountyTaskDataChange = "OnBountyTaskDataChange";
        public static string OnBountyTaskDataRefreshList = "OnBountyTaskDataRefreshList";
        public static string OnBountyTaskDataRefreshTopBox = "OnBountyTaskDataRefreshTopBox";

        #endregion

        #region 引导使用
        //点击了CardUI的卡牌
        public static string OnClickCardUICard = "OnClickCardUICard";
        //点击了数字藏品界面的卡牌
        public static string OnClickCollectionUICard = "OnClickCollectionUICard";
        //刷新数字藏品界面
        public static string RefreshCollectionUI = "RefreshCollectionUI";
        //点击了MailBoxUI的邮件
        public static string OnClickMailBoxUIMail = "OnClickMailBoxUIMail";
        //开始抽卡
        public static string OnStartRecruit = "OnStartRecruit";
        #endregion

        public static string OnClickArenaPadGotoFormationPad = "OnClickArenaPadGotoFormationPad";

        /// <summary>
        /// 竞技场信息刷新
        /// </summary>
        public static string OnArenaGetNewInfo = "OnArenaGetNewInfo";

        /// <summary>
        /// 服务端推送回来后刷新面板
        /// </summary>
        public static string Refresh_Normal_Task = "Refresh_Normal_Task";

        /// <summary>
        /// 球队升级
        /// </summary>
        public static string OnTeamlevelUp = "OnTeamlevelUp";

        public static string RefreshCardRecoverProperties = "RefreshCardRecoverProperties";

        public static string OnTriggerGuide2UIClose = "OnTriggerGuide2UIClose";

        public static string OnSeasonPassItemSetData = "OnSeasonPassItemSetData";

        public static string OnSeasonPassItemGetReward = "OnSeasonPassItemGetReward";

        public static string OnRefreshActivityTab = "OnRefreshActivityTab";

        /// <summary>
        /// 按下爬塔副本关卡按钮
        /// </summary>
        public static string OnClickFBTowerLevelItem = "OnClickFBTowerLevelItem";

        /// <summary>
        /// 爬塔副本选择了buff
        /// </summary>
        public static string OnClickFBTowerBuff = "OnClickFBTowerBuff";

        /// <summary>
        /// 爬塔副本领取累计星星奖励
        /// </summary>
        public static string OnClickFBTowerGetStarReward = "OnClickFBTowerGetStarReward";

        /// <summary>
        /// 爬塔副本服务器返回buff增加
        /// </summary>
        public static string AfterGetFBTowerBuff = "AfterGetFBTowerBuff";

        /// <summary>
        /// 爬塔副本完整数据推送（0点回刷新重置次数时）
        /// </summary>
        public static string AfterGetFBTowerData = "AfterGetFBTowerData";

        /// <summary>
        /// 爬塔副本花费殿堂荣誉后刷新
        /// </summary>
        public static string OnCostTowerHoner = "OnCostTowerHoner";

        /// <summary>
        /// 爬塔副本请求扫荡
        /// </summary>
        public static string OnTowerRaid = "OnTowerRaid";

        /// <summary>
        /// 正在显示的时间礼包时间到了
        /// </summary>
        public static string OnTimeGiftTimeEnd = "OnTimeGiftTimeEnd";

        #region 百分大战

        /// <summary>
        /// 点击了百分大战布阵界面，下方的卡牌
        /// </summary>
        public static string OnClickHundredCardItemDown = "OnClickHundredCardItemDown";

        /// <summary>
        /// 点击了百分大战布阵界面，上方的卡牌
        /// </summary>
        public static string OnClickHundredCardItemUp = "OnClickHundredCardItemUp";

        /// <summary>
        /// 当前界面与 stage 不匹配
        /// </summary>
        public static string OnHundredStageMismatch = "OnHundredStageMismatch";

        /// <summary>
        /// 百分大战获得了自己的新信息
        /// </summary>
        public static string OnHundredGetMineInfo = "OnHundredGetMineInfo";

        /// <summary>
        /// 百分大战需要刷新应援信息
        /// </summary>
        public static string OnHundredNeedRefreshGuess = "OnHundredNeedRefreshGuess";
        /// <summary>
        /// 百分大战需要关闭应援界面
        /// </summary>
        public static string OnHundredNeedCloseGuess = "OnHundredNeedCloseGuess";
        /// <summary>
        /// 百分大战应援某一队后
        /// </summary>
        public static string AfterHundredGuessSupport = "AfterHundredGuessSupport";

        /// <summary>
        /// 百分大战数据界面点击了某一届
        /// </summary>
        public static string OnClickHundredDataUISeasonItem = "OnClickHundredDataUISeasonItem";
        /// <summary>
        /// 百分大战数据界面点击了某一场战斗
        /// </summary>
        public static string OnClickHundredDataUIFightItem = "OnClickHundredDataUIFightItem";

        #endregion

        #region 圣诞树

        /// <summary>
        /// 节日任务的服务器状态发生改变
        /// </summary>
        public static string OnFestivalTaskDataChange = "OnFestivalTaskDataChange";

        #endregion

        #region 元旦签到（祈愿）

        /// <summary>
        /// 设置物品到许愿签成功
        /// </summary>
        public static string OnNewYearSignSelectItemSet = "OnNewYearSignSelectItemSet";

        #endregion

        #region 成就

        /// <summary>
        /// 领取成就奖励后
        /// </summary>
        public static string OnAfterGetAchievementReward = "OnAfterGetAchievementReward";

        #endregion

        #region 新联赛

        /// <summary>
        /// 点击了恢复界面的卡牌
        /// </summary>
        public static string OnClickFormationRecoverCardItem = "OnClickFormationRecoverCardItem";

        #endregion

        #region 龙年红包

        /// <summary>
        /// 发送龙年红包后
        /// </summary>
        public static string OnAfterSendRedEnvlope = "OnAfterSendRedEnvlope";

        /// <summary>
        /// 打开龙年红包后
        /// </summary>
        public static string OnAfterOpenRedEnvlope = "OnAfterOpenRedEnvlope";

        /// <summary>
        /// 收到龙年红包跑马灯推送后
        /// </summary>
        public static string OnAfterReceiveRedEnvlopeNotify = "OnAfterReceiveRedEnvlopeNotify";

        #endregion

        #region 2024全明星

        /// <summary>
        /// 点击了全明星布阵界面卡牌
        /// </summary>
        public static string OnClickAllStarFormationCardItem = "OnClickAllStarFormationCardItem";

        /// <summary>
        /// 刷新AllStarHomePad
        /// </summary>
        public static string RefreshAllStarHomePad = "RefreshAllStarHomePad";

        /// <summary>
        /// 全明星招募切换了区域
        /// </summary>
        public static string OnRecruitChangeArea = "OnRecruitChangeArea";

        #endregion

        #region 2024季后赛总决赛竞猜

        /// <summary>
        /// 刷新竞猜相关界面
        /// </summary>
        public static string RefreshPlayoffFinalsGuessUI = "RefreshPlayoffFinalsGuessUI";
        /// <summary>
        /// 选择了季后赛总决赛竞猜的MVP球员
        /// </summary>
        public static string OnSelectPlayoffFinalsGuessMVPPlayerItem = "OnSelectPlayoffFinalsGuessMVPPlayerItem";
        /// <summary>
        /// 点击了单场比赛的幸运数字
        /// </summary>
        public static string OnSelectPlayoffFinalsGuessNumberBallItem = "OnSelectPlayoffFinalsGuessNumberBallItem";

        #endregion

        #region 2024端午节赛龙舟

        /// <summary>
        /// 刷新赛龙舟相关界面
        /// </summary>
        public static string RefreshDragonBoatFestivalUI = "RefreshDragonBoatFestivalUI";

        /// <summary>
        /// 助力了某个龙舟队
        /// </summary>
        public static string OnUpDragonBoatFestivalTeam = "OnUpDragonBoatFestivalTeam";

        #endregion

    }
}
