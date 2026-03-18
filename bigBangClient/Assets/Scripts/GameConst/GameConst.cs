//using Babu;

using System.Collections.Generic;

namespace BigBang
{
    public class LocalSaveID
    {
        public const string CHALLENGE_MAP = "CHALLENGE_MAP";
    }

    public enum AchievementType
    {
        All = 0,
        Team = 1,
        Player = 2,
        Develop = 3,
    }

    public class GameConst
    {
        //  当天最大招募数
        public const int DayMaxRecruitTimes = 10000;
        //超训 cd 时间
        public static readonly int BigBangCdTime = 3 * 60 * 60 * 1000;

        //增加经验最小单位s
        public const float AddExpMinUnitSecond = 0.2f;
        public const float SyncTrainEventUnitSecond = 1.0f;

        public const float SyncHandshakeUnitSecond = 15.0f;

        //邀请赛次数名额
        public const int MaxInviteMatchKen = 2;

        //球员初始化体能
        public const int CardInitEnergy = 200;
        //球员体能单场警告值
        public const int CardSingleEnergyWarning = 40;
        //球员体能单场满值
        public const int CardSingleEnergy = 50;
        //玩家最大体力
        public const int PlayerMaxEnergy = 200;
        //单场消耗体力5点， 推图和剧情都用这个
        public const int BattleEnergy = 5;
        //体力价格
        public const string EnergyPrice = "50,100";
        //每次购买的体力数量
        public const int EnergyGoodCount = 50;

        //体力恢复时间
        public const int PlayerEnergyRecoverTime = 300;

        //球员最高星级
        public const int MaxCardStar = 10;

        //技能最高等级
        public const int MaxSkillLevel = 10;

        public const int ChallengeStart = 101;
        public const int ChallengeEndLast = 1520;
        public const int ChallengeEnd = 99999;
        // 一天内可以挑战多少次
        public const int ChallengeTimes = 3;
        //   - 小游戏每日3次免费游玩，超出免费次数后需付费，付费为：基础费用*2n（n-1次方）免费游玩次数每天刷新。付费的基础费用是20钻，n为额外游戏次数
        public const int ChallengeCostBase = 10;

        public const int EmailOverdueTime = 1728000;

        //竞技场S阶段ID
        public const int ArenaSStage = 9;
        //竞技场购买增加的挑战次数
        public const int ArenaBuyAddTimes = 3;
        //竞技场购买增加的刷新次数
        public const int ArenaBuyAddRefreshTimes = 5;
        //竞技场每日免费挑战和刷新次数
        public const int ArenaFreeTimes = 5;
        //竞技场每日购买次数限制
        public const int ArenaBuyTimesLimit = 3;
        //竞技场购买初始钻石
        public const int ArenaBuyTimesDiamond = 100;

        //战力放大缩小比例
        public const int ABILITY_NORMAL = 100;
        //解雇不能低于8
        public const int FIRE_MIN_LEFT = 8;
        //首充值
        public const int FIRST_CHARGE_COST = 6;

        //广告换成钻石
        public const int VIDEO_DIAMOND = 10;


        public const int MONTH_CARD_NORMAL = 5001;

        public const int MONTH_CARD_SUPER = 5002;

        public const int NOVICE_TASK_END_DADYS = 14;
    }

    public class ActivityID
    {
        public const int ChristmasTree = 14001;//圣诞树
        public const int ChristmasTask = 15001;//圣诞任务
        public const int NewYearChallenge = 12002;//元旦主页
        public const int NewYearTask = 15002;//元旦任务
        public const int NewYearGift = 16001;//元旦礼包
        public const int NewYearSign = 17001;//跨年签到

        public const int SpringFestivalGift = 16002;//春节礼包
        public const int SpringFestivalTask = 15003;//春节任务
        public const int DragonYearRedEnvelope = 20001;//龙年红包
        public const int SpringFestivalWish = 17002;//春节签到

        public const int FirstPay = 1001;//首次充值
    }

    public enum TrainUpLevelType
    {
        //升级一次
        UpgradeOne = 0,
        //升级十次
        UpgradeTen = 1,
        //升级一百次
        UpgradeHundred = 2,
        //升级到最高等级
        UpgradeMAX = 3,
    }

    public enum StrengthenState
    {
        //未训练
        Untrained = 1,
        //已训练
        Trained = 2,
    }

    public enum GuideCharacter
    {
        Board,  // 董事会
        Player, // 玩家
        Member, // 球员们
        Clerk   // 秘书
    }

    public class GuideEmailID
    {
        public const int WinEmail = 1001;
        public const int DeuceEmail = 1002;
        public const int FailEmail = 1003;
        public const int GuideEndEmail = 1004;
    }

    public enum GuideID
    {
        directorsLetter = 5,//董事会来信

        directorsTalk = 10,//董事会谈话

        teamTalk = 13,//与球员对话

        fightShow = 17,//战斗展示

        sendGuideMail = 20,//发送引导邮件//目前此引导已不再流程中
        UseGuideMail = 25,//引导领取邮件附件//目前此引导已不再流程中

        popWindowAfterFightShow = 30,//战斗总结弹窗//目前此引导已不再流程中

        //guideTalkClassic,//经典赛介绍
        //guideClickClassic,//主界面点击经典赛
        //guideClickCountry,//经典赛世界地图点第一个国家
        //guideClickClub,//经典赛国家界面点列表中最后一个俱乐部
        //guideClickEnterFight,//对手预览界面点开战
        guidePass13 = 35,//引导通关3-1

        guideGetProgressBox3Tip = 40,//引导点击第三个箱子之前的提示
        guideGetProgressBox3 = 45,//引导点击第三个箱子

        guideGetNewPlayerTip = 50,//引导抽卡提示
        guideGetNewPlayer = 55,//引导抽卡

        guideUpLevelPlayerTip = 60,//引导升级球员提示
        guideUpLevelPlayer = 65,//引导升级球员

        guideGotoFightAfterUpLevel = 70,//引导升级球员后去战斗

        /// <summary>
        /// 新手引导结束判断标记
        /// </summary>
        starterGuide = 100,//新手引导

        guideShootGame = 201,//投篮小游戏的引导

        test1 = 1301,//测试快速连续分消息发送完成1
        test2 = 1302,//测试快速连续分消息发送完成2
        //GUIDE2,
        //GUIDE2_1,
        ////GUIDE2_2,
        ////GUIDE2_5,
        //GUIDE3,
        //GUIDE3_1,
        //GUIDE4,
        //GUIDE4_1,
        //GUIDE4_2,
        //GUIDE4_3,
        //GUIDE5,
        //GUIDE6,
        //GUIDE7,
        //Trigger_Challenge_1,
        //Trigger_Challenge_2,
        //Trigger_Challenge_3,
        //Trigger_Recruit_1,
        //Trigger_Recruit_2,
        //Trigger_Recruit_3,
        //Trigger_League_1,
        //Trigger_League_2,
        //Trigger_League_3,
    }

    public class StartDialogueID
    {
        public const int Dialogue1 = 1001;
        public const int Dialogue2 = 2001;
        public const int Dialogue3 = 3001;
        public const int Dialogue4 = 4001;
        public const int Dialogue5 = 5001;
    }

    public class TrainId
    {
        //射门
        public const int Shoot = 1;
        //跑位
        public const int Run = 2;
        //传球
        public const int Pass = 3;
        //盘带
        public const int Dribble = 4;
        //抢断
        public const int Snatch = 5;
        //盯人
        public const int ManToMan = 6;
        //扑救
        public const int SuperbSave = 7;
        //速度
        public const int Speed = 8;
        //身体
        public const int Body = 9;
        //意志
        public const int Will = 10;
        //所有
        public const int All = 99;
    }

    public class AbilityId
    {
        //射门
        public const int Shoot = 1;
        //跑位
        public const int Run = 2;
        //传球
        public const int Pass = 3;
        //盘带
        public const int Dribble = 4;
        //抢断
        public const int Snatch = 5;
        //盯人
        public const int ManToMan = 6;
        //扑救
        public const int SuperbSave = 7;
        //速度
        public const int Speed = 8;
        //身体
        public const int Body = 9;
        //意志
        public const int Will = 10;
    }

    public class TrainEventIds
    {
        //升级
        public const int Upgrade = 1;
        //强化
        public const int Strengthen = 2;
        //突破
        public const int Break = 3;
        //觉醒
        public const int BigBang = 4;
    }

    public enum InviteMatchState
    {
        //初始化
        Init = 1,

        //比赛结束
        End = 2,

        //已经获得了奖励
        Rewarded = 3,
    }

    public enum PlayerCardStatus
    {
        //顶峰、上升、普通、下滑、低谷
        VeryDown = 1,//0%
        Down,//25%
        Ordinary,//50%
        Good,//75%
        VeryGood,//100%
    }

    public class CompitionID
    {
        // 无
        public const int None = 0;
        // 联赛
        public const int League = 1;
        // 杯赛
        public const int Cup = 2;
        // 百分大战
        public const int Hundred = 3;
    }

    public class GoodsId
    {
        //  招募点
        public const int ActRecruitPoint = 400210;// 高级球探契约 
        public const int RecruitPoint = 400201;//球探契约
        public const int ContractFragment = 400502; //合同碎片
        public const int TrainRoomUnlockGoods = 400205;//TRAIN_ROOM_UNLOCK_GOODS400202
        public const int TacticsCard = 400401;

        public const int ArenaMoneyId = 400501; //竞技币


        public const int MedicalBox = 400206;	//医疗箱
        public const int AdvMedicalBox = 400207;	//高级医疗箱
        public const int EnergyDrink = 400202;	//体能饮料
        public const int AdvEnergyDrink = 400208;	//高级体能饮料
        public const int CoachQuotes = 400204; //教练语录
        public const int AdvCoachQuotes = 400209;	//高级教练语录
        public const int TowerMoney = 100109; //殿堂荣誉

        public static readonly List<int> CardUpLevelGoodsId = new() { 100104, 100105, 100106, 100107 };//初级经验书,中级经验书,高级经验书,特级经验书

        public static readonly List<int> CrystalGoodsId = new() { 400105, 400104, 400103, 400102, 400101 }; //星晶(绿、蓝、紫、橙、红)

        public const int DragonYearRedEnvelope = 700002;	//龙年红包

        public const int HundredGuessWhistle = 100110;	//百分大战竞猜（应援）哨子
    }
    public class CardId
    {
        public const int ChenGuoHao = 104036;	//陈国豪
        public const int YangHanSen = 104038;	//杨瀚森
        public const int LiXiaoXu = 104039;	//2012-2013·李晓旭
        public const int ZhaoRui = 104040; //全明星·赵睿
        public const int HuMingXuan = 104041; //全明星·胡明轩
    }

    /*
道具类型		
1	普通道具	不可直接使用
2	宝箱类道具	可打开，参数为packageID
3	球员碎片道具	可合成，参数1为合成所需数量，参数2为合成目标的ID
4	材料道具	可使用，参数1为跳转的指定界面ID。如使用改名卡，则跳转至改名界面。参数2可以作为有效值，比如医疗保减少的时间值
5	道具碎片	可使用，参数1为合成所需数量，参数2为合成后打开的宝箱ID
6	训练材料	不可直接使用，参数1为提供的训练经验
7		
8	自选道具	基本同2，不同的是玩家只能在packageID的库中选1个
特殊的		
7开头	活动道具	这不是正常的类，但ID统一用7起，以示区分。
特色是活动道具是有失效日期的，其他道具一般不配失效期。
*/
    /// <summary>
    /// 对应 包裹物品表 中的 type字段
    /// </summary>
    public enum GoodsType
    {
        // 资源
        Res = 0,
        // 普通道具
        Normal = 1,
        // 宝箱道具
        Box = 2,
        // 球员碎片
        Pieces = 3,
        // 材料道具
        Material = 4,
        // 道具碎片
        PropSplinter = 5,
        // 训练材料
        TrainMaterial = 6,
        // 自选碎片道具
        SelectProp = 7,
        // 自选碎片道具
        SelectBoxProp = 8,
        // 随机已有碎片
        RandomExistCardPieces = 9
    }

    public class ResourceId
    {
        //钻石
        public const int Diamond = 1;
        //欧元
        public const int Money = 2;
        //一分钟经验奖励
        public const int TrainExpMin = 3;
        //玩家经验
        public const int PlayerExp = 4;
        //球员经验
        public const int HeroExp = 5;
        //体力
        public const int Energy = 6;

    }

    /// <summary> 游戏物品类型 </summary>
    public enum GameItemType
    {
        /// <summary> 未知 </summary>
        None = 0,
        /// <summary> 资源，如钻石、欧元、经验 </summary>
        Resource = 1,
        /// <summary> 物品，如球员碎片、球员自选卡、经验书 </summary>
        Goods = 2,
        /// <summary> 卡牌，整张卡牌 </summary>
        Card = 3,
        /// <summary> 荣誉，荣誉室用的 </summary>
        Honour = 4,
        /// <summary> NFT </summary>
        NFT = 99
    }

    public enum MessageType
    {
        //大突破弹窗
        BigBreakThrough,
        //小突破弹窗
        BreakThrough,
        //解锁弹窗
        Unlock,
        // 挑战解锁
        UnlockChallenge,
        // 联赛解锁
        UnlockLeague
    }
    public enum InjuryType
    {
        //健康
        None = 0,
        //健康
        Health = 1,//100%
        //轻伤
        MinorInjury,//50%
        //重伤
        SeriousInjury//0%
    }

    public enum RecruitCountType
    {
        Once = 1,
        Ten = 2,
    }

    public enum RecruitCostType
    {
        Diamond = 1,
        Goods = 2,
    }

    /// <summary>
    /// 篮球大位置
    /// </summary>
    public enum PositionType
    {
        All = 0,

        //前锋
        /// <summary>
        /// 后卫
        /// </summary>
        HouWei = 1,

        //中场
        /// <summary>
        /// 前锋
        /// </summary>
        QianFeng = 2,

        //后卫
        /// <summary>
        /// 中锋
        /// </summary>
        ZhongFeng = 3
    }

    /// <summary>
    /// 篮球小位置
    /// </summary>
    public enum PositionSeparatedType
    {
        All = 0,

        /// <summary>
        /// 控球后卫
        /// </summary>
        KongQiuHouWei = 1,

        /// <summary>
        /// 得分后卫
        /// </summary>
        DeFenHouWei = 2,

        /// <summary>
        /// 小前锋
        /// </summary>
        XiaoQianFeng = 3,

        /// <summary>
        /// 大前锋
        /// </summary>
        DaQianFeng = 4,

        /// <summary>
        /// 中锋
        /// </summary>
        ZhongFeng = 5
    }

    public enum RecruitAppointCardState
    {
        Miss = 1,
        Hit = 2,
    }
    public class QualityType
    {
        public const int Red = 5;
        public const int Orange = 4;
        public const int Purple = 3;
        public const int Blue = 2;
        public const int Green = 1;

        public const int All = 0;
    }

    public class OptionsType
    {
        public const int Quality = 1;
        public const int Position = 2;
    }

    public enum CameraID
    {
        None,
        TrainShenti,
        TrainChuanqiu,
        TrainFangshou,
        TrainKoulan,
        TrainLanban,
        TrainToulan,
        TrainQiangduan,
        TrainKongqiu,
        TrainGaimao,
        TrainWending,
        BigBangPlayerModel,
        RecruitModel,
        FightScene,
        WorldMap,//挑战大地图
        Challenge,//挑战
        Battle,//战斗
        BattleAni,//战斗过场
        Battle2,//战斗2
        Shoot,//投篮小游戏
        Collection,//数字藏品
    }

    public enum GameObjectID
    {
        None,
        RecruitModel,
        RecruitlistUI,
        BigBangPlayerModel,
        FightController
    }

    public class SkillType
    {
        public const int Atk = 1;
        public const int Def = 2;
        public const int Assist = 3;
    }

    public enum SkillState
    {
        //条件不满足
        ConditionsNotMet = 1,
        //条件满足但是未解锁
        ConditionsMetLock = 2,
        //已解锁 没人训练
        UnlockNoTraining = 3,
        //已解锁 有人训练
        UnlockTraining = 4,
    }

    public enum TeamState//联赛的几个阶段
    {
        INIT = 1,//报名阶段，未报名，可报名
        SIGNUP = 2,//报名阶段，已报名
        MATCHING = 3,//战斗阶段
        SETTLE = 4//结束阶段，可报名下一场
    }

    public enum SkillTrainRoomState
    {
        Lock = 1,
        Idle,
        Training,
    }

    public enum WayID
    {
        None,
        // 通过回收道具获得
        RecoveryAllProp,
        // 跳转到招募界面
        Recruit,
        // 跳转到邀请赛界面
        Invite,
        // 跳转到背包界面(描述不一样)
        Package,
        // 跳转到赛事主界面
        League,
        // 跳转到杯赛主界面
        Cup,
        RecoveryFragAll,
        RecoveryFrag5,
        RecoveryFrag4,
        RecoveryFrag3,
        RecoveryFrag2,
        RecoveryFrag1,
        MainTaskUICup,

    }

    public enum SkillTrainSelectCardState
    {
        // 正常可以训练状态
        Normal = 1,

        // 不能训练
        CanNotTrain = 2,

        // 已经训练过
        HaveBeenTrain = 3,

        // 正在训练中
        DoTraining = 4,
    }

    public enum SkillTrainSelectSkillState
    {
        // 正常可以训练状态
        Normal = 1,

        // 不能训练
        CanNotTrain = 2,

        // 已经训练过
        HaveBeenTrain = 3,

        // 正在训练中
        DoTraining = 4,
    }

    public class FormationBoardId
    {
        public const int GKId = 105;
    }

    public enum FormationCardState
    {
        // 后备队员
        Reserve = 0,

        // 主力
        Starter,

        // 替补
        Substitute,
    }

    public class FightTeamType
    {
        public const int PLAYER = 1;
        public const int ROBOT = 2;
        public const int NPC = 3;
    }

    public enum FightType
    {
        PVE = 1,
        League = 2,
        Cup = 3,
        Guide = 4,
        ARENA = 5,
        Hero = 6,
        Tower = 7,
        Hundred = 8,
    }

    public enum FightEvent
    {
        FT = 1, //罚球命中，得分+1，罚球toal+1，count+1
        FG = 2, //两分命中，得分+2，两分toal+1，count+1
        TP = 3, //三分命中，得分+3，三分toal+1，count+1
        FG_AST = 4,  //两分助攻，p1加2分 p2加1助攻
        TP_AST = 5,  //三分助攻，p1加2分 p2加1助攻
        FT_MISS = 6, //罚球未中，罚球toal+1
        FG_MISS = 7, //两分未中，两分total+1
        TP_MISS = 8, //三分未中，三分total+1
        REB = 9, //篮板
        STL = 10, //抢断失误，p1加1抢断 p2加1失误
        BLK = 11, //盖帽
        TOV = 12, //失误
        FOUL = 13, //犯规
        T_FOUL = 14, //技术犯规（忽略）
        HURT = 15, //受伤（是否受伤）+1
        SUB = 16, //换人
    }
    public enum ShotType
    {
        SHOT_DUNK = 9,  // -- 灌篮
        SHOT_RIM = 10,   //-- 篮下
        SHOT_MID = 11,   //-- 中距离
        SHOT_THREE = 12,  // -- 远投
    }

    public class FormationID
    {
        public const int Bounty = -2;//悬赏任务

        public const int PVE = 1; //经典赛
        public const int PVP = 2;
        public const int ARENA = 3; //竞技场
        public const int HERO = 4; //剧情推图
        public const int TOWER = 5; //爬塔玩法（篮球殿堂）
        public const int Hundred = 6; //百分大战
    }

    public class FormationConst
    {
        public const int StarterCount = 5;
        public const int SubstituteCount = 7;
    }

    public class UIID
    {
        public const int MyGameUI = 4;      // 我的比赛
        public const int ChallengeUI = 5;   // 挑战界面
        public const int FightUI = 6;       // 战斗界面
        public const int DailyTaskUI = 7;   // 日常任务界面
        public const int WeeklyTaskUI = 8;  // 周常任务界面
        public const int SkillListUI = 9;   // 特技列表界面
        public const int SkillTrainUI = 10; // 特技学习界面
        public const int GiftShopUI = 11;   // 礼包商城界面
        public const int TrainShopUI = 12;  // 训练商城界面
        public const int DiamondShopUI = 13;// 钻石商城界面
        public const int RecycleUI = 14; // 仓库回收界面
        public const int RenameUI = 15;  // 改名界面
        public const int RegularUI = 16; // 训练界面
        public const int StrengthUI = 17;// 强化界面
        public const int BigbangUI = 18; // 超能界面
        public const int InviteUI = 19;  // 邀请赛界面
        public const int RecruitUI = 20; // 招募界面
        public const int CardUI = 21;    // 球员列表界面
        public const int TacticUI = 22;    // 战术界面
        public const int ArenaExShopUI = 23;    // 竞技场兑换商店界面
        public const int RecruitShopUI = 24;    // 商店招募页签
        public const int PVPShopUI = 25;    // 商店联赛页签。
        public const int SelectProps = 26;    // 筛选道具弹窗。
    }

    public class GameResultType
    {
        public const int None = 0;
        public const int Win = 1;
        public const int Lose = 2;
        public const int Deuce = 3;

    }

    public class GetLeagueCourseType
    {
        public static int All = 1;
        public static int Mine = 2;
    }

    [System.Serializable]
    public enum TaskType
    {
        Normal = 1,
        Daily = 2,
        Weekly = 3
    }

    public class TaskState
    {
        public const int LOCK = 0;           // 未解锁
        public const int IN_PROGRESS = 1;    // 进行中
        public const int COMPLETE = 2;       // 完成(可以领取)
        public const int COLLECTED = 3;      // 领取奖励状态(领取完)
    }

    public class FormationSwapType
    {
        public const int MainToMain = 1;
        public const int MainToBench = 2;
        public const int BenchToBench = 3;
        public const int BenchToMan = 4;
        public const int BenchToBackup = 5;
        public const int BackupToBench = 6;
        public const int BackupToMain = 7;
        public const int MainToBackup = 8;
    }

    [System.Serializable]
    public enum MainTaskState
    {
        Lock = 0,
        InProgress = 1,
        Completed = 2,
    }

    [System.Serializable]
    public enum MainTaskType
    {
        None = 0,
        Train = 1,
        Player = 2,
        Challenge = 3,
        League = 4,
        Cup = 5,
        Champion = 6
    }
    public enum RewardStates
    {
        UNLOGIN = 1,  //未签到
        COLLECT = 2, //可领取
        RECEIVED = 3, //已领取
        UNCOLLECT = 4   //可补签
    }

    public enum RewardType
    {
        DaySigin = 1, //每天签到
        AddedSigin = 2,  //累计签到
    }

    public enum OnoffId
    {
        LEAGUE = 1,        // 联赛
        CHALLENGE = 2,     // 挑战
        RECRUIT = 3        // 招募
    }

    public class PurchaseSuccessReponseState
    {
        public const int SUCC = 1;
        public const int FAILED = 2;
        public const int INVLAID_ORDER = 3;
        public const int ALREADY_SUCC = 4;
    }

    //从PHP获取的服务器状态
    public enum ServerStatus
    {
        Gray = -1,//维护
        Green = 0,//流畅
        Red = 1,//爆满
        Yellow = 2//拥挤

    }
}
