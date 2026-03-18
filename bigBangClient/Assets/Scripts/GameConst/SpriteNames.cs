namespace BigBang
{
    public static class SpriteNames
    {
        //图集：Player
        public static class Player
        {
            // 球员状态图片；依次：低谷状态、下降状态、普通状态、上升状态、顶峰状态
            public static readonly string[] PlayerState = { "", "state_1", "state_2", "state_3", "state_4", "state_5" };
            public static readonly string YelloCard = "YellowCard";
            public static readonly string RedCard = "RedCard";
        }

        // 图集：Scout
        public static class Scout
        {
            public const string RecruitBtn = "recruit_btn";
            public const string RecruitGoods = "recruit_goods";
            public const string RecruitDiamond = "recruit_diamond";
        }

        // 图集：Inventory
        public static class Inventory
        {
            public const string Quality = "Quality";
        }

        // 图集：PropIcon 
        public static class PropIcon
        {
            // 欧元图片
            public const string EuroImg = "euro";
            // 钻石图片
            public const string DiamondImg = "diamond";
            // 经验图片
            public const string ExpImg = "exp";
            // 经验图片
            public const string PlayerExp = "playerexp";
            // 经验图片
            public const string CardExp = "cardexp";
            // 体力图片
            public const string PlayerEnergy = "playerenergy";
        }

        // 图集：Shop 
        public static class Shop
        {
            // 训练商城背景
            public const string TrainShop = "train{shopItemID}";
        }

        // 图集:UnlockTrain
        public static class UnlockTrain
        {
            public const string BG = "train{id}";
        }

        // 图集：Public
        public static class Public
        {
            public const string Dropdown = "Dropdown";
            public const string Error = "error";
            public const string Unknown = "unknown";
            public const string YellowBtnImg = "btn1";
            public const string GrayBtnImg = "btn2";
            public const string BlueBtnImg = "btn_9";
            public const string BlackBtnImg = "btn_9_3";
            public const string None = "none";
        }

        // 图集：TrainUI
        public static class TrainUI
        {
            // 倍率切换图片；依次：X1、X10、X100、MAX
            public static readonly string[] SpeedSwitch = { "speed_0", "speed_1", "speed_2", "speed_3" };
            // 升级次数图片；依次：X1、X10、X100、MAX
            public static readonly string[] UpgradeCount = { "upgrade_0", "upgrade_1", "upgrade_2", "upgrade_3" };
        }

        // 图集：League
        public static class League
        {
            // 排名图片（第一名、第二名、第三名）
            public const string RankImg = "Number";
            // 比赛结果图片（无=0、胜利=1、失败=2、平局=3）
            public const string GameResult = "GameResult";
            // 进球数图片
            public const string Goal = "Goal";
            // 红牌
            public const string RedCard = "red";
            // 黄牌
            public const string YellowCard = "yellow";
            // 换上
            public const string SubstitutionUp = "SubstitutionUp";
            // 换下
            public const string SubstitutionDown = "SubstitutionDown";
            public const string Assist = "img_763";
            public const string PenaltyGoal = "img_764";
            public const string PenaltyFail = "img_786";
        }

        // 图集：Card
        public static class Card
        {
            // 球员卡片底牌图片
            public const string Background = "Background{quality}";
            // 球员卡片边框图片
            public const string Border = "Border{quality}";
            // 球员卡片旗帜图片
            public const string Flag = "Flag{quality}";
            // 球员卡片足球图片
            public const string Ball = "Ball{quality}";
            // 球员卡片上阵图片
            public const string OnFormation = "OnFormation{quality}";
            // 球员卡片背光图片
            public const string Light = "Light{quality}";
            // 空星星图片
            public const string BackStar = "BackStar{quality}";
            // 背面图片
            public const string Back = "Back{quality}";
            // 矩形底板图片
            public const string Icon = "icon_{quality}";
            // 碎片边框
            public const string DebrisBorder = "DebrisBorder{quality}";
            // 碎片背景
            public const string DebrisBackground = "DebrisBackground{quality}";
            // 碎片边缘光
            public const string DebrisEdge = "DebrisEdge{quality}";
            // 碎片光
            public const string DebrisLight = "DebrisLight{quality}";
            // 布阵主力底牌图片
            public const string FormationMain = "FormationMain{quality}";
            // 布阵替补底牌图片
            public const string FormationBench = "FormationBench{quality}";

            //详情的方背景
            public const string SquareBack = "Square{quality}";

            //详情小卡示意
            public const string SmallQualityCard = "SmallQualityCard{quality}";

            public const string SmallQualityL2H = "SmallQualityL2H_{quality}";

            //统计那边的底
            public const string StatQuality = "Stat{quality}";
        }

        // 图集:Task
        public static class Task
        {
            public const string Close = "close";
            public const string Obtain = "obtain";
            public const string Open = "open";
            public const string MainTaskTab = "tab";
            public const string MainTaskBG = "bg";
        }

        // 图集:Guide
        public static class Guide
        {
            public const string Me = "me";
            public const string Clerk = "clerk";
            public const string Board = "board";
        }
    }
}
