//auto generate at 10/28/24 16:40:11

namespace BigBang
{
    public class ProcID
    {

        //-------基础模块-------
        //错误提示

        //-------战斗-------
        //开始观看比赛
        public static string WatchFight = "fight_module.cs_watchFight";
        //获取比赛帧序列
        public static string FetchFightFrames = "fight_module.cs_fetchFightFrames";
        //获取当前观看的开始帧
        public static string GetWatchBeginFrame = "fight_module.cs_getWatchBeginFrame";
        //点球大战开始前准备阶段 和服务器同步时间
        public static string GetShootoutPrepareLeftTime = "fight_module.cs_getShootoutPrepareLeftTime";
        //点球切换球员
        public static string ShootoutExchangeCard = "fight_module.cs_shootoutExchangeCard";
        //比赛中更新阵容
        public static string FightExchangeFormation = "fight_module.cs_fightExchangeFormation";
        //更新战斗数据
        //下发战斗开始信息
        //通知战报报告已存好
        //通知战斗重新计算帧

        //-------登录模块-------
        //登录请求
        public static string Login = "cs_login";
        //心跳请求
        public static string Heart = "cs_heart";
        //实名认证
        public static string Realname = "cs_realname";
        //拉取所有角色
        public static string FetchAllPlayers = "cs_fetchAllPlayers";
        //创建角色
        public static string CreatePlayer = "cs_createPlayer";
        //进入游戏
        public static string EnterGame = "cs_enterGame";
        //检查名字有效性
        public static string CheckName = "cs_checkName";

        //-------成就模块-------
        //检查成就
        public static string CheckPlayerAchievement = "achievement_module.cs_requestCheckPlayerAchievement";
        //领取成就奖励
        public static string ReceiveAchievement = "achievement_module.cs_receiveAchievement";
        //清除所有成就
        public static string ClearAllAchievement = "achievement_module.cs_clearAllAchievement";
        //更新成就

        //-------活动模块-------
        //领取奖励
        public static string Receive = "activity_module.cs_receiveReward";
        //月签到
        public static string MonthSign = "activity_module.cs_receiveMonthSign";
        //领取投篮游戏奖励
        public static string ReceiveShootGameReward = "activity_module.cs_receiveShootGameReward";
        //领取礼包码
        public static string ReceiveGiftCode = "activity_module.cs_receiveGiftCode";
        //领取积分奖励
        public static string ReceivePointReward = "activity_module.cs_receivePointReward";
        //领取小额充值
        public static string ReceivePayMicro = "activity_module.cs_receivePayMicro";
        //领取每日福利
        public static string ReceiveDailyGift = "activity_module.cs_receiveDailyGift";
        //获得排行列表
        public static string GetRankList = "activity_module.cs_getRankList";
        //获得排行球员详情
        public static string GetRankCardDetail = "activity_module.cs_getRankCardDetail";
        //获得排行球队详情
        public static string GetRankTeamDetail = "activity_module.cs_getRankTeamDetail";
        //限时领取体力
        public static string GetEnergyTimeLimit = "activity_module.cs_getEnergyTimeLimit";
        //开启节日宝箱
        public static string OpenFestivalBox = "activity_module.cs_openFestivalBox";
        //领取节日任务
        public static string GetFestivalTaskReward = "activity_module.cs_getFestivalTaskReward";
        //设置许愿签
        public static string SetWishSign = "activity_module.cs_setWishSign";
        //领取许愿签
        public static string GetWishSignReward = "activity_module.cs_getWishSignReward";
        //发红包
        public static string SendRedPacket = "activity_module.cs_sendRedPacket";
        //抢红包
        public static string SnatchRedPacket = "activity_module.cs_snatchRedPacket";
        //点赞
        public static string LikeRedPacket = "activity_module.cs_likeRedPacket";
        //获得红包信息
        public static string GetRedPacketInfo = "activity_module.cs_getRedPacketInfo";
        //获得红包滚动公告
        public static string GetRedPacketMarquees = "activity_module.cs_getRedPacketMarquees";
        //获得红包领取记录
        public static string GetRedPacketLogs = "activity_module.cs_getRedPacketLogs";
        //获得全明星信息
        public static string GetAllStarInfo = "activity_module.cs_getAllStarInfo";
        //选择全明星阵营
        public static string PickAllStarArea = "activity_module.cs_pickAllStarArea";
        //同步全明星数据
        public static string SyncAllStar = "activity_module.cs_syncAllStar";
        //获得全明星排行
        public static string GetAllStarRank = "activity_module.cs_getAllStarRank";
        //领取全明星战力奖励
        public static string GetAllStarStrengthReward = "activity_module.cs_getAllStarStrengthReward";
        //掷色子玩旅行棋盘
        public static string ThrowTravelDice = "activity_module.cs_throwTravelDice";
        //获得总决赛竞猜信息
        public static string GetFinalsGuessInfo = "activity_module.cs_getFinalsGuessInfo";
        //总决赛竞猜
        public static string FinalsGuess = "activity_module.cs_finalsGuess";
        //领取总决赛竞猜奖励
        public static string GetFinalsGuessReward = "activity_module.cs_getFinalsGuessReward";
        //获得赛龙舟信息
        public static string GetDragonBoatInfo = "activity_module.cs_getDragonBoatInfo";
        //选择支持赛龙舟队伍
        public static string PickDragonBoat = "activity_module.cs_pickDragonBoat";
        //增加赛龙舟米数
        public static string AddDragonBoatMeters = "activity_module.cs_addDragonBoatMeters";
        //领取赛龙舟里程奖励
        public static string GetDragonBoatMetersReward = "activity_module.cs_getDragonBoatMetersReward";
        //活动更新
        //积分活动信息更新
        //积分活动信息更新
        //条件触发礼包信息更新
        //节日任务更新，更新一批次任务
        //刷新节日任务，刷新一个活动的全部任务
        //发送新的红包滚动公告

        //-------竞技场模块-------
        //获取竞技场信息
        public static string ArenaInfo = "arena_module.cs_arenaInfo";
        //更换对手
        public static string ChangeOpponent = "arena_module.cs_changeOpponent";
        //发起挑战
        public static string Battle = "arena_module.cs_battle";
        //获得挑战记录
        public static string GetBattleLog = "arena_module.cs_getBattleLog";
        //竞技场排名
        public static string ArenaRank = "arena_module.cs_arenaRank";
        //排行榜查看详情
        public static string ArenaRankDetail = "arena_module.cs_arenaRankDetail";
        //领取每日奖励
        public static string CollectDailyAward = "arena_module.cs_collectDailyAward";
        //购买挑战次数
        public static string BuyEntries = "arena_module.cs_buyEntries";
        //竞技场商店购买物品
        public static string BuyGoodsFromArena = "arena_module.cs_buyGoodsFromArena";
        //更新竞技场模块

        //-------球员模块-------
        //球员升星
        public static string CardUpgradeStar = "card_module.cs_cardUpgradeStar";
        //球员进阶
        public static string CardUpgradeQuality = "card_module.cs_cardUpgradeQuality";
        //球员一键进阶
        public static string CardAdvance = "card_module.cs_cardAdvance";
        //球员升级技能
        public static string CardUpgradeSkill = "card_module.cs_cardUpgradeSkill";
        //球员信息同步
        public static string SynchCardInfo = "card_module.cs_synchCardInfo";
        //球员的解雇
        public static string CardFire = "card_module.cs_cardFire";
        //球员的合成
        public static string MergeCard = "card_module.cs_mergeCard";
        //球员恢复
        public static string Recover = "card_module.cs_recover";
        //招募
        public static string Recruit = "card_module.cs_recruit";
        //调整心愿球员
        public static string ChangeAppointCard = "card_module.cs_changeAppointCard";
        //领取招募奖励
        public static string CollectRecruitReward = "card_module.cs_collectRecruitReward";
        //解锁特技
        public static string UnlockSkill = "card_module.cs_unlockSkill";
        //解锁训练坑位
        public static string UnlockSkillTrainRoom = "card_module.cs_unlockSkillTrainRoom";
        //训练室开始训练
        public static string BeginTrainSkill = "card_module.cs_beginTrainSkill";
        //清除训练室cd
        public static string ClearTrainRoomCD = "card_module.cs_clearTrainRoomCD";
        //选择号码更改号码
        public static string ChangePlayerCardNumber = "card_module.cs_changePlayerCardNumber";
        //两个球员更换号码
        public static string ExchangePlayerCardNumber = "card_module.cs_exchangePlayerCardNumber";
        //球员升级
        public static string CardUpgradeLevel = "card_module.cs_cardUpgradeLevel";
        //装备升级
        public static string CardUpgradeJersey = "card_module.cs_cardUpgradeJersey";
        //装备突破
        public static string CardBreakJersey = "card_module.cs_cardBreakJersey";
        //从背包中上架出售球员道具
        public static string SaleCard = "card_module.cs_saleCard";
        //查询数字藏品的球员道具
        public static string GetPropCards = "card_module.cs_getPropCards";
        //从数字藏品使用球员道具
        public static string UsePropCard = "card_module.cs_usePropCard";
        //更新card
        //刷新招募信息
        //获得一张卡
        //训练室完成
        //更新对应卡片号码
        //招募次数更新

        //-------开发指令模块-------
        //指令
        public static string Develop = "develop_module.cs_developCommand";

        //-------邮件模块-------
        //获取服务器时间
        public static string FetchServerTime = "email_module.cs_fetchServerTime";
        //系统邮件测试
        public static string EmailSysTest = "email_module.cs_sysEmailTest";
        //读取邮件
        public static string EmailRead = "email_module.cs_read";
        //领取邮件
        public static string EmailReceive = "email_module.cs_receive";
        //领取全部邮件
        public static string EmailReceiveAll = "email_module.cs_receiveAll";
        //删除邮件
        public static string EmailDelete = "email_module.cs_delete";
        //删除全部邮件
        public static string EmailDeleteAll = "email_module.cs_deleteAll";
        //发送引导邮件
        public static string GuideEmail = "email_module.cs_guideEmail";
        //新邮件通知
        //邮件更新

        //-------战斗模块-------
        //保存阵容模板
        public static string SaveFormationTemp = "fight_module.cs_saveFormationTemp";
        //保存战术模板
        public static string SaveTacticsTemp = "fight_module.cs_saveTacticsTemp";
        //升级战术
        public static string UpgradeTactics = "fight_module.cs_upgradeTactics";
        //修改阵容模板名称
        public static string ChangeFormationTempName = "fight_module.cs_changeFormationTempName";
        //修改战术模板名称
        public static string ChangeTacticsTempName = "fight_module.cs_changeTacticsTempName";
        //删除阵容模板
        public static string DelFormationTemp = "fight_module.cs_delFormationTemp";
        //删除战术模板
        public static string DelTacticsTemp = "fight_module.cs_delTacticsTemp";
        //保存阵容
        public static string SaveFormation = "fight_module.cs_saveFormation";
        //获取默认阵容
        public static string GetDefaultFormation = "fight_module.cs_getDefaultFormationRequest";
        //获取比赛结果信息
        public static string GetFightReport = "fight_module.cs_getFightReport";
        //下发模块数据

        //-------新手模块-------
        //领取新手目标奖励
        public static string GetReward = "novice_module.cs_getReward";
        //新手目标更新

        //-------开关模块-------
        //开关模块更新
        //下发开关变动更新

        //-------包裹模块-------
        //分解道具
        public static string DelGoods = "package_module.cs_delGoodsList";
        //打开包裹
        public static string OpenBox = "package_module.cs_openBox";
        //设置道具新获得标签
        public static string SetGoodsAsOld = "package_module.cs_setGoodsAsOld";
        //合成新的道具
        public static string MergeSplinter = "package_module.cs_mergeSplinter";
        //更新package
        //更新资源
        //更新道具
        //打开自选宝箱
        public static string OpenOptionalBox = "package_module.cs_openOptionalBox";

        //-------PVE模块-------
        //获取挑战地图数据
        public static string GetChallengeMapData = "pve_module.cs_getChallengeMapData";
        //获取挑战剧情章节数据
        public static string GetChallengeHeroChapterData = "pve_module.cs_getChallengeHeroChapterData";
        //获取挑战数据
        public static string GetChallengeData = "pve_module.cs_getChallengeData";
        //发起挑战
        public static string ChallengeStart = "pve_module.cs_startChallenge";
        //发起快速挑战
        public static string ChallengeStartFast = "pve_module.cs_startFastChallenge";
        //新手挑战
        public static string GuideChallenge = "pve_module.cs_guideChallenge";
        //测试挑战
        public static string DevChallengeStart = "pve_module.cs_devStartChallenge";
        //发起剧情挑战
        public static string ChallengeStartHero = "pve_module.cs_startChallengeHero";
        //领取章节宝箱奖励
        public static string CollectChapterBoxReward = "pve_module.cs_collectChapterBoxReward";
        //发起爬塔挑战
        public static string StartTowerChallenge = "pve_module.cs_startTowerChallenge";
        //选择爬塔Buff
        public static string SelectTowerBuff = "pve_module.cs_selectTowerBuff";
        //爬塔扫荡
        public static string RaidTower = "pve_module.cs_raidTower";
        //领取累计星星奖励
        public static string CollectTowerStarReward = "pve_module.cs_collectTowerStarReward";
        //重置爬塔
        public static string ResetTower = "pve_module.cs_resetTower";
        //挑战数据变化
        //更新单个章节模块
        //更新PVE模块

        //-------PVP模块-------
        //获取积分榜信息
        public static string GetLeagueScorebar = "pvp_module.cs_getLeagueScorebar";
        //获取赛程
        public static string GetLeagueCourse = "pvp_module.cs_getLeagueCourse";
        //获得进球榜
        public static string GetLeagueCardRank = "pvp_module.cs_getLeagueCardRank";
        //报名联赛
        public static string GetLeagueSignUp = "pvp_module.cs_leagueSignUp";
        //请求联赛信息
        public static string GetLeagueData = "pvp_module.cs_getLeagueData";
        //请求联赛历届战绩
        public static string GetLeagueHistory = "pvp_module.cs_getLeagueHistory";
        //请求联赛巅峰榜单
        public static string GetLeagueChampionRank = "pvp_module.cs_getLeagueChampionRank";
        //领取联赛结算奖励
        public static string ReceiveLeagueSettleReward = "pvp_module.cs_receiveLeagueSettleReward";
        //请求赛事信息
        public static string GetCompitionData = "pvp_module.cs_getCompitionData";
        //请求修改时间
        public static string ChangeCourseTime = "pvp_module.cs_changeCourseTime";
        //请求下一场赛程数据
        public static string GetGamePreviewData = "pvp_module.cs_getGamePerviewData";
        //首页获取赛程
        public static string GetMainUIMatch = "pvp_module.cs_getMainUIMatch";
        //领取比赛奖励
        public static string ReceiveCompitionReward = "pvp_module.cs_receiveCompitionReward";
        //报名百分大战
        public static string SignUpHundred = "pvp_module.cs_signUpHundred";
        //获取百分大战赛程
        public static string GetHundredCourse = "pvp_module.cs_getHundredCourse";
        //获取百分大战往届赛季战绩
        public static string GetHundredHof = "pvp_module.cs_getHundredHof";
        //请求赛程队伍数据
        public static string GetCourseTeamData = "pvp_module.cs_getCourseTeamData";
        //获取百分大战应援数据
        public static string GetHundredSupport = "pvp_module.cs_getHundredSupport";
        //应援百分大战
        public static string SupportHundred = "pvp_module.cs_supportHundred";
        //获得百分大战历届比赛数据
        public static string GetHundredHistoryCourse = "pvp_module.cs_getHundredHistoryCourse";
        //更新联赛冠军数
        //杯赛冠军数
        //更新PVP模块

        //-------排行模块-------
        //获得全服排名
        public static string GetAllRankList = "rank_module.cs_getAllRankList";

        //-------商城模块-------
        //购买钻石
        public static string PurchaseDiamondSuccess = "shop_module.cs_purchaseDiamondSuccess";
        //购买礼包
        public static string PurchaseGiftSuccess = "shop_module.cs_purchaseGiftSuccess";
        //训练商城
        public static string TrainShop = "shop_module.cs_trainShop";
        //道具商城
        public static string GameItemShop = "shop_module.cs_gameItemShop";
        //购买月卡
        public static string PurchaseMonthCardSuccess = "shop_module.cs_purchaseMonthCardSuccess";
        //获得首充奖励
        public static string GetFirstChargeReward = "shop_module.cs_getFirstChargeReward";
        //领取月卡每日奖励
        public static string GetMonthCardReward = "shop_module.cs_getMonthCardReward";
        //查看NFT藏品
        public static string GetNFTGoods = "shop_module.cs_getNFTGoods";
        //购买体力
        public static string GetEnergy = "shop_module.cs_getEnergy";
        //消费支付订单号
        public static string ConsumeOrderNo = "shop_module.cs_consumeOrderNo";
        //下发商城变化
        //下发钻石购买成功
        //下发月卡购买成功
        //下发礼包购买成功
        //下发礼包购买成功2用于发送静默的通知

        //-------任务模块-------
        //领取任务奖励
        public static string CollectTaskReward = "task_module.cs_collectTaskReward";
        //领取日常/周常任务宝箱奖励
        public static string CollectTaskBoxReward = "task_module.cs_collectTaskBoxReward";
        //开始悬赏任务
        public static string StartBountyTask = "task_module.cs_startBountyTask";
        //领取悬赏任务奖励
        public static string CollectBountyTaskReward = "task_module.cs_collectBountyTaskReward";
        //领取悬赏任务宝箱奖励
        public static string CollectBountyTaskBoxReward = "task_module.cs_collectBountyTaskBoxReward";
        //更新任务状态
        //批量更新任务状态
        //循环任务信息
        //新增悬赏任务信息
        //删除某个悬赏任务
        //悬赏任务信息
        //更新任务模块
        //更新普通任务的完成情况
        //更新普通任务完成得任务组
        //删除某个task
        //更新任务活动点
        //更新领取过的宝箱信息

        //-------训练模块-------
        //增加经验
        //同步训练事件
        public static string SyncTrainEvents = "train.cs_syncTrainEvents";
        //清除Bigbang的cd
        public static string ClearBigbangCD = "train.cs_clearBigbangCD";
        //获取邀请赛信息
        public static string FetchInviteMatchInfo = "train.cs_fetchInviteMatchInfo";
        //邀请比赛
        public static string DoInviteMatch = "train.cs_doInviteMatch";
        //获取邀请奖励
        public static string DoInviteMatchReward = "train.cs_doInviteMatchReward";
        //获取离线奖励
        public static string DoOfflineReward = "train.cs_doOfflineReward";
        //更新训练信息
        //刷新bigbanginfo

        //-------玩家模块-------
        //完成引导
        public static string FinishGuide = "guide_module.cs_finishGuide";
        //更改球队名字
        public static string ReviseName = "cs_reviseName";
        //注销账号
        public static string DeletePlayer = "cs_deletePlayer";
        //倾程加密
        public static string QingChengEncrypt = "cs_qingChengEncrypt";
        //更新球队实力
        public static string GetStrength = "cs_getStrength";
        //更新玩家信息
        //更新球队名字
        //每日刷新
        //踢出
        //开启GM

    }
}

