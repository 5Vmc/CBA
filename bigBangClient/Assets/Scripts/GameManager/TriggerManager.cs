using Babu;
using Babu.Client.Fsm;
using BigBang;
using BigBang.Battle;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using Utils.GameItem;

/// <summary>
/// 对应功能定义表的 ID
/// module_define
/// </summary>
public enum TriggerModuleType
{
    Unknow = 0,
    Home = 1000,
    ClassicPVE = 1100,
    ClassicPVE_box = 1101,
    ClassicPVE_dungeon = 1102,
    ClassicHero = 1200,
    ClassicArena = 1300,
    ClassicArena_jinjie = 1301,
    ClassicArena_dailyrewards = 1302,
    ClassicArena_seasonrewards = 1303,
    ClassicArena_shoponce = 1304,
    ClassicArena_shopcommon = 1305,
    ClassicPVP = 1400,
    ClassicPVP_LianSai = 1401,
    ClassicPVP_BeiSai = 1402,
    BigBang = 1500,
    BigBang_qianghua = 1501,
    BigBang_chaoneng = 1502,
    BigBang_yaoqing = 1503,
    CardSkill = 1600,
    Task = 1700,
    Task_Daily = 1701,
    Task_Weekly = 1702,
    Task_Bounty = 1703,
    Games = 1800,
    Games_Shoot = 1801,
    Games_Energy = 1802,
    Games_Card = 1803,
    BlockChain = 1900,
    Career = 2000,
    Card = 2100,
    Card_Level = 2101,
    Card_Equip = 2102,
    Card_UpStar = 2103,
    Recruit = 2200,
    Recruit_Normal = 2201,
    Recruit_Time = 2202,
    Recruit_AllStar = 2203,
    Activity = 2300,
    Activity_Sign30 = 2301,
    Activity_MonthCard = 2302,
    Activity_NewPlay7Day = 2303,
    Activity_Sign7 = 2304,
    Activity_PayFirst = 2305,
    Achieve = 2400,
    Honour = 2401,
    Bag = 2500,
    Bag_Recycle = 2501,
    Formation = 2600,
    ZhenFa = 2601,
    Shop = 2700,
    Shop_diamond = 2701,
    Shop_train = 2702,
    Shop_gift = 2703,
    Shop_recruit = 2704,
    Shop_monthcard = 2705,

    NewActivity = 2800,
    /// <summary>
    /// 活动主面板
    /// </summary>
    NewActivity_GP1 = 2801,
    /// <summary>
    /// 条件礼包模板
    /// </summary>
    NewActivity_GP2 = 2802,
    /// <summary>
    /// 7日签到
    /// </summary>
    NewActivity_GP3 = 2803,
    /// <summary>
    /// 30天签到
    /// </summary>
    NewActivity_GP4 = 2804,
    /// <summary>
    /// 新手目标
    /// </summary>
    NewActivity_GP5 = 2805,
    /// <summary>
    /// 节日活动
    /// </summary>
    NewActivity_GP6 = 2806,
    /// <summary>
    /// 篮球殿堂
    /// </summary>
    Fuben_DianTang = 2900,
    /// <summary>
    /// 百分大战
    /// </summary>
    Hundred = 3001,
    /// <summary>
    /// 圣诞树
    /// </summary>
    ChristamsTree = 3101,

    /// <summary>
    /// 福利
    /// </summary>
    Welfare = 3002,
    /// <summary>
    /// 首充
    /// </summary>
    FirstCharge = 3003,
    /// <summary>
    /// 每日充值
    /// </summary>
    DailyCharge = 3004,
    /// <summary>
    /// 战令
    /// </summary>
    BattlePass = 3005,
    /// <summary>
    /// 排行榜
    /// </summary>
    Rank = 3006,
    /// <summary>
    /// 邮箱
    /// </summary>
    Mail = 3007,
    /// <summary>
    /// 公告
    /// </summary>
    Notice = 3008,

    /// <summary>
    /// 圣诞活动
    /// </summary>
    Christmas = 3009,
    /// <summary>
    /// 元旦活动
    /// </summary>
    NewYearTask = 3010,
    /// <summary>
    /// 跨年签到
    /// </summary>
    NewYearSign = 3011,
    /// <summary>
    /// 龙年红包
    /// </summary>
    DragonYearRedEnvelope = 3012,
    /// <summary>
    /// 龙年许愿签到
    /// </summary>
    DragonYearWishSign = 3013,
    /// <summary>
    /// 全明星
    /// </summary>
    AllStar = 3014,
    /// <summary>
    /// 劳动节 2024
    /// </summary>
    LabourDay = 3015,
    /// <summary>
    /// 季后赛总决赛竞猜2024
    /// </summary>
    PlayoffFinalsGuess = 3016,
    /// <summary>
    /// 端午节活动赛龙舟2024
    /// </summary>
    DragonBoatFestival = 3017,
    /// <summary>
    /// 奥运会2024
    /// </summary>
    Olympic2024 = 3018,

    /// <summary>
    /// 快捷兑换
    /// </summary>
    Quick_Exchange = 9999,

}

/// <summary>
/// 掉落指引的模块描述
/// </summary>
public class DropModule
{
    public int moduleId;
    public int openlv;
    public string txtmoduleName;
    public string txtDesc;
    /// <summary>
    /// 权重，代表在某个道具的所有掉落模块中，该模块所占的比重。
    /// </summary>
    public int weight;
}


public class TriggerManager : Singleton<TriggerManager>
{
    /// <summary>
    /// <等级，功能List<module_enum>>
    /// </summary>
    Dictionary<int, List<int>> Level_Module;
    /// <summary>
    /// module_enum, level
    /// </summary>
    Dictionary<int, int> Module_Level;

    /// <summary>
    /// itemtype:1数值，2道具，3卡牌等等  第2个int是目标Id
    /// </summary>
    Dictionary<GameItemType, Dictionary<int, List<DropModule>>> dropModuleDict;

    private bool isInited = false;
    public void InitOnce(bool forceInit = true)
    {
        if (isInited && !forceInit) return;
        isInited = true;
        var configs = Configs.CommonTrigger.GetConfigList().FindAll(p => p.TriggerId == 30001 && p.Action == "unlockModule");
        var count = configs.Count;

        Level_Module = new Dictionary<int, List<int>>();
        Module_Level = new Dictionary<int, int>();
        for (var index = 0; index < count; index++)
        {
            var _cfg = configs[index];
            if (!Level_Module.ContainsKey(_cfg.Condition)) Level_Module.Add(_cfg.Condition, new List<int>());
            Level_Module[_cfg.Condition].Add(int.Parse(_cfg.ActionParam));

            Module_Level[int.Parse(_cfg.ActionParam)] = _cfg.Condition;
        }

        initDropFrom();
    }

    /// <summary>
    /// 检查功能是否开启，如果这个功能没有进入配置表则默认开启。
    /// </summary>
    /// <param name="moduleId"></param>
    /// <param name="showtip"></param>
    /// <returns></returns>
    public bool CheckModuleOpen(int moduleId, bool showtip = true)
    {
        if (moduleId == 0) return true;
        bool result = Module_Level.ContainsKey(moduleId) ? Player.Level >= Module_Level[moduleId] : true;
        if (!result && showtip)
        {
            var moduleConfig = Configs.ModuleDefine.GetConfig(moduleId);
            if (moduleConfig != null)
            {
                Tips.PopTips(string.Format("{1}级开启[{0}]功能", moduleConfig.Name, Module_Level[moduleId]));
            }
        }
        return result;
    }

    /// <summary>
    /// 获得多少级开启的字符串
    /// </summary>
    public string GetShortLockTipStr(TriggerModuleType type)
    {
        int moduleId = (int)type;
        var moduleConfig = Configs.ModuleDefine.GetConfig(moduleId);
        return "{0}级开启".SafeFormat(Module_Level[moduleId]);
    }

    /// <summary>
    /// 检查功能是否开启，如果这个功能没有进入配置表则默认开启。
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool CheckModuleOpen(TriggerModuleType type, bool showtip = true)
    {
        int moduleId = (int)type;
        return CheckModuleOpen(moduleId, showtip);
    }

    /// <summary>
    /// 获取功能开启的相关信息
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public string GetModuleOpenInfo(TriggerModuleType type, bool isShort = false)
    {
        int moduleId = (int)type;
        return GetModuleOpenInfo(moduleId, isShort);
    }

    /// <summary>
    /// 获取功能开启的相关信息
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public string GetModuleOpenInfo(int moduleId, bool isShort = false)
    {
        string resultInfo = "";
        bool result = Module_Level.ContainsKey(moduleId) ? Player.Level >= Module_Level[moduleId] : true;
        if (!result)
        {
            var moduleConfig = Configs.ModuleDefine.GetConfig(moduleId);
            if (moduleConfig != null)
            {
                if (isShort)
                {
                    resultInfo = Module_Level[moduleId].ToString();
                }
                else
                {
                    resultInfo = "{1}级开启[{0}]功能".SafeFormat(moduleConfig.Name, Module_Level[moduleId]);
                }
            }
        }
        return resultInfo;
    }

    /// <summary>
    /// 当前等级是否有新功能开启，有就返回模块id，没有返回0
    /// 注意模块id，对应了图片id
    /// </summary>
    /// <param name="level"></param>
    /// <param name="funindex"></param>
    /// <param name="onlyForceGo">是否只看强制前往的</param>
    /// <returns></returns>
    public int IsNewModuleOpen(int level, int funindex = 0, bool onlyForceGo = true)
    {
        if (Level_Module.ContainsKey(level))
        {
            if (Level_Module[level].Count > 0)
            {
                if (onlyForceGo)
                {
                    var cfg = Configs.ModuleDefine.GetConfig(Level_Module[level][funindex]);
                    if (cfg == null)
                    {
                        Debug.LogWarningFormat("TriggerManager , IsNewModuleOpen , cfg == null , level = {0} , funindex = {1} , onlyForceGo = {2}", level, funindex, onlyForceGo);
                        return 0;
                    }
                    return cfg.Forcego == 1 ? Level_Module[level][funindex] : 0;
                }
                else
                {
                    return Level_Module[level][funindex];
                }
            }
            else return 0;
        }
        else return 0;
    }

    public void JumpPanel(TriggerModuleType module)
    {
        JumpPanel((int)module);
    }

    public void JumpPanel(int moduleId, bool isNeedShowWindowDesc = false, int extParam = 0, int extParam2 = 0)
    {
        if (moduleId == 0) return;
        //if (GuideManager.IsFinished(GuideID.starterGuide))
        //{
        //    switch (moduleId)
        //    {
        //        case (int)TriggerModuleType.ClassicPVE: break;
        //        case (int)TriggerModuleType.ClassicPVE_box: break;
        //        case (int)TriggerModuleType.ClassicPVE_dungeon: break;
        //        case (int)TriggerModuleType.Quick_Exchange: break;
        //        case (int)TriggerModuleType.Games: break;
        //        case (int)TriggerModuleType.Hundred: break;
        //        default:
        //            {
        //                UIController.Instance.CloseAllPanelAndWindow();
        //                UIController.Instance.ShowPanel<HomeUI>();
        //            }
        //            break;
        //    }
        //}

        Guide2UIProperties properties = null;
        if (isNeedShowWindowDesc && GuideManager.IsFinished(GuideID.starterGuide))
        {
            ModuleDefineConfig moduleDefineConfig = Configs.ModuleDefine.GetConfig(moduleId);
            if (moduleDefineConfig != null && moduleDefineConfig.Conversation != 0)
            {
                GuideDialogueConfig guideDialogueConfig = Configs.GuideDialogue.GetConfig(moduleDefineConfig.Conversation);
                if (guideDialogueConfig == null)
                {
                    Debug.LogWarningFormat("TriggerManager , JumpPanel , guideDialogueConfig == null , moduleId = {0} ,  moduleDefineConfig.Conversation = {1}", moduleId, moduleDefineConfig.Conversation);
                }
                else
                {
                    properties = new Guide2UIProperties(Configs.GuideDialogue.GetConfig(moduleDefineConfig.Conversation), () =>
                    {
                        EventManager.Instance.Dispatch(EventID.OnTriggerGuide2UIClose);
                    });
                }
            }
        }
        bool isNeedOpenGuide2UI = properties != null;

        switch (moduleId)
        {
            case (int)TriggerModuleType.Home:
                if (extParam2 == 1)
                {
                    UIController.Instance.ShowPanel<HomeUI>(new HomeUIProperties(true));
                }
                else
                {
                    UIController.Instance.ShowPanel<HomeUI>();
                }
                break;
            case (int)TriggerModuleType.ClassicArena:
            case (int)TriggerModuleType.ClassicArena_dailyrewards:
            case (int)TriggerModuleType.ClassicArena_jinjie:
            case (int)TriggerModuleType.ClassicArena_seasonrewards:
            case (int)TriggerModuleType.ClassicArena_shopcommon:
            case (int)TriggerModuleType.ClassicArena_shoponce: UIController.Instance.ShowPanel<ArenaUI>(new ArenaUIProperties(ArenaUI.SubUIID.Arena, isNeedOpenGuide2UI)); break;
            case (int)TriggerModuleType.ClassicPVE:
            case (int)TriggerModuleType.ClassicPVE_box:
            case (int)TriggerModuleType.ClassicPVE_dungeon: ClassicManager.Instance.OpenClassicMapUI(); break;
            case (int)TriggerModuleType.ClassicPVP:
            case (int)TriggerModuleType.ClassicPVP_BeiSai: //UIController.Instance.ShowPanel<MatchHomeUI>(new MatchHomeUIProperites()); break;
            case (int)TriggerModuleType.ClassicPVP_LianSai: UIController.Instance.ShowPanel<LeagueHomeUI>(); break;
            case (int)TriggerModuleType.ClassicHero: UIController.Instance.ShowPanel<FBMainUI>(); break;
            case (int)TriggerModuleType.BigBang: FsmManager.Instance.ChangeToState<StateTrain>(new StateCommonUserData() { OpenUIAction = async () => { await UIController.Instance.ShowPanel<TrainUI>(new TrainUIPanelProperties(TrainUI.SubUIID.Regular)); } }); break;
            case (int)TriggerModuleType.BigBang_qianghua: FsmManager.Instance.ChangeToState<StateTrain>(new StateCommonUserData() { OpenUIAction = async () => { await UIController.Instance.ShowPanel<TrainUI>(new TrainUIPanelProperties(TrainUI.SubUIID.Strength)); } }); break;
            case (int)TriggerModuleType.BigBang_chaoneng: FsmManager.Instance.ChangeToState<StateTrain>(new StateCommonUserData() { OpenUIAction = async () => { await UIController.Instance.ShowPanel<TrainUI>(new TrainUIPanelProperties(TrainUI.SubUIID.BigBang)); } }); break;
            case (int)TriggerModuleType.BigBang_yaoqing:
                FsmManager.Instance.ChangeToState<StateTrain>(
                    new StateCommonUserData()
                    {
                        OpenUIAction = async () =>
                        {
                            if (!Player.TrainManager.InviteMatchController.IsUnlock)
                            {
                                Tips.PopTips("邀请赛还未解锁，请继续训练");
                                await UIController.Instance.ShowPanel<TrainUI>(new TrainUIPanelProperties(TrainUI.SubUIID.Regular));
                            }
                            else
                            {
                                await UIController.Instance.ShowPanel<TrainUI>(new TrainUIPanelProperties(TrainUI.SubUIID.Invite));
                            }
                        }
                    });

                break;
            case (int)TriggerModuleType.CardSkill: UIController.Instance.ShowPanel<SkillUI>(); break;
            case (int)TriggerModuleType.Task:
            case (int)TriggerModuleType.Task_Daily:
            case (int)TriggerModuleType.Task_Weekly: FsmManager.Instance.ChangeToState<StateTask>(new StateCommonUserData() { OpenUIAction = async () => { await UIController.Instance.ShowPanel<TaskUI>(new TaskUIProperties(TaskUI.SubUIID.Daily)); } }); break;
            case (int)TriggerModuleType.Task_Bounty: FsmManager.Instance.ChangeToState<StateTask>(new StateCommonUserData() { OpenUIAction = async () => { await UIController.Instance.ShowPanel<TaskUI>(new TaskUIProperties(TaskUI.SubUIID.Bounty)); } }); break;
            case (int)TriggerModuleType.Games:
                FsmManager.Instance.ChangeToState<StateTinyFun>(new StateCommonUserData()
                {
                    OpenUIAction = async () =>
                    {
                        await UIController.Instance.ShowPanel<BigBang.Battle.ShootUI>(new ShootUIProperties(extParam == 0 ? ShootUIEnterPos.tinyFun : ShootUIEnterPos.Jump));
                    }
                });
                break;
            case (int)TriggerModuleType.Career: FsmManager.Instance.ChangeToState<StateMainTask>(new StateCommonUserData() { OpenUIAction = async () => { await UIController.Instance.ShowPanel<MainTaskUI>(); } }); break;
            case (int)TriggerModuleType.Recruit: UIController.Instance.ShowPanel<RecruitUI>(new RecruitUIProperties(RecruitUI.SubUIID.Auto)); break;
            case (int)TriggerModuleType.Recruit_Normal: UIController.Instance.ShowPanel<RecruitUI>(new RecruitUIProperties(RecruitUI.SubUIID.Normal)); break;
            case (int)TriggerModuleType.Recruit_Time: UIController.Instance.ShowPanel<RecruitUI>(new RecruitUIProperties(RecruitUI.SubUIID.Time)); break;
            case (int)TriggerModuleType.Recruit_AllStar: UIController.Instance.ShowPanel<RecruitUI>(new RecruitUIProperties(RecruitUI.SubUIID.AllStar, (AllStarManager.Area)extParam)); break;
            case (int)TriggerModuleType.Card:
            case (int)TriggerModuleType.Card_Equip:
            case (int)TriggerModuleType.Card_Level:
            case (int)TriggerModuleType.Card_UpStar: UIController.Instance.ShowPanel<CardUI>(new CardUIProperties(CardUI.SubUIID.Card)); break;
            case (int)TriggerModuleType.Activity_MonthCard:
                if (ServerConst.OPEN_BUY == false)
                {
                    Tips.PopTips("测试期间不开放充值");
                    break;
                }
                UIController.Instance.ShowPanel<MonthCardUI>();
                break;
            case (int)TriggerModuleType.Bag: UIController.Instance.ShowPanel<InventoryUI>(new InventoryUIProperties(InventoryUI.SubUIID.Inventory)); break;
            case (int)TriggerModuleType.Bag_Recycle: UIController.Instance.ShowPanel<InventoryUI>(new InventoryUIProperties(InventoryUI.SubUIID.Recycle, false)); break;
            case (int)TriggerModuleType.Formation:
                Player.FightManager.FormationController.GetAndCheckDefaultFormation(FormationID.PVE, formation =>
                {
                    UIController.Instance.ShowPanel<FormationUI>(new FormationProperties(formation, true));
                });
                break;
            case (int)TriggerModuleType.ZhenFa:
                Player.FightManager.FormationController.GetAndCheckDefaultFormation(FormationID.PVE, formation =>
                {
                    UIController.Instance.ShowPanel<FormationUI>(new FormationProperties(formation, true, FormationUI.FormationShowType.Tactics));
                });
                break;
            case (int)TriggerModuleType.Shop: UIController.Instance.ShowPanel<ShopUI>(new ShopUIProperties(ShopUI.SubUIID.Diamond)); break;
            case (int)TriggerModuleType.Shop_diamond: UIController.Instance.ShowPanel<ShopUI>(new ShopUIProperties(ShopUI.SubUIID.Diamond)); break;
            case (int)TriggerModuleType.Shop_train: UIController.Instance.ShowPanel<ShopUI>(new ShopUIProperties(ShopUI.SubUIID.Train)); break;
            case (int)TriggerModuleType.Shop_gift: UIController.Instance.ShowPanel<ShopUI>(new ShopUIProperties(ShopUI.SubUIID.Gift)); break;
            case (int)TriggerModuleType.Shop_recruit: UIController.Instance.ShowPanel<ShopUI>(new ShopUIProperties(ShopUI.SubUIID.Recruit)); break;
            case (int)TriggerModuleType.Shop_monthcard: UIController.Instance.ShowPanel<ShopUI>(new ShopUIProperties(ShopUI.SubUIID.MonthCard)); break;

            case (int)TriggerModuleType.Fuben_DianTang: UIController.Instance.ShowPanel<FBMainUI>(); UIController.Instance.ShowPanel<FBTowerHomeUI>(); break;
            case (int)TriggerModuleType.Quick_Exchange: UIController.Instance.OpenWindow<QuickExchangeUI>(new QuickExchangeUIProperties(extParam, extParam2)); break;
            case (int)TriggerModuleType.Hundred: HundredManager.Instance.OpenHundredHome(); break;
            case (int)TriggerModuleType.ChristamsTree: UIController.Instance.ShowPanel<ChristmasTreeUI>(); break;
            case (int)TriggerModuleType.Achieve: UIController.Instance.ShowPanel<AchievementUI>(new AchievementUIProperties(AchievementUI.SubUIID.Achievement)); break;
            case (int)TriggerModuleType.Honour: UIController.Instance.ShowPanel<AchievementUI>(new AchievementUIProperties(AchievementUI.SubUIID.Honour)); break;
            case (int)TriggerModuleType.Games_Shoot:
                FsmManager.Instance.ChangeToState<StateTinyFun>(new StateCommonUserData()
                {
                    OpenUIAction = async () =>
                    {
                        await UIController.Instance.ShowPanel<BigBang.Battle.ShootUI>(new ShootUIProperties(extParam == 0 ? ShootUIEnterPos.tinyFun : ShootUIEnterPos.Jump));
                    }
                });
                break;
            case (int)TriggerModuleType.Welfare: UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.Sign7Day, new() { ActivityClientType.Sign7Day, ActivityClientType.Sign30Day, ActivityClientType.EnergyCenter })); break;//福利界面
            case (int)TriggerModuleType.FirstCharge: UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.FirstPay, new() { ActivityClientType.FirstPay })); break;
            case (int)TriggerModuleType.DailyCharge: UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.TotalPay, new() { ActivityClientType.TotalPay, ActivityClientType.DailyPay })); break;
            case (int)TriggerModuleType.BattlePass: UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.BattlePass, new() { ActivityClientType.BattlePass })); break;
            case (int)TriggerModuleType.Rank: UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.RankAwards, new() { ActivityClientType.RankAwards })); break;
            case (int)TriggerModuleType.Mail: UIController.Instance.ShowPanel<MailBoxUI>(); break;
            //case (int)TriggerModuleType.Notice: UIController.Instance.ShowPanel<NoticeUI>(); break;//暂时还没有公告界面
            case (int)TriggerModuleType.Christmas: UIController.Instance.ShowPanel<ChristmasTreeUI>(); break;
            case (int)TriggerModuleType.NewYearTask: UIController.Instance.ShowPanel<NewYearHomeUI>(); break;
            case (int)TriggerModuleType.NewYearSign: UIController.Instance.ShowPanel<NewYearSignUI>(); break;
            //case (int)TriggerModuleType.SpringFestival: UIController.Instance.ShowPanel<AchievementUI>(); break;//春节等春节之前做
            case (int)TriggerModuleType.BlockChain: FsmManager.Instance.ChangeToState<StateNft>(new StateCommonUserData() { OpenUIAction = async () => { await UIController.Instance.ShowPanel<NFTChinaUI>(); } }); break;
            //活动相关的UI
            //case (int)TriggerModuleType.NewActivity_GP1: UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(extParam)); break;
            case (int)TriggerModuleType.NewActivity_GP2: UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.TimeGiftCollection, new() { ActivityClientType.TimeGiftCollection, ActivityClientType.GiftPay })); break;
            case (int)TriggerModuleType.NewActivity_GP3: UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.Sign7Day, new() { ActivityClientType.Sign7Day, ActivityClientType.Sign30Day, ActivityClientType.EnergyCenter })); break;
            case (int)TriggerModuleType.NewActivity_GP4: FsmManager.Instance.ChangeToState<StateMonthSign>(new StateCommonUserData() { OpenUIAction = async () => { await UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.Sign7Day, new() { ActivityClientType.Sign30Day, ActivityClientType.Sign30Day, ActivityClientType.EnergyCenter })); } }); break;
            case (int)TriggerModuleType.NewActivity_GP5: UIController.Instance.ShowPanel<NoviceTargetUI>(); break;
            //case (int)TriggerModuleType.NewActivity_GP6: UIController.Instance.ShowPanel<ActivityFestivalMainUI>(new ActivityFestivalMainUIProperties((int)EActivityType.FestivalGift)); break;
            case (int)TriggerModuleType.Activity_NewPlay7Day: UIController.Instance.ShowPanel<NoviceTargetUI>(); break;
            case (int)TriggerModuleType.DragonYearRedEnvelope:
                if (UnityEngine.PlayerPrefs.GetInt(PlayerPrefsKeys.LiXiaoXuMainPageShow + Player.GbId, 0) == 0)
                {
                    UnityEngine.PlayerPrefs.SetInt(PlayerPrefsKeys.LiXiaoXuMainPageShow + Player.GbId, 1);
                    UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.LiXiaoXuMainPage, new() { ActivityClientType.LiXiaoXuMainPage, ActivityClientType.SpringFestivalTask, ActivityClientType.SpringFestivalGift, ActivityClientType.DragonYearRedEnvelope }));
                }
                else
                {
                    UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.DragonYearRedEnvelope, new() { ActivityClientType.LiXiaoXuMainPage, ActivityClientType.SpringFestivalTask, ActivityClientType.SpringFestivalGift, ActivityClientType.DragonYearRedEnvelope }));
                }
                break;
            case (int)TriggerModuleType.DragonYearWishSign: UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.YangHanSenMainPage, new() { ActivityClientType.YangHanSenMainPage, ActivityClientType.SpringFestivalWish })); break;
            case (int)TriggerModuleType.AllStar: UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.AllStarHome, new() { ActivityClientType.AllStarTask, ActivityClientType.AllStarGift, ActivityClientType.AllStarHome })); break;
            case (int)TriggerModuleType.LabourDay: UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.LabourDayHome, new() { ActivityClientType.LabourDayGift, ActivityClientType.LabourDayTask, ActivityClientType.LabourDaySign, ActivityClientType.LabourDayHome })); break;
            case (int)TriggerModuleType.PlayoffFinalsGuess: UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.PlayoffFinalsGuessHome, new() { ActivityClientType.PlayoffFinalsGuessSingle, ActivityClientType.PlayoffFinalsGuessHome })); break;
            case (int)TriggerModuleType.DragonBoatFestival: UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.DragonBoatFestivalHome, new() { ActivityClientType.DragonBoatFestivalGift, ActivityClientType.DragonBoatFestivalSign, ActivityClientType.DragonBoatFestivalHome })); break;
            case (int)TriggerModuleType.Olympic2024: UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(ActivityClientType.Olympics2024Gift, new() { ActivityClientType.Olympics2024Sign, ActivityClientType.Olympics2024Gift })); break;
            default:
                {
                    Debug.Log("未实现的跳转：moduleId = " + moduleId);
                }
                break;
        }
        if (isNeedOpenGuide2UI)
        {
            // 打开对话面板
            UIController.Instance.OpenWindow<Guide2UI>(properties);
        }
    }

    private void initDropFrom()
    {
        dropModuleDict = new();

        var itemDict = new Dictionary<int, List<DropModule>>();
        var resourceDict = new Dictionary<int, List<DropModule>>();
        var cardDict = new Dictionary<int, List<DropModule>>();
        dropModuleDict.Add(GameItemType.Resource, resourceDict);
        dropModuleDict.Add(GameItemType.Goods, itemDict);
        dropModuleDict.Add(GameItemType.Card, cardDict);

        Configs.WayOfDesc.GetConfigList().ForEach((cfg) =>
        {

            if (cfg.DropItem != "")
            {
                var list = GameItemUtils.CreateGameItems(cfg.DropItem).ToList();
                list.ForEach(gitem =>
                {
                    if (gitem.Type == GameItemType.Resource)
                    {
                        _addItemToDict(resourceDict, gitem, cfg);
                    }
                    else if (gitem.Type == GameItemType.Goods)
                    {
                        _addItemToDict(itemDict, gitem, cfg);
                    }
                    else if (gitem.Type == GameItemType.Card)
                    {
                        _addItemToDict(cardDict, gitem, cfg);
                    }
                });
            }

            if (cfg.ItemRange != "")
            {
                List<string> gitemList = cfg.ItemRange.Split("|").ToList();
                gitemList.ForEach(rangestr =>
                {
                    //格式 起始ID:终止ID:权重
                    List<int> idstrLst = rangestr.Split(":").ToList<string>().ConvertAll<int>(p => int.Parse(p));

                    for (var index = idstrLst[0]; index < idstrLst[1]; index++)
                    {
                        GameItem gitem = GameItemUtils.CreateGameItem(GameItemType.Goods, index, idstrLst[2]);
                        _addItemToDict(itemDict, gitem, cfg);
                    }
                });

            }
        });
    }

    /// <summary>
    /// 通常获取掉落指引的地方
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public List<DropModule> GetItemDrop(GameItem item)
    {
        if (dropModuleDict.ContainsKey(item.Type))
        {
            if (dropModuleDict[item.Type].ContainsKey(item.Id))
            {
                return dropModuleDict[item.Type][item.Id];
            }
            else
            {
                return new List<DropModule>();
            }
        }
        else
        {
            return new List<DropModule>();
        }
    }

    private void _addItemToDict(Dictionary<int, List<DropModule>> dict, GameItem gitem, WayOfDescConfig cfg)
    {
        if (!dict.ContainsKey(gitem.Id)) dict.Add(gitem.Id, new List<DropModule>());
        var _module = new DropModule();
        _module.moduleId = cfg.Id;
        _module.weight = gitem.Count;
        _module.txtmoduleName = cfg.ModuleName + cfg.SubModuleName;
        _module.txtDesc = cfg.Desc;
        if (Module_Level.ContainsKey(cfg.Id))
        {
            _module.openlv = Module_Level[cfg.Id];
        }
        else _module.openlv = 0;

        dict[gitem.Id].Add(_module);
    }
}
