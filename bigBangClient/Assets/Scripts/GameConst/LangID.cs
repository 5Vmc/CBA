/// <summary>
/// 自动生成代码
/// 代码生成脚本:LangIDCreator.cs
/// 模板文件:LangIDTemplate.txt
/// </summary>
namespace Utils
{
    //LangID取值范围[1000-1999]
    public enum LangID
    {
        //解锁训练
        UnlockTrainText = 1001,
        //升级
        UpgradeText = 1002,
        //解锁
        UnlockText = 1003,
        //立刻完成超能训练
        ClearBigBangCDContent = 1004,
        //邀请赛胜利
        InviteMatchResultSuccessText = 1005,
        //邀请赛文本
        InviteContentText = 1006,
        //邀请赛标题
        InviteTitleText = 1007,
        //离线收益翻倍
        OfflineVideoReward = 1008,
        //开启
        EnableText = 1009,
        //超能准备
        BigBangReayText = 1010,
        //还需累计经验
        NeedExpText = 1011,
        //预估剩余时间
        ResidueTimeText = 1012,
        //经验产出效率
        ExpOutputText = 1013,
        //下一次超能训练
        NextBigBangText = 1014,
        //启动已准备就绪!
        ReadyText = 1015,
        //健康
        HealthText = 1016,
        //轻伤
        MinorInjuryText = 1017,
        //重伤
        SeriousInjury = 1018,
        //剩余多久可以抽到卡
        RecruitFloorsLeftCount = 1019,
        //未获得
        NotYetAchieve = 1020,
        //已拥有
        Achieve = 1021,
        //抽一次
        RecruitOnce = 1022,
        //抽十次
        RecruitTen = 1023,
        //使用
        UseText = 1024,
        //打开
        OpenText = 1025,
        //合成
        CompoundText = 1026,
        //{time}过期
        ExpirationTimeText = 1027,
        //{playerName}掌握了{skillName}
        MasterskillText = 1028,
        //已选 <color=#E2E9EF>{value}</color>
        HasBennSelected = 1029,
        //保存阵型
        SaveNewFormationTemp = 1030,
        //MM月dd日 HH:mm
        DateString = 1031,
        //第{value}轮
        RoundText = 1032,
        //进球数
        GoalsScoredText = 1033,
        //助攻数
        AssistsNumberText = 1034,
        //抢断数
        StealCountText = 1035,
        //零封数
        ZeroKeeperText = 1036,
        //联赛
        LeagueNameText = 1037,
        //杯赛
        CupNameText = 1038,
        //冠军赛
        ChampionshipNameText = 1039,
        //世俱杯
        WorldCompetitionNameText = 1040,
        //联赛赛制
        LeagueIntroduceTitle1 = 1041,
        //联赛赛制介绍
        LeagueIntroduceValue1 = 1042,
        //排名规则
        LeagueIntroduceTitle2 = 1043,
        //排名规则介绍
        LeagueIntroduceValue2 = 1044,
        //升降级规则
        LeagueIntroduceTitle3 = 1045,
        //升降级规则介绍
        LeagueIntroduceValue3 = 1046,
        //冠军赛资格
        LeagueIntroduceTitle4 = 1047,
        //冠军赛资格介绍
        LeagueIntroduceValue4 = 1048,
        //禁赛规则
        LeagueIntroduceTitle5 = 1049,
        //禁赛规则介绍
        LeagueIntroduceValue5 = 1050,
        //介绍
        IntroduceText = 1051,
        //奖励
        RewardText = 1052,
        //比赛名称
        CompitionNameText = 1053,
        //我的比赛{state}
        MyGameText = 1054,
        //未进行
        NotImplementedText = 1055,
        //已进行
        UnderwayText = 1056,
        //已淘汰
        EliminatedText = 1057,
        //MM月dd日
        DateString2 = 1058,
        //{win}胜{deuce}平{faild}负
        StatisticsText = 1059,
        //排名
        RankText = 1060,
        //新的时间已设定
        TheNewTimeHasBeenSet = 1061,
        //回收的物品包含橙色及以上品质，确认要回收吗
        RecycleConfirmText = 1062,
        //我方使用
        HomeFormationText = 1063,
        //对方使用
        AwayFormationText = 1064,
        //杯赛赛制
        CupIntroduceTitle1 = 1065,
        //杯赛赛制介绍
        CupIntroduceValue1 = 1066,
        //排名规则
        CupIntroduceTitle2 = 1067,
        //排名规则介绍
        CupIntroduceValue2 = 1068,
        //加时赛规则
        CupIntroduceTitle3 = 1069,
        //加时赛规则介绍
        CupIntroduceValue3 = 1070,
        //点球大战
        CupIntroduceTitle4 = 1071,
        //点球大战规则介绍
        CupIntroduceValue4 = 1072,
        //禁赛规则
        CupIntroduceTitle5 = 1073,
        //禁赛规则介绍
        CupIntroduceValue5 = 1074,
        //比赛胜利
        WinText = 1075,
        //比赛失败
        FailText = 1076,
        //平局
        DeuceText = 1077,
        //杯赛奖励
        CupRewardText = 1078,
        //删除
        DeleteEmail = 1079,
        //领取
        ReceiveEmail = 1080,
        //前
        Ago = 1081,
        //后过期
        OverDue = 1082,
        //联赛比赛日
        LeagueDayTxt = 1083,
        //杯赛比赛日
        CupDayTxt = 1084,
        //联赛战绩
        LeagueRecordTxt = 1085,
        //杯赛战绩
        CupRecordTxt = 1086,
        //经验
        Exp = 1087,
        //钻石
        Diamond = 1088,
        //钞票
        Euro = 1089,
        //第{value}天
        DayTxt = 1090,
        //进行中
        InProgressTxt = 1091,
        //心愿球员列表限制
        RecruitAppointLimit = 1092,
        //董事会
        BoardTxt = 1093,
        //我
        MeTxt = 1094,
        //秘书
        ClerkTxt = 1095,
        //欢迎您加入{name}俱乐部。\n董事会正在等待与您进行一次简短的交流。
        Guide1Txt = 1096,
        //在开始训练之前，先见见球队里的球员吧。\n他们非常想和你聊一聊。
        Guide2Txt = 1097,
        //看来您与董事会已经充分交流过了。\n今天正好有一场热身赛，也许您\n可以先看看球员的表现。
        Guide3Txt = 1098,
        //这真是一场 ……嗯……很棒的比赛。您已经很清楚球队的实力了，是吧？\n我们还是立刻开始训练吧，他们已经在<color=#1A7CC1>训练场</color>等您了。
        Guide4Txt = 1099,
        //球员们正在训练场等您
        WaitingForTxt = 1100,
        //也许我们应该在<color=#1A7CC1>联赛新赛季</color>开始前尽快提升一下训练水平。\n就这么简单，升级训练……经验提升……解锁新的训练！
        Guide5Txt = 1101,
        //请先升级第一个训练项目
        UpgradeFirstTxt = 1102,
        //确认
        ConfirmTxt = 1103,
        //前往
        GoTxt = 1104,
        //网络连接超时
        NetworkConnectionTimeout = 1105,
        //更换球员号码提示
        ChangeNumberConfirm = 1106,
        //月签到奖励
        MonthRewardTxt = 1107,
        //是否花费10钻石补签？
        MonthRewardCostTxt = 1108,
        //可回收成星晶
        RecycleToStarText = 1109,
        //可回收成金币
        RecycleToEruoText = 1110,
        //拥有
        HasCount = 1111,
        //普通道具
        NormalProp = 1112,
        //宝箱类
        BoxProp = 1113,
        //球员碎片
        SplinterProp = 1114,
        //材料道具
        MaterialProp = 1115,
        //钻石描述
        DiamondDesc = 1116,
        //金币描述
        EruoDesc = 1117,
        //经验描述
        ExpDesc = 1118,
        //球员描述
        GuysDesc = 1119,
        //资源
        ResourcesDesc = 1120,
        //球员
        Guys = 1121,
        //已拥有则转为球员碎片
        PropRecycleTip = 1122,
        //未知队伍
        UnknownClub = 1123,
        //球员的开始最好位置
        BestPositionTitle = 1124,
        //属性
        PropertiesTxt = 1125,
        //战中引导
        Guide6Txt = 1126,
        //战中引导按钮
        Guide6BtnTxt = 1127,
        //级
        LvTxt = 1128,
        //小时
        HourTxt = 1129,
        //秒
        SecondTxt = 1130,
        //分钟
        MinuteTxt = 1131,
        //翻{value}倍
        MakeAdditionTxt = 1132,
        //对阵
        VSTxt = 1133,
        //厘米
        CmTxt = 1134,
        //千克
        KgTxt = 1135,
        //版本号
        VersionTxt = 1136,
        //您的球队已经初具雏形，现在开始你的环球之旅吧
        Guide7Txt = 1137,
        //您已获得了参加联赛的资格。
        Guide8Txt = 1138,
        //你可以在此界面查看敌我双方的信息并根据敌方信息对阵容做出调整，准备好迎接你的第一场比赛吧
        Guide9Txt = 1139,
        //你需要在此模式里挑战来自五大洲的球队，挑战模式会提高可观的经验产出。点击挑战正式开启你的环球之旅
        Guide10Txt = 1140,
        //我们的球探找到了一名优秀的球员，快来看看吧
        Guide11Txt = 1141,
        //充值失败
        ChargeErrorTitle = 1142,
        //未满8周岁的用户不能付费
        ChargeError1 = 1143,
        //8周岁以上未满16周岁的用户，单次充值金额不得超过50元人民币，每月充值金额累计不得超过200元人民币
        ChargeError2 = 1144,
        //16周岁以上的未成年用户，单次充值金额不得超过100元人民币，每月充值金额累计不得超过400元人民币。
        ChargeError3 = 1145,
        //你已击败所有{name}俱乐部
        ChallengeCompletedTxt = 1146,
        //解锁第4个训练项目后开放挑战玩法
        UnlockChallengeTxt = 1147,
        //联赛分组已完成，快去看看吧！
        Guide12Txt = 1148,
        //轮次
        Shootout = 1149,
        //挑战赛通关第三关开放招募
        NeedToUnlockRecruit = 1150,
        //联赛排名奖励标题
        LeagueRewardRankTitleTxt = 1151,
        //联赛排名奖励内容
        LeagueRewardRankContentTxt = 1152,
        //杯赛排名奖励标题
        CupRewardRankTitleTxt = 1153,
        //杯赛排名奖励内容
        CupRewardRankContentTxt = 1154,
    }

    //ErrorID取值范围[9000-9999]
    public enum ErrorID
    {
        //无
        None = 9000,
        //系统错误
        SystemError = 9001,
        //经验不足
        ExpNotEnough = 9002,
        //钞票不足
        MoneyNotEnough = 9003,
        //钻石不足
        DiamondNotEnough = 9004,
        //球员升级失败
        CardUpgradeFail = 9005,
        //球员合成失败
        MergeCardFail = 9006,
        //升星道具不足
        UpgradeGoodsNotEnough = 9007,
        //不能升星
        CanNotUpgradeStar = 9008,
        //每日招募次数达到上限
        RecruitDayMax = 9009,

        //项目未解锁
        UnLock = 9011,
        //该球员已满星
        ErrorAlreadyFull = 9012,
        //心愿球员每个位置只能选择1名球员
        RecruitAppointPositionRepeat = 9013,
        //心愿栏已满
        RecruitAppointFull = 9014,
        //你已实现今日心愿
        RecruitAppointAlreadyHit = 9015,
        //特技重复解锁
        UnLockSkillRepeat = 9016,
        //特技解锁条件不满足
        UnLockSkillConditionsNotMet = 9017,
        //没有项目可强化
        NoItemStrengthen = 9018,
        //特技学习没有选卡
        SkillTrainRoomSelectCardNull = 9019,
        //特技学习没有选特技
        SkillTrainRoomSelectSkillNull = 9020,
        //没有空闲的房间
        SkillTrainRoomNoIdle = 9021,
        //该特技正在被其他球员学习
        SkillTrainSelectSkillDoTraining = 9022,
        //球员品质不足，无法学习该特技
        SkillTrainSelectSkillCanNotTrain = 9023,
        //已掌握此项特技
        SkillTrainSelectSkillHaveBeenTrain = 9024,
        //该球员正在学习中
        SkillTrainSelectCardDoTraining = 9025,
        //该球员品质不足，无法学习该特技
        SkillTrainSelectCardCanNotTrain = 9026,
        //已掌握此项特技
        SkillTrainSelectCardHaveBeenTrain = 9027,
        //已达最小数量
        InventoryUseMinNumber = 9028,
        //你已拥有该球员
        AlreadyHavePlayer = 9029,
        //碎片不足
        DebirsNotEnough = 9030,
        //阵容交换，不能上场
        LineupCardCanNotFight = 9031,
        //请先选择回收材料
        ChooseMaterial = 9032,
        //时间已过期
        TimeHasExpired = 9033,
        //只有主场球队才可以修改比赛时间
        ChangeTimeError = 9034,
        //战斗时获取阵容失败
        FightFormationError = 9035,
        //保存阵型模板超过99
        FormationTemplateMax = 9036,
        //阵型模板名字重复
        FormationTemplateRepeatName = 9037,
        //赛季筹备中
        CompitionIsNotReady = 9038,
        //该商品今日已售罄
        SellOutToday = 9039,
        //未能成功购买，请检测网络或联系客服
        BuyError = 9040,
        //今日已无挑战次数
        ChallengeNoChance = 9041,
        //阵型模板名字超过20字
        FormationTemplateNameTooLong = 9042,
        //该商品已售罄
        SellOut = 9043,
        //名称包含敏感词，请重新输入
        IllegalName = 9044,
        //名称超出上限，请重新输入
        NameOverflow = 9045,
        //名称重复，请重新输入
        NameRepetition = 9046,
        //请输入俱乐部名称
        NameEmpty = 9047,
        //主客场及客场备选队服主颜色不能完全相同
        SameJersey = 9048,
        //请先解锁一个训练项目
        UnlockOneTrainItem = 9049,
        //请先达成解锁要求
        UnlockRequirements = 9050,
        //请先完成任务
        PleaseFinishTheTask = 9051,
        //招募点不足但钻石足够
        RecruitCanDiamond = 9052,
        //守门员不能为空
        GoalKeeperCannotBeNull = 9053,
        //比赛期间最多只能更换5次球员
        CanOnlySwitchCardFiveTime = 9054,
        //改名卡不足
        RenameCardIsNotEnough = 9055,
        //名字不合规
        ReviseNameIsNotQualified = 9056,
        //联赛未开启上次赛事提示
        LeagueNotOpenTip = 9057,
        //联赛开启后没有上次赛事
        NoLastMatchMessageTip = 9058,
        //暂无比赛预览
        NoGamePreviewTemporarily = 9059,
        //暂时没有比赛信息
        NoMyGameTemporarily = 9060,
        //解锁10个训练项目后开启赛事
        NoTenTrainCanUnclock = 9061,
        //联赛正在筹备中
        LeagueNotReadyTip = 9062,
        //完成一届联赛后开放杯赛
        CupFirstOpenNotReady = 9063,
        //杯赛正在筹备中
        CupNotReadyTip = 9064,
        //临近比赛时间小于180分钟时，不能再做调整
        ChangeTimeErrorTip = 9065,
        //没有更多球员了
        NoMoreBackupCard = 9066,
        //候选室球员无需换位
        NoNeedToExchangeBackup = 9067,
        //暂无候选人员
        NoReserveCard = 9068,
        //每场比赛最多更换五名替补
        FiveExchangeOneGameAtMost = 9069,
        //解锁10个训练项目后开启竞技场
        NoTenTrainCanUnclockArena = 9070,
        //不能升阶
        CanNotUpgradeQuality = 9071,

        //招募点不足(普通)
        RecruitPointNotEnoughNormal = -100,
        //招募点不足(活动)
        RecruitPointNotEnoughActivity = -101,
    }
}